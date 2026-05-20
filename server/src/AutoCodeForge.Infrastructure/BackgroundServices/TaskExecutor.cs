using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Infrastructure.BackgroundServices.Handlers;
using AutoCodeForge.Infrastructure.AI;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Infrastructure.BackgroundServices;

/// <summary>
/// Executes one task by reusing AgentService and AgentExecutor.
/// </summary>
public class TaskExecutor
{
    private readonly TaskRepository _taskRepository;
    private readonly TaskLogRepository _taskLogRepository;
    private readonly AgentRepository _agentRepository;
    private readonly AgentExecutor _agentExecutor;
    private readonly RepoSyncTaskHandler _repoSyncTaskHandler;
    private readonly ReviewTaskHandler _reviewTaskHandler;
    private readonly ILogger<TaskExecutor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskExecutor"/> class.
    /// </summary>
    /// <param name="taskRepository">The task repository.</param>
    /// <param name="taskLogRepository">The task log repository.</param>
    /// <param name="agentRepository">The agent repository.</param>
    /// <param name="agentExecutor">The shared agent executor.</param>
    /// <param name="logger">The logger.</param>
    public TaskExecutor(
        TaskRepository taskRepository,
        TaskLogRepository taskLogRepository,
        AgentRepository agentRepository,
        AgentExecutor agentExecutor,
        RepoSyncTaskHandler repoSyncTaskHandler,
        ReviewTaskHandler reviewTaskHandler,
        ILogger<TaskExecutor> logger)
    {
        _taskRepository = taskRepository;
        _taskLogRepository = taskLogRepository;
        _agentRepository = agentRepository;
        _agentExecutor = agentExecutor;
        _repoSyncTaskHandler = repoSyncTaskHandler;
        _reviewTaskHandler = reviewTaskHandler;
        _logger = logger;
    }

    /// <summary>
    /// Executes one task in background.
    /// </summary>
    /// <param name="task">The task entity to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task ExecuteAsync(TaskEntity task, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _taskRepository.GetByIdAsync(task.Id, true, cancellationToken)
                ?? throw new NotFoundException("Task not found");

            if (entity.TaskType == TaskType.RepoSyncToSandbox)
            {
                await _repoSyncTaskHandler.ExecuteAsync(entity, cancellationToken);
                return;
            }

            if (entity.TaskType == TaskType.Review)
            {
                await _reviewTaskHandler.ExecuteAsync(entity, cancellationToken);
                return;
            }

            if (!entity.AgentId.HasValue)
            {
                throw new ValidationException("Task agent is required");
            }

            var agent = await _agentRepository.GetByIdAsync(entity.AgentId.Value, true, cancellationToken)
                ?? throw new NotFoundException("Task agent not found");

            if (!agent.IsEnabled)
            {
                throw new ValidationException("Task agent is disabled");
            }

            await AddLogAsync(entity.Id, "Info", "Task execution started", nameof(TaskExecutor), cancellationToken);
            var output = await _agentExecutor.ExecuteAsync(agent, entity.Input, [], cancellationToken);

            var result = JsonHelper.Serialize(new
            {
                output,
            });

            entity.Status = AutoCodeForge.Core.Entities.TaskStatus.Completed;
            entity.Progress = 100;
            entity.Result = result;
            entity.ErrorMessage = null;
            entity.CompletedAtUtc = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(entity, cancellationToken);
            await AddLogAsync(entity.Id, "Info", "Task execution finished", nameof(TaskExecutor), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Task execution failed for {TaskId}", task.Id);
            var failedEntity = await _taskRepository.GetByIdAsync(task.Id, true, cancellationToken);
            if (failedEntity is null)
            {
                return;
            }

            failedEntity.Status = AutoCodeForge.Core.Entities.TaskStatus.Failed;
            failedEntity.ErrorMessage = ex.Message;
            failedEntity.CompletedAtUtc = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(failedEntity, cancellationToken);
            await AddLogAsync(task.Id, "Error", ex.Message, nameof(TaskExecutor), cancellationToken);
        }
    }

    private async Task AddLogAsync(
        Guid taskId,
        string level,
        string message,
        string source,
        CancellationToken cancellationToken)
    {
        var log = new TaskLogEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Level = level,
            Message = message,
            Source = source,
        };

        await _taskLogRepository.CreateAsync(log, cancellationToken);
    }
}