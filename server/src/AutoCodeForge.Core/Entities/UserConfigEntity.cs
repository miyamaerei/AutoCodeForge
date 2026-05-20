using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents a user scoped key-value configuration.
/// </summary>
[SugarTable("UserConfigs")]
public class UserConfigEntity : UserOwnedEntity
{
    /// <summary>
    /// Gets or sets user config identifier.
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
}
