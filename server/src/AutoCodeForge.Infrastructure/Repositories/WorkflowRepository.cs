using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// 工作流定义仓储
/// </summary>
public class WorkflowRepository : BaseRepository<WorkflowEntity>
{
    /// <summary>
    /// 初始化工作流仓储
    /// </summary>
    /// <param name="db">SqlSugar客户端</param>
    /// <param name="currentUser">当前用户</param>
    public WorkflowRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// 获取最近创建的工作流
    /// </summary>
    /// <param name="take">获取数量</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工作流列表</returns>
    public async Task<List<WorkflowEntity>> GetRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var normalizedTake = take <= 0 ? 20 : Math.Min(take, 100);
        return await Queryable
            .OrderByDescending(w => w.CreatedAtUtc)
            .Take(normalizedTake)
            .ToListAsync();
    }

    /// <summary>
    /// 获取指定状态的工作流
    /// </summary>
    /// <param name="status">工作流状态</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工作流列表</returns>
    public async Task<List<WorkflowEntity>> GetByStatusAsync(WorkflowStatus status, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(w => w.Status == status)
            .OrderByDescending(w => w.CreatedAtUtc)
            .ToListAsync();
    }
}
