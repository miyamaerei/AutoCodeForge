using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Task;

/// <summary>
/// 创建工序的请求
/// </summary>
public class CreateTaskStepRequest
{
    [Required]
    public Guid TaskId { get; set; }

    [Required]
    [Range(1, 7)]
    public int Step { get; set; }

    [Required]
    public Core.Entities.TaskStepType StepType { get; set; }

    public string? Input { get; set; }
}
