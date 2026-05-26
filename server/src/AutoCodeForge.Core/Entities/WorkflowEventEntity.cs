using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// 工作流事件实体 - 存储工作流执行过程中的事件日志
/// </summary>
[SugarTable("WorkflowEvents")]
public class WorkflowEventEntity : UserOwnedEntity
{
    /// <summary>
    /// 关联的工作流实例ID
    /// </summary>
    public Guid InstanceId { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    [SugarColumn(Length = 50, IsNullable = false)]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// 事件消息
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Message { get; set; }

    /// <summary>
    /// 事件数据 (JSON)
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? DataJson { get; set; }

    /// <summary>
    /// 关联的节点ID (可选)
    /// </summary>
    [SugarColumn(Length = 100, IsNullable = true)]
    public string? NodeId { get; set; }

    /// <summary>
    /// 事件级别 (Info/Warning/Error)
    /// </summary>
    [SugarColumn(Length = 20, IsNullable = false)]
    public string Level { get; set; } = "Info";
}
