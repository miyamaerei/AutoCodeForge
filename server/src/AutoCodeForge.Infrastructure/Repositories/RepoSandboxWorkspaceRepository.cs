using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for repo sandbox workspace records.
/// </summary>
public class RepoSandboxWorkspaceRepository : BaseRepository<RepoSandboxWorkspaceEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RepoSandboxWorkspaceRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public RepoSandboxWorkspaceRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets a workspace record by task id.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="includeAllUsers">Whether to bypass user filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Workspace record or null.</returns>
    public async Task<RepoSandboxWorkspaceEntity?> GetByTaskIdAsync(
        Guid taskId,
        bool includeAllUsers = false,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = includeAllUsers ? QueryableWithoutNtIdFilter : Queryable;
        return await query.FirstAsync(item => item.TaskId == taskId);
    }

    /// <summary>
    /// Gets the latest workspace record for a repository.
    /// </summary>
    /// <param name="repositoryId">The repository identifier.</param>
    /// <param name="includeAllUsers">Whether to bypass user filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The latest workspace record, or null.</returns>
    public async Task<RepoSandboxWorkspaceEntity?> GetLatestByRepositoryIdAsync(
        Guid repositoryId,
        bool includeAllUsers = false,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = includeAllUsers ? QueryableWithoutNtIdFilter : Queryable;
        return await query
            .Where(item => item.RepositoryId == repositoryId)
            .OrderByDescending(item => item.FinishedAtUtc)
            .OrderByDescending(item => item.UpdatedAtUtc)
            .FirstAsync();
    }
}
