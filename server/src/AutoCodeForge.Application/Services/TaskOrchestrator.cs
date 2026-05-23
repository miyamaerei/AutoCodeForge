using AutoCodeForge.Application.Configuration;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

namespace AutoCodeForge.Application.Services;

public class TaskOrchestrator
{
    private readonly IAgentSelectionStrategy _selectionStrategy;
    private readonly AgentRepository _agentRepository;
    private readonly TaskStepRepository _taskStepRepository;
    private readonly OrchestrationSettings _settings;

    public TaskOrchestrator(
        IAgentSelectionStrategy selectionStrategy,
        AgentRepository agentRepository,
        TaskStepRepository taskStepRepository,
        IOptions<OrchestrationSettings> settings)
    {
        _selectionStrategy = selectionStrategy;
        _agentRepository = agentRepository;
        _taskStepRepository = taskStepRepository;
        _settings = settings.Value;
    }

    public async Task<(AgentEntity? Agent, bool UsedEscalation)> AssignTaskAsync(
        Guid taskId, 
        AgentRole requiredRole,
        CancellationToken cancellationToken = default)
    {
        var agent = await _selectionStrategy.SelectAgentAsync(taskId, requiredRole, cancellationToken);

        if (agent != null && await IsWithinCapacity(agent, requiredRole, cancellationToken))
        {
            await AssignAgentToTask(agent.Id, taskId, cancellationToken);
            return (agent, false);
        }

        if (_settings.EnableEscalation)
        {
            var escalationAgent = await TryEscalation(taskId, requiredRole, cancellationToken);
            if (escalationAgent != null)
            {
                await AssignAgentToTask(escalationAgent.Id, taskId, cancellationToken);
                return (escalationAgent, true);
            }
        }

        return (null, false);
    }

    public async Task<(AgentEntity? Agent, bool UsedEscalation)> ReassignTaskAsync(
        Guid taskId,
        Guid currentAgentId,
        CancellationToken cancellationToken = default)
    {
        var currentStep = await _taskStepRepository.GetCurrentStepAsync(taskId, cancellationToken);
        if (currentStep == null)
            return (null, false);

        var currentAgent = await _agentRepository.GetByIdAsync(currentAgentId, false, cancellationToken);
        if (currentAgent == null)
            return (null, false);

        var role = DetermineRoleFromStep(currentStep.StepType.ToString());
        return await AssignTaskAsync(taskId, role, cancellationToken);
    }

    private async Task<bool> IsWithinCapacity(AgentEntity agent, AgentRole role, CancellationToken cancellationToken)
    {
        var currentTaskCount = await _taskStepRepository.CountAgentActiveTasksAsync(agent.Id, cancellationToken);
        
        return role switch
        {
            AgentRole.Manager => currentTaskCount < _settings.MaxConcurrentTasksPerManager,
            AgentRole.Secretary => currentTaskCount < _settings.MaxConcurrentTasksPerSecretary,
            AgentRole.Worker => currentTaskCount < _settings.MaxConcurrentTasksPerWorker,
            _ => true
        };
    }

    private async Task<AgentEntity?> TryEscalation(Guid taskId, AgentRole originalRole, CancellationToken cancellationToken)
    {
        if (originalRole >= AgentRole.Manager)
            return null;

        var escalationRole = (AgentRole)((int)originalRole + 1);
        var agent = await _selectionStrategy.SelectAgentAsync(taskId, escalationRole, cancellationToken);
        
        if (agent != null && await IsWithinCapacity(agent, escalationRole, cancellationToken))
            return agent;

        if (escalationRole < AgentRole.Manager)
            return await TryEscalation(taskId, escalationRole, cancellationToken);

        return null;
    }

    private async Task AssignAgentToTask(Guid agentId, Guid taskId, CancellationToken cancellationToken)
    {
        await _taskStepRepository.AssignAgentAsync(taskId, agentId, cancellationToken);
        await _agentRepository.IncrementTaskCountAsync(agentId, cancellationToken);
    }

    private AgentRole DetermineRoleFromStep(string stepType)
    {
        return stepType.ToLower() switch
        {
            "requirementanalysis" => AgentRole.Secretary,
            "plandesign" => AgentRole.Manager,
            "taskdecomposition" => AgentRole.Secretary,
            "implementation" => AgentRole.Worker,
            "testing" => AgentRole.Worker,
            "deployment" => AgentRole.Manager,
            "finalreview" => AgentRole.Manager,
            _ => AgentRole.Worker
        };
    }
}