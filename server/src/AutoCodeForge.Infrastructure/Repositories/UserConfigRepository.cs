using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for user configuration.
/// </summary>
public class UserConfigRepository : BaseRepository<UserConfigEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserConfigRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public UserConfigRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets one user config by key for current user.
    /// </summary>
    /// <param name="configKey">The config key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The config or null.</returns>
    public async Task<UserConfigEntity?> GetByKeyAsync(string configKey, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable.FirstAsync(config => config.ConfigKey == configKey);
    }
}
