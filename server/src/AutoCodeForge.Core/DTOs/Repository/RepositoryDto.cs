using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Core.DTOs.Repository;

/// <summary>
/// Response DTO for repository information.
/// </summary>
public class RepositoryDto
{
    /// <summary>
    /// Gets or sets the repository identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the repository name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider type.
    /// </summary>
    public GitProvider Provider { get; set; }

    /// <summary>
    /// Gets or sets the authentication type.
    /// </summary>
    public AuthenticationType AuthType { get; set; }

    /// <summary>
    /// Gets or sets the merge strategy.
    /// </summary>
    public MergeStrategy MergeStrategy { get; set; }

    /// <summary>
    /// Gets or sets the default review rule set identifier.
    /// </summary>
    public Guid? DefaultReviewRuleSetId { get; set; }

    /// <summary>
    /// Gets or sets the default branch name.
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}

/// <summary>
/// Request to create a repository.
/// </summary>
public class CreateRepositoryRequest
{
    /// <summary>
    /// Gets or sets the repository name.
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the repository URL.
    /// </summary>
    [Required(ErrorMessage = "URL is required")]
    [StringLength(500)]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider type.
    /// </summary>
    public GitProvider Provider { get; set; } = GitProvider.GitHub;

    /// <summary>
    /// Gets or sets the authentication type.
    /// </summary>
    public AuthenticationType AuthType { get; set; } = AuthenticationType.Token;

    /// <summary>
    /// Gets or sets the authentication token.
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    [StringLength(500)]
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the merge strategy.
    /// </summary>
    public MergeStrategy MergeStrategy { get; set; } = MergeStrategy.Squash;

    /// <summary>
    /// Gets or sets the default branch name.
    /// </summary>
    [StringLength(100)]
    public string? Branch { get; set; }
}

/// <summary>
/// Request to update a repository.
/// </summary>
public class UpdateRepositoryRequest
{
    /// <summary>
    /// Gets or sets the repository name.
    /// </summary>
    [StringLength(200)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the merge strategy.
    /// </summary>
    public MergeStrategy? MergeStrategy { get; set; }

    /// <summary>
    /// Gets or sets the default branch name.
    /// </summary>
    [StringLength(100)]
    public string? Branch { get; set; }
}
