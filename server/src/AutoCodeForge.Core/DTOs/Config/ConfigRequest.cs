using AutoCodeForge.Core.Enums;

namespace AutoCodeForge.Core.DTOs.Config;

/// <summary>
/// Represents a request to create or update a configuration entry.
/// </summary>
public class ConfigRequest
{
    /// <summary>
    /// Gets or sets the configuration key.
    /// </summary>
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the configuration value.
    /// </summary>
    public string ConfigValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the configuration should be encrypted.
    /// </summary>
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Gets or sets the description of the configuration.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the logical group for the configuration.
    /// </summary>
    public string? Group { get; set; }
}

/// <summary>
/// Represents a request to batch update configurations.
/// </summary>
public class BatchConfigRequest
{
    /// <summary>
    /// Gets or sets the configurations to update.
    /// </summary>
    public List<ConfigRequestItem> Configs { get; set; } = new List<ConfigRequestItem>();

    /// <summary>
    /// Gets or sets a value indicating whether to overwrite existing configurations.
    /// </summary>
    public bool OverwriteExisting { get; set; }
}

/// <summary>
/// Represents a single configuration item in a batch request.
/// </summary>
public class ConfigRequestItem
{
    /// <summary>
    /// Gets or sets the configuration type.
    /// </summary>
    public ConfigType ConfigType { get; set; }

    /// <summary>
    /// Gets or sets the configuration key.
    /// </summary>
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the configuration value.
    /// </summary>
    public string ConfigValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the configuration should be encrypted.
    /// </summary>
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the configuration is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the description of the configuration.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the logical group for the configuration.
    /// </summary>
    public string? Group { get; set; }
}

/// <summary>
/// Represents a request to initialize configurations.
/// </summary>
public class InitConfigRequest
{
    /// <summary>
    /// Gets or sets the configuration type to initialize.
    /// If not specified, all user configurations will be initialized.
    /// </summary>
    public ConfigType? ConfigType { get; set; }

    /// <summary>
    /// Gets or sets the NTID for which to initialize configurations.
    /// If not specified, uses the current user.
    /// </summary>
    public string? NtId { get; set; }
}

/// <summary>
/// Represents a request to reset configurations to defaults.
/// </summary>
public class ResetConfigRequest
{
    /// <summary>
    /// Gets or sets the configuration type to reset.
    /// </summary>
    public ConfigType ConfigType { get; set; }

    /// <summary>
    /// Gets or sets the NTID for which to reset configurations.
    /// If not specified, uses the current user.
    /// </summary>
    public string? NtId { get; set; }
 }

/// <summary>
/// Represents a request to import configurations.
/// </summary>
public class ImportConfigRequest
{
    /// <summary>
    /// Gets or sets the JSON data containing configurations to import.
    /// </summary>
    public string JsonData { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to overwrite existing configurations.
    /// </summary>
    public bool OverwriteExisting { get; set; }
}