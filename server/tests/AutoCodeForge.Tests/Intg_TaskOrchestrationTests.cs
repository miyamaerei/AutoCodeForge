/**
 * TaskOrchestration 任务编排测试
 *
 * 测试覆盖：
 * 1. LeastLoadAgentSelectionStrategy - 最小负载策略选择Agent
 * 2. TaskOrchestrator.AssignTaskAsync - 任务分配
 * 3. TaskOrchestrator.ReassignTaskAsync - 任务重新分配
 * 4. Manager并发上限约束
 * 5. 越级兜底策略
 */

using AutoCodeForge.Application.Configuration;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using Microsoft.Extensions.Options;

namespace AutoCodeForge.Tests;

/// <summary>
/// 任务编排功能测试
/// </summary>
public sealed class Intg_TaskOrchestrationTests : IDisposable
{
    private readonly IntegrationTestContext _context;

    public Intg_TaskOrchestrationTests()
    {
        _context = new IntegrationTestContext("test-orchestration-user");
    }

    #region LeastLoadAgentSelectionStrategy Tests

    /// <summary>
    /// 测试最小负载策略 - 选择任务数最少的Agent
    /// </summary>
    [Fact]
    public async Task SelectAgentAsync_Should_SelectAgentWithLeastTasks()
    {
        // Arrange
        await _context.CreateTestAgentWithLifecycleAsync(
            name: "Agent A",
            state: AgentState.Idle,
            role: AgentRole.Worker);
        await _context.CreateTestAgentWithLifecycleAsync(
            name: "Agent B",
            state: AgentState.Idle,
            role: AgentRole.Worker);

        // 设置不同的任务计数
        var agentA = await _context.AgentRepository.GetByIdAsync(
            (await _context.AgentRepository.GetEnabledAsync()).First(a => a.Name == "Agent A").Id);
        agentA.CurrentTaskCount = 3;
        await _context.AgentRepository.UpdateAsync(agentA);

        var agentB = await _context.AgentRepository.GetByIdAsync(
            (await _context.AgentRepository.GetEnabledAsync()).First(a => a.Name == "Agent B").Id);
        agentB.CurrentTaskCount = 1;
        await _context.AgentRepository.UpdateAsync(agentB);

        // Act
        var selected = await _context.LeastLoadAgentSelectionStrategy.SelectAgentAsync(
            Guid.NewGuid(), AgentRole.Worker);

        // Assert
        Assert.NotNull(selected);
        Assert.Equal("Agent B", selected.Name);
        Console.WriteLine("[测试1] 最小负载策略正确选择了任务数最少的Agent: " + selected.Name);
    }

    /// <summary>
    /// 测试最小负载策略 - 没有可用Agent时返回null
    /// </summary>
    [Fact]
    public async Task SelectAgentAsync_Should_ReturnNull_WhenNoAvailableAgent()
    {
        // Arrange - 创建一个非Idle状态的Agent
        await _context.CreateTestAgentWithLifecycleAsync(
            name: "Busy Agent",
            state: AgentState.Handling,
            role: AgentRole.Worker);

        // Act
        var selected = await _context.LeastLoadAgentSelectionStrategy.SelectAgentAsync(
            Guid.NewGuid(), AgentRole.Worker);

        // Assert
        Assert.Null(selected);
        Console.WriteLine("[测试2] 无可用Agent时返回null");
    }

    /// <summary>
    /// 测试选择Secretary角色Agent
    /// </summary>
    [Fact]
    public async Task SelectSecretaryAsync_Should_SelectSecretaryRoleAgent()
    {
        // Arrange
        await _context.CreateTestAgentWithLifecycleAsync(
            name: "Secretary 1",
            state: AgentState.Idle,
            role: AgentRole.Secretary);
        await _context.CreateTestAgentWithLifecycleAsync(
            name: "Worker 1",
            state: AgentState.Idle,
            role: AgentRole.Worker);

        // Act
        var selected = await _context.LeastLoadAgentSelectionStrategy.SelectSecretaryAsync(Guid.NewGuid());

        // Assert
        Assert.NotNull(selected);
        Assert.Equal(AgentRole.Secretary, selected.Role);
        Console.WriteLine("[测试3] 正确选择Secretary角色Agent: " + selected.Name);
    }

    /// <summary>
    /// 测试选择Manager角色Agent
    /// </summary>
    [Fact]
    public async Task SelectManagerAsync_Should_SelectManagerRoleAgent()
    {
        // Arrange
        await _context.CreateTestAgentWithLifecycleAsync(
            name: "Manager 1",
            state: AgentState.Idle,
            role: AgentRole.Manager);
        await _context.CreateTestAgentWithLifecycleAsync(
            name: "Worker 1",
            state: AgentState.Idle,
            role: AgentRole.Worker);

        // Act
        var selected = await _context.LeastLoadAgentSelectionStrategy.SelectManagerAsync(Guid.NewGuid());

        // Assert
        Assert.NotNull(selected);
        Assert.Equal(AgentRole.Manager, selected.Role);
        Console.WriteLine("[测试4] 正确选择Manager角色Agent: " + selected.Name);
    }

    /// <summary>
    /// 测试选择Worker角色Agent
    /// </summary>
    [Fact]
    public async Task SelectWorkerAsync_Should_SelectWorkerRoleAgent()
    {
        // Arrange
        await _context.CreateTestAgentWithLifecycleAsync(
            name: "Worker 1",
            state: AgentState.Idle,
            role: AgentRole.Worker);
        await _context.CreateTestAgentWithLifecycleAsync(
            name: "Manager 1",
            state: AgentState.Idle,
            role: AgentRole.Manager);

        // Act
        var selected = await _context.LeastLoadAgentSelectionStrategy.SelectWorkerAsync(Guid.NewGuid());

        // Assert
        Assert.NotNull(selected);
        Assert.Equal(AgentRole.Worker, selected.Role);
        Console.WriteLine("[测试5] 正确选择Worker角色Agent: " + selected.Name);
    }

    #endregion

    #region TaskOrchestrator Tests

    /// <summary>
    /// 测试任务分配 - 成功分配给Worker角色
    /// </summary>
    [Fact]
    public async Task AssignTaskAsync_Should_SuccessfullyAssignToWorker()
    {
        // Arrange
        await _context.CreateTestAgentWithLifecycleAsync(
            name: "Worker Agent",
            state: AgentState.Idle,
            role: AgentRole.Worker);

        var taskId = Guid.NewGuid();

        // Act
        var (agent, usedEscalation) = await _context.TaskOrchestrator.AssignTaskAsync(taskId, AgentRole.Worker);

        // Assert
        Assert.NotNull(agent);
        Assert.Equal(AgentRole.Worker, agent.Role);
        Assert.False(usedEscalation);
        Console.WriteLine("[测试6] 成功分配任务给Worker: " + agent.Name);
    }

    /// <summary>
    /// 测试任务分配 - 成功分配给Secretary角色
    /// </summary>
    [Fact]
    public async Task AssignTaskAsync_Should_SuccessfullyAssignToSecretary()
    {
        // Arrange
        await _context.CreateTestAgentWithLifecycleAsync(
            name: "Secretary Agent",
            state: AgentState.Idle,
            role: AgentRole.Secretary);

        var taskId = Guid.NewGuid();

        // Act
        var (agent, usedEscalation) = await _context.TaskOrchestrator.AssignTaskAsync(taskId, AgentRole.Secretary);

        // Assert
        Assert.NotNull(agent);
        Assert.Equal(AgentRole.Secretary, agent.Role);
        Assert.False(usedEscalation);
        Console.WriteLine("[测试7] 成功分配任务给Secretary: " + agent.Name);
    }

    /// <summary>
    /// 测试任务分配 - 成功分配给Manager角色
    /// </summary>
    [Fact]
    public async Task AssignTaskAsync_Should_SuccessfullyAssignToManager()
    {
        // Arrange
        await _context.CreateTestAgentWithLifecycleAsync(
            name: "Manager Agent",
            state: AgentState.Idle,
            role: AgentRole.Manager);

        var taskId = Guid.NewGuid();

        // Act
        var (agent, usedEscalation) = await _context.TaskOrchestrator.AssignTaskAsync(taskId, AgentRole.Manager);

        // Assert
        Assert.NotNull(agent);
        Assert.Equal(AgentRole.Manager, agent.Role);
        Assert.False(usedEscalation);
        Console.WriteLine("[测试8] 成功分配任务给Manager: " + agent.Name);
    }

    /// <summary>
    /// 测试任务分配 - 没有可用Agent时返回null
    /// </summary>
    [Fact]
    public async Task AssignTaskAsync_Should_ReturnNull_WhenNoAvailableAgent()
    {
        // Arrange - 不创建任何Agent
        var taskId = Guid.NewGuid();

        // Act
        var (agent, usedEscalation) = await _context.TaskOrchestrator.AssignTaskAsync(taskId, AgentRole.Worker);

        // Assert
        Assert.Null(agent);
        Assert.False(usedEscalation);
        Console.WriteLine("[测试9] 无可用Agent时返回null");
    }

    /// <summary>
    /// 测试Manager并发上限约束
    /// </summary>
    [Fact]
    public async Task AssignTaskAsync_Should_RespectManagerConcurrentLimit()
    {
        // Arrange - 创建一个Manager，创建5个活跃任务步骤来模拟满载
        var manager = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Busy Manager",
            state: AgentState.Idle,
            role: AgentRole.Manager);
        
        // 创建5个活跃的TaskStep来模拟达到并发上限（MaxConcurrentTasksPerManager = 5）
        for (int i = 0; i < 5; i++)
        {
            var taskStep = new TaskStepEntity
            {
                Id = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                Step = 1,
                StepType = TaskStepType.Development,
                Status = TaskStepStatus.Handling,
                WorkerAgentId = manager.Id,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            };
            await _context.TaskStepRepository.CreateAsync(taskStep);
        }

        var taskId = Guid.NewGuid();

        // Act
        var (agent, usedEscalation) = await _context.TaskOrchestrator.AssignTaskAsync(taskId, AgentRole.Manager);

        // Assert - 由于达到并发上限且没有其他Manager，应该返回null
        Assert.Null(agent);
        Console.WriteLine("[测试10] Manager达到并发上限时拒绝分配");
    }

    /// <summary>
    /// 测试越级兜底策略 - Worker满载时升级到Secretary
    /// </summary>
    [Fact]
    public async Task AssignTaskAsync_Should_EscalateToSecretary_WhenWorkerFull()
    {
        // Arrange - Worker满载，Secretary可用
        var worker = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Full Worker",
            state: AgentState.Idle,
            role: AgentRole.Worker);
        
        // 创建3个活跃的TaskStep来模拟Worker达到并发上限（MaxConcurrentTasksPerWorker = 3）
        for (int i = 0; i < 3; i++)
        {
            var taskStep = new TaskStepEntity
            {
                Id = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                Step = 1,
                StepType = TaskStepType.Development,
                Status = TaskStepStatus.Handling,
                WorkerAgentId = worker.Id,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            };
            await _context.TaskStepRepository.CreateAsync(taskStep);
        }

        await _context.CreateTestAgentWithLifecycleAsync(
            name: "Available Secretary",
            state: AgentState.Idle,
            role: AgentRole.Secretary);

        var taskId = Guid.NewGuid();

        // Act
        var (agent, usedEscalation) = await _context.TaskOrchestrator.AssignTaskAsync(taskId, AgentRole.Worker);

        // Assert
        Assert.NotNull(agent);
        Assert.Equal(AgentRole.Secretary, agent.Role);
        Assert.True(usedEscalation);
        Console.WriteLine("[测试11] Worker满载时成功越级到Secretary: " + agent.Name);
    }

    /// <summary>
    /// 测试越级兜底策略 - Worker和Secretary都满载时升级到Manager
    /// </summary>
    [Fact]
    public async Task AssignTaskAsync_Should_EscalateToManager_WhenWorkerAndSecretaryFull()
    {
        // Arrange - Worker和Secretary都满载，Manager可用
        var worker = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Full Worker",
            state: AgentState.Idle,
            role: AgentRole.Worker);
        
        // Worker满载
        for (int i = 0; i < 3; i++)
        {
            var taskStep = new TaskStepEntity
            {
                Id = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                Step = 1,
                StepType = TaskStepType.Development,
                Status = TaskStepStatus.Handling,
                WorkerAgentId = worker.Id,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            };
            await _context.TaskStepRepository.CreateAsync(taskStep);
        }

        var secretary = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Full Secretary",
            state: AgentState.Idle,
            role: AgentRole.Secretary);
        
        // Secretary满载（MaxConcurrentTasksPerSecretary = 10）
        for (int i = 0; i < 10; i++)
        {
            var taskStep = new TaskStepEntity
            {
                Id = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                Step = 1,
                StepType = TaskStepType.Development,
                Status = TaskStepStatus.Handling,
                WorkerAgentId = secretary.Id,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            };
            await _context.TaskStepRepository.CreateAsync(taskStep);
        }

        await _context.CreateTestAgentWithLifecycleAsync(
            name: "Available Manager",
            state: AgentState.Idle,
            role: AgentRole.Manager);

        var taskId = Guid.NewGuid();

        // Act
        var (agent, usedEscalation) = await _context.TaskOrchestrator.AssignTaskAsync(taskId, AgentRole.Worker);

        // Assert
        Assert.NotNull(agent);
        Assert.Equal(AgentRole.Manager, agent.Role);
        Assert.True(usedEscalation);
        Console.WriteLine("[测试12] Worker和Secretary都满载时成功越级到Manager: " + agent.Name);
    }

    /// <summary>
    /// 测试越级兜底策略 - 所有层级都满载时返回null
    /// </summary>
    [Fact]
    public async Task AssignTaskAsync_Should_ReturnNull_WhenAllRolesFull()
    {
        // Arrange - 所有角色都满载
        var worker = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Full Worker",
            state: AgentState.Idle,
            role: AgentRole.Worker);
        
        for (int i = 0; i < 3; i++)
        {
            var taskStep = new TaskStepEntity
            {
                Id = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                Step = 1,
                StepType = TaskStepType.Development,
                Status = TaskStepStatus.Handling,
                WorkerAgentId = worker.Id,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            };
            await _context.TaskStepRepository.CreateAsync(taskStep);
        }

        var secretary = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Full Secretary",
            state: AgentState.Idle,
            role: AgentRole.Secretary);
        
        for (int i = 0; i < 10; i++)
        {
            var taskStep = new TaskStepEntity
            {
                Id = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                Step = 1,
                StepType = TaskStepType.Development,
                Status = TaskStepStatus.Handling,
                WorkerAgentId = secretary.Id,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            };
            await _context.TaskStepRepository.CreateAsync(taskStep);
        }

        var manager = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Full Manager",
            state: AgentState.Idle,
            role: AgentRole.Manager);
        
        for (int i = 0; i < 5; i++)
        {
            var taskStep = new TaskStepEntity
            {
                Id = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                Step = 1,
                StepType = TaskStepType.Development,
                Status = TaskStepStatus.Handling,
                WorkerAgentId = manager.Id,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            };
            await _context.TaskStepRepository.CreateAsync(taskStep);
        }

        var taskId = Guid.NewGuid();

        // Act
        var (agent, usedEscalation) = await _context.TaskOrchestrator.AssignTaskAsync(taskId, AgentRole.Worker);

        // Assert
        Assert.Null(agent);
        Assert.False(usedEscalation);
        Console.WriteLine("[测试13] 所有角色都满载时返回null");
    }

    /// <summary>
    /// 测试越级兜底策略 - 禁用越级时不触发升级
    /// </summary>
    [Fact]
    public async Task AssignTaskAsync_Should_NotEscalate_WhenEscalationDisabled()
    {
        // Arrange - Worker满载，禁用越级
        var worker = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Full Worker",
            state: AgentState.Idle,
            role: AgentRole.Worker);
        
        for (int i = 0; i < 3; i++)
        {
            var taskStep = new TaskStepEntity
            {
                Id = Guid.NewGuid(),
                TaskId = Guid.NewGuid(),
                Step = 1,
                StepType = TaskStepType.Development,
                Status = TaskStepStatus.Handling,
                WorkerAgentId = worker.Id,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            };
            await _context.TaskStepRepository.CreateAsync(taskStep);
        }

        await _context.CreateTestAgentWithLifecycleAsync(
            name: "Available Secretary",
            state: AgentState.Idle,
            role: AgentRole.Secretary);

        // 创建禁用越级的TaskOrchestrator
        var disabledSettings = Options.Create(new OrchestrationSettings { EnableEscalation = false });
        var disabledOrchestrator = new TaskOrchestrator(
            _context.LeastLoadAgentSelectionStrategy,
            _context.AgentRepository,
            _context.TaskStepRepository,
            disabledSettings);

        var taskId = Guid.NewGuid();

        // Act
        var (agent, usedEscalation) = await disabledOrchestrator.AssignTaskAsync(taskId, AgentRole.Worker);

        // Assert
        Assert.Null(agent);
        Assert.False(usedEscalation);
        Console.WriteLine("[测试14] 禁用越级时不会升级到Secretary");
    }

    /// <summary>
    /// 测试任务重新分配
    /// </summary>
    [Fact]
    public async Task ReassignTaskAsync_Should_ReassignToNewAgent()
    {
        // Arrange
        var worker1 = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Worker 1",
            state: AgentState.Handling,
            role: AgentRole.Worker);
        var worker2 = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Worker 2",
            state: AgentState.Idle,
            role: AgentRole.Worker);

        // 创建任务和步骤
        var taskId = Guid.NewGuid();
        var task = new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Pending,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        var taskStep = new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Step = 1,
            StepType = TaskStepType.Development,
            Status = TaskStepStatus.Pending,
            WorkerAgentId = worker1.Id,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskStepRepository.CreateAsync(taskStep);

        // Act
        var (agent, usedEscalation) = await _context.TaskOrchestrator.ReassignTaskAsync(taskId, worker1.Id);

        // Assert
        Assert.NotNull(agent);
        Assert.NotEqual(worker1.Id, agent.Id);
        Console.WriteLine("[测试15] 任务重新分配成功，从Worker 1分配到: " + agent.Name);
    }

    /// <summary>
    /// 测试任务重新分配 - 没有当前步骤时返回null
    /// </summary>
    [Fact]
    public async Task ReassignTaskAsync_Should_ReturnNull_WhenNoCurrentStep()
    {
        // Arrange
        var agent = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Worker",
            state: AgentState.Idle,
            role: AgentRole.Worker);

        var taskId = Guid.NewGuid();

        // Act
        var (resultAgent, usedEscalation) = await _context.TaskOrchestrator.ReassignTaskAsync(taskId, agent.Id);

        // Assert
        Assert.Null(resultAgent);
        Assert.False(usedEscalation);
        Console.WriteLine("[测试16] 没有当前步骤时返回null");
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}