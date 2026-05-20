using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.DTOs.RepoSync;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Infrastructure.Logging;
using AutoCodeForge.Infrastructure.Repositories;
using AutoCodeForge.Infrastructure.Sandbox;
using AutoCodeForge.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Infrastructure.BackgroundServices.Handlers;

/// <summary>
/// Handles repo sync task execution.
/// </summary>
public class RepoSyncTaskHandler
{
    private readonly RepoSandboxWorkspaceRepository _workspaceRepository;
    private readonly TaskRepository _taskRepository;
    private readonly TaskLogRepository _taskLogRepository;
    private readonly SandboxPathResolver _pathResolver;
    private readonly GitCloneService _gitCloneService;
    private readonly DataProtectionService _dataProtectionService;
    private readonly ILogger<RepoSyncTaskHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepoSyncTaskHandler"/> class.
    /// </summary>
    /// <param name="workspaceRepository">Workspace repository.</param>
    /// <param name="taskRepository">Task repository.</param>
    /// <param name="taskLogRepository">Task log repository.</param>
    /// <param name="pathResolver">Sandbox path resolver.</param>
    /// <param name="gitCloneService">Git clone service.</param>
    /// <param name="dataProtectionService">Data protection service.</param>
    /// <param name="logger">Logger instance.</param>
    public RepoSyncTaskHandler(
        RepoSandboxWorkspaceRepository workspaceRepository,
        TaskRepository taskRepository,
        TaskLogRepository taskLogRepository,
        SandboxPathResolver pathResolver,
        GitCloneService gitCloneService,
        DataProtectionService dataProtectionService,
        ILogger<RepoSyncTaskHandler> logger)
    {
        _workspaceRepository = workspaceRepository;
        _taskRepository = taskRepository;
        _taskLogRepository = taskLogRepository;
        _pathResolver = pathResolver;
        _gitCloneService = gitCloneService;
        _dataProtectionService = dataProtectionService;
        _logger = logger;
    }

    /// <summary>
    /// Executes one repo sync task.
    /// </summary>
    /// <param name="task">Task entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task ExecuteAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(task.SandboxSnapshotJson) || string.IsNullOrWhiteSpace(task.RepositorySnapshotJson))
        {
            throw new ValidationException("Repo sync task snapshot is missing");
        }

        var sandboxSnapshot = JsonHelper.Deserialize<SandboxSnapshot>(task.SandboxSnapshotJson)
            ?? throw new ValidationException("Invalid sandbox snapshot payload");
        var repositorySnapshot = JsonHelper.Deserialize<RepositorySnapshot>(task.RepositorySnapshotJson)
            ?? throw new ValidationException("Invalid repository snapshot payload");

        var sandboxConfig = new SandboxConfigDto
        {
            WorkspaceRootPath = sandboxSnapshot.WorkspaceRootPath,
            AllowedWritePaths = sandboxSnapshot.AllowedWritePaths,
            TimeoutSeconds = sandboxSnapshot.TimeoutSeconds,
            UserIsolationEnabled = sandboxSnapshot.UserIsolationEnabled,
        };

        var resolved = _pathResolver.Resolve(
            sandboxConfig,
            task.NtId,
            task.Id,
            repositorySnapshot.Provider.ToString(),
            repositorySnapshot.Owner,
            repositorySnapshot.RepoName);

        var workspace = await UpsertWorkspaceAsync(task, repositorySnapshot, resolved, cancellationToken);
        try
        {
            await EnsureNotCanceledByUserAsync(task.Id, cancellationToken);

            workspace.Status = RepoSandboxWorkspaceStatus.Ready;
            workspace.StartedAtUtc = DateTime.UtcNow;
            workspace.ErrorMessage = null;
            await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

            await AddLogAsync(task.Id, "Info", $"Repo sync started: {resolved.RepositoryPath}", cancellationToken);

            var decryptedToken = _dataProtectionService.Decrypt(repositorySnapshot.EncryptedToken);
            var timeoutSeconds = NormalizeTimeoutSeconds(sandboxSnapshot.TimeoutSeconds);
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var sha = await _gitCloneService.CloneOrPullAsync(
                repositorySnapshot.Url,
                decryptedToken,
                repositorySnapshot.Branch,
                resolved.RepositoryPath,
                linkedCts.Token);

            if (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                throw new TimeoutException($"Repo sync timed out after {timeoutSeconds} seconds");
            }

            await EnsureNotCanceledByUserAsync(task.Id, cancellationToken);

            workspace.Status = RepoSandboxWorkspaceStatus.Pulled;
            workspace.CommitSha = sha;
            workspace.Branch = repositorySnapshot.Branch;
            workspace.FinishedAtUtc = DateTime.UtcNow;
            workspace.ErrorMessage = null;
            await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

            task.WorkspaceRecordId = workspace.Id;
            task.Status = AutoCodeForge.Core.Entities.TaskStatus.Completed;
            task.Progress = 100;
            task.Result = JsonHelper.Serialize(new
            {
                workspacePath = resolved.RepositoryPath,
                commitSha = sha,
                branch = repositorySnapshot.Branch,
            });
            task.ErrorMessage = null;
            task.CompletedAtUtc = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task, cancellationToken);
            await AddLogAsync(task.Id, "Info", "Repo sync completed", cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            const string canceledMessage = "Task canceled by user";
            workspace.Status = RepoSandboxWorkspaceStatus.Cleaned;
            workspace.ErrorMessage = canceledMessage;
            workspace.FinishedAtUtc = DateTime.UtcNow;
            await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

            task.Status = AutoCodeForge.Core.Entities.TaskStatus.Canceled;
            task.ErrorMessage = canceledMessage;
            task.CompletedAtUtc = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task, cancellationToken);
            await AddLogAsync(task.Id, "Info", canceledMessage, cancellationToken);
        }
        catch (TimeoutException ex)
        {
            var timeoutMessage = LogSanitizer.Sanitize(ex.Message);
            workspace.Status = RepoSandboxWorkspaceStatus.Failed;
            workspace.ErrorMessage = timeoutMessage;
            workspace.FinishedAtUtc = DateTime.UtcNow;
            await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

            task.Status = AutoCodeForge.Core.Entities.TaskStatus.Failed;
            task.ErrorMessage = timeoutMessage;
            task.CompletedAtUtc = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task, cancellationToken);
            await AddLogAsync(task.Id, "Error", timeoutMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            var sanitized = LogSanitizer.Sanitize(ex.Message);
            _logger.LogError(ex, "Repo sync failed for task {TaskId}: {Error}", task.Id, sanitized);

            workspace.Status = RepoSandboxWorkspaceStatus.Failed;
            workspace.ErrorMessage = sanitized;
            workspace.FinishedAtUtc = DateTime.UtcNow;
            await _workspaceRepository.UpdateAsync(workspace, cancellationToken);

            task.Status = AutoCodeForge.Core.Entities.TaskStatus.Failed;
            task.ErrorMessage = sanitized;
            task.CompletedAtUtc = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task, cancellationToken);
            await AddLogAsync(task.Id, "Error", sanitized, cancellationToken);
        }
    }

    private static int NormalizeTimeoutSeconds(int value)
    {
        if (value < 30)
        {
            return 30;
        }

        return value > 7200 ? 7200 : value;
    }

    private async Task EnsureNotCanceledByUserAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var latest = await _taskRepository.GetByIdAsync(taskId, true, cancellationToken)
            ?? throw new ValidationException("Task not found during repo sync execution");

        if (latest.Status == AutoCodeForge.Core.Entities.TaskStatus.Canceled)
        {
            throw new OperationCanceledException("Task canceled by user");
        }
    }

    private async Task<RepoSandboxWorkspaceEntity> UpsertWorkspaceAsync(
        TaskEntity task,
        RepositorySnapshot repositorySnapshot,
        SandboxPathResolutionResult resolved,
        CancellationToken cancellationToken)
    {
        var existing = await _workspaceRepository.GetByTaskIdAsync(task.Id, true, cancellationToken);
        if (existing is not null)
        {
            existing.WorkspaceRootPath = resolved.WorkspaceRootPath;
            existing.EffectiveSandboxPath = resolved.EffectiveSandboxPath;
            existing.RelativeRepoPath = resolved.RelativeRepoPath;
            existing.RepositoryId = repositorySnapshot.RepositoryId;
            existing.Branch = repositorySnapshot.Branch;
            await _workspaceRepository.UpdateAsync(existing, cancellationToken);
            return existing;
        }

        var workspace = new RepoSandboxWorkspaceEntity
        {
            Id = Guid.NewGuid(),
            NtId = task.NtId,
            TaskId = task.Id,
            RepositoryId = repositorySnapshot.RepositoryId,
            WorkspaceRootPath = resolved.WorkspaceRootPath,
            EffectiveSandboxPath = resolved.EffectiveSandboxPath,
            RelativeRepoPath = resolved.RelativeRepoPath,
            Branch = repositorySnapshot.Branch,
            Status = RepoSandboxWorkspaceStatus.Ready,
        };

        await _workspaceRepository.CreateAsync(workspace, cancellationToken);
        task.WorkspaceRecordId = workspace.Id;
        await _taskRepository.UpdateAsync(task, cancellationToken);
        return workspace;
    }

    private async Task AddLogAsync(Guid taskId, string level, string message, CancellationToken cancellationToken)
    {
        var log = new TaskLogEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Level = level,
            Message = message,
            Source = nameof(RepoSyncTaskHandler),
        };

        await _taskLogRepository.CreateAsync(log, cancellationToken);
    }
}
