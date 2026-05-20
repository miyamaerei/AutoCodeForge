using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Provides persistence for admin cross-tenant audit log entries.
/// </summary>
public class AdminAuditLogRepository : BaseRepository<AdminAuditLogEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdminAuditLogRepository"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="currentUser">The current user provider.</param>
    public AdminAuditLogRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    /// <summary>
    /// Gets paged audit log entries, ordered by most recent first.
    /// </summary>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="adminNtId">Optional filter by admin NtId.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged audit log entries.</returns>
    public async Task<(List<AdminAuditLogEntity> Items, int Total)> GetPagedLogsAsync(
        int page,
        int pageSize,
        string? adminNtId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        var query = Db.Queryable<AdminAuditLogEntity>();
        if (!string.IsNullOrWhiteSpace(adminNtId))
        {
            query = query.Where(l => l.AdminNtId == adminNtId);
        }

        var normalizedPage = page <= 0 ? 1 : page;
        var normalizedSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);
        RefAsync<int> total = 0;
        var items = await query
            .OrderByDescending(l => l.OccurredAtUtc)
            .ToPageListAsync(normalizedPage, normalizedSize, total);

        return (items, total.Value);
    }
}
