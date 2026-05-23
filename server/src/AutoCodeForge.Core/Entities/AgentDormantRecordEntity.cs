using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// 休眠原因类型枚举
/// </summary>
public enum DormantReasonType
{
    /// <summary>
    /// 人工介入
    /// </summary>
    Manual = 0,
    /// <summary>
    /// 连续学习效果差
    /// </summary>
    PoorLearningPerformance = 1,
    /// <summary>
    /// 学习产出有害
    /// </summary>
    HarmfulOutput = 2,
    /// <summary>
    /// 连续任务失败
    /// </summary>
    ConsecutiveFailures = 3
}

/// <summary>
/// Represents an agent dormant record.
/// </summary>
[SugarTable("AgentDormantRecords")]
public class AgentDormantRecordEntity : UserOwnedEntity
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
    /// 休眠原因类型
    /// </summary>
    public DormantReasonType ReasonType { get; set; }

    /// <summary>
    /// 休眠原因详细描述
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? ReasonDescription { get; set; }

    /// <summary>
    /// 进入休眠时间（UTC）
    /// </summary>
    public DateTime DormantAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 唤醒时间（UTC）
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? WokenAtUtc { get; set; }

    /// <summary>
    /// 是否已唤醒
    /// </summary>
    public bool IsWoken { get; set; }

    /// <summary>
    /// 唤醒操作人的NTID
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = true)]
    public string? WokenByNtId { get; set; }

    /// <summary>
    /// 唤醒备注
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? WokenRemark { get; set; }
}