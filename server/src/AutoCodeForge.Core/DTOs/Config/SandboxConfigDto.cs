using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Config;

/// <summary>
/// Represents per-user sandbox configuration.
/// </summary>
public class SandboxConfigDto
{
    /// <summary>
    /// Gets or sets workspace root absolute path.
    /// </summary>
    [Required]
    [MaxLength(800)]
    public string WorkspaceRootPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets allowed write roots.
    /// </summary>
    public List<string> AllowedWritePaths { get; set; } = [];

    /// <summary>
    /// Gets or sets task timeout seconds.
    /// </summary>
    [Range(30, 7200)]
    public int TimeoutSeconds { get; set; } = 600;

    /// <summary>
    /// Gets or sets a value indicating whether user isolation is enabled.
    /// </summary>
    public bool UserIsolationEnabled { get; set; } = true;
}
