using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents one repository sandbox workspace execution record.
/// </summary>
[SugarTable("RepoSandboxWorkspaces")]
public class RepoSandboxWorkspaceEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets workspace record identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets related task identifier.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Gets or sets related repository identifier.
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets workspace root path snapshot.
    /// </summary>
    [SugarColumn(Length = 1000, IsNullable = false)]
    public string WorkspaceRootPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets effective sandbox path used by execution.
    /// </summary>
    [SugarColumn(Length = 1200, IsNullable = false)]
    public string EffectiveSandboxPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets repository relative path under sandbox root.
    /// </summary>
    [SugarColumn(Length = 800, IsNullable = false)]
    public string RelativeRepoPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets sync branch.
    /// </summary>
    [SugarColumn(Length = 200, IsNullable = true)]
    public string? Branch { get; set; }

    /// <summary>
    /// Gets or sets synced commit sha.
    /// </summary>
    [SugarColumn(Length = 120, IsNullable = true)]
    public string? CommitSha { get; set; }

    /// <summary>
    /// Gets or sets workspace execution status.
    /// </summary>
    public RepoSandboxWorkspaceStatus Status { get; set; } = RepoSandboxWorkspaceStatus.Ready;

    /// <summary>
    /// Gets or sets optional error message.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets execution start timestamp in UTC.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? StartedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets execution finish timestamp in UTC.
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? FinishedAtUtc { get; set; }
}

/// <summary>
/// Defines workspace execution status values.
/// </summary>
public enum RepoSandboxWorkspaceStatus
{
    Ready = 0,
    Cloned = 1,
    Pulled = 2,
    Failed = 3,
    Cleaned = 4,
}
