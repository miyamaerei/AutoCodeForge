using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Wiki;

/// <summary>
/// Represents one wiki page response payload.
/// </summary>
public class WikiPageResponse
{
    /// <summary>
    /// Gets or sets wiki page identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets page title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets unique page slug.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets markdown content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets associated repository identifier.
    /// </summary>
    public Guid? RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets creation timestamp in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets update timestamp in UTC.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}

/// <summary>
/// Request to create one wiki page.
/// </summary>
public class CreateWikiPageRequest
{
    /// <summary>
    /// Gets or sets page title.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets unique page slug.
    /// </summary>
    [Required]
    [StringLength(260)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets markdown content.
    /// </summary>
    [Required]
    [MaxLength(200000)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets associated repository identifier.
    /// </summary>
    public Guid? RepositoryId { get; set; }
}

/// <summary>
/// Request to update one wiki page.
/// </summary>
public class UpdateWikiPageRequest
{
    /// <summary>
    /// Gets or sets page title.
    /// </summary>
    [StringLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets unique page slug.
    /// </summary>
    [StringLength(260)]
    public string? Slug { get; set; }

    /// <summary>
    /// Gets or sets markdown content.
    /// </summary>
    [MaxLength(200000)]
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets associated repository identifier.
    /// Set to Guid.Empty to clear repository association.
    /// </summary>
    public Guid? RepositoryId { get; set; }
}