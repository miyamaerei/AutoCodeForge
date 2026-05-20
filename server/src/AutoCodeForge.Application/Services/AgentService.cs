using AutoCodeForge.Core.DTOs.Agent;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides agent CRUD and matching operations.
/// </summary>
public class AgentService
{
    private readonly AgentRepository _agentRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentService"/> class.
    /// </summary>
    /// <param name="agentRepository">The agent repository.</param>
    public AgentService(AgentRepository agentRepository)
    {
        _agentRepository = agentRepository;
    }

    /// <summary>
    /// Creates one agent.
    /// </summary>
    /// <param name="request">The create request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created agent.</returns>
    public async Task<AgentResponse> CreateAsync(CreateAgentRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Keywords = request.Keywords?.Trim(),
            SystemPrompt = request.SystemPrompt,
            LlmModelConfigId = request.LlmModelConfigId,
            IsEnabled = request.IsEnabled,
        };

        var created = await _agentRepository.CreateAsync(entity, cancellationToken);
        return ToResponse(created);
    }

    /// <summary>
    /// Updates one agent.
    /// </summary>
    /// <param name="id">The agent identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated agent.</returns>
    public async Task<AgentResponse> UpdateAsync(
        Guid id,
        UpdateAgentRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _agentRepository.GetByIdAsync(id, false, cancellationToken)
            ?? throw new NotFoundException("Agent not found");

        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.Keywords = request.Keywords?.Trim();
        entity.SystemPrompt = request.SystemPrompt;
        entity.LlmModelConfigId = request.LlmModelConfigId;
        entity.IsEnabled = request.IsEnabled;

        await _agentRepository.UpdateAsync(entity, cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// Gets one agent by identifier.
    /// </summary>
    /// <param name="id">The agent identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The agent response.</returns>
    public async Task<AgentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _agentRepository.GetByIdAsync(id, false, cancellationToken)
            ?? throw new NotFoundException("Agent not found");

        return ToResponse(entity);
    }

    /// <summary>
    /// Gets paged agents.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged agents response.</returns>
    public async Task<PagedResult<AgentResponse>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var paged = await _agentRepository.GetPagedAsync(page, pageSize, false, cancellationToken);
        return new PagedResult<AgentResponse>(
            paged.Items.Select(ToResponse).ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize);
    }

    /// <summary>
    /// Deletes one agent.
    /// </summary>
    /// <param name="id">The agent identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _agentRepository.SoftDeleteAsync(id, false, cancellationToken);
    }

    /// <summary>
    /// Matches the best enabled agent for one input.
    /// </summary>
    /// <param name="userInput">The user input.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The best matched agent or null.</returns>
    public async Task<AgentResponse?> MatchByInputAsync(string userInput, CancellationToken cancellationToken = default)
    {
        var enabled = await _agentRepository.GetEnabledAsync(cancellationToken);
        if (enabled.Count == 0)
        {
            return null;
        }

        var best = enabled
            .Select(agent => new
            {
                Agent = agent,
                Score = Score(agent.Keywords, userInput),
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Agent.Name)
            .First()
            .Agent;

        return ToResponse(best);
    }

    private static int Score(string? keywords, string userInput)
    {
        if (string.IsNullOrWhiteSpace(keywords) || string.IsNullOrWhiteSpace(userInput))
        {
            return 0;
        }

        var split = keywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return split.Count(keyword => userInput.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static AgentResponse ToResponse(AgentEntity entity)
    {
        return new AgentResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Keywords = entity.Keywords,
            SystemPrompt = entity.SystemPrompt,
            LlmModelConfigId = entity.LlmModelConfigId,
            IsEnabled = entity.IsEnabled,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
        };
    }
}
