namespace AutoCodeForge.Core.DTOs.Workflow;

/// <summary>
/// 执行工作流请求
/// </summary>
public class ExecuteWorkflowRequest
{
    /// <summary>
    /// 输入数据类型
    /// </summary>
    public string Type { get; set; } = "DefaultInput";

    /// <summary>
    /// 输入数据 (JSON)
    /// </summary>
    public string? DataJson { get; set; }

    /// <summary>
    /// 执行上下文
    /// </summary>
    public WorkflowContext? Context { get; set; }

    /// <summary>
    /// 关联的Agent ID (可选)
    /// </summary>
    public Guid? AgentId { get; set; }
}

/// <summary>
/// 工作流执行上下文
/// </summary>
public class WorkflowContext
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// 元数据
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}
