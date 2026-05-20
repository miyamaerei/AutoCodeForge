using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents an authenticated AutoCodeForge user.
/// </summary>
[SugarTable("Users")]
public class UserEntity : AuditableEntity
{
    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique NT account id.
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = false)]
    public string NtId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = false)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    [SugarColumn(Length = 256, IsNullable = true)]
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the password hash.
    /// </summary>
    [SugarColumn(Length = 512, IsNullable = false)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user is soft deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
}
