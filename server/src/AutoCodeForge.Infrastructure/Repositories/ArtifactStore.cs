using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Data;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

public class ArtifactStore : IArtifactStore
{
    private readonly ISqlSugarClient _db;

    public ArtifactStore(ISqlSugarClient db)
    {
        _db = db;
    }

    public async Task<ArtifactContract> StoreArtifactAsync(ArtifactContract artifact, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        
        artifact.Id = Guid.NewGuid();
        artifact.CreatedAtUtc = DateTime.UtcNow;
        
        await _db.Insertable(artifact)
            .Include(t => t.Artifacts)
            .ExecuteCommandAsync();
        
        return artifact;
    }

    public async Task<ArtifactContract?> GetArtifactAsync(Guid artifactId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        
        return await _db.Queryable<ArtifactContract>()
            .Include(t => t.Artifacts)
            .Where(a => a.Id == artifactId)
            .FirstAsync();
    }

    public async Task<List<ArtifactContract>> ListArtifactsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        
        return await _db.Queryable<ArtifactContract>()
            .Include(t => t.Artifacts)
            .Where(a => a.TaskId == taskId)
            .OrderBy(a => a.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<List<ArtifactContract>> ListArtifactsByStepIdAsync(Guid stepId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        
        return await _db.Queryable<ArtifactContract>()
            .Include(t => t.Artifacts)
            .Where(a => a.StepId == stepId)
            .OrderBy(a => a.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task DeleteArtifactAsync(Guid artifactId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        
        await _db.Deleteable<ArtifactContract>()
            .Where(a => a.Id == artifactId)
            .ExecuteCommandAsync();
    }
}