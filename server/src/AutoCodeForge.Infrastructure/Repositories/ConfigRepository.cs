using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for configuration entries with support for type-based and user-based filtering.
/// </summary>
public class ConfigRepository : BaseRepository<ConfigurationEntry>
{
    public ConfigRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser) { }

    /// <summary>
    /// Gets configuration entries by type.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <param name="ntId">Optional NTID for user-specific configs.</param>
    /// <param name="includeAllUsers">Whether to include configs from all users.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching configuration entries.</returns>
    public async Task<List<ConfigurationEntry>> GetByTypeAsync(
        ConfigType configType,
        string? ntId = null,
        bool includeAllUsers = false,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = Queryable.Where(c => c.ConfigType == configType && c.IsEnabled);

        if (!string.IsNullOrEmpty(ntId))
        {
            query = query.Where(c => c.NtId == ntId || c.NtId == null);
        }
        else if (!includeAllUsers)
        {
            query = query.Where(c => c.NtId == null);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets a single configuration entry by type and key.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <param name="configKey">The configuration key.</param>
    /// <param name="ntId">Optional NTID for user-specific configs.</param>
    /// <param name="includeAllUsers">Whether to include configs from all users.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching configuration entry, or null.</returns>
    public async Task<ConfigurationEntry?> GetByTypeAndKeyAsync(
        ConfigType configType,
        string configKey,
        string? ntId = null,
        bool includeAllUsers = false,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = Queryable
            .Where(c => c.ConfigType == configType && c.ConfigKey == configKey && c.IsEnabled);

        if (!string.IsNullOrEmpty(ntId))
        {
            query = query.Where(c => c.NtId == ntId || c.NtId == null);
        }
        else if (!includeAllUsers)
        {
            query = query.Where(c => c.NtId == null);
        }

        return await query.FirstAsync();
    }

    /// <summary>
    /// Gets all configuration entries for a specific user by NTID.
    /// </summary>
    /// <param name="ntId">The NTID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user's configuration entries.</returns>
    public async Task<List<ConfigurationEntry>> GetByNtIdAsync(
        string ntId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(c => c.NtId == ntId && c.IsEnabled)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a configuration entry exists by type and key.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <param name="configKey">The configuration key.</param>
    /// <param name="ntId">Optional NTID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the configuration exists.</returns>
    public async Task<bool> ExistsAsync(
        ConfigType configType,
        string configKey,
        string? ntId = null,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = Queryable.Where(c => c.ConfigType == configType && c.ConfigKey == configKey);

        if (!string.IsNullOrEmpty(ntId))
        {
            query = query.Where(c => c.NtId == ntId);
        }
        else
        {
            query = query.Where(c => c.NtId == null);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Deletes configuration entries by type.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <param name="ntId">Optional NTID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of deleted entries.</returns>
    public async Task<int> DeleteByTypeAsync(
        ConfigType configType,
        string? ntId = null,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = Db.Deleteable<ConfigurationEntry>()
            .Where(c => c.ConfigType == configType);

        if (!string.IsNullOrEmpty(ntId))
        {
            query = query.Where(c => c.NtId == ntId);
        }
        else
        {
            query = query.Where(c => c.NtId == null);
        }

        return await query.ExecuteCommandAsync();
    }

    /// <summary>
    /// Gets all configuration types with their entry counts.
    /// </summary>
    /// <param name="ntId">Optional NTID for filtering.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A dictionary of config types and their counts.</returns>
    public async Task<Dictionary<ConfigType, int>> GetTypeCountsAsync(
        string? ntId = null,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = Queryable.Where(c => c.IsEnabled);

        if (!string.IsNullOrEmpty(ntId))
        {
            query = query.Where(c => c.NtId == ntId || c.NtId == null);
        }

        var list = await query.ToListAsync();
        return list.GroupBy(c => c.ConfigType)
                   .ToDictionary(g => g.Key, g => g.Count());
    }
}