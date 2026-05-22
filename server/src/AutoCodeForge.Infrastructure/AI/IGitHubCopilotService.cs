using AutoCodeForge.Core.DTOs.AI;
using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Infrastructure.AI;

/// <summary>
/// Defines GitHub Copilot integration operations.
/// </summary>
public interface IGitHubCopilotService
{
    /// <summary>
    /// Executes a prompt using GitHub Copilot.
    /// </summary>
    /// <param name="model">The model configuration.</param>
    /// <param name="prompt">The user prompt.</param>
    /// <param name="systemPrompt">The system prompt.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The LLM response.</returns>
    Task<LlmResponse> ExecuteAsync(
        LLMModelConfigEntity model,
        string prompt,
        string? systemPrompt,
        CancellationToken cancellationToken = default);
}
