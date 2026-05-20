using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for scheduled task execution records.
/// </summary>
public class ScheduledTaskExecutionRepository : BaseRepository<ScheduledTaskExecutionEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledTaskExecutionRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public ScheduledTaskExecutionRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets paged execution history for one scheduled task.
    /// </summary>
    /// <param name="scheduledTaskId">The scheduled task identifier.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged execution records ordered by start time descending.</returns>
    public async Task<(List<ScheduledTaskExecutionEntity> Items, int Total)> GetPagedByScheduledTaskAsync(
        Guid scheduledTaskId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        int total = 0;
        var items = await Queryable
            .Where(e => e.ScheduledTaskId == scheduledTaskId)
            .OrderBy(e => e.StartedAtUtc, OrderByType.Desc)
            .ToPageListAsync(normalizedPage, normalizedSize, total);

        return (items, total);
    }
}
