using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Infrastructure.BackgroundServices;

public class AgentHeartbeatMonitor : BackgroundService
{
    private readonly AgentRegistrationRepository _registrationRepository;
    private readonly AgentRepository _agentRepository;
    private readonly ITaskEventPublisher _eventPublisher;
    private readonly ILogger<AgentHeartbeatMonitor> _logger;
    private readonly int _heartbeatIntervalSeconds = 30;
    private readonly int _timeoutSeconds = 120;

    public AgentHeartbeatMonitor(
        AgentRegistrationRepository registrationRepository,
        AgentRepository agentRepository,
        ITaskEventPublisher eventPublisher,
        ILogger<AgentHeartbeatMonitor> logger)
    {
        _registrationRepository = registrationRepository;
        _agentRepository = agentRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AgentHeartbeatMonitor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckHeartbeatTimeoutsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking agent heartbeats");
            }

            await Task.Delay(TimeSpan.FromSeconds(_heartbeatIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("AgentHeartbeatMonitor stopped");
    }

    private async Task CheckHeartbeatTimeoutsAsync(CancellationToken cancellationToken)
    {
        var timeoutAgents = await _registrationRepository.GetTimeoutAgentsAsync(_timeoutSeconds, cancellationToken);
        
        if (timeoutAgents.Any())
        {
            _logger.LogWarning("Found {Count} agents with timeout heartbeat", timeoutAgents.Count);
            
            foreach (var agent in timeoutAgents)
            {
                try
                {
                    agent.Status = AgentRegistrationStatus.Offline;
                    await _registrationRepository.UpdateAsync(agent, cancellationToken);
                    
                    await _agentRepository.SetStateAsync(agent.AgentId, AgentState.Idle, cancellationToken);
                    
                    _logger.LogInformation("Agent {AgentId} marked as offline due to heartbeat timeout", agent.AgentId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating agent {AgentId} status", agent.AgentId);
                }
            }
        }
    }
}