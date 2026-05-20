using AutoCodeForge.Core.Entities;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Infrastructure.BackgroundServices;

/// <summary>
/// Polls pending tasks and dispatches background execution.
/// </summary>
public class TaskQueueService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan IdleDelay = TimeSpan.FromMilliseconds(800);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TaskQueueService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskQueueService"/> class.
    /// </summary>
    /// <param name="scopeFactory">The scope factory.</param>
    /// <param name="logger">The logger.</param>
    public TaskQueueService(IServiceScopeFactory scopeFactory, ILogger<TaskQueueService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Executes queue polling loop.
    /// </summary>
    /// <param name="stoppingToken">The stopping token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Task queue service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var hadWork = await PollAndExecuteAsync(stoppingToken);
                await Task.Delay(hadWork ? IdleDelay : PollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Task queue polling failed");
                await Task.Delay(PollInterval, stoppingToken);
            }
        }

        _logger.LogInformation("Task queue service stopped");
    }

    private async Task<bool> PollAndExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var taskRepository = scope.ServiceProvider.GetRequiredService<TaskRepository>();
        var taskLogRepository = scope.ServiceProvider.GetRequiredService<TaskLogRepository>();
        var taskExecutor = scope.ServiceProvider.GetRequiredService<TaskExecutor>();

        var pending = await taskRepository.GetPendingAsync(10, cancellationToken);
        if (pending.Count == 0)
        {
            return false;
        }

        foreach (var task in pending)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var startedAt = DateTime.UtcNow;
            var started = await taskRepository.TryMarkRunningAsync(task.Id, startedAt, cancellationToken);
            if (!started)
            {
                continue;
            }

            await taskLogRepository.CreateAsync(new TaskLogEntity
            {
                Id = Guid.NewGuid(),
                TaskId = task.Id,
                Level = "Info",
                Message = "Task started",
                Source = nameof(TaskQueueService),
            }, cancellationToken);

            await taskExecutor.ExecuteAsync(task, cancellationToken);
        }

        return true;
    }
}