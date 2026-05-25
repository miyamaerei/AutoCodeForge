using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Core.DTOs.Dashboard;

public class DashboardOverview
{
    public AgentStats AgentStats { get; set; } = new();
    public TaskStats TaskStats { get; set; } = new();
    public GateStats GateStats { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class AgentStats
{
    public int Total { get; set; }
    public int Idle { get; set; }
    public int Handling { get; set; }
    public int Learning { get; set; }
    public int Dormant { get; set; }
}

public class TaskStats
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Running { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
    public int Paused { get; set; }
    public int Canceled { get; set; }
}

public class GateStats
{
    public int PendingCount { get; set; }
    public Dictionary<HumanGateType, int> ByType { get; set; } = new();
}

public class PipelineStepStat
{
    public string StepType { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Running { get; set; }
    public int Completed { get; set; }
    public int Failed { get; set; }
}

public class SystemMetrics
{
    public int AgentCount { get; set; }
    public int ActiveAgents { get; set; }
    public double TotalLearningHours { get; set; }
    public double AverageLoad { get; set; }
    public int MaxLoad { get; set; }
    public DateTime? LastHeartbeat { get; set; }
    public TimeSpan UpTime { get; set; }
}

public class DashboardSnapshot
{
    public AgentStats AgentStats { get; set; } = new();
    public TaskStats TaskStats { get; set; } = new();
    public GateStats GateStats { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class DashboardTaskLive
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public int CurrentStep { get; set; }
    public string CurrentStepName { get; set; } = string.Empty;
    public Guid? AgentId { get; set; }
    public string? AgentName { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsTimeout { get; set; }
    public bool HasRejectedGate { get; set; }
    public bool HasEmergencyGate { get; set; }
    public List<string> AlertTags { get; set; } = new();
    public string AlertLevel { get; set; } = "normal";
    public DateTime UpdatedAtUtc { get; set; }
}

public class DashboardAgentLive
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int Workload { get; set; }
    public int WorkstationStep { get; set; }
    public string WorkstationName { get; set; } = string.Empty;
    public Guid? CurrentTaskId { get; set; }
    public string? DormantReason { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class DashboardLivePayload
{
    public DashboardSnapshot Snapshot { get; set; } = new();
    public List<DashboardTaskLive> Tasks { get; set; } = new();
    public List<DashboardAgentLive> Agents { get; set; } = new();
    public DateTime GeneratedAtUtc { get; set; }
}

public class DashboardLogItem
{
    public DateTime Time { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid? TaskId { get; set; }
    public Guid? AgentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Level { get; set; } = "info";
}