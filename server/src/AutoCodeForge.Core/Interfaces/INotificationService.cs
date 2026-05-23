using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Core.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(
        Guid userId,
        NotificationChannel channel,
        string templateId,
        Dictionary<string, string> variables,
        NotificationPriority priority = NotificationPriority.Medium,
        CancellationToken cancellationToken = default);

    Task SendBatchNotificationsAsync(
        List<Guid> userIds,
        NotificationChannel channel,
        string templateId,
        Dictionary<string, string> variables,
        NotificationPriority priority = NotificationPriority.Medium,
        CancellationToken cancellationToken = default);

    Task RegisterChannelAsync(INotificationChannel channel, CancellationToken cancellationToken = default);

    Task<List<NotificationEntity>> GetUserNotificationsAsync(
        Guid userId,
        bool? isRead = null,
        CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface INotificationChannel
{
    NotificationChannel ChannelType { get; }
    
    Task SendAsync(NotificationEntity notification, CancellationToken cancellationToken = default);
}