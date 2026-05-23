using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AutoCodeForge.Api.Endpoints;

public static class FailureRecoveryEndpoints
{
    public static IEndpointRouteBuilder MapFailureRecoveryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/failure");

        group.MapPost("/recover", async (
            [FromBody] RecoverRequest request,
            FailureRecoveryService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.HandleFailureAsync(
                request.StepId,
                request.FailureCategory,
                request.ErrorMessage ?? string.Empty,
                cancellationToken);

            return Results.Ok(new
            {
                Status = result.Status,
                Message = result.Message,
                RetryCount = result.RetryCount,
                RetryDelayMs = result.RetryDelayMs,
                DegradationAction = result.DegradationAction
            });
        });

        group.MapGet("/stuck-steps", async (
            FailureRecoveryService service,
            CancellationToken cancellationToken,
            [FromQuery] int timeoutMinutes = 30) =>
        {
            var stuckSteps = await service.DetectStuckStepsAsync(timeoutMinutes, cancellationToken);
            return Results.Ok(stuckSteps);
        });

        group.MapPost("/emergency-unbind/{stepId:guid}", async (
            Guid stepId,
            FailureRecoveryService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.EmergencyUnbindAsync(stepId, cancellationToken);
            return Results.Ok(new { Status = result.Status, Message = result.Message });
        });

        return app;
    }
}

public class RecoverRequest
{
    public Guid StepId { get; set; }
    public FailureCategory FailureCategory { get; set; }
    public string? ErrorMessage { get; set; }
}