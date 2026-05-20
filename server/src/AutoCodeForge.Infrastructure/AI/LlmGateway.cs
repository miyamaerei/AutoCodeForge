using AutoCodeForge.Core.DTOs.AI;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Infrastructure.AI;

/// <summary>
/// Provides a reusable LLM gateway with model fallback, retries, and a simple circuit breaker.
/// </summary>
public class LlmGateway : ILlmGateway
{
    private const int MaxRetryCount = 3;
    private const int CircuitFailureThreshold = 3;
    private static readonly TimeSpan CircuitBreakDuration = TimeSpan.FromSeconds(30);
    private readonly object _gate = new();
    private readonly LLMModelConfigRepository _configRepository;
    private readonly ILogger<LlmGateway> _logger;
    private int _consecutiveFailures;
    private DateTime? _circuitOpenedAtUtc;

    /// <summary>
    /// Initializes a new instance of the <see cref="LlmGateway"/> class.
    /// </summary>
    /// <param name="configRepository">The model config repository.</param>
    /// <param name="logger">The logger.</param>
    public LlmGateway(LLMModelConfigRepository configRepository, ILogger<LlmGateway> logger)
    {
        _configRepository = configRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<LlmResponse> ChatAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        EnsureRequest(request);
        EnsureCircuitClosed();
        var model = await SelectModelAsync(request.PreferredModelId, cancellationToken);

        try
        {
            var response = await ExecuteWithRetryAsync(
                () => CallModelAsync(model, request, cancellationToken),
                cancellationToken);
            RecordSuccess();
            return response;
        }
        catch
        {
            RecordFailure();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<LlmResponse> ChatWithToolsAsync(
        LlmRequest request,
        IEnumerable<IAgentTool> tools,
        CancellationToken cancellationToken = default)
    {
        var response = await ChatAsync(request, cancellationToken);

        var userInput = request.Messages.LastOrDefault(message =>
            string.Equals(message.Role, "user", StringComparison.OrdinalIgnoreCase))?.Content ?? string.Empty;

        var matchedTool = tools.FirstOrDefault(tool =>
            userInput.Contains(tool.Name, StringComparison.OrdinalIgnoreCase));

        if (matchedTool is null)
        {
            return response;
        }

        var toolResult = await matchedTool.ExecuteAsync(
            new Dictionary<string, string>
            {
                ["query"] = userInput,
            },
            cancellationToken);

        response.Content = $"{response.Content}\n\n[Tool:{matchedTool.Name}] {toolResult}";
        return response;
    }

    private static void EnsureRequest(LlmRequest request)
    {
        if (request.Messages.Count == 0)
        {
            throw new ValidationException("At least one message is required");
        }
    }

    private async Task<LLMModelConfigEntity?> SelectModelAsync(Guid? preferredModelId, CancellationToken cancellationToken)
    {
        if (preferredModelId.HasValue)
        {
            var preferred = await _configRepository.GetByIdAsync(preferredModelId.Value, false, cancellationToken);
            if (preferred is not null)
            {
                return preferred;
            }
        }

        var models = await _configRepository.GetAllAsync(false, cancellationToken);
        return models.OrderBy(model => model.CreatedAtUtc).FirstOrDefault();
    }

    private async Task<LlmResponse> ExecuteWithRetryAsync(
        Func<Task<LlmResponse>> operation,
        CancellationToken cancellationToken)
    {
        Exception? lastException = null;
        for (var attempt = 1; attempt <= MaxRetryCount; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "LLM invocation attempt {Attempt}/{Max} failed", attempt, MaxRetryCount);

                if (attempt == MaxRetryCount)
                {
                    break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
            }
        }

        throw new CustomException($"LLM invocation failed after retries: {lastException?.Message}", 502);
    }

    private Task<LlmResponse> CallModelAsync(
        LLMModelConfigEntity? model,
        LlmRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var lastUserMessage = request.Messages.LastOrDefault(message =>
            string.Equals(message.Role, "user", StringComparison.OrdinalIgnoreCase))?.Content?.Trim();

        if (string.IsNullOrWhiteSpace(lastUserMessage))
        {
            throw new ValidationException("User message is required");
        }

        _logger.LogInformation(
            "Mock LLM call model={ModelName}, request={Request}",
            model?.ModelName ?? "default-model",
            JsonHelper.Serialize(request));

        var reply = $"[AutoCodeForge AI] {lastUserMessage}";
        return Task.FromResult(new LlmResponse
        {
            Content = reply,
            ModelName = model?.ModelName ?? "default-model",
            CompletedAtUtc = TimeHelper.UtcNow(),
            TotalTokens = Math.Max(1, lastUserMessage.Length / 4),
        });
    }

    private void EnsureCircuitClosed()
    {
        lock (_gate)
        {
            if (_circuitOpenedAtUtc is null)
            {
                return;
            }

            var elapsed = TimeHelper.UtcNow() - _circuitOpenedAtUtc.Value;
            if (elapsed >= CircuitBreakDuration)
            {
                _circuitOpenedAtUtc = null;
                _consecutiveFailures = 0;
                return;
            }

            throw new CustomException("LLM gateway circuit is open, please retry later", 503);
        }
    }

    private void RecordSuccess()
    {
        lock (_gate)
        {
            _consecutiveFailures = 0;
            _circuitOpenedAtUtc = null;
        }
    }

    private void RecordFailure()
    {
        lock (_gate)
        {
            _consecutiveFailures++;
            if (_consecutiveFailures >= CircuitFailureThreshold)
            {
                _circuitOpenedAtUtc = TimeHelper.UtcNow();
            }
        }
    }
}
