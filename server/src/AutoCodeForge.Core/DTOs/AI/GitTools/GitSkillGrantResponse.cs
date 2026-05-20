namespace AutoCodeForge.Core.DTOs.AI.GitTools;

/// <summary>
/// Response representing one repository-level Git skill grant.
/// </summary>
public class GitSkillGrantResponse
{
    /// <summary>
    /// Gets or sets repository identifier.
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets permission level text.
    /// </summary>
    public string Level { get; set; } = "ReadOnly";

    /// <summary>
    /// Gets or sets update timestamp.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
