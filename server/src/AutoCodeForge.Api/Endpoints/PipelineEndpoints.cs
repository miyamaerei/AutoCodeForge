using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Pipeline;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers pipeline API endpoints.
/// </summary>
public static class PipelineEndpoints
{
    /// <summary>
    /// Maps pipeline endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapPipelineEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/pipelines");

        group.MapGet("/", async (int page, int pageSize, PipelineService service, CancellationToken ct) =>
        {
            var result = await service.GetPagedAsync(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, ct);
            return Results.Ok(ApiResponse<PagedResult<PipelineResponse>>.Ok(result));
        });

        group.MapGet("/{id:guid}", async (Guid id, PipelineService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return Results.Ok(ApiResponse<PipelineResponse>.Ok(result));
        });

        group.MapGet("/{id:guid}/builds", async (
            Guid id,
            int page,
            int pageSize,
            PipelineService service,
            CancellationToken ct) =>
        {
            var result = await service.GetBuildsAsync(id, page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, ct);
            return Results.Ok(ApiResponse<PagedResult<BuildResponse>>.Ok(result));
        });

        group.MapPost("/", async (CreatePipelineRequest request, PipelineService service, CancellationToken ct) =>
        {
            ValidateModel(request);
            var result = await service.CreateAsync(request, ct);
            return Results.Ok(ApiResponse<PipelineResponse>.Ok(result, "Pipeline created"));
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdatePipelineRequest request,
            PipelineService service,
            CancellationToken ct) =>
        {
            ValidateModel(request);
            var result = await service.UpdateAsync(id, request, ct);
            return Results.Ok(ApiResponse<PipelineResponse>.Ok(result, "Pipeline updated"));
        });

        group.MapPost("/{id:guid}/trigger", async (
            Guid id,
            TriggerPipelineRequest? request,
            PipelineService service,
            CancellationToken ct) =>
        {
            var safeRequest = request ?? new TriggerPipelineRequest();
            ValidateModel(safeRequest);
            var result = await service.TriggerAsync(id, safeRequest, ct);
            return Results.Ok(ApiResponse<BuildResponse>.Ok(result, "Pipeline triggered"));
        });

        group.MapDelete("/{id:guid}", async (Guid id, PipelineService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.Ok(ApiResponse<object?>.Ok(null, "Pipeline deleted"));
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
