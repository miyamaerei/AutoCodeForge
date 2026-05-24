using AutoCodeForge.Application.Configuration;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Options;
using AutoCodeForge.Core.Exceptions;

namespace AutoCodeForge.Application.Services;

public class TaskOrchestrator
{
    private readonly IAgentSelectionStrategy _selectionStrategy;
    private readonly AgentRepository _agentRepository;
    private readonly TaskStepRepository _taskStepRepository;
    private readonly TaskRepository _taskRepository;
    private readonly OrchestrationSettings _settings;

    public TaskOrchestrator(
        IAgentSelectionStrategy selectionStrategy,
        AgentRepository agentRepository,
        TaskStepRepository taskStepRepository,
        TaskRepository taskRepository,
        IOptions<OrchestrationSettings> settings)
    {
        _selectionStrategy = selectionStrategy;
        _agentRepository = agentRepository;
        _taskStepRepository = taskStepRepository;
        _taskRepository = taskRepository;
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

    private bool IsWithinCapacityByProperty(AgentEntity agent, AgentRole role)
    {
        return role switch
        {
            AgentRole.Manager => agent.CurrentTaskCount < _settings.MaxConcurrentTasksPerManager,
            AgentRole.Secretary => agent.CurrentTaskCount < _settings.MaxConcurrentTasksPerSecretary,
            AgentRole.Worker => agent.CurrentTaskCount < _settings.MaxConcurrentTasksPerWorker,
            _ => true
        };
    }

    private async Task<AgentEntity?> TryEscalation(Guid taskId, AgentRole originalRole, CancellationToken cancellationToken)
    {
        var escalationOrder = new[] { AgentRole.Worker, AgentRole.Secretary, AgentRole.Manager };
        var currentIndex = Array.IndexOf(escalationOrder, originalRole);
        
        for (int i = currentIndex + 1; i < escalationOrder.Length; i++)
        {
            var escalationRole = escalationOrder[i];
            var agent = await _selectionStrategy.SelectAgentAsync(taskId, escalationRole, cancellationToken);
            
            if (agent != null && await IsWithinCapacity(agent, escalationRole, cancellationToken))
                return agent;
        }

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
                "demandanalyse" => AgentRole.Secretary,
                "querycurrent" => AgentRole.Secretary,
                "makeplan" => AgentRole.Manager,
                "development" => AgentRole.Worker,
                "testverify" => AgentRole.Worker,
                "commitpr" => AgentRole.Worker,
                "finalaudit" => AgentRole.Manager,
                _ => AgentRole.Worker
            };
        }

        public async Task<bool> PauseTaskAsync(Guid taskId, string? reason = null, CancellationToken cancellationToken = default)
        {
            var task = await _taskRepository.GetByIdAsync(taskId, false, cancellationToken)
                ?? throw new NotFoundException("Task not found");

            if (task.Status != Core.Entities.TaskStatus.Running && task.Status != Core.Entities.TaskStatus.Pending)
            {
                throw new ValidationException($"Cannot pause task in {task.Status} state");
            }

            task.Status = Core.Entities.TaskStatus.Paused;
            if (!string.IsNullOrWhiteSpace(reason))
            {
                task.ErrorMessage = reason;
            }
            task.UpdatedAtUtc = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task, cancellationToken);

            var activeStep = await _taskStepRepository.GetActiveStepAsync(taskId, cancellationToken);
            if (activeStep != null && activeStep.WorkerAgentId.HasValue)
            {
                await _agentRepository.SetStateAsync(activeStep.WorkerAgentId.Value, AgentState.Idle, cancellationToken);
                await _agentRepository.DecrementTaskCountAsync(activeStep.WorkerAgentId.Value, cancellationToken);
            }

            return true;
        }

        public async Task<bool> ResumeTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            var task = await _taskRepository.GetByIdAsync(taskId, false, cancellationToken)
                ?? throw new NotFoundException("Task not found");

            if (task.Status != Core.Entities.TaskStatus.Paused)
            {
                throw new ValidationException($"Cannot resume task in {task.Status} state");
            }

            task.Status = Core.Entities.TaskStatus.Running;
            task.UpdatedAtUtc = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task, cancellationToken);

            var activeStep = await _taskStepRepository.GetActiveStepAsync(taskId, cancellationToken);
            if (activeStep != null)
            {
                activeStep.Status = TaskStepStatus.Handling;
                activeStep.StartedAtUtc = DateTime.UtcNow;
                await _taskStepRepository.UpdateAsync(activeStep, cancellationToken);
            }

            return true;
        }

        public async Task<bool> ForceTerminateTaskAsync(Guid taskId, string? reason = null, CancellationToken cancellationToken = default)
        {
            var task = await _taskRepository.GetByIdAsync(taskId, false, cancellationToken)
                ?? throw new NotFoundException("Task not found");

            if (task.Status == Core.Entities.TaskStatus.Completed || task.Status == Core.Entities.TaskStatus.Canceled)
            {
                throw new ValidationException($"Cannot terminate task in {task.Status} state");
            }

            task.Status = Core.Entities.TaskStatus.Canceled;
            task.ErrorMessage = reason ?? "Task was forcibly terminated";
            task.CompletedAtUtc = DateTime.UtcNow;
            task.UpdatedAtUtc = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task, cancellationToken);

            var steps = await _taskStepRepository.GetByTaskIdAsync(taskId, cancellationToken);
            foreach (var step in steps.Where(s => s.Status == TaskStepStatus.Handling))
            {
                step.Status = TaskStepStatus.Failed;
                step.CompletedAtUtc = DateTime.UtcNow;
                await _taskStepRepository.UpdateAsync(step, cancellationToken);

                if (step.WorkerAgentId.HasValue)
                {
                    await _agentRepository.SetStateAsync(step.WorkerAgentId.Value, AgentState.Idle, cancellationToken);
                    await _agentRepository.DecrementTaskCountAsync(step.WorkerAgentId.Value, cancellationToken);
                }
            }

            return true;
        }

        public async Task<bool> UpdateRequirementAsync(Guid taskId, string newRequirement, CancellationToken cancellationToken = default)
        {
            var task = await _taskRepository.GetByIdAsync(taskId, false, cancellationToken)
                ?? throw new NotFoundException("Task not found");

            if (task.Status == Core.Entities.TaskStatus.Completed || task.Status == Core.Entities.TaskStatus.Canceled)
            {
                throw new ValidationException($"Cannot update requirement for task in {task.Status} state");
            }

            task.Input = newRequirement;
            task.UpdatedAtUtc = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task, cancellationToken);

            var activeStep = await _taskStepRepository.GetActiveStepAsync(taskId, cancellationToken);
            if (activeStep != null)
            {
                activeStep.Input = newRequirement;
                activeStep.Version++;
                await _taskStepRepository.UpdateAsync(activeStep, cancellationToken);
            }

            return true;
        }
    }