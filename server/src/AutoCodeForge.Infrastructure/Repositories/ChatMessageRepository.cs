using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for chat messages.
/// </summary>
public class ChatMessageRepository : BaseRepository<ChatMessageEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatMessageRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public ChatMessageRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets all messages for one session in creation order.
    /// </summary>
    /// <param name="chatSessionId">The chat session identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Message list.</returns>
    public async Task<List<ChatMessageEntity>> GetBySessionIdAsync(Guid chatSessionId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(message => message.ChatSessionId == chatSessionId)
            .OrderBy(message => message.CreatedAtUtc)
            .ToListAsync();
    }

    /// <summary>
    /// Gets most recent messages for one session.
    /// </summary>
    /// <param name="chatSessionId">The chat session identifier.</param>
    /// <param name="take">The maximum message count.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Message list.</returns>
    public async Task<List<ChatMessageEntity>> GetRecentBySessionIdAsync(
        Guid chatSessionId,
        int take,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var normalizedTake = Math.Max(1, take);
        var messages = await Queryable
            .Where(message => message.ChatSessionId == chatSessionId)
            .OrderByDescending(message => message.CreatedAtUtc)
            .Take(normalizedTake)
            .ToListAsync();

        messages.Reverse();
        return messages;
    }
}
