using System.Net;
using System.Net.Http.Json;
using AutoCodeForge.Core.DTOs.Agent;
using AutoCodeForge.Core.DTOs.Chat;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Tests;

/// <summary>
/// Smoke tests for Agent CRUD and Chat endpoints.
/// Verifies end-to-end workflow: user registration → agent creation → chat session → message send.
/// </summary>
public sealed class AgentChatSmokeTests : TestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AgentChatSmokeTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Smoke test: User registration → Agent creation → Chat session → Message.
    /// Verifies the full pipeline works end-to-end.
    /// </summary>
    [Fact]
    public async Task FullAgentChatWorkflow_ShouldWork()
    {
        using var client = _factory.CreateClient();

        var registerRequest = TestDataFactory.CreateRegisterRequest("agent.test");
        await WindowsLoginAndAuthenticateAsync(client, registerRequest.NtId);

        var createAgentRequest = TestDataFactory.CreateAgentRequest("smoke");
        var agentId = await CreateAgentAsync(client, createAgentRequest);

        var createSessionRequest = TestDataFactory.CreateSessionRequest(agentId, "smoke");
        var sessionId = await CreateSessionAsync(client, createSessionRequest);

        var sendMessageRequest = TestDataFactory.CreateSendMessageRequest("Hello, how are you?", agentId);

        var sendResponse = await client.PostAsJsonAsync($"/api/v1/chat/sessions/{sessionId}/messages", sendMessageRequest);
        sendResponse.EnsureSuccessStatusCode();
        var messageData = await sendResponse.Content.ReadFromJsonAsync<ApiResponse<SendMessageResponse>>(JsonOptions);
        Assert.NotNull(messageData?.Data?.UserMessage);
        Assert.NotNull(messageData?.Data?.AssistantMessage);

        var getMessagesResponse = await client.GetAsync($"/api/v1/chat/sessions/{sessionId}/messages");
        getMessagesResponse.EnsureSuccessStatusCode();
        var messagesData = await getMessagesResponse.Content.ReadFromJsonAsync<ApiResponse<List<ChatMessageResponse>>>(JsonOptions);
        Assert.NotNull(messagesData?.Data);
        Assert.True(messagesData!.Data!.Count >= 2); // At least user and assistant
    }

    /// <summary>
    /// Smoke test: Chat streaming endpoint basic connectivity.
    /// Verifies SSE stream responds without error.
    /// </summary>
    [Fact]
    public async Task ChatStreamingEndpoint_ShouldReturnSSEStream()
    {
        using var client = _factory.CreateClient();
        var registerRequest = TestDataFactory.CreateRegisterRequest("stream.test");
        await WindowsLoginAndAuthenticateAsync(client, registerRequest.NtId);

        var agentId = await CreateAgentAsync(client, TestDataFactory.CreateAgentRequest("stream"));
        var sessionId = await CreateSessionAsync(client, TestDataFactory.CreateSessionRequest(agentId, "stream"));

        var streamRequest = TestDataFactory.CreateSendMessageRequest("Hello stream", agentId);
        var streamResponse = await client.PostAsJsonAsync($"/api/v1/chat/sessions/{sessionId}/stream", streamRequest);

        Assert.Equal(HttpStatusCode.OK, streamResponse.StatusCode);
        var contentType = streamResponse.Content.Headers.ContentType?.MediaType;
        Assert.Equal("text/event-stream", contentType);

        var content = await streamResponse.Content.ReadAsStringAsync();
        Assert.Contains("event:", content);
        Assert.Contains("data:", content);
        Assert.Contains("event: done", content);
        Assert.Contains("[AutoCodeForge AI] Hello stream", content);
    }
}
