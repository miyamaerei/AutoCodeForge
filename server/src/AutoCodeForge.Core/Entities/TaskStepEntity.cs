using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// 代表任务的工序步骤
/// </summary>
[SugarTable("TaskSteps")]
public class TaskStepEntity : AuditableEntity
{
    /// <summary>
    /// 工序 ID
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// 关联的任务 ID
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// 工序编号 (1-7)
    /// </summary>
    public int Step { get; set; }

    /// <summary>
    /// 工序类型
    /// </summary>
    public TaskStepType StepType { get; set; }

    /// <summary>
    /// 工序状态
    /// </summary>
    public TaskStepStatus Status { get; set; } = TaskStepStatus.Pending;

    /// <summary>
    /// 执行该工序的 Agent ID
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? WorkerAgentId { get; set; }

    /// <summary>
    /// 审核该工序的 Agent ID
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? ReviewerAgentId { get; set; }

    /// <summary>
    /// 工序输入内容
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Input { get; set; }

    /// <summary>
    /// 工序输出内容
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Output { get; set; }

    /// <summary>
    /// 跳过原因（仅当状态为 Skipped 时有值）
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? SkipReason { get; set; }

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
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// 乐观锁版本号
    /// </summary>
    public int Version { get; set; }
}

/// <summary>
/// 工序类型枚举
/// </summary>
public enum TaskStepType
{
    /// <summary>
    /// 需求梳理
    /// </summary>
    DemandAnalyse = 1,
    /// <summary>
    /// 查询当前信息
    /// </summary>
    QueryCurrent = 2,
    /// <summary>
    /// 方案计划
    /// </summary>
    MakePlan = 3,
    /// <summary>
    /// 代码开发
    /// </summary>
    Development = 4,
    /// <summary>
    /// 测试校验
    /// </summary>
    TestVerify = 5,
    /// <summary>
    /// 版本提交
    /// </summary>
    CommitPr = 6,
    /// <summary>
    /// 最终审核
    /// </summary>
    FinalAudit = 7
}

/// <summary>
/// 工序状态枚举
/// </summary>
public enum TaskStepStatus
{
    /// <summary>
    /// 待处理
    /// </summary>
    Pending = 0,
    /// <summary>
    /// 处理中
    /// </summary>
    Handling = 1,
    /// <summary>
    /// 已完成
    /// </summary>
    Completed = 2,
    /// <summary>
    /// 失败
    /// </summary>
    Failed = 3,
    /// <summary>
    /// 已跳过
    /// </summary>
    Skipped = 4
}
