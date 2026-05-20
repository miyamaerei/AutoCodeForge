using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents one finding emitted by a review execution.
/// </summary>
[SugarTable("ReviewFindings")]
public class ReviewFindingEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the finding identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the owning review task identifier.
    /// </summary>
    public Guid ReviewTaskId { get; set; }

    /// <summary>
    /// Gets or sets the normalized severity.
    /// </summary>
    public ReviewFindingSeverity Severity { get; set; } = ReviewFindingSeverity.Info;

    /// <summary>
    /// Gets or sets the rule code.
    /// </summary>
    [SugarColumn(Length = 100, IsNullable = false)]
    public string RuleCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository-relative file path.
    /// </summary>
    [SugarColumn(Length = 1000, IsNullable = false)]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the matching line number.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public int? LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the human-readable message.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional suggestion.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Suggestion { get; set; }

    /// <summary>
    /// Gets or sets optional evidence captured from the source file.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Evidence { get; set; }
}

/// <summary>
/// Defines supported review severities.
/// </summary>
public enum ReviewFindingSeverity
{
    Critical = 0,
    High = 1,
    Medium = 2,
    Low = 3,
    Info = 4,
}