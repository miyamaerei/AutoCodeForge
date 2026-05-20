using AutoCodeForge.Core.DTOs.Review;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides review task orchestration and query operations.
/// </summary>
public class ReviewService
{
    private readonly TaskRepository _taskRepository;
    private readonly TaskLogRepository _taskLogRepository;
    private readonly RepositoryRepository _repositoryRepository;
    private readonly RepoSandboxWorkspaceRepository _workspaceRepository;
    private readonly ReviewRepository _reviewRepository;
    private readonly ReviewRuleSetRepository _reviewRuleSetRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewService"/> class.
    /// </summary>
    /// <param name="taskRepository">The task repository.</param>
    /// <param name="taskLogRepository">The task log repository.</param>
    /// <param name="repositoryRepository">The repository repository.</param>
    /// <param name="workspaceRepository">The repo sandbox workspace repository.</param>
    /// <param name="reviewRepository">The review repository.</param>
    /// <param name="reviewRuleSetRepository">The review rule set repository.</param>
    public ReviewService(
        TaskRepository taskRepository,
        TaskLogRepository taskLogRepository,
        RepositoryRepository repositoryRepository,
        RepoSandboxWorkspaceRepository workspaceRepository,
        ReviewRepository reviewRepository,
        ReviewRuleSetRepository reviewRuleSetRepository)
    {
        _taskRepository = taskRepository;
        _taskLogRepository = taskLogRepository;
        _repositoryRepository = repositoryRepository;
        _workspaceRepository = workspaceRepository;
        _reviewRepository = reviewRepository;
        _reviewRuleSetRepository = reviewRuleSetRepository;
    }

    /// <summary>
    /// Creates one review task from the latest synced repository workspace.
    /// </summary>
    /// <param name="request">The create request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created review task detail.</returns>
    public async Task<ReviewTaskDetailDto> CreateTaskAsync(
        CreateReviewTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var repository = await _repositoryRepository.GetByIdAsync(request.RepositoryId, false, cancellationToken)
            ?? throw new NotFoundException("Repository not found");
        var workspace = await _workspaceRepository.GetLatestByRepositoryIdAsync(request.RepositoryId, false, cancellationToken)
            ?? throw new ValidationException("No synced repository workspace found. Run repo sync first.");

        var workspacePath = ResolveWorkspacePath(workspace);
        if (!Directory.Exists(workspacePath))
        {
            throw new ValidationException("Synced repository workspace does not exist on disk");
        }

        var rules = await ResolveRulesAsync(request.RuleSetId ?? repository.DefaultReviewRuleSetId, cancellationToken);
        var taskId = Guid.NewGuid();
        var reviewTaskId = Guid.NewGuid();
        var branch = string.IsNullOrWhiteSpace(request.Branch) ? workspace.Branch : request.Branch!.Trim();

        var snapshot = new ReviewExecutionSnapshotDto
        {
            RepositoryId = repository.Id,
            WorkspaceRecordId = workspace.Id,
            WorkspacePath = workspacePath,
            Branch = branch,
            CommitSha = workspace.CommitSha,
            Rules = rules,
        };

        var task = new TaskEntity
        {
            Id = taskId,
            Title = string.IsNullOrWhiteSpace(request.Title)
                ? $"Review: {repository.Name} ({branch ?? workspace.Branch ?? "workspace"})"
                : request.Title.Trim(),
            Description = request.Description?.Trim(),
            Input = JsonHelper.Serialize(new
            {
                repositoryId = repository.Id,
                workspaceId = workspace.Id,
                branch,
            }),
            TaskType = TaskType.Review,
            DomainType = "Review",
            DomainRecordId = reviewTaskId,
            Status = AutoCodeForge.Core.Entities.TaskStatus.Pending,
            Progress = 0,
            WorkspaceRecordId = workspace.Id,
        };

        var reviewTask = new ReviewTaskEntity
        {
            Id = reviewTaskId,
            TaskId = taskId,
            RepositoryId = repository.Id,
            RuleSetId = request.RuleSetId ?? repository.DefaultReviewRuleSetId,
            SnapshotJson = JsonHelper.Serialize(snapshot),
            Status = ReviewTaskStatus.Pending,
        };

        await _taskRepository.CreateAsync(task, cancellationToken);
        await _reviewRepository.CreateAsync(reviewTask, cancellationToken);
        await AddLogAsync(taskId, "Info", "Review task created", nameof(ReviewService), cancellationToken);

        return await GetTaskAsync(taskId, cancellationToken);
    }

    /// <summary>
    /// Gets one review task detail by linked task identifier.
    /// </summary>
    /// <param name="taskId">The linked task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The review task detail.</returns>
    public async Task<ReviewTaskDetailDto> GetTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException("Task not found");

        if (task.TaskType != TaskType.Review)
        {
            throw new ValidationException("Task is not a review task");
        }

        var reviewTask = await _reviewRepository.GetByTaskIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException("Review task not found");

        var snapshot = JsonHelper.Deserialize<ReviewExecutionSnapshotDto>(reviewTask.SnapshotJson);
        var summary = string.IsNullOrWhiteSpace(reviewTask.SummaryJson)
            ? new ReviewSummarySnapshotDto()
            : JsonHelper.Deserialize<ReviewSummarySnapshotDto>(reviewTask.SummaryJson) ?? new ReviewSummarySnapshotDto();

        return new ReviewTaskDetailDto
        {
            ReviewTaskId = reviewTask.Id,
            TaskId = task.Id,
            RepositoryId = reviewTask.RepositoryId,
            Status = reviewTask.Status.ToString(),
            TotalFindings = summary.TotalFindings,
            CriticalCount = summary.CriticalCount,
            HighCount = summary.HighCount,
            MediumCount = summary.MediumCount,
            LowCount = summary.LowCount,
            InfoCount = summary.InfoCount,
            StartedAtUtc = reviewTask.StartedAtUtc,
            CompletedAtUtc = reviewTask.CompletedAtUtc,
            ErrorMessage = reviewTask.ErrorMessage ?? task.ErrorMessage,
            WorkspaceRecordId = snapshot?.WorkspaceRecordId,
            Branch = snapshot?.Branch,
            CommitSha = snapshot?.CommitSha,
        };
    }

    /// <summary>
    /// Cancels one review task.
    /// </summary>
    /// <param name="taskId">The linked task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated review task detail.</returns>
    public async Task<ReviewTaskDetailDto> CancelTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException("Task not found");

        if (task.TaskType != TaskType.Review)
        {
            throw new ValidationException("Task is not a review task");
        }

        if (task.Status is AutoCodeForge.Core.Entities.TaskStatus.Completed
            or AutoCodeForge.Core.Entities.TaskStatus.Failed
            or AutoCodeForge.Core.Entities.TaskStatus.Canceled)
        {
            throw new ValidationException("Task cannot be canceled in current state");
        }

        var reviewTask = await _reviewRepository.GetByTaskIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException("Review task not found");

        task.Status = AutoCodeForge.Core.Entities.TaskStatus.Canceled;
        task.ErrorMessage = "Review task canceled by user";
        task.CompletedAtUtc = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(task, cancellationToken);

        reviewTask.Status = ReviewTaskStatus.Canceled;
        reviewTask.ErrorMessage = task.ErrorMessage;
        reviewTask.CompletedAtUtc = task.CompletedAtUtc;
        await _reviewRepository.UpdateAsync(reviewTask, cancellationToken);
        await AddLogAsync(taskId, "Info", "Review task canceled by user", nameof(ReviewService), cancellationToken);

        return await GetTaskAsync(taskId, cancellationToken);
    }

    /// <summary>
    /// Requeues one completed, failed, or canceled review task with its original snapshot.
    /// </summary>
    /// <param name="taskId">The linked task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The reset review task detail.</returns>
    public async Task<ReviewTaskDetailDto> RerunTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException("Task not found");

        if (task.TaskType != TaskType.Review)
        {
            throw new ValidationException("Task is not a review task");
        }

        if (task.Status is AutoCodeForge.Core.Entities.TaskStatus.Pending or AutoCodeForge.Core.Entities.TaskStatus.Running)
        {
            throw new ValidationException("Only completed, failed, or canceled review tasks can be rerun");
        }

        var reviewTask = await _reviewRepository.GetByTaskIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException("Review task not found");

        task.Status = AutoCodeForge.Core.Entities.TaskStatus.Pending;
        task.Progress = 0;
        task.Result = null;
        task.ErrorMessage = null;
        task.StartedAtUtc = null;
        task.CompletedAtUtc = null;
        await _taskRepository.UpdateAsync(task, cancellationToken);

        reviewTask.Status = ReviewTaskStatus.Pending;
        reviewTask.SummaryJson = null;
        reviewTask.ErrorMessage = null;
        reviewTask.StartedAtUtc = null;
        reviewTask.CompletedAtUtc = null;
        await _reviewRepository.UpdateAsync(reviewTask, cancellationToken);
        await _reviewRepository.DeleteFindingsAsync(reviewTask.Id, cancellationToken);
        await AddLogAsync(taskId, "Info", "Review task requeued", nameof(ReviewService), cancellationToken);

        return await GetTaskAsync(taskId, cancellationToken);
    }

    /// <summary>
    /// Gets findings for one review task by linked task identifier.
    /// </summary>
    /// <param name="taskId">The linked task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Review findings.</returns>
    public async Task<List<ReviewFindingDto>> GetFindingsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var reviewTask = await _reviewRepository.GetByTaskIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException("Review task not found");
        var findings = await _reviewRepository.GetFindingsAsync(reviewTask.Id, false, cancellationToken);

        return findings.Select(item => new ReviewFindingDto
        {
            Severity = item.Severity,
            RuleCode = item.RuleCode,
            FilePath = item.FilePath,
            LineNumber = item.LineNumber,
            Message = item.Message,
            Suggestion = item.Suggestion,
            Evidence = item.Evidence,
        }).ToList();
    }

    /// <summary>
    /// Gets paged review task summaries for one repository.
    /// </summary>
    /// <param name="repositoryId">The repository identifier.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged review task summaries.</returns>
    public async Task<PagedResult<ReviewTaskSummaryDto>> GetRepositoryTasksAsync(
        Guid repositoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var paged = await _reviewRepository.GetPagedByRepositoryAsync(repositoryId, page, pageSize, cancellationToken);
        var items = new List<ReviewTaskSummaryDto>();
        foreach (var reviewTask in paged.Items)
        {
            var summary = string.IsNullOrWhiteSpace(reviewTask.SummaryJson)
                ? new ReviewSummarySnapshotDto()
                : JsonHelper.Deserialize<ReviewSummarySnapshotDto>(reviewTask.SummaryJson) ?? new ReviewSummarySnapshotDto();
            items.Add(new ReviewTaskSummaryDto
            {
                ReviewTaskId = reviewTask.Id,
                TaskId = reviewTask.TaskId,
                RepositoryId = reviewTask.RepositoryId,
                Status = reviewTask.Status.ToString(),
                TotalFindings = summary.TotalFindings,
                CriticalCount = summary.CriticalCount,
                HighCount = summary.HighCount,
                MediumCount = summary.MediumCount,
                LowCount = summary.LowCount,
                InfoCount = summary.InfoCount,
                StartedAtUtc = reviewTask.StartedAtUtc,
                CompletedAtUtc = reviewTask.CompletedAtUtc,
                ErrorMessage = reviewTask.ErrorMessage,
            });
        }

        return new PagedResult<ReviewTaskSummaryDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }

    private async Task AddLogAsync(
        Guid taskId,
        string level,
        string message,
        string source,
        CancellationToken cancellationToken)
    {
        await _taskLogRepository.CreateAsync(new TaskLogEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Level = level,
            Message = message,
            Source = source,
        }, cancellationToken);
    }

    private async Task<IReadOnlyList<ReviewRuleDto>> ResolveRulesAsync(Guid? ruleSetId, CancellationToken cancellationToken)
    {
        if (!ruleSetId.HasValue)
        {
            var latestGlobal = await _reviewRuleSetRepository.GetLatestEnabledGlobalAsync(cancellationToken);
            if (latestGlobal is not null)
            {
                return JsonHelper.Deserialize<List<ReviewRuleDto>>(latestGlobal.RulesJson) ?? GetDefaultRules().ToList();
            }

            return GetDefaultRules();
        }

        var ruleSet = await _reviewRuleSetRepository.GetByIdAsync(ruleSetId.Value, false, cancellationToken)
            ?? throw new NotFoundException("Review rule set not found");

        if (!ruleSet.IsEnabled)
        {
            throw new ValidationException("Review rule set is disabled");
        }

        return JsonHelper.Deserialize<List<ReviewRuleDto>>(ruleSet.RulesJson) ?? GetDefaultRules().ToList();
    }

    private static string ResolveWorkspacePath(RepoSandboxWorkspaceEntity workspace)
    {
        return Path.Combine(workspace.EffectiveSandboxPath, workspace.RelativeRepoPath);
    }

    private static IReadOnlyList<ReviewRuleDto> GetDefaultRules()
    {
        return
        [
            new ReviewRuleDto
            {
                Code = "REVIEW_TODO_001",
                Name = "TODO marker",
                Severity = ReviewFindingSeverity.Low,
                FilePattern = "*",
                ContainsText = "TODO",
                Message = "Source contains TODO marker",
                Suggestion = "Track incomplete work outside committed source when possible",
            },
            new ReviewRuleDto
            {
                Code = "REVIEW_SECRET_001",
                Name = "Potential secret literal",
                Severity = ReviewFindingSeverity.High,
                FilePattern = "*",
                ContainsText = "token=",
                Message = "Potential token literal detected",
                Suggestion = "Move secrets to configuration or vault-backed storage",
            },
        ];
    }
}