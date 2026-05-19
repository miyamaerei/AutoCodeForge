using SqlSugar;

namespace __ProjectName__.Entities;

[SugarTable("ChatSessions")]
public class ChatSessionEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NtId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public Guid? AgentId { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[SugarTable("ChatMessages")]
public class ChatMessageEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NtId { get; set; } = string.Empty;

    public Guid SessionId { get; set; }

    public MessageType Type { get; set; } = MessageType.User;

    public string Content { get; set; } = string.Empty;

    public string? MetadataJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum MessageType
{
    User,
    AI,
    System
}
