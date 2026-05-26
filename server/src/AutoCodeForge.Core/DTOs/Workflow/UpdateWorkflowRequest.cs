namespace AutoCodeForge.Core.DTOs.Workflow;

/// <summary>
/// 更新工作流请求
/// </summary>
public class UpdateWorkflowRequest
{
    /// <summary>
    /// 工作流名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 工作流描述
    /// </summary>
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

    /// <summary>
    /// 工作流状态
    /// </summary>
    public WorkflowStatus? Status { get; set; }
}

/// <summary>
/// 工作流状态枚举
/// </summary>
public enum WorkflowStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}
