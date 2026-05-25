using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// 注册工序步骤 API 端点
/// </summary>
public static class TaskStepEndpoints
{
    /// <summary>
    /// 映射工序步骤端点
    /// </summary>
    /// <param name="app">路由构建器</param>
    /// <returns>路由构建器</returns>
    public static IEndpointRouteBuilder MapTaskStepEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/tasks/{taskId:guid}/steps");

        // 获取任务的所有工序
        group.MapGet("/", async (Guid taskId, TaskStepService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetByTaskIdAsync(taskId, cancellationToken);
            return Results.Ok(ApiResponse<List<TaskStepResponse>>.Ok(result));
        });

        // 获取任务的当前活跃工序
        group.MapGet("/active", async (Guid taskId, TaskStepService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetActiveStepAsync(taskId, cancellationToken);
            return Results.Ok(ApiResponse<TaskStepResponse?>.Ok(result));
        });

        // 获取工序详情
        group.MapGet("/{stepId:guid}", async (Guid stepId, TaskStepService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetByIdAsync(stepId, cancellationToken);
            return Results.Ok(ApiResponse<TaskStepResponse>.Ok(result));
        });

        // 初始化7步工序
        group.MapPost("/init", async (Guid taskId, TaskStepService service, CancellationToken cancellationToken) =>
        {
            var result = await service.InitializeStepsAsync(taskId, cancellationToken);
            return Results.Ok(ApiResponse<List<TaskStepResponse>>.Ok(result, "Steps initialized"));
        });

        // 推进工序
        group.MapPost("/{stepId:guid}/advance", async (
            Guid taskId,
            Guid stepId,
            AdvanceTaskStepRequest request,
            TaskStepService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.AdvanceStepAsync(taskId, stepId, request, cancellationToken);
            return Results.Ok(ApiResponse<TaskStepResponse>.Ok(result, "Step advanced"));
        });

        // 跳过工序
        group.MapPost("/{stepId:guid}/skip", async (
            Guid taskId,
            Guid stepId,
            SkipTaskStepRequest request,
            TaskStepService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.SkipStepAsync(taskId, stepId, request, cancellationToken);
            return Results.Ok(ApiResponse<TaskStepResponse>.Ok(result, "Step skipped"));
        });

        // 解绑工序
        group.MapPost("/{stepId:guid}/unbind", async (
            Guid taskId,
            Guid stepId,
            UnbindTaskStepRequest request,
            TaskStepService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.UnbindStepAsync(taskId, stepId, request, cancellationToken);
            return Results.Ok(ApiResponse<TaskStepResponse>.Ok(result, "Step unbound"));
        });

        // 获取上下文
        group.MapGet("/context", async (
            Guid taskId,
            Guid? stepId,
            int? maxTokens,
            TaskStepService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.BuildContextAsync(taskId, stepId, maxTokens ?? 8000, cancellationToken);
            return Results.Ok(ApiResponse<string>.Ok(result));
        });

        // 更新工序
        group.MapPut("/{stepId:guid}", async (
            Guid stepId,
            UpdateTaskStepRequest request,
            TaskStepService service,
            CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.UpdateAsync(stepId, request, cancellationToken);
            return Results.Ok(ApiResponse<TaskStepResponse>.Ok(result, "Step updated"));
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
