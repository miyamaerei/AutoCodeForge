using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.RepoSync;
using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers repo sync task endpoints.
/// </summary>
public static class RepoSyncEndpoints
{
    /// <summary>
    /// Maps repo sync endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapRepoSyncEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/repo-sync");

        group.MapPost("/tasks", async (
            CreateRepoSyncTaskRequest request,
            RepoSyncService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.CreateTaskAsync(request, cancellationToken);
            return Results.Ok(ApiResponse<TaskResponse>.Ok(result, "Repo sync task created"));
        });

        group.MapGet("/tasks/{taskId:guid}", async (
            Guid taskId,
            RepoSyncService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetTaskDetailAsync(taskId, cancellationToken);
            return Results.Ok(ApiResponse<RepoSyncTaskDetailResponse>.Ok(result));
        });

        group.MapPost("/tasks/{taskId:guid}/cancel", async (
            Guid taskId,
            RepoSyncService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.CancelTaskAsync(taskId, cancellationToken);
            return Results.Ok(ApiResponse<RepoSyncTaskDetailResponse>.Ok(result, "Repo sync task canceled"));
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
