using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Core.DTOs.Review;

/// <summary>
/// Represents one executable review rule.
/// </summary>
public class ReviewRuleDto
{
    /// <summary>
    /// Gets or sets the stable rule code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rule display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the severity emitted on match.
    /// </summary>
    public ReviewFindingSeverity Severity { get; set; } = ReviewFindingSeverity.Info;

    /// <summary>
    /// Gets or sets the optional file glob pattern.
    /// </summary>
    public string? FilePattern { get; set; }

    /// <summary>
    /// Gets or sets the required literal text.
    /// </summary>
    public string? ContainsText { get; set; }

    /// <summary>
    /// Gets or sets the finding message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional remediation suggestion.
    /// </summary>
    public string? Suggestion { get; set; }
}

/// <summary>
/// Represents the immutable input for one review execution.
/// </summary>
public class ReviewExecutionRequestDto
{
    /// <summary>
    /// Gets or sets the repository workspace root path.
    /// </summary>
    public string WorkspacePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rules to evaluate.
    /// </summary>
    public IReadOnlyList<ReviewRuleDto> Rules { get; set; } = [];
}

/// <summary>
/// Represents the result of one review execution.
/// </summary>
public class ReviewExecutionResultDto
{
    /// <summary>
    /// Gets or sets the findings emitted during execution.
    /// </summary>
    public IReadOnlyList<ReviewFindingDto> Findings { get; set; } = [];

    /// <summary>
    /// Gets or sets the number of scanned files.
    /// </summary>
    public int FilesScanned { get; set; }

    /// <summary>
    /// Gets or sets the number of evaluated rules.
    /// </summary>
    public int RulesEvaluated { get; set; }

    /// <summary>
    /// Gets or sets the non-fatal execution errors.
    /// </summary>
    public IReadOnlyList<string> Errors { get; set; } = [];
}