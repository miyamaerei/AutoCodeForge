using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;
using System.Text.Json;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides human gate operations and intervention capabilities.
/// </summary>
public class HumanGateService
{
    private readonly HumanGateRepository _humanGateRepository;
    private readonly TaskRepository _taskRepository;
    private readonly TaskStepRepository _taskStepRepository;
    private readonly TaskLogRepository _taskLogRepository;

    public HumanGateService(
        HumanGateRepository humanGateRepository,
        TaskRepository taskRepository,
        TaskStepRepository taskStepRepository,
        TaskLogRepository taskLogRepository)
    {
        _humanGateRepository = humanGateRepository;
        _taskRepository = taskRepository;
        _taskStepRepository = taskStepRepository;
        _taskLogRepository = taskLogRepository;
    }

    public async Task<HumanGateResponse> CreateGateAsync(CreateHumanGateRequest request, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<HumanGateType>(request.GateType, true, out var gateType))
        {
            throw new ValidationException("Invalid gate type");
        }

        var task = await _taskRepository.GetByIdAsync(request.TaskId, false, cancellationToken)
            ?? throw new NotFoundException("Task not found");

        if (request.TaskStepId.HasValue)
        {
            var step = await _taskStepRepository.GetByIdAsync(request.TaskStepId.Value, false, cancellationToken);
            if (step == null || step.TaskId != request.TaskId)
            {
                throw new ValidationException("Invalid task step");
            }
        }

        var existingPending = await _humanGateRepository.GetPendingByTaskIdAsync(request.TaskId, cancellationToken);
        if (existingPending != null)
        {
            throw new ValidationException("A pending gate already exists for this task");
        }

        var gate = new HumanGateEntity
        {
            Id = Guid.NewGuid(),
            TaskId = request.TaskId,
            TaskStepId = request.TaskStepId,
            GateType = gateType,
            Status = HumanGateStatus.Pending,
            Reason = request.Reason,
        };

        var created = await _humanGateRepository.CreateAsync(gate, cancellationToken);
        await AddTaskLogAsync(request.TaskId, "Info", $"Human gate created: {gateType}", nameof(HumanGateService), cancellationToken);
        return ToResponse(created);
    }

    public async Task<HumanGateResponse> ApproveAsync(Guid gateId, ApproveRequest request, CancellationToken cancellationToken = default)
    {
        var gate = await GetGateOrThrowAsync(gateId, cancellationToken);
        
        if (gate.Status != HumanGateStatus.Pending)
        {
            throw new ValidationException("Only pending gates can be approved");
        }

        gate.Status = HumanGateStatus.Approved;
        gate.HumanResponse = request.Comment;
        gate.RespondedAtUtc = DateTime.UtcNow;

        await _humanGateRepository.UpdateAsync(gate, cancellationToken);
        await AddTaskLogAsync(gate.TaskId, "Info", "Human gate approved", nameof(HumanGateService), cancellationToken);
        return ToResponse(gate);
    }

    public async Task<HumanGateResponse> RejectAsync(Guid gateId, RejectRequest request, CancellationToken cancellationToken = default)
    {
        var gate = await GetGateOrThrowAsync(gateId, cancellationToken);
        
        if (gate.Status != HumanGateStatus.Pending)
        {
            throw new ValidationException("Only pending gates can be rejected");
        }

        gate.Status = HumanGateStatus.Rejected;
        gate.HumanResponse = request.Reason;
        gate.RespondedAtUtc = DateTime.UtcNow;

        await _humanGateRepository.UpdateAsync(gate, cancellationToken);
        
        if (gate.TaskStepId.HasValue)
        {
            await ResetStepAsync(gate.TaskStepId.Value, gate.TaskId, cancellationToken);
        }

        await AddTaskLogAsync(gate.TaskId, "Info", $"Human gate rejected: {request.Reason}", nameof(HumanGateService), cancellationToken);
        return ToResponse(gate);
    }

    public async Task<HumanGateResponse> ModifyApproveAsync(Guid gateId, ModifyApproveRequest request, CancellationToken cancellationToken = default)
    {
        var gate = await GetGateOrThrowAsync(gateId, cancellationToken);
        
        if (gate.Status != HumanGateStatus.Pending)
        {
            throw new ValidationException("Only pending gates can be modified and approved");
        }

        gate.Status = HumanGateStatus.Modified;
        gate.ModificationsJson = request.Modifications != null 
            ? JsonSerializer.Serialize(request.Modifications) 
            : null;
        gate.RespondedAtUtc = DateTime.UtcNow;

        await _humanGateRepository.UpdateAsync(gate, cancellationToken);
        await AddTaskLogAsync(gate.TaskId, "Info", "Human gate modified and approved", nameof(HumanGateService), cancellationToken);
        return ToResponse(gate);
    }

    public async Task<HumanGateResponse> CancelAsync(Guid gateId, CancellationToken cancellationToken = default)
    {
        var gate = await GetGateOrThrowAsync(gateId, cancellationToken);
        
        if (gate.Status != HumanGateStatus.Pending)
        {
            throw new ValidationException("Only pending gates can be cancelled");
        }

        gate.Status = HumanGateStatus.Cancelled;
        gate.RespondedAtUtc = DateTime.UtcNow;

        await _humanGateRepository.UpdateAsync(gate, cancellationToken);
        await AddTaskLogAsync(gate.TaskId, "Info", "Human gate cancelled", nameof(HumanGateService), cancellationToken);
        return ToResponse(gate);
    }

    public async Task<List<HumanGateResponse>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var gates = await _humanGateRepository.GetByTaskIdAsync(taskId, cancellationToken);
        return gates.Select(ToResponse).ToList();
    }

    public async Task<List<HumanGateResponse>> GetPendingGatesAsync(CancellationToken cancellationToken = default)
    {
        var gates = await _humanGateRepository.GetPendingGatesAsync(cancellationToken);
        return gates.Select(ToResponse).ToList();
    }

    public async Task<HumanGateResponse> GetByIdAsync(Guid gateId, CancellationToken cancellationToken = default)
    {
        var gate = await GetGateOrThrowAsync(gateId, cancellationToken);
        return ToResponse(gate);
    }

    private async Task<HumanGateEntity> GetGateOrThrowAsync(Guid gateId, CancellationToken cancellationToken)
    {
        var gate = await _humanGateRepository.GetByIdAsync(gateId, false, cancellationToken)
            ?? throw new NotFoundException("Human gate not found");
        return gate;
    }

    private async Task ResetStepAsync(Guid stepId, Guid taskId, CancellationToken cancellationToken)
    {
        var step = await _taskStepRepository.GetByIdAsync(stepId, false, cancellationToken);
        if (step != null)
        {
            step.Status = TaskStepStatus.Pending;
            step.RetryCount++;
            await _taskStepRepository.UpdateAsync(step, cancellationToken);
            await AddTaskLogAsync(taskId, "Info", $"Step {step.Step} reset due to gate rejection", nameof(HumanGateService), cancellationToken);
        }
    }

    private async Task AddTaskLogAsync(Guid taskId, string level, string message, string source, CancellationToken cancellationToken)
    {
        var log = new TaskLogEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Level = level,
            Message = message,
            Source = source,
        };
        await _taskLogRepository.CreateAsync(log, cancellationToken);
    }

    private static HumanGateResponse ToResponse(HumanGateEntity entity)
    {
        object? modifications = null;
        if (!string.IsNullOrWhiteSpace(entity.ModificationsJson))
        {
            try
            {
                modifications = JsonSerializer.Deserialize<object>(entity.ModificationsJson);
            }
            catch
            {
                modifications = entity.ModificationsJson;
            }
        }

        return new HumanGateResponse
        {
            Id = entity.Id,
            TaskId = entity.TaskId,
            TaskStepId = entity.TaskStepId,
            GateType = entity.GateType.ToString(),
            GateTypeName = GetGateTypeName(entity.GateType),
            Status = entity.Status.ToString(),
            Reason = entity.Reason,
            HumanResponse = entity.HumanResponse,
            Modifications = modifications,
            ReviewerUserId = entity.ReviewerUserId,
            CreatedAtUtc = entity.CreatedAtUtc,
            RespondedAtUtc = entity.RespondedAtUtc,
        };
    }

    private static string GetGateTypeName(HumanGateType gateType)
    {
        return gateType switch
        {
            HumanGateType.RequirementConfirm => "需求确认",
            HumanGateType.PlanApproval => "方案审批",
            HumanGateType.CodeReview => "代码审核",
            HumanGateType.TestAcceptance => "测试验收",
            HumanGateType.MergeApproval => "合并审批",
            HumanGateType.FinalSignoff => "最终签收",
            HumanGateType.Emergency => "紧急介入",
            _ => gateType.ToString(),
        };
    }
}