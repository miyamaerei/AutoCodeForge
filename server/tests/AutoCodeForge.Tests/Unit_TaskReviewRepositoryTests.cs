/**
 * TaskReviewRepository 数据访问测试
 *
 * 测试覆盖：
 * 1. CreateAsync - 创建审核记录
 * 2. GetByTaskIdAsync - 按任务ID查询审核记录
 * 3. GetLatestReviewAsync - 获取最新审核记录
 */

using AutoCodeForge.Core.Entities;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Tests;

/// <summary>
/// TaskReviewRepository 数据访问测试
/// </summary>
public sealed class Unit_TaskReviewRepositoryTests : IDisposable
{
    private readonly IntegrationTestContext _context;
    private readonly TaskReviewRepository _repository;

    public Unit_TaskReviewRepositoryTests()
    {
        _context = new IntegrationTestContext("test-user");
        _repository = new TaskReviewRepository(_context.Db, _context.CurrentUser);
    }

    [Fact]
    public async Task CreateAsync_Should_SaveReview()
    {
        var review = new TaskReviewEntity
        {
            Id = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            TaskStepId = Guid.NewGuid(),
            ReviewerAgentId = Guid.NewGuid(),
            Verdict = ReviewVerdict.Approved,
            Comment = "Approved",
            ReviewedAtUtc = DateTime.UtcNow,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        var result = await _repository.CreateAsync(review);

        Assert.NotNull(result);
        Assert.Equal(review.Id, result.Id);
        Assert.Equal(ReviewVerdict.Approved, result.Verdict);

        var retrieved = await _repository.GetByIdAsync(review.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(review.Comment, retrieved.Comment);
    }

    [Fact]
    public async Task GetByTaskIdAsync_Should_ReturnReviews()
    {
        var taskId = Guid.NewGuid();
        
        await _repository.CreateAsync(new TaskReviewEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            TaskStepId = Guid.NewGuid(),
            ReviewerAgentId = Guid.NewGuid(),
            Verdict = ReviewVerdict.Approved,
            ReviewedAtUtc = DateTime.UtcNow.AddMinutes(-10),
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10),
            UpdatedAtUtc = DateTime.UtcNow.AddMinutes(-10),
        });

        await _repository.CreateAsync(new TaskReviewEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            TaskStepId = Guid.NewGuid(),
            ReviewerAgentId = Guid.NewGuid(),
            Verdict = ReviewVerdict.Rejected,
            ReviewedAtUtc = DateTime.UtcNow,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        var result = await _repository.GetByTaskIdAsync(taskId);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(ReviewVerdict.Rejected, result[0].Verdict);
    }

    [Fact]
    public async Task GetByTaskIdAsync_Should_ReturnEmpty_WhenNoReviews()
    {
        var taskId = Guid.NewGuid();

        var result = await _repository.GetByTaskIdAsync(taskId);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLatestReviewAsync_Should_ReturnMostRecent()
    {
        var taskStepId = Guid.NewGuid();
        
        await _repository.CreateAsync(new TaskReviewEntity
        {
            Id = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            TaskStepId = taskStepId,
            ReviewerAgentId = Guid.NewGuid(),
            Verdict = ReviewVerdict.Approved,
            ReviewedAtUtc = DateTime.UtcNow.AddMinutes(-10),
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-10),
            UpdatedAtUtc = DateTime.UtcNow.AddMinutes(-10),
        });

        var latestReview = new TaskReviewEntity
        {
            Id = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            TaskStepId = taskStepId,
            ReviewerAgentId = Guid.NewGuid(),
            Verdict = ReviewVerdict.Rejected,
            ReviewedAtUtc = DateTime.UtcNow,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _repository.CreateAsync(latestReview);

        var result = await _repository.GetLatestReviewAsync(taskStepId);

        Assert.NotNull(result);
        Assert.Equal(latestReview.Id, result.Id);
        Assert.Equal(ReviewVerdict.Rejected, result.Verdict);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}