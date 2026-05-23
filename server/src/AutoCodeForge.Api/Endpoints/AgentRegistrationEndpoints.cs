using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoCodeForge.Api.Endpoints;

public static class AgentRegistrationEndpoints
{
    public static IEndpointRouteBuilder MapAgentRegistrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/agents");

        group.MapPost("/register", async (
            [FromBody] RegisterAgentRequest request,
            IAgentRegistryService registryService,
            CancellationToken cancellationToken) =>
        {
            var registration = await registryService.RegisterAgentAsync(
                request.AgentId,
                request.ServerId,
                request.InstanceId,
                cancellationToken);

            return Results.Created($"/api/v1/agents/{registration.AgentId}", registration);
        });

        group.MapPut("/heartbeat", async (
            [FromBody] HeartbeatRequest request,
            IAgentRegistryService registryService,
            CancellationToken cancellationToken) =>
        {
            var success = await registryService.RenewHeartbeatAsync(request.AgentId, cancellationToken);
            if (!success)
            {
                return Results.NotFound(new { Message = "Agent not found or not registered" });
            }
            return Results.Ok(new { Message = "Heartbeat updated successfully" });
        });

        group.MapDelete("/{agentId:guid}", async (
            Guid agentId,
            IAgentRegistryService registryService,
            CancellationToken cancellationToken) =>
        {
            var success = await registryService.DeregisterAgentAsync(agentId, cancellationToken);
            if (!success)
            {
                return Results.NotFound(new { Message = "Agent not found" });
            }
            return Results.Ok(new { Message = "Agent deregistered successfully" });
        });

        group.MapGet("/available", async (
            IAgentRegistryService registryService,
            CancellationToken cancellationToken) =>
        {
            var agents = await registryService.GetAvailableAgentsAsync(cancellationToken);
            return Results.Ok(agents);
        });

        group.MapGet("/{agentId:guid}", async (
            Guid agentId,
            IAgentRegistryService registryService,
            CancellationToken cancellationToken) =>
        {
            var registration = await registryService.GetAgentRegistrationAsync(agentId, cancellationToken);
            if (registration == null)
            {
                return Results.NotFound();
            }
            return Results.Ok(registration);
        });

        return app;
    }
}

public class RegisterAgentRequest
{
    public Guid AgentId { get; set; }
    public string ServerId { get; set; } = string.Empty;
    public string InstanceId { get; set; } = string.Empty;
}

public class HeartbeatRequest
{
    public Guid AgentId { get; set; }
}