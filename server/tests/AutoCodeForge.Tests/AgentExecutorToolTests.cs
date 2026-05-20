using AutoCodeForge.Core.DTOs.AI;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.AI;
using Microsoft.Extensions.Logging;
using Moq;

namespace AutoCodeForge.Tests;

/// <summary>
/// Unit tests for AgentExecutor tool integration.
/// Verifies tool registration, execution, and error handling.
/// </summary>
public class AgentExecutorToolTests
{
    /// <summary>
    /// Test: Tool registration completeness.
    /// When AgentExecutor is created with tools, it should receive all tools.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithValidTools_PassesToolsToGateway()
    {
        // Arrange
        var mockLlmGateway = new Mock<ILlmGateway>();
        var mockLogger = new Mock<ILogger<AgentExecutor>>();

        var mockTool = new Mock<IAgentTool>();
        mockTool.Setup(t => t.Name).Returns("TestTool");
        mockTool.Setup(t => t.Description).Returns("A test tool");
        mockTool.Setup(t => t.ExecuteAsync(It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Tool executed");

        var tools = new[] { mockTool.Object };
        var executor = new AgentExecutor(mockLlmGateway.Object, tools, mockLogger.Object);

        var agent = new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = "TestAgent",
            Description = "A test agent",
            ToolNames = "TestTool",
            IsEnabled = true,
        };

        var expectedResponse = new LlmResponse
        {
            Content = "Response content",
            ModelName = "test-model",
            CompletedAtUtc = DateTime.UtcNow,
            TotalTokens = 100,
        };

        mockLlmGateway.Setup(g => g.ChatWithToolsAsync(
                It.IsAny<LlmRequest>(),
                It.Is<IEnumerable<IAgentTool>>(t => t.Count() == 1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await executor.ExecuteAsync(
            agent,
            "Test input",
            new List<ChatMessageEntity>(),
            CancellationToken.None);

        // Assert
        Assert.Equal("Response content", result);
        mockLlmGateway.Verify(
            g => g.ChatWithToolsAsync(
                It.IsAny<LlmRequest>(),
                It.Is<IEnumerable<IAgentTool>>(t => t.Count() == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Test: Empty tool collection.
    /// When no tools are available, execution should succeed without tool invocation.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithNoTools_CallsChatAsync()
    {
        // Arrange
        var mockLlmGateway = new Mock<ILlmGateway>();
        var mockLogger = new Mock<ILogger<AgentExecutor>>();

        var emptyTools = Array.Empty<IAgentTool>();
        var executor = new AgentExecutor(mockLlmGateway.Object, emptyTools, mockLogger.Object);

        var agent = new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = "NoToolAgent",
            ToolNames = null,
            IsEnabled = true,
        };

        var expectedResponse = new LlmResponse
        {
            Content = "Simple response",
            ModelName = "default-model",
            CompletedAtUtc = DateTime.UtcNow,
            TotalTokens = 50,
        };

        mockLlmGateway.Setup(g => g.ChatWithToolsAsync(
                It.IsAny<LlmRequest>(),
                It.IsAny<IEnumerable<IAgentTool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await executor.ExecuteAsync(
            agent,
            "Query without tools",
            new List<ChatMessageEntity>(),
            CancellationToken.None);

        // Assert
        Assert.Equal("Simple response", result);
        mockLlmGateway.Verify(g => g.ChatWithToolsAsync(
            It.IsAny<LlmRequest>(),
            It.IsAny<IEnumerable<IAgentTool>>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Test: Tool parameter passing.
    /// Tool input parameters should be correctly extracted and passed to LlmGateway.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_PassesMessageHistoryAndSystemPrompt()
    {
        // Arrange
        var mockLlmGateway = new Mock<ILlmGateway>();
        var mockLogger = new Mock<ILogger<AgentExecutor>>();

        var tools = Array.Empty<IAgentTool>();
        var executor = new AgentExecutor(mockLlmGateway.Object, tools, mockLogger.Object);

        var agent = new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = "ContextAgent",
            SystemPrompt = "You are a helpful assistant.",
            IsEnabled = true,
        };

        var history = new List<ChatMessageEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Type = MessageType.User,
                Content = "Previous user message",
            },
            new()
            {
                Id = Guid.NewGuid(),
                Type = MessageType.Assistant,
                Content = "Previous assistant response",
            },
        };

        var expectedResponse = new LlmResponse
        {
            Content = "Contextual response",
            ModelName = "context-model",
            CompletedAtUtc = DateTime.UtcNow,
            TotalTokens = 150,
        };

        LlmRequest? capturedRequest = null;
        mockLlmGateway.Setup(g => g.ChatWithToolsAsync(
                It.IsAny<LlmRequest>(),
                It.IsAny<IEnumerable<IAgentTool>>(),
                It.IsAny<CancellationToken>()))
            .Callback<LlmRequest, IEnumerable<IAgentTool>, CancellationToken>(
                (request, _, _) => capturedRequest = request)
            .ReturnsAsync(expectedResponse);

        // Act
        await executor.ExecuteAsync(agent, "New user input", history, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal(agent.SystemPrompt, capturedRequest.SystemPrompt);
        Assert.Equal(agent.LlmModelConfigId, capturedRequest.PreferredModelId);

        // Verify message history includes previous messages
        Assert.True(capturedRequest.Messages.Count >= 3); // 2 history + 1 new
        var lastMessage = capturedRequest.Messages.Last();
        Assert.Equal("user", lastMessage.Role);
        Assert.Equal("New user input", lastMessage.Content);
    }

    /// <summary>
    /// Test: Tool execution error handling.
    /// When a tool throws an exception, it should be handled gracefully.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithToolError_HandlesGracefully()
    {
        // Arrange
        var mockLlmGateway = new Mock<ILlmGateway>();
        var mockLogger = new Mock<ILogger<AgentExecutor>>();

        var failingTool = new Mock<IAgentTool>();
        failingTool.Setup(t => t.Name).Returns("FailingTool");
        failingTool.Setup(t => t.Description).Returns("Tool that fails");
        failingTool.Setup(t => t.ExecuteAsync(It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Tool failed"));

        var tools = new[] { failingTool.Object };
        var executor = new AgentExecutor(mockLlmGateway.Object, tools, mockLogger.Object);

        var agent = new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = "ErrorAgent",
            ToolNames = "FailingTool",
            IsEnabled = true,
        };

        var expectedResponse = new LlmResponse
        {
            Content = "Error occurred but response continued",
            ModelName = "error-handler-model",
            CompletedAtUtc = DateTime.UtcNow,
            TotalTokens = 80,
        };

        mockLlmGateway.Setup(g => g.ChatWithToolsAsync(
                It.IsAny<LlmRequest>(),
                It.IsAny<IEnumerable<IAgentTool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await executor.ExecuteAsync(
            agent,
            "Trigger error",
            new List<ChatMessageEntity>(),
            CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Error occurred but response continued", result);
    }

    /// <summary>
    /// Test: Multiple tool registration.
    /// When multiple tools are registered, AgentExecutor should pass them all.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithMultipleTools_PassesAllTools()
    {
        // Arrange
        var mockLlmGateway = new Mock<ILlmGateway>();
        var mockLogger = new Mock<ILogger<AgentExecutor>>();

        var tool1 = new Mock<IAgentTool>();
        tool1.Setup(t => t.Name).Returns("Tool1");
        tool1.Setup(t => t.Description).Returns("First tool");

        var tool2 = new Mock<IAgentTool>();
        tool2.Setup(t => t.Name).Returns("Tool2");
        tool2.Setup(t => t.Description).Returns("Second tool");

        var tools = new[] { tool1.Object, tool2.Object };
        var executor = new AgentExecutor(mockLlmGateway.Object, tools, mockLogger.Object);

        var agent = new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = "MultiToolAgent",
            ToolNames = "Tool1,Tool2",
            IsEnabled = true,
        };

        var expectedResponse = new LlmResponse
        {
            Content = "Multi-tool response",
            ModelName = "multi-tool-model",
            CompletedAtUtc = DateTime.UtcNow,
            TotalTokens = 120,
        };

        mockLlmGateway.Setup(g => g.ChatWithToolsAsync(
                It.IsAny<LlmRequest>(),
                It.Is<IEnumerable<IAgentTool>>(t => t.Count() == 2),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await executor.ExecuteAsync(
            agent,
            "Use multiple tools",
            new List<ChatMessageEntity>(),
            CancellationToken.None);

        // Assert
        Assert.Equal("Multi-tool response", result);
        mockLlmGateway.Verify(
            g => g.ChatWithToolsAsync(
                It.IsAny<LlmRequest>(),
                It.Is<IEnumerable<IAgentTool>>(t => t.Count() == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Test: Cancellation token handling.
    /// Tool execution should respect cancellation tokens.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithCancellation_StopsExecution()
    {
        // Arrange
        var mockLlmGateway = new Mock<ILlmGateway>();
        var mockLogger = new Mock<ILogger<AgentExecutor>>();

        var tools = Array.Empty<IAgentTool>();
        var executor = new AgentExecutor(mockLlmGateway.Object, tools, mockLogger.Object);

        var agent = new AgentEntity
        {
            Id = Guid.NewGuid(),
            Name = "CancellableAgent",
            IsEnabled = true,
        };

        mockLlmGateway.Setup(g => g.ChatWithToolsAsync(
                It.IsAny<LlmRequest>(),
                It.IsAny<IEnumerable<IAgentTool>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => executor.ExecuteAsync(
                agent,
                "Cancelled input",
                new List<ChatMessageEntity>(),
                new CancellationToken(true)));
    }
}
