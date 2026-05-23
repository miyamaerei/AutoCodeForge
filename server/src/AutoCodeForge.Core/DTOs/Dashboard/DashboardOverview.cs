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