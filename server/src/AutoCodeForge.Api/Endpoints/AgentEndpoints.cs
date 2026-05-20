using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Agent;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers agent API endpoints.
/// </summary>
public static class AgentEndpoints
{
    /// <summary>
    /// Maps agent endpoints.
    /// </summary>
    /// <param name="app">The route builder.</param>
    /// <returns>The route builder.</returns>
    public static IEndpointRouteBuilder MapAgentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/agents");

        group.MapGet("/", async (int page, int pageSize, AgentService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetPagedAsync(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, cancellationToken);
            return Results.Ok(ApiResponse<PagedResult<AgentResponse>>.Ok(result));
        });

        group.MapGet("/{id:guid}", async (Guid id, AgentService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetByIdAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<AgentResponse>.Ok(result));
        });

        group.MapGet("/match", async (string input, AgentService service, CancellationToken cancellationToken) =>
        {
            var result = await service.MatchByInputAsync(input, cancellationToken);
            return Results.Ok(ApiResponse<AgentResponse?>.Ok(result));
        });

        group.MapPost("/", async (CreateAgentRequest request, AgentService service, CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.CreateAsync(request, cancellationToken);
            return Results.Ok(ApiResponse<AgentResponse>.Ok(result, "Agent created"));
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateAgentRequest request,
            AgentService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.UpdateAsync(id, request, cancellationToken);
            return Results.Ok(ApiResponse<AgentResponse>.Ok(result, "Agent updated"));
        });

        group.MapDelete("/{id:guid}", async (Guid id, AgentService service, CancellationToken cancellationToken) =>
        {
            await service.DeleteAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<object?>.Ok(null, "Agent deleted"));
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
