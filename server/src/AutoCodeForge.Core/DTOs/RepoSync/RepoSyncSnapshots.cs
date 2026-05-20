using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Core.DTOs.RepoSync;

/// <summary>
/// Represents sandbox snapshot stored on task creation.
/// </summary>
public class SandboxSnapshot
{
    /// <summary>
    /// Gets or sets workspace root path.
    /// </summary>
    public string WorkspaceRootPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets allowed write paths.
    /// </summary>
    public List<string> AllowedWritePaths { get; set; } = [];

    /// <summary>
    /// Gets or sets timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether user isolation is enabled.
    /// </summary>
    public bool UserIsolationEnabled { get; set; } = true;
}

/// <summary>
/// Represents repository snapshot stored on task creation.
/// </summary>
public class RepositorySnapshot
{
    /// <summary>
    /// Gets or sets repository identifier.
    /// </summary>
    public Guid RepositoryId { get; set; }

    /// <summary>
    /// Gets or sets repository url.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets provider type.
    /// </summary>
    public GitProvider Provider { get; set; }

    /// <summary>
    /// Gets or sets encrypted token snapshot.
    /// </summary>
    public string EncryptedToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets requested branch.
    /// </summary>
    public string Branch { get; set; } = "main";

    /// <summary>
    /// Gets or sets normalized owner name.
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets normalized repository name.
    /// </summary>
    public string RepoName { get; set; } = string.Empty;
}
