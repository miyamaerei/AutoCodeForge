using AutoCodeForge.Core.DTOs.ScheduledTask;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;
using Cronos;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides CRUD operations and next-run calculation for scheduled tasks.
/// </summary>
public class ScheduledTaskService
{
    private readonly ScheduledTaskRepository _scheduledTaskRepository;
    private readonly ScheduledTaskExecutionRepository _executionRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledTaskService"/> class.
    /// </summary>
    /// <param name="scheduledTaskRepository">The scheduled task repository.</param>
    /// <param name="executionRepository">The execution history repository.</param>
    public ScheduledTaskService(
        ScheduledTaskRepository scheduledTaskRepository,
        ScheduledTaskExecutionRepository executionRepository)
    {
        _scheduledTaskRepository = scheduledTaskRepository;
        _executionRepository = executionRepository;
    }

    /// <summary>
    /// Creates one scheduled task and pre-calculates first run time.
    /// </summary>
    /// <param name="request">The creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created scheduled task response.</returns>
    public async Task<ScheduledTaskResponse> CreateAsync(
        CreateScheduledTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateCronExpression(request.CronExpression);

        var entity = new ScheduledTaskEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            CronExpression = request.CronExpression.Trim(),
            TriggerType = request.TriggerType,
            AgentId = request.AgentId,
            Input = request.Input.Trim(),
            TaskTitle = request.TaskTitle.Trim(),
            Status = ScheduleStatus.Active,
            NextRunAtUtc = CalculateNextRun(request.CronExpression, DateTime.UtcNow),
        };

        var created = await _scheduledTaskRepository.CreateAsync(entity, cancellationToken);
        return ToResponse(created);
    }

    /// <summary>
    /// Updates mutable fields of an existing scheduled task.
    /// </summary>
    /// <param name="taskId">The scheduled task identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated scheduled task response.</returns>
    public async Task<ScheduledTaskResponse> UpdateAsync(
        Guid taskId,
        UpdateScheduledTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(taskId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            entity.Name = request.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.CronExpression))
        {
            ValidateCronExpression(request.CronExpression);
            entity.CronExpression = request.CronExpression.Trim();
            entity.NextRunAtUtc = CalculateNextRun(entity.CronExpression, DateTime.UtcNow);
        }

        if (!string.IsNullOrWhiteSpace(request.Input))
        {
            entity.Input = request.Input.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.TaskTitle))
        {
            entity.TaskTitle = request.TaskTitle.Trim();
        }

        entity.AgentId = request.AgentId ?? entity.AgentId;

        if (request.Status.HasValue)
        {
            entity.Status = request.Status.Value;
        }

        await _scheduledTaskRepository.UpdateAsync(entity, cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// Gets one scheduled task by identifier.
    /// </summary>
    /// <param name="taskId">The scheduled task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The scheduled task response.</returns>
    public async Task<ScheduledTaskResponse> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(taskId, cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// Gets paged scheduled tasks.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged scheduled task responses.</returns>
    public async Task<PagedResult<ScheduledTaskResponse>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var paged = await _scheduledTaskRepository.GetPagedAsync(page, pageSize, false, cancellationToken);
        return new PagedResult<ScheduledTaskResponse>(
            paged.Items.Select(ToResponse).ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize);
    }

    /// <summary>
    /// Gets paged execution history for one scheduled task.
    /// </summary>
    /// <param name="taskId">The scheduled task identifier.</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged execution history responses.</returns>
    public async Task<PagedResult<ScheduledTaskExecutionResponse>> GetExecutionsAsync(
        Guid taskId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _ = await GetEntityOrThrowAsync(taskId, cancellationToken);
        var (items, total) = await _executionRepository.GetPagedByScheduledTaskAsync(taskId, page, pageSize, cancellationToken);
        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
        return new PagedResult<ScheduledTaskExecutionResponse>(
            items.Select(ToExecutionResponse).ToList(),
            total,
            normalizedPage,
            normalizedSize);
    }

    /// <summary>
    /// Pauses an active scheduled task.
    /// </summary>
    /// <param name="taskId">The scheduled task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated scheduled task response.</returns>
    public async Task<ScheduledTaskResponse> PauseAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(taskId, cancellationToken);
        if (entity.Status != ScheduleStatus.Active)
        {
            throw new ValidationException("Only active scheduled tasks can be paused");
        }

        entity.Status = ScheduleStatus.Paused;
        await _scheduledTaskRepository.UpdateAsync(entity, cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// Resumes a paused scheduled task and recalculates next run time.
    /// </summary>
    /// <param name="taskId">The scheduled task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated scheduled task response.</returns>
    public async Task<ScheduledTaskResponse> ResumeAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(taskId, cancellationToken);
        if (entity.Status != ScheduleStatus.Paused)
        {
            throw new ValidationException("Only paused scheduled tasks can be resumed");
        }

        entity.Status = ScheduleStatus.Active;
        entity.NextRunAtUtc = CalculateNextRun(entity.CronExpression, DateTime.UtcNow);
        await _scheduledTaskRepository.UpdateAsync(entity, cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// Soft-deletes a scheduled task.
    /// </summary>
    /// <param name="taskId">The scheduled task identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task DeleteAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        await _scheduledTaskRepository.SoftDeleteAsync(taskId, false, cancellationToken);
    }

    /// <summary>
    /// Calculates next occurrence from a Cron expression after the specified UTC time.
    /// </summary>
    /// <param name="cronExpression">The Cron expression (6-part with seconds).</param>
    /// <param name="fromUtc">The reference time in UTC.</param>
    /// <returns>Next UTC occurrence, or <see langword="null"/> if none exists.</returns>
    public static DateTime? CalculateNextRun(string cronExpression, DateTime fromUtc)
    {
        try
        {
            var expression = CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
            return expression.GetNextOccurrence(fromUtc, TimeZoneInfo.Utc);
        }
        catch
        {
            return null;
        }
    }

    private static void ValidateCronExpression(string cronExpression)
    {
        try
        {
            CronExpression.Parse(cronExpression, CronFormat.IncludeSeconds);
        }
        catch (CronFormatException ex)
        {
            throw new ValidationException($"Invalid Cron expression: {ex.Message}");
        }
    }

    private async Task<ScheduledTaskEntity> GetEntityOrThrowAsync(
        Guid taskId,
        CancellationToken cancellationToken)
    {
        return await _scheduledTaskRepository.GetByIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException($"Scheduled task {taskId} not found");
    }

    private static ScheduledTaskResponse ToResponse(ScheduledTaskEntity entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            TriggerType = entity.TriggerType.ToString(),
            CronExpression = entity.CronExpression,
            Status = entity.Status.ToString(),
            AgentId = entity.AgentId,
            Input = entity.Input,
            TaskTitle = entity.TaskTitle,
            NextRunAtUtc = entity.NextRunAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc,
        };

    private static ScheduledTaskExecutionResponse ToExecutionResponse(ScheduledTaskExecutionEntity entity) =>
        new()
        {
            Id = entity.Id,
            ScheduledTaskId = entity.ScheduledTaskId,
            Status = entity.Status.ToString(),
            StartedAtUtc = entity.StartedAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc,
            Output = entity.Output,
        };
}
