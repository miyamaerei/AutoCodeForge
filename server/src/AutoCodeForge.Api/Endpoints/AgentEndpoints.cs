
using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Agent;
using AutoCodeForge.Core.Entities;
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

        // ============ 基础 CRUD ============
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

        // ============ 状态查询 ============
        group.MapGet("/state/{state}", async (string state, AgentService service, CancellationToken cancellationToken) =>
        {
            if (!Enum.TryParse<AgentState>(state, true, out var agentState))
            {
                throw new AutoCodeForge.Core.Exceptions.ValidationException($"Invalid state: {state}");
            }
            var result = await service.GetByStateAsync(agentState, cancellationToken);
            return Results.Ok(ApiResponse<List<AgentResponse>>.Ok(result));
        });

        group.MapGet("/dormant", async (AgentService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetDormantAgentsAsync(cancellationToken);
            return Results.Ok(ApiResponse<List<AgentResponse>>.Ok(result));
        });

        // ============ 状态操作 ============
        group.MapPost("/{id:guid}/assign", async (
            Guid id,
            AssignTaskRequest request,
            AgentService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.AssignTaskAsync(id, request, cancellationToken);
            return Results.Ok(ApiResponse<AgentResponse>.Ok(result, "Task assigned"));
        });

        group.MapPost("/{id:guid}/complete", async (Guid id, AgentService service, CancellationToken cancellationToken) =>
        {
            var result = await service.CompleteTaskAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<AgentResponse>.Ok(result, "Task completed"));
        });

        group.MapPost("/{id:guid}/fail", async (
            Guid id,
            FailTaskRequest request,
            AgentService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.FailTaskAsync(id, request, cancellationToken);
            return Results.Ok(ApiResponse<AgentResponse>.Ok(result, "Task marked as failed"));
        });

        group.MapPost("/{id:guid}/learn", async (Guid id, AgentService service, CancellationToken cancellationToken) =>
        {
            var result = await service.StartLearningAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<AgentResponse>.Ok(result, "Learning started"));
        });

        group.MapPost("/{id:guid}/dormant", async (
            Guid id,
            EnterDormantRequest request,
            AgentService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.EnterDormantAsync(id, request, cancellationToken);
            return Results.Ok(ApiResponse<AgentResponse>.Ok(result, "Agent entered dormant state"));
        });

        group.MapPost("/{id:guid}/wake", async (
            Guid id,
            WakeUpRequest? request,
            AgentService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.WakeUpAsync(id, request?.Remark, cancellationToken);
            return Results.Ok(ApiResponse<AgentResponse>.Ok(result, "Agent woken up"));
        });

        // ============ 历史记录 ============
        group.MapGet("/{id:guid}/learning-records", async (Guid id, AgentService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetLearningRecordsAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<List<AgentLearningRecordResponse>>.Ok(result));
        });

        group.MapGet("/{id:guid}/dormant-records", async (Guid id, AgentService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetDormantRecordsAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<List<AgentDormantRecordResponse>>.Ok(result));
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

/// <summary>
/// 唤醒请求
/// </summary>
public class WakeUpRequest
{
    /// <summary>
    /// 唤醒备注
    /// </summary>
    public string? Remark { get; set; }
}
