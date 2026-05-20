namespace AutoCodeForge.Core.DTOs.Review;

/// <summary>
/// Represents one review task detail payload.
/// </summary>
public class ReviewTaskDetailDto : ReviewTaskSummaryDto
{
    /// <summary>
    /// Gets or sets the linked repository workspace record identifier.
    /// </summary>
    public Guid? WorkspaceRecordId { get; set; }

    /// <summary>
    /// Gets or sets the reviewed branch.
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// Gets or sets the reviewed commit SHA.
    /// </summary>
    public string? CommitSha { get; set; }
}