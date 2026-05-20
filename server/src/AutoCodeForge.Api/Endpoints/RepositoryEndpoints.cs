using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Api.Endpoints;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Repository;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers repository API endpoints.
/// </summary>
public static class RepositoryEndpoints
{
    /// <summary>
    /// Maps repository endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapRepositoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/repositories");

        group.MapGet("/", async (int page, int pageSize, RepositoryService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetPagedAsync(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, cancellationToken);
            return Results.Ok(ApiResponse<AutoCodeForge.Core.Models.PagedResult<RepositoryDto>>.Ok(result));
        });

        group.MapGet("/{id:guid}", async (Guid id, RepositoryService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetByIdAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<RepositoryDto>.Ok(result));
        });

        group.MapGet("/{id:guid}/branches", async (Guid id, RepositoryService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetBranchesAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<IEnumerable<GitBranchDto>>.Ok(result));
        });

        group.MapGet("/{id:guid}/commits", async (
            Guid id,
            string branch,
            int limit,
            RepositoryService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetCommitsAsync(id, branch ?? "main", limit <= 0 ? 10 : limit, cancellationToken);
            return Results.Ok(ApiResponse<IEnumerable<GitCommitDto>>.Ok(result));
        });

        group.MapGet("/{id:guid}/pull-requests", async (
            Guid id,
            string state,
            int limit,
            RepositoryService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.ListPullRequestsAsync(id, state ?? "open", limit <= 0 ? 20 : limit, cancellationToken);
            return Results.Ok(ApiResponse<IEnumerable<GitPullRequestDto>>.Ok(result));
        });

        group.MapPost("/", async (CreateRepositoryRequest request, RepositoryService service, CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.CreateAsync(request, cancellationToken);
            return Results.Ok(ApiResponse<RepositoryDto>.Ok(result, "Repository created"));
        });

        group.MapPost("/{id:guid}/pull-requests", async (
            Guid id,
            CreateGitPullRequestRequest request,
            RepositoryService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.CreatePullRequestAsync(id, request, cancellationToken);
            return Results.Ok(ApiResponse<GitPullRequestDto>.Ok(result, "Pull request created"));
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateRepositoryRequest request,
            RepositoryService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.UpdateAsync(id, request, cancellationToken);
            return Results.Ok(ApiResponse<RepositoryDto>.Ok(result, "Repository updated"));
        });

        group.MapPut("/{id:guid}/review-settings", async (
            Guid id,
            UpdateRepositoryReviewSettingsRequest request,
            RepositoryReviewSettingsService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.UpdateAsync(id, request, cancellationToken);
            return Results.Ok(ApiResponse<RepositoryReviewSettingsDto>.Ok(result, "Repository review settings updated"));
        });

        group.MapDelete("/{id:guid}", async (Guid id, RepositoryService service, CancellationToken cancellationToken) =>
        {
            await service.DeleteAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<object?>.Ok(null, "Repository deleted"));
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
