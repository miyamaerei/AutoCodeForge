using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Core.DTOs.ScheduledTask;

/// <summary>
/// Represents one scheduled task creation request.
/// </summary>
public class CreateScheduledTaskRequest
{
    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Cron expression (6-part with seconds, e.g. "0 */5 * * * ?").
    /// </summary>
    [Required]
    [MaxLength(120)]
    public string CronExpression { get; set; } = "0 */5 * * * ?";

    /// <summary>
    /// Gets or sets trigger type.
    /// </summary>
    public TriggerType TriggerType { get; set; } = TriggerType.Cron;

    /// <summary>
    /// Gets or sets optional bound agent identifier.
    /// </summary>
    public Guid? AgentId { get; set; }

    /// <summary>
    /// Gets or sets the input payload forwarded to every spawned task.
    /// </summary>
    [Required]
    [MaxLength(16000)]
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title template for spawned tasks.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string TaskTitle { get; set; } = string.Empty;
}
