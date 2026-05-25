using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for agent dormant records.
/// </summary>
public class AgentDormantRecordRepository : BaseRepository<AgentDormantRecordEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AgentDormantRecordRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public AgentDormantRecordRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// 根据Agent ID获取休眠记录列表
    /// </summary>
    public async Task<List<AgentDormantRecordEntity>> GetByAgentIdAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(record => record.AgentId == agentId)
            .OrderByDescending(record => record.CreatedAtUtc)
            .ToListAsync();
    }

    /// <summary>
    /// 获取Agent当前的休眠记录（如果存在）
    /// </summary>
    public async Task<AgentDormantRecordEntity?> GetCurrentDormantRecordAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(record => record.AgentId == agentId && !record.IsWoken)
            .OrderByDescending(record => record.CreatedAtUtc)
            .FirstAsync();
    }

    /// <summary>
    /// 获取所有当前处于休眠状态的Agent记录
    /// </summary>
    public async Task<List<AgentDormantRecordEntity>> GetAllDormantAgentsAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(record => !record.IsWoken)
            .OrderByDescending(record => record.CreatedAtUtc)
            .ToListAsync();
    }
}