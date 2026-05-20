using AutoCodeForge.Core.Entities.Base;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories.Base;

/// <summary>
/// Provides reusable CRUD, pagination, soft-delete, and user-isolation behavior.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class BaseRepository<T> : IBaseRepository<T>
    where T : AuditableEntity, new()
{
    protected readonly ISqlSugarClient Db;
    protected readonly string? CurrentUserNtId;
    private static readonly bool SupportsNtId = typeof(T).GetProperty(nameof(UserOwnedEntity.NtId)) is not null;
    private static readonly bool SupportsSoftDelete = typeof(T).GetProperty(nameof(UserOwnedEntity.IsDeleted)) is not null;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseRepository{T}"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public BaseRepository(ISqlSugarClient db, ICurrentUser currentUser)
    {
        Db = db;
        CurrentUserNtId = currentUser.GetCurrentNtId();
    }

    protected ISugarQueryable<T> Queryable
    {
        get
        {
            var query = Db.Queryable<T>();
            if (SupportsSoftDelete)
            {
                query = query.Where("IsDeleted = @isDeleted", new { isDeleted = false });
            }

            if (SupportsNtId && !string.IsNullOrWhiteSpace(CurrentUserNtId))
            {
                query = query.Where("NtId = @ntId", new { ntId = CurrentUserNtId });
            }

            return query;
        }
    }

    protected ISugarQueryable<T> QueryableWithoutNtIdFilter
    {
        get
        {
            var query = Db.Queryable<T>();
            if (SupportsSoftDelete)
            {
                query = query.Where("IsDeleted = @isDeleted", new { isDeleted = false });
            }

            return query;
        }
    }

    /// <summary>
    /// Gets an entity by identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="includeAllUsers">Whether to bypass the current-user filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching entity, or <see langword="null"/>.</returns>
    public virtual async Task<T?> GetByIdAsync(
        Guid id,
        bool includeAllUsers = false,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = includeAllUsers ? QueryableWithoutNtIdFilter : Queryable;
        return await query.FirstAsync(entity => entity.Id == id);
    }

    /// <summary>
    /// Gets all entities visible to the caller.
    /// </summary>
    /// <param name="includeAllUsers">Whether to bypass the current-user filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching entities.</returns>
    public virtual async Task<List<T>> GetAllAsync(
        bool includeAllUsers = false,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = includeAllUsers ? QueryableWithoutNtIdFilter : Queryable;
        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets a paged list of entities visible to the caller.
    /// </summary>
    /// <param name="page">The requested page number.</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <param name="includeAllUsers">Whether to bypass the current-user filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged result.</returns>
    public virtual async Task<PagedResult<T>> GetPagedAsync(
        int page,
        int pageSize,
        bool includeAllUsers = false,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = includeAllUsers ? QueryableWithoutNtIdFilter : Queryable;
        var normalized = PaginationHelper.Normalize(page, pageSize);
        RefAsync<int> totalCount = 0;
        var items = await query
            .OrderByDescending(entity => entity.CreatedAtUtc)
            .ToPageListAsync(normalized.Page, normalized.PageSize, totalCount);

        return PaginationHelper.ToPagedResult(items, totalCount, normalized.Page, normalized.PageSize);
    }

    /// <summary>
    /// Creates a new entity and stamps audit metadata.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created entity.</returns>
    public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        entity.CreatedAtUtc = DateTime.UtcNow;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        if (SupportsNtId && !string.IsNullOrWhiteSpace(CurrentUserNtId))
        {
            var ntIdProperty = typeof(T).GetProperty(nameof(UserOwnedEntity.NtId));
            var existingNtId = ntIdProperty?.GetValue(entity) as string;
            if (string.IsNullOrWhiteSpace(existingNtId))
            {
                ntIdProperty?.SetValue(entity, CurrentUserNtId);
            }
        }

        await Db.Insertable(entity).ExecuteCommandAsync();
        return entity;
    }

    /// <summary>
    /// Updates an existing entity and refreshes the audit timestamp.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await Db.Updateable(entity).ExecuteCommandAsync();
    }

    /// <summary>
    /// Soft-deletes an entity by identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="includeAllUsers">Whether to bypass the current-user filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public virtual async Task SoftDeleteAsync(
        Guid id,
        bool includeAllUsers = false,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var entity = await GetByIdAsync(id, includeAllUsers);
        if (entity is null)
        {
            return;
        }

        if (SupportsSoftDelete)
        {
            typeof(T).GetProperty(nameof(UserOwnedEntity.IsDeleted))?.SetValue(entity, true);
            entity.UpdatedAtUtc = DateTime.UtcNow;
            await Db.Updateable(entity).ExecuteCommandAsync();
            return;
        }

        await Db.Deleteable<T>().In(id).ExecuteCommandAsync();
    }
}