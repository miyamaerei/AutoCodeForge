namespace AutoCodeForge.Core.DTOs.Task;

/// <summary>
/// Represents one task log response payload.
/// </summary>
public class TaskLogResponse
{
    /// <summary>
    /// Gets or sets task log identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets task identifier.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Gets or sets log level.
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets log message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional log source.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }
}
