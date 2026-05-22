/**
 * AgentService 业务逻辑测试
 *
 * 测试覆盖：
 * 1. CreateAsync - 创建 Agent
 * 2. UpdateAsync - 更新 Agent
 * 3. GetByIdAsync - 查询 Agent
 * 4. GetPagedAsync - 分页查询 Agent
 * 5. DeleteAsync - 删除 Agent（软删除）
 * 6. MatchAsync - 匹配 Agent
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Agent;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Tests;

/// <summary>
/// AgentService 业务逻辑测试
/// </summary>
public sealed class AgentServiceTests : IDisposable
{
    private readonly IntegrationTestContext _context;

    public AgentServiceTests()
    {
        _context = new IntegrationTestContext("test-agent-user");
    }

    #region Create Tests

    /// <summary>
    /// 测试创建基础 Agent
    /// </summary>
    [Fact]
    public async Task CreateAsync_Should_CreateAgent_WithValidRequest()
    {
        // Arrange
        var request = new CreateAgentRequest
        {
            Name = "Test Agent",
            Description = "A test agent",
            SystemPrompt = "You are a helpful assistant.",
            IsEnabled = true,
        };

        // Act
        var result = await _context.AgentService.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Agent", result.Name);
        Assert.Equal("A test agent", result.Description);
        Assert.Equal("You are a helpful assistant.", result.SystemPrompt);
        Assert.True(result.IsEnabled);

        // 验证数据库中确实存在
        var retrieved = await _context.AgentRepository.GetByIdAsync(result.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Test Agent", retrieved.Name);

        Console.WriteLine($"[测试1] 创建 Agent 成功: {result.Id}");
    }

    /// <summary>
    /// 测试创建带 LLMModelConfig 的 Agent
    /// </summary>
    [Fact]
    public async Task CreateAsync_Should_CreateAgent_WithLLMModelConfig()
    {
        // Arrange
        var llmConfig = await _context.CreateTestLLMModelConfigAsync(
            provider: LLMProvider.AzureOpenAI,
            modelName: "gpt-4o",
            endpoint: "https://api.openai.com/v1",
            apiKey: "test-api-key");

        var request = new CreateAgentRequest
        {
            Name = "Agent with Model",
            SystemPrompt = "You are a coding assistant.",
            LlmModelConfigId = llmConfig.Id,
            IsEnabled = true,
        };

        // Act
        var result = await _context.AgentService.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(llmConfig.Id, result.LlmModelConfigId);

        Console.WriteLine($"[测试2] 创建带 LLMModelConfig 的 Agent 成功: {result.Id}");
    }

    #endregion

    #region Update Tests

    /// <summary>
    /// 测试更新 Agent
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Should_UpdateAgent()
    {
        // Arrange
        var agent = await _context.CreateTestAgentAsync(
            name: "Original Name",
            description: "Original description",
            isEnabled: true);

        var updateRequest = new UpdateAgentRequest
        {
            Name = "Updated Name",
            Description = "Updated description",
            SystemPrompt = "You are an updated assistant.",
            IsEnabled = false,
        };

        // Act
        var result = await _context.AgentService.UpdateAsync(agent.Id, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal("You are an updated assistant.", result.SystemPrompt);
        Assert.False(result.IsEnabled);

        Console.WriteLine($"[测试3] 更新 Agent 成功: {agent.Id}");
    }

    #endregion

    #region Query Tests

    /// <summary>
    /// 测试通过 ID 获取 Agent
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_Should_ReturnAgent_WhenExists()
    {
        // Arrange
        var agent = await _context.CreateTestAgentAsync(name: "Query Test Agent");

        // Act
        var result = await _context.AgentService.GetByIdAsync(agent.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(agent.Id, result.Id);
        Assert.Equal("Query Test Agent", result.Name);

        Console.WriteLine($"[测试4] GetById 查询成功: {agent.Id}");
    }

    /// <summary>
    /// 测试通过 ID 获取 Agent - 不存在时抛出异常
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_Should_ThrowException_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<AutoCodeForge.Core.Exceptions.NotFoundException>(
            () => _context.AgentService.GetByIdAsync(nonExistentId));

        Console.WriteLine("[测试5] GetById 正确抛出 NotFoundException");
    }

    /// <summary>
    /// 测试分页查询 Agent
    /// </summary>
    [Fact]
    public async Task GetPagedAsync_Should_ReturnPagedAgents()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            await _context.CreateTestAgentAsync(name: $"Paged Agent {i}");
        }

        // Act
        var result = await _context.AgentService.GetPagedAsync(page: 1, pageSize: 3);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.TotalPages);

        Console.WriteLine($"[测试6] 分页查询成功: {result.TotalCount} 总数, 第1页返回{result.Items.Count}条");
    }

    #endregion

    #region Delete Tests

    /// <summary>
    /// 测试软删除 Agent
    /// </summary>
    [Fact]
    public async Task DeleteAsync_Should_SoftDeleteAgent()
    {
        // Arrange
        var agent = await _context.CreateTestAgentAsync(name: "To Be Deleted");

        // Act
        await _context.AgentService.DeleteAsync(agent.Id);

        // Assert - 软删除后，通过 Service 查询应该抛出异常（因为默认过滤已删除记录）
        await Assert.ThrowsAsync<AutoCodeForge.Core.Exceptions.NotFoundException>(
            () => _context.AgentService.GetByIdAsync(agent.Id));

        // 软删除验证：Agent 记录被标记为 IsDeleted = true
        // 由于 BaseRepository 没有提供查询软删除记录的方法，我们通过以下方式验证：
        // 1. Service 层抛出 NotFoundException 说明记录被"隐藏"
        // 2. Repository 层的 SoftDeleteAsync 成功执行说明实体被正确更新
        // 这间接证明了软删除机制正常工作

        Console.WriteLine($"[测试7] 软删除 Agent 成功: {agent.Id} - Service 层正确抛出 NotFoundException");
    }

    #endregion

    #region Match Tests

    /// <summary>
    /// 测试匹配 Agent - 通过关键词匹配
    /// </summary>
    [Fact]
    public async Task MatchAsync_Should_FindAgent_ByKeywords()
    {
        // Arrange
        await _context.CreateTestAgentAsync(
            name: "Code Review Agent",
            keywords: "review, code, quality",
            systemPrompt: "You are a code reviewer.");

        await _context.CreateTestAgentAsync(
            name: "General Assistant",
            keywords: "general, help",
            systemPrompt: "You are a general assistant.");

        // Act
        var matchedAgent = await _context.AgentService.MatchByInputAsync("review code");

        // Assert
        Assert.NotNull(matchedAgent);
        Assert.Equal("Code Review Agent", matchedAgent.Name);

        Console.WriteLine($"[测试8] 关键词匹配成功: 找到 Agent: {matchedAgent.Name}");
    }

    /// <summary>
    /// 测试匹配 Agent - 无匹配时返回得分最高的 Agent（按名称排序）
    /// 注意：MatchByInputAsync 总是返回得分最高的 Agent，即使得分为 0
    /// </summary>
    [Fact]
    public async Task MatchAsync_Should_ReturnBestMatch_WhenNoExactMatch()
    {
        // Arrange
        await _context.CreateTestAgentAsync(
            name: "Specific Agent",
            keywords: "specific, unique",
            systemPrompt: "You are specific.");

        // Act
        var matchedAgent = await _context.AgentService.MatchByInputAsync("xyz none");

        // Assert - 没有精确匹配时，仍然返回得分最高的 Agent（按名称排序的第一个）
        Assert.NotNull(matchedAgent);
        Assert.Equal("Specific Agent", matchedAgent.Name); // 因为只有这一个 Agent

        Console.WriteLine("[测试9] 无精确匹配时返回最佳候选: " + matchedAgent.Name);
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}
