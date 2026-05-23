using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

[SugarTable("Notifications")]
public class NotificationEntity : AuditableEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public NotificationChannel Channel { get; set; }

    public NotificationPriority Priority { get; set; } = NotificationPriority.Medium;

    [SugarColumn(Length = 200)]
    public string Title { get; set; } = string.Empty;

    [SugarColumn(Length = 2000)]
    public string Content { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    public DateTime? ReadAt { get; set; }

    public Guid? TaskId { get; set; }

    public Guid? GateId { get; set; }

    [SugarColumn(Length = 100)]
    public string ActionUrl { get; set; } = string.Empty;
}

public enum NotificationChannel
{
    InApp = 0,
    Email = 1,
    Webhook = 2,
    IM = 3
}

public enum NotificationPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}