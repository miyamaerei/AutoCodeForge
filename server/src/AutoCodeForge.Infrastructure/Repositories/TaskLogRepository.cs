using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for task runtime logs.
/// </summary>
public class TaskLogRepository : BaseRepository<TaskLogEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskLogRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public TaskLogRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets all logs for one task ordered by creation time.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Task logs.</returns>
    public async Task<List<TaskLogEntity>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(log => log.TaskId == taskId)
            .OrderBy(log => log.CreatedAtUtc)
            .ToListAsync();
    }
}