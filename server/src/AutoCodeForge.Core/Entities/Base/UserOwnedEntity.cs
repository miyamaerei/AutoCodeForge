namespace AutoCodeForge.Core.Entities.Base;

/// <summary>
/// Extends an auditable entity with user ownership and soft-delete state.
/// </summary>
public abstract class UserOwnedEntity : AuditableEntity
{
    /// <summary>
    /// Gets or sets the owner NtId used for user isolation.
    /// </summary>
    public string NtId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the entity is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
}