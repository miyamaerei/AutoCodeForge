/**
 * LlmConfigService 业务逻辑测试
 *
 * 测试覆盖：
 * 1. LLM模型配置的创建、查询、更新、删除
 * 2. LLM凭证配置的创建、查询（支持加密）
 * 3. LLM全局设置的管理
 * 4. 默认模型获取逻辑
 * 5. 多模型配置支持
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Enums;

namespace AutoCodeForge.Tests;

/// <summary>
/// LlmConfigService 业务逻辑测试
/// </summary>
public sealed class LlmConfigServiceTests : IDisposable
{
    private readonly IntegrationTestContext _context;
    private readonly LlmConfigService _llmConfigService;

    public LlmConfigServiceTests()
    {
        _context = new IntegrationTestContext("test-user");
        _llmConfigService = new LlmConfigService(_context.ConfigService);
    }

    #region 模型配置测试

    /// <summary>
    /// 测试创建LLM模型配置
    /// </summary>
    [Fact]
    public async Task UpsertModel_Should_CreateNewModelConfig()
    {
        // Arrange
        var modelConfig = new LlmModelConfigDto
        {
            Provider = LlmProvider.AzureOpenAI,
            ModelName = "gpt-4o",
            Endpoint = "https://test.openai.azure.com",
            ContextWindow = 128000,
            Weight = 10,
            IsDefault = true,
            IsActive = true
        };

        // Act
        var result = await _llmConfigService.UpsertModelAsync(modelConfig);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ConfigType.Llm, result.ConfigType);
        Assert.StartsWith("model.azureopenai-gpt-4o", result.ConfigKey);
        Assert.Equal("models", result.Group);
        Assert.False(result.IsEncrypted);

        Console.WriteLine("[测试] 创建LLM模型配置成功: " + result.ConfigKey);
    }

    /// <summary>
    /// 测试获取所有LLM模型配置
    /// </summary>
    [Fact]
    public async Task GetAllModels_Should_ReturnAllModels()
    {
        // Arrange
        await _llmConfigService.UpsertModelAsync(new LlmModelConfigDto
        {
            Provider = LlmProvider.AzureOpenAI,
            ModelName = "gpt-4",
            IsDefault = false,
            IsActive = true
        });

        await _llmConfigService.UpsertModelAsync(new LlmModelConfigDto
        {
            Provider = LlmProvider.Ollama,
            ModelName = "llama3",
            Endpoint = "http://localhost:11434",
            IsDefault = true,
            IsActive = true
        });

        // Act
        var models = await _llmConfigService.GetAllModelsAsync();

        // Assert
        Assert.NotEmpty(models);
        Assert.True(models.Count >= 2);
        Assert.Contains(models, m => m.ModelName == "gpt-4");
        Assert.Contains(models, m => m.ModelName == "llama3");

        Console.WriteLine("[测试] 获取所有LLM模型配置成功，共: " + models.Count + " 个");
    }

    /// <summary>
    /// 测试多模型配置：同时配置多个不同提供商的模型
    /// </summary>
    [Fact]
    public async Task UpsertModel_Should_SupportMultipleModels_DifferentProviders()
    {
        // Arrange - 配置多个不同提供商的模型
        var models = new[]
        {
            new LlmModelConfigDto
            {
                Provider = LlmProvider.AzureOpenAI,
                ModelName = "gpt-4o",
                Endpoint = "https://azure.openai.azure.com",
                ContextWindow = 128000,
                Weight = 10,
                IsDefault = true,
                IsActive = true
            },
            new LlmModelConfigDto
            {
                Provider = LlmProvider.GitHubCopilot,
                ModelName = "copilot-cli",
                ContextWindow = 8192,
                Weight = 8,
                IsDefault = false,
                IsActive = true
            },
            new LlmModelConfigDto
            {
                Provider = LlmProvider.Ollama,
                ModelName = "llama3",
                Endpoint = "http://localhost:11434",
                ContextWindow = 8192,
                Weight = 5,
                IsDefault = false,
                IsActive = true
            },
            new LlmModelConfigDto
            {
                Provider = LlmProvider.OpenAI,
                ModelName = "gpt-4-turbo",
                ContextWindow = 128000,
                Weight = 7,
                IsDefault = false,
                IsActive = true
            }
        };

        // Act - 逐个创建模型
        foreach (var model in models)
        {
            await _llmConfigService.UpsertModelAsync(model);
        }

        // Assert - 验证所有模型都已创建
        var allModels = await _llmConfigService.GetAllModelsAsync();
        Assert.Equal(4, allModels.Count);

        // 验证不同提供商的模型都存在
        Assert.Contains(allModels, m => m.Provider == LlmProvider.AzureOpenAI && m.ModelName == "gpt-4o");
        Assert.Contains(allModels, m => m.Provider == LlmProvider.GitHubCopilot && m.ModelName == "copilot-cli");
        Assert.Contains(allModels, m => m.Provider == LlmProvider.Ollama && m.ModelName == "llama3");
        Assert.Contains(allModels, m => m.Provider == LlmProvider.OpenAI && m.ModelName == "gpt-4-turbo");

        // 验证只有1个默认模型
        var defaultModels = allModels.Where(m => m.IsDefault).ToList();
        Assert.Single(defaultModels);
        Assert.Equal("gpt-4o", defaultModels[0].ModelName);

        Console.WriteLine("[测试] 多模型配置成功，共配置: " + allModels.Count + " 个模型");
    }

    /// <summary>
    /// 测试多模型配置：同一提供商多个模型
    /// </summary>
    [Fact]
    public async Task UpsertModel_Should_SupportMultipleModels_SameProvider()
    {
        // Arrange - 配置同一提供商的不同模型
        var models = new[]
        {
            new LlmModelConfigDto
            {
                Provider = LlmProvider.AzureOpenAI,
                ModelName = "gpt-4",
                Endpoint = "https://azure.openai.azure.com",
                ContextWindow = 8192,
                Weight = 10,
                IsDefault = true,
                IsActive = true
            },
            new LlmModelConfigDto
            {
                Provider = LlmProvider.AzureOpenAI,
                ModelName = "gpt-4-turbo",
                Endpoint = "https://azure.openai.azure.com",
                ContextWindow = 128000,
                Weight = 8,
                IsDefault = false,
                IsActive = true
            },
            new LlmModelConfigDto
            {
                Provider = LlmProvider.AzureOpenAI,
                ModelName = "gpt-35-turbo",
                Endpoint = "https://azure.openai.azure.com",
                ContextWindow = 16385,
                Weight = 5,
                IsDefault = false,
                IsActive = true
            }
        };

        // Act
        foreach (var model in models)
        {
            await _llmConfigService.UpsertModelAsync(model);
        }

        // Assert
        var allModels = await _llmConfigService.GetAllModelsAsync();
        var azureModels = allModels.Where(m => m.Provider == LlmProvider.AzureOpenAI).ToList();
        
        Assert.Equal(3, azureModels.Count);
        Assert.Contains(azureModels, m => m.ModelName == "gpt-4");
        Assert.Contains(azureModels, m => m.ModelName == "gpt-4-turbo");
        Assert.Contains(azureModels, m => m.ModelName == "gpt-35-turbo");

        // 验证configKey唯一性
        var configKeys = allModels.Select(m => LlmConfigKeyBuilder.BuildModelKey(m.Provider, m.ModelName)).ToList();
        Assert.Equal(configKeys.Distinct().Count(), configKeys.Count);

        Console.WriteLine("[测试] 同一提供商多模型配置成功，共: " + azureModels.Count + " 个");
    }

    /// <summary>
    /// 测试多模型配置：模型选择权重
    /// </summary>
    [Fact]
    public async Task UpsertModel_Should_SupportModelWeighting()
    {
        // Arrange - 配置不同权重的模型
        await _llmConfigService.UpsertModelAsync(new LlmModelConfigDto
        {
            Provider = LlmProvider.OpenAI,
            ModelName = "gpt-4",
            ContextWindow = 8192,
            Weight = 10,
            IsDefault = true,
            IsActive = true
        });

        await _llmConfigService.UpsertModelAsync(new LlmModelConfigDto
        {
            Provider = LlmProvider.OpenAI,
            ModelName = "gpt-3.5-turbo",
            ContextWindow = 16385,
            Weight = 5,
            IsDefault = false,
            IsActive = true
        });

        await _llmConfigService.UpsertModelAsync(new LlmModelConfigDto
        {
            Provider = LlmProvider.OpenAI,
            ModelName = "gpt-3.5-turbo-16k",
            ContextWindow = 16385,
            Weight = 3,
            IsDefault = false,
            IsActive = false // 非活动模型
        });

        // Act
        var allModels = await _llmConfigService.GetAllModelsAsync();

        // Assert - 按权重排序（降序）
        var orderedModels = allModels.OrderByDescending(m => m.Weight).ToList();
        Assert.Equal("gpt-4", orderedModels[0].ModelName);
        Assert.Equal("gpt-3.5-turbo", orderedModels[1].ModelName);
        Assert.Equal("gpt-3.5-turbo-16k", orderedModels[2].ModelName);

        // 验证只返回活动模型
        var activeModels = allModels.Where(m => m.IsActive).ToList();
        Assert.Equal(2, activeModels.Count);

        Console.WriteLine("[测试] 模型权重配置成功，按权重排序: " + string.Join(", ", orderedModels.Select(m => $"{m.ModelName}(w:{m.Weight})")));
    }

    /// <summary>
    /// 测试获取单个LLM模型配置
    /// </summary>
    [Fact]
    public async Task GetModel_Should_ReturnModel_WhenExists()
    {
        // Arrange
        var configKey = LlmConfigKeyBuilder.BuildModelKey(LlmProvider.GitHubCopilot, "copilot-cli");
        await _llmConfigService.UpsertModelAsync(new LlmModelConfigDto
        {
            Provider = LlmProvider.GitHubCopilot,
            ModelName = "copilot-cli",
            IsActive = true
        });

        // Act
        var model = await _llmConfigService.GetModelAsync(configKey);

        // Assert
        Assert.NotNull(model);
        Assert.Equal(LlmProvider.GitHubCopilot, model.Provider);
        Assert.Equal("copilot-cli", model.ModelName);
        Assert.Null(model.CredentialKey); // GitHub Copilot无需密钥

        Console.WriteLine("[测试] 获取单个LLM模型配置成功: " + model.ModelName);
    }

    /// <summary>
    /// 测试更新LLM模型配置
    /// </summary>
    [Fact]
    public async Task UpsertModel_Should_UpdateExistingModel()
    {
        // Arrange
        await _llmConfigService.UpsertModelAsync(new LlmModelConfigDto
        {
            Provider = LlmProvider.OpenAI,
            ModelName = "gpt-3.5-turbo",
            ContextWindow = 4096,
            IsDefault = false
        });

        // Act - 更新配置
        var updatedConfig = new LlmModelConfigDto
        {
            Provider = LlmProvider.OpenAI,
            ModelName = "gpt-3.5-turbo",
            ContextWindow = 16384, // 更新窗口大小
            IsDefault = true
        };
        var result = await _llmConfigService.UpsertModelAsync(updatedConfig);

        // Assert
        Assert.NotNull(result);
        
        // 验证更新后的值
        var retrieved = await _llmConfigService.GetModelAsync(result.ConfigKey);
        Assert.NotNull(retrieved);
        Assert.Equal(16384, retrieved.ContextWindow);
        Assert.True(retrieved.IsDefault);

        Console.WriteLine("[测试] 更新LLM模型配置成功");
    }

    #endregion

    #region 凭证配置测试

    /// <summary>
    /// 测试创建API Key类型凭证（应加密存储）
    /// </summary>
    [Fact]
    public async Task UpsertCredential_WithApiKey_Should_EncryptValue()
    {
        // Arrange
        var credentialConfig = new LlmCredentialConfigDto
        {
            ProviderType = CredentialProviderType.ApiKey,
            ApiKey = "sk-xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
            Description = "OpenAI API Key"
        };

        // Act
        var result = await _llmConfigService.UpsertCredentialAsync(credentialConfig, "openai-main");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ConfigType.Llm, result.ConfigType);
        Assert.StartsWith("credential.apikey-openai-main", result.ConfigKey);
        Assert.True(result.IsEncrypted); // 验证已加密

        Console.WriteLine("[测试] 创建API Key凭证成功，已加密存储");
    }

    /// <summary>
    /// 测试创建环境变量类型凭证（不加密）
    /// </summary>
    [Fact]
    public async Task UpsertCredential_WithEnvVar_Should_NotEncrypt()
    {
        // Arrange
        var credentialConfig = new LlmCredentialConfigDto
        {
            ProviderType = CredentialProviderType.EnvVar,
            EnvVarName = "AZURE_OPENAI_API_KEY",
            Description = "Azure OpenAI Key from Environment"
        };

        // Act
        var result = await _llmConfigService.UpsertCredentialAsync(credentialConfig, "azure-env");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsEncrypted); // 验证未加密

        Console.WriteLine("[测试] 创建环境变量凭证成功，未加密存储");
    }

    /// <summary>
    /// 测试创建无凭证类型（用于Copilot等）
    /// </summary>
    [Fact]
    public async Task UpsertCredential_WithNoneType_Should_Work()
    {
        // Arrange
        var credentialConfig = new LlmCredentialConfigDto
        {
            ProviderType = CredentialProviderType.None,
            Description = "No credential required (GitHub Copilot)"
        };

        // Act
        var result = await _llmConfigService.UpsertCredentialAsync(credentialConfig, "copilot");

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsEncrypted);
        Assert.Equal("credential.none-copilot", result.ConfigKey);

        Console.WriteLine("[测试] 创建无凭证配置成功");
    }

    /// <summary>
    /// 测试获取凭证配置（应自动解密）
    /// </summary>
    [Fact]
    public async Task GetCredential_Should_ReturnDecryptedValue()
    {
        // Arrange
        var credentialConfig = new LlmCredentialConfigDto
        {
            ProviderType = CredentialProviderType.ApiKey,
            ApiKey = "test-secret-key",
            Description = "Test Credential"
        };
        var created = await _llmConfigService.UpsertCredentialAsync(credentialConfig, "test");

        // Act
        var retrieved = await _llmConfigService.GetCredentialAsync(created.ConfigKey);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("test-secret-key", retrieved.ApiKey); // 验证已解密

        Console.WriteLine("[测试] 获取凭证配置成功，已自动解密");
    }

    #endregion

    #region 全局设置测试

    /// <summary>
    /// 测试创建/更新LLM全局设置
    /// </summary>
    [Fact]
    public async Task UpsertSettings_Should_SaveSettings()
    {
        // Arrange
        var settings = new LlmSettingsConfigDto
        {
            DefaultModelKey = "model.azureopenai-gpt4o",
            FallbackModelKey = "model.ollama-llama3",
            Temperature = 0.5m,
            MaxTokens = 4096,
            TimeoutSeconds = 120,
            MaxRetries = 5
        };

        // Act
        var result = await _llmConfigService.UpsertSettingsAsync(settings);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("settings.default", result.ConfigKey);

        // 验证读取
        var retrieved = await _llmConfigService.GetSettingsAsync();
        Assert.Equal(0.5m, retrieved.Temperature);
        Assert.Equal(4096, retrieved.MaxTokens);
        Assert.Equal("model.azureopenai-gpt4o", retrieved.DefaultModelKey);

        Console.WriteLine("[测试] 创建/更新LLM全局设置成功");
    }

    /// <summary>
    /// 测试获取默认设置（无配置时返回默认值）
    /// </summary>
    [Fact]
    public async Task GetSettings_Should_ReturnDefault_WhenNotConfigured()
    {
        // Act
        var settings = await _llmConfigService.GetSettingsAsync();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(0.7m, settings.Temperature); // 默认值
        Assert.Equal(2048, settings.MaxTokens);    // 默认值
        Assert.Equal(60, settings.TimeoutSeconds); // 默认值

        Console.WriteLine("[测试] 获取默认LLM设置成功");
    }

    #endregion

    #region 默认模型测试

    /// <summary>
    /// 测试获取默认模型（通过设置指定）
    /// </summary>
    [Fact]
    public async Task GetDefaultModel_Should_ReturnDefaultFromSettings()
    {
        // Arrange
        // 创建模型
        var modelKey = LlmConfigKeyBuilder.BuildModelKey(LlmProvider.AzureOpenAI, "gpt-4o");
        await _llmConfigService.UpsertModelAsync(new LlmModelConfigDto
        {
            Provider = LlmProvider.AzureOpenAI,
            ModelName = "gpt-4o",
            IsDefault = false, // 通过设置指定默认，而非模型本身
            IsActive = true
        });

        // 设置默认模型
        await _llmConfigService.UpsertSettingsAsync(new LlmSettingsConfigDto
        {
            DefaultModelKey = modelKey
        });

        // Act
        var defaultModel = await _llmConfigService.GetDefaultModelAsync();

        // Assert
        Assert.NotNull(defaultModel);
        Assert.Equal("gpt-4o", defaultModel.ModelName);

        Console.WriteLine("[测试] 通过设置获取默认模型成功");
    }

    /// <summary>
    /// 测试获取默认模型（回退到IsDefault标记）
    /// </summary>
    [Fact]
    public async Task GetDefaultModel_Should_FallbackToIsDefaultFlag()
    {
        // Arrange - 创建标记为默认的模型，但不设置全局设置
        await _llmConfigService.UpsertModelAsync(new LlmModelConfigDto
        {
            Provider = LlmProvider.Ollama,
            ModelName = "llama3",
            IsDefault = true,
            IsActive = true
        });

        await _llmConfigService.UpsertModelAsync(new LlmModelConfigDto
        {
            Provider = LlmProvider.OpenAI,
            ModelName = "gpt-3.5",
            IsDefault = false,
            IsActive = true
        });

        // Act
        var defaultModel = await _llmConfigService.GetDefaultModelAsync();

        // Assert
        Assert.NotNull(defaultModel);
        Assert.Equal("llama3", defaultModel.ModelName);

        Console.WriteLine("[测试] 通过IsDefault标记获取默认模型成功");
    }

    #endregion

    #region 删除测试

    /// <summary>
    /// 测试删除LLM配置
    /// </summary>
    [Fact]
    public async Task DeleteConfig_Should_RemoveConfig()
    {
        // Arrange
        await _llmConfigService.UpsertModelAsync(new LlmModelConfigDto
        {
            Provider = LlmProvider.Custom,
            ModelName = "test-model",
            IsActive = true
        });
        var configKey = LlmConfigKeyBuilder.BuildModelKey(LlmProvider.Custom, "test-model");

        // Act
        await _llmConfigService.DeleteConfigAsync(configKey);

        // Assert
        var deleted = await _llmConfigService.GetModelAsync(configKey);
        Assert.Null(deleted);

        Console.WriteLine("[测试] 删除LLM配置成功");
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}
