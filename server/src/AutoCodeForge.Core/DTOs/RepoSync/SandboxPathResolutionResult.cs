namespace AutoCodeForge.Core.DTOs.RepoSync;

/// <summary>
/// Represents resolved sandbox paths for one repo sync task.
/// </summary>
public class SandboxPathResolutionResult
{
    /// <summary>
    /// Gets or sets normalized workspace root path.
    /// </summary>
    public string WorkspaceRootPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets effective task sandbox path.
    /// </summary>
    public string EffectiveSandboxPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets target repository path.
    /// </summary>
    public string RepositoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets repository relative path from workspace root.
    /// </summary>
    public string RelativeRepoPath { get; set; } = string.Empty;
}
