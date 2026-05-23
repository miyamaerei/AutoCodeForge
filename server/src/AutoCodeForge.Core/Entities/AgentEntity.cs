using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Agent 四状态枚举
/// </summary>
public enum AgentState
{
    /// <summary>
    /// 空闲 - 可接单
    /// </summary>
    Idle = 0,
    /// <summary>
    /// 处理问题 - 锁定资源
    /// </summary>
    Handling = 1,
    /// <summary>
    /// 学习 - 暂停接单
    /// </summary>
    Learning = 2,
    /// <summary>
    /// 休眠 - 不学习、不接单、等待人工介入
    /// </summary>
    Dormant = 3
}

/// <summary>
/// Agent 角色枚举
/// </summary>
public enum AgentRole
{
    /// <summary>
    /// 秘书 - 响应快，60秒无任务即触发学习
    /// </summary>
    Secretary = 0,
    /// <summary>
    /// 经理 - 决策慢，120秒无任务即触发学习
    /// </summary>
    Manager = 1,
    /// <summary>
    /// 工人 - 执行快，300秒无任务即触发学习
    /// </summary>
    Worker = 2
}

/// <summary>
/// 失败类别枚举
/// </summary>
public enum FailureCategory
{
    /// <summary>
    /// 代码错误
    /// </summary>
    CodeError = 0,
    /// <summary>
    /// LLM异常
    /// </summary>
    LlmException = 1,
    /// <summary>
    /// 需求问题
    /// </summary>
    RequirementIssue = 2,
    /// <summary>
    /// 评审驳回
    /// </summary>
    ReviewRejection = 3,
    /// <summary>
    /// 超时
    /// </summary>
    Timeout = 4,
    /// <summary>
    /// 未知
    /// </summary>
    Unknown = 5
}

/// <summary>
/// Represents an automation agent profile.
/// </summary>
[SugarTable("Agents")]
public class AgentEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the agent identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the agent name.
    /// </summary>
    [SugarColumn(Length = 120, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the agent description.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets matching keywords separated by comma.
    /// </summary>
    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Keywords { get; set; }

    /// <summary>
    /// Gets or sets optional system prompt.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Gets or sets the selected model config identifier.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? LlmModelConfigId { get; set; }

    /// <summary>
    /// Gets or sets comma-separated tool names supported by this agent.
    /// Used for filtering available tools in tool execution pipeline.
    /// </summary>
    [SugarColumn(Length = 500, IsNullable = true)]
    public string? ToolNames { get; set; }

    /// <summary>
    /// Gets or sets the agent skill profile (ReadOnly, Collaborator, Reviewer).
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? SkillProfile { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the agent is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Agent 当前状态
    /// </summary>
    public AgentState State { get; set; } = AgentState.Idle;

    /// <summary>
    /// Agent 角色
    /// </summary>
    public AgentRole Role { get; set; } = AgentRole.Worker;

    /// <summary>
    /// 状态变更时间戳（UTC）
    /// </summary>
    public DateTime StateChangedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 当前处理的任务ID
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? CurrentTaskId { get; set; }

    /// <summary>
    /// 休眠原因（当 State = Dormant 时有效）
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? DormantReason { get; set; }

    /// <summary>
    /// 技能标签（逗号分隔）
    /// </summary>
    [SugarColumn(Length = 1000, IsNullable = true)]
    public string? SkillTags { get; set; }

    /// <summary>
    /// 学习进度（0-100）
    /// </summary>
    public int LearningProgress { get; set; }

    /// <summary>
    /// 当前任务计数（用于负载均衡）
    /// </summary>
    public int CurrentTaskCount { get; set; } = 0;

    /// <summary>
    /// 乐观锁版本号
    /// </summary>
    public int Version { get; set; } = 0;
}
