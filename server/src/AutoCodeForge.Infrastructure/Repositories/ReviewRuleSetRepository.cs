using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for review rule sets.
/// </summary>
public class ReviewRuleSetRepository : BaseRepository<ReviewRuleSetEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewRuleSetRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public ReviewRuleSetRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets paged rule sets with optional repository filter.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="repositoryId">Optional repository identifier filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged rule sets.</returns>
    public async Task<PagedResult<ReviewRuleSetEntity>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? repositoryId = null,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedPageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
        var query = Queryable;

        if (repositoryId.HasValue)
        {
            query = query.Where(item => item.RepositoryId == repositoryId.Value || item.Level == ReviewRuleSetLevel.Global);
        }

        RefAsync<int> totalCount = 0;
        var items = await query
            .OrderByDescending(item => item.UpdatedAtUtc)
            .ToPageListAsync(normalizedPage, normalizedPageSize, totalCount);

        return new PagedResult<ReviewRuleSetEntity>(items, totalCount, normalizedPage, normalizedPageSize);
    }

    /// <summary>
    /// Gets one enabled global rule set.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The latest enabled global rule set, or null.</returns>
    public async Task<ReviewRuleSetEntity?> GetLatestEnabledGlobalAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(item => item.Level == ReviewRuleSetLevel.Global && item.IsEnabled)
            .OrderByDescending(item => item.UpdatedAtUtc)
            .FirstAsync();
    }
}