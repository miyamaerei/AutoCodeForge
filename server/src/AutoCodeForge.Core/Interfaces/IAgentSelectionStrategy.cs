using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Core.Interfaces;

public interface IAgentSelectionStrategy
{
    Task<AgentEntity?> SelectAgentAsync(Guid taskId, AgentRole role, CancellationToken cancellationToken = default);
    
    Task<AgentEntity?> SelectSecretaryAsync(Guid taskId, CancellationToken cancellationToken = default);
    
    Task<AgentEntity?> SelectManagerAsync(Guid taskId, CancellationToken cancellationToken = default);
    
    Task<AgentEntity?> SelectWorkerAsync(Guid taskId, CancellationToken cancellationToken = default);
}