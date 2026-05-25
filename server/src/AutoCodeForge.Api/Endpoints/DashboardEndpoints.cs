using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Dashboard;
using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Infrastructure.Repositories;
using System.Text.Json;

namespace AutoCodeForge.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var legacyGroup = app.MapGroup("/api/v1/dashboard");
        MapDashboardRoutes(legacyGroup);

        var stableGroup = app.MapGroup("/api/dashboard");
        MapDashboardRoutes(stableGroup);

        return app;
    }

    private static void MapDashboardRoutes(RouteGroupBuilder group)
    {
        group.MapGet("/snapshot", async (
            AgentService agentService,
            TaskService taskService,
            HumanGateService humanGateService,
            CancellationToken cancellationToken) =>
        {
            var snapshot = await FetchDashboardSnapshotAsync(
                agentService,
                taskService,
                humanGateService,
                cancellationToken);
            return Results.Ok(ApiResponse<DashboardSnapshot>.Ok(snapshot));
        });

        group.MapGet("/tasks", async (
            TaskService taskService,
            AgentService agentService,
            HumanGateRepository humanGateRepository,
            CancellationToken cancellationToken) =>
        {
            var response = await FetchDashboardTasksAsync(
                taskService,
                agentService,
                humanGateRepository,
                cancellationToken);
            return Results.Ok(ApiResponse<List<DashboardTaskLive>>.Ok(response));
        });

        group.MapGet("/agents", async (
            AgentService agentService,
            TaskService taskService,
            CancellationToken cancellationToken) =>
        {
            var response = await FetchDashboardAgentsAsync(
                agentService,
                taskService,
                cancellationToken);
            return Results.Ok(ApiResponse<List<DashboardAgentLive>>.Ok(response));
        });

        group.MapGet("/live/stream", async (
            int? intervalMs,
            AgentService agentService,
            TaskService taskService,
            HumanGateService humanGateService,
            HumanGateRepository humanGateRepository,
            HttpContext context,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var effectiveIntervalMs = Math.Clamp(intervalMs ?? 5000, 1000, 30000);

            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");
            context.Response.Headers.Append("X-Accel-Buffering", "no");
            context.Response.ContentType = "text/event-stream";

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                context.RequestAborted);
            var linkedToken = linkedCts.Token;

            await WriteSseEventAsync(
                context,
                "connected",
                JsonSerializer.Serialize(new { intervalMs = effectiveIntervalMs }),
                linkedToken);

            while (!linkedToken.IsCancellationRequested)
            {
                try
                {
                    var agentsTask = agentService.GetAllAsync(linkedToken);
                    var tasksTask = taskService.GetAllAsync(linkedToken);
                    var pendingGatesTask = humanGateService.GetPendingGatesAsync(linkedToken);
                    var gatesTask = humanGateRepository.GetAllAsync(true, linkedToken);

                    await Task.WhenAll(agentsTask, tasksTask, pendingGatesTask, gatesTask);

                    var agents = await agentsTask;
                    var tasks = await tasksTask;
                    var pendingGates = await pendingGatesTask;
                    var gates = await gatesTask;

                    var payload = new DashboardLivePayload
                    {
                        Snapshot = BuildSnapshot(agents, tasks, pendingGates),
                        Tasks = BuildDashboardTasks(tasks, agents, gates),
                        Agents = BuildDashboardAgents(agents, tasks),
                        GeneratedAtUtc = DateTime.UtcNow,
                    };

                    await WriteSseEventAsync(
                        context,
                        "live",
                        JsonSerializer.Serialize(payload),
                        linkedToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to stream dashboard live payload.");
                    await WriteSseEventAsync(
                        context,
                        "error",
                        JsonSerializer.Serialize(new { message = "Failed to fetch dashboard live payload." }),
                        linkedToken);
                }

                try
                {
                    await Task.Delay(effectiveIntervalMs, linkedToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            return Results.Empty;
        });

        group.MapGet("/logs", async (
            string? type,
            Guid? taskId,
            Guid? agentId,
            TaskLogRepository taskLogRepository,
            AgentLearningRecordRepository learningRepository,
            AgentDormantRecordRepository dormantRepository,
            HumanGateRepository humanGateRepository,
            TaskService taskService,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var logs = await FetchDashboardLogsAsync(
                    type,
                    taskId,
                    agentId,
                    taskLogRepository,
                    learningRepository,
                    dormantRepository,
                    humanGateRepository,
                    taskService,
                    cancellationToken);

                return Results.Ok(ApiResponse<List<DashboardLogItem>>.Ok(logs));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ApiResponse<List<DashboardLogItem>>.Fail(ex.Message));
            }
        });

        group.MapGet("/logs/stream", async (
            string? type,
            Guid? taskId,
            Guid? agentId,
            int? intervalMs,
            TaskLogRepository taskLogRepository,
            AgentLearningRecordRepository learningRepository,
            AgentDormantRecordRepository dormantRepository,
            HumanGateRepository humanGateRepository,
            TaskService taskService,
            HttpContext context,
            ILogger<Program> logger,
            CancellationToken cancellationToken) =>
        {
            var normalizedType = (type ?? "task").Trim().ToLowerInvariant();
            if (normalizedType is not ("task" or "agent" or "system"))
            {
                return Results.BadRequest(ApiResponse<List<DashboardLogItem>>.Fail("type must be task, agent, or system"));
            }

            var effectiveIntervalMs = Math.Clamp(intervalMs ?? 3000, 1000, 30000);

            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");
            context.Response.Headers.Append("X-Accel-Buffering", "no");
            context.Response.ContentType = "text/event-stream";

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                context.RequestAborted);
            var linkedToken = linkedCts.Token;

            await WriteSseEventAsync(
                context,
                "connected",
                JsonSerializer.Serialize(new { intervalMs = effectiveIntervalMs, type = normalizedType }),
                linkedToken);

            while (!linkedToken.IsCancellationRequested)
            {
                try
                {
                    var logs = await FetchDashboardLogsAsync(
                        normalizedType,
                        taskId,
                        agentId,
                        taskLogRepository,
                        learningRepository,
                        dormantRepository,
                        humanGateRepository,
                        taskService,
                        linkedToken);

                    await WriteSseEventAsync(
                        context,
                        "logs",
                        JsonSerializer.Serialize(logs),
                        linkedToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to stream dashboard logs payload.");
                    await WriteSseEventAsync(
                        context,
                        "error",
                        JsonSerializer.Serialize(new { message = "Failed to fetch dashboard logs payload." }),
                        linkedToken);
                }

                try
                {
                    await Task.Delay(effectiveIntervalMs, linkedToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            return Results.Empty;
        });

        group.MapGet("/overview", async (
            AgentService agentService,
            TaskService taskService,
            HumanGateService humanGateService,
            CancellationToken cancellationToken) =>
        {
            var agentsTask = agentService.GetAllAsync(cancellationToken);
            var tasksTask = taskService.GetAllAsync(cancellationToken);
            var pendingGatesTask = humanGateService.GetPendingGatesAsync(cancellationToken);

            await Task.WhenAll(agentsTask, tasksTask, pendingGatesTask);

            var agents = await agentsTask;
            var tasks = await tasksTask;
            var pendingGates = await pendingGatesTask;

            var snapshot = BuildSnapshot(agents, tasks, pendingGates);
            var overview = new DashboardOverview
            {
                AgentStats = snapshot.AgentStats,
                TaskStats = snapshot.TaskStats,
                GateStats = snapshot.GateStats,
                LastUpdated = snapshot.LastUpdated,
            };

            return Results.Ok(ApiResponse<DashboardOverview>.Ok(overview));
        });

        group.MapGet("/pipeline-stats", async (
            TaskStepRepository stepRepository,
            CancellationToken cancellationToken) =>
        {
            var steps = await stepRepository.GetAllAsync(true, cancellationToken);
            
            var pipelineStats = steps
                .GroupBy(s => s.StepType)
                .Select(g => new PipelineStepStat
                {
                    StepType = g.Key.ToString(),
                    Total = g.Count(),
                    Pending = g.Count(s => s.Status == TaskStepStatus.Pending),
                    Running = g.Count(s => s.Status == TaskStepStatus.Handling),
                    Completed = g.Count(s => s.Status == TaskStepStatus.Completed),
                    Failed = g.Count(s => s.Status == TaskStepStatus.Failed)
                })
                .ToList();

            return Results.Ok(ApiResponse<List<PipelineStepStat>>.Ok(pipelineStats));
        });

        group.MapGet("/system-metrics", async (
            AgentRepository agentRepository,
            AgentLearningRecordRepository learningRecordRepository,
            CancellationToken cancellationToken) =>
        {
            var agents = await agentRepository.GetAllAsync(true, cancellationToken);
            var learningRecords = await learningRecordRepository.GetAllAsync(true, cancellationToken);

            var metrics = new SystemMetrics
            {
                AgentCount = agents.Count,
                ActiveAgents = agents.Count(a => a.State != AgentState.Dormant),
                TotalLearningHours = learningRecords.Sum(r => (r.CompletedAtUtc - r.StartedAtUtc)?.TotalHours ?? 0),
                AverageLoad = agents.Any() ? agents.Average(a => a.CurrentTaskCount) : 0,
                MaxLoad = agents.Any() ? agents.Max(a => a.CurrentTaskCount) : 0,
                LastHeartbeat = null,
                UpTime = DateTime.UtcNow - new DateTime(2024, 1, 1)
            };

            return Results.Ok(ApiResponse<SystemMetrics>.Ok(metrics));
        });

        group.MapGet("/recent-tasks", async (
            TaskService taskService,
            CancellationToken cancellationToken) =>
        {
            var tasks = await taskService.GetRecentTasksAsync(20, cancellationToken);
            return Results.Ok(ApiResponse<List<Core.DTOs.Task.TaskResponse>>.Ok(tasks));
        });

        group.MapGet("/agent-list", async (
            AgentService agentService,
            CancellationToken cancellationToken) =>
        {
            var agents = await agentService.GetAllAsync(cancellationToken);
            return Results.Ok(ApiResponse<List<Core.DTOs.Agent.AgentResponse>>.Ok(agents));
        });

    }

    private static async Task<DashboardSnapshot> FetchDashboardSnapshotAsync(
        AgentService agentService,
        TaskService taskService,
        HumanGateService humanGateService,
        CancellationToken cancellationToken)
    {
        var agentsTask = agentService.GetAllAsync(cancellationToken);
        var tasksTask = taskService.GetAllAsync(cancellationToken);
        var pendingGatesTask = humanGateService.GetPendingGatesAsync(cancellationToken);

        await Task.WhenAll(agentsTask, tasksTask, pendingGatesTask);

        var agents = await agentsTask;
        var tasks = await tasksTask;
        var pendingGates = await pendingGatesTask;

        return BuildSnapshot(agents, tasks, pendingGates);
    }

    private static async Task<List<DashboardTaskLive>> FetchDashboardTasksAsync(
        TaskService taskService,
        AgentService agentService,
        HumanGateRepository humanGateRepository,
        CancellationToken cancellationToken)
    {
        var tasksTask = taskService.GetAllAsync(cancellationToken);
        var agentsTask = agentService.GetAllAsync(cancellationToken);
        var gatesTask = humanGateRepository.GetAllAsync(true, cancellationToken);
        await Task.WhenAll(tasksTask, agentsTask, gatesTask);

        var tasks = await tasksTask;
        var agents = await agentsTask;
        var gates = await gatesTask;
        return BuildDashboardTasks(tasks, agents, gates);
    }

    private static async Task<List<DashboardAgentLive>> FetchDashboardAgentsAsync(
        AgentService agentService,
        TaskService taskService,
        CancellationToken cancellationToken)
    {
        var agentsTask = agentService.GetAllAsync(cancellationToken);
        var tasksTask = taskService.GetAllAsync(cancellationToken);
        await Task.WhenAll(agentsTask, tasksTask);

        var agents = await agentsTask;
        var tasks = await tasksTask;
        return BuildDashboardAgents(agents, tasks);
    }

    private static async Task<List<DashboardLogItem>> FetchDashboardLogsAsync(
        string? type,
        Guid? taskId,
        Guid? agentId,
        TaskLogRepository taskLogRepository,
        AgentLearningRecordRepository learningRepository,
        AgentDormantRecordRepository dormantRepository,
        HumanGateRepository humanGateRepository,
        TaskService taskService,
        CancellationToken cancellationToken)
    {
        var normalizedType = (type ?? "task").Trim().ToLowerInvariant();

        if (normalizedType == "task")
        {
            var tasks = await taskService.GetAllAsync(cancellationToken);
            var taskMap = tasks.ToDictionary(t => t.Id, t => t);
            var taskLogs = await taskLogRepository.GetAllAsync(true, cancellationToken);

            var filteredTaskLogs = taskLogs.AsEnumerable();
            if (taskId.HasValue)
            {
                filteredTaskLogs = filteredTaskLogs.Where(log => log.TaskId == taskId.Value);
            }

            if (agentId.HasValue)
            {
                filteredTaskLogs = filteredTaskLogs.Where(log =>
                    taskMap.TryGetValue(log.TaskId, out var task) && task.AgentId == agentId.Value);
            }

            return filteredTaskLogs
                .OrderByDescending(log => log.CreatedAtUtc)
                .Take(200)
                .Select(log => new DashboardLogItem
                {
                    Time = log.CreatedAtUtc,
                    Type = "task",
                    TaskId = log.TaskId,
                    AgentId = taskMap.TryGetValue(log.TaskId, out var task) ? task.AgentId : null,
                    Content = log.Message,
                    Level = NormalizeLevel(log.Level),
                })
                .ToList();
        }

        if (normalizedType == "agent")
        {
            var learningRecords = await learningRepository.GetAllAsync(true, cancellationToken);
            var dormantRecords = await dormantRepository.GetAllAsync(true, cancellationToken);

            var learningLogs = learningRecords.Select(record => new DashboardLogItem
            {
                Time = record.CreatedAtUtc,
                Type = "agent",
                TaskId = record.RelatedTaskId,
                AgentId = record.AgentId,
                Content = $"Agent学习事件: {record.TriggerType}"
                    + (string.IsNullOrWhiteSpace(record.TriggerReason) ? string.Empty : $" - {record.TriggerReason}"),
                Level = record.IsSuccessful ? "info" : "error",
            });

            var dormantLogs = dormantRecords.Select(record => new DashboardLogItem
            {
                Time = record.CreatedAtUtc,
                Type = "agent",
                TaskId = null,
                AgentId = record.AgentId,
                Content = record.IsWoken
                    ? $"Agent从休眠恢复: {record.ReasonType}"
                    : $"Agent进入休眠: {record.ReasonType}",
                Level = record.IsWoken ? "info" : "warning",
            });

            return learningLogs
                .Concat(dormantLogs)
                .Where(log => !agentId.HasValue || log.AgentId == agentId.Value)
                .OrderByDescending(log => log.Time)
                .Take(200)
                .ToList();
        }

        if (normalizedType == "system")
        {
            var tasks = await taskService.GetAllAsync(cancellationToken);
            var gates = await humanGateRepository.GetAllAsync(true, cancellationToken);

            var gateLogs = gates.Select(gate => new DashboardLogItem
            {
                Time = gate.UpdatedAtUtc,
                Type = "system",
                TaskId = gate.TaskId,
                AgentId = null,
                Content = $"门控事件: {gate.GateType} - {gate.Status}",
                Level = gate.Status switch
                {
                    HumanGateStatus.Rejected => "error",
                    HumanGateStatus.Timeout => "warning",
                    _ => "info",
                },
            });

            var taskFailureLogs = tasks
                .Where(task => task.Status == AutoCodeForge.Core.Entities.TaskStatus.Failed)
                .Select(task => new DashboardLogItem
                {
                    Time = task.UpdatedAtUtc,
                    Type = "system",
                    TaskId = task.Id,
                    AgentId = task.AgentId,
                    Content = $"任务异常: {task.Title}",
                    Level = "error",
                });

            return gateLogs
                .Concat(taskFailureLogs)
                .Where(log => !taskId.HasValue || log.TaskId == taskId.Value)
                .OrderByDescending(log => log.Time)
                .Take(200)
                .ToList();
        }

        throw new ArgumentException("type must be task, agent, or system");
    }

    private static List<DashboardTaskLive> BuildDashboardTasks(
        List<TaskEntity> tasks,
        List<Core.DTOs.Agent.AgentResponse> agents,
        List<HumanGateEntity> gates)
    {
        var agentMap = agents.ToDictionary(a => a.Id, a => a.Name);
        var gateMap = gates.GroupBy(gate => gate.TaskId).ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());
        var nowUtc = DateTime.UtcNow;

        return tasks
            .OrderByDescending(t => t.UpdatedAtUtc)
            .Select(task =>
            {
                var step = NormalizeStep(task.CurrentStep);
                gateMap.TryGetValue(task.Id, out var taskGates);
                taskGates ??= new List<HumanGateEntity>();

                var isTimeout = task.DueAtUtc.HasValue
                    && task.DueAtUtc.Value <= nowUtc
                    && task.Status is Core.Entities.TaskStatus.Pending or Core.Entities.TaskStatus.Running;
                var hasRejectedGate = taskGates.Any(gate => gate.Status == HumanGateStatus.Rejected);
                var hasEmergencyGate = taskGates.Any(gate =>
                    gate.GateType == HumanGateType.Emergency
                    && gate.Status is not HumanGateStatus.Cancelled);

                var alertTags = new List<string>();
                if (isTimeout)
                {
                    alertTags.Add("timeout");
                }

                if (hasRejectedGate)
                {
                    alertTags.Add("rejected");
                }

                if (hasEmergencyGate)
                {
                    alertTags.Add("emergency");
                }

                if (task.Status is Core.Entities.TaskStatus.Failed or Core.Entities.TaskStatus.Canceled)
                {
                    alertTags.Add("failed");
                }

                var alertLevel = alertTags.Contains("failed") || alertTags.Contains("timeout") || alertTags.Contains("rejected")
                    ? "critical"
                    : alertTags.Contains("emergency")
                        ? "warning"
                        : "normal";

                return new DashboardTaskLive
                {
                    Id = task.Id,
                    Title = task.Title,
                    Status = task.Status.ToString(),
                    Progress = task.Progress,
                    CurrentStep = step,
                    CurrentStepName = GetStepName(step),
                    AgentId = task.AgentId,
                    AgentName = task.AgentId.HasValue && agentMap.TryGetValue(task.AgentId.Value, out var name)
                        ? name
                        : null,
                    ErrorMessage = task.ErrorMessage,
                    IsTimeout = isTimeout,
                    HasRejectedGate = hasRejectedGate,
                    HasEmergencyGate = hasEmergencyGate,
                    AlertTags = alertTags,
                    AlertLevel = alertLevel,
                    UpdatedAtUtc = task.UpdatedAtUtc,
                };
            })
            .ToList();
    }

    private static List<DashboardAgentLive> BuildDashboardAgents(
        List<Core.DTOs.Agent.AgentResponse> agents,
        List<TaskEntity> tasks)
    {
        var taskMap = tasks.ToDictionary(t => t.Id, t => t);
        var workloadMap = tasks
            .Where(task => task.AgentId.HasValue && task.Status == Core.Entities.TaskStatus.Running)
            .GroupBy(task => task.AgentId!.Value)
            .ToDictionary(group => group.Key, group => group.Count());

        return agents
            .OrderBy(a => a.Name)
            .Select(agent =>
            {
                var workstationStep = 1;
                if (agent.CurrentTaskId.HasValue && taskMap.TryGetValue(agent.CurrentTaskId.Value, out var currentTask))
                {
                    workstationStep = NormalizeStep(currentTask.CurrentStep);
                }

                return new DashboardAgentLive
                {
                    Id = agent.Id,
                    Name = agent.Name,
                    Role = agent.Role.ToString(),
                    State = agent.State.ToString(),
                    Workload = workloadMap.TryGetValue(agent.Id, out var workload) ? workload : 0,
                    WorkstationStep = workstationStep,
                    WorkstationName = GetStepName(workstationStep),
                    CurrentTaskId = agent.CurrentTaskId,
                    DormantReason = agent.DormantReason,
                    UpdatedAtUtc = agent.UpdatedAtUtc,
                };
            })
            .ToList();
    }

    private static async Task WriteSseEventAsync(
        HttpContext context,
        string eventName,
        string data,
        CancellationToken cancellationToken)
    {
        await context.Response.WriteAsync($"event: {eventName}\n", cancellationToken);
        await context.Response.WriteAsync($"data: {data}\n\n", cancellationToken);
        await context.Response.Body.FlushAsync(cancellationToken);
    }

    private static DashboardSnapshot BuildSnapshot(
        List<Core.DTOs.Agent.AgentResponse> agents,
        List<TaskEntity> tasks,
        List<HumanGateResponse> pendingGates)
    {
        var agentStats = new AgentStats
        {
            Total = agents.Count,
            Idle = agents.Count(a => a.State == AgentState.Idle.ToString()),
            Handling = agents.Count(a => a.State == AgentState.Handling.ToString()),
            Learning = agents.Count(a => a.State == AgentState.Learning.ToString()),
            Dormant = agents.Count(a => a.State == AgentState.Dormant.ToString()),
        };

        var taskStats = new TaskStats
        {
            Total = tasks.Count,
            Pending = tasks.Count(t => t.Status == Core.Entities.TaskStatus.Pending),
            Running = tasks.Count(t => t.Status == Core.Entities.TaskStatus.Running),
            Completed = tasks.Count(t => t.Status == Core.Entities.TaskStatus.Completed),
            Failed = tasks.Count(t => t.Status == Core.Entities.TaskStatus.Failed),
            Paused = tasks.Count(t => t.Status == Core.Entities.TaskStatus.Paused),
            Canceled = tasks.Count(t => t.Status == Core.Entities.TaskStatus.Canceled),
        };

        var gateStats = new GateStats
        {
            PendingCount = pendingGates.Count,
            ByType = pendingGates
                .GroupBy(g => g.GateType)
                .ToDictionary(g => Enum.Parse<HumanGateType>(g.Key), g => g.Count()),
        };

        return new DashboardSnapshot
        {
            AgentStats = agentStats,
            TaskStats = taskStats,
            GateStats = gateStats,
            LastUpdated = DateTime.UtcNow,
        };
    }

    private static int NormalizeStep(int? step)
    {
        if (!step.HasValue)
        {
            return 1;
        }

        if (step.Value < 1)
        {
            return 1;
        }

        if (step.Value > 7)
        {
            return 7;
        }

        return step.Value;
    }

    private static string GetStepName(int step)
    {
        return step switch
        {
            1 => "需求梳理",
            2 => "信息查询",
            3 => "方案制定",
            4 => "代码开发",
            5 => "测试校验",
            6 => "版本提交",
            7 => "最终审核",
            _ => "需求梳理",
        };
    }

    private static string NormalizeLevel(string? level)
    {
        if (string.IsNullOrWhiteSpace(level))
        {
            return "info";
        }

        return level.Trim().ToLowerInvariant();
    }
}