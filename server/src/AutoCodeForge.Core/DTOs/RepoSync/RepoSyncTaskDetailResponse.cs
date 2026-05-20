namespace AutoCodeForge.Core.DTOs.RepoSync;

/// <summary>
/// Represents repo sync task detail with workspace info.
/// </summary>
public class RepoSyncTaskDetailResponse
{
    /// <summary>
    /// Gets or sets task identifier.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Gets or sets task status.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets workspace status.
    /// </summary>
    public string WorkspaceStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets effective sandbox path.
    /// </summary>
    public string EffectiveSandboxPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets branch.
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// Gets or sets commit sha.
    /// </summary>
    public string? CommitSha { get; set; }

    /// <summary>
    /// Gets or sets error message.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
