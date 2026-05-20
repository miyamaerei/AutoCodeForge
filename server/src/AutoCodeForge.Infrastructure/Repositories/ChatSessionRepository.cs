using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for chat sessions.
/// </summary>
public class ChatSessionRepository : BaseRepository<ChatSessionEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatSessionRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public ChatSessionRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets sessions sorted by latest activity then creation time.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The sessions list.</returns>
    public async Task<List<ChatSessionEntity>> GetLatestFirstAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .OrderByDescending(session => session.LastMessageAtUtc)
            .OrderByDescending(session => session.CreatedAtUtc)
            .ToListAsync();
    }
}
