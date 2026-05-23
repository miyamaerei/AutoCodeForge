/**
 * FailureRecoveryTests 失败重试机制测试
 *
 * 测试覆盖：
 * 1. FailureRecoveryService - 失败处理、重试、降级
 * 2. Step卡死检测
 * 3. 应急解绑流程
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Tests;

/// <summary>
/// 失败重试机制测试
/// </summary>
public sealed class Intg_FailureRecoveryTests : IDisposable
{
    private readonly IntegrationTestContext _context;

    public Intg_FailureRecoveryTests()
    {
        _context = new IntegrationTestContext("test-failure-recovery-user");
    }

    #region HandleFailure Tests

    /// <summary>
    /// 测试处理失败 - 执行重试
    /// </summary>
    [Fact]
    public async Task HandleFailureAsync_Should_ExecuteRetry_WhenRetryCountNotExceeded()
    {
        // Arrange
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        var step = new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            Step = 1,
            StepType = TaskStepType.Development,
            Status = TaskStepStatus.Handling,
            RetryCount = 0,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskStepRepository.CreateAsync(step);

        // Act
        var result = await _context.FailureRecoveryService.HandleFailureAsync(
            step.Id,
            FailureCategory.CodeError,
            "Test error",
            CancellationToken.None);

        // Assert
        Assert.Equal(RecoveryStatus.Retry, result.Status);
        Assert.Equal(1, result.RetryCount);
        Console.WriteLine("[测试1] 失败处理 - 执行重试成功");
    }

    /// <summary>
    /// 测试处理失败 - 触发降级
    /// </summary>
    [Fact]
    public async Task HandleFailureAsync_Should_TriggerDegradation_WhenRetryCountExceeded()
    {
        // Arrange
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        var step = new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            Step = 1,
            StepType = TaskStepType.Development,
            Status = TaskStepStatus.Handling,
            RetryCount = 5,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskStepRepository.CreateAsync(step);

        // Act
        var result = await _context.FailureRecoveryService.HandleFailureAsync(
            step.Id,
            FailureCategory.CodeError,
            "Test error",
            CancellationToken.None);

        // Assert
        Assert.Equal(RecoveryStatus.Degradation, result.Status);
        Assert.NotNull(result.DegradationAction);
        Console.WriteLine("[测试2] 失败处理 - 触发降级成功");
    }

    /// <summary>
    /// 测试处理失败 - Step不存在
    /// </summary>
    [Fact]
    public async Task HandleFailureAsync_Should_ReturnFailure_WhenStepNotFound()
    {
        // Arrange
        var nonExistentStepId = Guid.NewGuid();

        // Act
        var result = await _context.FailureRecoveryService.HandleFailureAsync(
            nonExistentStepId,
            FailureCategory.CodeError,
            "Test error",
            CancellationToken.None);

        // Assert
        Assert.Equal(RecoveryStatus.Failure, result.Status);
        Assert.Contains("Step not found", result.Message);
        Console.WriteLine("[测试3] 失败处理 - Step不存在返回失败");
    }

    /// <summary>
    /// 测试处理失败 - RequirementIssue不重试
    /// </summary>
    [Fact]
    public async Task HandleFailureAsync_Should_NotRetry_ForRequirementIssue()
    {
        // Arrange
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        var step = new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            Step = 1,
            StepType = TaskStepType.Development,
            Status = TaskStepStatus.Handling,
            RetryCount = 0,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskStepRepository.CreateAsync(step);

        // Act
        var result = await _context.FailureRecoveryService.HandleFailureAsync(
            step.Id,
            FailureCategory.RequirementIssue,
            "Requirement issue",
            CancellationToken.None);

        // Assert
        Assert.Equal(RecoveryStatus.Degradation, result.Status);
        Console.WriteLine("[测试4] 失败处理 - RequirementIssue不重试");
    }

    #endregion

    #region DetectStuckSteps Tests

    /// <summary>
    /// 测试检测卡死步骤 - 超时步骤被检测到
    /// </summary>
    [Fact]
    public async Task DetectStuckStepsAsync_Should_ReturnTimeoutSteps()
    {
        // Arrange
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        // 创建一个超时的步骤（StartedAtUtc设置为31分钟前）
        var stuckStep = new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            Step = 1,
            StepType = TaskStepType.Development,
            Status = TaskStepStatus.Handling,
            StartedAtUtc = DateTime.UtcNow.AddMinutes(-31),
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-31),
            UpdatedAtUtc = DateTime.UtcNow.AddMinutes(-31),
        };
        await _context.TaskStepRepository.CreateAsync(stuckStep);

        // 创建一个正常的步骤
        var normalStep = new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            Step = 2,
            StepType = TaskStepType.TestVerify,
            Status = TaskStepStatus.Handling,
            StartedAtUtc = DateTime.UtcNow.AddMinutes(-5),
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5),
            UpdatedAtUtc = DateTime.UtcNow.AddMinutes(-5),
        };
        await _context.TaskStepRepository.CreateAsync(normalStep);

        // Act
        var stuckSteps = await _context.FailureRecoveryService.DetectStuckStepsAsync(30, CancellationToken.None);

        // Assert
        Assert.Single(stuckSteps);
        Assert.Equal(stuckStep.Id, stuckSteps[0].Id);
        Console.WriteLine("[测试5] 卡死检测 - 超时步骤被检测到");
    }

    #endregion

    #region EmergencyUnbind Tests

    /// <summary>
    /// 测试应急解绑 - 成功解绑
    /// </summary>
    [Fact]
    public async Task EmergencyUnbindAsync_Should_ReleaseAgentAndMarkFailed()
    {
        // Arrange
        var agent = new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Agent",
            State = AgentState.Handling,
            Role = AgentRole.Worker,
            CurrentTaskCount = 1,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.AgentRepository.CreateAsync(agent);

        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        var step = new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            Step = 1,
            StepType = TaskStepType.Development,
            Status = TaskStepStatus.Handling,
            WorkerAgentId = agent.Id,
            StartedAtUtc = DateTime.UtcNow.AddMinutes(-35),
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-35),
            UpdatedAtUtc = DateTime.UtcNow.AddMinutes(-35),
        };
        await _context.TaskStepRepository.CreateAsync(step);

        // Act
        var result = await _context.FailureRecoveryService.EmergencyUnbindAsync(step.Id, CancellationToken.None);

        // Assert
        Assert.Equal(RecoveryStatus.Success, result.Status);
        
        var updatedStep = await _context.TaskStepRepository.GetByIdAsync(step.Id, false, CancellationToken.None);
        Assert.Equal(TaskStepStatus.Failed, updatedStep?.Status);
        
        var updatedAgent = await _context.AgentRepository.GetByIdAsync(agent.Id, cancellationToken: CancellationToken.None);
        Assert.Equal(AgentState.Idle, updatedAgent?.State);
        Assert.Equal(0, updatedAgent?.CurrentTaskCount);
        Console.WriteLine("[测试6] 应急解绑 - 成功释放Agent并标记失败");
    }

    /// <summary>
    /// 测试应急解绑 - Step不存在
    /// </summary>
    [Fact]
    public async Task EmergencyUnbindAsync_Should_ReturnFailure_WhenStepNotFound()
    {
        // Arrange
        var nonExistentStepId = Guid.NewGuid();

        // Act
        var result = await _context.FailureRecoveryService.EmergencyUnbindAsync(nonExistentStepId, CancellationToken.None);

        // Assert
        Assert.Equal(RecoveryStatus.Failure, result.Status);
        Assert.Contains("Step not found", result.Message);
        Console.WriteLine("[测试7] 应急解绑 - Step不存在返回失败");
    }

    #endregion

    #region GetFailureHistory Tests

    /// <summary>
    /// 测试获取失败历史
    /// </summary>
    [Fact]
    public async Task GetFailureHistoryAsync_Should_ReturnFailedSteps()
    {
        // Arrange
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        var failedStep = new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            Step = 1,
            StepType = TaskStepType.Development,
            Status = TaskStepStatus.Failed,
            CompletedAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10),
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskStepRepository.CreateAsync(failedStep);

        // Act
        var history = await _context.FailureRecoveryService.GetFailureHistoryAsync(CancellationToken.None);

        // Assert
        Assert.Single(history);
        Assert.Equal(failedStep.Id, history[0].StepId);
        Console.WriteLine("[测试8] 获取失败历史 - 返回失败步骤");
    }

    #endregion

    #region GetFailureStats Tests

    /// <summary>
    /// 测试获取失败统计
    /// </summary>
    [Fact]
    public async Task GetFailureStatsAsync_Should_ReturnStats()
    {
        // Arrange
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
            TaskType = TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        // 创建多个失败步骤
        for (int i = 0; i < 3; i++)
        {
            var failedStep = new TaskStepEntity
            {
                Id = Guid.NewGuid(),
                TaskId = task.Id,
                Step = i + 1,
                StepType = TaskStepType.Development,
                Status = TaskStepStatus.Failed,
                CompletedAtUtc = DateTime.UtcNow.AddHours(-i),
                CreatedAtUtc = DateTime.UtcNow.AddHours(-i - 1),
                UpdatedAtUtc = DateTime.UtcNow.AddHours(-i),
            };
            await _context.TaskStepRepository.CreateAsync(failedStep);
        }

        // Act
        var stats = await _context.FailureRecoveryService.GetFailureStatsAsync(CancellationToken.None);

        // Assert
        Assert.Equal(3, stats.TotalFailures);
        Assert.Equal(3, stats.Last24Hours);
        Console.WriteLine("[测试9] 获取失败统计 - 返回正确统计数据");
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}