using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for wiki pages.
/// </summary>
public class WikiPageRepository : BaseRepository<WikiPageEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WikiPageRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public WikiPageRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets a wiki page by slug.
    /// </summary>
    /// <param name="slug">The page slug.</param>
    /// <param name="excludeId">Optional identifier to exclude from search.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching page, or null when none exists.</returns>
    public async Task<WikiPageEntity?> GetBySlugAsync(
        string slug,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = Queryable.Where(w => w.Slug == slug);
        if (excludeId.HasValue)
        {
            query = query.Where(w => w.Id != excludeId.Value);
        }

        return await query.FirstAsync();
    }

    /// <summary>
    /// Gets wiki pages by optional keyword and repository filter.
    /// </summary>
    /// <param name="keyword">Optional keyword to search title, slug, and content.</param>
    /// <param name="repositoryId">Optional repository identifier filter.</param>
    /// <param name="page">Requested page number.</param>
    /// <param name="pageSize">Requested page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged wiki page result.</returns>
    public async Task<PagedResult<WikiPageEntity>> SearchPagedAsync(
        string? keyword,
        Guid? repositoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var normalized = PaginationHelper.Normalize(page, pageSize);
        var query = Queryable;

        if (repositoryId.HasValue)
        {
            query = query.Where(w => w.RepositoryId == repositoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var trimmed = keyword.Trim();
            query = query.Where(w =>
                w.Title.Contains(trimmed) ||
                w.Slug.Contains(trimmed) ||
                w.Content.Contains(trimmed));
        }

        RefAsync<int> totalCount = 0;
        var items = await query
            .OrderByDescending(w => w.UpdatedAtUtc)
            .ToPageListAsync(normalized.Page, normalized.PageSize, totalCount);

        return PaginationHelper.ToPagedResult(items, totalCount, normalized.Page, normalized.PageSize);
    }
}