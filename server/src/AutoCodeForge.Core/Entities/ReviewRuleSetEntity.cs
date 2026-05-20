using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents a reusable review rule set definition.
/// </summary>
[SugarTable("ReviewRuleSets")]
public class ReviewRuleSetEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the rule set identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the rule set name.
    /// </summary>
    [SugarColumn(Length = 200, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional rule set description.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the rule set scope level.
    /// </summary>
    public ReviewRuleSetLevel Level { get; set; } = ReviewRuleSetLevel.Global;

    /// <summary>
    /// Gets or sets the repository identifier when the rule set is repository-scoped.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the rule set can be selected.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the semantic version of the rule set.
    /// </summary>
    [SugarColumn(Length = 50, IsNullable = false)]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the serialized rule payload.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
    public string RulesJson { get; set; } = "[]";
}

/// <summary>
/// Defines supported review rule set scopes.
/// </summary>
public enum ReviewRuleSetLevel
{
    Global = 0,
    Repository = 1,
}