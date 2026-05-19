using SqlSugar;

namespace __ProjectName__.Entities;

[SugarTable("GlobalConfigs")]
public class GlobalConfigEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Category { get; set; } = string.Empty;

    public string ConfigKey { get; set; } = string.Empty;

    public string ConfigValue { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[SugarTable("UserConfigs")]
public class UserConfigEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NtId { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string ConfigKey { get; set; } = string.Empty;

    public string ConfigValue { get; set; } = string.Empty;

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
