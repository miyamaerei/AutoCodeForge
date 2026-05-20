using AutoCodeForge.Core.DTOs.AI;

namespace AutoCodeForge.Core.Interfaces;

/// <summary>
/// Defines LLM gateway operations.
/// </summary>
public interface ILlmGateway
{
    /// <summary>
    /// Sends one chat completion request.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response payload.</returns>
    Task<LlmResponse> ChatAsync(LlmRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends one chat completion request with tool metadata.
    /// </summary>
    /// <param name="request">The request payload.</param>
    /// <param name="tools">The available agent tools.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response payload.</returns>
    Task<LlmResponse> ChatWithToolsAsync(
        LlmRequest request,
        IEnumerable<IAgentTool> tools,
        CancellationToken cancellationToken = default);
}
