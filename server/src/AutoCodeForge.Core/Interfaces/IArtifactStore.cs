using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Core.Interfaces;

public interface IArtifactStore
{
    Task<ArtifactContract> StoreArtifactAsync(ArtifactContract artifact, CancellationToken cancellationToken = default);
    
    Task<ArtifactContract?> GetArtifactAsync(Guid artifactId, CancellationToken cancellationToken = default);
    
    Task<List<ArtifactContract>> ListArtifactsByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    
    Task<List<ArtifactContract>> ListArtifactsByStepIdAsync(Guid stepId, CancellationToken cancellationToken = default);
    
    Task DeleteArtifactAsync(Guid artifactId, CancellationToken cancellationToken = default);
}