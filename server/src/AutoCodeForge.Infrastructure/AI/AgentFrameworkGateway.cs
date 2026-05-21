using AutoCodeForge.Core.DTOs.AI;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Azure;
using Azure.Identity;
using Azure.AI.OpenAI;
using System.Diagnostics;
using System.Text.Json;

namespace AutoCodeForge.Infrastructure.AI;

/// <summary>
/// LLM Gateway implementation using Microsoft Agent Framework.
/// Provides AI chat completion capabilities with tool integration.
/// </summary>
public class AgentFrameworkGateway : ILlmGateway
{
    private readonly LLMModelConfigRepository _modelConfigRepository;
    private readonly ILogger<AgentFrameworkGateway> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentFrameworkGateway"/> class.
    /// </summary>
    /// <param name="modelConfigRepository">The model config repository.</param>
    /// <param name="logger">The logger.</param>
    public AgentFrameworkGateway(LLMModelConfigRepository modelConfigRepository, ILogger<AgentFrameworkGateway> logger)
    {
        _modelConfigRepository = modelConfigRepository;
        _logger = logger;
    }

    /// <summary>
    /// Sends a chat request to the LLM model via Microsoft Agent Framework.
    /// </summary>
    /// <param name="request">The chat request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The LLM response.</returns>
    public async Task<LlmResponse> ChatAsync(LlmRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting chat request with Microsoft Agent Framework");

        var model = await GetModelConfigAsync(request.PreferredModelId, cancellationToken);
        if (model == null)
        {
            _logger.LogError("No model configuration found");
            throw new InvalidOperationException("No model configuration found");
        }

        try
        {
            var lastUserMessage = request.Messages.LastOrDefault(m => m.Role == "user")?.Content ?? string.Empty;
            var instructions = request.SystemPrompt ?? "You are a helpful AI assistant.";

            var chatClient = CreateChatClient(model);
            var agent = new ChatClientAgent(chatClient, instructions);

            var response = await agent.RunAsync(lastUserMessage, cancellationToken: cancellationToken);

            return new LlmResponse
            {
                Content = response.Text,
                ModelName = model.ModelName ?? "Microsoft.Agents.AI",
                CompletedAtUtc = DateTime.UtcNow,
                TotalTokens = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Microsoft Agent Framework call failed, falling back to mock");
            return CreateMockResponse(request.Messages.LastOrDefault(m => m.Role == "user")?.Content ?? string.Empty, model);
        }
    }

    /// <summary>
    /// Sends a chat request with tools to the LLM model via Microsoft Agent Framework.
    /// </summary>
    /// <param name="request">The chat request.</param>
    /// <param name="tools">The available tools.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The LLM response.</returns>
    public async Task<LlmResponse> ChatWithToolsAsync(LlmRequest request, IEnumerable<IAgentTool> tools, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting chat request with tools via Microsoft Agent Framework");

        var model = await GetModelConfigAsync(request.PreferredModelId, cancellationToken);
        if (model == null)
        {
            _logger.LogError("No model configuration found");
            throw new InvalidOperationException("No model configuration found");
        }

        try
        {
            var lastUserMessage = request.Messages.LastOrDefault(m => m.Role == "user")?.Content ?? string.Empty;
            var instructions = request.SystemPrompt ?? "You are a helpful AI assistant.";
            var agentTools = tools.Select(t => t.ToAgentTool()).Cast<AITool>().ToList();

            var chatClient = CreateChatClient(model);
            var agent = new ChatClientAgent(chatClient, instructions, null, null, agentTools);

            var response = await agent.RunAsync(lastUserMessage, cancellationToken: cancellationToken);

            return new LlmResponse
            {
                Content = response.Text,
                ModelName = model.ModelName ?? "Microsoft.Agents.AI",
                CompletedAtUtc = DateTime.UtcNow,
                TotalTokens = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Microsoft Agent Framework call with tools failed, falling back to mock");
            return CreateMockResponse(request.Messages.LastOrDefault(m => m.Role == "user")?.Content ?? string.Empty, model);
        }
    }

    /// <summary>
    /// Creates a chat client based on the model configuration.
    /// </summary>
    private IChatClient CreateChatClient(LLMModelConfigEntity model)
    {
        if (string.IsNullOrWhiteSpace(model.Endpoint))
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
            var config = await _modelConfigRepository.GetByIdAsync(modelConfigId.Value, false, cancellationToken);
            if (config != null)
            {
                return config;
            }
        }

        var configs = await _modelConfigRepository.GetAllAsync(false, cancellationToken);
        return configs.OrderBy(c => c.CreatedAtUtc).FirstOrDefault();
    }

    /// <summary>
    /// Creates a mock response for fallback.
    /// </summary>
    private LlmResponse CreateMockResponse(string userMessage, LLMModelConfigEntity? model)
    {
        var reply = $"[AutoCodeForge AI via Microsoft Agent Framework] {userMessage}";
        return new LlmResponse
        {
            Content = reply,
            ModelName = model?.ModelName ?? "Microsoft.Agents.AI (Mock)",
            CompletedAtUtc = DateTime.UtcNow,
            TotalTokens = Math.Max(1, userMessage.Length / 4),
        };
    }

    /// <summary>
    /// Calls GitHub Copilot CLI to get code suggestions.
    /// </summary>
    private async Task<LlmResponse> CallGitHubCopilotCliAsync(
        LLMModelConfigEntity model,
        string prompt,
        CancellationToken cancellationToken)
    {
        var executable = string.IsNullOrWhiteSpace(model.CliExecutable) 
            ? "copilot" 
            : model.CliExecutable;

        _logger.LogInformation(
            "Calling GitHub Copilot CLI: {Executable}, prompt={Prompt}",
            executable,
            prompt);

        try
        {
            using var process = new Process();
            process.StartInfo.FileName = executable;
            process.StartInfo.Arguments = $"suggest \"{EscapeForShell(prompt)}\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            if (!string.IsNullOrWhiteSpace(model.PatEnvVar))
            {
                process.StartInfo.EnvironmentVariables[model.PatEnvVar] = model.ApiKey ?? string.Empty;
            }

            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));

            await process.WaitForExitAsync(timeoutCts.Token);

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                _logger.LogError(
                    "GitHub Copilot CLI failed with exit code {ExitCode}: {Error}",
                    process.ExitCode,
                    error);
                throw new InvalidOperationException($"CLI execution failed: {error}");
            }

            _logger.LogInformation("GitHub Copilot CLI completed successfully");

            return new LlmResponse
            {
                Content = output,
                ModelName = model.ModelName ?? "GitHub Copilot CLI",
                CompletedAtUtc = DateTime.UtcNow,
                TotalTokens = 0,
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GitHub Copilot CLI request timed out");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GitHub Copilot CLI");
            throw;
        }
    }

    /// <summary>
    /// Escapes a string for shell command.
    /// </summary>
    private string EscapeForShell(string input)
    {
        return input.Replace("\"", "\\\"").Replace("`", "\\`").Replace("$", "\\$");
    }

    /// <summary>
    /// Generates structured output using the specified format.
    /// </summary>
    /// <typeparam name="T">The type of the response object.</typeparam>
    /// <param name="request">The chat request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The structured response.</returns>
    public async Task<T?> GenerateStructuredOutputAsync<T>(LlmRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting structured output generation");

        var response = await ChatAsync(request, cancellationToken);
        
        try
        {
            return JsonSerializer.Deserialize<T>(response.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize structured output");
            return default;
        }
    }
}