using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Infrastructure.Notification.Channels;

public class InAppNotificationChannel : INotificationChannel
{
    private readonly NotificationRepository _notificationRepository;

    public InAppNotificationChannel(NotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public NotificationChannel ChannelType => NotificationChannel.InApp;

    public async Task SendAsync(NotificationEntity notification, CancellationToken cancellationToken = default)
    {
        await _notificationRepository.CreateAsync(notification, cancellationToken);
    }
}