/**
 * AgentIdleMonitorService 空闲监控服务测试
 *
 * 测试覆盖：
 * 1. Worker空闲超时触发学习
 * 2. Worker未超时不触发学习
 * 3. Manager空闲超时触发学习
 * 4. Secretary空闲超时触发学习
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace AutoCodeForge.Tests;

/// <summary>
/// AgentIdleMonitorService 空闲监控服务测试
/// </summary>
public sealed class Unit_AgentIdleMonitorServiceTests : IDisposable
{
    private readonly IntegrationTestContext _context;
    private readonly ServiceProvider _serviceProvider;

    public Unit_AgentIdleMonitorServiceTests()
    {
        _context = new IntegrationTestContext("test-user");
        
        var services = new ServiceCollection();
        services.AddSingleton(_context.AgentRepository);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task ScanIdleAgentsAsync_Should_TriggerLearning_WhenWorkerIdleTooLong()
    {
        var idleWorker = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Idle Worker",
            state: AgentState.Idle,
            role: AgentRole.Worker);
        idleWorker.StateChangedAtUtc = DateTime.UtcNow.AddMinutes(-20);
        await _context.AgentRepository.UpdateAsync(idleWorker);

        var scanMethod = typeof(AgentIdleMonitorService)
            .GetMethod("ScanIdleAgentsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var service = new AgentIdleMonitorService(_serviceProvider.GetRequiredService<IServiceScopeFactory>());
        await (Task)scanMethod.Invoke(service, new object[] { CancellationToken.None })!;

        var updatedWorker = await _context.AgentRepository.GetByIdAsync(idleWorker.Id);
        Assert.Equal(AgentState.Learning, updatedWorker?.State);
    }

    [Fact]
    public async Task ScanIdleAgentsAsync_Should_NotTriggerLearning_WhenWorkerNotIdleEnough()
    {
        var activeWorker = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Active Worker",
            state: AgentState.Idle,
            role: AgentRole.Worker);
        activeWorker.StateChangedAtUtc = DateTime.UtcNow.AddMinutes(-10);
        await _context.AgentRepository.UpdateAsync(activeWorker);

        var scanMethod = typeof(AgentIdleMonitorService)
            .GetMethod("ScanIdleAgentsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var service = new AgentIdleMonitorService(_serviceProvider.GetRequiredService<IServiceScopeFactory>());
        await (Task)scanMethod.Invoke(service, new object[] { CancellationToken.None })!;

        var updatedWorker = await _context.AgentRepository.GetByIdAsync(activeWorker.Id);
        Assert.Equal(AgentState.Idle, updatedWorker?.State);
    }

    [Fact]
    public async Task ScanIdleAgentsAsync_Should_TriggerLearning_WhenManagerIdleTooLong()
    {
        var idleManager = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Idle Manager",
            state: AgentState.Idle,
            role: AgentRole.Manager);
        idleManager.StateChangedAtUtc = DateTime.UtcNow.AddMinutes(-65);
        await _context.AgentRepository.UpdateAsync(idleManager);

        var scanMethod = typeof(AgentIdleMonitorService)
            .GetMethod("ScanIdleAgentsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var service = new AgentIdleMonitorService(_serviceProvider.GetRequiredService<IServiceScopeFactory>());
        await (Task)scanMethod.Invoke(service, new object[] { CancellationToken.None })!;

        var updatedManager = await _context.AgentRepository.GetByIdAsync(idleManager.Id);
        Assert.Equal(AgentState.Learning, updatedManager?.State);
    }

    [Fact]
    public async Task ScanIdleAgentsAsync_Should_TriggerLearning_WhenSecretaryIdleTooLong()
    {
        var idleSecretary = await _context.CreateTestAgentWithLifecycleAsync(
            name: "Idle Secretary",
            state: AgentState.Idle,
            role: AgentRole.Secretary);
        idleSecretary.StateChangedAtUtc = DateTime.UtcNow.AddMinutes(-35);
        await _context.AgentRepository.UpdateAsync(idleSecretary);

        var scanMethod = typeof(AgentIdleMonitorService)
            .GetMethod("ScanIdleAgentsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var service = new AgentIdleMonitorService(_serviceProvider.GetRequiredService<IServiceScopeFactory>());
        await (Task)scanMethod.Invoke(service, new object[] { CancellationToken.None })!;

        var updatedSecretary = await _context.AgentRepository.GetByIdAsync(idleSecretary.Id);
        Assert.Equal(AgentState.Learning, updatedSecretary?.State);
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        _context.Dispose();
    }
}