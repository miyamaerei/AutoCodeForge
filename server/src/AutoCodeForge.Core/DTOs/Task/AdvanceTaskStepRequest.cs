using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Task;

/// <summary>
/// 推进工序的请求
/// </summary>
public class AdvanceTaskStepRequest
{
    [Required]
    public string Output { get; set; } = string.Empty;
}
