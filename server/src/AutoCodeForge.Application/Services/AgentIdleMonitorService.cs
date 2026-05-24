using AutoCodeForge.Application.StateMachines;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AutoCodeForge.Application.Services;

public class AgentIdleMonitorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _scanInterval = TimeSpan.FromSeconds(30);
    private readonly Dictionary<AgentRole, TimeSpan> _idleTimeouts = new()
    {
        { AgentRole.Secretary, TimeSpan.FromMinutes(30) },
        { AgentRole.Manager, TimeSpan.FromMinutes(60) },
        { AgentRole.Worker, TimeSpan.FromMinutes(15) }
    };

    public AgentIdleMonitorService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanIdleAgentsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
            }

            await Task.Delay(_scanInterval, stoppingToken);
        }
    }

    private async Task ScanIdleAgentsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var agentRepository = scope.ServiceProvider.GetRequiredService<AgentRepository>();
        var stateMachine = new AgentStateMachine();

        var idleAgents = await agentRepository.GetByStateAsync(AgentState.Idle, cancellationToken);

        foreach (var agent in idleAgents)
        {
            var idleTimeout = _idleTimeouts.TryGetValue(agent.Role, out var timeout) ? timeout : TimeSpan.FromMinutes(30);
            
            if (DateTime.UtcNow - agent.StateChangedAtUtc >= idleTimeout)
            {
                try
                {
                    await stateMachine.HandleEventAsync(agent, StateEvent.StartLearning, cancellationToken);
                    await agentRepository.UpdateAsync(agent, cancellationToken);
                }
                catch (InvalidOperationException)
                {
                }
            }
        }
    }
}