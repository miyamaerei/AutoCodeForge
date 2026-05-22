namespace AutoCodeForge.Core.DTOs.Task;

/// <summary>
/// 工序响应
/// </summary>
public class TaskStepResponse
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public int Step { get; set; }
    public string StepType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? WorkerAgentId { get; set; }
    public Guid? ReviewerAgentId { get; set; }
    public string? Input { get; set; }
    public string? Output { get; set; }
    public string? SkipReason { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
