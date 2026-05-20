using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for Git skill grants.
/// </summary>
public class GitSkillGrantRepository : BaseRepository<GitSkillGrantEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GitSkillGrantRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public GitSkillGrantRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets grant by repository identifier for current user.
    /// </summary>
    /// <param name="repositoryId">Repository identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Grant entity when exists; otherwise null.</returns>
    public async Task<GitSkillGrantEntity?> GetByRepositoryIdAsync(Guid repositoryId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable.FirstAsync(grant => grant.RepositoryId == repositoryId);
    }
}
