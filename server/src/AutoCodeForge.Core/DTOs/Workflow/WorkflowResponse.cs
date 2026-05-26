namespace AutoCodeForge.Core.DTOs.Workflow;

/// <summary>
/// 工作流响应
/// </summary>
public class WorkflowResponse
{
    /// <summary>
    /// 工作流ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 工作流名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 工作流描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 工作流状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 版本号
    /// </summary>
    public int Version { get; set; }

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

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
