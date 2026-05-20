using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Core.DTOs.Review;

/// <summary>
/// Represents one review finding returned to callers.
/// </summary>
public class ReviewFindingDto
{
    /// <summary>
    /// Gets or sets the severity.
    /// </summary>
    public ReviewFindingSeverity Severity { get; set; } = ReviewFindingSeverity.Info;

    /// <summary>
    /// Gets or sets the rule code.
    /// </summary>
    public string RuleCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository-relative file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the matching line number.
    /// </summary>
    public int? LineNumber { get; set; }

    /// <summary>
    /// Gets or sets the finding message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional remediation suggestion.
    /// </summary>
    public string? Suggestion { get; set; }

    /// <summary>
    /// Gets or sets the optional evidence line.
    /// </summary>
    public string? Evidence { get; set; }
}