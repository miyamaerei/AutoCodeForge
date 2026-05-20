using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.ScheduledTask;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers scheduled task API endpoints.
/// </summary>
public static class ScheduledTaskEndpoints
{
    /// <summary>
    /// Maps scheduled task endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapScheduledTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/scheduled-tasks");

        group.MapGet("/", async (int page, int pageSize, ScheduledTaskService service, CancellationToken ct) =>
        {
            var result = await service.GetPagedAsync(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, ct);
            return Results.Ok(ApiResponse<PagedResult<ScheduledTaskResponse>>.Ok(result));
        });

        group.MapGet("/{id:guid}", async (Guid id, ScheduledTaskService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return Results.Ok(ApiResponse<ScheduledTaskResponse>.Ok(result));
        });

        group.MapGet("/{id:guid}/executions", async (
            Guid id,
            int page,
            int pageSize,
            ScheduledTaskService service,
            CancellationToken ct) =>
        {
            var result = await service.GetExecutionsAsync(id, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, ct);
            return Results.Ok(ApiResponse<PagedResult<ScheduledTaskExecutionResponse>>.Ok(result));
        });

        group.MapPost("/", async (CreateScheduledTaskRequest request, ScheduledTaskService service, CancellationToken ct) =>
        {
            ValidateModel(request);
            var result = await service.CreateAsync(request, ct);
            return Results.Ok(ApiResponse<ScheduledTaskResponse>.Ok(result, "Scheduled task created"));
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateScheduledTaskRequest request,
            ScheduledTaskService service,
            CancellationToken ct) =>
        {
            ValidateModel(request);
            var result = await service.UpdateAsync(id, request, ct);
            return Results.Ok(ApiResponse<ScheduledTaskResponse>.Ok(result, "Scheduled task updated"));
        });

        group.MapPost("/{id:guid}/pause", async (Guid id, ScheduledTaskService service, CancellationToken ct) =>
        {
            var result = await service.PauseAsync(id, ct);
            return Results.Ok(ApiResponse<ScheduledTaskResponse>.Ok(result, "Scheduled task paused"));
        });

        group.MapPost("/{id:guid}/resume", async (Guid id, ScheduledTaskService service, CancellationToken ct) =>
        {
            var result = await service.ResumeAsync(id, ct);
            return Results.Ok(ApiResponse<ScheduledTaskResponse>.Ok(result, "Scheduled task resumed"));
        });

        group.MapDelete("/{id:guid}", async (Guid id, ScheduledTaskService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.Ok(ApiResponse<object?>.Ok(null, "Scheduled task deleted"));
        });

        return app;
    }

    private static void ValidateModel(object request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, context, results, true))
        {
            var message = string.Join("; ", results.Select(r => r.ErrorMessage));
            throw new AutoCodeForge.Core.Exceptions.ValidationException(message);
        }
    }
}
