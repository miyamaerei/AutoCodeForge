using AutoCodeForge.Core.Entities.Base;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Core.Interfaces;

/// <summary>
/// Defines reusable CRUD operations for user-owned entities.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IBaseRepository<T>
    where T : AuditableEntity, new()
{
    /// <summary>
    /// Gets an entity by identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="includeAllUsers">Whether to bypass the current-user filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching entity, or <see langword="null"/> when not found.</returns>
    Task<T?> GetByIdAsync(Guid id, bool includeAllUsers = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities visible to the caller.
    /// </summary>
    /// <param name="includeAllUsers">Whether to bypass the current-user filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching entities.</returns>
    Task<List<T>> GetAllAsync(bool includeAllUsers = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paged list of entities visible to the caller.
    /// </summary>
    /// <param name="page">The requested page number.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="includeAllUsers">Whether to bypass the current-user filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result.</returns>
    Task<PagedResult<T>> GetPagedAsync(
        int page,
        int pageSize,
        bool includeAllUsers = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new entity.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created entity.</returns>
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes an entity by identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="includeAllUsers">Whether to bypass the current-user filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SoftDeleteAsync(Guid id, bool includeAllUsers = false, CancellationToken cancellationToken = default);
}