using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

public class AgentRegistryService : IAgentRegistryService
{
    private readonly AgentRegistrationRepository _registrationRepository;
    private readonly AgentRepository _agentRepository;
    private readonly ITaskEventPublisher _eventPublisher;

    public AgentRegistryService(
        AgentRegistrationRepository registrationRepository,
        AgentRepository agentRepository,
        ITaskEventPublisher eventPublisher)
    {
        _registrationRepository = registrationRepository;
        _agentRepository = agentRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<AgentRegistrationEntity> RegisterAgentAsync(
        Guid agentId,
        string serverId,
        string instanceId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _registrationRepository.GetByAgentIdAsync(agentId, cancellationToken);
        
        if (existing != null)
        {
            existing.ServerId = serverId;
            existing.InstanceId = instanceId;
            existing.LastHeartbeat = DateTime.UtcNow;
            existing.Status = AgentRegistrationStatus.Online;
            await _registrationRepository.UpdateAsync(existing, cancellationToken);
            return existing;
        }

        var registration = new AgentRegistrationEntity
        {
            AgentId = agentId,
            ServerId = serverId,
            InstanceId = instanceId,
            LastHeartbeat = DateTime.UtcNow,
            Status = AgentRegistrationStatus.Online,
            RegisteredAt = DateTime.UtcNow
        };

        await _registrationRepository.CreateAsync(registration, cancellationToken);
        return registration;
    }

    public async Task<bool> RenewHeartbeatAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        return await _registrationRepository.UpdateHeartbeatAsync(agentId, cancellationToken);
    }

    public async Task<bool> DeregisterAgentAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var registration = await _registrationRepository.GetByAgentIdAsync(agentId, cancellationToken);
        if (registration == null)
            return false;

        registration.Status = AgentRegistrationStatus.Offline;
        await _registrationRepository.UpdateAsync(registration, cancellationToken);
        
        await _agentRepository.SetStateAsync(agentId, AgentState.Idle, cancellationToken);
        
        return true;
    }

    public Task<List<AgentRegistrationEntity>> GetAvailableAgentsAsync(CancellationToken cancellationToken = default)
    {
        return _registrationRepository.GetOnlineAgentsAsync(cancellationToken);
    }

    public Task<List<AgentRegistrationEntity>> GetAgentsByServerIdAsync(string serverId, CancellationToken cancellationToken = default)
    {
        return _registrationRepository.GetAgentsByServerIdAsync(serverId, cancellationToken);
    }

    public Task<AgentRegistrationEntity?> GetAgentRegistrationAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        return _registrationRepository.GetByAgentIdAsync(agentId, cancellationToken);
    }
}