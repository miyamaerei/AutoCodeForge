using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Dashboard;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/dashboard");

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

            var agentStats = new AgentStats
            {
                Total = agents.Count,
                Idle = agents.Count(a => a.State == "Idle"),
                Handling = agents.Count(a => a.State == "Handling"),
                Learning = agents.Count(a => a.State == "Learning"),
                Dormant = agents.Count(a => a.State == "Dormant")
            };

            var taskStats = new TaskStats
            {
                Total = tasks.Count,
                Pending = tasks.Count(t => t.Status == Core.Entities.TaskStatus.Pending),
                Running = tasks.Count(t => t.Status == Core.Entities.TaskStatus.Running),
                Completed = tasks.Count(t => t.Status == Core.Entities.TaskStatus.Completed),
                Failed = tasks.Count(t => t.Status == Core.Entities.TaskStatus.Failed),
                Paused = tasks.Count(t => t.Status == Core.Entities.TaskStatus.Paused),
                Canceled = tasks.Count(t => t.Status == Core.Entities.TaskStatus.Canceled)
            };

            var gateStats = new GateStats
            {
                PendingCount = pendingGates.Count,
                ByType = pendingGates
                    .GroupBy(g => g.GateType)
                    .ToDictionary(g => Enum.Parse<HumanGateType>(g.Key), g => g.Count())
            };

            var overview = new DashboardOverview
            {
                AgentStats = agentStats,
                TaskStats = taskStats,
                GateStats = gateStats,
                LastUpdated = DateTime.UtcNow
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

        return app;
    }
}