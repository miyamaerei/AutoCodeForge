using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

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
}
