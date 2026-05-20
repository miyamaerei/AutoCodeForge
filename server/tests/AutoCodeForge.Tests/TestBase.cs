using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AutoCodeForge.Core.DTOs.Agent;
using AutoCodeForge.Core.DTOs.Chat;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Tests;

public abstract class TestBase
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    protected static async Task<string> WindowsLoginAndAuthenticateAsync(HttpClient client, string ntId)
    {
        client.DefaultRequestHeaders.Remove("X-NtId");
        client.DefaultRequestHeaders.Add("X-NtId", ntId);

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/windows-login", new { });
        loginResponse.EnsureSuccessStatusCode();

        var envelope = await loginResponse.Content.ReadFromJsonAsync<ApiEnvelope<AutoCodeForge.Core.DTOs.Auth.AuthResponse>>(JsonOptions);
        Assert.NotNull(envelope);
        Assert.True(envelope!.Success);
        Assert.NotNull(envelope.Data);
        Assert.False(string.IsNullOrWhiteSpace(envelope.Data!.AccessToken));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", envelope.Data.AccessToken);

        return envelope.Data.AccessToken;
    }

    protected static async Task<Guid> CreateAgentAsync(HttpClient client, CreateAgentRequest request)
    {
        var response = await client.PostAsJsonAsync("/api/v1/agents", request);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<AgentResponse>>(JsonOptions);
        Assert.NotNull(envelope?.Data);
        return envelope!.Data!.Id;
    }

    protected static async Task<Guid> CreateSessionAsync(HttpClient client, CreateSessionRequest request)
    {
        var response = await client.PostAsJsonAsync("/api/v1/chat/sessions", request);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<ChatSessionResponse>>(JsonOptions);
        Assert.NotNull(envelope?.Data);
        return envelope!.Data!.Id;
    }

    protected sealed class ApiEnvelope<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }
    }
}