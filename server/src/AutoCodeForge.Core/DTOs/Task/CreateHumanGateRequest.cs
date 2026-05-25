using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Task;

/// <summary>
/// Represents a request to create a human gate.
/// </summary>
public class CreateHumanGateRequest
{
    [Required]
    public Guid TaskId { get; set; }

    public Guid? TaskStepId { get; set; }

    [Required]
    public string GateType { get; set; } = string.Empty;

    public string? Reason { get; set; }
}