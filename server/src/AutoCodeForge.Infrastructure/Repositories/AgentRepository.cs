using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for agents.
/// </summary>
public class AgentRepository : BaseRepository<AgentEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AgentRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public AgentRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets enabled agents visible to the current user.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Enabled agents.</returns>
    public async Task<List<AgentEntity>> GetEnabledAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(agent => agent.IsEnabled)
            .OrderBy(agent => agent.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 获取指定状态的Agent列表
    /// </summary>
    public async Task<List<AgentEntity>> GetByStateAsync(AgentState state, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(agent => agent.State == state && agent.IsEnabled)
            .OrderBy(agent => agent.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 获取所有休眠状态的Agent列表
    /// </summary>
    public async Task<List<AgentEntity>> GetDormantAgentsAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(agent => agent.State == AgentState.Dormant && agent.IsEnabled)
            .OrderBy(agent => agent.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 获取指定角色的可用Agent列表
    /// </summary>
    public async Task<List<AgentEntity>> GetAvailableAgentsByRoleAsync(AgentRole role, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(agent => agent.Role == role && agent.State == AgentState.Idle && agent.IsEnabled)
            .OrderBy(agent => agent.CurrentTaskCount)
            .ToListAsync();
    }

    /// <summary>
    /// 增加Agent的当前任务计数
    /// </summary>
    public async Task IncrementTaskCountAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await Db.Updateable<AgentEntity>()
            .SetColumns(agent => agent.CurrentTaskCount == agent.CurrentTaskCount + 1)
            .Where(agent => agent.Id == agentId)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 减少Agent的当前任务计数
    /// </summary>
    public async Task DecrementTaskCountAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await Db.Updateable<AgentEntity>()
            .SetColumns(agent => agent.CurrentTaskCount == agent.CurrentTaskCount - 1)
            .Where(agent => agent.Id == agentId && agent.CurrentTaskCount > 0)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 设置Agent状态
    /// </summary>
    public async Task SetStateAsync(Guid agentId, AgentState state, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await Db.Updateable<AgentEntity>()
            .SetColumns(agent => agent.State == state)
            .Where(agent => agent.Id == agentId)
            .ExecuteCommandAsync();
    }
}
