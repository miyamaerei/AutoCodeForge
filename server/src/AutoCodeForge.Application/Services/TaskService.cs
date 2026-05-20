using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides task CRUD, status transitions, and task log operations.
/// </summary>
public class TaskService
{
    private readonly TaskRepository _taskRepository;
    private readonly TaskLogRepository _taskLogRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskService"/> class.
    /// </summary>
    /// <param name="taskRepository">The task repository.</param>
    /// <param name="taskLogRepository">The task log repository.</param>
    public TaskService(TaskRepository taskRepository, TaskLogRepository taskLogRepository)
    {
        _taskRepository = taskRepository;
        _taskLogRepository = taskLogRepository;
    }

    /// <summary>
    /// Creates one task and appends an initialization log.
    /// </summary>
    /// <param name="request">The create request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created task response.</returns>
    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Input = request.Input.Trim(),
            AgentId = request.AgentId,
            DueAtUtc = request.DueAtUtc,
            Status = AutoCodeForge.Core.Entities.TaskStatus.Pending,
            Progress = 0,
        };

        var created = await _taskRepository.CreateAsync(entity, cancellationToken);
        await AddLogAsync(created.Id, "Info", "Task created", nameof(TaskService), cancellationToken);
        return ToResponse(created);
    }

    /// <summary>
    /// Updates mutable task fields.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated task response.</returns>
    public async Task<TaskResponse> UpdateAsync(
        Guid taskId,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(taskId, cancellationToken);
        if (entity.Status == AutoCodeForge.Core.Entities.TaskStatus.Running)
        {
            throw new ValidationException("Running task cannot be edited");
        }

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            entity.Title = request.Title.Trim();
        }

        if (request.Description is not null)
        {
            entity.Description = request.Description.Trim();
        }

        if (request.Input is not null)
        {
            var input = request.Input.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ValidationException("Input cannot be empty");
            }

            entity.Input = input;
        }

        entity.AgentId = request.AgentId;
        entity.DueAtUtc = request.DueAtUtc;

        await _taskRepository.UpdateAsync(entity, cancellationToken);
        await AddLogAsync(taskId, "Info", "Task updated", nameof(TaskService), cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// Gets one task by identifier.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task response.</returns>
    public async Task<TaskResponse> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(taskId, cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// Gets paged tasks.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged task response.</returns>
    public async Task<PagedResult<TaskResponse>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var paged = await _taskRepository.GetPagedAsync(page, pageSize, false, cancellationToken);
        return new PagedResult<TaskResponse>(
            paged.Items.Select(ToResponse).ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize);
    }

    /// <summary>
    /// Gets all runtime logs for one task.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Task log responses.</returns>
    public async Task<List<TaskLogResponse>> GetLogsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        _ = await GetEntityOrThrowAsync(taskId, cancellationToken);
        var logs = await _taskLogRepository.GetByTaskIdAsync(taskId, cancellationToken);
        return logs.Select(ToResponse).ToList();
    }

    /// <summary>
    /// Pauses one task if it is not completed.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated task response.</returns>
    public async Task<TaskResponse> PauseAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(taskId, cancellationToken);
        if (entity.Status is AutoCodeForge.Core.Entities.TaskStatus.Completed or AutoCodeForge.Core.Entities.TaskStatus.Failed)
        {
            throw new ValidationException("Completed or failed task cannot be paused");
        }

        entity.Status = AutoCodeForge.Core.Entities.TaskStatus.Paused;
        await _taskRepository.UpdateAsync(entity, cancellationToken);
        await AddLogAsync(taskId, "Info", "Task paused", nameof(TaskService), cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// Resumes one paused task back to pending.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated task response.</returns>
    public async Task<TaskResponse> ResumeAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(taskId, cancellationToken);
        if (entity.Status != AutoCodeForge.Core.Entities.TaskStatus.Paused)
        {
            throw new ValidationException("Only paused task can be resumed");
        }

        entity.Status = AutoCodeForge.Core.Entities.TaskStatus.Pending;
        entity.Progress = 0;
        entity.ErrorMessage = null;
        entity.CompletedAtUtc = null;
        await _taskRepository.UpdateAsync(entity, cancellationToken);
        await AddLogAsync(taskId, "Info", "Task resumed", nameof(TaskService), cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// Soft deletes one task.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task DeleteAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        await _taskRepository.SoftDeleteAsync(taskId, false, cancellationToken);
        await AddLogAsync(taskId, "Info", "Task deleted", nameof(TaskService), cancellationToken);
    }

    /// <summary>
    /// Attempts to transition task from pending to running.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when transition succeeds; otherwise <see langword="false"/>.</returns>
    public async Task<bool> TryStartAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var startedAt = TimeHelper.UtcNow();
        var started = await _taskRepository.TryMarkRunningAsync(taskId, startedAt, cancellationToken);
        if (started)
        {
            await AddLogAsync(taskId, "Info", "Task started", nameof(TaskService), cancellationToken);
        }

        return started;
    }

    /// <summary>
    /// Marks task completed with result payload.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="resultPayload">The result payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task MarkCompletedAsync(
        Guid taskId,
        string resultPayload,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(taskId, cancellationToken);
        entity.Status = AutoCodeForge.Core.Entities.TaskStatus.Completed;
        entity.Progress = 100;
        entity.Result = resultPayload;
        entity.ErrorMessage = null;
        entity.CompletedAtUtc = TimeHelper.UtcNow();

        await _taskRepository.UpdateAsync(entity, cancellationToken);
        await AddLogAsync(taskId, "Info", "Task completed", nameof(TaskService), cancellationToken);
    }

    /// <summary>
    /// Marks task failed with error message.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task MarkFailedAsync(
        Guid taskId,
        string errorMessage,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(taskId, cancellationToken);
        entity.Status = AutoCodeForge.Core.Entities.TaskStatus.Failed;
        entity.ErrorMessage = errorMessage;
        entity.CompletedAtUtc = TimeHelper.UtcNow();

        await _taskRepository.UpdateAsync(entity, cancellationToken);
        await AddLogAsync(taskId, "Error", errorMessage, nameof(TaskService), cancellationToken);
    }

    /// <summary>
    /// Gets pending tasks for queue polling.
    /// </summary>
    /// <param name="take">Maximum number of rows.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Pending task entities.</returns>
    public Task<List<TaskEntity>> GetPendingTasksAsync(int take, CancellationToken cancellationToken = default)
    {
        return _taskRepository.GetPendingAsync(take, cancellationToken);
    }

    /// <summary>
    /// Adds one task log entry.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <param name="level">The log level.</param>
    /// <param name="message">The log message.</param>
    /// <param name="source">The log source.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task AddLogAsync(
        Guid taskId,
        string level,
        string message,
        string source,
        CancellationToken cancellationToken = default)
    {
        var log = new TaskLogEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Level = string.IsNullOrWhiteSpace(level) ? "Info" : level,
            Message = message,
            Source = source,
        };

        await _taskLogRepository.CreateAsync(log, cancellationToken);
    }

    private async Task<TaskEntity> GetEntityOrThrowAsync(Guid taskId, CancellationToken cancellationToken)
    {
        return await _taskRepository.GetByIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException("Task not found");
    }

    private static TaskResponse ToResponse(TaskEntity entity)
    {
        return new TaskResponse
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

    private static TaskLogResponse ToResponse(TaskLogEntity entity)
    {
        return new TaskLogResponse
        {
            Id = entity.Id,
            TaskId = entity.TaskId,
            Level = entity.Level,
            Message = entity.Message,
            Source = entity.Source,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }
}
