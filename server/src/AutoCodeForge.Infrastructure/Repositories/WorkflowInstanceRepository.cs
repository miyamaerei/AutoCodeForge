using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// 工作流实例仓储
/// </summary>
public class WorkflowInstanceRepository : BaseRepository<WorkflowInstanceEntity>
{
    /// <summary>
    /// 初始化工作流实例仓储
    /// </summary>
    /// <param name="db">SqlSugar客户端</param>
    /// <param name="currentUser">当前用户</param>
    public WorkflowInstanceRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// 获取工作流的所有实例
    /// </summary>
    /// <param name="workflowId">工作流ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>实例列表</returns>
    public async Task<List<WorkflowInstanceEntity>> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(i => i.WorkflowId == workflowId)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync();
    }

    /// <summary>
    /// 获取正在运行的实例
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>正在运行的实例列表</returns>
    public async Task<List<WorkflowInstanceEntity>> GetRunningAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(i => i.Status == WorkflowInstanceStatus.Running)
            .OrderByDescending(i => i.StartedAtUtc)
            .ToListAsync();
    }

    /// <summary>
    /// 获取最近的实例
    /// </summary>
    /// <param name="take">获取数量</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>实例列表</returns>
    public async Task<List<WorkflowInstanceEntity>> GetRecentAsync(int take, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var normalizedTake = take <= 0 ? 20 : Math.Min(take, 100);
        return await Queryable
            .OrderByDescending(i => i.CreatedAtUtc)
            .Take(normalizedTake)
            .ToListAsync();
    }

    /// <summary>
    /// 尝试将实例标记为运行中
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    /// <param name="startedAtUtc">开始时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    public async Task<bool> TryMarkRunningAsync(Guid instanceId, DateTime startedAtUtc, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var affected = await Db.Updateable<WorkflowInstanceEntity>()
            .SetColumns(i => i.Status == WorkflowInstanceStatus.Running)
            .SetColumns(i => i.StartedAtUtc == startedAtUtc)
            .SetColumns(i => i.UpdatedAtUtc == DateTime.UtcNow)
            .Where(i => i.Id == instanceId && i.Status == WorkflowInstanceStatus.Pending)
            .ExecuteCommandAsync();

        return affected > 0;
    }

    /// <summary>
    /// 更新实例进度
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    /// <param name="progress">进度</param>
    /// <param name="currentNodeId">当前节点ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task UpdateProgressAsync(Guid instanceId, int progress, string? currentNodeId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await Db.Updateable<WorkflowInstanceEntity>()
            .SetColumns(i => i.Progress == progress)
            .SetColumns(i => i.CurrentNodeId == currentNodeId)
            .SetColumns(i => i.UpdatedAtUtc == DateTime.UtcNow)
            .Where(i => i.Id == instanceId)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 标记实例为已完成
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    /// <param name="outputJson">输出JSON</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task MarkCompletedAsync(Guid instanceId, string? outputJson, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await Db.Updateable<WorkflowInstanceEntity>()
            .SetColumns(i => i.Status == WorkflowInstanceStatus.Completed)
            .SetColumns(i => i.Progress == 100)
            .SetColumns(i => i.OutputJson == outputJson)
            .SetColumns(i => i.CompletedAtUtc == DateTime.UtcNow)
            .SetColumns(i => i.UpdatedAtUtc == DateTime.UtcNow)
            .Where(i => i.Id == instanceId)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 标记实例为失败
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    /// <param name="errorMessage">错误信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task MarkFailedAsync(Guid instanceId, string errorMessage, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await Db.Updateable<WorkflowInstanceEntity>()
            .SetColumns(i => i.Status == WorkflowInstanceStatus.Failed)
            .SetColumns(i => i.ErrorMessage == errorMessage)
            .SetColumns(i => i.CompletedAtUtc == DateTime.UtcNow)
            .SetColumns(i => i.UpdatedAtUtc == DateTime.UtcNow)
            .Where(i => i.Id == instanceId)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 标记实例为暂停
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task MarkPausedAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await Db.Updateable<WorkflowInstanceEntity>()
            .SetColumns(i => i.Status == WorkflowInstanceStatus.Paused)
            .SetColumns(i => i.UpdatedAtUtc == DateTime.UtcNow)
            .Where(i => i.Id == instanceId && i.Status == WorkflowInstanceStatus.Running)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 标记实例为已恢复
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task MarkResumedAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await Db.Updateable<WorkflowInstanceEntity>()
            .SetColumns(i => i.Status == WorkflowInstanceStatus.Running)
            .SetColumns(i => i.UpdatedAtUtc == DateTime.UtcNow)
            .Where(i => i.Id == instanceId && i.Status == WorkflowInstanceStatus.Paused)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 标记实例为已终止
    /// </summary>
    /// <param name="instanceId">实例ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task MarkTerminatedAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await Db.Updateable<WorkflowInstanceEntity>()
            .SetColumns(i => i.Status == WorkflowInstanceStatus.Terminated)
            .SetColumns(i => i.CompletedAtUtc == DateTime.UtcNow)
            .SetColumns(i => i.UpdatedAtUtc == DateTime.UtcNow)
            .Where(i => i.Id == instanceId)
            .ExecuteCommandAsync();
    }
}
