using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

public class NotificationRepository : BaseRepository<NotificationEntity>
{
    public NotificationRepository(ISqlSugarClient db, ICurrentUser currentUser)
        : base(db, currentUser)
    {
    }

    public async Task<List<NotificationEntity>> GetByUserIdAsync(
        Guid userId,
        bool? isRead = null,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var query = QueryableWithoutNtIdFilter
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAtUtc);

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<int> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Db.Updateable<NotificationEntity>()
            .SetColumns(n => n.IsRead == true)
            .SetColumns(n => n.ReadAt == DateTime.UtcNow)
            .Where(n => n.Id == notificationId)
            .ExecuteCommandAsync();
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Db.Updateable<NotificationEntity>()
            .SetColumns(n => n.IsRead == true)
            .SetColumns(n => n.ReadAt == DateTime.UtcNow)
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteCommandAsync();
    }
}