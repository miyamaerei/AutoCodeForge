using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// 工作流实例实体 - 存储工作流的一次执行记录
/// </summary>
[SugarTable("WorkflowInstances")]
public class WorkflowInstanceEntity : UserOwnedEntity
{
    /// <summary>
    /// 获取或设置实体标识
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 关联的工作流定义ID
    /// </summary>
    public Guid WorkflowId { get; set; }

    /// <summary>
    /// 当前执行的节点ID
    /// </summary>
    [SugarColumn(Length = 100, IsNullable = true)]
    public string? CurrentNodeId { get; set; }

    /// <summary>
    /// 序列化的工作流输入数据 (JSON)
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? InputJson { get; set; }

    /// <summary>
    /// 序列化的工作流输出数据 (JSON)
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? OutputJson { get; set; }

    /// <summary>
    /// 执行状态
    /// </summary>
    public WorkflowInstanceStatus Status { get; set; } = WorkflowInstanceStatus.Pending;

    /// <summary>
    /// 执行进度百分比 (0-100)
    /// </summary>
    public int Progress { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 开始执行时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? StartedAtUtc { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// 关联的Agent ID (可选)
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? AgentId { get; set; }

    /// <summary>
    /// 执行上下文数据 (JSON)
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? ContextJson { get; set; }
}

/// <summary>
/// 工作流实例状态枚举
/// </summary>
public enum WorkflowInstanceStatus
{
    /// <summary>
    /// 等待执行
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 正在执行
    /// </summary>
    Running = 1,

    /// <summary>
    /// 已暂停
    /// </summary>
    Paused = 2,

    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 3,

    /// <summary>
    /// 执行失败
    /// </summary>
    Failed = 4,

    /// <summary>
    /// 已终止
    /// </summary>
    Terminated = 5,
}
