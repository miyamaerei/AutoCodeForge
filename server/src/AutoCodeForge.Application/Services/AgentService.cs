using AutoCodeForge.Core.DTOs.Agent;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides agent CRUD and matching operations.
/// </summary>
public class AgentService
{
    private readonly AgentRepository _agentRepository;
    private readonly AgentLearningRecordRepository _learningRecordRepository;
    private readonly AgentDormantRecordRepository _dormantRecordRepository;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentService"/> class.
    /// </summary>
    /// <param name="agentRepository">The agent repository.</param>
    /// <param name="learningRecordRepository">The learning record repository.</param>
    /// <param name="dormantRecordRepository">The dormant record repository.</param>
    /// <param name="currentUser">The current user.</param>
    public AgentService(
        AgentRepository agentRepository,
        AgentLearningRecordRepository learningRecordRepository,
        AgentDormantRecordRepository dormantRecordRepository,
        ICurrentUser currentUser)
    {
        _agentRepository = agentRepository;
        _learningRecordRepository = learningRecordRepository;
        _dormantRecordRepository = dormantRecordRepository;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Creates one agent.
    /// </summary>
    /// <param name="request">The create request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created agent.</returns>
    public async Task<AgentResponse> CreateAsync(CreateAgentRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Keywords = request.Keywords?.Trim(),
            SystemPrompt = request.SystemPrompt,
            LlmModelConfigId = request.LlmModelConfigId,
            IsEnabled = request.IsEnabled,
            State = AgentState.Idle,
            StateChangedAtUtc = DateTime.UtcNow,
        };

        // 解析角色
        if (!string.IsNullOrWhiteSpace(request.Role) && Enum.TryParse<AgentRole>(request.Role, true, out var role))
        {
            entity.Role = role;
        }

        var created = await _agentRepository.CreateAsync(entity, cancellationToken);
        return ToResponse(created);
    }

    /// <summary>
    /// Updates one agent.
    /// </summary>
    /// <param name="id">The agent identifier.</param>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated agent.</returns>
    public async Task<AgentResponse> UpdateAsync(
        Guid id,
        UpdateAgentRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _agentRepository.GetByIdAsync(id, false, cancellationToken)
            ?? throw new NotFoundException("Agent not found");

        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.Keywords = request.Keywords?.Trim();
        entity.SystemPrompt = request.SystemPrompt;
        entity.LlmModelConfigId = request.LlmModelConfigId;
        entity.IsEnabled = request.IsEnabled;

        // 解析角色
        if (!string.IsNullOrWhiteSpace(request.Role) && Enum.TryParse<AgentRole>(request.Role, true, out var role))
        {
            entity.Role = role;
        }

        await _agentRepository.UpdateAsync(entity, cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// Gets one agent by identifier.
    /// </summary>
    /// <param name="id">The agent identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The agent response.</returns>
    public async Task<AgentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _agentRepository.GetByIdAsync(id, false, cancellationToken)
            ?? throw new NotFoundException("Agent not found");

        return ToResponse(entity);
    }

    /// <summary>
    /// Gets paged agents.
    /// </summary>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paged agents response.</returns>
    public async Task<PagedResult<AgentResponse>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var paged = await _agentRepository.GetPagedAsync(page, pageSize, false, cancellationToken);
        return new PagedResult<AgentResponse>(
            paged.Items.Select(ToResponse).ToList(),
            paged.TotalCount,
            paged.Page,
            paged.PageSize);
    }

    /// <summary>
    /// Deletes one agent.
    /// </summary>
    /// <param name="id">The agent identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _agentRepository.SoftDeleteAsync(id, false, cancellationToken);
    }

    /// <summary>
    /// Matches the best enabled agent for one input.
    /// </summary>
    /// <param name="userInput">The user input.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The best matched agent or null.</returns>
    public async Task<AgentResponse?> MatchByInputAsync(string userInput, CancellationToken cancellationToken = default)
    {
        var enabled = await _agentRepository.GetEnabledAsync(cancellationToken);
        if (enabled.Count == 0)
        {
            return null;
        }

        var best = enabled
            .Select(agent => new
            {
                Agent = agent,
                Score = Score(agent.Keywords, userInput),
            })
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Agent.Name)
            .First()
            .Agent;

        return ToResponse(best);
    }

    /// <summary>
    /// 获取指定状态的Agent列表
    /// </summary>
    public async Task<List<AgentResponse>> GetByStateAsync(AgentState state, CancellationToken cancellationToken = default)
    {
        var agents = await _agentRepository.GetByStateAsync(state, cancellationToken);
        return agents.Select(ToResponse).ToList();
    }

    /// <summary>
    /// 获取所有休眠状态的Agent列表
    /// </summary>
    public async Task<List<AgentResponse>> GetDormantAgentsAsync(CancellationToken cancellationToken = default)
    {
        var agents = await _agentRepository.GetDormantAgentsAsync(cancellationToken);
        return agents.Select(ToResponse).ToList();
    }

    /// <summary>
    /// 分配任务给Agent
    /// </summary>
    public async Task<AgentResponse> AssignTaskAsync(Guid agentId, AssignTaskRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _agentRepository.GetByIdAsync(agentId, false, cancellationToken)
            ?? throw new NotFoundException("Agent not found");

        // 验证状态转换合法性
        if (entity.State != AgentState.Idle)
        {
            throw new InvalidOperationException($"Agent is currently in {entity.State} state, cannot assign task");
        }

        // 执行状态转换
        entity.State = AgentState.Handling;
        entity.StateChangedAtUtc = DateTime.UtcNow;
        entity.CurrentTaskId = request.TaskId;
        entity.Version++;

        await _agentRepository.UpdateAsync(entity, cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// 标记任务完成
    /// </summary>
    public async Task<AgentResponse> CompleteTaskAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var entity = await _agentRepository.GetByIdAsync(agentId, false, cancellationToken)
            ?? throw new NotFoundException("Agent not found");

        if (entity.State != AgentState.Handling)
        {
            throw new InvalidOperationException($"Agent is currently in {entity.State} state, cannot complete task");
        }

        // 执行状态转换
        entity.State = AgentState.Idle;
        entity.StateChangedAtUtc = DateTime.UtcNow;
        entity.CurrentTaskId = null;
        entity.Version++;

        await _agentRepository.UpdateAsync(entity, cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// 标记任务失败
    /// </summary>
    public async Task<AgentResponse> FailTaskAsync(Guid agentId, FailTaskRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _agentRepository.GetByIdAsync(agentId, false, cancellationToken)
            ?? throw new NotFoundException("Agent not found");

        if (entity.State != AgentState.Handling)
        {
            throw new InvalidOperationException($"Agent is currently in {entity.State} state, cannot mark task as failed");
        }

        // 执行状态转换
        entity.State = AgentState.Idle;
        entity.StateChangedAtUtc = DateTime.UtcNow;
        entity.CurrentTaskId = null;
        entity.Version++;

        await _agentRepository.UpdateAsync(entity, cancellationToken);

        // 记录失败信息，触发异常学习
        await _learningRecordRepository.CreateAsync(new AgentLearningRecordEntity
        {
            Id = Guid.NewGuid(),
            AgentId = agentId,
            TriggerType = LearningTriggerType.Exception,
            TriggerReason = $"Task failed: {request.Reason}",
            IsSuccessful = false,
            ErrorMessage = request.Reason,
            EffectivenessScore = 30,
        }, cancellationToken);

        return ToResponse(entity);
    }

    /// <summary>
    /// 触发Learning
    /// </summary>
    public async Task<AgentResponse> StartLearningAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var entity = await _agentRepository.GetByIdAsync(agentId, false, cancellationToken)
            ?? throw new NotFoundException("Agent not found");

        if (entity.State != AgentState.Idle)
        {
            throw new InvalidOperationException($"Agent is currently in {entity.State} state, cannot start learning");
        }

        // 执行状态转换
        entity.State = AgentState.Learning;
        entity.StateChangedAtUtc = DateTime.UtcNow;
        entity.Version++;

        await _agentRepository.UpdateAsync(entity, cancellationToken);
        return ToResponse(entity);
    }

    /// <summary>
    /// 完成Learning
    /// </summary>
    public async Task<AgentResponse> CompleteLearningAsync(Guid agentId, CompleteLearningRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _agentRepository.GetByIdAsync(agentId, false, cancellationToken)
            ?? throw new NotFoundException("Agent not found");

        if (entity.State != AgentState.Learning)
        {
            throw new InvalidOperationException($"Agent is currently in {entity.State} state, cannot complete learning");
        }

        // 执行状态转换
        entity.State = AgentState.Idle;
        entity.StateChangedAtUtc = DateTime.UtcNow;

        // 更新技能标签
        if (!string.IsNullOrWhiteSpace(request.SkillTags))
        {
            var existingTags = entity.SkillTags ?? string.Empty;
            entity.SkillTags = string.IsNullOrWhiteSpace(existingTags)
                ? request.SkillTags
                : $"{existingTags},{request.SkillTags}";
        }

        entity.Version++;

        await _agentRepository.UpdateAsync(entity, cancellationToken);

        // 创建学习记录
        await _learningRecordRepository.CreateAsync(new AgentLearningRecordEntity
        {
            Id = Guid.NewGuid(),
            AgentId = agentId,
            TriggerType = LearningTriggerType.Manual,
            TriggerReason = request.Summary,
            IsSuccessful = true,
            LearningResult = request.Result,
            NewSkillTags = request.SkillTags,
            CompletedAtUtc = DateTime.UtcNow,
            EffectivenessScore = 80,
        }, cancellationToken);

        return ToResponse(entity);
    }

    /// <summary>
    /// 进入休眠状态
    /// </summary>
    public async Task<AgentResponse> EnterDormantAsync(Guid agentId, EnterDormantRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _agentRepository.GetByIdAsync(agentId, false, cancellationToken)
            ?? throw new NotFoundException("Agent not found");

        if (entity.State == AgentState.Handling)
        {
            throw new InvalidOperationException("Agent is currently handling a task, cannot enter dormant state");
        }

        // 执行状态转换
        entity.State = AgentState.Dormant;
        entity.StateChangedAtUtc = DateTime.UtcNow;
        entity.DormantReason = request.Reason;
        entity.Version++;

        await _agentRepository.UpdateAsync(entity, cancellationToken);

        // 记录休眠历史
        await _dormantRecordRepository.CreateAsync(new AgentDormantRecordEntity
        {
            Id = Guid.NewGuid(),
            AgentId = agentId,
            ReasonType = DormantReasonType.Manual,
            ReasonDescription = request.Reason,
            DormantAtUtc = DateTime.UtcNow,
            IsWoken = false,
        }, cancellationToken);

        return ToResponse(entity);
    }

    /// <summary>
    /// 唤醒Agent
    /// </summary>
    public async Task<AgentResponse> WakeUpAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        return await WakeUpAsync(agentId, null, cancellationToken);
    }

    /// <summary>
    /// 唤醒Agent
    /// </summary>
    public async Task<AgentResponse> WakeUpAsync(Guid agentId, string? remark, CancellationToken cancellationToken = default)
    {
        var entity = await _agentRepository.GetByIdAsync(agentId, false, cancellationToken)
            ?? throw new NotFoundException("Agent not found");

        if (entity.State != AgentState.Dormant)
        {
            throw new InvalidOperationException($"Agent is currently in {entity.State} state, cannot wake up");
        }

        // 执行状态转换
        entity.State = AgentState.Idle;
        entity.StateChangedAtUtc = DateTime.UtcNow;
        entity.DormantReason = null;
        entity.Version++;

        await _agentRepository.UpdateAsync(entity, cancellationToken);

        // 更新休眠记录
        var dormantRecord = await _dormantRecordRepository.GetCurrentDormantRecordAsync(agentId, cancellationToken);
        if (dormantRecord != null)
        {
            dormantRecord.IsWoken = true;
            dormantRecord.WokenAtUtc = DateTime.UtcNow;
            dormantRecord.WokenByNtId = _currentUser.GetCurrentNtId();
            dormantRecord.WokenRemark = remark;
            await _dormantRecordRepository.UpdateAsync(dormantRecord, cancellationToken);
        }

        return ToResponse(entity);
    }

    /// <summary>
    /// 获取Agent的学习记录
    /// </summary>
    public async Task<List<AgentLearningRecordResponse>> GetLearningRecordsAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var records = await _learningRecordRepository.GetByAgentIdAsync(agentId, cancellationToken);
        return records.Select(ToLearningRecordResponse).ToList();
    }

    /// <summary>
    /// 获取Agent的休眠记录
    /// </summary>
    public async Task<List<AgentDormantRecordResponse>> GetDormantRecordsAsync(Guid agentId, CancellationToken cancellationToken = default)
    {
        var records = await _dormantRecordRepository.GetByAgentIdAsync(agentId, cancellationToken);
        return records.Select(ToDormantRecordResponse).ToList();
    }

    /// <summary>
    /// 获取所有Agent
    /// </summary>
    public async Task<List<AgentResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var agents = await _agentRepository.GetAllAsync(true, cancellationToken);
        return agents.Select(ToResponse).ToList();
    }

    /// <summary>
    /// 获取所有学习记录
    /// </summary>
    public async Task<List<AgentLearningRecordEntity>> GetAllLearningRecordsAsync(CancellationToken cancellationToken = default)
    {
        return await _learningRecordRepository.GetAllAsync(true, cancellationToken);
    }

    private static int Score(string? keywords, string userInput)
    {
        if (string.IsNullOrWhiteSpace(keywords) || string.IsNullOrWhiteSpace(userInput))
        {
            return 0;
        }

        var split = keywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return split.Count(keyword => userInput.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static AgentResponse ToResponse(AgentEntity entity)
    {
        return new AgentResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            Keywords = entity.Keywords,
            SystemPrompt = entity.SystemPrompt,
            LlmModelConfigId = entity.LlmModelConfigId,
            IsEnabled = entity.IsEnabled,
            State = entity.State.ToString(),
            Role = entity.Role.ToString(),
            StateChangedAtUtc = entity.StateChangedAtUtc,
            CurrentTaskId = entity.CurrentTaskId,
            DormantReason = entity.DormantReason,
            SkillTags = entity.SkillTags,
            LearningProgress = entity.LearningProgress,
            Version = entity.Version,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
        };
    }

    private static AgentLearningRecordResponse ToLearningRecordResponse(AgentLearningRecordEntity entity)
    {
        return new AgentLearningRecordResponse
        {
            Id = entity.Id,
            AgentId = entity.AgentId,
            TriggerType = entity.TriggerType.ToString(),
            TriggerReason = entity.TriggerReason,
            RelatedTaskId = entity.RelatedTaskId,
            StartedAtUtc = entity.StartedAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc,
            IsSuccessful = entity.IsSuccessful,
            LearningResult = entity.LearningResult,
            NewSkillTags = entity.NewSkillTags,
            ErrorMessage = entity.ErrorMessage,
            EffectivenessScore = entity.EffectivenessScore,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }

    private static AgentDormantRecordResponse ToDormantRecordResponse(AgentDormantRecordEntity entity)
    {
        return new AgentDormantRecordResponse
        {
            Id = entity.Id,
            AgentId = entity.AgentId,
            ReasonType = entity.ReasonType.ToString(),
            ReasonDescription = entity.ReasonDescription,
            DormantAtUtc = entity.DormantAtUtc,
            WokenAtUtc = entity.WokenAtUtc,
            IsWoken = entity.IsWoken,
            WokenByNtId = entity.WokenByNtId,
            WokenRemark = entity.WokenRemark,
            CreatedAtUtc = entity.CreatedAtUtc,
        };
    }
}
