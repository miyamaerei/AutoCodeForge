using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents a repository pipeline definition.
/// </summary>
[SugarTable("Pipelines")]
public class PipelineEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the pipeline identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the related repository identifier.
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets the pipeline name.
    /// </summary>
    [SugarColumn(Length = 180, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets third-party pipeline identifier.
    /// </summary>
    [SugarColumn(Length = 200, IsNullable = true)]
    public string? ExternalPipelineId { get; set; }

    /// <summary>
    /// Gets or sets status.
    /// </summary>
    public PipelineStatus Status { get; set; } = PipelineStatus.Active;

    /// <summary>
    /// Gets or sets optional yaml body.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? DefinitionYaml { get; set; }

    /// <summary>
    /// Gets or sets last synchronization timestamp in UTC.
    /// </summary>
    public DateTime? LastSyncedAtUtc { get; set; }
}

/// <summary>
/// Defines pipeline statuses.
/// </summary>
public enum PipelineStatus
{
    Draft = 0,
    Active = 1,
    Disabled = 2,
}
