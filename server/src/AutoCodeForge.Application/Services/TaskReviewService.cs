using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

public class TaskReviewService
{
    private readonly TaskReviewRepository _taskReviewRepository;
    private readonly TaskStepRepository _taskStepRepository;
    private readonly AgentRepository _agentRepository;

    public TaskReviewService(
        TaskReviewRepository taskReviewRepository,
        TaskStepRepository taskStepRepository,
        AgentRepository agentRepository)
    {
        _taskReviewRepository = taskReviewRepository;
        _taskStepRepository = taskStepRepository;
        _agentRepository = agentRepository;
    }

    public async Task<TaskReviewEntity> ApproveStepAsync(Guid taskId, Guid taskStepId, Guid managerId, string? comment = null, CancellationToken cancellationToken = default)
    {
        await ValidateReviewRequestAsync(taskId, taskStepId, managerId, cancellationToken);

        var review = new TaskReviewEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            TaskStepId = taskStepId,
            ReviewerAgentId = managerId,
            Verdict = ReviewVerdict.Approved,
            Comment = comment,
            ReviewedAtUtc = DateTime.UtcNow
        };

        return await _taskReviewRepository.CreateAsync(review, cancellationToken);
    }

    public async Task<TaskReviewEntity> RejectStepAsync(Guid taskId, Guid taskStepId, Guid managerId, string? reason = null, string? issues = null, CancellationToken cancellationToken = default)
    {
        await ValidateReviewRequestAsync(taskId, taskStepId, managerId, cancellationToken);

        var review = new TaskReviewEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            TaskStepId = taskStepId,
            ReviewerAgentId = managerId,
            Verdict = ReviewVerdict.Rejected,
            Comment = reason,
            Issues = issues,
            ReviewedAtUtc = DateTime.UtcNow
        };

        return await _taskReviewRepository.CreateAsync(review, cancellationToken);
    }

    public async Task<List<TaskReviewEntity>> GetPendingReviewsAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        return await _taskReviewRepository.GetPendingReviewsAsync(managerId, cancellationToken);
    }

    public async Task<List<TaskReviewEntity>> GetTaskReviewsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _taskReviewRepository.GetByTaskIdAsync(taskId, cancellationToken);
    }

    public async Task<TaskReviewEntity?> GetLatestReviewAsync(Guid taskStepId, CancellationToken cancellationToken = default)
    {
        return await _taskReviewRepository.GetLatestReviewAsync(taskStepId, cancellationToken);
    }

    public async Task<int> GetRejectionCountAsync(Guid taskStepId, CancellationToken cancellationToken = default)
    {
        return await _taskReviewRepository.CountRejectedReviewsAsync(taskStepId, cancellationToken);
    }

    private async Task ValidateReviewRequestAsync(Guid taskId, Guid taskStepId, Guid managerId, CancellationToken cancellationToken)
    {
        var step = await _taskStepRepository.GetByIdAsync(taskStepId, false, cancellationToken)
            ?? throw new NotFoundException("Task step not found");

        if (step.TaskId != taskId)
        {
            throw new ValidationException("Task step does not belong to the specified task");
        }

        var manager = await _agentRepository.GetByIdAsync(managerId, false, cancellationToken)
            ?? throw new NotFoundException("Manager agent not found");

        if (manager.Role != AgentRole.Manager)
        {
            throw new ValidationException("Only Manager agents can perform reviews");
        }
    }
}