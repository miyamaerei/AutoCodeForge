namespace AutoCodeForge.Core.DTOs.Repository;

/// <summary>
/// Represents a git branch.
/// </summary>
public class GitBranchDto
{
    /// <summary>
    /// Gets or sets the branch name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the commit SHA that the branch points to.
    /// </summary>
    public string CommitSha { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this is the default branch.
    /// </summary>
    public bool IsDefault { get; set; }
}
