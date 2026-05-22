using AutoCodeForge.Core.DTOs.RepoSync;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides repo sync task creation and detail query operations.
/// </summary>
public class RepoSyncService
{
    private readonly TaskRepository _taskRepository;
    private readonly TaskLogRepository _taskLogRepository;
    private readonly RepositoryRepository _repositoryRepository;
    private readonly RepoSandboxWorkspaceRepository _workspaceRepository;
    private readonly ConfigService _configService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepoSyncService"/> class.
    /// </summary>
    /// <param name="taskRepository">Task repository.</param>
    /// <param name="taskLogRepository">Task log repository.</param>
    /// <param name="repositoryRepository">Repository repository.</param>
    /// <param name="workspaceRepository">Workspace repository.</param>
    /// <param name="configService">Config service.</param>
    public RepoSyncService(
        TaskRepository taskRepository,
        TaskLogRepository taskLogRepository,
        RepositoryRepository repositoryRepository,
        RepoSandboxWorkspaceRepository workspaceRepository,
        ConfigService configService)
    {
        _taskRepository = taskRepository;
        _taskLogRepository = taskLogRepository;
        _repositoryRepository = repositoryRepository;
        _workspaceRepository = workspaceRepository;
        _configService = configService;
    }

    /// <summary>
    /// Creates one repo sync task with sandbox and repository snapshots.
    /// </summary>
    /// <param name="request">Create request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Created task response.</returns>
    public async Task<AutoCodeForge.Core.DTOs.Task.TaskResponse> CreateTaskAsync(
        CreateRepoSyncTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var repository = await _repositoryRepository.GetByIdAsync(request.RepositoryId, false, cancellationToken)
            ?? throw new NotFoundException("Repository not found");

        var sandboxConfig = await _configService.GetSandboxConfigAsync(cancellationToken)
            ?? throw new ValidationException("Sandbox config is missing. Configure sandbox first.");

        var (owner, repoName) = ExtractOwnerRepo(repository.Url);
        if (string.IsNullOrWhiteSpace(owner) && string.IsNullOrWhiteSpace(repoName))
        {
            throw new ValidationException("Repository URL format is invalid. URL cannot be empty.");
        }
        if (string.IsNullOrWhiteSpace(owner))
        {
            throw new ValidationException("Repository URL is missing owner/organization. Please configure the repository with a valid URL.");
        }
        if (string.IsNullOrWhiteSpace(repoName))
        {
            throw new ValidationException("Repository URL is missing repository name. Please configure the repository with a valid URL.");
        }

        var branch = string.IsNullOrWhiteSpace(request.Branch) 
            ? (string.IsNullOrWhiteSpace(repository.Branch) ? "main" : repository.Branch.Trim()) 
            : request.Branch.Trim();

        var sandboxSnapshot = new SandboxSnapshot
        {
            WorkspaceRootPath = sandboxConfig.WorkspaceRootPath,
            AllowedWritePaths = sandboxConfig.AllowedWritePaths,
            TimeoutSeconds = sandboxConfig.TimeoutSeconds,
            UserIsolationEnabled = sandboxConfig.UserIsolationEnabled,
        };

        var repositorySnapshot = new RepositorySnapshot
        {
            RepositoryId = repository.Id,
            Url = repository.Url,
            Provider = repository.Provider,
            EncryptedToken = repository.EncryptedToken,
            Branch = branch,
            Owner = owner,
            RepoName = repoName,
        };

        var taskEntity = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = string.IsNullOrWhiteSpace(request.Title)
                ? $"RepoSync: {owner}/{repoName} ({branch})"
                : request.Title.Trim(),
            Description = request.Description?.Trim(),
            TaskType = TaskType.RepoSyncToSandbox,
            Input = JsonHelper.Serialize(new
            {
                request.RepositoryId,
                branch,
            }),
            Status = AutoCodeForge.Core.Entities.TaskStatus.Pending,
            Progress = 0,
            SandboxSnapshotJson = JsonHelper.Serialize(sandboxSnapshot),
            RepositorySnapshotJson = JsonHelper.Serialize(repositorySnapshot),
        };

        await _taskRepository.CreateAsync(taskEntity, cancellationToken);
        await _taskLogRepository.CreateAsync(new TaskLogEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskEntity.Id,
            Level = "Info",
            Message = "Repo sync task created",
            Source = nameof(RepoSyncService),
        }, cancellationToken);

        return ToTaskResponse(taskEntity);
    }

    /// <summary>
    /// Gets repo sync task detail and workspace state.
    /// </summary>
    /// <param name="taskId">Task id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Repo sync task detail.</returns>
    public async Task<RepoSyncTaskDetailResponse> GetTaskDetailAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException("Task not found");

        if (task.TaskType != TaskType.RepoSyncToSandbox)
        {
            throw new ValidationException("Task is not a repo sync task");
        }

        var workspace = await _workspaceRepository.GetByTaskIdAsync(taskId, false, cancellationToken);
        return new RepoSyncTaskDetailResponse
        {
            TaskId = taskId,
            Status = task.Status.ToString(),
            WorkspaceStatus = workspace?.Status.ToString() ?? RepoSandboxWorkspaceStatus.Ready.ToString(),
            EffectiveSandboxPath = workspace?.EffectiveSandboxPath ?? string.Empty,
            Branch = workspace?.Branch,
            CommitSha = workspace?.CommitSha,
            ErrorMessage = workspace?.ErrorMessage ?? task.ErrorMessage,
        };
    }

    /// <summary>
    /// Cancels one repo sync task.
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Updated task detail response.</returns>
    public async Task<RepoSyncTaskDetailResponse> CancelTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException("Task not found");

        if (task.TaskType != TaskType.RepoSyncToSandbox)
        {
            throw new ValidationException("Task is not a repo sync task");
        }

        if (task.Status is AutoCodeForge.Core.Entities.TaskStatus.Completed
            or AutoCodeForge.Core.Entities.TaskStatus.Failed
            or AutoCodeForge.Core.Entities.TaskStatus.Canceled)
        {
            throw new ValidationException("Task cannot be canceled in current state");
        }

        task.Status = AutoCodeForge.Core.Entities.TaskStatus.Canceled;
        task.ErrorMessage = "Task canceled by user";
        task.CompletedAtUtc = DateTime.UtcNow;
        await _taskRepository.UpdateAsync(task, cancellationToken);

        await _taskLogRepository.CreateAsync(new TaskLogEntity
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            Level = "Info",
            Message = "Repo sync task canceled by user",
            Source = nameof(RepoSyncService),
        }, cancellationToken);

        var workspace = await _workspaceRepository.GetByTaskIdAsync(taskId, false, cancellationToken);
        if (workspace is not null)
        {
            workspace.Status = RepoSandboxWorkspaceStatus.Cleaned;
            workspace.ErrorMessage = "Task canceled by user";
            workspace.FinishedAtUtc = DateTime.UtcNow;
            await _workspaceRepository.UpdateAsync(workspace, cancellationToken);
        }

        return await GetTaskDetailAsync(taskId, cancellationToken);
    }

    private static AutoCodeForge.Core.DTOs.Task.TaskResponse ToTaskResponse(TaskEntity entity)
    {
        return new AutoCodeForge.Core.DTOs.Task.TaskResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Status = entity.Status.ToString(),
            TaskType = entity.TaskType.ToString(),
            Progress = entity.Progress,
            Input = entity.Input,
            Result = entity.Result,
            ErrorMessage = entity.ErrorMessage,
            AgentId = entity.AgentId,
            DueAtUtc = entity.DueAtUtc,
            StartedAtUtc = entity.StartedAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
        };
    }

    private static (string owner, string repoName) ExtractOwnerRepo(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return (string.Empty, string.Empty);
        }

        var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            return (string.Empty, string.Empty);
        }

        var owner = segments[0];
        var repoName = segments[1].Replace(".git", string.Empty, StringComparison.OrdinalIgnoreCase);
        return (owner, repoName);
    }
}
