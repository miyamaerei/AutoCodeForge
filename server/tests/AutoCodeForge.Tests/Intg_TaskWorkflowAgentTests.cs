/**
 * Task-Workflow-Agent 集成测试
 *
 * 测试覆盖：
 * 1. Workflow 种子数据存在性验证
 * 2. Agent 种子数据存在性验证
 * 3. TaskEntity 字段扩展验证（WorkflowId, WorkflowInstanceId, WorkflowNodeId）
 * 4. WorkflowEntity 和 WorkflowInstanceEntity 基本操作
 */

using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Tests;

/// <summary>
/// Task-Workflow-Agent 集成测试
/// </summary>
public sealed class Intg_TaskWorkflowAgentTests : IDisposable
{
    private readonly IntegrationTestContext _context;

    public Intg_TaskWorkflowAgentTests()
    {
        _context = new IntegrationTestContext("test-user");
    }

    #region 测试组1：Workflow 创建和基本操作

    /// <summary>
    /// 测试 Workflow 创建
    /// </summary>
    [Fact]
    public async Task Workflow_Should_CanCreateAndRetrieve()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var workflow = new WorkflowEntity
        {
            Id = workflowId,
            NtId = "test-user",
            Name = "测试工作流",
            Description = "测试描述",
            Version = 1,
            Status = WorkflowStatus.Published,
            NodesJson = GetIssueWorkflowNodes(),
            EdgesJson = GetIssueWorkflowEdges(),
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        // Act
        await _context.WorkflowRepository.CreateAsync(workflow);
        var savedWorkflow = await _context.WorkflowRepository.GetByIdAsync(workflowId);

        // Assert
        Assert.NotNull(savedWorkflow);
        Assert.Equal("测试工作流", savedWorkflow.Name);
        Assert.Equal(WorkflowStatus.Published, savedWorkflow.Status);
        Assert.NotNull(savedWorkflow.NodesJson);
        Assert.NotNull(savedWorkflow.EdgesJson);

        Console.WriteLine($"[测试1] Workflow 创建成功: {savedWorkflow.Name}");
    }

    #endregion

    #region 测试组2：Agent 创建和基本操作

    /// <summary>
    /// 测试 Agent 创建
    /// </summary>
    [Fact]
    public async Task Agent_Should_CanCreateAndRetrieve()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var agent = new AgentEntity
        {
            Id = agentId,
            NtId = "test-user",
            Name = "TestAgent",
            Description = "测试 Agent",
            Keywords = "测试,开发",
            SkillTags = "testing,development",
            SystemPrompt = "你是一个测试 Agent",
            ToolNames = "TestTool",
            State = AgentState.Idle,
            Role = AgentRole.Worker,
            IsEnabled = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        // Act
        await _context.AgentRepository.CreateAsync(agent);
        var savedAgent = await _context.AgentRepository.GetByIdAsync(agentId);

        // Assert
        Assert.NotNull(savedAgent);
        Assert.Equal("TestAgent", savedAgent.Name);
        Assert.Equal(AgentRole.Worker, savedAgent.Role);
        Assert.Equal(AgentState.Idle, savedAgent.State);
        Assert.True(savedAgent.IsEnabled);

        Console.WriteLine($"[测试2] Agent 创建成功: {savedAgent.Name}, Role={savedAgent.Role}");
    }

    /// <summary>
    /// 测试不同角色的 Agent 创建
    /// </summary>
    [Fact]
    public async Task Agents_Should_HaveDifferentRoles()
    {
        // Arrange
        var secretaryId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var workerId = Guid.NewGuid();

        var secretary = new AgentEntity
        {
            Id = secretaryId,
            NtId = "test-user",
            Name = "Secretary",
            Role = AgentRole.Secretary,
            SkillTags = "coordination,dispatch",
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        var manager = new AgentEntity
        {
            Id = managerId,
            NtId = "test-user",
            Name = "Manager",
            Role = AgentRole.Manager,
            SkillTags = "approval,decision",
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        var worker = new AgentEntity
        {
            Id = workerId,
            NtId = "test-user",
            Name = "Worker",
            Role = AgentRole.Worker,
            SkillTags = "execution,coding",
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        // Act
        await _context.AgentRepository.CreateAsync(secretary);
        await _context.AgentRepository.CreateAsync(manager);
        await _context.AgentRepository.CreateAsync(worker);

        var savedSecretary = await _context.AgentRepository.GetByIdAsync(secretaryId);
        var savedManager = await _context.AgentRepository.GetByIdAsync(managerId);
        var savedWorker = await _context.AgentRepository.GetByIdAsync(workerId);

        // Assert
        Assert.NotNull(savedSecretary);
        Assert.NotNull(savedManager);
        Assert.NotNull(savedWorker);

        Assert.Equal(AgentRole.Secretary, savedSecretary.Role);
        Assert.Equal(AgentRole.Manager, savedManager.Role);
        Assert.Equal(AgentRole.Worker, savedWorker.Role);

        Console.WriteLine($"[测试3] 不同角色 Agent 创建成功:");
        Console.WriteLine($"  - Secretary: Role={savedSecretary.Role}");
        Console.WriteLine($"  - Manager: Role={savedManager.Role}");
        Console.WriteLine($"  - Worker: Role={savedWorker.Role}");
    }

    #endregion

    #region 测试组3：TaskEntity 字段扩展验证

    /// <summary>
    /// 测试 TaskEntity 支持 WorkflowId 字段
    /// </summary>
    [Fact]
    public async Task TaskEntity_Should_SupportWorkflowId()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var workflowId = Guid.NewGuid();

        // Act
        var task = new TaskEntity
        {
            Id = taskId,
            NtId = "test-user",
            Title = "测试任务",
            Input = """{"test": "data"}""",
            DomainType = "issue-processing",
            WorkflowId = workflowId,  // 新字段
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        await _context.TaskRepository.CreateAsync(task);

        // Assert
        var savedTask = await _context.TaskRepository.GetByIdAsync(taskId);
        Assert.NotNull(savedTask);
        Assert.NotNull(savedTask.WorkflowId);
        Assert.Equal(workflowId, savedTask.WorkflowId);

        Console.WriteLine($"[测试7] TaskEntity.WorkflowId 正常工作: {savedTask.WorkflowId}");
    }

    /// <summary>
    /// 测试 TaskEntity 支持 WorkflowInstanceId 字段
    /// </summary>
    [Fact]
    public async Task TaskEntity_Should_SupportWorkflowInstanceId()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var instanceId = Guid.NewGuid();

        // Act
        var task = new TaskEntity
        {
            Id = taskId,
            NtId = "test-user",
            Title = "测试任务",
            Input = """{"test": "data"}""",
            DomainType = "issue-processing",
            WorkflowInstanceId = instanceId,  // 新字段
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        await _context.TaskRepository.CreateAsync(task);

        // Assert
        var savedTask = await _context.TaskRepository.GetByIdAsync(taskId);
        Assert.NotNull(savedTask);
        Assert.NotNull(savedTask.WorkflowInstanceId);
        Assert.Equal(instanceId, savedTask.WorkflowInstanceId);

        Console.WriteLine($"[测试8] TaskEntity.WorkflowInstanceId 正常工作: {savedTask.WorkflowInstanceId}");
    }

    /// <summary>
    /// 测试 TaskEntity 支持 WorkflowNodeId 字段
    /// </summary>
    [Fact]
    public async Task TaskEntity_Should_SupportWorkflowNodeId()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        // Act
        var task = new TaskEntity
        {
            Id = taskId,
            NtId = "test-user",
            Title = "测试任务",
            Input = """{"test": "data"}""",
            DomainType = "issue-processing",
            WorkflowNodeId = "analyze",  // 新字段
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        await _context.TaskRepository.CreateAsync(task);

        // Assert
        var savedTask = await _context.TaskRepository.GetByIdAsync(taskId);
        Assert.NotNull(savedTask);
        Assert.Equal("analyze", savedTask.WorkflowNodeId);

        Console.WriteLine($"[测试9] TaskEntity.WorkflowNodeId 正常工作: {savedTask.WorkflowNodeId}");
    }

    #endregion

    #region 测试组4：WorkflowInstance 基本操作

    /// <summary>
    /// 测试 WorkflowInstance 创建
    /// </summary>
    [Fact]
    public async Task WorkflowInstance_Should_CanCreateAndRetrieve()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var instanceId = Guid.NewGuid();

        // 先创建一个 Task 关联
        var task = new TaskEntity
        {
            Id = taskId,
            NtId = "test-user",
            Title = "测试任务",
            Input = """{"test": "data"}""",
            DomainType = "issue-processing",
            WorkflowId = workflowId,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        // Act
        var instance = new WorkflowInstanceEntity
        {
            Id = instanceId,
            NtId = "test-user",
            WorkflowId = workflowId,
            Status = WorkflowInstanceStatus.Running,
            CurrentNodeId = "start",
            Progress = 0,
            InputJson = """{"test": "data"}""",
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        await _context.WorkflowInstanceRepository.CreateAsync(instance);

        // Assert
        var savedInstance = await _context.WorkflowInstanceRepository.GetByIdAsync(instanceId);
        Assert.NotNull(savedInstance);
        Assert.Equal(WorkflowInstanceStatus.Running, savedInstance.Status);
        Assert.Equal("start", savedInstance.CurrentNodeId);
        Assert.NotNull(savedInstance.InputJson);

        // 更新 Task 的 WorkflowInstanceId
        task.WorkflowInstanceId = instanceId;
        await _context.TaskRepository.UpdateAsync(task);

        var updatedTask = await _context.TaskRepository.GetByIdAsync(taskId);
        Assert.Equal(instanceId, updatedTask!.WorkflowInstanceId);

        Console.WriteLine($"[测试10] WorkflowInstance 创建成功，关联 Task.WorkflowInstanceId = {instanceId}");
    }

    /// <summary>
    /// 测试 WorkflowInstance 状态更新
    /// </summary>
    [Fact]
    public async Task WorkflowInstance_Should_UpdateStatus()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var instanceId = Guid.NewGuid();

        var instance = new WorkflowInstanceEntity
        {
            Id = instanceId,
            NtId = "test-user",
            WorkflowId = workflowId,
            Status = WorkflowInstanceStatus.Running,
            CurrentNodeId = "start",
            Progress = 0,
            InputJson = """{"test": "data"}""",
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.WorkflowInstanceRepository.CreateAsync(instance);

        // Act - 更新到 Agent 节点
        instance.CurrentNodeId = "analyze";
        instance.Progress = 25;
        await _context.WorkflowInstanceRepository.UpdateAsync(instance);

        // Assert
        var updatedInstance = await _context.WorkflowInstanceRepository.GetByIdAsync(instanceId);
        Assert.NotNull(updatedInstance);
        Assert.Equal("analyze", updatedInstance.CurrentNodeId);
        Assert.Equal(25, updatedInstance.Progress);

        Console.WriteLine($"[测试11] WorkflowInstance 状态更新: Node={updatedInstance.CurrentNodeId}, Progress={updatedInstance.Progress}%");
    }

    /// <summary>
    /// 测试 WorkflowInstance 暂停与恢复
    /// </summary>
    [Fact]
    public async Task WorkflowInstance_Should_PauseAndResume()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var instanceId = Guid.NewGuid();

        var instance = new WorkflowInstanceEntity
        {
            Id = instanceId,
            NtId = "test-user",
            WorkflowId = workflowId,
            Status = WorkflowInstanceStatus.Running,
            CurrentNodeId = "human-gate",  // HumanGate 节点
            Progress = 50,
            InputJson = """{"test": "data"}""",
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.WorkflowInstanceRepository.CreateAsync(instance);

        // Act - 暂停（HumanGate）
        instance.Status = WorkflowInstanceStatus.Paused;
        await _context.WorkflowInstanceRepository.UpdateAsync(instance);

        // Assert - 验证暂停状态
        var pausedInstance = await _context.WorkflowInstanceRepository.GetByIdAsync(instanceId);
        Assert.NotNull(pausedInstance);
        Assert.Equal(WorkflowInstanceStatus.Paused, pausedInstance.Status);
        Assert.Equal("human-gate", pausedInstance.CurrentNodeId);

        // Act - 恢复
        pausedInstance.Status = WorkflowInstanceStatus.Running;
        await _context.WorkflowInstanceRepository.UpdateAsync(pausedInstance);

        // Assert - 验证恢复状态
        var resumedInstance = await _context.WorkflowInstanceRepository.GetByIdAsync(instanceId);
        Assert.NotNull(resumedInstance);
        Assert.Equal(WorkflowInstanceStatus.Running, resumedInstance.Status);

        Console.WriteLine($"[测试12] WorkflowInstance 暂停与恢复: {pausedInstance.Status} → {resumedInstance.Status}");
    }

    /// <summary>
    /// 测试 WorkflowInstance 完成
    /// </summary>
    [Fact]
    public async Task WorkflowInstance_Should_Complete()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var instanceId = Guid.NewGuid();

        var instance = new WorkflowInstanceEntity
        {
            Id = instanceId,
            NtId = "test-user",
            WorkflowId = workflowId,
            Status = WorkflowInstanceStatus.Running,
            CurrentNodeId = "end",
            Progress = 100,
            InputJson = """{"test": "data"}""",
            OutputJson = """{"result": "completed"}""",
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.WorkflowInstanceRepository.CreateAsync(instance);

        // Act - 标记完成
        instance.Status = WorkflowInstanceStatus.Completed;
        instance.CompletedAtUtc = DateTime.UtcNow;
        await _context.WorkflowInstanceRepository.UpdateAsync(instance);

        // Assert
        var completedInstance = await _context.WorkflowInstanceRepository.GetByIdAsync(instanceId);
        Assert.NotNull(completedInstance);
        Assert.Equal(WorkflowInstanceStatus.Completed, completedInstance.Status);
        Assert.NotNull(completedInstance.CompletedAtUtc);
        Assert.Equal(100, completedInstance.Progress);

        Console.WriteLine($"[测试13] WorkflowInstance 完成: Status={completedInstance.Status}, CompletedAt={completedInstance.CompletedAtUtc}");
    }

    #endregion

    #region 测试组5：Task-Workflow 关联流程

    /// <summary>
    /// 测试完整 Task-Workflow 关联流程
    /// </summary>
    [Fact]
    public async Task Task_Should_LinkToWorkflowAndCreateInstance()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var instanceId = Guid.NewGuid();

        // 先创建 Workflow
        var workflow = new WorkflowEntity
        {
            Id = workflowId,
            NtId = "test-user",
            Name = "Issue处理流程",
            Version = 1,
            Status = WorkflowStatus.Published,
            NodesJson = GetIssueWorkflowNodes(),
            EdgesJson = GetIssueWorkflowEdges(),
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.WorkflowRepository.CreateAsync(workflow);

        // Step 1: 创建 Task 并关联 Workflow
        var task = new TaskEntity
        {
            Id = taskId,
            NtId = "test-user",
            Title = "完整流程测试",
            Input = """{"issue_id": "123", "title": "Bug修复"}""",
            DomainType = "issue-processing",
            Status = Core.Entities.TaskStatus.Pending,
            WorkflowId = workflowId,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        // Step 2: 启动 WorkflowInstance
        var instance = new WorkflowInstanceEntity
        {
            Id = instanceId,
            NtId = "test-user",
            WorkflowId = workflowId,
            Status = WorkflowInstanceStatus.Running,
            CurrentNodeId = "start",
            Progress = 0,
            InputJson = task.Input,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.WorkflowInstanceRepository.CreateAsync(instance);

        // Step 3: 更新 Task 关联 WorkflowInstance
        task.WorkflowInstanceId = instanceId;
        task.Status = Core.Entities.TaskStatus.Running;
        await _context.TaskRepository.UpdateAsync(task);

        // Step 4: 执行 Agent 节点
        instance.CurrentNodeId = "analyze";
        instance.Progress = 25;
        await _context.WorkflowInstanceRepository.UpdateAsync(instance);

        task.AgentId = Guid.Parse("44444444-4444-4444-4444-444444444444");  // RequirementAnalyzer
        task.WorkflowNodeId = "analyze";
        await _context.TaskRepository.UpdateAsync(task);

        // Step 5: 到达 HumanGate 暂停
        instance.CurrentNodeId = "human-gate";
        instance.Progress = 50;
        instance.Status = WorkflowInstanceStatus.Paused;
        await _context.WorkflowInstanceRepository.UpdateAsync(instance);

        task.WorkflowNodeId = "human-gate";
        task.Status = Core.Entities.TaskStatus.Paused;
        await _context.TaskRepository.UpdateAsync(task);

        // Step 6: 审批通过，继续执行
        instance.Status = WorkflowInstanceStatus.Running;
        instance.CurrentNodeId = "execute";
        instance.Progress = 75;
        await _context.WorkflowInstanceRepository.UpdateAsync(instance);

        task.WorkflowNodeId = "execute";
        task.Status = Core.Entities.TaskStatus.Running;
        task.AgentId = Guid.Parse("55555555-5555-5555-5555-555555555555");  // Executor
        await _context.TaskRepository.UpdateAsync(task);

        // Step 7: 完成
        instance.CurrentNodeId = "end";
        instance.Progress = 100;
        instance.Status = WorkflowInstanceStatus.Completed;
        instance.CompletedAtUtc = DateTime.UtcNow;
        instance.OutputJson = """{"result": "completed", "agent": "Executor"}""";
        await _context.WorkflowInstanceRepository.UpdateAsync(instance);

        task.Status = Core.Entities.TaskStatus.Completed;
        task.WorkflowNodeId = "end";
        task.Result = instance.OutputJson;
        task.CompletedAtUtc = DateTime.UtcNow;
        await _context.TaskRepository.UpdateAsync(task);

        // Assert - 验证完整流程
        var finalTask = await _context.TaskRepository.GetByIdAsync(taskId);
        var finalInstance = await _context.WorkflowInstanceRepository.GetByIdAsync(instanceId);
        var finalWorkflow = await _context.WorkflowRepository.GetByIdAsync(workflowId);

        Assert.NotNull(finalTask);
        Assert.NotNull(finalInstance);
        Assert.NotNull(finalWorkflow);

        Assert.Equal(workflowId, finalTask.WorkflowId);
        Assert.Equal(instanceId, finalTask.WorkflowInstanceId);
        Assert.Equal("end", finalTask.WorkflowNodeId);
        Assert.Equal(Core.Entities.TaskStatus.Completed, finalTask.Status);

        Assert.Equal(workflowId, finalInstance.WorkflowId);
        Assert.Equal("end", finalInstance.CurrentNodeId);
        Assert.Equal(WorkflowInstanceStatus.Completed, finalInstance.Status);
        Assert.Equal(100, finalInstance.Progress);

        Assert.Equal("Issue处理流程", finalWorkflow.Name);

        Console.WriteLine($"[测试14] 完整 Task-Workflow 关联流程:");
        Console.WriteLine($"  Task: {finalTask.Title}");
        Console.WriteLine($"  Workflow: {finalWorkflow.Name}");
        Console.WriteLine($"  Final Status: {finalTask.Status}");
        Console.WriteLine($"  Final Node: {finalTask.WorkflowNodeId}");
        Console.WriteLine($"  Agent Used: {finalTask.AgentId}");
    }

    #endregion

    #region 测试组6：Workflow 节点解析

    /// <summary>
    /// 测试解析 Workflow 节点获取 Agent 配置
    /// </summary>
    [Fact]
    public async Task Workflow_Should_ParseNodesAndGetAgentConfig()
    {
        // Arrange - 先创建一个带有节点的 Workflow
        var workflowId = Guid.NewGuid();
        var nodesJson = GetIssueWorkflowNodes();
        var edgesJson = GetIssueWorkflowEdges();

        var workflow = new WorkflowEntity
        {
            Id = workflowId,
            NtId = "test-user",
            Name = "Issue处理流程",
            Version = 1,
            Status = WorkflowStatus.Published,
            NodesJson = nodesJson,
            EdgesJson = edgesJson,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.WorkflowRepository.CreateAsync(workflow);

        // Act
        var savedWorkflow = await _context.WorkflowRepository.GetByIdAsync(workflowId);
        Assert.NotNull(savedWorkflow);
        Assert.NotNull(savedWorkflow.NodesJson);

        // 解析节点 JSON
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var nodes = System.Text.Json.JsonSerializer.Deserialize<List<WorkflowNode>>(savedWorkflow.NodesJson, options);
        Assert.NotNull(nodes);

        // 找到 Agent 节点
        var agentNodes = nodes.Where(n => n.Type != null && n.Type.ToLower() == "agent").ToList();
        Assert.NotEmpty(agentNodes);

        // 验证 Agent 节点配置
        var analyzeNode = agentNodes.FirstOrDefault(n => n.Id == "analyze");
        Assert.NotNull(analyzeNode);
        Assert.NotNull(analyzeNode.Data);

        var executeNode = agentNodes.FirstOrDefault(n => n.Id == "execute");
        Assert.NotNull(executeNode);
        Assert.NotNull(executeNode.Data);

        // 找到 HumanGate 节点
        var humanGateNodes = nodes.Where(n => n.Type == "human-gate").ToList();
        Assert.NotEmpty(humanGateNodes);

        var gateNode = humanGateNodes.First();
        Assert.NotNull(gateNode.Data);

        Console.WriteLine($"[测试15] Workflow 节点解析:");
        Console.WriteLine($"  Agent Nodes: {agentNodes.Count}");
        Console.WriteLine($"  - analyze: AgentId={analyzeNode.Data.AgentId}");
        Console.WriteLine($"  - execute: AgentId={executeNode.Data.AgentId}");
        Console.WriteLine($"  HumanGate Nodes: {humanGateNodes.Count}");
        Console.WriteLine($"  - {gateNode.Id}: GateType={gateNode.Data.GateType}");
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 获取 Issue Workflow 的节点 JSON
    /// </summary>
    private static string GetIssueWorkflowNodes()
    {
        return """
        [
          {
            "id": "start",
            "type": "start",
            "position": { "x": 250, "y": 0 },
            "data": { "label": "开始", "description": "接收 Issue" }
          },
          {
            "id": "analyze",
            "type": "agent",
            "position": { "x": 250, "y": 100 },
            "data": {
              "label": "分析 Issue",
              "agentId": "44444444-4444-4444-4444-444444444444",
              "agentName": "RequirementAnalyzer",
              "action": "analyze_issue",
              "timeout": 300
            }
          },
          {
            "id": "human-gate",
            "type": "human-gate",
            "position": { "x": 250, "y": 200 },
            "data": {
              "label": "人工审批",
              "gateType": "approval",
              "assignee": "22222222-2222-2222-2222-222222222222"
            }
          },
          {
            "id": "execute",
            "type": "agent",
            "position": { "x": 250, "y": 300 },
            "data": {
              "label": "执行任务",
              "agentId": "55555555-5555-5555-5555-555555555555",
              "agentName": "Executor",
              "action": "execute_task",
              "timeout": 600
            }
          },
          {
            "id": "end",
            "type": "end",
            "position": { "x": 250, "y": 400 },
            "data": { "label": "完成" }
          }
        ]
        """;
    }

    /// <summary>
    /// 获取 Issue Workflow 的边 JSON
    /// </summary>
    private static string GetIssueWorkflowEdges()
    {
        return """
        [
          { "id": "e1", "source": "start", "target": "analyze", "type": "sequential" },
          { "id": "e2", "source": "analyze", "target": "human-gate", "type": "conditional", "condition": "needsApproval" },
          { "id": "e3", "source": "human-gate", "target": "execute", "type": "approval" },
          { "id": "e4", "source": "execute", "target": "end", "type": "sequential" }
        ]
        """;
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}

/// <summary>
/// Workflow 节点 JSON 结构
/// </summary>
public class WorkflowNode
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public WorkflowNodeData? Data { get; set; }
}

/// <summary>
/// Workflow 节点数据
/// </summary>
public class WorkflowNodeData
{
    public string? Label { get; set; }
    public string? AgentId { get; set; }
    public string? AgentName { get; set; }
    public string? Action { get; set; }
    public int Timeout { get; set; }
    public string? GateType { get; set; }
    public string? Assignee { get; set; }
}
