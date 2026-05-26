using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Workflow;

/// <summary>
/// 创建工作流请求
/// </summary>
public class CreateWorkflowRequest
{
    /// <summary>
    /// 工作流名称
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 工作流描述
    /// </summary>
    [MaxLength(4000)]
    public string? Description { get; set; }

    /// <summary>
    /// 工作流节点配置 (JSON)
    /// </summary>
    public string? NodesJson { get; set; }

    /// <summary>
    /// 工作流边配置 (JSON)
    /// </summary>
    public string? EdgesJson { get; set; }

    /// <summary>
    /// 工作流执行器配置 (JSON)
    /// </summary>
    public string? ExecutorsJson { get; set; }

    /// <summary>
    /// 上下文提供者列表
    /// </summary>
    public List<string>? ContextProviders { get; set; }
}
