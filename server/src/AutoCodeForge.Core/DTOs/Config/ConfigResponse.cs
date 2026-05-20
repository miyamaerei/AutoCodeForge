using AutoCodeForge.Core.Enums;

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
    /// Gets or sets the configuration type.
    /// </summary>
    public ConfigType ConfigType { get; set; }

    /// <summary>
    /// Gets or sets the config key.
    /// </summary>
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the config value.
    /// </summary>
    public string ConfigValue { get; set; } = string.Empty;



    /// <summary>
    /// Gets or sets a value indicating whether the configuration is encrypted.
    /// </summary>
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the configuration is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the logical group.
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the creator.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the last updater.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Represents a configuration history response object.
/// </summary>
public class ConfigHistoryResponse
{
    /// <summary>
    /// Gets or sets the history record identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the configuration entry ID.
    /// </summary>
    public Guid ConfigId { get; set; }

    /// <summary>
    /// Gets or sets the configuration type.
    /// </summary>
    public ConfigType ConfigType { get; set; }

    /// <summary>
    /// Gets or sets the configuration key.
    /// </summary>
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the configuration value before the change.
    /// </summary>
    public string? PreviousValue { get; set; }

    /// <summary>
    /// Gets or sets the configuration value after the change.
    /// </summary>
    public string NewValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of operation performed.
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user who performed the change.
    /// </summary>
    public string ChangedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp of the change.
    /// </summary>
    public DateTime ChangedAt { get; set; }
}

/// <summary>
/// Represents a configuration template response.
/// </summary>
public class ConfigTemplateResponse
{
    /// <summary>
    /// Gets or sets the configuration type.
    /// </summary>
    public ConfigType ConfigType { get; set; }

    /// <summary>
    /// Gets or sets the configuration type name.
    /// </summary>
    public string TypeName => ConfigType.ToString();

    /// <summary>
    /// Gets or sets the default configuration key.
    /// </summary>
    public string DefaultKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default configuration value.
    /// </summary>
    public string DefaultValue { get; set; } = string.Empty;
}

/// <summary>
/// Represents the result of a configuration initialization operation.
/// </summary>
public class ConfigInitResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the number of configurations initialized.
    /// </summary>
    public int InitializedCount { get; set; }

    /// <summary>
    /// Gets or sets the message describing the result.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents the result of a configuration import operation.
/// </summary>
public class ConfigImportResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the number of configurations imported.
    /// </summary>
    public int ImportedCount { get; set; }

    /// <summary>
    /// Gets or sets the message describing the result.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}