using AutoCodeForge.Core.DTOs.AI;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Infrastructure.AI;

/// <summary>
/// Executes one agent request by composing history, prompt, and tool context.
/// Uses Microsoft Agent Framework for enhanced agent capabilities.
/// </summary>
public class AgentExecutor
{
    private readonly ILlmGateway _llmGateway;
    private readonly AgentFactory _agentFactory;
    private readonly IEnumerable<IAgentTool> _tools;
    private readonly ILogger<AgentExecutor> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentExecutor"/> class.
    /// </summary>
    /// <param name="llmGateway">The LLM gateway.</param>
    /// <param name="agentFactory">The agent factory for creating Microsoft Agent Framework agents.</param>
    /// <param name="tools">The registered tools.</param>
    /// <param name="logger">The logger.</param>
    public AgentExecutor(ILlmGateway llmGateway, AgentFactory agentFactory, IEnumerable<IAgentTool> tools, ILogger<AgentExecutor> logger)
    {
        _llmGateway = llmGateway;
        _agentFactory = agentFactory;
        _tools = tools;
        _logger = logger;
    }

    /// <summary>
    /// Executes one user input against a selected agent using Microsoft Agent Framework.
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
        try
        {
            // Try using Microsoft Agent Framework first
            return await ExecuteWithAgentFrameworkAsync(agent, userInput, history, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Microsoft Agent Framework execution failed, falling back to LLM gateway");
            // Fallback to traditional LLM gateway
            return await ExecuteWithLlmGatewayAsync(agent, userInput, history, cancellationToken);
        }
    }

    /// <summary>
    /// Executes using Microsoft Agent Framework's ChatClientAgent.
    /// </summary>
    private async Task<string> ExecuteWithAgentFrameworkAsync(
        AgentEntity agent,
        string userInput,
        List<ChatMessageEntity> history,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing agent {AgentId} ({AgentName}) using Microsoft Agent Framework",
            agent.Id, agent.Name);

        // Create agent using AgentFactory
        var chatAgent = await _agentFactory.CreateAgentAsync(agent, cancellationToken);

        // Run the agent with the user input
        var result = await chatAgent.RunAsync(userInput, cancellationToken: cancellationToken);

        _logger.LogDebug("Agent {AgentName} returned response: {Response}", agent.Name, result.Text);

        return result.Text;
    }

    /// <summary>
    /// Executes using traditional LLM gateway as fallback.
    /// </summary>
    private async Task<string> ExecuteWithLlmGatewayAsync(
        AgentEntity agent,
        string userInput,
        List<ChatMessageEntity> history,
        CancellationToken cancellationToken)
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
            "Executing agent {AgentId} ({AgentName}) with {ToolCount} tools via LLM gateway",
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