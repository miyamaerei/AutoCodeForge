using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Repository for HumanGateEntity operations.
/// </summary>
public class HumanGateRepository : BaseRepository<HumanGateEntity>
{
    public HumanGateRepository(ISqlSugarClient db, ICurrentUser currentUser) : base(db, currentUser)
    {
    }

    public async Task<List<HumanGateEntity>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(gate => gate.TaskId == taskId)
            .OrderByDescending(gate => gate.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<HumanGateEntity?> GetPendingByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(gate => gate.TaskId == taskId && gate.Status == HumanGateStatus.Pending)
            .FirstAsync();
    }

    public async Task<List<HumanGateEntity>> GetPendingGatesAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(gate => gate.Status == HumanGateStatus.Pending)
            .OrderBy(gate => gate.CreatedAtUtc)
            .ToListAsync();
    }
}