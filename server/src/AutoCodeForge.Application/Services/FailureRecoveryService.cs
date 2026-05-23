using AutoCodeForge.Application.Configuration;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

namespace AutoCodeForge.Application.Services;

public class FailureRecoveryService
{
    private readonly TaskStepRepository _taskStepRepository;
    private readonly AgentRepository _agentRepository;
    private readonly ITaskEventPublisher _eventPublisher;
    private readonly RetryPolicySettings _retrySettings;

    public FailureRecoveryService(
        TaskStepRepository taskStepRepository,
        AgentRepository agentRepository,
        ITaskEventPublisher eventPublisher,
        IOptions<RetryPolicySettings> retrySettings)
    {
        _taskStepRepository = taskStepRepository;
        _agentRepository = agentRepository;
        _eventPublisher = eventPublisher;
        _retrySettings = retrySettings.Value;
    }

    public async Task<RecoveryResult> HandleFailureAsync(
        Guid stepId,
        FailureCategory failureCategory,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        var step = await _taskStepRepository.GetByIdAsync(stepId, false, cancellationToken);
        if (step == null)
        {
            return RecoveryResult.Failure("Step not found");
        }

        var policy = _retrySettings.GetPolicy(failureCategory);
        
        if (policy.MaxRetries > 0 && step.RetryCount < policy.MaxRetries)
        {
            return await ExecuteRetryAsync(step, failureCategory, policy, cancellationToken);
        }

        return await TriggerDegradationAsync(step, failureCategory, policy, errorMessage, cancellationToken);
    }

    private async Task<RecoveryResult> ExecuteRetryAsync(
        TaskStepEntity step,
        FailureCategory failureCategory,
        RetryPolicy policy,
        CancellationToken cancellationToken)
    {
        var retryDelay = TimeSpan.FromMilliseconds(policy.IntervalMs * Math.Pow(policy.BackoffMultiplier, step.RetryCount));
        
        await Task.Delay(retryDelay, cancellationToken);
        
        step.RetryCount++;
        step.Status = TaskStepStatus.Pending;
        
        await _taskStepRepository.UpdateAsync(step, cancellationToken);
        
        await _eventPublisher.PublishStepTransitionAsync(step, "Handling", cancellationToken);
        
        return RecoveryResult.Retry(step.RetryCount, (int)retryDelay.TotalMilliseconds);
    }

    private async Task<RecoveryResult> TriggerDegradationAsync(
        TaskStepEntity step,
        FailureCategory failureCategory,
        RetryPolicy policy,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        step.Status = TaskStepStatus.Failed;
        
        await _taskStepRepository.UpdateAsync(step, cancellationToken);
        
        if (step.WorkerAgentId.HasValue)
        {
            await _agentRepository.SetStateAsync(step.WorkerAgentId.Value, AgentState.Idle, cancellationToken);
            await _agentRepository.DecrementTaskCountAsync(step.WorkerAgentId.Value, cancellationToken);
        }
        
        await _eventPublisher.PublishFailureAsync(step.TaskId, step.Id, failureCategory.ToString(), errorMessage, cancellationToken);
        
        return RecoveryResult.Degradation(policy.DegradationAction);
    }

    public async Task<List<TaskStepEntity>> DetectStuckStepsAsync(int timeoutMinutes = 30, CancellationToken cancellationToken = default)
    {
        return await _taskStepRepository.GetTimeoutStepsAsync(timeoutMinutes, cancellationToken);
    }

    public async Task<RecoveryResult> EmergencyUnbindAsync(Guid stepId, CancellationToken cancellationToken = default)
    {
        var step = await _taskStepRepository.GetByIdAsync(stepId, false, cancellationToken);
        if (step == null)
        {
            return RecoveryResult.Failure("Step not found");
        }

        if (step.WorkerAgentId.HasValue)
        {
            await _agentRepository.SetStateAsync(step.WorkerAgentId.Value, AgentState.Idle, cancellationToken);
            await _agentRepository.DecrementTaskCountAsync(step.WorkerAgentId.Value, cancellationToken);
        }

        step.Status = TaskStepStatus.Failed;
        step.CompletedAtUtc = DateTime.UtcNow;
        
        await _taskStepRepository.UpdateAsync(step, cancellationToken);
        
        await _eventPublisher.PublishFailureAsync(step.TaskId, step.Id, FailureCategory.Timeout.ToString(), "Step timeout - emergency unbind", cancellationToken);
        
        return RecoveryResult.Success("Emergency unbind successful");
    }
}

public class RecoveryResult
{
    public RecoveryStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public int RetryDelayMs { get; set; }
    public DegradationAction? DegradationAction { get; set; }

    public static RecoveryResult Success(string message) => new() { Status = RecoveryStatus.Success, Message = message };
    public static RecoveryResult Retry(int retryCount, int delayMs) => new() { Status = RecoveryStatus.Retry, RetryCount = retryCount, RetryDelayMs = delayMs };
    public static RecoveryResult Degradation(DegradationAction action) => new() { Status = RecoveryStatus.Degradation, DegradationAction = action };
    public static RecoveryResult Failure(string message) => new() { Status = RecoveryStatus.Failure, Message = message };
}

public enum RecoveryStatus
{
    Success = 0,
    Retry = 1,
    Degradation = 2,
    Failure = 3
}