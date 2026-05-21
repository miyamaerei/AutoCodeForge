using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for configuration history records.
/// </summary>
public class ConfigHistoryRepository : BaseRepository<ConfigHistoryEntity>
{
    private const int MaxHistoryRecords = 100;

    public ConfigHistoryRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser) { }

    /// <summary>
    /// Gets history records for a specific configuration entry.
    /// </summary>
    /// <param name="configId">The configuration entry ID.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paginated history records.</returns>
    public async Task<List<ConfigHistoryEntity>> GetByConfigIdAsync(
        Guid configId,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(h => h.ConfigId == configId)
            .OrderByDescending(h => h.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Gets history records for configurations of a specific type.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paginated history records.</returns>
    public async Task<List<ConfigHistoryEntity>> GetByConfigTypeAsync(
        ConfigType configType,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(h => h.ConfigType == configType)
            .OrderByDescending(h => h.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all history records for a specific user.
    /// </summary>
    /// <param name="changedBy">The user identifier.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paginated history records.</returns>
    public async Task<List<ConfigHistoryEntity>> GetByChangedByAsync(
        string changedBy,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(h => h.ChangedBy == changedBy)
            .OrderByDescending(h => h.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the latest history record for a configuration entry.
    /// </summary>
    /// <param name="configId">The configuration entry ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The latest history record, or null.</returns>
    public async Task<ConfigHistoryEntity?> GetLatestAsync(
        Guid configId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(h => h.ConfigId == configId)
            .OrderByDescending(h => h.CreatedAtUtc)
            .FirstAsync();
    }

    /// <summary>
    /// Gets all history records with pagination.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paginated history records.</returns>
    public async Task<List<ConfigHistoryEntity>> GetAllAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .OrderByDescending(h => h.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Cleans up old history records beyond the maximum allowed.
    /// </summary>
    /// <param name="configId">The configuration entry ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of deleted records.</returns>
    public async Task<int> CleanupOldRecordsAsync(
        Guid configId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var records = await Queryable
            .Where(h => h.ConfigId == configId)
            .OrderByDescending(h => h.CreatedAtUtc)
            .Skip(MaxHistoryRecords)
            .Select(h => h.Id)
            .ToListAsync();

        if (records.Count == 0)
        {
            return 0;
        }

        return await Db.Deleteable<ConfigHistoryEntity>()
            .In(records)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// Gets the count of history records for a configuration entry.
    /// </summary>
    /// <param name="configId">The configuration entry ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The count of history records.</returns>
    public async Task<int> GetCountAsync(
        Guid configId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(h => h.ConfigId == configId)
            .CountAsync();
    }
}