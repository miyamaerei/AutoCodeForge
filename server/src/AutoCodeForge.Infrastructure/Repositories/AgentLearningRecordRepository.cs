using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides data access for agent learning records.
/// </summary>
public class AgentLearningRecordRepository : BaseRepository<AgentLearningRecordEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AgentLearningRecordRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public AgentLearningRecordRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// 根据Agent ID获取学习记录列表
    /// </summary>
    public async Task<List<AgentLearningRecordEntity>> GetByAgentIdAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(record => record.AgentId == agentId)
            .OrderByDescending(record => record.CreatedAtUtc)
            .ToListAsync();
    }

    /// <summary>
    /// 获取Agent最近的学习记录（用于判断连续学习效果）
    /// </summary>
    public async Task<List<AgentLearningRecordEntity>> GetRecentLearningsAsync(Guid agentId, int count, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(record => record.AgentId == agentId && record.IsSuccessful)
            .OrderByDescending(record => record.CreatedAtUtc)
            .Take(count)
            .ToListAsync();
    }
}