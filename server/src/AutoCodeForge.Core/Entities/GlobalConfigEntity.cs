using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents a shared global key-value configuration.
/// </summary>
[SugarTable("GlobalConfigs")]
public class GlobalConfigEntity : AuditableEntity
{
    /// <summary>
    /// Gets or sets the config identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets config key.
    /// </summary>
    [SugarColumn(Length = 120, IsNullable = false)]
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets config value.
    /// </summary>
    [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
    public string ConfigValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional description.
    /// </summary>
    [SugarColumn(Length = 300, IsNullable = true)]
    public string? Description { get; set; }
}
