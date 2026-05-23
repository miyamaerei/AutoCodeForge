using AutoCodeForge.Application.Configuration;
using AutoCodeForge.Application.Security;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.AI;
using AutoCodeForge.Infrastructure.BackgroundServices;
using AutoCodeForge.Infrastructure.Data;
using AutoCodeForge.Infrastructure.Git;
using AutoCodeForge.Infrastructure.Helpers;
using AutoCodeForge.Infrastructure.Logging;
using AutoCodeForge.Infrastructure.Repositories;
using AutoCodeForge.Infrastructure.Repositories.Base;
using AutoCodeForge.Infrastructure.Services;
using AutoCodeForge.Application.AI;
using AutoCodeForge.Application.Tools;
using Microsoft.Extensions.Options;

namespace AutoCodeForge.Api.Extensions;

/// <summary>
/// ServiceCollection 依赖注入扩展方法集合
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册核心基础服务
    /// </summary>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        return services;
    }

    /// <summary>
    /// 注册所有 Repository 依赖
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        services.AddScoped<UserRepository>();
        services.AddScoped<GlobalConfigRepository>();
        services.AddScoped<UserConfigRepository>();
        services.AddScoped<LLMModelConfigRepository>();
        services.AddScoped<AgentRepository>();
        services.AddScoped<AgentLearningRecordRepository>();
        services.AddScoped<AgentDormantRecordRepository>();
        services.AddScoped<ChatSessionRepository>();
        services.AddScoped<ChatMessageRepository>();
        services.AddScoped<TaskRepository>();
        services.AddScoped<TaskLogRepository>();
        services.AddScoped<TaskStepRepository>();
        services.AddScoped<HumanGateRepository>();
        services.AddScoped<RepoSandboxWorkspaceRepository>();
        services.AddScoped<ReviewRepository>();
        services.AddScoped<ReviewRuleSetRepository>();
        services.AddScoped<ScheduledTaskRepository>();
        services.AddScoped<ScheduledTaskExecutionRepository>();
        services.AddScoped<PipelineRepository>();
        services.AddScoped<BuildRepository>();
        services.AddScoped<WikiPageRepository>();
        services.AddScoped<GitSkillGrantRepository>();
        services.AddScoped<AgentToolInvocationRepository>();
        services.AddScoped<AdminAuditLogRepository>();
        services.AddScoped<ConfigRepository>();
        services.AddScoped<ConfigHistoryRepository>();
        services.AddScoped<RepositoryRepository>();
        return services;
    }

    /// <summary>
    /// 注册所有 Application Service 依赖
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<JwtService>();
        services.AddScoped<RepositoryService>();
        services.AddScoped<RepositoryReviewSettingsService>();
        services.AddScoped<RepoSyncService>();
        services.AddScoped<ReviewService>();
        services.AddScoped<ReviewRuleSetService>();
        services.AddScoped<AdminAuditService>();
        services.AddScoped<AutoCodeForge.Application.Validators.SandboxConfigValidator>();
        services.AddScoped<AuthService>();
        services.AddScoped<ChatDefaultsProvisioningService>();
        services.AddScoped<AgentService>();
        services.AddScoped<ChatService>();
        services.AddScoped<TaskService>();
        services.AddScoped<TaskStepService>();
        services.AddScoped<HumanGateService>();
        services.AddScoped<ScheduledTaskService>();
        services.AddScoped<PipelineService>();
        services.AddScoped<WikiService>();
        services.AddScoped<ConfigService>();
        services.AddScoped<ConfigHistoryService>();
        services.AddScoped<ConfigExportService>();
        services.AddScoped<ConfigInitializationService>();
        services.AddScoped<GitSkillPolicyService>();
        services.AddScoped<GitSkillPermissionGuard>();
        services.AddScoped<GitSkillErrorMapper>();
        services.AddScoped<GitContextHydrator>();
        services.AddScoped<AgentToolAuditLogger>();
        services.AddScoped<ChatSessionManager>();
        services.AddScoped<AgentExecutor>();
        services.AddScoped<AgentMatcher>();
        services.AddScoped<AgentFactory>();
        services.AddScoped<TaskExecutor>();
        services.AddScoped<AutoCodeForge.Infrastructure.Sandbox.SandboxPathResolver>();
        services.AddScoped<GitCloneService>();
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<SeedData>();
        services.AddSingleton<PasswordHelper>();
        services.AddScoped<DataProtectionService>();
        services.AddScoped<IAgentSelectionStrategy, LeastLoadAgentSelectionStrategy>();
        services.AddScoped<TaskOrchestrator>();
        services.AddScoped<ITaskEventPublisher, InMemoryTaskEventPublisher>();
        services.AddScoped<IArtifactStore, DatabaseArtifactStore>();
        services.AddScoped<ContextChainService>();
        services.AddScoped<FailureRecoveryService>();
        services.AddScoped<AgentRegistrationRepository>();
        services.AddScoped<IAgentRegistryService, AgentRegistryService>();
        return services;
    }

    /// <summary>
    /// 注册 AI/LLM 相关依赖
    /// </summary>
    public static IServiceCollection AddAIServices(this IServiceCollection services)
    {
        services.AddScoped<IReviewEngine, AutoCodeForge.Infrastructure.Review.RuleBasedReviewEngine>();
        services.AddScoped<ILlmGateway, AgentFrameworkGateway>();
        services.AddScoped<IGitHubCopilotService, GitHubCopilotCliService>();
        return services;
    }

    /// <summary>
    /// 注册 Agent Tool 依赖
    /// </summary>
    public static IServiceCollection AddAgentTools(this IServiceCollection services)
    {
        services.AddScoped<IAgentTool, GitReadToolset>();
        services.AddScoped<IAgentTool, GitWriteToolset>();
        return services;
    }

    /// <summary>
    /// 注册 Git Provider 依赖
    /// </summary>
    public static IServiceCollection AddGitServices(this IServiceCollection services)
    {
        services.AddScoped<LibGit2SharpProvider>(sp =>
        {
            var gitOptions = sp.GetRequiredService<IOptions<GitOptions>>().Value;
            return new LibGit2SharpProvider(gitOptions);
        });
        services.AddScoped<GitProviderFactory>(sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
            return new GitProviderFactory(httpClient);
        });
        return services;
    }

    /// <summary>
    /// 注册 Background Services
    /// </summary>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<TaskQueueService>();
        services.AddHostedService<CronSchedulerService>();
        services.AddHostedService<PipelineSyncService>();
        services.AddScoped<AutoCodeForge.Infrastructure.BackgroundServices.Handlers.RepoSyncTaskHandler>();
        services.AddScoped<AutoCodeForge.Infrastructure.BackgroundServices.Handlers.ReviewTaskHandler>();
        return services;
    }

    /// <summary>
    /// 注册带工厂方法的特殊依赖
    /// </summary>
    public static IServiceCollection AddFactoryServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<EncryptionService>(sp =>
        {
            var encryptionKey = configuration["Encryption:Key"];
            if (string.IsNullOrWhiteSpace(encryptionKey) || encryptionKey == "CHANGE_ME_IN_PRODUCTION_WITH_A_32_BYTE_BASE64_KEY")
            {
                encryptionKey = EncryptionService.GenerateKey();
            }
            return new EncryptionService(encryptionKey);
        });
        return services;
    }
}
