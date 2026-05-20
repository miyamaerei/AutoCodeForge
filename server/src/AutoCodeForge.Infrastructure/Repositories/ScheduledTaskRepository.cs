using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for scheduled tasks.
/// </summary>
public class ScheduledTaskRepository : BaseRepository<ScheduledTaskEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledTaskRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public ScheduledTaskRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets all active scheduled tasks whose next run is due.
    /// </summary>
    /// <param name="utcNow">The current UTC timestamp.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Due active scheduled tasks across all users.</returns>
    public async Task<List<ScheduledTaskEntity>> GetDueTasksAsync(
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await QueryableWithoutNtIdFilter
            .Where(t => t.Status == ScheduleStatus.Active && t.NextRunAtUtc != null && t.NextRunAtUtc <= utcNow)
            .ToListAsync();
    }

    /// <summary>
    /// Updates only the NextRunAtUtc column for a scheduled task.
    /// </summary>
    /// <param name="taskId">The scheduled task identifier.</param>
    /// <param name="nextRunAtUtc">The next run timestamp in UTC.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task UpdateNextRunAsync(
        Guid taskId,
        DateTime? nextRunAtUtc,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await Db.Updateable<ScheduledTaskEntity>()
            .SetColumns(t => t.NextRunAtUtc == nextRunAtUtc)
            .SetColumns(t => t.UpdatedAtUtc == DateTime.UtcNow)
            .Where(t => t.Id == taskId)
            .ExecuteCommandAsync();
    }
}
