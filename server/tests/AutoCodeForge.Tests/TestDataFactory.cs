/**
 * TestDataFactory - 测试数据工厂
 * 
 * 提供各种测试数据的构造方法，支持多种测试场景。
 */

using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Tests;

public static class TestDataFactory
{
    #region Agent Factory

    public static AgentEntity CreateSecretary(string? name = null)
    {
        return new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = name ?? $"Secretary_{Guid.NewGuid():N}",
            Role = AgentRole.Secretary,
            State = AgentState.Idle,
            CurrentTaskCount = 0,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public static AgentEntity CreateManager(string? name = null)
    {
        return new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = name ?? $"Manager_{Guid.NewGuid():N}",
            Role = AgentRole.Manager,
            State = AgentState.Idle,
            CurrentTaskCount = 0,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public static AgentEntity CreateWorker(string? name = null)
    {
        return new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = name ?? $"Worker_{Guid.NewGuid():N}",
            Role = AgentRole.Worker,
            State = AgentState.Idle,
            CurrentTaskCount = 0,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public static AgentEntity CreateAgentInState(AgentRole role, AgentState state)
    {
        return new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = $"{role}_{state}_{Guid.NewGuid():N}",
            Role = role,
            State = state,
            CurrentTaskCount = state == AgentState.Handling ? 1 : 0,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    #endregion

    #region Task Factory

    public static TaskEntity CreateTask(string? title = null)
    {
        return new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = title ?? $"TestTask_{Guid.NewGuid():N}",
            Description = "Test task description",
            Status = Core.Entities.TaskStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public static TaskEntity CreateTaskWithStatus(Core.Entities.TaskStatus status)
    {
        return new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = $"Task_{status}_{Guid.NewGuid():N}",
            Description = "Test task",
            Status = status,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    #endregion

    #region TaskStep Factory

    public static TaskStepEntity CreateStep(Guid taskId, int stepNumber, TaskStepType type)
    {
        return new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Step = stepNumber,
            StepType = type,
            Status = TaskStepStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    public static List<TaskStepEntity> CreateStepsForTask(Guid taskId, params TaskStepType[] types)
    {
        var steps = new List<TaskStepEntity>();
        for (int i = 0; i < types.Length; i++)
        {
            steps.Add(CreateStep(taskId, i + 1, types[i]));
        }
        return steps;
    }

    public static TaskStepEntity CreateStepWithStatus(Guid taskId, int stepNumber, TaskStepType type, TaskStepStatus status)
    {
        return new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Step = stepNumber,
            StepType = type,
            Status = status,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    #endregion

    #region HumanGate Factory

    public static HumanGateEntity CreateHumanGate(Guid taskId, Guid stepId, HumanGateType gateType)
    {
        return new HumanGateEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            TaskStepId = stepId,
            GateType = gateType,
            Status = HumanGateStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    public static HumanGateEntity CreatePendingGate(Guid taskId, Guid stepId, HumanGateType gateType)
    {
        return CreateHumanGate(taskId, stepId, gateType);
    }

    public static HumanGateEntity CreateApprovedGate(Guid taskId, Guid stepId, HumanGateType gateType)
    {
        var gate = CreateHumanGate(taskId, stepId, gateType);
        gate.Status = HumanGateStatus.Approved;
        gate.RespondedAtUtc = DateTime.UtcNow;
        return gate;
    }

    public static HumanGateEntity CreateRejectedGate(Guid taskId, Guid stepId, HumanGateType gateType)
    {
        var gate = CreateHumanGate(taskId, stepId, gateType);
        gate.Status = HumanGateStatus.Rejected;
        gate.RespondedAtUtc = DateTime.UtcNow;
        return gate;
    }

    #endregion

    #region AgentRegistration Factory

    public static AgentRegistrationEntity CreateAgentRegistration(Guid agentId, string serverId)
    {
        return new AgentRegistrationEntity
        {
            AgentId = agentId,
            ServerId = serverId,
            InstanceId = $"instance_{Guid.NewGuid():N}",
            LastHeartbeat = DateTime.UtcNow,
            Status = AgentRegistrationStatus.Online,
            RegisteredAt = DateTime.UtcNow
        };
    }

    public static AgentRegistrationEntity CreateOfflineAgentRegistration(Guid agentId, string serverId)
    {
        return new AgentRegistrationEntity
        {
            AgentId = agentId,
            ServerId = serverId,
            InstanceId = $"instance_{Guid.NewGuid():N}",
            LastHeartbeat = DateTime.UtcNow.AddMinutes(-3),
            Status = AgentRegistrationStatus.Offline,
            RegisteredAt = DateTime.UtcNow.AddHours(-1)
        };
    }

    #endregion

    #region Notification Factory

    public static NotificationEntity CreateNotification(Guid userId, NotificationChannel channel)
    {
        return new NotificationEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Channel = channel,
            Priority = NotificationPriority.Medium,
            Title = "Test Notification",
            Content = "Test notification content",
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    #endregion
}