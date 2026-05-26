using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// 工作流定义实体 - 存储工作流的结构定义
/// </summary>
[SugarTable("Workflows")]
public class WorkflowEntity : UserOwnedEntity
{
    /// <summary>
    /// 工作流名称
    /// </summary>
    [SugarColumn(Length = 200, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 工作流描述
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 序列化的工作流节点配置 (JSON)
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? NodesJson { get; set; }

    /// <summary>
    /// 序列化的工作流边配置 (JSON)
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? EdgesJson { get; set; }

    /// <summary>
    /// 序列化的工作流执行器配置 (JSON)
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? ExecutorsJson { get; set; }

    /// <summary>
    /// 上下文提供者列表 (JSON数组)
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? ContextProvidersJson { get; set; }

    /// <summary>
    /// 工作流状态
    /// </summary>
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;

    /// <summary>
    /// 版本号
    /// </summary>
    public int Version { get; set; } = 1;
}

/// <summary>
/// 工作流状态枚举
/// </summary>
public enum WorkflowStatus
{
    /// <summary>
    /// 草稿状态
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 已发布状态
    /// </summary>
    Published = 1,

    /// <summary>
    /// 已归档状态
    /// </summary>
    Archived = 2,
}
