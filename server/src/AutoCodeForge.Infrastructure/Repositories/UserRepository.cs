using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides user-specific queries.
/// </summary>
public class UserRepository : BaseRepository<UserEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public UserRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets one user by ntid.
    /// </summary>
    /// <param name="ntId">The user ntid.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching user or null.</returns>
    public async Task<UserEntity?> GetByNtIdAsync(string ntId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Db.Queryable<UserEntity>()
            .Where(user => user.NtId == ntId && !user.IsDeleted)
            .FirstAsync();
    }
}
