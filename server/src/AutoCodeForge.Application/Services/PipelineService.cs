using AutoCodeForge.Core.DTOs.Pipeline;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides CRUD, trigger, history, and status synchronization operations for pipelines.
/// </summary>
public class PipelineService
{
    private readonly PipelineRepository _pipelineRepository;
    private readonly BuildRepository _buildRepository;
    private readonly RepositoryRepository _repositoryRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineService"/> class.
    /// </summary>
    /// <param name="pipelineRepository">The pipeline repository.</param>
    /// <param name="buildRepository">The build repository.</param>
    /// <param name="repositoryRepository">The repository repository.</param>
    public PipelineService(
        PipelineRepository pipelineRepository,
        BuildRepository buildRepository,
        RepositoryRepository repositoryRepository)
    {
        _pipelineRepository = pipelineRepository;
        _buildRepository = buildRepository;
        _repositoryRepository = repositoryRepository;
    }

    /// <summary>
    /// Creates one pipeline.
    /// </summary>
    /// <param name="request">The creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created pipeline response.</returns>
    public async Task<PipelineResponse> CreateAsync(CreatePipelineRequest request, CancellationToken cancellationToken = default)
    {
        if (request.RepositoryId == Guid.Empty)
        {
            throw new ValidationException("RepositoryId is required");
        }

        _ = await _repositoryRepository.GetByIdAsync(request.RepositoryId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Repository {request.RepositoryId} not found");

        var existing = await _pipelineRepository.GetByRepositoryAndNameAsync(
            request.RepositoryId,
            request.Name.Trim(),
            cancellationToken);
        if (existing != null)
        {
            throw new ValidationException("Pipeline name already exists in this repository");
        }

        var entity = new PipelineEntity
        {
            Id = Guid.NewGuid(),
            RepositoryId = request.RepositoryId,
            Name = request.Name.Trim(),
            ExternalPipelineId = string.IsNullOrWhiteSpace(request.ExternalPipelineId) ? null : request.ExternalPipelineId.Trim(),
            DefinitionYaml = string.IsNullOrWhiteSpace(request.DefinitionYaml) ? null : request.DefinitionYaml.Trim(),
            Status = request.Status,
            LastSyncedAtUtc = null,
        };

        var created = await _pipelineRepository.CreateAsync(entity, cancellationToken);
        return ToPipelineResponse(created);
    }

    /// <summary>
    /// Gets one pipeline by identifier.
    /// </summary>
    /// <param name="pipelineId">The pipeline identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The pipeline response.</returns>
    public async Task<PipelineResponse> GetByIdAsync(Guid pipelineId, CancellationToken cancellationToken = default)
    {
        var entity = await GetPipelineOrThrowAsync(pipelineId, cancellationToken);
        return ToPipelineResponse(entity);
    }

    /// <summary>
    /// Gets pipelines with pagination.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged pipeline response payload.</returns>
    public async Task<PagedResult<PipelineResponse>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var paged = await _pipelineRepository.GetPagedAsync(page, pageSize, false, cancellationToken);
        return new PagedResult<PipelineResponse>(
            paged.Items.Select(ToPipelineResponse).ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize);
    }

    /// <summary>
    /// Updates mutable fields of one pipeline.
    /// </summary>
    /// <param name="pipelineId">The pipeline identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated pipeline response.</returns>
    public async Task<PipelineResponse> UpdateAsync(
        Guid pipelineId,
        UpdatePipelineRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetPipelineOrThrowAsync(pipelineId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            entity.Name = request.Name.Trim();
        }

        if (request.ExternalPipelineId != null)
        {
            entity.ExternalPipelineId = string.IsNullOrWhiteSpace(request.ExternalPipelineId)
                ? null
                : request.ExternalPipelineId.Trim();
        }

        if (request.DefinitionYaml != null)
        {
            entity.DefinitionYaml = string.IsNullOrWhiteSpace(request.DefinitionYaml)
                ? null
                : request.DefinitionYaml.Trim();
        }

        if (request.Status.HasValue)
        {
            entity.Status = request.Status.Value;
        }

        await _pipelineRepository.UpdateAsync(entity, cancellationToken);
        return ToPipelineResponse(entity);
    }

    /// <summary>
    /// Soft-deletes one pipeline.
    /// </summary>
    /// <param name="pipelineId">The pipeline identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task DeleteAsync(Guid pipelineId, CancellationToken cancellationToken = default)
    {
        await _pipelineRepository.SoftDeleteAsync(pipelineId, false, cancellationToken);
    }

    /// <summary>
    /// Triggers one pipeline build and records an initial build history entry.
    /// </summary>
    /// <param name="pipelineId">The pipeline identifier.</param>
    /// <param name="request">The trigger request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created build response.</returns>
    public async Task<BuildResponse> TriggerAsync(
        Guid pipelineId,
        TriggerPipelineRequest request,
        CancellationToken cancellationToken = default)
    {
        var pipeline = await GetPipelineOrThrowAsync(pipelineId, cancellationToken);
        if (pipeline.Status != PipelineStatus.Active)
        {
            throw new ValidationException("Only active pipelines can be triggered");
        }

        var buildNumber = string.IsNullOrWhiteSpace(request.BuildNumber)
            ? $"build-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : request.BuildNumber.Trim();

        var now = DateTime.UtcNow;
        var build = new BuildEntity
        {
            Id = Guid.NewGuid(),
            PipelineId = pipelineId,
            BuildNumber = buildNumber,
            ExternalBuildId = string.IsNullOrWhiteSpace(request.ExternalBuildId) ? null : request.ExternalBuildId.Trim(),
            Status = BuildStatus.Queued,
            TriggeredAtUtc = now,
            LogContent = "Build queued",
        };

        var created = await _buildRepository.CreateAsync(build, cancellationToken);
        pipeline.LastSyncedAtUtc = now;
        await _pipelineRepository.UpdateAsync(pipeline, cancellationToken);

        return ToBuildResponse(created);
    }

    /// <summary>
    /// Gets build history for one pipeline.
    /// </summary>
    /// <param name="pipelineId">The pipeline identifier.</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged build history response payload.</returns>
    public async Task<PagedResult<BuildResponse>> GetBuildsAsync(
        Guid pipelineId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _ = await GetPipelineOrThrowAsync(pipelineId, cancellationToken);
        var (items, total) = await _buildRepository.GetPagedByPipelineAsync(pipelineId, page, pageSize, cancellationToken);

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
        return new PagedResult<BuildResponse>(
            items.Select(ToBuildResponse).ToList(),
            total,
            normalizedPage,
            normalizedSize);
    }

    /// <summary>
    /// Performs one synchronization pass for active pipelines and updates build status progression.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of pipelines processed.</returns>
    public async Task<int> SyncStatusesAsync(CancellationToken cancellationToken = default)
    {
        var pipelines = await _pipelineRepository.GetSyncCandidatesAsync(cancellationToken);
        var utcNow = DateTime.UtcNow;

        foreach (var pipeline in pipelines)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var latestBuild = await _buildRepository.GetLatestByPipelineAsync(
                pipeline.Id,
                includeAllUsers: true,
                cancellationToken: cancellationToken);
            if (latestBuild is null)
            {
                pipeline.LastSyncedAtUtc = utcNow;
                await _pipelineRepository.UpdateAsync(pipeline, cancellationToken);
                continue;
            }

            if (latestBuild.Status == BuildStatus.Queued && latestBuild.TriggeredAtUtc <= utcNow.AddSeconds(-30))
            {
                latestBuild.Status = BuildStatus.Running;
                latestBuild.LogContent = AppendLog(latestBuild.LogContent, "Build started");
                await _buildRepository.UpdateAsync(latestBuild, cancellationToken);
            }
            else if (latestBuild.Status == BuildStatus.Running && latestBuild.TriggeredAtUtc <= utcNow.AddSeconds(-60))
            {
                latestBuild.Status = BuildStatus.Succeeded;
                latestBuild.CompletedAtUtc = utcNow;
                latestBuild.LogContent = AppendLog(latestBuild.LogContent, "Build completed successfully");
                await _buildRepository.UpdateAsync(latestBuild, cancellationToken);
            }

            pipeline.LastSyncedAtUtc = utcNow;
            await _pipelineRepository.UpdateAsync(pipeline, cancellationToken);
        }

        return pipelines.Count;
    }

    private async Task<PipelineEntity> GetPipelineOrThrowAsync(Guid pipelineId, CancellationToken cancellationToken)
    {
        return await _pipelineRepository.GetByIdAsync(pipelineId, false, cancellationToken)
            ?? throw new NotFoundException($"Pipeline {pipelineId} not found");
    }

    private static string AppendLog(string? source, string message)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return message;
        }

        return source + Environment.NewLine + message;
    }

    private static PipelineResponse ToPipelineResponse(PipelineEntity entity) =>
        new()
        {
            Id = entity.Id,
            RepositoryId = entity.RepositoryId,
            Name = entity.Name,
            ExternalPipelineId = entity.ExternalPipelineId,
            Status = entity.Status.ToString(),
            DefinitionYaml = entity.DefinitionYaml,
            LastSyncedAtUtc = entity.LastSyncedAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
        };

    private static BuildResponse ToBuildResponse(BuildEntity entity) =>
        new()
        {
            Id = entity.Id,
            PipelineId = entity.PipelineId,
            BuildNumber = entity.BuildNumber,
            ExternalBuildId = entity.ExternalBuildId,
            Status = entity.Status.ToString(),
            TriggeredAtUtc = entity.TriggeredAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc,
            LogContent = entity.LogContent,
        };
}
