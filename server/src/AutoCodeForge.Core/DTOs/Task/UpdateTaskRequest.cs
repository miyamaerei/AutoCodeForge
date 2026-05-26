using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Task;

/// <summary>
/// Represents one task update request.
/// </summary>
public class UpdateTaskRequest
{
    /// <summary>
    /// Gets or sets optional task title.
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets optional task description.
    /// </summary>
    [MaxLength(4000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets optional task input payload.
    /// </summary>
    [MaxLength(16000)]
    public string? Input { get; set; }

    /// <summary>
    /// Gets or sets optional bound agent identifier.
    /// </summary>
    public Guid? AgentId { get; set; }

    /// <summary>
    /// Gets or sets optional due timestamp in UTC.
    /// </summary>
    public DateTime? DueAtUtc { get; set; }

    /// <summary>
    /// Gets or sets optional workflow identifier to associate with the task.
    /// </summary>
    public Guid? WorkflowId { get; set; }
}