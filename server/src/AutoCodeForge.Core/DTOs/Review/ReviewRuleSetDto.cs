using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Core.DTOs.Review;

/// <summary>
/// Represents one review rule set payload.
/// </summary>
public class ReviewRuleSetDto
{
    /// <summary>
    /// Gets or sets the rule set identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the rule set name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the scope level.
    /// </summary>
    public ReviewRuleSetLevel Level { get; set; } = ReviewRuleSetLevel.Global;

    /// <summary>
    /// Gets or sets the bound repository identifier for repository-scoped rule sets.
    /// </summary>
    public Guid? RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the rule set is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the rule set version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the executable rules.
    /// </summary>
    public IReadOnlyList<ReviewRuleDto> Rules { get; set; } = [];

    /// <summary>
    /// Gets or sets the creation time in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the last update time in UTC.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}

/// <summary>
/// Represents one request to create a review rule set.
/// </summary>
public class CreateReviewRuleSetRequest
{
    /// <summary>
    /// Gets or sets the rule set name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description.
    /// </summary>
    [MaxLength(4000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the scope level.
    /// </summary>
    public ReviewRuleSetLevel Level { get; set; } = ReviewRuleSetLevel.Global;

    /// <summary>
    /// Gets or sets the bound repository identifier for repository-scoped rule sets.
    /// </summary>
    public Guid? RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the rule set is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the rule set version.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the executable rules.
    /// </summary>
    [Required]
    public List<ReviewRuleDto> Rules { get; set; } = [];
}

/// <summary>
/// Represents one request to update a review rule set.
/// </summary>
public class UpdateReviewRuleSetRequest
{
    /// <summary>
    /// Gets or sets the rule set name.
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the optional description.
    /// </summary>
    [MaxLength(4000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the scope level.
    /// </summary>
    public ReviewRuleSetLevel? Level { get; set; }

    /// <summary>
    /// Gets or sets the repository identifier for repository-scoped rule sets.
    /// </summary>
    public Guid? RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the rule set is enabled.
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    [MaxLength(50)]
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets replacement rules when provided.
    /// </summary>
    public List<ReviewRuleDto>? Rules { get; set; }
}