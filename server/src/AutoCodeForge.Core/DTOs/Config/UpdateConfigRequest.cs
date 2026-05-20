using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Config;

/// <summary>
/// Represents a config update request for both global and user configs.
/// </summary>
public class UpdateConfigRequest
{
    /// <summary>
    /// Gets or sets the config key.
    /// </summary>
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the config value.
    /// </summary>
    [Required]
    public string ConfigValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional description (only for global config).
    /// </summary>
    [StringLength(300)]
    public string? Description { get; set; }
}
