using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for pipeline build entities.
/// </summary>
public class BuildRepository : BaseRepository<BuildEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BuildRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public BuildRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets paged build history for a pipeline.
    /// </summary>
    /// <param name="pipelineId">The pipeline identifier.</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged builds with total count.</returns>
    public async Task<(List<BuildEntity> Items, int Total)> GetPagedByPipelineAsync(
        Guid pipelineId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        int total = 0;
        var items = await Queryable
            .Where(b => b.PipelineId == pipelineId)
            .OrderBy(b => b.TriggeredAtUtc, OrderByType.Desc)
            .ToPageListAsync(normalizedPage, normalizedSize, total);

        return (items, total);
    }

    /// <summary>
    /// Gets the latest build for one pipeline.
    /// </summary>
    /// <param name="pipelineId">The pipeline identifier.</param>
    /// <param name="includeAllUsers">Whether to bypass user filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The latest build entity, or null if not found.</returns>
    public async Task<BuildEntity?> GetLatestByPipelineAsync(
        Guid pipelineId,
        bool includeAllUsers = false,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = includeAllUsers ? QueryableWithoutNtIdFilter : Queryable;
        return await query
            .Where(b => b.PipelineId == pipelineId)
            .OrderByDescending(b => b.TriggeredAtUtc)
            .FirstAsync();
    }
}
