using AutoCodeForge.Core.Entities;
using AutoCodeForge.Infrastructure.Repositories;
using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Infrastructure.BackgroundServices;

/// <summary>
/// Polls active scheduled tasks and spawns TaskEntity entries when a run is due.
/// Reuses the existing <see cref="TaskQueueService"/> execution pipeline.
/// </summary>
public class CronSchedulerService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CronSchedulerService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CronSchedulerService"/> class.
    /// </summary>
    /// <param name="scopeFactory">The scope factory.</param>
    /// <param name="logger">The logger.</param>
    public CronSchedulerService(IServiceScopeFactory scopeFactory, ILogger<CronSchedulerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Executes the Cron polling loop.
    /// </summary>
    /// <param name="stoppingToken">The stopping token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cron scheduler service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await TickAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cron scheduler tick failed");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }

        _logger.LogInformation("Cron scheduler service stopped");
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var scheduledTaskRepo = scope.ServiceProvider.GetRequiredService<ScheduledTaskRepository>();
        var executionRepo = scope.ServiceProvider.GetRequiredService<ScheduledTaskExecutionRepository>();
        var taskRepo = scope.ServiceProvider.GetRequiredService<TaskRepository>();

        var utcNow = DateTime.UtcNow;
        var dueTasks = await scheduledTaskRepo.GetDueTasksAsync(utcNow, cancellationToken);

        foreach (var scheduled in dueTasks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await SpawnTaskAsync(scheduled, utcNow, taskRepo, executionRepo, cancellationToken);

                var nextRun = CalculateNextRun(scheduled.CronExpression, utcNow);
                await scheduledTaskRepo.UpdateNextRunAsync(scheduled.Id, nextRun, cancellationToken);

                _logger.LogInformation(
                    "Scheduled task {Id} fired. Next run: {NextRun}",
                    scheduled.Id,
                    nextRun?.ToString("O") ?? "none");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fire scheduled task {Id}", scheduled.Id);
            }
        }
    }

    private static async Task SpawnTaskAsync(
        ScheduledTaskEntity scheduled,
        DateTime utcNow,
        TaskRepository taskRepo,
        ScheduledTaskExecutionRepository executionRepo,
        CancellationToken cancellationToken)
    {
        var taskEntity = new TaskEntity
        {
            Id = Guid.NewGuid(),
            NtId = scheduled.NtId,
            Title = scheduled.TaskTitle,
            Input = scheduled.Input,
            AgentId = scheduled.AgentId,
            Status = AutoCodeForge.Core.Entities.TaskStatus.Pending,
            Progress = 0,
        };
        await taskRepo.CreateAsync(taskEntity, cancellationToken);

        var execution = new ScheduledTaskExecutionEntity
        {
            Id = Guid.NewGuid(),
            NtId = scheduled.NtId,
            ScheduledTaskId = scheduled.Id,
            Status = ExecutionStatus.Running,
            StartedAtUtc = utcNow,
            Output = $"Spawned task {taskEntity.Id}",
        };
        await executionRepo.CreateAsync(execution, cancellationToken);
    }

    private static DateTime? CalculateNextRun(string cronExpression, DateTime fromUtc)
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
}
