namespace AutoCodeForge.Core.DTOs.Task;

/// <summary>
/// Represents one task response payload.
/// </summary>
public class TaskResponse
{
    /// <summary>
    /// Gets or sets task identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets task title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional task description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets current status text.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets task type text.
    /// </summary>
    public string TaskType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets progress percentage.
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// Gets or sets task input payload.
    /// </summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets task result payload.
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// Gets or sets last error message.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets bound agent identifier.
    /// </summary>
    public Guid? AgentId { get; set; }

    /// <summary>
    /// Gets or sets optional due timestamp in UTC.
    /// </summary>
    public DateTime? DueAtUtc { get; set; }

    /// <summary>
    /// Gets or sets execution start timestamp in UTC.
    /// </summary>
    public DateTime? StartedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets execution completion timestamp in UTC.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the current step number (1-7).
    /// </summary>
    public int? CurrentStep { get; set; }

    /// <summary>
    /// Gets or sets the current step ID.
    /// </summary>
    public Guid? CurrentStepId { get; set; }

    /// <summary>
    /// Gets or sets creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets update timestamp in UTC.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets associated workflow identifier.
    /// </summary>
    public Guid? WorkflowId { get; set; }

    /// <summary>
    /// Gets or sets associated workflow instance identifier.
    /// </summary>
    public Guid? WorkflowInstanceId { get; set; }

    /// <summary>
    /// Gets or sets current workflow node identifier.
    /// </summary>
    public string? WorkflowNodeId { get; set; }
}