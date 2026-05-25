using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutoCodeForge.Api.Endpoints;

public static class AgentCommunicationEndpoints
{
    public static IEndpointRouteBuilder MapAgentCommunicationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/communication");

        group.MapPost("/events", async (
            [FromBody] EventPublishRequest request,
            ITaskEventPublisher publisher,
            CancellationToken cancellationToken) =>
        {
            await publisher.PublishFailureAsync(
                request.TaskId, 
                request.StepId, 
                "Manual", 
                request.Message ?? "Manual event", 
                cancellationToken);
            
            return Results.Ok(new { Message = "Event published" });
        });

        var artifactGroup = app.MapGroup("/api/v1/artifacts");

        artifactGroup.MapPost("/", async (
            [FromBody] StoreArtifactRequest request,
            IArtifactStore artifactStore,
            ITaskEventPublisher eventPublisher,
            CancellationToken cancellationToken) =>
        {
            var artifact = new ArtifactContract
            {
                TaskId = request.TaskId,
                StepId = request.StepId,
                AgentId = request.AgentId,
                StepName = request.StepName,
                Artifacts = request.Artifacts ?? new(),
                Summary = request.Summary ?? string.Empty,
                Issues = request.Issues ?? new(),
                Metrics = request.Metrics ?? new()
            };

            await artifactStore.StoreArtifactAsync(artifact, cancellationToken);
            await eventPublisher.PublishArtifactCreatedAsync(request.TaskId, request.StepId, request.AgentId, cancellationToken);

            return Results.Created($"/api/v1/artifacts/{artifact.Id}", artifact);
        });

        artifactGroup.MapGet("/{artifactId:guid}", async (
            Guid artifactId,
            IArtifactStore artifactStore,
            CancellationToken cancellationToken) =>
        {
            var artifact = await artifactStore.GetArtifactAsync(artifactId, cancellationToken);
            if (artifact == null)
            {
                return Results.NotFound();
            }
            return Results.Ok(artifact);
        });

        artifactGroup.MapGet("/task/{taskId:guid}", async (
            Guid taskId,
            IArtifactStore artifactStore,
            CancellationToken cancellationToken) =>
        {
            var artifacts = await artifactStore.ListArtifactsByTaskIdAsync(taskId, cancellationToken);
            return Results.Ok(artifacts);
        });

        artifactGroup.MapDelete("/{artifactId:guid}", async (
            Guid artifactId,
            IArtifactStore artifactStore,
            CancellationToken cancellationToken) =>
        {
            await artifactStore.DeleteArtifactAsync(artifactId, cancellationToken);
            return Results.NoContent();
        });

        return app;
    }
}

public class EventPublishRequest
{
    public Guid TaskId { get; set; }
    public Guid? StepId { get; set; }
    public string? Message { get; set; }
}

public class StoreArtifactRequest
{
    public Guid TaskId { get; set; }
    public Guid StepId { get; set; }
    public Guid AgentId { get; set; }
    public string? StepName { get; set; }
    public List<ArtifactItem>? Artifacts { get; set; }
    public string? Summary { get; set; }
    public List<string>? Issues { get; set; }
    public ArtifactMetrics? Metrics { get; set; }
}