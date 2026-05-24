using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

public class TaskStepFlowService
{
    private readonly TaskStepRepository _taskStepRepository;
    private readonly TaskRepository _taskRepository;
    private readonly TaskReviewService _taskReviewService;
    private readonly HumanGateService _humanGateService;
    private readonly int _maxRetryCount = 3;
    private readonly int _maxContextTokens = 8192;

    public TaskStepFlowService(
        TaskStepRepository taskStepRepository,
        TaskRepository taskRepository,
        TaskReviewService taskReviewService,
        HumanGateService humanGateService)
    {
        _taskStepRepository = taskStepRepository;
        _taskRepository = taskRepository;
        _taskReviewService = taskReviewService;
        _humanGateService = humanGateService;
    }

    public async Task InitializeStepsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var existingSteps = await _taskStepRepository.GetByTaskIdAsync(taskId, cancellationToken);
        if (existingSteps.Any())
        {
            throw new ValidationException("Steps already initialized for this task");
        }

        var steps = new List<TaskStepEntity>();
        foreach (TaskStepType stepType in Enum.GetValues(typeof(TaskStepType)))
        {
            var stepNumber = (int)stepType;
            steps.Add(new TaskStepEntity
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                Step = stepNumber,
                StepType = stepType,
                Status = stepNumber == 1 ? TaskStepStatus.Handling : TaskStepStatus.Pending,
                StartedAtUtc = stepNumber == 1 ? DateTime.UtcNow : null
            });
        }

        await _taskStepRepository.CreateManyAsync(steps, cancellationToken);
    }

    public async Task<TaskStepEntity?> MoveToNextStepAsync(Guid taskId, Guid stepId, string? comment = null, CancellationToken cancellationToken = default)
    {
        var step = await _taskStepRepository.GetByIdAsync(stepId, false, cancellationToken)
            ?? throw new NotFoundException("Step not found");

        if (step.TaskId != taskId)
        {
            throw new ValidationException("Step does not belong to the task");
        }

        if (step.Status != TaskStepStatus.Handling)
        {
            throw new ValidationException("Only handling steps can be completed");
        }

        step.Status = TaskStepStatus.Completed;
        step.CompletedAtUtc = DateTime.UtcNow;
        await _taskStepRepository.UpdateAsync(step, cancellationToken);

        var allSteps = await _taskStepRepository.GetByTaskIdAsync(taskId, cancellationToken);
        var nextStep = allSteps.FirstOrDefault(s => s.Step == step.Step + 1);

        if (nextStep != null)
        {
            nextStep.Status = TaskStepStatus.Handling;
            nextStep.StartedAtUtc = DateTime.UtcNow;
            
            if (!string.IsNullOrEmpty(step.Output))
            {
                nextStep.Input = TruncateContext(step.Output);
            }

            await _taskStepRepository.UpdateAsync(nextStep, cancellationToken);
        }

        return nextStep;
    }

    public async Task RejectStepAsync(Guid taskId, Guid stepId, string reason, CancellationToken cancellationToken = default)
    {
        var step = await _taskStepRepository.GetByIdAsync(stepId, false, cancellationToken)
            ?? throw new NotFoundException("Step not found");

        if (step.TaskId != taskId)
        {
            throw new ValidationException("Step does not belong to the task");
        }

        if (step.RetryCount >= _maxRetryCount)
        {
            throw new ValidationException("Maximum retry count exceeded");
        }

        step.Status = TaskStepStatus.Pending;
        step.RetryCount++;
        step.CompletedAtUtc = null;
        step.StartedAtUtc = null;
        await _taskStepRepository.UpdateAsync(step, cancellationToken);
    }

    public async Task SkipStepAsync(Guid taskId, Guid stepId, string reason, CancellationToken cancellationToken = default)
    {
        var step = await _taskStepRepository.GetByIdAsync(stepId, false, cancellationToken)
            ?? throw new NotFoundException("Step not found");

        if (step.TaskId != taskId)
        {
            throw new ValidationException("Step does not belong to the task");
        }

        step.Status = TaskStepStatus.Skipped;
        step.SkipReason = reason;
        step.CompletedAtUtc = DateTime.UtcNow;
        await _taskStepRepository.UpdateAsync(step, cancellationToken);

        var allSteps = await _taskStepRepository.GetByTaskIdAsync(taskId, cancellationToken);
        var nextStep = allSteps.FirstOrDefault(s => s.Step == step.Step + 1);

        if (nextStep != null)
        {
            nextStep.Status = TaskStepStatus.Handling;
            nextStep.StartedAtUtc = DateTime.UtcNow;
            await _taskStepRepository.UpdateAsync(nextStep, cancellationToken);
        }
    }

    public async Task<TaskStepEntity?> GetActiveStepAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _taskStepRepository.GetActiveStepAsync(taskId, cancellationToken);
    }

    public async Task<List<TaskStepEntity>> GetCompletedStepsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _taskStepRepository.GetCompletedStepsAsync(taskId, cancellationToken: cancellationToken);
    }

    private string TruncateContext(string context)
    {
        if (context.Length <= _maxContextTokens)
        {
            return context;
        }
        return context.Substring(0, _maxContextTokens) + "...";
    }
}