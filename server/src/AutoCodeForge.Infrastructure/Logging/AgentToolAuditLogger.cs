using System.Security.Cryptography;
using System.Text;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Infrastructure.Logging;

/// <summary>
/// Persists agent tool invocation audit records.
/// </summary>
public class AgentToolAuditLogger
{
    private readonly AgentToolInvocationRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AgentToolAuditLogger> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentToolAuditLogger"/> class.
    /// </summary>
    /// <param name="repository">Invocation repository.</param>
    /// <param name="currentUser">Current user provider.</param>
    /// <param name="logger">Logger instance.</param>
    public AgentToolAuditLogger(
        AgentToolInvocationRepository repository,
        ICurrentUser currentUser,
        ILogger<AgentToolAuditLogger> logger)
    {
        _repository = repository;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Logs one tool invocation.
    /// </summary>
    /// <param name="toolName">Tool name.</param>
    /// <param name="input">Serialized or plain input summary.</param>
    /// <param name="output">Serialized or plain output summary.</param>
    /// <param name="status">Invocation status.</param>
    /// <param name="latencyMs">Latency in milliseconds.</param>
    /// <param name="sessionId">Optional session identifier.</param>
    /// <param name="taskId">Optional task identifier.</param>
    /// <param name="repositoryId">Optional repository identifier.</param>
    /// <param name="errorCode">Optional error code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task LogAsync(
        string toolName,
        string? input,
        string? output,
        string status,
        long latencyMs,
        Guid? sessionId,
        Guid? taskId,
        Guid? repositoryId,
        string? errorCode,
        CancellationToken cancellationToken = default)
    {
        var entry = new AgentToolInvocationEntity
        {
            Id = Guid.NewGuid(),
            NtId = _currentUser.GetCurrentNtId(),
            SessionId = sessionId,
            TaskId = taskId,
            RepositoryId = repositoryId,
            ToolName = toolName,
            InputDigest = ToDigest(input),
            OutputDigest = ToDigest(output),
            Status = status,
            ErrorCode = errorCode,
            LatencyMs = latencyMs,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        try
        {
            await _repository.CreateAsync(entry, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist tool audit log for {ToolName}", toolName);
        }
    }

    private static string? ToDigest(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
