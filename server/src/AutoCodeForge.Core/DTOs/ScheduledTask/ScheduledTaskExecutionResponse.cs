namespace AutoCodeForge.Core.DTOs.ScheduledTask;

/// <summary>
/// Represents one scheduled task execution history record.
/// </summary>
public class ScheduledTaskExecutionResponse
{
    /// <summary>
    /// Gets or sets execution identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets scheduled task identifier.
    /// </summary>
    public Guid ScheduledTaskId { get; set; }

    /// <summary>
    /// Gets or sets execution status name.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets execution start timestamp in UTC.
    /// </summary>
    public DateTime StartedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets completion timestamp in UTC.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets optional output message.
    /// </summary>
    public string? Output { get; set; }
}
