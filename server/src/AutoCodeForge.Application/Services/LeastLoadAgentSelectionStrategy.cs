using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

public class LeastLoadAgentSelectionStrategy : IAgentSelectionStrategy
{
    private readonly AgentRepository _agentRepository;

    public LeastLoadAgentSelectionStrategy(AgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    public async Task<AgentEntity?> SelectAgentAsync(Guid taskId, AgentRole role, CancellationToken cancellationToken = default)
    {
        var agents = await _agentRepository.GetAvailableAgentsByRoleAsync(role, cancellationToken);
        
        if (!agents.Any())
            return null;

        return agents
            .OrderBy(a => a.CurrentTaskCount)
            .FirstOrDefault();
    }

    public Task<AgentEntity?> SelectSecretaryAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return SelectAgentAsync(taskId, AgentRole.Secretary, cancellationToken);
    }

    public Task<AgentEntity?> SelectManagerAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return SelectAgentAsync(taskId, AgentRole.Manager, cancellationToken);
    }

    public Task<AgentEntity?> SelectWorkerAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return SelectAgentAsync(taskId, AgentRole.Worker, cancellationToken);
    }
}