using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents one pipeline build execution.
/// </summary>
[SugarTable("Builds")]
public class BuildEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the build identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning pipeline identifier.
    /// </summary>
    public Guid PipelineId { get; set; }

    /// <summary>
    /// Gets or sets build number.
    /// </summary>
    [SugarColumn(Length = 60, IsNullable = false)]
    public string BuildNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets external build identifier from CI/CD provider.
    /// </summary>
    [SugarColumn(Length = 200, IsNullable = true)]
    public string? ExternalBuildId { get; set; }

    /// <summary>
    /// Gets or sets current build status.
    /// </summary>
    public BuildStatus Status { get; set; } = BuildStatus.Queued;

    /// <summary>
    /// Gets or sets triggered timestamp in UTC.
    /// </summary>
    public DateTime TriggeredAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets completion timestamp in UTC.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets optional build log content.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? LogContent { get; set; }
}

/// <summary>
/// Defines build status values.
/// </summary>
public enum BuildStatus
{
    Queued = 0,
    Running = 1,
    Succeeded = 2,
    Failed = 3,
    Cancelled = 4,
}
