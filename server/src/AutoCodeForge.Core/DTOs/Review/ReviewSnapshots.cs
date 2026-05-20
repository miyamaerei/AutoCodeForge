namespace AutoCodeForge.Core.DTOs.Review;

/// <summary>
/// Represents the immutable snapshot used by one review execution.
/// </summary>
public class ReviewExecutionSnapshotDto
{
    /// <summary>
    /// Gets or sets the repository identifier.
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets the workspace record identifier.
    /// </summary>
    public Guid WorkspaceRecordId { get; set; }

    /// <summary>
    /// Gets or sets the workspace path to scan.
    /// </summary>
    public string WorkspacePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target branch.
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// Gets or sets the commit SHA captured at task creation time.
    /// </summary>
    public string? CommitSha { get; set; }

    /// <summary>
    /// Gets or sets the executable rules.
    /// </summary>
    public IReadOnlyList<ReviewRuleDto> Rules { get; set; } = [];
}

/// <summary>
/// Represents one persisted review summary snapshot.
/// </summary>
public class ReviewSummarySnapshotDto
{
    /// <summary>
    /// Gets or sets total findings count.
    /// </summary>
    public int TotalFindings { get; set; }

    /// <summary>
    /// Gets or sets critical findings count.
    /// </summary>
    public int CriticalCount { get; set; }

    /// <summary>
    /// Gets or sets high findings count.
    /// </summary>
    public int HighCount { get; set; }

    /// <summary>
    /// Gets or sets medium findings count.
    /// </summary>
    public int MediumCount { get; set; }

    /// <summary>
    /// Gets or sets low findings count.
    /// </summary>
    public int LowCount { get; set; }

    /// <summary>
    /// Gets or sets info findings count.
    /// </summary>
    public int InfoCount { get; set; }
}