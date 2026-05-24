/**
 * TaskReviewService 审核业务逻辑测试
 *
 * 测试覆盖：
 * 1. 审批通过 - 正常场景
 * 2. 审批拒绝 - 正常场景
 * 3. 步骤不存在 - 异常场景
 * 4. 步骤不属于任务 - 异常场景
 * 5. 非Manager角色审批 - 异常场景
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Tests;

/// <summary>
/// TaskReviewService 审核业务逻辑测试
/// </summary>
public sealed class Unit_TaskReviewServiceTests : IDisposable
{
    private readonly IntegrationTestContext _context;
    private readonly TaskReviewService _service;

    public Unit_TaskReviewServiceTests()
    {
        _context = new IntegrationTestContext("test-user");
        _service = new TaskReviewService(
            new TaskReviewRepository(_context.Db, _context.CurrentUser),
            _context.TaskStepRepository,
            _context.AgentRepository);
    }

    #region ApproveStepAsync

    [Fact]
    public async Task ApproveStepAsync_ValidRequest_ReturnsReview()
    {
        var taskId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        await _context.TaskStepRepository.CreateAsync(new TaskStepEntity
        {
            Id = stepId,
            TaskId = taskId,
            Step = 1,
            Status = TaskStepStatus.Handling,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        await _context.AgentRepository.CreateAsync(new AgentEntity
        {
            Id = managerId,
            Name = "Manager Agent",
            Role = AgentRole.Manager,
            IsEnabled = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        var result = await _service.ApproveStepAsync(taskId, stepId, managerId, "Approved");

        Assert.NotNull(result);
        Assert.Equal(ReviewVerdict.Approved, result.Verdict);
        Assert.Equal("Approved", result.Comment);
        Assert.Equal(stepId, result.TaskStepId);
        Assert.Equal(managerId, result.ReviewerAgentId);
    }

    [Fact]
    public async Task ApproveStepAsync_StepNotFound_ThrowsNotFoundException()
    {
        var taskId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.ApproveStepAsync(taskId, stepId, managerId));
    }

    [Fact]
    public async Task ApproveStepAsync_StepNotBelongToTask_ThrowsValidationException()
    {
        var taskId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        await _context.TaskStepRepository.CreateAsync(new TaskStepEntity
        {
            Id = stepId,
            TaskId = Guid.NewGuid(),
            Step = 1,
            Status = TaskStepStatus.Handling,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        await _context.AgentRepository.CreateAsync(new AgentEntity
        {
            Id = managerId,
            Name = "Manager Agent",
            Role = AgentRole.Manager,
            IsEnabled = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.ApproveStepAsync(taskId, stepId, managerId));
    }

    [Fact]
    public async Task ApproveStepAsync_NotManager_ThrowsValidationException()
    {
        var taskId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        await _context.TaskStepRepository.CreateAsync(new TaskStepEntity
        {
            Id = stepId,
            TaskId = taskId,
            Step = 1,
            Status = TaskStepStatus.Handling,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        await _context.AgentRepository.CreateAsync(new AgentEntity
        {
            Id = managerId,
            Name = "Worker Agent",
            Role = AgentRole.Worker,
            IsEnabled = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.ApproveStepAsync(taskId, stepId, managerId));
    }

    #endregion

    #region RejectStepAsync

    [Fact]
    public async Task RejectStepAsync_ValidRequest_ReturnsReview()
    {
        var taskId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var managerId = Guid.NewGuid();

        await _context.TaskStepRepository.CreateAsync(new TaskStepEntity
        {
            Id = stepId,
            TaskId = taskId,
            Step = 1,
            Status = TaskStepStatus.Handling,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        await _context.AgentRepository.CreateAsync(new AgentEntity
        {
            Id = managerId,
            Name = "Manager Agent",
            Role = AgentRole.Manager,
            IsEnabled = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        var result = await _service.RejectStepAsync(taskId, stepId, managerId, "Rejected", "Issue 1");

        Assert.NotNull(result);
        Assert.Equal(ReviewVerdict.Rejected, result.Verdict);
        Assert.Equal("Rejected", result.Comment);
        Assert.Equal("Issue 1", result.Issues);
        Assert.Equal(stepId, result.TaskStepId);
        Assert.Equal(managerId, result.ReviewerAgentId);
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}