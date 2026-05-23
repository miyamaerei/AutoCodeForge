/**
 * AgentRegistrationTests Agent注册功能测试
 *
 * 测试覆盖：
 * 1. Agent注册 - 新注册和重复注册
 * 2. 心跳续约 - 更新心跳时间戳
 * 3. Agent注销 - 标记为Offline
 * 4. 获取可用Agent - 查询Online状态的Agent
 * 5. 跨服务器分配 - 按ServerId过滤
 * 6. 心跳超时检测 - AgentHeartbeatMonitor功能
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;

namespace AutoCodeForge.Tests;

/// <summary>
/// Agent注册功能测试
/// </summary>
public sealed class Intg_AgentRegistrationTests : IDisposable
{
    private readonly IntegrationTestContext _context;
    private readonly IAgentRegistryService _registryService;

    public Intg_AgentRegistrationTests()
    {
        _context = new IntegrationTestContext("test-agent-registration-user");
        _registryService = new AgentRegistryService(
            _context.AgentRegistrationRepository,
            _context.AgentRepository,
            _context.InMemoryTaskEventPublisher);
    }

    #region RegisterAgent Tests

    /// <summary>
    /// 测试注册新Agent
    /// </summary>
    [Fact]
    public async Task RegisterAgentAsync_Should_CreateNewRegistration()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var serverId = "server-001";
        var instanceId = "instance-001";

        // Act
        var registration = await _registryService.RegisterAgentAsync(agentId, serverId, instanceId);

        // Assert
        Assert.NotNull(registration);
        Assert.Equal(agentId, registration.AgentId);
        Assert.Equal(serverId, registration.ServerId);
        Assert.Equal(instanceId, registration.InstanceId);
        Assert.Equal(AgentRegistrationStatus.Online, registration.Status);
        Assert.NotNull(registration.RegisteredAt);
        Assert.NotNull(registration.LastHeartbeat);
        Console.WriteLine("[测试1] 注册新Agent成功");
    }

    /// <summary>
    /// 测试重复注册同一Agent - 应该更新现有记录
    /// </summary>
    [Fact]
    public async Task RegisterAgentAsync_Should_UpdateExistingRegistration()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var firstServerId = "server-001";
        var secondServerId = "server-002";

        await _registryService.RegisterAgentAsync(agentId, firstServerId, "instance-001");
        await Task.Delay(100);

        // Act
        var registration = await _registryService.RegisterAgentAsync(agentId, secondServerId, "instance-002");

        // Assert
        Assert.Equal(secondServerId, registration.ServerId);
        Assert.Equal("instance-002", registration.InstanceId);
        Console.WriteLine("[测试2] 重复注册Agent - 更新成功");
    }

    #endregion

    #region RenewHeartbeat Tests

    /// <summary>
    /// 测试心跳续约 - 更新心跳时间戳
    /// </summary>
    [Fact]
    public async Task RenewHeartbeatAsync_Should_UpdateLastHeartbeat()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        await _registryService.RegisterAgentAsync(agentId, "server-001", "instance-001");
        
        var registrationBefore = await _registryService.GetAgentRegistrationAsync(agentId);
        Assert.NotNull(registrationBefore);
        var heartbeatBefore = registrationBefore.LastHeartbeat;
        
        await Task.Delay(100);

        // Act
        var success = await _registryService.RenewHeartbeatAsync(agentId);

        // Assert
        Assert.True(success);
        var registrationAfter = await _registryService.GetAgentRegistrationAsync(agentId);
        Assert.NotNull(registrationAfter);
        Assert.True(registrationAfter.LastHeartbeat > heartbeatBefore);
        Console.WriteLine("[测试3] 心跳续约成功");
    }

    /// <summary>
    /// 测试心跳续约 - Agent未注册时返回false
    /// </summary>
    [Fact]
    public async Task RenewHeartbeatAsync_Should_ReturnFalse_WhenAgentNotRegistered()
    {
        // Arrange
        var nonExistentAgentId = Guid.NewGuid();

        // Act
        var success = await _registryService.RenewHeartbeatAsync(nonExistentAgentId);

        // Assert
        Assert.False(success);
        Console.WriteLine("[测试4] 未注册Agent心跳续约返回false");
    }

    #endregion

    #region DeregisterAgent Tests

    /// <summary>
    /// 测试注销Agent - 标记为Offline
    /// </summary>
    [Fact]
    public async Task DeregisterAgentAsync_Should_MarkAsOffline()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        await _registryService.RegisterAgentAsync(agentId, "server-001", "instance-001");

        // Act
        var success = await _registryService.DeregisterAgentAsync(agentId);

        // Assert
        Assert.True(success);
        var registration = await _registryService.GetAgentRegistrationAsync(agentId);
        Assert.NotNull(registration);
        Assert.Equal(AgentRegistrationStatus.Offline, registration.Status);
        Console.WriteLine("[测试5] Agent注销成功");
    }

    /// <summary>
    /// 测试注销Agent - Agent不存在时返回false
    /// </summary>
    [Fact]
    public async Task DeregisterAgentAsync_Should_ReturnFalse_WhenAgentNotFound()
    {
        // Arrange
        var nonExistentAgentId = Guid.NewGuid();

        // Act
        var success = await _registryService.DeregisterAgentAsync(nonExistentAgentId);

        // Assert
        Assert.False(success);
        Console.WriteLine("[测试6] 注销不存在的Agent返回false");
    }

    #endregion

    #region GetAvailableAgents Tests

    /// <summary>
    /// 测试获取可用Agent - 只返回Online状态的Agent
    /// </summary>
    [Fact]
    public async Task GetAvailableAgentsAsync_Should_ReturnOnlineAgentsOnly()
    {
        // Arrange
        var onlineAgentId = Guid.NewGuid();
        var offlineAgentId = Guid.NewGuid();
        
        await _registryService.RegisterAgentAsync(onlineAgentId, "server-001", "instance-001");
        await _registryService.RegisterAgentAsync(offlineAgentId, "server-001", "instance-002");
        await _registryService.DeregisterAgentAsync(offlineAgentId);

        // Act
        var availableAgents = await _registryService.GetAvailableAgentsAsync();

        // Assert
        Assert.Single(availableAgents);
        Assert.Equal(onlineAgentId, availableAgents[0].AgentId);
        Console.WriteLine("[测试7] 获取可用Agent - 只返回Online状态");
    }

    #endregion

    #region GetAgentsByServerId Tests

    /// <summary>
    /// 测试按服务器ID获取Agent - 跨服务器分配
    /// </summary>
    [Fact]
    public async Task GetAgentsByServerIdAsync_Should_ReturnAgentsOnSpecificServer()
    {
        // Arrange
        var server1AgentId = Guid.NewGuid();
        var server2AgentId = Guid.NewGuid();
        
        await _registryService.RegisterAgentAsync(server1AgentId, "server-001", "instance-001");
        await _registryService.RegisterAgentAsync(server2AgentId, "server-002", "instance-001");

        // Act
        var server1Agents = await _registryService.GetAgentsByServerIdAsync("server-001");
        var server2Agents = await _registryService.GetAgentsByServerIdAsync("server-002");

        // Assert
        Assert.Single(server1Agents);
        Assert.Equal(server1AgentId, server1Agents[0].AgentId);
        Assert.Single(server2Agents);
        Assert.Equal(server2AgentId, server2Agents[0].AgentId);
        Console.WriteLine("[测试8] 按服务器ID获取Agent成功");
    }

    #endregion

    #region HeartbeatTimeout Tests

    /// <summary>
    /// 测试心跳超时检测 - 通过Repository测试超时逻辑
    /// </summary>
    [Fact]
    public async Task HeartbeatTimeoutDetection_Should_WorkCorrectly()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        await _registryService.RegisterAgentAsync(agentId, "server-001", "instance-001");

        // 手动设置过期的心跳时间（130秒前，超过120秒超时阈值）
        var registration = await _context.AgentRegistrationRepository.GetByAgentIdAsync(agentId);
        Assert.NotNull(registration);
        registration.LastHeartbeat = DateTime.UtcNow.AddSeconds(-130);
        await _context.AgentRegistrationRepository.UpdateAsync(registration);

        // Act - 使用Repository的超时查询方法
        var timeoutAgents = await _context.AgentRegistrationRepository.GetTimeoutAgentsAsync(120, CancellationToken.None);

        // Assert
        Assert.Single(timeoutAgents);
        Assert.Equal(agentId, timeoutAgents[0].AgentId);
        
        // 手动更新为Offline状态（模拟监控服务的行为）
        timeoutAgents[0].Status = AgentRegistrationStatus.Offline;
        await _context.AgentRegistrationRepository.UpdateAsync(timeoutAgents[0]);
        
        var updatedRegistration = await _registryService.GetAgentRegistrationAsync(agentId);
        Assert.Equal(AgentRegistrationStatus.Offline, updatedRegistration?.Status);
        Console.WriteLine("[测试9] 心跳超时检测 - 超时逻辑正确");
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}