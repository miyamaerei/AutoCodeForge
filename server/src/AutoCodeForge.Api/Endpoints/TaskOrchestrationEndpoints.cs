using AutoCodeForge.Application.Configuration;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AutoCodeForge.Api.Endpoints;

public static class TaskOrchestrationEndpoints
{
    public static IEndpointRouteBuilder MapTaskOrchestrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orchestration");

        group.MapPost("/tasks/{taskId:guid}/assign", async (
            Guid taskId,
            [FromBody] OrchestrationAssignRequest request,
            TaskOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
        {
            var (agent, usedEscalation) = await orchestrator.AssignTaskAsync(
                taskId, 
                request.Role, 
                cancellationToken);

            if (agent == null)
            {
                return Results.Ok(ApiResponse<object?>.Fail("No available agent found"));
            }

            return Results.Ok(ApiResponse<OrchestrationAssignResponse>.Ok(new OrchestrationAssignResponse
            {
                TaskId = taskId,
                AgentId = agent.Id,
                AgentName = agent.Name,
                UsedEscalation = usedEscalation
            }));
        });

        group.MapPost("/tasks/{taskId:guid}/reassign", async (
            Guid taskId,
            [FromBody] OrchestrationReassignRequest request,
            TaskOrchestrator orchestrator,
            CancellationToken cancellationToken) =>
        {
            var (agent, usedEscalation) = await orchestrator.ReassignTaskAsync(
                taskId, 
                request.CurrentAgentId, 
                cancellationToken);

            if (agent == null)
            {
                return Results.Ok(ApiResponse<object?>.Fail("No available agent found for reassignment"));
            }

            return Results.Ok(ApiResponse<OrchestrationAssignResponse>.Ok(new OrchestrationAssignResponse
            {
                TaskId = taskId,
                AgentId = agent.Id,
                AgentName = agent.Name,
                UsedEscalation = usedEscalation
            }));
        });

        group.MapGet("/settings", (IOptions<OrchestrationSettings> settings) =>
        {
            return Results.Ok(ApiResponse<OrchestrationSettings>.Ok(settings.Value));
        });

        return app;
    }
}

public class OrchestrationAssignRequest
{
    public AgentRole Role { get; set; }
}

public class OrchestrationReassignRequest
{
    public Guid CurrentAgentId { get; set; }
}

public class OrchestrationAssignResponse
{
    public Guid TaskId { get; set; }
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public bool UsedEscalation { get; set; }
}