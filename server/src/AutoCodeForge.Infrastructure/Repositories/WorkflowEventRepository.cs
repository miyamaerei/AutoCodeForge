using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// 工作流事件仓储
/// </summary>
public class WorkflowEventRepository : BaseRepository<WorkflowEventEntity>
{
    /// <summary>
    /// 初始化工作流事件仓储
    /// </summary>
    /// <param name="db">SqlSugar客户端</param>
    /// <param name="currentUser">当前用户</param>
    public WorkflowEventRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// 获取实例的所有事件
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>事件列表</returns>
    public async Task<List<WorkflowEventEntity>> GetByInstanceIdAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(e => e.InstanceId == instanceId)
            .OrderBy(e => e.CreatedAtUtc)
            .ToListAsync();
    }

    /// <summary>
    /// 获取实例的最新事件
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    /// <param name="take">获取数量</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>事件列表</returns>
    public async Task<List<WorkflowEventEntity>> GetLatestByInstanceIdAsync(Guid instanceId, int take = 10, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(e => e.InstanceId == instanceId)
            .OrderByDescending(e => e.CreatedAtUtc)
            .Take(take)
            .ToListAsync();
    }
}
