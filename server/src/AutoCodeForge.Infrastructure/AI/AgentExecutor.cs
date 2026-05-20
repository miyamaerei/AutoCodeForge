using AutoCodeForge.Core.DTOs.AI;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Infrastructure.AI;

/// <summary>
/// Executes one agent request by composing history, prompt, and tool context.
/// </summary>
public class AgentExecutor
{
    private readonly ILlmGateway _llmGateway;
    private readonly IEnumerable<IAgentTool> _tools;
    private readonly ILogger<AgentExecutor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentExecutor"/> class.
    /// </summary>
    /// <param name="llmGateway">The LLM gateway.</param>
    /// <param name="tools">The registered tools.</param>
    /// <param name="logger">The logger.</param>
    public AgentExecutor(ILlmGateway llmGateway, IEnumerable<IAgentTool> tools, ILogger<AgentExecutor> logger)
    {
        _llmGateway = llmGateway;
        _tools = tools;
        _logger = logger;
    }

    /// <summary>
    /// Executes one user input against a selected agent.
    /// </summary>
    /// <param name="agent">The selected agent.</param>
    /// <param name="userInput">The user input.</param>
    /// <param name="history">Conversation history.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The assistant output text.</returns>
    public async Task<string> ExecuteAsync(
        AgentEntity agent,
        string userInput,
        List<ChatMessageEntity> history,
        CancellationToken cancellationToken = default)
    {
        var messages = history.Select(ToDto).ToList();
        messages.Add(new ChatMessage
        {
            Role = "user",
            Content = userInput,
        });

        var request = new LlmRequest
        {
            PreferredModelId = agent.LlmModelConfigId,
            SystemPrompt = agent.SystemPrompt,
            Messages = messages,
        };

        var tools = FilterToolsByAgent(agent).ToList();
        _logger.LogInformation(
            "Executing agent {AgentId} ({AgentName}) with {ToolCount} tools",
            agent.Id,
            agent.Name,
            tools.Count);

        var response = await _llmGateway.ChatWithToolsAsync(request, tools, cancellationToken);
        return response.Content;
    }

    private IEnumerable<IAgentTool> FilterToolsByAgent(AgentEntity agent)
    {
        if (string.IsNullOrWhiteSpace(agent.ToolNames))
        {
            return _tools;
        }

        var allowed = agent.ToolNames
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return _tools.Where(tool => allowed.Contains(tool.Name));
    }

    private static ChatMessage ToDto(ChatMessageEntity message)
    {
        return new ChatMessage
        {
            Role = message.Type switch
            {
                MessageType.System => "system",
                MessageType.Assistant => "assistant",
                MessageType.Tool => "tool",
                _ => "user",
            },
            Content = message.Content,
        };
    }
}
