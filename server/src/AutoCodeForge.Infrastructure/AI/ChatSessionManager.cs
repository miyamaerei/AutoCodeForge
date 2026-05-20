using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Infrastructure.AI;

/// <summary>
/// Manages chat session message history and token-aware trimming.
/// </summary>
public class ChatSessionManager
{
    private readonly ChatMessageRepository _chatMessageRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatSessionManager"/> class.
    /// </summary>
    /// <param name="chatMessageRepository">The chat message repository.</param>
    public ChatSessionManager(ChatMessageRepository chatMessageRepository)
    {
        _chatMessageRepository = chatMessageRepository;
    }

    /// <summary>
    /// Gets recent message history for one session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="maxMessages">The max message count.</param>
    /// <param name="maxTokens">The max estimated token count.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Trimmed message history.</returns>
    public async Task<List<ChatMessageEntity>> GetHistoryAsync(
        Guid sessionId,
        int maxMessages = 40,
        int maxTokens = 3000,
        CancellationToken cancellationToken = default)
    {
        var history = await _chatMessageRepository.GetRecentBySessionIdAsync(sessionId, maxMessages, cancellationToken);
        return TrimByTokenLimit(history, maxTokens);
    }

    /// <summary>
    /// Adds one message into the session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="type">The message type.</param>
    /// <param name="content">The message content.</param>
    /// <param name="modelName">The optional model name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created message entity.</returns>
    public async Task<ChatMessageEntity> AddMessageAsync(
        Guid sessionId,
        MessageType type,
        string content,
        string? modelName = null,
        CancellationToken cancellationToken = default)
    {
        var message = new ChatMessageEntity
        {
            Id = Guid.NewGuid(),
            ChatSessionId = sessionId,
            Type = type,
            Content = content.Trim(),
            ModelName = modelName,
            CreatedAtUtc = TimeHelper.UtcNow(),
            UpdatedAtUtc = TimeHelper.UtcNow(),
        };

        return await _chatMessageRepository.CreateAsync(message, cancellationToken);
    }

    private static List<ChatMessageEntity> TrimByTokenLimit(List<ChatMessageEntity> messages, int maxTokens)
    {
        if (messages.Count == 0)
        {
            return messages;
        }

        var total = EstimateTokens(messages);
        if (total <= maxTokens)
        {
            return messages;
        }

        var trimmed = new List<ChatMessageEntity>(messages);
        while (trimmed.Count > 1 && EstimateTokens(trimmed) > maxTokens)
        {
            trimmed.RemoveAt(0);
        }

        return trimmed;
    }

    private static int EstimateTokens(IEnumerable<ChatMessageEntity> messages)
    {
        return messages.Sum(message => Math.Max(1, (message.Content?.Length ?? 0) / 4));
    }
}
