namespace AutoCodeForge.Core.DTOs.ScheduledTask;

/// <summary>
/// Represents one scheduled task response payload.
/// </summary>
public class ScheduledTaskResponse
{
    /// <summary>
    /// Gets or sets the scheduled task identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets trigger type name.
    /// </summary>
    public string TriggerType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Cron expression.
    /// </summary>
    public string CronExpression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schedule status name.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional bound agent identifier.
    /// </summary>
    public Guid? AgentId { get; set; }

    /// <summary>
    /// Gets or sets the input payload.
    /// </summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title template for spawned tasks.
    /// </summary>
    public string TaskTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets next execution timestamp in UTC.
    /// </summary>
    public DateTime? NextRunAtUtc { get; set; }

    /// <summary>
    /// Gets or sets creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }
}
