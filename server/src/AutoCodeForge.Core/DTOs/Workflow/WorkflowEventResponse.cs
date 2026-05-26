namespace AutoCodeForge.Core.DTOs.Workflow;

/// <summary>
/// 工作流事件响应 (用于SSE推送)
/// </summary>
public class WorkflowEventResponse
{
    /// <summary>
    /// 事件ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 工作流实例ID
    /// </summary>
    public Guid InstanceId { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// 事件消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 事件数据 (JSON)
    /// </summary>
    public string? DataJson { get; set; }

    /// <summary>
    /// 关联的节点ID
    /// </summary>
    public string? NodeId { get; set; }

    /// <summary>
    /// 事件级别
    /// </summary>
    public string Level { get; set; } = "Info";

    /// <summary>
    /// 事件时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 工作流事件类型常量
/// </summary>
public static class WorkflowEventTypes
{
    public const string Started = "started";
    public const string NodeEntered = "node_entered";
    public const string NodeCompleted = "node_completed";
    public const string NodeFailed = "node_failed";
    public const string ProgressUpdated = "progress_updated";
    public const string Paused = "paused";
    public const string Resumed = "resumed";
    public const string Completed = "completed";
    public const string Failed = "failed";
    public const string Terminated = "terminated";
}
