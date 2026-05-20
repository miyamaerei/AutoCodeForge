using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.RepoSync;

/// <summary>
/// Represents a request to create a repository sync task.
/// </summary>
public class CreateRepoSyncTaskRequest
{
    /// <summary>
    /// Gets or sets target repository identifier.
    /// </summary>
    [Required]
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets target branch. Defaults to main.
    /// </summary>
    [MaxLength(200)]
    public string? Branch { get; set; }

    /// <summary>
    /// Gets or sets optional task title.
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets optional task description.
    /// </summary>
    [MaxLength(4000)]
    public string? Description { get; set; }
}
