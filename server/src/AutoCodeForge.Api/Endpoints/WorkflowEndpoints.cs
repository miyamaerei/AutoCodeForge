using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Workflow;
using AutoCodeForge.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// 工作流 API 端点 - 提供工作流的 CRUD 和执行控制功能
/// </summary>
public static class WorkflowEndpoints
{
    /// <summary>
    /// 注册工作流端点
    /// </summary>
    public static IEndpointRouteBuilder MapWorkflowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/workflows");
        var instanceGroup = app.MapGroup("/api/v1/workflow-instances");

        #region Workflow Definition Endpoints

        // 获取工作流列表
        group.MapGet("/", async (int page, int pageSize, WorkflowService service, CancellationToken ct) =>
        {
            var result = await service.GetPagedAsync(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, ct);
            return Results.Ok(ApiResponse<PagedResult<WorkflowResponse>>.Ok(result));
        });

        // 获取最近工作流
        group.MapGet("/recent", async (int take, WorkflowService service, CancellationToken ct) =>
        {
            var result = await service.GetRecentAsync(take <= 0 ? 10 : Math.Min(take, 50), ct);
            return Results.Ok(ApiResponse<List<WorkflowResponse>>.Ok(result));
        });

        // 获取单个工作流
        group.MapGet("/{id:guid}", async (Guid id, WorkflowService service, CancellationToken ct) =>
        {
            var result = await service.GetByIdAsync(id, ct);
            return Results.Ok(ApiResponse<WorkflowResponse>.Ok(result));
        });

        // 创建工作流
        group.MapPost("/", async (CreateWorkflowRequest request, WorkflowService service, CancellationToken ct) =>
        {
            ValidateModel(request);
            var result = await service.CreateAsync(request, ct);
            return Results.Created($"/api/v1/workflows/{result.Id}", ApiResponse<WorkflowResponse>.Ok(result, "Workflow created"));
        });

        // 更新工作流
        group.MapPut("/{id:guid}", async (Guid id, UpdateWorkflowRequest request, WorkflowService service, CancellationToken ct) =>
        {
            var result = await service.UpdateAsync(id, request, ct);
            return Results.Ok(ApiResponse<WorkflowResponse>.Ok(result, "Workflow updated"));
        });

        // 删除工作流
        group.MapDelete("/{id:guid}", async (Guid id, WorkflowService service, CancellationToken ct) =>
        {
            await service.DeleteAsync(id, ct);
            return Results.Ok(ApiResponse<object?>.Ok(null, "Workflow deleted"));
        });

        // 执行工作流
        group.MapPost("/{id:guid}/execute", async (Guid id, ExecuteWorkflowRequest request, WorkflowService service, CancellationToken ct) =>
        {
            ValidateModel(request);
            var result = await service.ExecuteAsync(id, request, ct);
            return Results.Ok(ApiResponse<WorkflowInstanceResponse>.Ok(result, "Workflow execution started"));
        });

        // 获取工作流的所有实例
        group.MapGet("/{id:guid}/instances", async (Guid id, WorkflowService service, CancellationToken ct) =>
        {
            var result = await service.GetInstancesByWorkflowIdAsync(id, ct);
            return Results.Ok(ApiResponse<List<WorkflowInstanceResponse>>.Ok(result));
        });

        #endregion

        #region Workflow Instance Endpoints

        // 获取最近实例
        instanceGroup.MapGet("/recent", async (int take, WorkflowService service, CancellationToken ct) =>
        {
            var result = await service.GetRecentInstancesAsync(take <= 0 ? 10 : Math.Min(take, 50), ct);
            return Results.Ok(ApiResponse<List<WorkflowInstanceResponse>>.Ok(result));
        });

        // 获取单个实例
        instanceGroup.MapGet("/{id:guid}", async (Guid id, WorkflowService service, CancellationToken ct) =>
        {
            var result = await service.GetInstanceByIdAsync(id, ct);
            return Results.Ok(ApiResponse<WorkflowInstanceResponse>.Ok(result));
        });

        // 暂停实例
        instanceGroup.MapPost("/{id:guid}/pause", async (Guid id, WorkflowService service, CancellationToken ct) =>
        {
            var success = await service.PauseAsync(id, ct);
            return success
                ? Results.Ok(ApiResponse<object?>.Ok(null, "Workflow paused"))
                : Results.BadRequest(ApiResponse<object?>.Fail("Cannot pause workflow in current state"));
        });

        // 恢复实例
        instanceGroup.MapPost("/{id:guid}/resume", async (Guid id, WorkflowService service, CancellationToken ct) =>
        {
            var success = await service.ResumeAsync(id, ct);
            return success
                ? Results.Ok(ApiResponse<object?>.Ok(null, "Workflow resumed"))
                : Results.BadRequest(ApiResponse<object?>.Fail("Cannot resume workflow in current state"));
        });

        // 终止实例
        instanceGroup.MapPost("/{id:guid}/terminate", async (Guid id, WorkflowService service, CancellationToken ct) =>
        {
            var success = await service.TerminateAsync(id, ct);
            return success
                ? Results.Ok(ApiResponse<object?>.Ok(null, "Workflow terminated"))
                : Results.BadRequest(ApiResponse<object?>.Fail("Cannot terminate workflow in current state"));
        });

        // 删除实例
        instanceGroup.MapDelete("/{id:guid}", async (Guid id, WorkflowService service, CancellationToken ct) =>
        {
            await service.DeleteInstanceAsync(id, ct);
            return Results.Ok(ApiResponse<object?>.Ok(null, "Instance deleted"));
        });

        // 获取实例事件
        instanceGroup.MapGet("/{id:guid}/events", async (Guid id, WorkflowService service, CancellationToken ct) =>
        {
            var result = await service.GetEventsAsync(id, ct);
            return Results.Ok(ApiResponse<List<WorkflowEventResponse>>.Ok(result));
        });

        #endregion

        #region SSE Event Stream

        // SSE 事件流 - 获取实例的实时事件
        instanceGroup.MapGet("/{id:guid}/events/stream", async (
            Guid id, 
            WorkflowService service, 
            HttpContext context,
            CancellationToken ct) =>
        {
            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");
            context.Response.ContentType = "text/event-stream";

            var lastEventId = Guid.Empty;

            while (!ct.IsCancellationRequested && !context.RequestAborted.IsCancellationRequested)
            {
                try
                {
                    // 获取自上次以来的新事件
                    var events = await service.GetEventsAsync(id, ct);

                    foreach (var evt in events.Where(e => e.Id != lastEventId))
                    {
                        lastEventId = evt.Id;

                        var eventData = new
                        {
                            id = evt.Id,
                            instanceId = evt.InstanceId,
                            eventType = evt.EventType,
                            message = evt.Message,
                            dataJson = evt.DataJson,
                            nodeId = evt.NodeId,
                            level = evt.Level,
                            timestamp = evt.Timestamp.ToString("O")
                        };

                        var json = JsonSerializer.Serialize(eventData);
                        await WriteEventAsync(context, evt.EventType, json, ct);
                    }

                    // 等待一段时间后再检查新事件
                    await Task.Delay(500, ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    // 发生错误时短暂等待后重试
                    await Task.Delay(1000, ct);
                }
            }

            return Results.Empty;
        });

        #endregion

        return app;
    }

    /// <summary>
    /// 验证模型
    /// </summary>
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

    private static async Task WriteEventAsync(
        HttpContext context,
        string eventName,
        string data,
        CancellationToken cancellationToken)
    {
        await context.Response.WriteAsync($"event: {eventName}\n", cancellationToken);
        await context.Response.WriteAsync($"data: {data}\n\n", cancellationToken);
        await context.Response.Body.FlushAsync(cancellationToken);
    }
}