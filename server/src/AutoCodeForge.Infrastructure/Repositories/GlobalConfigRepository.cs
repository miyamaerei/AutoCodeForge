using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for global configuration.
/// </summary>
public class GlobalConfigRepository : BaseRepository<GlobalConfigEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalConfigRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public GlobalConfigRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets a config by key.
    /// </summary>
    /// <param name="configKey">The config key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The config or null.</returns>
    public async Task<GlobalConfigEntity?> GetByKeyAsync(string configKey, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Db.Queryable<GlobalConfigEntity>()
            .Where(config => config.ConfigKey == configKey)
            .FirstAsync();
    }
}
