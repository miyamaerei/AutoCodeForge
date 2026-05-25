using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// 学习触发类型枚举
/// </summary>
public enum LearningTriggerType
{
    /// <summary>
    /// 空闲超时触发
    /// </summary>
    IdleTimeout = 0,
    /// <summary>
    /// 任务后复盘触发
    /// </summary>
    TaskReview = 1,
    /// <summary>
    /// 异常触发
    /// </summary>
    Exception = 2,
    /// <summary>
    /// 手动触发
    /// </summary>
    Manual = 3
}

/// <summary>
/// Represents an agent learning record.
/// </summary>
[SugarTable("AgentLearningRecords")]
public class AgentLearningRecordEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the record identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Agent ID
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    /// 学习触发类型
    /// </summary>
    public LearningTriggerType TriggerType { get; set; }

    /// <summary>
    /// 触发原因描述
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? TriggerReason { get; set; }

    /// <summary>
    /// 关联的任务ID（如果是任务相关学习）
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? RelatedTaskId { get; set; }

    /// <summary>
    /// 学习开始时间（UTC）
    /// </summary>
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 学习完成时间（UTC）
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// 学习是否成功
    /// </summary>
    public bool IsSuccessful { get; set; }

    /// <summary>
    /// 学习结果（LLM返回的内容）
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? LearningResult { get; set; }

    /// <summary>
    /// 新增的技能标签（逗号分隔）
    /// </summary>
    [SugarColumn(Length = 1000, IsNullable = true)]
    public string? NewSkillTags { get; set; }

    /// <summary>
    /// 错误信息（如果学习失败）
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 学习效果评分（0-100）
    /// </summary>
    public int? EffectivenessScore { get; set; }
}