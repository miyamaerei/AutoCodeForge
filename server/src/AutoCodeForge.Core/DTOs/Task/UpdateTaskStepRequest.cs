namespace AutoCodeForge.Core.DTOs.Task;

/// <summary>
/// 更新工序的请求
/// </summary>
public class UpdateTaskStepRequest
{
    public Core.Entities.TaskStepStatus? Status { get; set; }
    public Guid? WorkerAgentId { get; set; }
    public Guid? ReviewerAgentId { get; set; }
    public string? Input { get; set; }
    public string? Output { get; set; }
    public string? SkipReason { get; set; }
}
