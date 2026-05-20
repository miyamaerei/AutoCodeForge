using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Repository for repository entity CRUD operations with user isolation.
/// </summary>
public class RepositoryRepository : BaseRepository<RepositoryEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public RepositoryRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets a repository by name for the current user.
    /// </summary>
    /// <param name="name">The repository name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The repository entity, or null if not found.</returns>
    public virtual async Task<RepositoryEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable.FirstAsync(r => r.Name == name);
    }

    /// <summary>
    /// Gets a repository by URL for the current user.
    /// </summary>
    /// <param name="url">The repository URL.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The repository entity, or null if not found.</returns>
    public virtual async Task<RepositoryEntity?> GetByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable.FirstAsync(r => r.Url == url);
    }
}
