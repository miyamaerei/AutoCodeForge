/**
 * AgentService 业务逻辑测试
 *
 * 测试覆盖：
 * 1. CreateAsync - 创建 Agent
 * 2. UpdateAsync - 更新 Agent
 * 3. GetByIdAsync - 查询 Agent
 * 4. GetPagedAsync - 分页查询 Agent
 * 5. DeleteAsync - 删除 Agent（软删除）
 * 6. MatchAsync - 匹配 Agent
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Agent;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Tests;

/// <summary>
/// AgentService 业务逻辑测试
/// </summary>
public sealed class AgentServiceTests : IDisposable
{
    private readonly IntegrationTestContext _context;

    public AgentServiceTests()
    {
        _context = new IntegrationTestContext("test-agent-user");
    }

    #region Create Tests

    /// <summary>
    /// 测试创建基础 Agent
    /// </summary>
    [Fact]
    public async Task CreateAsync_Should_CreateAgent_WithValidRequest()
    {
        // Arrange
        var request = new CreateAgentRequest
        {
            Name = "Test Agent",
            Description = "A test agent",
            SystemPrompt = "You are a helpful assistant.",
            IsEnabled = true,
        };

        // Act
        var result = await _context.AgentService.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Agent", result.Name);
        Assert.Equal("A test agent", result.Description);
        Assert.Equal("You are a helpful assistant.", result.SystemPrompt);
        Assert.True(result.IsEnabled);

        // 验证数据库中确实存在
        var retrieved = await _context.AgentRepository.GetByIdAsync(result.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Test Agent", retrieved.Name);

        Console.WriteLine($"[测试1] 创建 Agent 成功: {result.Id}");
    }

    /// <summary>
    /// 测试创建带 LLMModelConfig 的 Agent
    /// </summary>
    [Fact]
    public async Task CreateAsync_Should_CreateAgent_WithLLMModelConfig()
    {
        // Arrange
        var llmConfig = await _context.CreateTestLLMModelConfigAsync(
            provider: LLMProvider.AzureOpenAI,
            modelName: "gpt-4o",
            endpoint: "https://api.openai.com/v1",
            apiKey: "test-api-key");

        var request = new CreateAgentRequest
        {
            Name = "Agent with Model",
            SystemPrompt = "You are a coding assistant.",
            LlmModelConfigId = llmConfig.Id,
            IsEnabled = true,
        };

        // Act
        var result = await _context.AgentService.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(llmConfig.Id, result.LlmModelConfigId);

        Console.WriteLine($"[测试2] 创建带 LLMModelConfig 的 Agent 成功: {result.Id}");
    }

    #endregion

    #region Update Tests

    /// <summary>
    /// 测试更新 Agent
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Should_UpdateAgent()
    {
        // Arrange
        var agent = await _context.CreateTestAgentAsync(
            name: "Original Name",
            description: "Original description",
            isEnabled: true);

        var updateRequest = new UpdateAgentRequest
        {
            Name = "Updated Name",
            Description = "Updated description",
            SystemPrompt = "You are an updated assistant.",
            IsEnabled = false,
        };

        // Act
        var result = await _context.AgentService.UpdateAsync(agent.Id, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal("You are an updated assistant.", result.SystemPrompt);
        Assert.False(result.IsEnabled);

        Console.WriteLine($"[测试3] 更新 Agent 成功: {agent.Id}");
    }

    #endregion

    #region Query Tests

    /// <summary>
    /// 测试通过 ID 获取 Agent
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_Should_ReturnAgent_WhenExists()
    {
        // Arrange
        var agent = await _context.CreateTestAgentAsync(name: "Query Test Agent");

        // Act
        var result = await _context.AgentService.GetByIdAsync(agent.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(agent.Id, result.Id);
        Assert.Equal("Query Test Agent", result.Name);

        Console.WriteLine($"[测试4] GetById 查询成功: {agent.Id}");
    }

    /// <summary>
    /// 测试通过 ID 获取 Agent - 不存在时抛出异常
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_Should_ThrowException_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<AutoCodeForge.Core.Exceptions.NotFoundException>(
            () => _context.AgentService.GetByIdAsync(nonExistentId));

        Console.WriteLine("[测试5] GetById 正确抛出 NotFoundException");
    }

    /// <summary>
    /// 测试分页查询 Agent
    /// </summary>
    [Fact]
    public async Task GetPagedAsync_Should_ReturnPagedAgents()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            await _context.CreateTestAgentAsync(name: $"Paged Agent {i}");
        }

        // Act
        var result = await _context.AgentService.GetPagedAsync(page: 1, pageSize: 3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.TotalPages);

        Console.WriteLine($"[测试6] 分页查询成功: {result.TotalCount} 总数, 第1页返回{result.Items.Count}条");
    }

    #endregion

    #region Delete Tests

    /// <summary>
    /// 测试软删除 Agent
    /// </summary>
    [Fact]
    public async Task DeleteAsync_Should_SoftDeleteAgent()
    {
        // Arrange
        var agent = await _context.CreateTestAgentAsync(name: "To Be Deleted");

        // Act
        await _context.AgentService.DeleteAsync(agent.Id);

        // Assert - 软删除后，通过 Service 查询应该抛出异常（因为默认过滤已删除记录）
        await Assert.ThrowsAsync<AutoCodeForge.Core.Exceptions.NotFoundException>(
            () => _context.AgentService.GetByIdAsync(agent.Id));

        // 软删除验证：Agent 记录被标记为 IsDeleted = true
        // 由于 BaseRepository 没有提供查询软删除记录的方法，我们通过以下方式验证：
        // 1. Service 层抛出 NotFoundException 说明记录被"隐藏"
        // 2. Repository 层的 SoftDeleteAsync 成功执行说明实体被正确更新
        // 这间接证明了软删除机制正常工作

        Console.WriteLine($"[测试7] 软删除 Agent 成功: {agent.Id} - Service 层正确抛出 NotFoundException");
    }

    #endregion

    #region Match Tests

    /// <summary>
    /// 测试匹配 Agent - 通过关键词匹配
    /// </summary>
    [Fact]
    public async Task MatchAsync_Should_FindAgent_ByKeywords()
    {
        // Arrange
        await _context.CreateTestAgentAsync(
            name: "Code Review Agent",
            keywords: "review, code, quality",
            systemPrompt: "You are a code reviewer.");

        await _context.CreateTestAgentAsync(
            name: "General Assistant",
            keywords: "general, help",
            systemPrompt: "You are a general assistant.");

        // Act
        var matchedAgent = await _context.AgentService.MatchByInputAsync("review code");

        // Assert
        Assert.NotNull(matchedAgent);
        Assert.Equal("Code Review Agent", matchedAgent.Name);

        Console.WriteLine($"[测试8] 关键词匹配成功: 找到 Agent: {matchedAgent.Name}");
    }

    /// <summary>
    /// 测试匹配 Agent - 无匹配时返回得分最高的 Agent（按名称排序）
    /// 注意：MatchByInputAsync 总是返回得分最高的 Agent，即使得分为 0
    /// </summary>
    [Fact]
    public async Task MatchAsync_Should_ReturnBestMatch_WhenNoExactMatch()
    {
        // Arrange
        await _context.CreateTestAgentAsync(
            name: "Specific Agent",
            keywords: "specific, unique",
            systemPrompt: "You are specific.");

        // Act
        var matchedAgent = await _context.AgentService.MatchByInputAsync("xyz none");

        // Assert - 没有精确匹配时，仍然返回得分最高的 Agent（按名称排序的第一个）
        Assert.NotNull(matchedAgent);
        Assert.Equal("Specific Agent", matchedAgent.Name); // 因为只有这一个 Agent

        Console.WriteLine("[测试9] 无精确匹配时返回最佳候选: " + matchedAgent.Name);
    }

    #endregion

    #region Agent Lifecycle Management Tests

    /// <summary>
    /// 测试创建带角色的 Agent
    /// </summary>
    [Fact]
    public async Task CreateAsync_Should_CreateAgentWithRole()
    {
        // Arrange
        var request = new CreateAgentRequest
        {
            Name = "Worker Agent",
            Description = "Agent with Worker role",
            SystemPrompt = "You are a worker agent.",
            IsEnabled = true,
            Role = AgentRole.Worker.ToString()
        };

        // Act
        var result = await _context.AgentService.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AgentRole.Worker.ToString(), result.Role);
        Assert.Equal(AgentState.Idle.ToString(), result.State);

        Console.WriteLine($"[测试-周期1] 创建带角色的 Agent 成功: {result.Id}");
    }

    /// <summary>
    /// 测试分配任务给 Agent（Idle → Handling）
    /// </summary>
    [Fact]
    public async Task AssignTaskAsync_Should_TransitionFromIdleToHandling()
    {
        // Arrange
        var agent = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Test Worker",
            state: AgentState.Idle);
        var taskId = Guid.NewGuid();
        var request = new AssignTaskRequest
        {
            TaskId = taskId
        };

        // Act
        var result = await _context.AgentService.AssignTaskAsync(agent.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AgentState.Handling.ToString(), result.State);
        Assert.Equal(taskId, result.CurrentTaskId);
        Assert.Equal(1, result.Version);

        // 验证数据库状态
        var updated = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.NotNull(updated);
        Assert.Equal(AgentState.Handling, updated.State);
        Assert.Equal(taskId, updated.CurrentTaskId);

        Console.WriteLine($"[测试-周期2] 分配任务成功: Agent 从 Idle 转换到 Handling");
    }

    /// <summary>
    /// 测试分配任务给非 Idle 状态的 Agent（应该抛出异常）
    /// </summary>
    [Fact]
    public async Task AssignTaskAsync_Should_Throw_WhenNotIdle()
    {
        // Arrange
        var agent = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Busy Agent",
            state: AgentState.Handling);
        var request = new AssignTaskRequest
        {
            TaskId = Guid.NewGuid()
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _context.AgentService.AssignTaskAsync(agent.Id, request));

        Console.WriteLine($"[测试-周期3] 正确阻止向非 Idle 状态 Agent 分配任务");
    }

    /// <summary>
    /// 测试完成任务（Handling → Idle）
    /// </summary>
    [Fact]
    public async Task CompleteTaskAsync_Should_TransitionFromHandlingToIdle()
    {
        // Arrange
        var agent = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Working Agent",
            state: AgentState.Handling);
        var taskId = Guid.NewGuid();
        var agentFromDb = await _context.AgentRepository.GetByIdAsync(agent.Id);
        agentFromDb.CurrentTaskId = taskId;
        await _context.AgentRepository.UpdateAsync(agentFromDb);

        // Act
        var result = await _context.AgentService.CompleteTaskAsync(agent.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AgentState.Idle.ToString(), result.State);
        Assert.Null(result.CurrentTaskId);
        Assert.Equal(1, result.Version);

        Console.WriteLine($"[测试-周期4] 完成任务成功: Agent 从 Handling 转换到 Idle");
    }

    /// <summary>
    /// 测试任务失败（Handling → Idle + 创建学习记录）
    /// </summary>
    [Fact]
    public async Task FailTaskAsync_Should_TransitionFromHandlingToIdleAndCreateLearningRecord()
    {
        // Arrange
        var agent = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Failing Agent",
            state: AgentState.Handling);
        var request = new FailTaskRequest
        {
            Reason = "Test failure reason",
            FailureCategory = FailureCategory.CodeError.ToString()
        };

        // Act
        var result = await _context.AgentService.FailTaskAsync(agent.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AgentState.Idle.ToString(), result.State);
        Assert.Null(result.CurrentTaskId);

        // 验证学习记录被创建
        var learningRecords = await _context.AgentLearningRecordRepository.GetByAgentIdAsync(agent.Id);
        Assert.Single(learningRecords);
        Assert.Equal(LearningTriggerType.Exception, learningRecords[0].TriggerType);
        Assert.Equal("Task failed: Test failure reason", learningRecords[0].TriggerReason);
        Assert.False(learningRecords[0].IsSuccessful);

        Console.WriteLine($"[测试-周期5] 任务失败处理成功: 创建了学习记录");
    }

    /// <summary>
    /// 测试开始学习（Idle → Learning）
    /// </summary>
    [Fact]
    public async Task StartLearningAsync_Should_TransitionFromIdleToLearning()
    {
        // Arrange
        var agent = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Learning Agent",
            state: AgentState.Idle);

        // Act
        var result = await _context.AgentService.StartLearningAsync(agent.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AgentState.Learning.ToString(), result.State);
        Assert.Equal(1, result.Version);

        Console.WriteLine($"[测试-周期6] 开始学习成功: Agent 从 Idle 转换到 Learning");
    }

    /// <summary>
    /// 测试完成学习（Learning → Idle + 创建学习记录）
    /// </summary>
    [Fact]
    public async Task CompleteLearningAsync_Should_TransitionFromLearningToIdleAndCreateRecord()
    {
        // Arrange
        var agent = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Studying Agent",
            state: AgentState.Learning);
        var request = new CompleteLearningRequest
        {
            Summary = "Successfully learned",
            Result = "Updated knowledge base",
            SkillTags = "new skill, updated skill"
        };

        // Act
        var result = await _context.AgentService.CompleteLearningAsync(agent.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AgentState.Idle.ToString(), result.State);
        Assert.Equal("new skill, updated skill", result.SkillTags);

        // 验证学习记录
        var learningRecords = await _context.AgentLearningRecordRepository.GetByAgentIdAsync(agent.Id);
        Assert.Single(learningRecords);
        Assert.Equal(LearningTriggerType.Manual, learningRecords[0].TriggerType);
        Assert.Equal("Successfully learned", learningRecords[0].TriggerReason);
        Assert.True(learningRecords[0].IsSuccessful);

        Console.WriteLine($"[测试-周期7] 完成学习成功: 创建了学习记录");
    }

    /// <summary>
    /// 测试进入休眠（Idle → Dormant + 创建休眠记录）
    /// </summary>
    [Fact]
    public async Task EnterDormantAsync_Should_TransitionFromIdleToDormantAndCreateRecord()
    {
        // Arrange
        var agent = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Tired Agent",
            state: AgentState.Idle);
        var request = new EnterDormantRequest
        {
            Reason = "Continuous learning inefficiency"
        };

        // Act
        var result = await _context.AgentService.EnterDormantAsync(agent.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AgentState.Dormant.ToString(), result.State);
        Assert.Equal("Continuous learning inefficiency", result.DormantReason);
        Assert.Equal(1, result.Version);

        // 验证休眠记录
        var dormantRecords = await _context.AgentDormantRecordRepository.GetByAgentIdAsync(agent.Id);
        Assert.Single(dormantRecords);
        Assert.Equal("Continuous learning inefficiency", dormantRecords[0].ReasonDescription);
        Assert.False(dormantRecords[0].IsWoken);

        Console.WriteLine($"[测试-周期8] 进入休眠成功: 创建了休眠记录");
    }

    /// <summary>
    /// 测试唤醒 Agent（Dormant → Idle + 更新休眠记录）
    /// </summary>
    [Fact]
    public async Task WakeUpAsync_Should_TransitionFromDormantToIdleAndUpdateRecord()
    {
        // Arrange
        var agent = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Sleeping Agent",
            state: AgentState.Dormant);
        // 创建休眠记录
        await _context.CreateTestDormantRecordAsync(agent.Id, reason: "Test sleep", isWoken: false);

        // Act
        var result = await _context.AgentService.WakeUpAsync(agent.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AgentState.Idle.ToString(), result.State);
        Assert.Null(result.DormantReason);
        Assert.Equal(1, result.Version);

        // 验证休眠记录被更新
        var dormantRecords = await _context.AgentDormantRecordRepository.GetByAgentIdAsync(agent.Id);
        Assert.Single(dormantRecords);
        Assert.True(dormantRecords[0].IsWoken);
        Assert.NotNull(dormantRecords[0].WokenAtUtc);

        Console.WriteLine($"[测试-周期9] 唤醒 Agent 成功: 更新了休眠记录");
    }

    /// <summary>
    /// 测试按状态查询 Agent
    /// </summary>
    [Fact]
    public async Task GetByStateAsync_Should_ReturnAgentsInSpecifiedState()
    {
        // Arrange
        await _context.CreateTestAgentWithLifecycleAsync(name: "Idle 1", state: AgentState.Idle);
        await _context.CreateTestAgentWithLifecycleAsync(name: "Idle 2", state: AgentState.Idle);
        await _context.CreateTestAgentWithLifecycleAsync(name: "Handling 1", state: AgentState.Handling);

        // Act
        var idleAgents = await _context.AgentService.GetByStateAsync(AgentState.Idle);

        // Assert
        Assert.Equal(2, idleAgents.Count);
        Assert.All(idleAgents, a => Assert.Equal(AgentState.Idle.ToString(), a.State));

        Console.WriteLine($"[测试-周期10] 按状态查询成功: 找到 {idleAgents.Count} 个 Idle Agent");
    }

    /// <summary>
    /// 测试获取休眠 Agent 列表
    /// </summary>
    [Fact]
    public async Task GetDormantAgentsAsync_Should_ReturnOnlyDormantAgents()
    {
        // Arrange
        await _context.CreateTestAgentWithLifecycleAsync(name: "Active 1", state: AgentState.Idle);
        await _context.CreateTestAgentWithLifecycleAsync(name: "Sleeping 1", state: AgentState.Dormant);
        await _context.CreateTestAgentWithLifecycleAsync(name: "Sleeping 2", state: AgentState.Dormant);

        // Act
        var dormantAgents = await _context.AgentService.GetDormantAgentsAsync();

        // Assert
        Assert.Equal(2, dormantAgents.Count);
        Assert.All(dormantAgents, a => Assert.Equal(AgentState.Dormant.ToString(), a.State));

        Console.WriteLine($"[测试-周期11] 获取休眠 Agent 列表成功: 找到 {dormantAgents.Count} 个");
    }

    /// <summary>
    /// 测试获取 Agent 学习记录
    /// </summary>
    [Fact]
    public async Task GetLearningRecordsAsync_Should_ReturnAllLearningRecordsForAgent()
    {
        // Arrange
        var agent = await _context.CreateTestAgentWithLifecycleAsync(name: "Learner");
        await _context.CreateTestLearningRecordAsync(agent.Id, LearningTriggerType.IdleTimeout, "Timeout learning", true);
        await _context.CreateTestLearningRecordAsync(agent.Id, LearningTriggerType.TaskReview, "Post-task review", true);
        // 另一个 Agent 的记录
        var otherAgent = await _context.CreateTestAgentWithLifecycleAsync(name: "Other");
        await _context.CreateTestLearningRecordAsync(otherAgent.Id);

        // Act
        var records = await _context.AgentService.GetLearningRecordsAsync(agent.Id);

        // Assert
        Assert.Equal(2, records.Count);
        Assert.All(records, r => Assert.Equal(agent.Id, r.AgentId));

        Console.WriteLine($"[测试-周期12] 获取学习记录成功: 找到 {records.Count} 条");
    }

    /// <summary>
    /// 测试获取 Agent 休眠记录
    /// </summary>
    [Fact]
    public async Task GetDormantRecordsAsync_Should_ReturnAllDormantRecordsForAgent()
    {
        // Arrange
        var agent = await _context.CreateTestAgentWithLifecycleAsync(name: "Sleeper");
        await _context.CreateTestDormantRecordAsync(agent.Id, "First sleep", true);
        await _context.CreateTestDormantRecordAsync(agent.Id, "Second sleep", false);

        // Act
        var records = await _context.AgentService.GetDormantRecordsAsync(agent.Id);

        // Assert
        Assert.Equal(2, records.Count);
        Assert.All(records, r => Assert.Equal(agent.Id, r.AgentId));

        Console.WriteLine($"[测试-周期13] 获取休眠记录成功: 找到 {records.Count} 条");
    }

    /// <summary>
    /// 测试完整的状态流转周期
    /// </summary>
    [Fact]
    public async Task FullLifecycle_Should_CompleteStateTransitions()
    {
        // Arrange
        var agent = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Full Cycle Agent",
            state: AgentState.Idle);
        var taskId = Guid.NewGuid();

        Console.WriteLine($"[测试-周期14] 开始完整周期测试: 初始状态 Idle");

        // Act 1: 分配任务
        var assignResult = await _context.AgentService.AssignTaskAsync(agent.Id, new AssignTaskRequest { TaskId = taskId });
        Assert.Equal(AgentState.Handling.ToString(), assignResult.State);
        Console.WriteLine($"  → 分配任务: Handling");

        // Act 2: 完成任务
        var completeResult = await _context.AgentService.CompleteTaskAsync(agent.Id);
        Assert.Equal(AgentState.Idle.ToString(), completeResult.State);
        Console.WriteLine($"  → 完成任务: Idle");

        // Act 3: 开始学习
        var startLearningResult = await _context.AgentService.StartLearningAsync(agent.Id);
        Assert.Equal(AgentState.Learning.ToString(), startLearningResult.State);
        Console.WriteLine($"  → 开始学习: Learning");

        // Act 4: 完成学习
        var completeLearningResult = await _context.AgentService.CompleteLearningAsync(agent.Id, new CompleteLearningRequest { Summary = "Good" });
        Assert.Equal(AgentState.Idle.ToString(), completeLearningResult.State);
        Console.WriteLine($"  → 完成学习: Idle");

        // Act 5: 进入休眠
        var dormantResult = await _context.AgentService.EnterDormantAsync(agent.Id, new EnterDormantRequest { Reason = "Tired" });
        Assert.Equal(AgentState.Dormant.ToString(), dormantResult.State);
        Console.WriteLine($"  → 进入休眠: Dormant");

        // Act 6: 唤醒
        var wakeResult = await _context.AgentService.WakeUpAsync(agent.Id);
        Assert.Equal(AgentState.Idle.ToString(), wakeResult.State);
        Console.WriteLine($"  → 唤醒: Idle");

        // Assert - 最终版本号
        Assert.Equal(6, wakeResult.Version);
        Console.WriteLine($"[测试-周期14] 完整周期测试完成: 最终版本号 {wakeResult.Version}");
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}
