using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Wiki;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers wiki API endpoints.
/// </summary>
public static class WikiEndpoints
{
    /// <summary>
    /// Maps wiki endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapWikiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/wiki");

        group.MapGet("/", async (
            string? keyword,
            Guid? repositoryId,
            int page,
            int pageSize,
            WikiService service,
            CancellationToken ct) =>
        {
            var result = await service.GetPagedAsync(
                keyword,
                repositoryId,
                page <= 0 ? 1 : page,
                pageSize <= 0 ? 20 : pageSize,
                ct);
            return Results.Ok(ApiResponse<PagedResult<WikiPageResponse>>.Ok(result));
        });

        group.MapGet("/{id:guid}", async (Guid id, WikiService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return Results.Ok(ApiResponse<WikiPageResponse>.Ok(result));
        });

        group.MapPost("/", async (CreateWikiPageRequest request, WikiService service, CancellationToken ct) =>
        {
            ValidateModel(request);
            var result = await service.CreateAsync(request, ct);
            return Results.Ok(ApiResponse<WikiPageResponse>.Ok(result, "Wiki page created"));
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateWikiPageRequest request,
            WikiService service,
            CancellationToken ct) =>
        {
            ValidateModel(request);
            var result = await service.UpdateAsync(id, request, ct);
            return Results.Ok(ApiResponse<WikiPageResponse>.Ok(result, "Wiki page updated"));
        });

        group.MapDelete("/{id:guid}", async (Guid id, WikiService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.Ok(ApiResponse<object?>.Ok(null, "Wiki page deleted"));
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