using AutoCodeForge.Application.Models;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

namespace AutoCodeForge.Application.Services;

public class NotificationService : INotificationService
{
    private readonly NotificationRepository _notificationRepository;
    private readonly ITaskEventPublisher _eventPublisher;
    private readonly NotificationTemplateSettings _templateSettings;
    private readonly List<INotificationChannel> _channels = new();
    private readonly Dictionary<string, DateTime> _lastNotificationTimes = new();
    private readonly object _lock = new();
    private readonly int _debounceSeconds = 30;

    public NotificationService(
        NotificationRepository notificationRepository,
        ITaskEventPublisher eventPublisher,
        IOptions<NotificationTemplateSettings> templateSettings,
        IEnumerable<INotificationChannel> channels)
    {
        _notificationRepository = notificationRepository;
        _eventPublisher = eventPublisher;
        _templateSettings = templateSettings.Value;
        _channels.AddRange(channels);
    }

    public async Task SendNotificationAsync(
        Guid userId,
        NotificationChannel channel,
        string templateId,
        Dictionary<string, string> variables,
        NotificationPriority priority = NotificationPriority.Medium,
        CancellationToken cancellationToken = default)
    {
        var template = _templateSettings.GetTemplate(templateId);
        if (template == null)
        {
            throw new ArgumentException($"Template {templateId} not found");
        }

        if (!ShouldSendNotification(templateId, userId))
        {
            return;
        }

        var notification = new NotificationEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Channel = channel,
            Priority = priority,
            Title = template.ResolveSubject(variables),
            Content = template.ResolveContent(variables),
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow,
            TaskId = variables.TryGetValue("TaskId", out var taskId) ? Guid.Parse(taskId) : null,
            GateId = variables.TryGetValue("GateId", out var gateId) ? Guid.Parse(gateId) : null,
            ActionUrl = variables.TryGetValue("ActionUrl", out var url) ? url : string.Empty
        };

        var channelHandler = _channels.FirstOrDefault(c => c.ChannelType == channel);
        if (channelHandler != null)
        {
            await channelHandler.SendAsync(notification, cancellationToken);
        }
    }

    public async Task SendBatchNotificationsAsync(
        List<Guid> userIds,
        NotificationChannel channel,
        string templateId,
        Dictionary<string, string> variables,
        NotificationPriority priority = NotificationPriority.Medium,
        CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds)
        {
            await SendNotificationAsync(userId, channel, templateId, variables, priority, cancellationToken);
        }
    }

    public Task RegisterChannelAsync(INotificationChannel channel, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var existing = _channels.FirstOrDefault(c => c.ChannelType == channel.ChannelType);
        if (existing == null)
        {
            _channels.Add(channel);
        }
        return Task.CompletedTask;
    }

    public Task<List<NotificationEntity>> GetUserNotificationsAsync(
        Guid userId,
        bool? isRead = null,
        CancellationToken cancellationToken = default)
    {
        return _notificationRepository.GetByUserIdAsync(userId, isRead, cancellationToken);
    }

    public Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        return _notificationRepository.MarkAsReadAsync(notificationId, cancellationToken);
    }

    public Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _notificationRepository.MarkAllAsReadAsync(userId, cancellationToken);
    }

    private bool ShouldSendNotification(string templateId, Guid userId)
    {
        var key = $"{templateId}_{userId}";
        
        lock (_lock)
        {
            if (_lastNotificationTimes.TryGetValue(key, out var lastTime))
            {
                if ((DateTime.UtcNow - lastTime).TotalSeconds < _debounceSeconds)
                {
                    return false;
                }
            }
            
            _lastNotificationTimes[key] = DateTime.UtcNow;
            return true;
        }
    }
}