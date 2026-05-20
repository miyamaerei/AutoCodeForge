using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents the execution state for one review run.
/// </summary>
[SugarTable("ReviewTasks")]
public class ReviewTaskEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the review task identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the linked task identifier.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Gets or sets the repository identifier.
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets the rule set identifier used for execution.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? RuleSetId { get; set; }

    /// <summary>
    /// Gets or sets the execution snapshot payload.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
    public string SnapshotJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized summary payload.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? SummaryJson { get; set; }

    /// <summary>
    /// Gets or sets the current review execution status.
    /// </summary>
    public ReviewTaskStatus Status { get; set; } = ReviewTaskStatus.Pending;

    /// <summary>
    /// Gets or sets the execution start timestamp in UTC.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? StartedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the execution completion timestamp in UTC.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional execution error message.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Defines supported review task states.
/// </summary>
public enum ReviewTaskStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Canceled = 4,
}