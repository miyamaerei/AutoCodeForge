namespace AutoCodeForge.Core.DTOs.Repository;

/// <summary>
/// Represents a git pull/merge request.
/// </summary>
public class GitPullRequestDto
{
    /// <summary>
    /// Gets or sets the pull request identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pull request number.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description/body.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the source branch.
    /// </summary>
    public string SourceBranch { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target branch.
    /// </summary>
    public string TargetBranch { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the PR state (open, closed, merged).
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the web URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
