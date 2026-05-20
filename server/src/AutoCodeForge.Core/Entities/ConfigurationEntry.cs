using AutoCodeForge.Core.Entities.Base;
using AutoCodeForge.Core.Enums;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents a unified configuration entry stored in a single table.
/// Supports global and user-specific configurations with JSON value storage.
/// </summary>
[SugarTable("configuration_entries")]
public class ConfigurationEntry : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the configuration identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the configuration type.
    /// </summary>
    [SugarColumn(Length = 32, IsNullable = false)]
    public ConfigType ConfigType { get; set; }

    /// <summary>
    /// Gets or sets the configuration key.
    /// Follows naming convention: {config_type}.{module}.{name}
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = false)]
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the configuration value in JSON format.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
    public string ConfigValue { get; set; } = string.Empty;

    

    /// <summary>
    /// Gets or sets a value indicating whether the configuration is encrypted.
    /// </summary>
    [SugarColumn(IsNullable = false, DefaultValue = "false")]
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the configuration is enabled.
    /// </summary>
    [SugarColumn(IsNullable = false, DefaultValue = "true")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the description of the configuration.
    /// </summary>
    [SugarColumn(Length = 512, IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the logical group for organizing configurations.
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? Group { get; set; }

    /// <summary>
    /// Gets or sets the creator of the configuration.
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the last updater of the configuration.
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? UpdatedBy { get; set; }
}