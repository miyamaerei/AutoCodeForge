using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AutoCodeForge.Core.DTOs.Auth;

namespace AutoCodeForge.Tests;

public sealed class AuthEndpointsTests : TestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task WindowsLoginAndMe_ShouldWorkEndToEnd()
    {
        using var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var ntId = $"api.user.{suffix}";
        client.DefaultRequestHeaders.Add("X-NtId", ntId);

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/windows-login", new { });
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
        var responseNtId = meDoc.RootElement.GetProperty("data").GetProperty("ntId").GetString();
        Assert.Equal(ntId, responseNtId);
    }

    [Fact]
    public async Task MeWithoutToken_ShouldReturnUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
