namespace AutoCodeForge.Core.DTOs.Workflow;

/// <summary>
/// 工作流实例响应
/// </summary>
public class WorkflowInstanceResponse
{
    /// <summary>
    /// 实例ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 工作流ID
    /// </summary>
    public Guid WorkflowId { get; set; }

    /// <summary>
    /// 工作流名称
    /// </summary>
    public string WorkflowName { get; set; } = string.Empty;

    /// <summary>
    /// 当前执行节点ID
    /// </summary>
    public string? CurrentNodeId { get; set; }

    /// <summary>
    /// 执行状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 执行进度 (0-100)
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// 输入数据 (JSON)
    /// </summary>
    public string? InputJson { get; set; }

    /// <summary>
    /// 输出数据 (JSON)
    /// </summary>
    public string? OutputJson { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartedAtUtc { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// 关联的Agent ID
    /// </summary>
    public Guid? AgentId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
