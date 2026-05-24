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
using AutoCodeForge.Core.Exceptions;
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
            _context.TaskRepository,
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

    #region 贯穿式介入测试

    /// <summary>
    /// 测试暂停任务 - Running状态任务可以暂停
    /// </summary>
    [Fact]
    public async Task PauseTaskAsync_Should_PauseRunningTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        // Act
        var result = await _context.TaskOrchestrator.PauseTaskAsync(taskId, "Manual pause");

        // Assert
        Assert.True(result);
        var updatedTask = await _context.TaskRepository.GetByIdAsync(taskId, false);
        Assert.Equal(AutoCodeForge.Core.Entities.TaskStatus.Paused, updatedTask.Status);
        Assert.Equal("Manual pause", updatedTask.ErrorMessage);
        Console.WriteLine("[测试17] Running状态任务成功暂停");
    }

    /// <summary>
    /// 测试暂停任务 - Pending状态任务可以暂停
    /// </summary>
    [Fact]
    public async Task PauseTaskAsync_Should_PausePendingTask()
    {
        // Arrange
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

        // Act
        var result = await _context.TaskOrchestrator.PauseTaskAsync(taskId);

        // Assert
        Assert.True(result);
        var updatedTask = await _context.TaskRepository.GetByIdAsync(taskId, false);
        Assert.Equal(AutoCodeForge.Core.Entities.TaskStatus.Paused, updatedTask.Status);
        Console.WriteLine("[测试18] Pending状态任务成功暂停");
    }

    /// <summary>
    /// 测试暂停任务 - Completed状态任务不能暂停
    /// </summary>
    [Fact]
    public async Task PauseTaskAsync_Should_ThrowException_WhenTaskCompleted()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Completed,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        // Act & Assert
        await Assert.ThrowsAsync<AutoCodeForge.Core.Exceptions.ValidationException>(
            () => _context.TaskOrchestrator.PauseTaskAsync(taskId));
        Console.WriteLine("[测试19] Completed状态任务无法暂停");
    }

    /// <summary>
    /// 测试暂停任务 - 释放绑定的Agent
    /// </summary>
    [Fact]
    public async Task PauseTaskAsync_Should_ReleaseAssignedAgent()
    {
        // Arrange
        var agent = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Worker Agent",
            state: AgentState.Handling,
            role: AgentRole.Worker);

        var taskId = Guid.NewGuid();
        var task = new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
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
            Status = TaskStepStatus.Handling,
            WorkerAgentId = agent.Id,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskStepRepository.CreateAsync(taskStep);

        // Act
        await _context.TaskOrchestrator.PauseTaskAsync(taskId);

        // Assert
        var updatedAgent = await _context.AgentRepository.GetByIdAsync(agent.Id, false);
        Assert.Equal(AgentState.Idle, updatedAgent.State);
        Console.WriteLine("[测试20] 暂停任务时成功释放绑定的Agent");
    }

    /// <summary>
    /// 测试恢复任务 - Paused状态任务可以恢复
    /// </summary>
    [Fact]
    public async Task ResumeTaskAsync_Should_ResumePausedTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Paused,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        // Act
        var result = await _context.TaskOrchestrator.ResumeTaskAsync(taskId);

        // Assert
        Assert.True(result);
        var updatedTask = await _context.TaskRepository.GetByIdAsync(taskId, false);
        Assert.Equal(AutoCodeForge.Core.Entities.TaskStatus.Running, updatedTask.Status);
        Console.WriteLine("[测试21] Paused状态任务成功恢复");
    }

    /// <summary>
    /// 测试恢复任务 - Running状态任务不能恢复
    /// </summary>
    [Fact]
    public async Task ResumeTaskAsync_Should_ThrowException_WhenTaskRunning()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        // Act & Assert
        await Assert.ThrowsAsync<AutoCodeForge.Core.Exceptions.ValidationException>(
            () => _context.TaskOrchestrator.ResumeTaskAsync(taskId));
        Console.WriteLine("[测试22] Running状态任务无法恢复");
    }

    /// <summary>
    /// 测试强制终止任务 - Running状态任务可以终止
    /// </summary>
    [Fact]
    public async Task ForceTerminateTaskAsync_Should_TerminateRunningTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        // Act
        var result = await _context.TaskOrchestrator.ForceTerminateTaskAsync(taskId, "Manual termination");

        // Assert
        Assert.True(result);
        var updatedTask = await _context.TaskRepository.GetByIdAsync(taskId, false);
        Assert.Equal(AutoCodeForge.Core.Entities.TaskStatus.Canceled, updatedTask.Status);
        Assert.Equal("Manual termination", updatedTask.ErrorMessage);
        Assert.NotNull(updatedTask.CompletedAtUtc);
        Console.WriteLine("[测试23] Running状态任务成功终止");
    }

    /// <summary>
    /// 测试强制终止任务 - 释放所有绑定的Agent
    /// </summary>
    [Fact]
    public async Task ForceTerminateTaskAsync_Should_ReleaseAllAgents()
    {
        // Arrange
        var agent1 = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Worker 1",
            state: AgentState.Handling,
            role: AgentRole.Worker);
        var agent2 = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Worker 2",
            state: AgentState.Handling,
            role: AgentRole.Worker);

        var taskId = Guid.NewGuid();
        var task = new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        var step1 = new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Step = 1,
            StepType = TaskStepType.Development,
            Status = TaskStepStatus.Handling,
            WorkerAgentId = agent1.Id,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskStepRepository.CreateAsync(step1);

        var step2 = new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Step = 2,
            StepType = TaskStepType.QueryCurrent,
            Status = TaskStepStatus.Handling,
            WorkerAgentId = agent2.Id,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskStepRepository.CreateAsync(step2);

        // Act
        await _context.TaskOrchestrator.ForceTerminateTaskAsync(taskId);

        // Assert
        var updatedAgent1 = await _context.AgentRepository.GetByIdAsync(agent1.Id, false);
        var updatedAgent2 = await _context.AgentRepository.GetByIdAsync(agent2.Id, false);
        Assert.Equal(AgentState.Idle, updatedAgent1.State);
        Assert.Equal(AgentState.Idle, updatedAgent2.State);

        var updatedStep1 = await _context.TaskStepRepository.GetByIdAsync(step1.Id, false);
        var updatedStep2 = await _context.TaskStepRepository.GetByIdAsync(step2.Id, false);
        Assert.Equal(TaskStepStatus.Failed, updatedStep1.Status);
        Assert.Equal(TaskStepStatus.Failed, updatedStep2.Status);
        Console.WriteLine("[测试24] 终止任务时成功释放所有绑定的Agent");
    }

    /// <summary>
    /// 测试强制终止任务 - Completed状态任务不能终止
    /// </summary>
    [Fact]
    public async Task ForceTerminateTaskAsync_Should_ThrowException_WhenTaskCompleted()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Completed,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        // Act & Assert
        await Assert.ThrowsAsync<AutoCodeForge.Core.Exceptions.ValidationException>(
            () => _context.TaskOrchestrator.ForceTerminateTaskAsync(taskId));
        Console.WriteLine("[测试25] Completed状态任务无法终止");
    }

    /// <summary>
    /// 测试更新需求 - Running状态任务可以更新需求
    /// </summary>
    [Fact]
    public async Task UpdateRequirementAsync_Should_UpdateRequirement()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Input = "Original requirement",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        // Act
        var result = await _context.TaskOrchestrator.UpdateRequirementAsync(taskId, "Updated requirement");

        // Assert
        Assert.True(result);
        var updatedTask = await _context.TaskRepository.GetByIdAsync(taskId, false);
        Assert.Equal("Updated requirement", updatedTask.Input);
        Console.WriteLine("[测试26] Running状态任务成功更新需求");
    }

    /// <summary>
    /// 测试更新需求 - 更新活跃步骤的输入
    /// </summary>
    [Fact]
    public async Task UpdateRequirementAsync_Should_UpdateActiveStepInput()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Input = "Original requirement",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
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
            Status = TaskStepStatus.Handling,
            Input = "Original input",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            Version = 1,
        };
        await _context.TaskStepRepository.CreateAsync(taskStep);

        // Act
        await _context.TaskOrchestrator.UpdateRequirementAsync(taskId, "Updated requirement");

        // Assert
        var updatedStep = await _context.TaskStepRepository.GetByIdAsync(taskStep.Id, false);
        Assert.Equal("Updated requirement", updatedStep.Input);
        Assert.Equal(2, updatedStep.Version);
        Console.WriteLine("[测试27] 更新需求时同步更新活跃步骤的输入");
    }

    /// <summary>
    /// 测试更新需求 - Completed状态任务不能更新需求
    /// </summary>
    [Fact]
    public async Task UpdateRequirementAsync_Should_ThrowException_WhenTaskCompleted()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var task = new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Input = "Original requirement",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Completed,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        // Act & Assert
        await Assert.ThrowsAsync<AutoCodeForge.Core.Exceptions.ValidationException>(
            () => _context.TaskOrchestrator.UpdateRequirementAsync(taskId, "Updated requirement"));
        Console.WriteLine("[测试28] Completed状态任务无法更新需求");
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}