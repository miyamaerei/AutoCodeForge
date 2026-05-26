using System.Text.Json;
using AutoCodeForge.Core.DTOs.Workflow;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// 工作流服务 - 提供工作流的CRUD和执行控制功能
/// </summary>
public class WorkflowService
{
    private readonly WorkflowRepository _workflowRepository;
    private readonly WorkflowInstanceRepository _instanceRepository;
    private readonly WorkflowEventRepository _eventRepository;
    private readonly ILogger<WorkflowService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// 初始化工作流服务
    /// </summary>
    public WorkflowService(
        WorkflowRepository workflowRepository,
        WorkflowInstanceRepository instanceRepository,
        WorkflowEventRepository eventRepository,
        ILogger<WorkflowService> logger)
    {
        _workflowRepository = workflowRepository;
        _instanceRepository = instanceRepository;
        _eventRepository = eventRepository;
        _logger = logger;
    }

    #region Workflow Definition CRUD

    /// <summary>
    /// 创建工作流
    /// </summary>
    public async Task<WorkflowResponse> CreateAsync(CreateWorkflowRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new WorkflowEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            NodesJson = request.NodesJson,
            EdgesJson = request.EdgesJson,
            ExecutorsJson = request.ExecutorsJson,
            ContextProvidersJson = request.ContextProviders != null
                ? JsonSerializer.Serialize(request.ContextProviders, JsonOptions)
                : null,
            Status = Core.Entities.WorkflowStatus.Draft,
            Version = 1
        };

        var created = await _workflowRepository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created workflow {WorkflowId}: {WorkflowName}", created.Id, created.Name);

        return ToResponse(created);
    }

    /// <summary>
    /// 更新工作流
    /// </summary>
    public async Task<WorkflowResponse> UpdateAsync(Guid id, UpdateWorkflowRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(id, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Name))
            entity.Name = request.Name.Trim();
        if (request.Description != null)
            entity.Description = request.Description.Trim();
        if (request.NodesJson != null)
            entity.NodesJson = request.NodesJson;
        if (request.EdgesJson != null)
            entity.EdgesJson = request.EdgesJson;
        if (request.ExecutorsJson != null)
            entity.ExecutorsJson = request.ExecutorsJson;
        if (request.ContextProviders != null)
            entity.ContextProvidersJson = JsonSerializer.Serialize(request.ContextProviders, JsonOptions);
        if (request.Status.HasValue)
            entity.Status = (Core.Entities.WorkflowStatus)request.Status.Value;

        entity.Version++;

        await _workflowRepository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated workflow {WorkflowId}", id);

        return ToResponse(entity);
    }

    /// <summary>
    /// 获取工作流详情
    /// </summary>
    public async Task<WorkflowResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(id, cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// 获取分页工作流列表
    /// </summary>
    public async Task<PagedResult<WorkflowResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _workflowRepository.GetPagedAsync(page, pageSize, cancellationToken: cancellationToken);
        return new PagedResult<WorkflowResponse>
        {
            Items = result.Items.Select(ToResponse).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    /// <summary>
    /// 获取最近的工作流
    /// </summary>
    public async Task<List<WorkflowResponse>> GetRecentAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var entities = await _workflowRepository.GetRecentAsync(take, cancellationToken);
        return entities.Select(ToResponse).ToList();
    }

    /// <summary>
    /// 删除工作流
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await GetEntityOrThrowAsync(id, cancellationToken);
        await _workflowRepository.SoftDeleteAsync(id, cancellationToken: cancellationToken);
        _logger.LogInformation("Deleted workflow {WorkflowId}", id);
    }

    #endregion

    #region Workflow Instance Management

    /// <summary>
    /// 执行工作流
    /// </summary>
    public async Task<WorkflowInstanceResponse> ExecuteAsync(Guid workflowId, ExecuteWorkflowRequest request, CancellationToken cancellationToken = default)
    {
        var workflow = await GetEntityOrThrowAsync(workflowId, cancellationToken);

        var instance = new WorkflowInstanceEntity
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflowId,
            InputJson = request.DataJson,
            ContextJson = request.Context != null ? JsonSerializer.Serialize(request.Context, JsonOptions) : null,
            Status = WorkflowInstanceStatus.Pending,
            Progress = 0,
            AgentId = request.AgentId
        };

        var created = await _instanceRepository.CreateAsync(instance, cancellationToken);

        // 发布启动事件
        await PublishEventAsync(created.Id, WorkflowEventTypes.Started, "Workflow started", null, "Info", cancellationToken);

        // 尝试标记为运行中
        var marked = await _instanceRepository.TryMarkRunningAsync(created.Id, DateTime.UtcNow, cancellationToken);
        if (marked)
        {
            await PublishEventAsync(created.Id, WorkflowEventTypes.NodeEntered, "Workflow execution started", null, "Info", cancellationToken);
            await _instanceRepository.UpdateProgressAsync(created.Id, 10, "start", cancellationToken);
        }

        _logger.LogInformation("Started workflow instance {InstanceId} for workflow {WorkflowId}", created.Id, workflowId);

        return await ToInstanceResponseAsync(created, workflow.Name, cancellationToken);
    }

    /// <summary>
    /// 获取实例详情
    /// </summary>
    public async Task<WorkflowInstanceResponse> GetInstanceByIdAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        var instance = await GetInstanceEntityOrThrowAsync(instanceId, cancellationToken);
        var workflow = await GetEntityOrThrowAsync(instance.WorkflowId, cancellationToken);
        return ToInstanceResponse(instance, workflow.Name);
    }

    /// <summary>
    /// 获取工作流的所有实例
    /// </summary>
    public async Task<List<WorkflowInstanceResponse>> GetInstancesByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        var workflow = await GetEntityOrThrowAsync(workflowId, cancellationToken);
        var instances = await _instanceRepository.GetByWorkflowIdAsync(workflowId, cancellationToken);
        return instances.Select(i => ToInstanceResponse(i, workflow.Name)).ToList();
    }

    /// <summary>
    /// 获取最近实例
    /// </summary>
    public async Task<List<WorkflowInstanceResponse>> GetRecentInstancesAsync(int take = 10, CancellationToken cancellationToken = default)
    {
        var instances = await _instanceRepository.GetRecentAsync(take, cancellationToken);
        var results = new List<WorkflowInstanceResponse>();

        foreach (var instance in instances)
        {
            try
            {
                var workflow = await _workflowRepository.GetByIdAsync(instance.WorkflowId, cancellationToken: cancellationToken);
                results.Add(ToInstanceResponse(instance, workflow?.Name ?? "Unknown"));
            }
            catch
            {
                results.Add(ToInstanceResponse(instance, "Unknown"));
            }
        }

        return results;
    }

    /// <summary>
    /// 暂停工作流实例
    /// </summary>
    public async Task<bool> PauseAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        var instance = await GetInstanceEntityOrThrowAsync(instanceId, cancellationToken);
        if (instance.Status != WorkflowInstanceStatus.Running)
            return false;

        await _instanceRepository.MarkPausedAsync(instanceId, cancellationToken);
        await PublishEventAsync(instanceId, WorkflowEventTypes.Paused, "Workflow paused", instance.CurrentNodeId, "Warning", cancellationToken);

        _logger.LogInformation("Paused workflow instance {InstanceId}", instanceId);
        return true;
    }

    /// <summary>
    /// 恢复工作流实例
    /// </summary>
    public async Task<bool> ResumeAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        var instance = await GetInstanceEntityOrThrowAsync(instanceId, cancellationToken);
        if (instance.Status != WorkflowInstanceStatus.Paused)
            return false;

        await _instanceRepository.MarkResumedAsync(instanceId, cancellationToken);
        await PublishEventAsync(instanceId, WorkflowEventTypes.Resumed, "Workflow resumed", instance.CurrentNodeId, "Info", cancellationToken);

        _logger.LogInformation("Resumed workflow instance {InstanceId}", instanceId);
        return true;
    }

    /// <summary>
    /// 终止工作流实例
    /// </summary>
    public async Task<bool> TerminateAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        var instance = await GetInstanceEntityOrThrowAsync(instanceId, cancellationToken);
        if (instance.Status == WorkflowInstanceStatus.Completed || instance.Status == WorkflowInstanceStatus.Terminated)
            return false;

        await _instanceRepository.MarkTerminatedAsync(instanceId, cancellationToken);
        await PublishEventAsync(instanceId, WorkflowEventTypes.Terminated, "Workflow terminated", instance.CurrentNodeId, "Warning", cancellationToken);

        _logger.LogInformation("Terminated workflow instance {InstanceId}", instanceId);
        return true;
    }

    /// <summary>
    /// 更新实例进度（内部方法，供执行引擎调用）
    /// </summary>
    public async Task UpdateProgressAsync(Guid instanceId, int progress, string? currentNodeId, CancellationToken cancellationToken = default)
    {
        await _instanceRepository.UpdateProgressAsync(instanceId, progress, currentNodeId, cancellationToken);
        await PublishEventAsync(instanceId, WorkflowEventTypes.ProgressUpdated, $"Progress: {progress}%", currentNodeId, "Info", cancellationToken);
    }

    /// <summary>
    /// 完成实例（内部方法，供执行引擎调用）
    /// </summary>
    public async Task CompleteAsync(Guid instanceId, string? outputJson, CancellationToken cancellationToken = default)
    {
        await _instanceRepository.MarkCompletedAsync(instanceId, outputJson, cancellationToken);
        await PublishEventAsync(instanceId, WorkflowEventTypes.Completed, "Workflow completed successfully", null, "Info", cancellationToken);

        _logger.LogInformation("Completed workflow instance {InstanceId}", instanceId);
    }

    /// <summary>
    /// 标记实例失败（内部方法，供执行引擎调用）
    /// </summary>
    public async Task FailAsync(Guid instanceId, string errorMessage, CancellationToken cancellationToken = default)
    {
        await _instanceRepository.MarkFailedAsync(instanceId, errorMessage, cancellationToken);
        await PublishEventAsync(instanceId, WorkflowEventTypes.Failed, errorMessage, null, "Error", cancellationToken);

        _logger.LogError("Workflow instance {InstanceId} failed: {ErrorMessage}", instanceId, errorMessage);
    }

    /// <summary>
    /// 删除实例
    /// </summary>
    public async Task DeleteInstanceAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        await GetInstanceEntityOrThrowAsync(instanceId, cancellationToken);
        await _instanceRepository.SoftDeleteAsync(instanceId, cancellationToken: cancellationToken);

        _logger.LogInformation("Deleted workflow instance {InstanceId}", instanceId);
    }

    #endregion

    #region Event Management

    /// <summary>
    /// 获取实例的事件列表
    /// </summary>
    public async Task<List<WorkflowEventResponse>> GetEventsAsync(Guid instanceId, CancellationToken cancellationToken = default)
    {
        var events = await _eventRepository.GetByInstanceIdAsync(instanceId, cancellationToken);
        return events.Select(ToEventResponse).ToList();
    }

    /// <summary>
    /// 发布事件
    /// </summary>
    private async Task PublishEventAsync(
        Guid instanceId,
        string eventType,
        string? message,
        string? nodeId,
        string level,
        CancellationToken cancellationToken = default)
    {
        var evt = new WorkflowEventEntity
        {
            Id = Guid.NewGuid(),
            InstanceId = instanceId,
            EventType = eventType,
            Message = message,
            NodeId = nodeId,
            Level = level
        };

        await _eventRepository.CreateAsync(evt, cancellationToken);
    }

    #endregion

    #region Private Helpers

    private async Task<WorkflowEntity> GetEntityOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _workflowRepository.GetByIdAsync(id, cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Workflow with id {id} not found");
        return entity;
    }

    private async Task<WorkflowInstanceEntity> GetInstanceEntityOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _instanceRepository.GetByIdAsync(id, cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Workflow instance with id {id} not found");
        return entity;
    }

    private static WorkflowResponse ToResponse(WorkflowEntity entity)
    {
        List<string>? contextProviders = null;
        if (!string.IsNullOrEmpty(entity.ContextProvidersJson))
        {
            try
            {
                contextProviders = JsonSerializer.Deserialize<List<string>>(entity.ContextProvidersJson, JsonOptions);
            }
            catch { }
        }

        return new WorkflowResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Status = entity.Status.ToString(),
            Version = entity.Version,
            NodesJson = entity.NodesJson,
            EdgesJson = entity.EdgesJson,
            ExecutorsJson = entity.ExecutorsJson,
            ContextProviders = contextProviders,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }

    private async Task<WorkflowInstanceResponse> ToInstanceResponseAsync(WorkflowInstanceEntity entity, string workflowName, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        return ToInstanceResponse(entity, workflowName);
    }

    private static WorkflowInstanceResponse ToInstanceResponse(WorkflowInstanceEntity entity, string workflowName)
    {
        return new WorkflowInstanceResponse
        {
            Id = entity.Id,
            WorkflowId = entity.WorkflowId,
            WorkflowName = workflowName,
            CurrentNodeId = entity.CurrentNodeId,
            Status = entity.Status.ToString(),
            Progress = entity.Progress,
            InputJson = entity.InputJson,
            OutputJson = entity.OutputJson,
            ErrorMessage = entity.ErrorMessage,
            StartedAtUtc = entity.StartedAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc,
            AgentId = entity.AgentId,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }

    private static WorkflowEventResponse ToEventResponse(WorkflowEventEntity entity)
    {
        return new WorkflowEventResponse
        {
            Id = entity.Id,
            InstanceId = entity.InstanceId,
            EventType = entity.EventType,
            Message = entity.Message,
            DataJson = entity.DataJson,
            NodeId = entity.NodeId,
            Level = entity.Level,
            Timestamp = entity.CreatedAtUtc
        };
    }

    #endregion
}
