using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Task;

/// <summary>
/// Represents one task creation request.
/// </summary>
public class CreateTaskRequest
{
    /// <summary>
    /// Gets or sets task title.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional task description.
    /// </summary>
    [MaxLength(4000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets task input payload used by the executor.
    /// </summary>
    [Required]
    [MaxLength(16000)]
    public string Input { get; set; } = string.Empty;

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