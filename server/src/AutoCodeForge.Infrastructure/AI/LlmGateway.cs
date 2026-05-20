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
        // Validate tools collection
        var toolList = tools.ToList();
        if (!toolList.Any())
        {
            _logger.LogWarning("No tools available for agent execution");
            return await ChatAsync(request, cancellationToken);
        }

        // Build tool registry for lookup
        var toolRegistry = toolList.ToDictionary(
            t => t.Name,
            t => t,
            StringComparer.OrdinalIgnoreCase);

        // Get last user message for tool selection
        var userInput = request.Messages.LastOrDefault(message =>
            string.Equals(message.Role, "user", StringComparison.OrdinalIgnoreCase))?.Content ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userInput))
        {
            return await ChatAsync(request, cancellationToken);
        }

        // Attempt to match and execute a tool
        var matchedTool = MatchToolByName(userInput, toolRegistry);
        if (matchedTool is null)
        {
            _logger.LogDebug("No matching tool found for user input: {UserInput}", userInput);
            return await ChatAsync(request, cancellationToken);
        }

        _logger.LogInformation("Matched tool '{ToolName}' for user input", matchedTool.Name);

        try
        {
            // Execute the matched tool with structured input
            var toolInput = ExtractToolInput(userInput, matchedTool.Name);
            var toolResult = await ExecuteToolAsync(matchedTool, toolInput, cancellationToken);

            // Get base LLM response first
            var response = await ChatAsync(request, cancellationToken);

            // Append tool result to response
            response.Content = FormatToolExecutionResult(response.Content, matchedTool.Name, toolResult);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tool execution failed for tool '{ToolName}'", matchedTool.Name);

            // Get base response and append error
            var response = await ChatAsync(request, cancellationToken);
            response.Content = FormatToolExecutionError(response.Content, matchedTool.Name, ex.Message);
            return response;
        }
    }

    /// <summary>
    /// Matches a tool by name from the registry, handling case-insensitive lookup.
    /// Supports both exact name match and prefix match (e.g., "GetUserInfo query:alice" matches "GetUserInfo" tool).
    /// </summary>
    private static IAgentTool? MatchToolByName(string userInput, Dictionary<string, IAgentTool> toolRegistry)
    {
        // Try exact match first (case-insensitive)
        foreach (var toolName in toolRegistry.Keys)
        {
            if (userInput.Contains(toolName, StringComparison.OrdinalIgnoreCase))
            {
                return toolRegistry[toolName];
            }
        }

        return null;
    }

    /// <summary>
    /// Extracts tool input parameters from user message.
    /// Returns a dictionary with "query" key containing the user message.
    /// Can be extended for structured parameter extraction in future versions.
    /// </summary>
    private static Dictionary<string, string> ExtractToolInput(string userInput, string toolName)
    {
        var query = userInput.Replace(toolName, string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
        var effectiveQuery = string.IsNullOrWhiteSpace(query) ? userInput : query;

        var input = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["query"] = effectiveQuery,
        };

        // Parse lightweight key-value tokens, e.g. operation=create-pull-request repositoryId=... branch=main.
        var knownKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "operation",
            "repositoryId",
            "sessionId",
            "taskId",
            "branch",
            "limit",
            "state",
            "title",
            "sourceBranch",
            "targetBranch",
            "description",
            "localPath",
        };

        var parts = effectiveQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            var separatorIndex = part.IndexOf('=');
            if (separatorIndex <= 0)
            {
                separatorIndex = part.IndexOf(':');
            }

            if (separatorIndex <= 0 || separatorIndex >= part.Length - 1)
            {
                continue;
            }

            var key = part[..separatorIndex].Trim();
            var value = part[(separatorIndex + 1)..].Trim();
            if (!knownKeys.Contains(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            input[key] = value.Trim('"', '\'', ',');
        }

        return input;
    }

    /// <summary>
    /// Executes a tool with the provided input, applying timeout and error handling.
    /// </summary>
    private async Task<string> ExecuteToolAsync(
        IAgentTool tool,
        IReadOnlyDictionary<string, string> input,
        CancellationToken cancellationToken)
    {
        // Apply timeout wrapper (5 seconds default)
        using (var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
        {
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
            try
            {
                return await tool.ExecuteAsync(input, timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new CustomException($"Tool '{tool.Name}' execution timeout after 5 seconds", 504);
            }
        }
    }

    /// <summary>
    /// Formats the tool execution result as structured output in the response.
    /// </summary>
    private static string FormatToolExecutionResult(string baseContent, string toolName, string toolResult)
    {
        return $"{baseContent}\n\n[Tool:{toolName}]\n{toolResult}";
    }

    /// <summary>
    /// Formats tool execution error as structured output in the response.
    /// </summary>
    private static string FormatToolExecutionError(string baseContent, string toolName, string errorMessage)
    {
        return $"{baseContent}\n\n[Tool:{toolName}:ERROR]\n{errorMessage}";
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
