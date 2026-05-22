using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.DTOs.Repository;
using AutoCodeForge.Core.DTOs.RepoSync;
using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Git;
using AutoCodeForge.Infrastructure.Repositories;
using AutoCodeForge.Infrastructure.Sandbox;
using AutoCodeForge.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using SqlSugar;
using System.Text.Json;

namespace AutoCodeForge.Tests;

/// <summary>
/// 集成测试辅助类，提供通用的测试基础设施配置
/// </summary>
public sealed class IntegrationTestContext : IDisposable
{
    private readonly string _dbPath;
    private bool _disposed;

    #region Properties

    /// <summary>
    /// 数据库客户端
    /// </summary>
    public ISqlSugarClient Db { get; }

    /// <summary>
    /// 当前测试用户
    /// </summary>
    public TestCurrentUser CurrentUser { get; }

    /// <summary>
    /// 配置仓储
    /// </summary>
    public ConfigRepository ConfigRepository { get; }

    /// <summary>
    /// 配置历史仓储
    /// </summary>
    public ConfigHistoryRepository ConfigHistoryRepository { get; }

    /// <summary>
    /// 仓库仓储
    /// </summary>
    public RepositoryRepository RepositoryRepository { get; }

    /// <summary>
    /// 任务仓储
    /// </summary>
    public TaskRepository TaskRepository { get; }

    /// <summary>
    /// 任务日志仓储
    /// </summary>
    public TaskLogRepository TaskLogRepository { get; }

    /// <summary>
    /// 工作空间仓储
    /// </summary>
    public RepoSandboxWorkspaceRepository WorkspaceRepository { get; }

    /// <summary>
    /// Agent仓储
    /// </summary>
    public AgentRepository AgentRepository { get; }

    /// <summary>
    /// LLM模型配置仓储
    /// </summary>
    public LLMModelConfigRepository LLMModelConfigRepository { get; }

    /// <summary>
    /// 加密服务（使用默认测试密钥）
    /// </summary>
    public EncryptionService EncryptionService { get; }

    /// <summary>
    /// DataProtection服务
    /// </summary>
    public DataProtectionService DataProtectionService { get; }

    /// <summary>
    /// GitProvider工厂
    /// </summary>
    public GitProviderFactory GitProviderFactory { get; }

    /// <summary>
    /// LibGit2SharpProvider实例
    /// </summary>
    public LibGit2SharpProvider LibGit2SharpProvider { get; }

    /// <summary>
    /// 默认Git选项
    /// </summary>
    public GitOptions DefaultGitOptions { get; }

    /// <summary>
    /// 配置服务
    /// </summary>
    public ConfigService ConfigService { get; }

    /// <summary>
    /// 仓库服务
    /// </summary>
    public RepositoryService RepositoryService { get; }

    /// <summary>
    /// RepoSync服务
    /// </summary>
    public RepoSyncService RepoSyncService { get; }

    /// <summary>
    /// 沙箱路径解析器
    /// </summary>
    public SandboxPathResolver SandboxPathResolver { get; }

    /// <summary>
    /// Agent服务
    /// </summary>
    public AgentService AgentService { get; }

    #endregion

    /// <summary>
    /// 使用默认配置初始化集成测试上下文
    /// </summary>
    /// <param name="userId">测试用户ID</param>
    public IntegrationTestContext(string userId = "test-user")
    {
        // 初始化数据库
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.test.{Guid.NewGuid():N}.db");
        Db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        // 初始化表
        Db.CodeFirst.InitTables(
            typeof(TaskEntity),
            typeof(TaskLogEntity),
            typeof(UserConfigEntity),
            typeof(RepositoryEntity),
            typeof(RepoSandboxWorkspaceEntity),
            typeof(GlobalConfigEntity),
            typeof(ConfigurationEntry),
            typeof(ConfigHistoryEntity),
            typeof(AgentEntity),
            typeof(LLMModelConfigEntity));

        // 初始化测试用户
        CurrentUser = new TestCurrentUser(userId);

        // 初始化仓储
        ConfigRepository = new ConfigRepository(Db, CurrentUser);
        ConfigHistoryRepository = new ConfigHistoryRepository(Db, CurrentUser);
        RepositoryRepository = new RepositoryRepository(Db, CurrentUser);
        TaskRepository = new TaskRepository(Db, CurrentUser);
        TaskLogRepository = new TaskLogRepository(Db, CurrentUser);
        WorkspaceRepository = new RepoSandboxWorkspaceRepository(Db, CurrentUser);
        AgentRepository = new AgentRepository(Db, CurrentUser);
        LLMModelConfigRepository = new LLMModelConfigRepository(Db, CurrentUser);

        // 初始化基础设施服务
        EncryptionService = new EncryptionService("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");
        DataProtectionService = new DataProtectionService(new StubDataProtectionProvider());
        GitProviderFactory = new GitProviderFactory(new HttpClient());

        // 初始化Git选项
        DefaultGitOptions = GitOptionsFactory.CreateDefault();

        // 初始化LibGit2SharpProvider
        LibGit2SharpProvider = new LibGit2SharpProvider(DefaultGitOptions);

        // 初始化业务服务
        ConfigService = new ConfigService(ConfigRepository, ConfigHistoryRepository, EncryptionService, CurrentUser);
        RepositoryService = new RepositoryService(RepositoryRepository, GitProviderFactory, DataProtectionService);
        RepoSyncService = new RepoSyncService(TaskRepository, TaskLogRepository, RepositoryRepository, WorkspaceRepository, ConfigService);
        SandboxPathResolver = new SandboxPathResolver();
        AgentService = new AgentService(AgentRepository);
    }

    /// <summary>
    /// 创建测试沙箱配置并保存
    /// </summary>
    public async Task<SandboxConfigDto> CreateAndSaveTestSandboxConfigAsync(
        string workspaceRootPath = @"C:\sandbox\workspace",
        int timeoutSeconds = 300,
        bool userIsolationEnabled = true)
    {
        var config = new SandboxConfigDto
        {
            WorkspaceRootPath = workspaceRootPath,
            AllowedWritePaths = [workspaceRootPath],
            TimeoutSeconds = timeoutSeconds,
            UserIsolationEnabled = userIsolationEnabled,
        };

        return await ConfigService.UpsertSandboxConfigAsync(config);
    }

    /// <summary>
    /// 创建测试仓库记录（不进行Token验证）
    /// </summary>
    public async Task<Core.DTOs.Repository.RepositoryDto> CreateTestRepositoryAsync(
        string name,
        string url,
        GitProvider provider,
        string? token = null)
    {
        var request = new CreateRepositoryRequest
        {
            Name = name,
            Url = url,
            Provider = provider,
            Token = token ?? string.Empty,
            AuthType = AutoCodeForge.Core.Entities.AuthenticationType.Token,
            MergeStrategy = AutoCodeForge.Core.Entities.MergeStrategy.Squash,
        };

        return await RepositoryService.CreateAsync(request);
    }

    /// <summary>
    /// 直接在数据库中创建测试仓库记录（绕过验证）
    /// </summary>
    public async Task<Guid> CreateTestRepositoryDirectlyAsync(
        string name,
        string url,
        GitProvider provider)
    {
        var entity = new RepositoryEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Url = url,
            Provider = provider,
            EncryptedToken = string.Empty,
            AuthType = AutoCodeForge.Core.Entities.AuthenticationType.Token,
            MergeStrategy = AutoCodeForge.Core.Entities.MergeStrategy.Squash,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        await RepositoryRepository.CreateAsync(entity);
        return entity.Id;
    }

    /// <summary>
    /// 创建RepoSync任务
    /// </summary>
    public async Task<AutoCodeForge.Core.DTOs.Task.TaskResponse> CreateRepoSyncTaskAsync(
        Guid repositoryId,
        string branch,
        string? title = null,
        string? description = null)
    {
        var request = new CreateRepoSyncTaskRequest
        {
            RepositoryId = repositoryId,
            Branch = branch,
            Title = title ?? $"Sync Task - {DateTime.UtcNow:yyyyMMdd-HHmmss}",
            Description = description ?? "Test sync task",
        };

        return await RepoSyncService.CreateTaskAsync(request);
    }

    /// <summary>
    /// 创建测试 LLM 模型配置
    /// </summary>
    public async Task<LLMModelConfigEntity> CreateTestLLMModelConfigAsync(
        LLMProvider provider = LLMProvider.AzureOpenAI,
        string modelName = "gpt-4o",
        string? endpoint = null,
        string? apiKey = null)
    {
        var entity = new LLMModelConfigEntity
        {
            Id = Guid.NewGuid(),
            Provider = provider,
            ModelName = modelName,
            Endpoint = endpoint,
            ApiKey = apiKey,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        return await LLMModelConfigRepository.CreateAsync(entity);
    }

    /// <summary>
    /// 创建测试 Agent（不绑定 LLMModelConfig）
    /// </summary>
    public async Task<AgentEntity> CreateTestAgentAsync(
        string name = "Test Agent",
        string? description = null,
        string? systemPrompt = null,
        string? keywords = null,
        string? toolNames = null,
        bool isEnabled = true)
    {
        var entity = new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Keywords = keywords,
            SystemPrompt = systemPrompt ?? "You are a helpful assistant.",
            ToolNames = toolNames,
            IsEnabled = isEnabled,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        return await AgentRepository.CreateAsync(entity);
    }

    /// <summary>
    /// 创建测试 Agent（带 LLMModelConfig 绑定）
    /// </summary>
    public async Task<AgentEntity> CreateTestAgentWithModelAsync(
        string name = "Test Agent",
        string? systemPrompt = null,
        LLMProvider provider = LLMProvider.AzureOpenAI,
        string modelName = "gpt-4o",
        string? endpoint = null,
        string? apiKey = null,
        string? toolNames = null)
    {
        var modelConfig = await CreateTestLLMModelConfigAsync(provider, modelName, endpoint, apiKey);

        var entity = new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Test agent for integration testing",
            SystemPrompt = systemPrompt ?? "You are a helpful assistant.",
            LlmModelConfigId = modelConfig.Id,
            ToolNames = toolNames,
            IsEnabled = true,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        return await AgentRepository.CreateAsync(entity);
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }
        catch
        {
            // 忽略清理失败
        }

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Azure DevOps 测试配置
/// </summary>
public sealed class AzureDevOpsTestConfig
{
    public string Token { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string Branch { get; set; } = "main";

    /// <summary>
    /// 生成完整的仓库URL
    /// </summary>
    public string RepositoryUrl => $"https://dev.azure.com/{Uri.EscapeDataString(Organization)}/{Uri.EscapeDataString(Project)}/_git/{Uri.EscapeDataString(Repository)}";
}

/// <summary>
/// Azure DevOps 配置加载器
/// </summary>
public static class AzureDevOpsConfigLoader
{
    private static AzureDevOpsTestConfig? _cachedConfig;
    private static readonly object _lock = new();

    /// <summary>
    /// 从test-configs/azure-devops-config.json加载配置
    /// </summary>
    public static AzureDevOpsTestConfig Load()
    {
        if (_cachedConfig != null) return _cachedConfig;

        lock (_lock)
        {
            if (_cachedConfig != null) return _cachedConfig;

            var configPath = FindConfigPath();
            if (string.IsNullOrEmpty(configPath) || !File.Exists(configPath))
            {
                throw new FileNotFoundException(
                    "配置文件不存在: test-configs/azure-devops-config.json。请确保文件存在于测试项目目录中。",
                    configPath);
            }

            var jsonContent = File.ReadAllText(configPath);
            var configDoc = JsonDocument.Parse(jsonContent);

            if (!configDoc.RootElement.TryGetProperty("azureDefault", out var azureDefault))
            {
                throw new InvalidDataException("配置文件中缺少 azureDefault 节点");
            }

            _cachedConfig = new AzureDevOpsTestConfig
            {
                Token = azureDefault.GetProperty("Token").GetString() ?? string.Empty,
                Organization = azureDefault.GetProperty("Org").GetString() ?? string.Empty,
                Project = azureDefault.GetProperty("Project").GetString() ?? string.Empty,
                Repository = azureDefault.GetProperty("Repo").GetString() ?? string.Empty,
                Branch = azureDefault.TryGetProperty("Branch", out var branch) ? branch.GetString() ?? "main" : "main",
            };

            return _cachedConfig;
        }
    }

    /// <summary>
    /// 查找配置文件路径
    /// </summary>
    private static string FindConfigPath()
    {
        // 1. 尝试从当前执行程序集位置向上查找
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        if (!string.IsNullOrEmpty(assemblyLocation))
        {
            var currentDir = Path.GetDirectoryName(assemblyLocation)!;
            var path = SearchUpwardForConfig(currentDir);
            if (!string.IsNullOrEmpty(path)) return path;
        }

        // 2. 尝试从调用堆栈中的文件位置向上查找
        var stackFrame = new System.Diagnostics.StackTrace(true).GetFrame(0);
        var callerFilePath = stackFrame?.GetFileName();
        if (!string.IsNullOrEmpty(callerFilePath))
        {
            var currentDir = Path.GetDirectoryName(callerFilePath)!;
            var path = SearchUpwardForConfig(currentDir);
            if (!string.IsNullOrEmpty(path)) return path;
        }

        // 3. 尝试当前工作目录
        var cwdPath = SearchUpwardForConfig(Environment.CurrentDirectory);
        if (!string.IsNullOrEmpty(cwdPath)) return cwdPath;

        return string.Empty;
    }

    /// <summary>
    /// 向上搜索配置文件
    /// </summary>
    private static string SearchUpwardForConfig(string startDir)
    {
        var currentDir = startDir;
        while (!string.IsNullOrEmpty(currentDir))
        {
            var configPath = Path.Combine(currentDir, "test-configs", "azure-devops-config.json");
            if (File.Exists(configPath))
            {
                return configPath;
            }
            currentDir = Path.GetDirectoryName(currentDir) ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// 清除缓存的配置（用于测试重新加载）
    /// </summary>
    public static void ClearCache()
    {
        lock (_lock)
        {
            _cachedConfig = null;
        }
    }
}

/// <summary>
/// Git 选项创建辅助类
/// </summary>
public static class GitOptionsFactory
{
    /// <summary>
    /// 创建默认的Git选项（适用于Azure DevOps）
    /// </summary>
    public static GitOptions CreateDefault()
    {
        return new GitOptions
        {
            AzureDevOps = new AzureDevOpsOptions
            {
                Username = string.Empty,
                EnableUrlEncoding = true,
                DomainPatterns = new List<string> { "dev.azure.com", "visualstudio.com" }
            },
            StringHandling = new StringHandlingOptions
            {
                GitHubUsername = "x-access-token",
                GitLabUsername = "oauth2",
                SpecialCharacters = new List<string> { " ", "@", "#", "$", "%", "^", "&", "*", "(", ")", "+", ",", ";", "=", "?", "/" },
                NormalizeWhitespace = true,
                WhitespaceReplacement = "_",
            },
            Path = new PathOptions
            {
                MaxPathLength = 260,
                AutoShortenPaths = true,
                ShortPathIdLength = 3,
            },
            Providers = new ProviderOptions
            {
                DefaultUsername = "git",
            },
        };
    }

    /// <summary>
    /// 创建仅包含Azure DevOps配置的Git选项
    /// </summary>
    public static GitOptions CreateAzureDevOpsOnly()
    {
        return new GitOptions
        {
            AzureDevOps = new AzureDevOpsOptions
            {
                Username = string.Empty,
                EnableUrlEncoding = true,
                DomainPatterns = new List<string> { "dev.azure.com", "visualstudio.com" }
            },
        };
    }
}

#region 测试辅助类

/// <summary>
/// 测试用户实现（实现ICurrentUser接口）
/// </summary>
public sealed class TestCurrentUser : ICurrentUser
{
    private readonly string? _ntId;

    public TestCurrentUser(string? ntId)
    {
        _ntId = ntId;
    }

    public string? GetCurrentNtId() => _ntId;

    public bool IsAdmin() => false;
}

/// <summary>
/// Stub DataProtectionProvider用于测试
/// </summary>
public sealed class StubDataProtectionProvider : IDataProtectionProvider
{
    public IDataProtector CreateProtector(string purpose)
    {
        return new StubDataProtector();
    }
}

/// <summary>
/// Stub DataProtector用于测试
/// </summary>
public sealed class StubDataProtector : IDataProtector
{
    public IDataProtector CreateProtector(string purpose)
    {
        return this;
    }

    public byte[] Protect(byte[] plaintext)
    {
        return plaintext;
    }

    public byte[] Unprotect(byte[] protectedData)
    {
        return protectedData;
    }

    public bool TryUnprotect(byte[] protectedData, out byte[] plaintext)
    {
        plaintext = protectedData;
        return true;
    }
}

#endregion
