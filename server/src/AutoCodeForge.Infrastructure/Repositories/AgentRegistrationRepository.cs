using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

public class AgentRegistrationRepository : BaseRepository<AgentRegistrationEntity>
{
    public AgentRegistrationRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    public async Task<AgentRegistrationEntity?> GetByAgentIdAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await QueryableWithoutNtIdFilter
            .FirstAsync(r => r.AgentId == agentId);
    }

    public async Task<List<AgentRegistrationEntity>> GetOnlineAgentsAsync(CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await QueryableWithoutNtIdFilter
            .Where(r => r.Status == AgentRegistrationStatus.Online)
            .ToListAsync();
    }

    public async Task<List<AgentRegistrationEntity>> GetAgentsByServerIdAsync(string serverId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await QueryableWithoutNtIdFilter
            .Where(r => r.ServerId == serverId)
            .ToListAsync();
    }

    public async Task<List<AgentRegistrationEntity>> GetTimeoutAgentsAsync(int timeoutSeconds, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var threshold = DateTime.UtcNow.AddSeconds(-timeoutSeconds);
        return await QueryableWithoutNtIdFilter
            .Where(r => r.Status == AgentRegistrationStatus.Online && r.LastHeartbeat <= threshold)
            .ToListAsync();
    }

    public async Task<int> UpdateStatusForTimeoutAgentsAsync(int timeoutSeconds, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var threshold = DateTime.UtcNow.AddSeconds(-timeoutSeconds);
        return await Db.Updateable<AgentRegistrationEntity>()
            .SetColumns(r => r.Status == AgentRegistrationStatus.Offline)
            .Where(r => r.Status == AgentRegistrationStatus.Online && r.LastHeartbeat <= threshold)
            .ExecuteCommandAsync();
    }

    public async Task<bool> UpdateHeartbeatAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var result = await Db.Updateable<AgentRegistrationEntity>()
            .SetColumns(r => r.LastHeartbeat == DateTime.UtcNow)
            .SetColumns(r => r.Status == AgentRegistrationStatus.Online)
            .Where(r => r.AgentId == agentId)
            .ExecuteCommandAsync();
        return result > 0;
    }
}