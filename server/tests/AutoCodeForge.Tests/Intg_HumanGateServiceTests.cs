/**
 * HumanGateService 业务逻辑测试
 *
 * 测试覆盖：
 * 1. 创建门控 - 正常场景
 * 2. 创建门控 - 无效任务ID
 * 3. 创建门控 - 已存在待处理门控
 * 4. 批准门控
 * 5. 驳回门控（带步骤重置）
 * 6. 修改后批准门控
 * 7. 取消门控
 * 8. 获取待处理门控列表
 * 9. 根据任务ID获取门控列表
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;

namespace AutoCodeForge.Tests;

/// <summary>
/// HumanGateService 业务逻辑测试
/// </summary>
public sealed class HumanGateServiceTests : IDisposable
{
    private readonly IntegrationTestContext _context;

    public HumanGateServiceTests()
    {
        _context = new IntegrationTestContext("test-user");
    }

    #region 创建门控测试

    [Fact]
    public async Task CreateGateAsync_Should_CreateGateSuccessfully()
    {
        // Arrange
        var task = await CreateTestTaskAsync();

        var request = new CreateHumanGateRequest
        {
            TaskId = task.Id,
            GateType = "PlanApproval",
            Reason = "Step 3 completed, awaiting plan approval",
        };

        // Act
        var result = await _context.HumanGateService.CreateGateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(task.Id, result.TaskId);
        Assert.Equal("PlanApproval", result.GateType);
        Assert.Equal("方案审批", result.GateTypeName);
        Assert.Equal("Pending", result.Status);
        Assert.Equal("Step 3 completed, awaiting plan approval", result.Reason);

        // 验证数据库中确实存在
        var retrieved = await _context.HumanGateRepository.GetByIdAsync(result.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(HumanGateType.PlanApproval, retrieved.GateType);
        Assert.Equal(HumanGateStatus.Pending, retrieved.Status);
    }

    [Fact]
    public async Task CreateGateAsync_Should_ThrowNotFoundException_WhenTaskNotFound()
    {
        // Arrange
        var request = new CreateHumanGateRequest
        {
            TaskId = Guid.NewGuid(),
            GateType = "PlanApproval",
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _context.HumanGateService.CreateGateAsync(request));
    }

    [Fact]
    public async Task CreateGateAsync_Should_ThrowValidationException_WhenGateTypeInvalid()
    {
        // Arrange
        var task = await CreateTestTaskAsync();

        var request = new CreateHumanGateRequest
        {
            TaskId = task.Id,
            GateType = "InvalidType",
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _context.HumanGateService.CreateGateAsync(request));
    }

    [Fact]
    public async Task CreateGateAsync_Should_ThrowValidationException_WhenPendingGateExists()
    {
        // Arrange
        var task = await CreateTestTaskAsync();

        // 创建第一个门控
        await _context.HumanGateService.CreateGateAsync(new CreateHumanGateRequest
        {
            TaskId = task.Id,
            GateType = "PlanApproval",
        });

        // 尝试创建第二个待处理门控
        var request = new CreateHumanGateRequest
        {
            TaskId = task.Id,
            GateType = "CodeReview",
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _context.HumanGateService.CreateGateAsync(request));
    }

    #endregion

    #region 批准门控测试

    [Fact]
    public async Task ApproveAsync_Should_UpdateStatusToApproved()
    {
        // Arrange
        var gate = await CreateTestHumanGateAsync();

        var request = new ApproveRequest
        {
            Comment = "Approved by admin",
        };

        // Act
        var result = await _context.HumanGateService.ApproveAsync(gate.Id, request);

        // Assert
        Assert.Equal("Approved", result.Status);
        Assert.Equal("Approved by admin", result.HumanResponse);
        Assert.NotNull(result.RespondedAtUtc);

        // 验证数据库状态
        var updated = await _context.HumanGateRepository.GetByIdAsync(gate.Id);
        Assert.Equal(HumanGateStatus.Approved, updated?.Status);
    }

    [Fact]
    public async Task ApproveAsync_Should_ThrowValidationException_WhenNotPending()
    {
        // Arrange
        var gate = await CreateTestHumanGateAsync();
        await _context.HumanGateService.ApproveAsync(gate.Id, new ApproveRequest());

        // Act & Assert - 再次批准已批准的门控
        await Assert.ThrowsAsync<ValidationException>(
            () => _context.HumanGateService.ApproveAsync(gate.Id, new ApproveRequest()));
    }

    #endregion

    #region 驳回门控测试

    [Fact]
    public async Task RejectAsync_Should_UpdateStatusToRejected()
    {
        // Arrange
        var gate = await CreateTestHumanGateAsync();

        var request = new RejectRequest
        {
            Reason = "Plan needs revision",
        };

        // Act
        var result = await _context.HumanGateService.RejectAsync(gate.Id, request);

        // Assert
        Assert.Equal("Rejected", result.Status);
        Assert.Equal("Plan needs revision", result.HumanResponse);
        Assert.NotNull(result.RespondedAtUtc);
    }

    [Fact]
    public async Task RejectAsync_Should_ResetStep_WhenStepIdProvided()
    {
        // Arrange
        var task = await CreateTestTaskAsync();
        var step = await CreateTestTaskStepAsync(task.Id);

        var gate = await CreateTestHumanGateAsync(task.Id, step.Id);

        var request = new RejectRequest
        {
            Reason = "Step needs to be redone",
        };

        // Act
        await _context.HumanGateService.RejectAsync(gate.Id, request);

        // Assert - 验证步骤被重置
        var updatedStep = await _context.TaskStepRepository.GetByIdAsync(step.Id);
        Assert.Equal(TaskStepStatus.Pending, updatedStep?.Status);
        Assert.Equal(1, updatedStep?.RetryCount);
    }

    #endregion

    #region 修改后批准测试

    [Fact]
    public async Task ModifyApproveAsync_Should_UpdateStatusToModified()
    {
        // Arrange
        var gate = await CreateTestHumanGateAsync();

        var request = new ModifyApproveRequest
        {
            Modifications = new GateModifications
            {
                Input = "Modified input",
                Instructions = "Please follow these changes",
            },
        };

        // Act
        var result = await _context.HumanGateService.ModifyApproveAsync(gate.Id, request);

        // Assert
        Assert.Equal("Modified", result.Status);
        Assert.NotNull(result.Modifications);
        Assert.NotNull(result.RespondedAtUtc);
    }

    #endregion

    #region 取消门控测试

    [Fact]
    public async Task CancelAsync_Should_UpdateStatusToCancelled()
    {
        // Arrange
        var gate = await CreateTestHumanGateAsync();

        // Act
        var result = await _context.HumanGateService.CancelAsync(gate.Id);

        // Assert
        Assert.Equal("Cancelled", result.Status);
        Assert.NotNull(result.RespondedAtUtc);
    }

    [Fact]
    public async Task CancelAsync_Should_ThrowValidationException_WhenNotPending()
    {
        // Arrange
        var gate = await CreateTestHumanGateAsync();
        await _context.HumanGateService.ApproveAsync(gate.Id, new ApproveRequest());

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _context.HumanGateService.CancelAsync(gate.Id));
    }

    #endregion

    #region 查询测试

    [Fact]
    public async Task GetPendingGatesAsync_Should_ReturnOnlyPendingGates()
    {
        // Arrange
        await CreateTestHumanGateAsync();
        await CreateTestHumanGateAsync();

        // 批准其中一个门控
        var gates = await _context.HumanGateRepository.GetPendingGatesAsync();
        if (gates.Any())
        {
            await _context.HumanGateService.ApproveAsync(gates[0].Id, new ApproveRequest());
        }

        // Act
        var pendingGates = await _context.HumanGateService.GetPendingGatesAsync();

        // Assert
        Assert.All(pendingGates, gate => Assert.Equal("Pending", gate.Status));
    }

    [Fact]
    public async Task GetByTaskIdAsync_Should_ReturnGatesForTask()
    {
        // Arrange
        var task1 = await CreateTestTaskAsync();
        var task2 = await CreateTestTaskAsync();

        await CreateTestHumanGateAsync(task1.Id);
        await CreateTestHumanGateAsync(task1.Id);
        await CreateTestHumanGateAsync(task2.Id);

        // Act
        var gates = await _context.HumanGateService.GetByTaskIdAsync(task1.Id);

        // Assert
        Assert.Equal(2, gates.Count);
        Assert.All(gates, gate => Assert.Equal(task1.Id, gate.TaskId));
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnGate()
    {
        // Arrange
        var gate = await CreateTestHumanGateAsync();

        // Act
        var result = await _context.HumanGateService.GetByIdAsync(gate.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(gate.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ThrowNotFoundException_WhenGateNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _context.HumanGateService.GetByIdAsync(Guid.NewGuid()));
    }

    #endregion

    #region 辅助方法

    private async Task<TaskEntity> CreateTestTaskAsync()
    {
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Description = "Test Description",
            Input = "Test Input",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Pending,
            Progress = 0,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        return await _context.TaskRepository.CreateAsync(task);
    }

    private async Task<TaskStepEntity> CreateTestTaskStepAsync(Guid taskId)
    {
        var step = new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Step = 3,
            StepType = TaskStepType.MakePlan,
            Status = TaskStepStatus.Completed,
            RetryCount = 0,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        return await _context.TaskStepRepository.CreateAsync(step);
    }

    private async Task<HumanGateEntity> CreateTestHumanGateAsync(
        Guid? taskId = null,
        Guid? stepId = null)
    {
        var task = taskId.HasValue
            ? await _context.TaskRepository.GetByIdAsync(taskId.Value) ?? await CreateTestTaskAsync()
            : await CreateTestTaskAsync();

        var gate = new HumanGateEntity
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            TaskStepId = stepId,
            GateType = HumanGateType.PlanApproval,
            Status = HumanGateStatus.Pending,
            Reason = "Test gate reason",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        return await _context.HumanGateRepository.CreateAsync(gate);
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}