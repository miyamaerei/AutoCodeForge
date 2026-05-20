using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents a user configured scheduler task.
/// </summary>
[SugarTable("ScheduledTasks")]
public class ScheduledTaskEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets scheduled task identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the task name.
    /// </summary>
    [SugarColumn(Length = 200, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets trigger mode.
    /// </summary>
    public TriggerType TriggerType { get; set; } = TriggerType.Cron;

    /// <summary>
    /// Gets or sets schedule expression.
    /// </summary>
    [SugarColumn(Length = 120, IsNullable = false)]
    public string CronExpression { get; set; } = "0 */5 * * * ?";

    /// <summary>
    /// Gets or sets schedule status.
    /// </summary>
    public ScheduleStatus Status { get; set; } = ScheduleStatus.Active;

    /// <summary>
    /// Gets or sets optional bound agent identifier used when spawning tasks.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? AgentId { get; set; }

    /// <summary>
    /// Gets or sets the input payload forwarded to every spawned task.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title template for spawned tasks.
    /// </summary>
    [SugarColumn(Length = 200, IsNullable = false)]
    public string TaskTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets next execution timestamp in UTC.
    /// </summary>
    public DateTime? NextRunAtUtc { get; set; }
}

/// <summary>
/// Defines scheduler trigger type.
/// </summary>
public enum TriggerType
{
    Cron = 0,
    Interval = 1,
    Once = 2,
}

/// <summary>
/// Defines scheduler state.
/// </summary>
public enum ScheduleStatus
{
    Active = 0,
    Paused = 1,
    Disabled = 2,
}
