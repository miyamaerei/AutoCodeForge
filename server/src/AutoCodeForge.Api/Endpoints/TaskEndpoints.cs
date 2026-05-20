using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers task API endpoints.
/// </summary>
public static class TaskEndpoints
{
    /// <summary>
    /// Maps task endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/tasks");

        group.MapGet("/", async (int page, int pageSize, TaskService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetPagedAsync(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, cancellationToken);
            return Results.Ok(ApiResponse<AutoCodeForge.Core.Models.PagedResult<TaskResponse>>.Ok(result));
        });

        group.MapGet("/{id:guid}", async (Guid id, TaskService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetByIdAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<TaskResponse>.Ok(result));
        });

        group.MapGet("/{id:guid}/logs", async (Guid id, TaskService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetLogsAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<List<TaskLogResponse>>.Ok(result));
        });

        group.MapPost("/", async (CreateTaskRequest request, TaskService service, CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.CreateAsync(request, cancellationToken);
            return Results.Ok(ApiResponse<TaskResponse>.Ok(result, "Task created"));
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateTaskRequest request,
            TaskService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.UpdateAsync(id, request, cancellationToken);
            return Results.Ok(ApiResponse<TaskResponse>.Ok(result, "Task updated"));
        });

        group.MapPost("/{id:guid}/pause", async (Guid id, TaskService service, CancellationToken cancellationToken) =>
        {
            var result = await service.PauseAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<TaskResponse>.Ok(result, "Task paused"));
        });

        group.MapPost("/{id:guid}/resume", async (Guid id, TaskService service, CancellationToken cancellationToken) =>
        {
            var result = await service.ResumeAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<TaskResponse>.Ok(result, "Task resumed"));
        });

        group.MapDelete("/{id:guid}", async (Guid id, TaskService service, CancellationToken cancellationToken) =>
        {
            await service.DeleteAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<object?>.Ok(null, "Task deleted"));
        });

        return app;
    }

    private static void ValidateModel(object request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, context, results, true))
        {
            var message = string.Join("; ", results.Select(result => result.ErrorMessage));
            throw new AutoCodeForge.Core.Exceptions.ValidationException(message);
        }
    }
}
