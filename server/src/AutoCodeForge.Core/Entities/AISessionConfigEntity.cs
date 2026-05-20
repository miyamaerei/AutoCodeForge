using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents chat/session runtime configuration.
/// </summary>
[SugarTable("AiSessionConfigs")]
public class AISessionConfigEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets session config identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets temperature.
    /// </summary>
    public decimal Temperature { get; set; } = 0.7m;

    /// <summary>
    /// Gets or sets maximum tokens.
    /// </summary>
    public int MaxTokens { get; set; } = 2048;

    /// <summary>
    /// Gets or sets optional system prompt.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? SystemPrompt { get; set; }
}
