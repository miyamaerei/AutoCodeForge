using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents one scheduled execution record.
/// </summary>
[SugarTable("ScheduledTaskExecutions")]
public class ScheduledTaskExecutionEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets execution identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets scheduled task identifier.
    /// </summary>
    public Guid ScheduledTaskId { get; set; }

    /// <summary>
    /// Gets or sets execution status.
    /// </summary>
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Running;

    /// <summary>
    /// Gets or sets start time in UTC.
    /// </summary>
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets finish time in UTC.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets optional output message.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Output { get; set; }
}

/// <summary>
/// Defines execution statuses.
/// </summary>
public enum ExecutionStatus
{
    Running = 0,
    Succeeded = 1,
    Failed = 2,
    Cancelled = 3,
}
