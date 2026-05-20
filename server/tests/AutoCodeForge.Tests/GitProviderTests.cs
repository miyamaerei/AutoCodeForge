using Moq;
using AutoCodeForge.Core.DTOs.Repository;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Infrastructure.Git;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Text;

namespace AutoCodeForge.Tests;

/// <summary>
/// Unit tests for Git providers.
/// </summary>
[TestClass]
public class GitProviderTests
{
    private GitProviderFactory _factory = null!;
    private StubHttpMessageHandler _httpHandler = null!;

    [TestInitialize]
    public void Setup()
    {
        _httpHandler = new StubHttpMessageHandler();
        _factory = new GitProviderFactory(new HttpClient(_httpHandler));
    }

    [TestMethod]
    public void GitProviderFactory_CreateGitHubProvider_ReturnsGitHubProvider()
    {
        // Act
        var provider = _factory.CreateProvider(GitProvider.GitHub);

        // Assert
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(provider);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsInstanceOfType(provider, typeof(GitHubProvider));
    }

    [TestMethod]
    public void GitProviderFactory_CreateGitLabProvider_ReturnsGitLabProvider()
    {
        // Act
        var provider = _factory.CreateProvider(GitProvider.GitLab);

        // Assert
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(provider);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsInstanceOfType(provider, typeof(GitLabProvider));
    }

    [TestMethod]
    public void GitProviderFactory_CreateAzureDevOpsProvider_ReturnsAzureDevOpsProvider()
    {
        // Act
        var provider = _factory.CreateProvider(GitProvider.AzureDevOps);

        // Assert
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(provider);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsInstanceOfType(provider, typeof(AzureDevOpsProvider));
    }

    [TestMethod]
    public async Task GitHubProvider_VerifyCredentials_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var provider = _factory.CreateProvider(GitProvider.GitHub);
        var invalidUrl = "https://github.com/invalid/repo";
        var invalidToken = "invalid_token";
        _httpHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };

        // Act
        var result = await provider!.VerifyCredentialsAsync(invalidUrl, invalidToken);

        // Assert
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task GitLabProvider_VerifyCredentials_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var provider = _factory.CreateProvider(GitProvider.GitLab);
        var invalidUrl = "https://gitlab.com/invalid/repo";
        var invalidToken = "invalid_token";
        _httpHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };

        // Act
        var result = await provider!.VerifyCredentialsAsync(invalidUrl, invalidToken);

        // Assert
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task AzureDevOpsProvider_VerifyCredentials_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var provider = _factory.CreateProvider(GitProvider.AzureDevOps);
        var invalidUrl = "https://dev.azure.com/org/project/_git/repo";
        var invalidToken = "invalid_token";
        _httpHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };

        // Act
        var result = await provider!.VerifyCredentialsAsync(invalidUrl, invalidToken);

        // Assert
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(result);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage> ResponseFactory { get; set; } = _ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json"),
            };

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(ResponseFactory(request));
        }
    }
}
