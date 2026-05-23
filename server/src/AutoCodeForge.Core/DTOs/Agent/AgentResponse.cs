namespace AutoCodeForge.Core.DTOs.Agent;

/// <summary>
/// Represents an agent response.
/// </summary>
public class AgentResponse
{
    /// <summary>
    /// Gets or sets agent identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets agent name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets matching keywords.
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// Gets or sets system prompt.
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Gets or sets model config identifier.
    /// </summary>
    public Guid? LlmModelConfigId { get; set; }

    /// <summary>
    /// Gets or sets whether agent is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets UTC created timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets UTC updated timestamp.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// Agent 当前状态
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Agent 角色
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 状态变更时间戳（UTC）
    /// </summary>
    public DateTime StateChangedAtUtc { get; set; }

    /// <summary>
    /// 当前处理的任务ID
    /// </summary>
    public Guid? CurrentTaskId { get; set; }

    /// <summary>
    /// 休眠原因（当 State = Dormant 时有效）
    /// </summary>
    public string? DormantReason { get; set; }

    /// <summary>
    /// 技能标签（逗号分隔）
    /// </summary>
    public string? SkillTags { get; set; }

    /// <summary>
    /// 学习进度（0-100）
    /// </summary>
    public int LearningProgress { get; set; }

    /// <summary>
    /// 乐观锁版本号
    /// </summary>
    public int Version { get; set; }
}

/// <summary>
/// 分配任务请求
/// </summary>
public class AssignTaskRequest
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public Guid TaskId { get; set; }
}

/// <summary>
/// 标记任务失败请求
/// </summary>
public class FailTaskRequest
{
    /// <summary>
    /// 失败原因
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// 失败类别
    /// </summary>
    public string FailureCategory { get; set; } = string.Empty;
}

/// <summary>
/// 进入休眠请求
/// </summary>
public class EnterDormantRequest
{
    /// <summary>
    /// 休眠原因
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// 完成学习请求
/// </summary>
public class CompleteLearningRequest
{
    /// <summary>
    /// 学习总结
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 学习结果/产出
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// 新增的技能标签（逗号分隔）
    /// </summary>
    public string? SkillTags { get; set; }
}

/// <summary>
/// Agent学习记录响应
/// </summary>
public class AgentLearningRecordResponse
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public string TriggerType { get; set; } = string.Empty;
    public string? TriggerReason { get; set; }
    public Guid? RelatedTaskId { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public bool IsSuccessful { get; set; }
    public string? LearningResult { get; set; }
    public string? NewSkillTags { get; set; }
    public string? ErrorMessage { get; set; }
    public int? EffectivenessScore { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

/// <summary>
/// Agent休眠记录响应
/// </summary>
public class AgentDormantRecordResponse
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public string ReasonType { get; set; } = string.Empty;
    public string? ReasonDescription { get; set; }
    public DateTime DormantAtUtc { get; set; }
    public DateTime? WokenAtUtc { get; set; }
    public bool IsWoken { get; set; }
    public string? WokenByNtId { get; set; }
    public string? WokenRemark { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
