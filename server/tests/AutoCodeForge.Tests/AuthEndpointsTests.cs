using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AutoCodeForge.Core.DTOs.Auth;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AutoCodeForge.Tests;

public sealed class AuthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly WebApplicationFactory<Program> _factory;

    public AuthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RegisterLoginAndMe_ShouldWorkEndToEnd()
    {
        using var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var registerRequest = new RegisterRequest
        {
            NtId = $"api.user.{suffix}",
            UserName = "API User",
            Email = "api.user@example.com",
            Password = "P@ssw0rd123",
        };

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var registerEnvelope = await registerResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthResponse>>(JsonOptions);
        Assert.NotNull(registerEnvelope);
        Assert.True(registerEnvelope!.Success);
        Assert.NotNull(registerEnvelope.Data);
        Assert.False(string.IsNullOrWhiteSpace(registerEnvelope.Data!.AccessToken));

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest
        {
            NtId = registerRequest.NtId,
            Password = registerRequest.Password,
        });
        loginResponse.EnsureSuccessStatusCode();
        var loginEnvelope = await loginResponse.Content.ReadFromJsonAsync<ApiEnvelope<AuthResponse>>(JsonOptions);
        Assert.NotNull(loginEnvelope);
        Assert.True(loginEnvelope!.Success);
        Assert.NotNull(loginEnvelope.Data);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginEnvelope.Data!.AccessToken);

        var meResponse = await client.GetAsync("/api/v1/auth/me");
        meResponse.EnsureSuccessStatusCode();

        using var meDoc = JsonDocument.Parse(await meResponse.Content.ReadAsStringAsync());
        Assert.True(meDoc.RootElement.GetProperty("success").GetBoolean());
        var ntId = meDoc.RootElement.GetProperty("data").GetProperty("ntId").GetString();
        Assert.Equal(registerRequest.NtId, ntId);
    }

    [Fact]
    public async Task MeWithoutToken_ShouldReturnUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed class ApiEnvelope<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }
    }
}
