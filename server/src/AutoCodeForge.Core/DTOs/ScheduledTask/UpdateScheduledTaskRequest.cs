using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Core.DTOs.ScheduledTask;

/// <summary>
/// Represents one scheduled task update request.
/// </summary>
public class UpdateScheduledTaskRequest
{
    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the Cron expression.
    /// </summary>
    [MaxLength(120)]
    public string? CronExpression { get; set; }

    /// <summary>
    /// Gets or sets optional bound agent identifier.
    /// </summary>
    public Guid? AgentId { get; set; }

    /// <summary>
    /// Gets or sets the input payload forwarded to every spawned task.
    /// </summary>
    [MaxLength(16000)]
    public string? Input { get; set; }

    /// <summary>
    /// Gets or sets the title template for spawned tasks.
    /// </summary>
    [MaxLength(200)]
    public string? TaskTitle { get; set; }

    /// <summary>
    /// Gets or sets the schedule status.
    /// </summary>
    public ScheduleStatus? Status { get; set; }
}
