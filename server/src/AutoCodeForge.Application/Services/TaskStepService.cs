using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// 提供工序步骤的业务逻辑
/// </summary>
public class TaskStepService
{
    private readonly TaskStepRepository _taskStepRepository;
    private readonly TaskRepository _taskRepository;
    private readonly ISqlSugarClient _db;

    /// <summary>
    /// 初始化 TaskStepService 实例
    /// </summary>
    /// <param name="taskStepRepository">工序步骤 Repository</param>
    /// <param name="taskRepository">任务 Repository</param>
    /// <param name="db">SqlSugar 客户端</param>
    public TaskStepService(
        TaskStepRepository taskStepRepository,
        TaskRepository taskRepository,
        ISqlSugarClient db)
    {
        _taskStepRepository = taskStepRepository;
        _taskRepository = taskRepository;
        _db = db;
    }

    /// <summary>
    /// 初始化任务的7步工序
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>初始化后的工序列表</returns>
    public async Task<List<TaskStepResponse>> InitializeStepsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException("Task not found");

        if (task.Status != Core.Entities.TaskStatus.Pending)
        {
            throw new ValidationException("Only pending task can initialize steps");
        }

        // 检查是否已经初始化
        var existingSteps = await _taskStepRepository.GetByTaskIdAsync(taskId, cancellationToken);
        if (existingSteps.Any())
        {
            throw new ValidationException("Steps already initialized");
        }

        var steps = new List<TaskStepEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                Step = 1,
                StepType = TaskStepType.DemandAnalyse,
                Status = TaskStepStatus.Pending,
                Input = task.Input,
                RetryCount = 0,
                Version = 1
            },
            new()
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                Step = 2,
                StepType = TaskStepType.QueryCurrent,
                Status = TaskStepStatus.Pending,
                RetryCount = 0,
                Version = 1
            },
            new()
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                Step = 3,
                StepType = TaskStepType.MakePlan,
                Status = TaskStepStatus.Pending,
                RetryCount = 0,
                Version = 1
            },
            new()
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                Step = 4,
                StepType = TaskStepType.Development,
                Status = TaskStepStatus.Pending,
                RetryCount = 0,
                Version = 1
            },
            new()
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                Step = 5,
                StepType = TaskStepType.TestVerify,
                Status = TaskStepStatus.Pending,
                RetryCount = 0,
                Version = 1
            },
            new()
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                Step = 6,
                StepType = TaskStepType.CommitPr,
                Status = TaskStepStatus.Pending,
                RetryCount = 0,
                Version = 1
            },
            new()
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                Step = 7,
                StepType = TaskStepType.FinalAudit,
                Status = TaskStepStatus.Pending,
                RetryCount = 0,
                Version = 1
            }
        };

        foreach (var step in steps)
        {
            await _taskStepRepository.CreateAsync(step, cancellationToken);
        }

        // 更新任务的当前工序
        task.CurrentStep = 1;
        task.CurrentStepId = steps[0].Id;
        await _taskRepository.UpdateAsync(task, cancellationToken);

        return steps.Select(ToResponse).ToList();
    }

    /// <summary>
    /// 推进工序到下一步
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="stepId">当前工序 ID</param>
    /// <param name="request">推进请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>下一个工序</returns>
    public async Task<TaskStepResponse> AdvanceStepAsync(
        Guid taskId,
        Guid stepId,
        AdvanceTaskStepRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException("Task not found");

        var step = await _taskStepRepository.GetByIdAsync(stepId, false,cancellationToken)
            ?? throw new NotFoundException("Step not found");

        if (step.TaskId != taskId)
        {
            throw new ValidationException("Step does not belong to this task");
        }

        if (step.Status != TaskStepStatus.Handling)
        {
            throw new ValidationException("Only handling step can be advanced");
        }

        if (string.IsNullOrWhiteSpace(request.Output))
        {
            throw new ValidationException("Output is required");
        }

        // 更新当前工序
        step.Status = TaskStepStatus.Completed;
        step.Output = request.Output.Trim();
        step.CompletedAtUtc = DateTime.UtcNow;
        step.Version++;
        await _taskStepRepository.UpdateAsync(step, cancellationToken);

        // 获取所有工序
        var allSteps = await _taskStepRepository.GetByTaskIdAsync(taskId, cancellationToken);
        var nextStep = allSteps.FirstOrDefault(s => s.Step == step.Step + 1);

        if (nextStep != null)
        {
            // 更新下一个工序的输入
            nextStep.Input = step.Output;
            nextStep.Status = TaskStepStatus.Pending;
            nextStep.Version++;
            await _taskStepRepository.UpdateAsync(nextStep, cancellationToken);

            // 更新任务的当前工序
            task.CurrentStep = nextStep.Step;
            task.CurrentStepId = nextStep.Id;
            await _taskRepository.UpdateAsync(task, cancellationToken);

            return ToResponse(nextStep);
        }
        else
        {
            // 所有工序都完成，标记任务为完成
            task.Status = Core.Entities.TaskStatus.Completed;
            task.CurrentStep = null;
            task.CurrentStepId = null;
            task.CompletedAtUtc = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task, cancellationToken);

            return ToResponse(step);
        }
    }

    /// <summary>
    /// 跳过工序
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="stepId">要跳过的工序 ID</param>
    /// <param name="request">跳过请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>下一个工序</returns>
    public async Task<TaskStepResponse> SkipStepAsync(
        Guid taskId,
        Guid stepId,
        SkipTaskStepRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException("Task not found");

        var step = await _taskStepRepository.GetByIdAsync(stepId,false, cancellationToken)
            ?? throw new NotFoundException("Step not found");

        if (step.TaskId != taskId)
        {
            throw new ValidationException("Step does not belong to this task");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ValidationException("Skip reason is required");
        }

        // 获取活跃工序
        var activeStep = await _taskStepRepository.GetActiveStepAsync(taskId, cancellationToken);
        if (activeStep == null)
        {
            throw new ValidationException("No active step found");
        }

        // 只能跳过活跃工序的下一个
        if (step.Step != activeStep.Step + 1)
        {
            throw new ValidationException("Can only skip the next step of the active step");
        }

        // 标记工序为跳过
        step.Status = TaskStepStatus.Skipped;
        step.SkipReason = request.Reason.Trim();
        step.Version++;
        await _taskStepRepository.UpdateAsync(step, cancellationToken);

        // 获取所有工序
        var allSteps = await _taskStepRepository.GetByTaskIdAsync(taskId, cancellationToken);
        var nextStep = allSteps.FirstOrDefault(s => s.Step == step.Step + 1);

        if (nextStep != null)
        {
            // 激活再下一个工序
            nextStep.Status = TaskStepStatus.Pending;
            nextStep.Version++;
            await _taskStepRepository.UpdateAsync(nextStep, cancellationToken);

            task.CurrentStep = nextStep.Step;
            task.CurrentStepId = nextStep.Id;
            await _taskRepository.UpdateAsync(task, cancellationToken);

            return ToResponse(nextStep);
        }
        else
        {
            // 没有更多工序，任务完成
            task.Status = Core.Entities.TaskStatus.Completed;
            task.CurrentStep = null;
            task.CurrentStepId = null;
            task.CompletedAtUtc = DateTime.UtcNow;
            await _taskRepository.UpdateAsync(task, cancellationToken);

            return ToResponse(step);
        }
    }

    /// <summary>
    /// 解绑工序（处理超时或失败）
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="stepId">工序 ID</param>
    /// <param name="request">解绑请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>解绑后的工序</returns>
    public async Task<TaskStepResponse> UnbindStepAsync(
        Guid taskId,
        Guid stepId,
        UnbindTaskStepRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException("Task not found");

        var step = await _taskStepRepository.GetByIdAsync(stepId,false, cancellationToken)
            ?? throw new NotFoundException("Step not found");

        if (step.TaskId != taskId)
        {
            throw new ValidationException("Step does not belong to this task");
        }

        if (step.Status != TaskStepStatus.Handling)
        {
            throw new ValidationException("Only handling step can be unbound");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ValidationException("Unbind reason is required");
        }

        // 使用事务确保原子性
        await _db.Ado.UseTranAsync(async () =>
        {
            // 判断是否为前置工序（1-3）
            var isEarlyStep = step.Step <= 3;

            if (isEarlyStep)
            {
                // 前置工序失败，标记为 Failed
                step.Status = TaskStepStatus.Failed;
                step.RetryCount++;
                step.Version++;
                await _taskStepRepository.UpdateAsync(step, cancellationToken);

                // 标记任务为失败
                task.Status = Core.Entities.TaskStatus.Failed;
                task.ErrorMessage = $"Step {step.Step} failed: {request.Reason}";
                task.CompletedAtUtc = DateTime.UtcNow;
                await _taskRepository.UpdateAsync(task, cancellationToken);
            }
            else
            {
                // 后置工序失败，重置为 Pending
                step.Status = TaskStepStatus.Pending;
                step.WorkerAgentId = null;
                step.RetryCount = 0;
                step.StartedAtUtc = null;
                step.Version++;
                await _taskStepRepository.UpdateAsync(step, cancellationToken);
            }
        });

        return ToResponse(step);
    }

    /// <summary>
    /// 构建工序上下文
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="currentStepId">当前工序 ID</param>
    /// <param name="maxTokens">最大 Token 数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>上下文字符串</returns>
    public async Task<string> BuildContextAsync(
        Guid taskId,
        Guid? currentStepId = null,
        int maxTokens = 8000,
        CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, false, cancellationToken)
            ?? throw new NotFoundException("Task not found");

        var completedSteps = await _taskStepRepository.GetCompletedStepsAsync(taskId, currentStepId, cancellationToken);

        // 构建上下文
        var contextParts = new List<string>
        {
            $"# Original Input:\n{task.Input}\n"
        };

        // 最近2个完整保留
        var recentSteps = completedSteps.TakeLast(2).ToList();
        foreach (var step in recentSteps)
        {
            contextParts.Add($"# Step {step.Step} ({step.StepType}):\n{step.Output}\n");
        }

        // 更早的只保留摘要（截断）
        var earlierSteps = completedSteps.Take(completedSteps.Count - 2).ToList();
        foreach (var step in earlierSteps)
        {
            var summary = TruncateToTokens(step.Output ?? "", 500);
            contextParts.Add($"# Step {step.Step} ({step.StepType}) Summary:\n{summary}\n");
        }

        var fullContext = string.Join("\n", contextParts);

        // 如果总长度超过限制，按优先级截断
        if (EstimateTokenCount(fullContext) > maxTokens)
        {
            // 简化实现：只保留原始输入和最近2个步骤
            contextParts = new List<string>
            {
                $"# Original Input:\n{task.Input}\n"
            };

            foreach (var step in recentSteps)
            {
                contextParts.Add($"# Step {step.Step} ({step.StepType}):\n{step.Output}\n");
            }

            fullContext = string.Join("\n", contextParts);
        }

        return fullContext;
    }

    /// <summary>
    /// 获取任务的所有工序
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工序列表</returns>
    public async Task<List<TaskStepResponse>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var steps = await _taskStepRepository.GetByTaskIdAsync(taskId, cancellationToken);
        return steps.Select(ToResponse).ToList();
    }

    /// <summary>
    /// 获取任务的当前活跃工序
    /// </summary>
    /// <param name="taskId">任务 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>当前活跃工序</returns>
    public async Task<TaskStepResponse?> GetActiveStepAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var step = await _taskStepRepository.GetActiveStepAsync(taskId, cancellationToken);
        return step != null ? ToResponse(step) : null;
    }

    /// <summary>
    /// 获取工序详情
    /// </summary>
    /// <param name="stepId">工序 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>工序详情</returns>
    public async Task<TaskStepResponse> GetByIdAsync(Guid stepId, CancellationToken cancellationToken = default)
    {
        var step = await _taskStepRepository.GetByIdAsync(stepId,false, cancellationToken)
            ?? throw new NotFoundException("Step not found");
        return ToResponse(step);
    }

    /// <summary>
    /// 更新工序
    /// </summary>
    /// <param name="stepId">工序 ID</param>
    /// <param name="request">更新请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>更新后的工序</returns>
    public async Task<TaskStepResponse> UpdateAsync(
        Guid stepId,
        UpdateTaskStepRequest request,
        CancellationToken cancellationToken = default)
    {
        var step = await _taskStepRepository.GetByIdAsync(stepId,false, cancellationToken)
            ?? throw new NotFoundException("Step not found");

        if (request.Status.HasValue)
            step.Status = request.Status.Value;

        if (request.WorkerAgentId.HasValue)
            step.WorkerAgentId = request.WorkerAgentId.Value;

        if (request.ReviewerAgentId.HasValue)
            step.ReviewerAgentId = request.ReviewerAgentId.Value;

        if (request.Input != null)
            step.Input = request.Input.Trim();

        if (request.Output != null)
            step.Output = request.Output.Trim();

        if (request.SkipReason != null)
            step.SkipReason = request.SkipReason.Trim();

        step.Version++;
        await _taskStepRepository.UpdateAsync(step, cancellationToken);

        return ToResponse(step);
    }

    private static TaskStepResponse ToResponse(TaskStepEntity entity)
    {
        return new TaskStepResponse
        {
            Id = entity.Id,
            TaskId = entity.TaskId,
            Step = entity.Step,
            StepType = entity.StepType.ToString(),
            Status = entity.Status.ToString(),
            WorkerAgentId = entity.WorkerAgentId,
            ReviewerAgentId = entity.ReviewerAgentId,
            Input = entity.Input,
            Output = entity.Output,
            SkipReason = entity.SkipReason,
            StartedAtUtc = entity.StartedAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc,
            RetryCount = entity.RetryCount,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }

    private static string TruncateToTokens(string text, int maxTokens)
    {
        // 简化实现：假设每个字符约0.5个 token
        var maxChars = maxTokens * 2;
        if (text.Length <= maxChars)
            return text;

        return text.Substring(0, maxChars) + "...";
    }

    private static int EstimateTokenCount(string text)
    {
        // 简化实现：假设每个字符约0.5个 token
        return (int)(text.Length * 0.5);
    }
}
