namespace AutoCodeForge.Core.DTOs.Review;

/// <summary>
/// Represents the aggregated status of one review task.
/// </summary>
public class ReviewTaskSummaryDto
{
    /// <summary>
    /// Gets or sets the review task identifier.
    /// </summary>
    public Guid ReviewTaskId { get; set; }

    /// <summary>
    /// Gets or sets the linked task identifier.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Gets or sets the repository identifier.
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets the current status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

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

    /// <summary>
    /// Gets or sets execution start timestamp in UTC.
    /// </summary>
    public DateTime? StartedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets execution completion timestamp in UTC.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the optional error message.
    /// </summary>
    public string? ErrorMessage { get; set; }
}