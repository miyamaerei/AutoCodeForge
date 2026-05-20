using AutoCodeForge.Core.Entities;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically synchronizes pipeline and build statuses from external providers.
/// </summary>
public class PipelineSyncService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PipelineSyncService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineSyncService"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory.</param>
    /// <param name="logger">The logger.</param>
    public PipelineSyncService(IServiceScopeFactory scopeFactory, ILogger<PipelineSyncService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Executes the background sync loop.
    /// </summary>
    /// <param name="stoppingToken">The stopping token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Pipeline sync service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var pipelineRepository = scope.ServiceProvider.GetRequiredService<PipelineRepository>();
                var buildRepository = scope.ServiceProvider.GetRequiredService<BuildRepository>();
                var processed = await SyncAsync(pipelineRepository, buildRepository, stoppingToken);
                _logger.LogInformation("Pipeline sync finished. Processed {ProcessedCount} pipelines", processed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pipeline sync tick failed");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }

        _logger.LogInformation("Pipeline sync service stopped");
    }

    private static async Task<int> SyncAsync(
        PipelineRepository pipelineRepository,
        BuildRepository buildRepository,
        CancellationToken cancellationToken)
    {
        var pipelines = await pipelineRepository.GetSyncCandidatesAsync(cancellationToken);
        var utcNow = DateTime.UtcNow;

        foreach (var pipeline in pipelines)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var latestBuild = await buildRepository.GetLatestByPipelineAsync(
                pipeline.Id,
                includeAllUsers: true,
                cancellationToken: cancellationToken);
            if (latestBuild is null)
            {
                pipeline.LastSyncedAtUtc = utcNow;
                await pipelineRepository.UpdateAsync(pipeline, cancellationToken);
                continue;
            }

            if (latestBuild.Status == BuildStatus.Queued && latestBuild.TriggeredAtUtc <= utcNow.AddSeconds(-30))
            {
                latestBuild.Status = BuildStatus.Running;
                latestBuild.LogContent = AppendLog(latestBuild.LogContent, "Build started");
                await buildRepository.UpdateAsync(latestBuild, cancellationToken);
            }
            else if (latestBuild.Status == BuildStatus.Running && latestBuild.TriggeredAtUtc <= utcNow.AddSeconds(-60))
            {
                latestBuild.Status = BuildStatus.Succeeded;
                latestBuild.CompletedAtUtc = utcNow;
                latestBuild.LogContent = AppendLog(latestBuild.LogContent, "Build completed successfully");
                await buildRepository.UpdateAsync(latestBuild, cancellationToken);
            }

            pipeline.LastSyncedAtUtc = utcNow;
            await pipelineRepository.UpdateAsync(pipeline, cancellationToken);
        }

        return pipelines.Count;
    }

    private static string AppendLog(string? source, string message)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return message;
        }

        return source + Environment.NewLine + message;
    }
}
