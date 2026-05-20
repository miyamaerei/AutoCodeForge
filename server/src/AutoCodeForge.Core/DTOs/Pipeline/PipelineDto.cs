using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Core.DTOs.Pipeline;

/// <summary>
/// Represents one pipeline response payload.
/// </summary>
public class PipelineResponse
{
    /// <summary>
    /// Gets or sets the pipeline identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the repository identifier.
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets the pipeline display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets third-party pipeline identifier.
    /// </summary>
    public string? ExternalPipelineId { get; set; }

    /// <summary>
    /// Gets or sets the current status text.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional pipeline yaml content.
    /// </summary>
    public string? DefinitionYaml { get; set; }

    /// <summary>
    /// Gets or sets the latest synchronization timestamp in UTC.
    /// </summary>
    public DateTime? LastSyncedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets update timestamp in UTC.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}

/// <summary>
/// Represents one build history response payload.
/// </summary>
public class BuildResponse
{
    /// <summary>
    /// Gets or sets build identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets owning pipeline identifier.
    /// </summary>
    public Guid PipelineId { get; set; }

    /// <summary>
    /// Gets or sets build number.
    /// </summary>
    public string BuildNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets external build identifier.
    /// </summary>
    public string? ExternalBuildId { get; set; }

    /// <summary>
    /// Gets or sets build status text.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets trigger timestamp in UTC.
    /// </summary>
    public DateTime TriggeredAtUtc { get; set; }

    /// <summary>
    /// Gets or sets completion timestamp in UTC.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets optional build logs.
    /// </summary>
    public string? LogContent { get; set; }
}

/// <summary>
/// Request to create a new pipeline.
/// </summary>
public class CreatePipelineRequest
{
    /// <summary>
    /// Gets or sets repository identifier.
    /// </summary>
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets pipeline name.
    /// </summary>
    [Required]
    [StringLength(180)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets external pipeline identifier.
    /// </summary>
    [StringLength(200)]
    public string? ExternalPipelineId { get; set; }

    /// <summary>
    /// Gets or sets optional yaml definition.
    /// </summary>
    [MaxLength(100000)]
    public string? DefinitionYaml { get; set; }

    /// <summary>
    /// Gets or sets initial pipeline status.
    /// </summary>
    public PipelineStatus Status { get; set; } = PipelineStatus.Active;
}

/// <summary>
/// Request to update one pipeline.
/// </summary>
public class UpdatePipelineRequest
{
    /// <summary>
    /// Gets or sets pipeline name.
    /// </summary>
    [StringLength(180)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets external pipeline identifier.
    /// </summary>
    [StringLength(200)]
    public string? ExternalPipelineId { get; set; }

    /// <summary>
    /// Gets or sets updated yaml definition.
    /// </summary>
    [MaxLength(100000)]
    public string? DefinitionYaml { get; set; }

    /// <summary>
    /// Gets or sets pipeline status.
    /// </summary>
    public PipelineStatus? Status { get; set; }
}

/// <summary>
/// Request to trigger a pipeline build.
/// </summary>
public class TriggerPipelineRequest
{
    /// <summary>
    /// Gets or sets optional build number override.
    /// </summary>
    [StringLength(60)]
    public string? BuildNumber { get; set; }

    /// <summary>
    /// Gets or sets optional external build identifier.
    /// </summary>
    [StringLength(200)]
    public string? ExternalBuildId { get; set; }
}
