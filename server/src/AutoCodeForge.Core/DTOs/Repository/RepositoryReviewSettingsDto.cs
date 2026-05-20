namespace AutoCodeForge.Core.DTOs.Repository;

/// <summary>
/// Represents review-related repository settings.
/// </summary>
public class RepositoryReviewSettingsDto
{
    /// <summary>
    /// Gets or sets the repository identifier.
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets the default review rule set identifier.
    /// </summary>
    public Guid? DefaultReviewRuleSetId { get; set; }
}

/// <summary>
/// Represents one request to update repository review settings.
/// </summary>
public class UpdateRepositoryReviewSettingsRequest
{
    /// <summary>
    /// Gets or sets the default review rule set identifier.
    /// </summary>
    public Guid? DefaultReviewRuleSetId { get; set; }
}