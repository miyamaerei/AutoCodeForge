using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Application.Tools;

/// <summary>
/// Notification tool for agent to send notifications to users.
/// Supports: approval requests, task completion notifications, etc.
/// </summary>
public class NotificationTool : IAgentTool
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationTool> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationTool"/> class.
    /// </summary>
    public NotificationTool(
        INotificationService notificationService,
        ILogger<NotificationTool> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "NotificationTool";

    /// <inheritdoc />
    public string Description => "Sends notifications: approval requests, task completion alerts, and workflow notifications.";

    /// <inheritdoc />
    public async Task<string> ExecuteAsync(
        IReadOnlyDictionary<string, string> input,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing NotificationTool with input: {InputKeys}", 
            string.Join(", ", input.Keys));

        if (!input.TryGetValue("type", out var notificationType))
        {
            throw new ArgumentException("Notification type is required");
        }

        if (!input.TryGetValue("userId", out var userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new ArgumentException("User ID is required");
        }

        var templateId = notificationType switch
        {
            "approval" => "PlanApproval",
            "completion" => "CodeReview",
            "alert" => "Emergency",
            _ => "PlanApproval"
        };

        var variables = new Dictionary<string, string>();
        if (input.TryGetValue("taskId", out var taskId))
        {
            variables["TaskId"] = taskId;
        }
        if (input.TryGetValue("actionUrl", out var actionUrl))
        {
            variables["ActionUrl"] = actionUrl;
        }

        await _notificationService.SendNotificationAsync(
            userId,
            NotificationChannel.InApp,
            templateId,
            variables,
            notificationType == "alert" ? NotificationPriority.High : NotificationPriority.Medium,
            cancellationToken);

        _logger.LogInformation("Notification sent: {Type} to UserId {UserId}", notificationType, userId);
        return $"Notification sent successfully. Type: {notificationType}, UserId: {userId}";
    }
}