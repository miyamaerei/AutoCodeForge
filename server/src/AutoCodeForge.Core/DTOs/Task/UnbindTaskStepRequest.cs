using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Task;

/// <summary>
/// 解绑工序的请求
/// </summary>
public class UnbindTaskStepRequest
{
    [Required]
    public string Reason { get; set; } = string.Empty;
    public string? FailureCategory { get; set; }
}
