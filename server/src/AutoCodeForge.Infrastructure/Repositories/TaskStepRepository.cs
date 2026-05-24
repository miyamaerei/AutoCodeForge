using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// 工序步骤的数据访问层
/// </summary>
public class TaskStepRepository : BaseRepository<TaskStepEntity>
{
    /// <summary>
    /// 初始化 TaskStepRepository 实例
    /// </summary>
    /// <param name="db">SqlSugar 客户端</param>
    /// <param name="currentUser">当前用户提供者</param>
    public TaskStepRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// 根据任务 ID 获取所有工序
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工序列表</returns>
    public async Task<List<TaskStepEntity>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(step => step.TaskId == taskId)
            .OrderBy(step => step.Step)
            .ToListAsync();
    }

    /// <summary>
    /// 获取任务的当前活跃工序
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>当前活跃工序</returns>
    public async Task<TaskStepEntity?> GetActiveStepAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(step => step.TaskId == taskId)
            .OrderBy(step => step.Step)
            .FirstAsync(step => step.Status == TaskStepStatus.Pending || step.Status == TaskStepStatus.Handling);
    }

    /// <summary>
    /// 获取任务的当前工序（用于任务编排）
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>当前工序</returns>
    public async Task<TaskStepEntity?> GetCurrentStepAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(step => step.TaskId == taskId)
            .OrderBy(step => step.Step)
            .FirstAsync(step => step.Status == TaskStepStatus.Pending);
    }

    /// <summary>
    /// 获取任务的已完成工序（用于构建上下文）
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="currentStepId">当前工序 ID（排除）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>已完成工序列表</returns>
    public async Task<List<TaskStepEntity>> GetCompletedStepsAsync(Guid taskId, Guid? currentStepId = null, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = Queryable
            .Where(step => step.TaskId == taskId && step.Status == TaskStepStatus.Completed)
            .OrderBy(step => step.Step);

        if (currentStepId.HasValue)
        {
            query = query.Where(step => step.Id != currentStepId.Value);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// 获取超时的工序（用于解绑）
    /// </summary>
    /// <param name="timeoutMinutes">超时分钟数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>超时工序列表</returns>
    public async Task<List<TaskStepEntity>> GetTimeoutStepsAsync(int timeoutMinutes = 30, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var timeoutThreshold = DateTime.UtcNow.AddMinutes(-timeoutMinutes);
        return await Queryable
            .Where(step => step.Status == TaskStepStatus.Handling && step.StartedAtUtc <= timeoutThreshold)
            .OrderBy(step => step.StartedAtUtc)
            .ToListAsync();
    }

    /// <summary>
    /// 统计Agent的活跃任务数
    /// </summary>
    public async Task<int> CountAgentActiveTasksAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(step => step.WorkerAgentId == agentId && step.Status == TaskStepStatus.Handling)
            .CountAsync();
    }

    /// <summary>
    /// 将Agent分配给任务的当前步骤
    /// </summary>
    public async Task AssignAgentAsync(Guid taskId, Guid agentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await Db.Updateable<TaskStepEntity>()
            .SetColumns(step => step.WorkerAgentId == agentId)
            .SetColumns(step => step.Status == TaskStepStatus.Handling)
            .SetColumns(step => step.StartedAtUtc == DateTime.UtcNow)
            .Where(step => step.TaskId == taskId && step.Status == TaskStepStatus.Pending)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 获取失败的工序列表
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>失败工序列表</returns>
    public async Task<List<TaskStepEntity>> GetFailedStepsAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(step => step.Status == TaskStepStatus.Failed)
            .OrderByDescending(step => step.CompletedAtUtc)
            .ToListAsync();
    }

    /// <summary>
    /// 批量创建工序步骤
    /// </summary>
    /// <param name="entities">工序步骤实体列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task CreateManyAsync(List<TaskStepEntity> entities, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        foreach (var entity in entities)
        {
            entity.CreatedAtUtc = DateTime.UtcNow;
            entity.UpdatedAtUtc = DateTime.UtcNow;
        }
        await Db.Insertable(entities).ExecuteCommandAsync();
    }
}
