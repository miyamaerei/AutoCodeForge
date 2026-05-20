using AutoCodeForge.Core.Enums;
using AutoCodeForge.Core.Helpers;

namespace AutoCodeForge.Core.Providers;

/// <summary>
/// Provides default configuration values for all configuration modules.
/// </summary>
public static class ConfigDefaultsProvider
{
    /// <summary>
    /// Gets the default configuration value for the specified type.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <returns>The default configuration value as JSON string.</returns>
    public static string GetDefaultValue(ConfigType configType)
    {
        return configType switch
        {
            ConfigType.Preferences => GetPreferencesDefault(),
            ConfigType.Sandbox => GetSandboxDefault(),
            ConfigType.Workflow => GetWorkflowDefault(),
            ConfigType.Notification => GetNotificationDefault(),
            ConfigType.Knowledge => GetKnowledgeDefault(),
            ConfigType.Schedule => GetScheduleDefault(),
            ConfigType.Review => GetReviewDefault(),
            ConfigType.DeepWiki => GetDeepWikiDefault(),
            ConfigType.Integration => GetIntegrationDefault(),
            ConfigType.Skill => GetSkillDefault(),
            ConfigType.Repository => GetRepositoryDefault(),
            ConfigType.ApiKey => GetApiKeyDefault(),
            ConfigType.Model => GetModelDefault(),
            ConfigType.System => GetSystemDefault(),
            _ => "{}"
        };
    }

    /// <summary>
    /// Gets the default configuration key for the specified type.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <returns>The default configuration key.</returns>
    public static string GetDefaultKey(ConfigType configType)
    {
        return configType switch
        {
            ConfigType.Global => "global.default",
            ConfigType.User => "user.default",
            ConfigType.Preferences => "user.preferences.main",
            ConfigType.Repository => "user.repository.default",
            ConfigType.Knowledge => "user.knowledge.default",
            ConfigType.Skill => "user.skill.default",
            ConfigType.Schedule => "user.schedule.default",
            ConfigType.DeepWiki => "user.deepwiki.default",
            ConfigType.Review => "user.review.default",
            ConfigType.Integration => "user.integration.default",
            ConfigType.Notification => "user.notification.default",
            ConfigType.Sandbox => "user.sandbox.default",
            ConfigType.Workflow => "user.workflow.team-default",
            ConfigType.ApiKey => "global.api.default",
            ConfigType.Model => "global.model.default",
            ConfigType.System => "global.system.version",
            _ => "unknown.default"
        };
    }

    /// <summary>
    /// Gets the description for the specified configuration type.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <returns>The description.</returns>
    public static string GetDescription(ConfigType configType)
    {
        return configType switch
        {
            ConfigType.Global => "全局配置",
            ConfigType.User => "用户配置",
            ConfigType.Preferences => "用户偏好设置",
            ConfigType.Repository => "仓库集成配置",
            ConfigType.Knowledge => "知识库管理配置",
            ConfigType.Skill => "技能配置",
            ConfigType.Schedule => "定时任务配置",
            ConfigType.DeepWiki => "向量索引配置",
            ConfigType.Review => "代码评审配置",
            ConfigType.Integration => "第三方集成配置",
            ConfigType.Notification => "通知策略配置",
            ConfigType.Sandbox => "沙盒执行配置",
            ConfigType.Workflow => "流程编排配置",
            ConfigType.ApiKey => "API密钥配置",
            ConfigType.Model => "AI模型配置",
            ConfigType.System => "系统配置",
            _ => "未知配置类型"
        };
    }

    /// <summary>
    /// Gets all configuration types that should be initialized for a new user.
    /// </summary>
    /// <returns>The list of user configuration types.</returns>
    public static List<ConfigType> GetUserConfigTypes()
    {
        return new List<ConfigType>
        {
            ConfigType.Preferences,
            ConfigType.Sandbox,
            ConfigType.Workflow,
            ConfigType.Notification,
            ConfigType.Knowledge,
            ConfigType.Schedule,
            ConfigType.Review,
            ConfigType.DeepWiki,
            ConfigType.Integration,
            ConfigType.Skill,
            ConfigType.Repository
        };
    }

    /// <summary>
    /// Gets all global configuration types that should be initialized.
    /// </summary>
    /// <returns>The list of global configuration types.</returns>
    public static List<ConfigType> GetGlobalConfigTypes()
    {
        return new List<ConfigType>
        {
            ConfigType.Global,
            ConfigType.ApiKey,
            ConfigType.Model,
            ConfigType.System
        };
    }

    private static string GetPreferencesDefault()
    {
        var config = new
        {
            locale = "zh-CN",
            timezone = "Asia/Shanghai",
            theme = "light",
            landingPage = "/",
            enableInAppNotice = true,
            enableEmailNotice = false
        };
        return JsonHelper.Serialize(config);
    }

    private static string GetSandboxDefault()
    {
        var config = new
        {
            profileName = "default-sandbox",
            workspaceRootPath = "C:/gitrepos",
            artifactOutputPath = ".sandbox-artifacts",
            allowedWritePaths = "src/**\ndocs/**",
            ignoredPaths = "node_modules/**\ndist/**\n.git/**",
            executionMode = "dry-run",
            approvalMode = "strict",
            maxParallelTasks = 3,
            commandTimeoutSec = 300,
            allowWriteOps = false,
            allowNetworkAccess = false,
            storeTerminalLogs = true,
            maskSecretsInLogs = true,
            defaultModel = "gpt-4",
            fallbackModel = "gpt-3.5-turbo",
            promptGuardrail = "先分析风险，再执行最小改动。"
        };
        return JsonHelper.Serialize(config);
    }

    private static string GetWorkflowDefault()
    {
        var config = new
        {
            profileName = "team-default",
            commandTimeoutSec = 300,
            maxParallelTasks = 3,
            requireApprovalForWrite = true,
            autoCreateTodo = true,
            outputStyle = "standard",
            stages = new
            {
                initialization = new
                {
                    mode = "default",
                    mustAskClarifying = true,
                    useRepoSearch = true,
                    runValidation = true,
                    preloadInstructionFiles = true,
                    preloadRepoMemory = true,
                    generateChecklist = true
                },
                question = new
                {
                    mode = "default",
                    mustAskClarifying = false,
                    alwaysCiteFiles = true,
                    includeAlternatives = false
                },
                bugfix = new
                {
                    mode = "default",
                    requireReproSteps = true,
                    maxFixAttempts = 3,
                    autoRunUnitTests = true
                },
                newfeature = new
                {
                    mode = "default",
                    requireAcceptanceCriteria = true,
                    createImplementationPlan = true,
                    includeRollbackPlan = true
                }
            }
        };
        return JsonHelper.Serialize(config);
    }

    private static string GetNotificationDefault()
    {
        var config = new
        {
            enableInApp = true,
            enableEmail = false,
            digestMode = "hourly",
            focusMode = "balanced",
            onlyMentioned = false,
            notifyOnBuildFailed = true,
            notifyOnReviewRequested = true,
            notifyOnDeploymentFinished = false,
            notifyOnSecurityAlert = true,
            emailProvider = "smtp",
            deliveryWindow = new
            {
                enabled = false,
                start = "09:00",
                end = "20:00",
                timezone = "Asia/Shanghai"
            }
        };
        return JsonHelper.Serialize(config);
    }

    private static string GetKnowledgeDefault()
    {
        var config = new
        {
            sources = Array.Empty<object>(),
            defaultChunkSize = 1000,
            defaultChunkOverlap = 200,
            defaultRefreshPolicy = "daily",
            defaultAccessLevel = "internal"
        };
        return JsonHelper.Serialize(config);
    }

    private static string GetScheduleDefault()
    {
        var config = new
        {
            tasks = new[]
            {
                new
                {
                    scheduleName = "daily-knowledge-sync",
                    cron = "0 2 * * *",
                    timezone = "Asia/Shanghai",
                    retryLimit = 2,
                    enabled = true,
                    alertChannel = "in-app"
                }
            }
        };
        return JsonHelper.Serialize(config);
    }

    private static string GetReviewDefault()
    {
        var config = new
        {
            minApprovals = 1,
            blockOnFailingChecks = true,
            enforceCodeOwners = true,
            requiredChecks = new[] { "ci", "lint" },
            firstResponseSlaHours = 8,
            exceptionApproverRole = "Tech Lead"
        };
        return JsonHelper.Serialize(config);
    }

    private static string GetDeepWikiDefault()
    {
        var config = new
        {
            workspace = "AutoCodeForge",
            indexName = "autocodeforge-main-index",
            embeddingModel = "text-embedding-3-large",
            topK = 8,
            metric = "cosine",
            retentionDays = 90,
            autoReindex = true
        };
        return JsonHelper.Serialize(config);
    }

    private static string GetIntegrationDefault()
    {
        var config = new
        {
            integrations = Array.Empty<object>()
        };
        return JsonHelper.Serialize(config);
    }

    private static string GetSkillDefault()
    {
        var config = new
        {
            skills = Array.Empty<object>(),
            defaultPriority = 50
        };
        return JsonHelper.Serialize(config);
    }

    private static string GetRepositoryDefault()
    {
        var config = new
        {
            provider = "github",
            owner = "",
            repositoryName = "",
            defaultBranch = "main",
            authMode = "token",
            mergeStrategies = new[] { "squash", "merge" },
            requireChecks = true
        };
        return JsonHelper.Serialize(config);
    }

    private static string GetApiKeyDefault()
    {
        var config = new
        {
            keys = Array.Empty<object>()
        };
        return JsonHelper.Serialize(config);
    }

    private static string GetModelDefault()
    {
        var config = new
        {
            defaultModel = "gpt-4",
            fallbackModel = "gpt-3.5-turbo",
            availableModels = new[] { "gpt-4", "gpt-3.5-turbo", "claude-3-opus" }
        };
        return JsonHelper.Serialize(config);
    }

    private static string GetSystemDefault()
    {
        var config = new
        {
            version = "1.0.0",
            environment = "production",
            maxLoginAttempts = 5,
            sessionTimeoutMinutes = 60
        };
        return JsonHelper.Serialize(config);
    }
}