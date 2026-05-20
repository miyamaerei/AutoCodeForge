namespace AutoCodeForge.Core.DTOs.Repository;

/// <summary>
/// Represents a git commit.
/// </summary>
public class GitCommitDto
{
    /// <summary>
    /// Gets or sets the commit SHA.
    /// </summary>
    public string Sha { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the commit message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the commit author.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the commit date.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }
}
