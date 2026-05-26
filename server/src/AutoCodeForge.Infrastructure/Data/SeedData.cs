using AutoCodeForge.Core.Entities;
using AutoCodeForge.Infrastructure.Helpers;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Data;

/// <summary>
/// Inserts baseline development seed records.
/// </summary>
public class SeedData
{
    private readonly ISqlSugarClient _db;
    private readonly PasswordHelper _passwordHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedData"/> class.
    /// </summary>
    /// <param name="db">The SqlSugar client.</param>
    /// <param name="passwordHelper">Password helper.</param>
    public SeedData(ISqlSugarClient db, PasswordHelper passwordHelper)
    {
        _db = db;
        _passwordHelper = passwordHelper;
    }

    /// <summary>
    /// Seeds baseline data when no records exist.
    /// </summary>
    /// <returns>A task representing asynchronous execution.</returns>
    public async Task InitializeSeedDataAsync()
    {
        var llmModelConfigId = await EnsureDefaultLlmModelConfigAsync();

        var exists = await _db.Queryable<UserEntity>().AnyAsync(user => user.NtId == "demo.user");
        if (!exists)
        {
            await _db.Insertable(new UserEntity
            {
                Id = Guid.NewGuid(),
                NtId = "demo.user",
                UserName = "Demo User",
                Email = "demo@autocodeforge.local",
                PasswordHash = _passwordHelper.HashPassword("Demo@123456"),
                IsDeleted = false,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            }).ExecuteCommandAsync();
        }

        var hasGlobalConfig = await _db.Queryable<GlobalConfigEntity>()
            .AnyAsync(config => config.ConfigKey == "System.DefaultLanguage");
        if (!hasGlobalConfig)
        {
            await _db.Insertable(new GlobalConfigEntity
            {
                Id = Guid.NewGuid(),
                ConfigKey = "System.DefaultLanguage",
                ConfigValue = "en-US",
                Description = "Default language for new sessions",
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            }).ExecuteCommandAsync();
        }

        var hasAgent = await _db.Queryable<AgentEntity>().AnyAsync(agent => agent.Name == "default-worker");
        if (!hasAgent)
        {
            await _db.Insertable(new AgentEntity
            {
                Id = Guid.NewGuid(),
                NtId = "demo.user",
                Name = "default-worker",
                Description = "Default worker agent profile",
                LlmModelConfigId = llmModelConfigId,
                IsEnabled = true,
                IsDeleted = false,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            }).ExecuteCommandAsync();
        }

        // Seed Agents and Workflows
        await SeedAgentsAndWorkflowsAsync(llmModelConfigId);
    }

    /// <summary>
    /// Seeds Agents and Workflows for the system.
    /// </summary>
    /// <param name="llmModelConfigId">LLM Model Config ID for Agents.</param>
    private async Task SeedAgentsAndWorkflowsAsync(Guid llmModelConfigId)
    {
        // Agent 1: Secretary
        await EnsureAgentAsync(new AgentEntity
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            NtId = "system",
            Name = "Secretary",
            Description = "秘书 Agent - 负责接收任务、分配工作、协调流程",
            Keywords = "协调,分配,整理,汇总",
            SkillTags = "coordination,dispatch,summary",
            SystemPrompt = "你是一个专业的秘书，负责接收任务、协调资源、分配工作。你的职责是确保工作流程顺畅进行。",
            ToolNames = "TaskDispatcher,ResourceAllocator,ReportGenerator",
            State = AgentState.Idle,
            Role = AgentRole.Secretary,
            IsEnabled = true,
            LlmModelConfigId = llmModelConfigId,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        // Agent 2: Manager
        await EnsureAgentAsync(new AgentEntity
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            NtId = "system",
            Name = "Manager",
            Description = "管理者 Agent - 负责审批、决策、监控进度",
            Keywords = "审批,决策,监控,审核",
            SkillTags = "approval,decision,monitoring,audit",
            SystemPrompt = "你是一个经验丰富的管理者，负责审批重要决策、监控项目进度、确保质量标准。",
            ToolNames = "ApprovalSystem,ProgressMonitor,QualityChecker",
            State = AgentState.Idle,
            Role = AgentRole.Manager,
            IsEnabled = true,
            LlmModelConfigId = llmModelConfigId,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        // Agent 3: Project Initializer
        await EnsureAgentAsync(new AgentEntity
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            NtId = "system",
            Name = "ProjectInitializer",
            Description = "项目初始化 Agent - 负责创建项目文档、初始化项目结构",
            Keywords = "初始化,文档,项目结构,模板",
            SkillTags = "project-init,documentation,template",
            SystemPrompt = "你是一个专业的项目初始化专家，负责创建项目文档、初始化项目结构、设置开发环境。",
            ToolNames = "ProjectCreator,DocumentGenerator,TemplateManager",
            State = AgentState.Idle,
            Role = AgentRole.Worker,
            IsEnabled = true,
            LlmModelConfigId = llmModelConfigId,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        // Agent 4: Requirement Analyzer
        await EnsureAgentAsync(new AgentEntity
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            NtId = "system",
            Name = "RequirementAnalyzer",
            Description = "需求分析 Agent - 负责分析需求文档、创建分析计划",
            Keywords = "需求,分析,计划,规格",
            SkillTags = "requirement-analysis,planning,specification",
            SystemPrompt = "你是一个资深的需求分析师，负责分析需求文档、提取关键信息、制定详细实施计划。",
            ToolNames = "RequirementParser,PlanGenerator,SpecWriter",
            State = AgentState.Idle,
            Role = AgentRole.Worker,
            IsEnabled = true,
            LlmModelConfigId = llmModelConfigId,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        // Agent 5: Executor
        await EnsureAgentAsync(new AgentEntity
        {
            Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
            NtId = "system",
            Name = "Executor",
            Description = "执行者 Agent - 负责执行任务、生成代码、完成交付",
            Keywords = "执行,代码,开发,交付",
            SkillTags = "execution,coding,development,delivery",
            SystemPrompt = "你是一个高效的执行者，负责根据计划执行任务、生成代码、完成交付物。",
            ToolNames = "CodeGenerator,TaskExecutor,DeliveryManager",
            State = AgentState.Idle,
            Role = AgentRole.Worker,
            IsEnabled = true,
            LlmModelConfigId = llmModelConfigId,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        // Workflow 1: Issue Processing
        await EnsureWorkflowAsync(new WorkflowEntity
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            NtId = "system",
            Name = "Issue处理流程",
            Description = "处理 GitHub Issue 的标准流程：接收 → 分析 → 分配 → 执行 → 完成",
            Version = 1,
            NodesJson = GetIssueWorkflowNodes(),
            EdgesJson = GetIssueWorkflowEdges(),
            ContextProvidersJson = "[]",
            Status = WorkflowStatus.Published,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        // Workflow 2: Requirement Analysis
        await EnsureWorkflowAsync(new WorkflowEntity
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            NtId = "system",
            Name = "需求分析流程",
            Description = "需求分析标准流程：接收需求 → 分析需求 → 创建计划 → 审批 → 完成",
            Version = 1,
            NodesJson = GetRequirementWorkflowNodes(),
            EdgesJson = GetRequirementWorkflowEdges(),
            ContextProvidersJson = "[]",
            Status = WorkflowStatus.Published,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });

        // Workflow 3: Project Initialization
        await EnsureWorkflowAsync(new WorkflowEntity
        {
            Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
            NtId = "system",
            Name = "项目初始化流程",
            Description = "新项目初始化标准流程：创建项目 → 初始化文档 → 创建计划 → 审批 → 完成",
            Version = 1,
            NodesJson = GetProjectInitWorkflowNodes(),
            EdgesJson = GetProjectInitWorkflowEdges(),
            ContextProvidersJson = "[]",
            Status = WorkflowStatus.Published,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        });
    }

    private async Task EnsureAgentAsync(AgentEntity agent)
    {
        var exists = await _db.Queryable<AgentEntity>().AnyAsync(a => a.Name == agent.Name);
        if (!exists)
        {
            await _db.Insertable(agent).ExecuteCommandAsync();
        }
    }

    private async Task EnsureWorkflowAsync(WorkflowEntity workflow)
    {
        var exists = await _db.Queryable<WorkflowEntity>().AnyAsync(w => w.Name == workflow.Name);
        if (!exists)
        {
            await _db.Insertable(workflow).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// Gets the nodes JSON for Issue Processing workflow.
    /// </summary>
    private static string GetIssueWorkflowNodes()
    {
        return """
        [
          {
            "id": "start",
            "type": "start",
            "position": { "x": 250, "y": 0 },
            "data": { "label": "开始", "description": "接收 Issue" }
          },
          {
            "id": "analyze",
            "type": "agent",
            "position": { "x": 250, "y": 100 },
            "data": {
              "label": "分析 Issue",
              "agentId": "44444444-4444-4444-4444-444444444444",
              "agentName": "RequirementAnalyzer",
              "action": "analyze_issue",
              "timeout": 300
            }
          },
          {
            "id": "human-gate",
            "type": "human-gate",
            "position": { "x": 250, "y": 200 },
            "data": {
              "label": "审批确认",
              "gateType": "approval",
              "assignee": "22222222-2222-2222-2222-222222222222"
            }
          },
          {
            "id": "execute",
            "type": "agent",
            "position": { "x": 250, "y": 300 },
            "data": {
              "label": "执行任务",
              "agentId": "55555555-5555-5555-5555-555555555555",
              "agentName": "Executor",
              "action": "execute_issue",
              "timeout": 600
            }
          },
          {
            "id": "end",
            "type": "end",
            "position": { "x": 250, "y": 400 },
            "data": { "label": "完成", "status": "completed" }
          }
        ]
        """;
    }

    /// <summary>
    /// Gets the edges JSON for Issue Processing workflow.
    /// </summary>
    private static string GetIssueWorkflowEdges()
    {
        return """
        [
          { "id": "e-start-analyze", "source": "start", "target": "analyze", "type": "default" },
          { "id": "e-analyze-gate", "source": "analyze", "target": "human-gate", "type": "default" },
          { "id": "e-gate-execute", "source": "human-gate", "target": "execute", "type": "default", "condition": "approved" },
          { "id": "e-execute-end", "source": "execute", "target": "end", "type": "default" }
        ]
        """;
    }

    /// <summary>
    /// Gets the nodes JSON for Requirement Analysis workflow.
    /// </summary>
    private static string GetRequirementWorkflowNodes()
    {
        return """
        [
          {
            "id": "start",
            "type": "start",
            "position": { "x": 250, "y": 0 },
            "data": { "label": "开始", "description": "接收需求文档" }
          },
          {
            "id": "parse",
            "type": "agent",
            "position": { "x": 250, "y": 100 },
            "data": {
              "label": "解析需求",
              "agentId": "44444444-4444-4444-4444-444444444444",
              "agentName": "RequirementAnalyzer",
              "action": "parse_requirement",
              "timeout": 300
            }
          },
          {
            "id": "plan",
            "type": "agent",
            "position": { "x": 250, "y": 200 },
            "data": {
              "label": "创建分析计划",
              "agentId": "44444444-4444-4444-4444-444444444444",
              "agentName": "RequirementAnalyzer",
              "action": "create_plan",
              "timeout": 300
            }
          },
          {
            "id": "review",
            "type": "human-gate",
            "position": { "x": 250, "y": 300 },
            "data": {
              "label": "计划审批",
              "gateType": "approval",
              "assignee": "22222222-2222-2222-2222-222222222222"
            }
          },
          {
            "id": "end",
            "type": "end",
            "position": { "x": 250, "y": 400 },
            "data": { "label": "完成", "status": "completed" }
          }
        ]
        """;
    }

    /// <summary>
    /// Gets the edges JSON for Requirement Analysis workflow.
    /// </summary>
    private static string GetRequirementWorkflowEdges()
    {
        return """
        [
          { "id": "e-start-parse", "source": "start", "target": "parse", "type": "default" },
          { "id": "e-parse-plan", "source": "parse", "target": "plan", "type": "default" },
          { "id": "e-plan-review", "source": "plan", "target": "review", "type": "default" },
          { "id": "e-review-end", "source": "review", "target": "end", "type": "default", "condition": "approved" }
        ]
        """;
    }

    /// <summary>
    /// Gets the nodes JSON for Project Initialization workflow.
    /// </summary>
    private static string GetProjectInitWorkflowNodes()
    {
        return """
        [
          {
            "id": "start",
            "type": "start",
            "position": { "x": 250, "y": 0 },
            "data": { "label": "开始", "description": "新项目初始化" }
          },
          {
            "id": "create-docs",
            "type": "agent",
            "position": { "x": 250, "y": 100 },
            "data": {
              "label": "初始化项目文档",
              "agentId": "33333333-3333-3333-3333-333333333333",
              "agentName": "ProjectInitializer",
              "action": "init_docs",
              "timeout": 300
            }
          },
          {
            "id": "create-plan",
            "type": "agent",
            "position": { "x": 250, "y": 200 },
            "data": {
              "label": "创建实施计划",
              "agentId": "44444444-4444-4444-4444-444444444444",
              "agentName": "RequirementAnalyzer",
              "action": "create_implementation_plan",
              "timeout": 300
            }
          },
          {
            "id": "approval",
            "type": "human-gate",
            "position": { "x": 250, "y": 300 },
            "data": {
              "label": "项目审批",
              "gateType": "approval",
              "assignee": "22222222-2222-2222-2222-222222222222"
            }
          },
          {
            "id": "execute",
            "type": "agent",
            "position": { "x": 250, "y": 400 },
            "data": {
              "label": "执行初始化",
              "agentId": "33333333-3333-3333-3333-333333333333",
              "agentName": "ProjectInitializer",
              "action": "execute_init",
              "timeout": 600
            }
          },
          {
            "id": "end",
            "type": "end",
            "position": { "x": 250, "y": 500 },
            "data": { "label": "完成", "status": "completed" }
          }
        ]
        """;
    }

    /// <summary>
    /// Gets the edges JSON for Project Initialization workflow.
    /// </summary>
    private static string GetProjectInitWorkflowEdges()
    {
        return """
        [
          { "id": "e-start-docs", "source": "start", "target": "create-docs", "type": "default" },
          { "id": "e-docs-plan", "source": "create-docs", "target": "create-plan", "type": "default" },
          { "id": "e-plan-approval", "source": "create-plan", "target": "approval", "type": "default" },
          { "id": "e-approval-execute", "source": "approval", "target": "execute", "type": "default", "condition": "approved" },
          { "id": "e-execute-end", "source": "execute", "target": "end", "type": "default" }
        ]
        """;
    }

    private async Task<Guid> EnsureDefaultLlmModelConfigAsync()
    {
        var existing = await _db.Queryable<LLMModelConfigEntity>()
            .FirstAsync(config => config.NtId == "demo.user" && config.ModelName == "gpt-4o-mini");
        if (existing is not null)
        {
            return existing.Id;
        }

        var entity = new LLMModelConfigEntity
        {
            Id = Guid.NewGuid(),
            NtId = "demo.user",
            Provider = LLMProvider.AzureOpenAI,
            ModelName = "gpt-4o-mini",
            Endpoint = "https://example.local/azure-openai",
            ApiKey = "encrypted-placeholder",
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        await _db.Insertable(entity).ExecuteCommandAsync();
        return entity.Id;
    }
}
