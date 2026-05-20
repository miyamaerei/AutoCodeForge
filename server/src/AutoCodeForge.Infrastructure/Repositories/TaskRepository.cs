using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for tasks.
/// </summary>
public class TaskRepository : BaseRepository<TaskEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public TaskRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets pending tasks ordered by creation time.
    /// </summary>
    /// <param name="take">Maximum number of rows.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Pending tasks.</returns>
    public async Task<List<TaskEntity>> GetPendingAsync(int take, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var normalizedTake = take <= 0 ? 20 : Math.Min(take, 100);

        return await Queryable
            .Where(task => task.Status == AutoCodeForge.Core.Entities.TaskStatus.Pending)
            .OrderBy(task => task.CreatedAtUtc)
            .Take(normalizedTake)
            .ToListAsync();
    }

    /// <summary>
    /// Attempts to atomically mark one task as running.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="startedAtUtc">The start timestamp in UTC.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when updated; otherwise <see langword="false"/>.</returns>
    public async Task<bool> TryMarkRunningAsync(
        Guid taskId,
        DateTime startedAtUtc,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var affected = await Db.Updateable<TaskEntity>()
            .SetColumns(task => task.Status == AutoCodeForge.Core.Entities.TaskStatus.Running)
            .SetColumns(task => task.StartedAtUtc == startedAtUtc)
            .SetColumns(task => task.CompletedAtUtc == null)
            .SetColumns(task => task.UpdatedAtUtc == DateTime.UtcNow)
            .Where(task => task.Id == taskId && task.Status == AutoCodeForge.Core.Entities.TaskStatus.Pending)
            .ExecuteCommandAsync();

        return affected > 0;
    }
}