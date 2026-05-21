using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Azure;
using Azure.Identity;
using Azure.AI.OpenAI;

namespace AutoCodeForge.Infrastructure.AI;

/// <summary>
/// Factory service for creating and managing Agent instances using Microsoft Agent Framework.
/// </summary>
public class AgentFactory
{
    private readonly LLMModelConfigRepository _configRepository;
    private readonly ILogger<AgentFactory> _logger;
    private readonly IEnumerable<IAgentTool> _tools;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentFactory"/> class.
    /// </summary>
    /// <param name="configRepository">The model config repository.</param>
    /// <param name="tools">The registered agent tools.</param>
    /// <param name="logger">The logger.</param>
    public AgentFactory(LLMModelConfigRepository configRepository, IEnumerable<IAgentTool> tools, ILogger<AgentFactory> logger)
    {
        _configRepository = configRepository;
        _tools = tools;
        _logger = logger;
    }

    /// <summary>
    /// Creates an AI Agent using the specified model configuration.
    /// </summary>
    /// <param name="modelConfigId">The model configuration ID.</param>
    /// <param name="instructions">The agent instructions/system prompt.</param>
    /// <param name="agentName">The agent name.</param>
    /// <param name="allowedToolNames">Optional list of allowed tool names.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An AI Agent instance.</returns>
    public async Task<ChatClientAgent> CreateAgentAsync(
        Guid? modelConfigId,
        string instructions,
        string agentName,
        IEnumerable<string>? allowedToolNames = null,
        CancellationToken cancellationToken = default)
    {
        var model = await GetModelConfigAsync(modelConfigId, cancellationToken);
        var agentTools = GetAgentTools(allowedToolNames).Cast<AITool>().ToList();

        var chatClient = CreateChatClient(model);
        var agent = new ChatClientAgent(chatClient, instructions, agentName, null, agentTools);

        _logger.LogInformation("Created agent {AgentName} using Microsoft Agent Framework with {ToolCount} tools", agentName, agentTools.Count);

        return agent;
    }

    /// <summary>
    /// Creates an AI Agent using an existing AgentEntity configuration.
    /// </summary>
    /// <param name="agentEntity">The agent entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An AI Agent instance.</returns>
    public async Task<ChatClientAgent> CreateAgentAsync(AgentEntity agentEntity, CancellationToken cancellationToken = default)
    {
        var allowedTools = string.IsNullOrWhiteSpace(agentEntity.ToolNames) 
            ? null 
            : agentEntity.ToolNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return await CreateAgentAsync(
            agentEntity.LlmModelConfigId,
            agentEntity.SystemPrompt ?? string.Empty,
            agentEntity.Name,
            allowedTools,
            cancellationToken);
    }

    /// <summary>
    /// Creates a chat client based on the model configuration.
    /// </summary>
    private IChatClient CreateChatClient(LLMModelConfigEntity? model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Endpoint))
        {
            throw new InvalidOperationException("Model endpoint is required for Microsoft Agent Framework");
        }

        var endpoint = new Uri(model.Endpoint);

        if (!string.IsNullOrWhiteSpace(model.ApiKey))
        {
            // Use Azure OpenAI client with API key
            var azureClient = new AzureOpenAIClient(endpoint, new AzureKeyCredential(model.ApiKey));
            return azureClient.GetChatClient(model.ModelName ?? "gpt-4o").AsIChatClient();
        }
        else
        {
            // Use Azure Identity for authentication
            var azureClient = new AzureOpenAIClient(endpoint, new DefaultAzureCredential());
            return azureClient.GetChatClient(model.ModelName ?? "gpt-4o").AsIChatClient();
        }
    }

    /// <summary>
    /// Gets the model configuration.
    /// </summary>
    private async Task<LLMModelConfigEntity?> GetModelConfigAsync(Guid? modelConfigId, CancellationToken cancellationToken)
    {
        if (modelConfigId.HasValue)
        {
            var config = await _configRepository.GetByIdAsync(modelConfigId.Value, false, cancellationToken);
            if (config != null)
            {
                return config;
            }
        }

        var configs = await _configRepository.GetAllAsync(false, cancellationToken);
        return configs.OrderBy(c => c.CreatedAtUtc).FirstOrDefault();
    }

    /// <summary>
    /// Gets the tools filtered by allowed names.
    /// </summary>
    private IEnumerable<AIFunction> GetAgentTools(IEnumerable<string>? allowedToolNames)
    {
        var tools = _tools;
        
        if (allowedToolNames != null && allowedToolNames.Any())
        {
            var allowedSet = allowedToolNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
            tools = tools.Where(t => allowedSet.Contains(t.Name));
        }

        return tools.Select(t => t.ToAgentTool());
    }
}