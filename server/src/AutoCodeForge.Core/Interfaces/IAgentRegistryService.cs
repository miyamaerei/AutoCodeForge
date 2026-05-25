using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Core.Interfaces;

public interface IAgentRegistryService
{
    Task<AgentRegistrationEntity> RegisterAgentAsync(
        Guid agentId,
        string serverId,
        string instanceId,
        CancellationToken cancellationToken = default);
    
    Task<bool> RenewHeartbeatAsync(Guid agentId, CancellationToken cancellationToken = default);
    
    Task<bool> DeregisterAgentAsync(Guid agentId, CancellationToken cancellationToken = default);
    
    Task<List<AgentRegistrationEntity>> GetAvailableAgentsAsync(CancellationToken cancellationToken = default);
    
    Task<List<AgentRegistrationEntity>> GetAgentsByServerIdAsync(string serverId, CancellationToken cancellationToken = default);
    
    Task<AgentRegistrationEntity?> GetAgentRegistrationAsync(Guid agentId, CancellationToken cancellationToken = default);
}