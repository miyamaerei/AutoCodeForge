using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Repository;

/// <summary>
/// Request to create a pull/merge request.
/// </summary>
public class CreateGitPullRequestRequest
{
    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description/body.
    /// </summary>
    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the source branch.
    /// </summary>
    [Required(ErrorMessage = "Source branch is required")]
    [StringLength(100)]
    public string SourceBranch { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target branch.
    /// </summary>
    [Required(ErrorMessage = "Target branch is required")]
    [StringLength(100)]
    public string TargetBranch { get; set; } = string.Empty;
}
