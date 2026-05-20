using AutoCodeForge.Core.Entities.Base;
using AutoCodeForge.Core.Enums;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents a historical record of configuration changes.
/// </summary>
[SugarTable("configuration_history")]
public class ConfigHistoryEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets the history record identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the configuration entry that was changed.
    /// </summary>
    [SugarColumn(IsNullable = false)]
    public Guid ConfigId { get; set; }

    /// <summary>
    /// Gets or sets the configuration type.
    /// </summary>
    [SugarColumn(Length = 32, IsNullable = false)]
    public ConfigType ConfigType { get; set; }

    /// <summary>
    /// Gets or sets the configuration key.
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = false)]
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the configuration value before the change.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? PreviousValue { get; set; }

    /// <summary>
    /// Gets or sets the configuration value after the change.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
    public string NewValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of operation performed (Created/Updated/Deleted).
    /// </summary>
    [SugarColumn(Length = 16, IsNullable = false)]
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user who performed the change.
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = false)]
    public string ChangedBy { get; set; } = string.Empty;
}