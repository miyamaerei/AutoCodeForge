using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Agent;
using AutoCodeForge.Core.DTOs.AI;
using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.AI;
using AutoCodeForge.Infrastructure.BackgroundServices;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SqlSugar;

namespace AutoCodeForge.Tests;

/// <summary>
/// 完整任务工作流集成测试
/// 模拟用户场景：拉取仓库 → 分析代码 → 提出优化需求 → 创建任务 → 执行完成
/// </summary>
public sealed class TaskWorkflowIntegrationTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly TaskService _taskService;
    private readonly AgentService _agentService;
    private readonly AgentExecutor _agentExecutor;
    private readonly TaskExecutor _taskExecutor;
    private Guid _agentId;

    public TaskWorkflowIntegrationTests()
    {
        // 初始化SQLite数据库
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.workflow.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        // 创建所需表
        _db.CodeFirst.InitTables(
            typeof(TaskEntity),
            typeof(TaskLogEntity),
            typeof(AgentEntity),
            typeof(LLMModelConfigEntity));

        var currentUser = new TestCurrentUser("workflow.user");
        
        // 初始化服务
        var taskRepository = new TaskRepository(_db, currentUser);
        var taskLogRepository = new TaskLogRepository(_db, currentUser);
        var agentRepository = new AgentRepository(_db, currentUser);
        var modelConfigRepository = new LLMModelConfigRepository(_db, currentUser);

        _taskService = new TaskService(taskRepository, taskLogRepository);
        _agentService = new AgentService(agentRepository);

        // Mock LLM网关，模拟AI响应
        var mockLlmGateway = new Mock<ILlmGateway>();
        mockLlmGateway.Setup(g => g.ChatWithToolsAsync(
                It.IsAny<LlmRequest>(),
                It.IsAny<IEnumerable<IAgentTool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LlmResponse
            {
                Content = @"基于代码分析，我发现以下优化建议：
1. 将重复的数据库查询逻辑提取为通用方法
2. 添加输入验证以防止空指针异常
3. 优化循环嵌套，时间复杂度从O(n²)降至O(n log n)
4. 添加单元测试覆盖核心业务逻辑",
                ModelName = "gpt-4o-mini",
                CompletedAtUtc = DateTime.UtcNow,
                TotalTokens = 256,
            });

        mockLlmGateway.Setup(g => g.ChatAsync(
                It.IsAny<LlmRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LlmResponse
            {
                Content = "分析完成，已生成优化建议",
                ModelName = "gpt-4o-mini",
                CompletedAtUtc = DateTime.UtcNow,
                TotalTokens = 64,
            });

        // 创建AgentExecutor（使用StubAgentFactory强制使用LLM网关路径）
        var stubAgentFactory = new StubAgentFactory();
        _agentExecutor = new AgentExecutor(
            mockLlmGateway.Object, 
            stubAgentFactory, 
            Array.Empty<IAgentTool>(), 
            NullLogger<AgentExecutor>.Instance);

        // 创建TaskExecutor
        _taskExecutor = new TaskExecutor(
            taskRepository,
            taskLogRepository,
            agentRepository,
            _agentExecutor,
            null!, // RepoSyncTaskHandler - 测试General类型任务不需要
            null!, // ReviewTaskHandler - 测试General类型任务不需要
            NullLogger<TaskExecutor>.Instance);

        // 初始化测试数据
        SeedTestData(agentRepository, modelConfigRepository).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 完整工作流测试：
    /// 1. 用户提交代码分析请求作为任务
    /// 2. 任务进入待执行队列
    /// 3. TaskExecutor执行任务（调用LLM分析代码）
    /// 4. 任务完成，记录结果
    /// </summary>
    [Fact]
    public async Task FullTaskWorkflow_UserRequestCodeAnalysis_ShouldCompleteSuccessfully()
    {
        // ========== Step 1: 用户创建代码分析任务 ==========
        var createdTask = await _taskService.CreateAsync(new CreateTaskRequest
        {
            Title = "代码优化分析任务",
            Description = "分析最新提交的代码，识别潜在的性能问题和代码质量问题",
            Input = @"分析以下代码变更：
- 文件: src/Services/OrderService.cs
- 变更: 添加了新的订单处理逻辑
- 提交信息: feat: 实现订单批量处理功能

请识别代码中的潜在问题和优化建议。",
            AgentId = _agentId,
        });

        // 验证任务创建成功，状态为Pending
        Assert.Equal("代码优化分析任务", createdTask.Title);
        Assert.Equal(AutoCodeForge.Core.Entities.TaskStatus.Pending.ToString(), createdTask.Status);

        var initialLogs = await _taskService.GetLogsAsync(createdTask.Id);
        Assert.Single(initialLogs);
        Assert.Equal("Task created", initialLogs[0].Message);

        // ========== Step 2: 启动任务（模拟TaskQueueService的行为） ==========
        var started = await _taskService.TryStartAsync(createdTask.Id);
        Assert.True(started, "任务应该成功启动");

        var taskAfterStart = await _taskService.GetByIdAsync(createdTask.Id);
        Assert.Equal(AutoCodeForge.Core.Entities.TaskStatus.Running.ToString(), taskAfterStart.Status);
        Assert.NotNull(taskAfterStart.StartedAtUtc);

        // ========== Step 3: 执行任务（调用AgentExecutor处理） ==========
        var taskEntity = await _db.Queryable<TaskEntity>().FirstAsync(t => t.Id == createdTask.Id);
        await _taskExecutor.ExecuteAsync(taskEntity);

        // ========== Step 4: 验证任务完成状态 ==========
        var completedTask = await _taskService.GetByIdAsync(createdTask.Id);
        
        Assert.Equal(AutoCodeForge.Core.Entities.TaskStatus.Completed.ToString(), completedTask.Status);
        Assert.Equal(100, completedTask.Progress);
        Assert.NotNull(completedTask.CompletedAtUtc);
        Assert.NotNull(completedTask.Result);
        Assert.Null(completedTask.ErrorMessage);

        // 验证结果包含AI分析建议（结果被序列化为JSON格式，中文会被转义）
        var result = completedTask.Result!;
        Assert.Contains("output", result);
        Assert.Contains("\\u57FA\\u4E8E\\u4EE3\\u7801\\u5206", result); // "基于代码分析"的Unicode编码

        // ========== Step 5: 验证完整的日志链 ==========
        var allLogs = await _taskService.GetLogsAsync(createdTask.Id);
        
        // 日志应该包含：创建、启动、执行开始、执行完成
        Assert.True(allLogs.Count >= 3, "应该至少有3条日志记录");
        
        var logMessages = allLogs.Select(l => l.Message).ToList();
        Assert.Contains("Task created", logMessages);
        Assert.Contains("Task started", logMessages);
        Assert.Contains("Task execution finished", logMessages);
    }

    /// <summary>
    /// 测试任务执行失败场景
    /// 当Agent不可用时，任务应该标记为失败并记录错误信息
    /// </summary>
    [Fact]
    public async Task TaskExecution_WithDisabledAgent_ShouldFail()
    {
        // 创建一个禁用的Agent
        var disabledAgent = await _agentService.CreateAsync(new CreateAgentRequest
        {
            Name = "disabled-agent",
            Description = "测试用禁用Agent",
            IsEnabled = false,
        });

        // 创建任务关联禁用的Agent
        var task = await _taskService.CreateAsync(new CreateTaskRequest
        {
            Title = "测试失败任务",
            Input = "测试输入",
            AgentId = disabledAgent.Id,
        });

        // 启动任务
        await _taskService.TryStartAsync(task.Id);

        // 执行任务
        var taskEntity = await _db.Queryable<TaskEntity>().FirstAsync(t => t.Id == task.Id);
        await _taskExecutor.ExecuteAsync(taskEntity);

        // 验证任务失败
        var failedTask = await _taskService.GetByIdAsync(task.Id);
        Assert.Equal(AutoCodeForge.Core.Entities.TaskStatus.Failed.ToString(), failedTask.Status);
        Assert.Contains("disabled", failedTask.ErrorMessage);

        // 验证错误日志
        var logs = await _taskService.GetLogsAsync(task.Id);
        Assert.Contains(logs, l => l.Level == "Error");
    }

    /// <summary>
    /// 测试任务重试流程
    /// 完成的任务可以通过更新状态重新执行
    /// </summary>
    [Fact]
    public async Task CompletedTask_CanBeResetAndReExecute()
    {
        // 创建任务并执行完成
        var task = await _taskService.CreateAsync(new CreateTaskRequest
        {
            Title = "可重试任务",
            Input = "测试重试",
            AgentId = _agentId,
        });

        await _taskService.TryStartAsync(task.Id);
        var taskEntity = await _db.Queryable<TaskEntity>().FirstAsync(t => t.Id == task.Id);
        await _taskExecutor.ExecuteAsync(taskEntity);

        // 验证任务完成
        var completed = await _taskService.GetByIdAsync(task.Id);
        Assert.Equal(AutoCodeForge.Core.Entities.TaskStatus.Completed.ToString(), completed.Status);

        // 通过更新API重置任务状态（模拟重新执行）
        await _db.Updateable<TaskEntity>()
            .SetColumns(t => t.Status == AutoCodeForge.Core.Entities.TaskStatus.Pending)
            .SetColumns(t => t.Progress == 0)
            .SetColumns(t => t.Result == null)
            .SetColumns(t => t.ErrorMessage == null)
            .SetColumns(t => t.StartedAtUtc == null)
            .SetColumns(t => t.CompletedAtUtc == null)
            .Where(t => t.Id == task.Id)
            .ExecuteCommandAsync();

        // 验证任务已重置
        var resetTask = await _taskService.GetByIdAsync(task.Id);
        Assert.Equal(AutoCodeForge.Core.Entities.TaskStatus.Pending.ToString(), resetTask.Status);
        Assert.Equal(0, resetTask.Progress);
        Assert.Null(resetTask.Result);
        Assert.Null(resetTask.ErrorMessage);
    }

    private async Task SeedTestData(AgentRepository agentRepository, LLMModelConfigRepository modelConfigRepository)
    {
        // 创建LLM模型配置
        var modelConfig = new LLMModelConfigEntity
        {
            Id = Guid.NewGuid(),
            Provider = LLMProvider.AzureOpenAI,
            ModelName = "gpt-4o-mini",
            Endpoint = "https://test.openai.azure.com",
            ApiKey = "test-api-key",
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await modelConfigRepository.CreateAsync(modelConfig);

        // 创建Agent
        var agent = new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = "code-analyzer-agent",
            Description = "代码分析专用Agent，用于分析代码质量和性能问题",
            SystemPrompt = @"你是一位资深的软件工程师，擅长代码审查和性能优化。
请分析用户提供的代码，识别潜在问题并提供具体的优化建议。",
            LlmModelConfigId = modelConfig.Id,
            IsEnabled = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        _agentId = agent.Id;
        await agentRepository.CreateAsync(agent);
    }

    public void Dispose()
    {
        if (_db is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (File.Exists(_dbPath))
        {
            try
            {
                File.Delete(_dbPath);
            }
            catch (IOException)
            {
                // 忽略临时SQLite锁
            }
        }
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        private readonly string? _ntId;

        public TestCurrentUser(string? ntId)
        {
            _ntId = ntId;
        }

        public string? GetCurrentNtId() => _ntId;

        public bool IsAdmin() => false;
    }

    /// <summary>
    /// 强制使用LLM Gateway路径的Stub AgentFactory
    /// </summary>
    private class StubAgentFactory : AgentFactory
    {
        public StubAgentFactory() : base(null!, null!, null!) { }
    }
}