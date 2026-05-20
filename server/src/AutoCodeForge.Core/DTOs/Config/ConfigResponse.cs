namespace AutoCodeForge.Core.DTOs.Config;

/// <summary>
/// Represents a config response object.
/// </summary>
public class ConfigResponse
{
    /// <summary>
    /// Gets or sets the config identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the config key.
    /// </summary>
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the config value.
    /// </summary>
    public string ConfigValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
