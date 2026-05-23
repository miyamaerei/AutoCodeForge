using AutoCodeForge.Application.Models;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AutoCodeForge.Api.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications");

        group.MapPost("/send", async (
            [FromBody] SendNotificationRequest request,
            INotificationService notificationService,
            CancellationToken cancellationToken) =>
        {
            await notificationService.SendNotificationAsync(
                request.UserId,
                request.Channel,
                request.TemplateId,
                request.Variables ?? new Dictionary<string, string>(),
                request.Priority,
                cancellationToken);

            return Results.Ok(new { Message = "Notification sent successfully" });
        });

        group.MapGet("/user/{userId:guid}", async (
            Guid userId,
            [FromQuery] bool? isRead,
            INotificationService notificationService,
            CancellationToken cancellationToken) =>
        {
            var notifications = await notificationService.GetUserNotificationsAsync(userId, isRead, cancellationToken);
            return Results.Ok(notifications);
        });

        group.MapPut("/{notificationId:guid}/read", async (
            Guid notificationId,
            INotificationService notificationService,
            CancellationToken cancellationToken) =>
        {
            await notificationService.MarkAsReadAsync(notificationId, cancellationToken);
            return Results.Ok(new { Message = "Notification marked as read" });
        });

        group.MapPut("/user/{userId:guid}/read-all", async (
            Guid userId,
            INotificationService notificationService,
            CancellationToken cancellationToken) =>
        {
            await notificationService.MarkAllAsReadAsync(userId, cancellationToken);
            return Results.Ok(new { Message = "All notifications marked as read" });
        });

        group.MapGet("/templates", (IOptions<NotificationTemplateSettings> templateSettings) =>
        {
            return Results.Ok(templateSettings.Value.Templates);
        });

        return app;
    }
}

public class SendNotificationRequest
{
    public Guid UserId { get; set; }
    public NotificationChannel Channel { get; set; }
    public string TemplateId { get; set; } = string.Empty;
    public Dictionary<string, string>? Variables { get; set; }
    public NotificationPriority Priority { get; set; } = NotificationPriority.Medium;
}