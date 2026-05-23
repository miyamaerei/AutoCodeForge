using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

[SugarTable("AgentRegistrations")]
public class AgentRegistrationEntity : AuditableEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid AgentId { get; set; }

    [SugarColumn(Length = 100)]
    public string ServerId { get; set; } = string.Empty;

    [SugarColumn(Length = 100)]
    public string InstanceId { get; set; } = string.Empty;

    public DateTime LastHeartbeat { get; set; }

    public AgentRegistrationStatus Status { get; set; } = AgentRegistrationStatus.Online;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}

public enum AgentRegistrationStatus
{
    Online = 0,
    Offline = 1,
    Unknown = 2
}