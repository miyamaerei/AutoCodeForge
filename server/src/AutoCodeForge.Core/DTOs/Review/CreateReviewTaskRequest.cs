using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Review;

/// <summary>
/// Represents a request to create one review task.
/// </summary>
public class CreateReviewTaskRequest
{
    /// <summary>
    /// Gets or sets the repository identifier.
    /// </summary>
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets the optional rule set identifier.
    /// </summary>
    public Guid? RuleSetId { get; set; }

    /// <summary>
    /// Gets or sets the target branch.
    /// </summary>
    [MaxLength(200)]
    public string? Branch { get; set; }

    /// <summary>
    /// Gets or sets the optional task title.
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the optional task description.
    /// </summary>
    [MaxLength(4000)]
    public string? Description { get; set; }
}