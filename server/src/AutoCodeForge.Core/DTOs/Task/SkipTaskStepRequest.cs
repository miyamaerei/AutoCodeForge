using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Task;

/// <summary>
/// 跳过工序的请求
/// </summary>
public class SkipTaskStepRequest
{
    [Required]
    public string Reason { get; set; } = string.Empty;
}
