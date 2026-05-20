using AutoCodeForge.Core.DTOs.Review;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Logging;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Infrastructure.BackgroundServices.Handlers;

/// <summary>
/// Handles background execution for review tasks.
/// </summary>
public class ReviewTaskHandler
{
    private readonly TaskRepository _taskRepository;
    private readonly TaskLogRepository _taskLogRepository;
    private readonly ReviewRepository _reviewRepository;
    private readonly IReviewEngine _reviewEngine;
    private readonly ILogger<ReviewTaskHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewTaskHandler"/> class.
    /// </summary>
    /// <param name="taskRepository">The task repository.</param>
    /// <param name="taskLogRepository">The task log repository.</param>
    /// <param name="reviewRepository">The review repository.</param>
    /// <param name="reviewEngine">The review engine.</param>
    /// <param name="logger">The logger instance.</param>
    public ReviewTaskHandler(
        TaskRepository taskRepository,
        TaskLogRepository taskLogRepository,
        ReviewRepository reviewRepository,
        IReviewEngine reviewEngine,
        ILogger<ReviewTaskHandler> logger)
    {
        _taskRepository = taskRepository;
        _taskLogRepository = taskLogRepository;
        _reviewRepository = reviewRepository;
        _reviewEngine = reviewEngine;
        _logger = logger;
    }

    /// <summary>
    /// Executes one review task.
    /// </summary>
    /// <param name="task">The linked task entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task ExecuteAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        var reviewTask = await _reviewRepository.GetByTaskIdAsync(task.Id, true, cancellationToken)
            ?? throw new ValidationException("Review task record not found");
        var snapshot = JsonHelper.Deserialize<ReviewExecutionSnapshotDto>(reviewTask.SnapshotJson)
            ?? throw new ValidationException("Invalid review snapshot payload");

        try
        {
            await EnsureNotCanceledByUserAsync(task.Id, cancellationToken);

            reviewTask.Status = ReviewTaskStatus.Running;
            reviewTask.StartedAtUtc = DateTime.UtcNow;
            reviewTask.ErrorMessage = null;
            await _reviewRepository.UpdateAsync(reviewTask, cancellationToken);
            await AddLogAsync(task.Id, "Info", "Review execution started", cancellationToken);

            var result = await _reviewEngine.ExecuteAsync(new ReviewExecutionRequestDto
            {
                WorkspacePath = snapshot.WorkspacePath,
                Rules = snapshot.Rules,
            }, cancellationToken);

            await EnsureNotCanceledByUserAsync(task.Id, cancellationToken);

            var findingEntities = result.Findings.Select(item => new ReviewFindingEntity
            {
                Id = Guid.NewGuid(),
                ReviewTaskId = reviewTask.Id,
                NtId = reviewTask.NtId,
                Severity = item.Severity,
                RuleCode = item.RuleCode,
                FilePath = item.FilePath,
                LineNumber = item.LineNumber,
                Message = item.Message,
                Suggestion = item.Suggestion,
                Evidence = item.Evidence,
            }).ToList();

            await _reviewRepository.ReplaceFindingsAsync(reviewTask.Id, findingEntities, cancellationToken);
            var summary = BuildSummary(result.Findings);

            reviewTask.Status = ReviewTaskStatus.Completed;
            reviewTask.SummaryJson = JsonHelper.Serialize(summary);
            reviewTask.CompletedAtUtc = DateTime.UtcNow;
            reviewTask.ErrorMessage = result.Errors.Count == 0 ? null : string.Join("; ", result.Errors);
            await _reviewRepository.UpdateAsync(reviewTask, cancellationToken);

            task.Status = AutoCodeForge.Core.Entities.TaskStatus.Completed;
            task.Progress = 100;
            task.Result = JsonHelper.Serialize(new
            {
                summary.TotalFindings,
                summary.CriticalCount,
                summary.HighCount,
                summary.MediumCount,
                summary.LowCount,
                summary.InfoCount,
            });
            task.ErrorMessage = reviewTask.ErrorMessage;
            task.CompletedAtUtc = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task, cancellationToken);
            await AddLogAsync(task.Id, "Info", "Review execution completed", cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            const string canceledMessage = "Review task canceled by user";
            reviewTask.Status = ReviewTaskStatus.Canceled;
            reviewTask.ErrorMessage = canceledMessage;
            reviewTask.CompletedAtUtc = DateTime.UtcNow;
            await _reviewRepository.UpdateAsync(reviewTask, cancellationToken);

            task.Status = AutoCodeForge.Core.Entities.TaskStatus.Canceled;
            task.ErrorMessage = canceledMessage;
            task.CompletedAtUtc = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task, cancellationToken);
            await AddLogAsync(task.Id, "Info", canceledMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            var sanitized = LogSanitizer.Sanitize(ex.Message);
            _logger.LogError(ex, "Review execution failed for task {TaskId}: {Error}", task.Id, sanitized);

            reviewTask.Status = ReviewTaskStatus.Failed;
            reviewTask.ErrorMessage = sanitized;
            reviewTask.CompletedAtUtc = DateTime.UtcNow;
            await _reviewRepository.UpdateAsync(reviewTask, cancellationToken);

            task.Status = AutoCodeForge.Core.Entities.TaskStatus.Failed;
            task.ErrorMessage = sanitized;
            task.CompletedAtUtc = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task, cancellationToken);
            await AddLogAsync(task.Id, "Error", sanitized, cancellationToken);
        }
    }

    private async Task EnsureNotCanceledByUserAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var latest = await _taskRepository.GetByIdAsync(taskId, true, cancellationToken)
            ?? throw new ValidationException("Task not found during review execution");

        if (latest.Status == AutoCodeForge.Core.Entities.TaskStatus.Canceled)
        {
            throw new OperationCanceledException("Review task canceled by user");
        }
    }

    private static ReviewSummarySnapshotDto BuildSummary(IReadOnlyList<ReviewFindingDto> findings)
    {
        return new ReviewSummarySnapshotDto
        {
            TotalFindings = findings.Count,
            CriticalCount = findings.Count(item => item.Severity == ReviewFindingSeverity.Critical),
            HighCount = findings.Count(item => item.Severity == ReviewFindingSeverity.High),
            MediumCount = findings.Count(item => item.Severity == ReviewFindingSeverity.Medium),
            LowCount = findings.Count(item => item.Severity == ReviewFindingSeverity.Low),
            InfoCount = findings.Count(item => item.Severity == ReviewFindingSeverity.Info),
        };
    }

    private async Task AddLogAsync(Guid taskId, string level, string message, CancellationToken cancellationToken)
    {
        await _taskLogRepository.CreateAsync(new TaskLogEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Level = level,
            Message = message,
            Source = nameof(ReviewTaskHandler),
        }, cancellationToken);
    }
}