using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers human gate API endpoints.
/// </summary>
public static class HumanGateEndpoints
{
    public static IEndpointRouteBuilder MapHumanGateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/human-gates");

        group.MapGet("/pending", async (HumanGateService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetPendingGatesAsync(cancellationToken);
            return Results.Ok(ApiResponse<List<HumanGateResponse>>.Ok(result));
        });

        group.MapGet("/{id:guid}", async (Guid id, HumanGateService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetByIdAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<HumanGateResponse>.Ok(result));
        });

        group.MapGet("/task/{taskId:guid}", async (Guid taskId, HumanGateService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetByTaskIdAsync(taskId, cancellationToken);
            return Results.Ok(ApiResponse<List<HumanGateResponse>>.Ok(result));
        });

        group.MapPost("/", async (CreateHumanGateRequest request, HumanGateService service, CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.CreateGateAsync(request, cancellationToken);
            return Results.Ok(ApiResponse<HumanGateResponse>.Ok(result, "Human gate created"));
        });

        group.MapPost("/{id:guid}/approve", async (Guid id, ApproveRequest request, HumanGateService service, CancellationToken cancellationToken) =>
        {
            var result = await service.ApproveAsync(id, request, cancellationToken);
            return Results.Ok(ApiResponse<HumanGateResponse>.Ok(result, "Gate approved"));
        });

        group.MapPost("/{id:guid}/reject", async (Guid id, RejectRequest request, HumanGateService service, CancellationToken cancellationToken) =>
        {
            var result = await service.RejectAsync(id, request, cancellationToken);
            return Results.Ok(ApiResponse<HumanGateResponse>.Ok(result, "Gate rejected"));
        });

        group.MapPost("/{id:guid}/modify-approve", async (Guid id, ModifyApproveRequest request, HumanGateService service, CancellationToken cancellationToken) =>
        {
            var result = await service.ModifyApproveAsync(id, request, cancellationToken);
            return Results.Ok(ApiResponse<HumanGateResponse>.Ok(result, "Gate modified and approved"));
        });

        group.MapPost("/{id:guid}/cancel", async (Guid id, HumanGateService service, CancellationToken cancellationToken) =>
        {
            var result = await service.CancelAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<HumanGateResponse>.Ok(result, "Gate cancelled"));
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