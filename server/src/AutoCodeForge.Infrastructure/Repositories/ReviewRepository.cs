using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides review task and finding queries.
/// </summary>
public class ReviewRepository : BaseRepository<ReviewTaskEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public ReviewRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets a review task by linked task id.
    /// </summary>
    /// <param name="taskId">The linked task identifier.</param>
    /// <param name="includeAllUsers">Whether to bypass user isolation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The review task, or null.</returns>
    public async Task<ReviewTaskEntity?> GetByTaskIdAsync(
        Guid taskId,
        bool includeAllUsers = false,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = includeAllUsers ? QueryableWithoutNtIdFilter : Queryable;
        return await query.FirstAsync(item => item.TaskId == taskId);
    }

    /// <summary>
    /// Gets paged review tasks for a repository.
    /// </summary>
    /// <param name="repositoryId">The repository identifier.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged review tasks.</returns>
    public async Task<PagedResult<ReviewTaskEntity>> GetPagedByRepositoryAsync(
        Guid repositoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
        RefAsync<int> totalCount = 0;
        var items = await Queryable
            .Where(item => item.RepositoryId == repositoryId)
            .OrderByDescending(item => item.UpdatedAtUtc)
            .ToPageListAsync(normalizedPage, normalizedPageSize, totalCount);

        return new PagedResult<ReviewTaskEntity>(items, totalCount, normalizedPage, normalizedPageSize);
    }

    /// <summary>
    /// Gets findings for one review task.
    /// </summary>
    /// <param name="reviewTaskId">The review task identifier.</param>
    /// <param name="includeAllUsers">Whether to bypass user isolation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The findings ordered by severity and location.</returns>
    public async Task<List<ReviewFindingEntity>> GetFindingsAsync(
        Guid reviewTaskId,
        bool includeAllUsers = false,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = includeAllUsers ? Db.Queryable<ReviewFindingEntity>().Where(item => item.IsDeleted == false) : Db.Queryable<ReviewFindingEntity>()
            .Where(item => item.IsDeleted == false && item.NtId == CurrentUserNtId);

        return await query
            .Where(item => item.ReviewTaskId == reviewTaskId)
            .OrderBy(item => item.Severity)
            .OrderBy(item => item.FilePath)
            .OrderBy(item => item.LineNumber)
            .ToListAsync();
    }

    /// <summary>
    /// Replaces findings for one review task.
    /// </summary>
    /// <param name="reviewTaskId">The review task identifier.</param>
    /// <param name="findings">The findings to persist.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task ReplaceFindingsAsync(
        Guid reviewTaskId,
        IReadOnlyCollection<ReviewFindingEntity> findings,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await Db.Deleteable<ReviewFindingEntity>().Where(item => item.ReviewTaskId == reviewTaskId).ExecuteCommandAsync();

        if (findings.Count == 0)
        {
            return;
        }

        await Db.Insertable(findings.ToList()).ExecuteCommandAsync();
    }

    /// <summary>
    /// Soft-deletes all findings for one review task.
    /// </summary>
    /// <param name="reviewTaskId">The review task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task DeleteFindingsAsync(Guid reviewTaskId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await Db.Deleteable<ReviewFindingEntity>().Where(item => item.ReviewTaskId == reviewTaskId).ExecuteCommandAsync();
    }
}