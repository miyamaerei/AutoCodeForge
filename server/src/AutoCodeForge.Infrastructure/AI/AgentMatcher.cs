using AutoCodeForge.Core.Entities;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Infrastructure.AI;

/// <summary>
/// Matches user input to the most suitable enabled agent.
/// </summary>
public class AgentMatcher
{
    private readonly AgentRepository _agentRepository;
    private readonly ILogger<AgentMatcher> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentMatcher"/> class.
    /// </summary>
    /// <param name="agentRepository">The agent repository.</param>
    /// <param name="logger">The logger.</param>
    public AgentMatcher(AgentRepository agentRepository, ILogger<AgentMatcher> logger)
    {
        _agentRepository = agentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Matches the best agent for one user input.
    /// </summary>
    /// <param name="userInput">The user input.</param>
    /// <param name="preferredAgentId">Optional preferred agent identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matched agent, or null when no enabled agent exists.</returns>
    public async Task<AgentEntity?> MatchAgentAsync(
        string userInput,
        Guid? preferredAgentId = null,
        CancellationToken cancellationToken = default)
    {
        if (preferredAgentId.HasValue)
        {
            var preferred = await _agentRepository.GetByIdAsync(preferredAgentId.Value, false, cancellationToken);
            if (preferred is { IsEnabled: true })
            {
                return preferred;
            }
        }

        var enabledAgents = await _agentRepository.GetEnabledAsync(cancellationToken);
        if (enabledAgents.Count == 0)
        {
            return null;
        }

        var normalizedInput = userInput.Trim();
        var scored = enabledAgents
            .Select(agent => new
            {
                Agent = agent,
                Score = ScoreByKeyword(agent, normalizedInput),
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Agent.Name)
            .ToList();

        var best = scored.First();
        _logger.LogInformation("Matched agent {AgentName} with score {Score}", best.Agent.Name, best.Score);
        return best.Agent;
    }

    private static int ScoreByKeyword(AgentEntity agent, string input)
    {
        if (string.IsNullOrWhiteSpace(agent.Keywords) || string.IsNullOrWhiteSpace(input))
        {
            return 0;
        }

        var keywords = agent.Keywords
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return keywords.Count(keyword =>
            input.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
