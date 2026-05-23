namespace AutoCodeForge.Core.DTOs.Task;

/// <summary>
/// Represents a human gate response payload.
/// </summary>
public class HumanGateResponse
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid? TaskStepId { get; set; }
    public string GateType { get; set; } = string.Empty;
    public string GateTypeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? HumanResponse { get; set; }
    public object? Modifications { get; set; }
    public Guid? ReviewerUserId { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? RespondedAtUtc { get; set; }
}