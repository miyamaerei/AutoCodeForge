using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

public class TaskReviewRepository : BaseRepository<TaskReviewEntity>
{
    public TaskReviewRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    public async Task<List<TaskReviewEntity>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(review => review.TaskId == taskId)
            .OrderByDescending(review => review.ReviewedAtUtc)
            .ToListAsync();
    }

    public async Task<List<TaskReviewEntity>> GetByTaskStepIdAsync(Guid taskStepId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(review => review.TaskStepId == taskStepId)
            .OrderBy(review => review.ReviewedAtUtc)
            .ToListAsync();
    }

    public async Task<List<TaskReviewEntity>> GetPendingReviewsAsync(Guid reviewerAgentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(review => review.ReviewerAgentId == reviewerAgentId)
            .OrderBy(review => review.ReviewedAtUtc)
            .ToListAsync();
    }

    public async Task<TaskReviewEntity?> GetLatestReviewAsync(Guid taskStepId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(review => review.TaskStepId == taskStepId)
            .OrderByDescending(review => review.ReviewedAtUtc)
            .FirstAsync();
    }

    public async Task<int> CountRejectedReviewsAsync(Guid taskStepId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(review => review.TaskStepId == taskStepId && review.Verdict == ReviewVerdict.Rejected)
            .CountAsync();
    }
}