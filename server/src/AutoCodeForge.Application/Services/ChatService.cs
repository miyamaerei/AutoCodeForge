using AutoCodeForge.Core.DTOs.Chat;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.AI;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides chat session management and message send operations.
/// </summary>
public class ChatService
{
    private readonly ChatSessionRepository _chatSessionRepository;
    private readonly ChatMessageRepository _chatMessageRepository;
    private readonly ChatSessionManager _chatSessionManager;
    private readonly AgentMatcher _agentMatcher;
    private readonly AgentExecutor _agentExecutor;
    private readonly ChatDefaultsProvisioningService? _chatDefaultsProvisioningService;
    private readonly ILogger<ChatService>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatService"/> class.
    /// </summary>
    /// <param name="chatSessionRepository">The chat session repository.</param>
    /// <param name="chatMessageRepository">The chat message repository.</param>
    /// <param name="chatSessionManager">The chat session manager.</param>
    /// <param name="agentMatcher">The agent matcher.</param>
    /// <param name="agentExecutor">The agent executor.</param>
    public ChatService(
        ChatSessionRepository chatSessionRepository,
        ChatMessageRepository chatMessageRepository,
        ChatSessionManager chatSessionManager,
        AgentMatcher agentMatcher,
        AgentExecutor agentExecutor,
        ChatDefaultsProvisioningService? chatDefaultsProvisioningService = null,
        ILogger<ChatService>? logger = null)
    {
        _chatSessionRepository = chatSessionRepository;
        _chatMessageRepository = chatMessageRepository;
        _chatSessionManager = chatSessionManager;
        _agentMatcher = agentMatcher;
        _agentExecutor = agentExecutor;
        _chatDefaultsProvisioningService = chatDefaultsProvisioningService;
        _logger = logger;
    }

    /// <summary>
    /// Creates one chat session.
    /// </summary>
    /// <param name="request">The create request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created session.</returns>
    public async Task<ChatSessionResponse> CreateSessionAsync(
        CreateSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = new ChatSessionEntity
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            AgentId = request.AgentId,
            LastMessageAtUtc = null,
            CreatedAtUtc = TimeHelper.UtcNow(),
            UpdatedAtUtc = TimeHelper.UtcNow(),
        };

        var created = await _chatSessionRepository.CreateAsync(session, cancellationToken);
        return ToSessionResponse(created);
    }

    /// <summary>
    /// Gets one chat session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The session response.</returns>
    public async Task<ChatSessionResponse> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _chatSessionRepository.GetByIdAsync(sessionId, false, cancellationToken)
            ?? throw new NotFoundException("Chat session not found");

        return ToSessionResponse(session);
    }

    /// <summary>
    /// Gets paged chat sessions.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged sessions.</returns>
    public async Task<PagedResult<ChatSessionResponse>> GetSessionsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var paged = await _chatSessionRepository.GetPagedAsync(page, pageSize, false, cancellationToken);
        return new PagedResult<ChatSessionResponse>(
            paged.Items.Select(ToSessionResponse).ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize);
    }

    /// <summary>
    /// Gets all messages from one session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Session messages.</returns>
    public async Task<List<ChatMessageResponse>> GetMessagesAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _chatSessionRepository.GetByIdAsync(sessionId, false, cancellationToken)
            ?? throw new NotFoundException("Chat session not found");

        _ = session;
        var messages = await _chatMessageRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        return messages.Select(ToMessageResponse).ToList();
    }

    /// <summary>
    /// Sends one user message and returns both user and assistant records.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="request">The send message request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Send message result.</returns>
    public async Task<SendMessageResponse> SendMessageAsync(
        Guid sessionId,
        SendMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _chatSessionRepository.GetByIdAsync(sessionId, false, cancellationToken)
            ?? throw new NotFoundException("Chat session not found");

        var messageText = request.Message.Trim();
        if (string.IsNullOrWhiteSpace(messageText))
        {
            throw new ValidationException("Message is required");
        }

        if (_chatDefaultsProvisioningService is not null && !string.IsNullOrWhiteSpace(session.NtId))
        {
            await _chatDefaultsProvisioningService.EnsureDefaultsForNtIdAsync(session.NtId, cancellationToken);
        }

        AgentEntity? matchedAgent;
        try
        {
            matchedAgent = await _agentMatcher.MatchAgentAsync(
                messageText,
                request.AgentId ?? session.AgentId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Agent matching failed for session {SessionId}; falling back to generic chat", sessionId);
            matchedAgent = null;
        }

        var history = await _chatSessionManager.GetHistoryAsync(sessionId, cancellationToken: cancellationToken);
        var userMessage = await _chatSessionManager.AddMessageAsync(
            sessionId,
            MessageType.User,
            messageText,
            null,
            cancellationToken);

        AgentEntity? effectiveAgent = matchedAgent;
        string assistantText;

        if (matchedAgent is null)
        {
            assistantText = await _agentExecutor.ExecuteGenericAsync(messageText, history, cancellationToken);
        }
        else
        {
            try
            {
                assistantText = await _agentExecutor.ExecuteAsync(matchedAgent, messageText, history, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Agent execution failed for session {SessionId}; falling back to generic chat", sessionId);
                effectiveAgent = null;
                assistantText = await _agentExecutor.ExecuteGenericAsync(messageText, history, cancellationToken);
            }
        }

        var assistantMessage = await _chatSessionManager.AddMessageAsync(
            sessionId,
            MessageType.Assistant,
            assistantText,
            effectiveAgent?.Name,
            cancellationToken);

        session.LastMessageAtUtc = TimeHelper.UtcNow();
        if (effectiveAgent is not null)
        {
            session.AgentId = effectiveAgent.Id;
        }
        await _chatSessionRepository.UpdateAsync(session, cancellationToken);

        return new SendMessageResponse
        {
            SessionId = sessionId,
            AgentId = effectiveAgent?.Id,
            UserMessage = ToMessageResponse(userMessage),
            AssistantMessage = ToMessageResponse(assistantMessage),
        };
    }

    /// <summary>
    /// Soft deletes one chat session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task DeleteSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        await _chatSessionRepository.SoftDeleteAsync(sessionId, false, cancellationToken);
    }

    private static ChatSessionResponse ToSessionResponse(ChatSessionEntity entity)
    {
        return new ChatSessionResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            AgentId = entity.AgentId,
            LastMessageAtUtc = entity.LastMessageAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }

    private static ChatMessageResponse ToMessageResponse(ChatMessageEntity entity)
    {
        return new ChatMessageResponse
        {
            Id = entity.Id,
            ChatSessionId = entity.ChatSessionId,
            Type = entity.Type.ToString(),
            Content = entity.Content,
            ModelName = entity.ModelName,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }
}
