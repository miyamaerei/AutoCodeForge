using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for pipeline entities.
/// </summary>
public class PipelineRepository : BaseRepository<PipelineEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public PipelineRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets one pipeline by repository and name.
    /// </summary>
    /// <param name="repositoryId">The repository identifier.</param>
    /// <param name="name">The pipeline name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The pipeline entity, or null if not found.</returns>
    public async Task<PipelineEntity?> GetByRepositoryAndNameAsync(
        Guid repositoryId,
        string name,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable.FirstAsync(p => p.RepositoryId == repositoryId && p.Name == name);
    }

    /// <summary>
    /// Gets active pipelines that can be synchronized.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>All active pipelines that have an external identifier.</returns>
    public async Task<List<PipelineEntity>> GetSyncCandidatesAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await QueryableWithoutNtIdFilter
            .Where(p => p.Status == PipelineStatus.Active && p.ExternalPipelineId != null && p.ExternalPipelineId != string.Empty)
            .ToListAsync();
    }
}
