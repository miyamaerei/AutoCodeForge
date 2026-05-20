using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Review;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers review task endpoints.
/// </summary>
public static class ReviewEndpoints
{
    /// <summary>
    /// Maps review endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/reviews");
        var ruleSetGroup = group.MapGroup("/rule-sets");

        ruleSetGroup.MapGet("/", async (
            int page,
            int pageSize,
            Guid? repositoryId,
            ReviewRuleSetService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetPagedAsync(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, repositoryId, cancellationToken);
            return Results.Ok(ApiResponse<PagedResult<ReviewRuleSetDto>>.Ok(result));
        });

        ruleSetGroup.MapGet("/{id:guid}", async (
            Guid id,
            ReviewRuleSetService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetByIdAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<ReviewRuleSetDto>.Ok(result));
        });

        ruleSetGroup.MapPost("/", async (
            CreateReviewRuleSetRequest request,
            ReviewRuleSetService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.CreateAsync(request, cancellationToken);
            return Results.Ok(ApiResponse<ReviewRuleSetDto>.Ok(result, "Review rule set created"));
        });

        ruleSetGroup.MapPut("/{id:guid}", async (
            Guid id,
            UpdateReviewRuleSetRequest request,
            ReviewRuleSetService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.UpdateAsync(id, request, cancellationToken);
            return Results.Ok(ApiResponse<ReviewRuleSetDto>.Ok(result, "Review rule set updated"));
        });

        ruleSetGroup.MapDelete("/{id:guid}", async (
            Guid id,
            ReviewRuleSetService service,
            CancellationToken cancellationToken) =>
        {
            await service.DeleteAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<object?>.Ok(null, "Review rule set deleted"));
        });

        group.MapPost("/tasks", async (
            CreateReviewTaskRequest request,
            ReviewService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.CreateTaskAsync(request, cancellationToken);
            return Results.Ok(ApiResponse<ReviewTaskDetailDto>.Ok(result, "Review task created"));
        });

        group.MapGet("/tasks/{taskId:guid}", async (
            Guid taskId,
            ReviewService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetTaskAsync(taskId, cancellationToken);
            return Results.Ok(ApiResponse<ReviewTaskDetailDto>.Ok(result));
        });

        group.MapPost("/tasks/{taskId:guid}/cancel", async (
            Guid taskId,
            ReviewService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.CancelTaskAsync(taskId, cancellationToken);
            return Results.Ok(ApiResponse<ReviewTaskDetailDto>.Ok(result, "Review task canceled"));
        });

        group.MapPost("/tasks/{taskId:guid}/rerun", async (
            Guid taskId,
            ReviewService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.RerunTaskAsync(taskId, cancellationToken);
            return Results.Ok(ApiResponse<ReviewTaskDetailDto>.Ok(result, "Review task requeued"));
        });

        group.MapGet("/tasks/{taskId:guid}/findings", async (
            Guid taskId,
            ReviewService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetFindingsAsync(taskId, cancellationToken);
            return Results.Ok(ApiResponse<List<ReviewFindingDto>>.Ok(result));
        });

        group.MapGet("/repositories/{repositoryId:guid}/tasks", async (
            Guid repositoryId,
            int page,
            int pageSize,
            ReviewService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.GetRepositoryTasksAsync(repositoryId, page, pageSize, cancellationToken);
            return Results.Ok(ApiResponse<PagedResult<ReviewTaskSummaryDto>>.Ok(result));
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