using Moq;
using AutoCodeForge.Core.DTOs.Repository;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Git;
using AutoCodeForge.Infrastructure.Repositories;
using AutoCodeForge.Infrastructure.Services;
using AutoCodeForge.Application.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSugar;
using System.Net;
using System.Text;

namespace AutoCodeForge.Tests;

/// <summary>
/// Unit tests for RepositoryService.
/// </summary>
[TestClass]
public class RepositoryServiceTests
{
    private RepositoryService _service = null!;
    private Mock<RepositoryRepository> _mockRepository = null!;
    private GitProviderFactory _factory = null!;
    private DataProtectionService _dataProtection = null!;
    private StubHttpMessageHandler _httpHandler = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<RepositoryRepository>(
            Mock.Of<ISqlSugarClient>(),
            Mock.Of<ICurrentUser>());

        _httpHandler = new StubHttpMessageHandler();
        _factory = new GitProviderFactory(new HttpClient(_httpHandler));
        _dataProtection = new DataProtectionService(new StubDataProtectionProvider());

        _service = new RepositoryService(_mockRepository.Object, _factory, _dataProtection);
    }

    [TestMethod]
    public async Task CreateAsync_WithValidRequest_CreatesRepository()
    {
        // Arrange
        var request = new CreateRepositoryRequest
        {
            Name = "Test Repo",
            Url = "https://github.com/test/repo",
            Provider = GitProvider.GitHub,
            AuthType = AuthenticationType.Token,
            Token = "test_token",
            MergeStrategy = MergeStrategy.Squash,
        };

        _mockRepository
            .Setup(r => r.GetByUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RepositoryEntity?)null);

        _httpHandler.ResponseFactory = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };

        var createdEntity = new RepositoryEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Url = request.Url,
            Provider = request.Provider,
            AuthType = request.AuthType,
            MergeStrategy = request.MergeStrategy,
            EncryptedToken = _dataProtection.Encrypt(request.Token),
        };

        _mockRepository
            .Setup(r => r.CreateAsync(It.IsAny<RepositoryEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdEntity);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(result);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(request.Name, result.Name);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(request.Url, result.Url);
    }

    [TestMethod]
    public async Task CreateAsync_WithDuplicateUrl_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateRepositoryRequest
        {
            Name = "Test Repo",
            Url = "https://github.com/test/repo",
            Provider = GitProvider.GitHub,
            AuthType = AuthenticationType.Token,
            Token = "test_token",
            MergeStrategy = MergeStrategy.Squash,
        };

        var existingEntity = new RepositoryEntity
        {
            Id = Guid.NewGuid(),
            Name = "Existing",
            Url = request.Url,
            Provider = GitProvider.GitHub,
        };

        _mockRepository
            .Setup(r => r.GetByUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        await Microsoft.VisualStudio.TestTools.UnitTesting.Assert.ThrowsExceptionAsync<ValidationException>(() =>
            _service.CreateAsync(request));
    }

    [TestMethod]
    public async Task CreateAsync_WithInvalidUrl_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateRepositoryRequest
        {
            Name = "Test Repo",
            Url = "not_a_valid_url",
            Provider = GitProvider.GitHub,
            AuthType = AuthenticationType.Token,
            Token = "test_token",
            MergeStrategy = MergeStrategy.Squash,
        };

        await Microsoft.VisualStudio.TestTools.UnitTesting.Assert.ThrowsExceptionAsync<ValidationException>(() =>
            _service.CreateAsync(request));
    }

    [TestMethod]
    public async Task GetByIdAsync_WithValidId_ReturnsRepository()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();
        var entity = new RepositoryEntity
        {
            Id = repositoryId,
            Name = "Test Repo",
            Url = "https://github.com/test/repo",
            Provider = GitProvider.GitHub,
            AuthType = AuthenticationType.Token,
            MergeStrategy = MergeStrategy.Squash,
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _service.GetByIdAsync(repositoryId);

        // Assert
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(result);
        Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(entity.Name, result.Name);
    }

    [TestMethod]
    public async Task GetByIdAsync_WithInvalidId_ThrowsValidationException()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RepositoryEntity?)null);

        await Microsoft.VisualStudio.TestTools.UnitTesting.Assert.ThrowsExceptionAsync<ValidationException>(() =>
            _service.GetByIdAsync(Guid.NewGuid()));
    }

    [TestMethod]
    public async Task DeleteAsync_WithValidId_SoftDeletesRepository()
    {
        // Arrange
        var repositoryId = Guid.NewGuid();

        // Act
        await _service.DeleteAsync(repositoryId);

        // Assert
        _mockRepository.Verify(
            r => r.SoftDeleteAsync(repositoryId, false, It.IsAny<CancellationToken>()),
            Times.Once);
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

    private sealed class StubDataProtectionProvider : IDataProtectionProvider
    {
        public IDataProtector CreateProtector(string purpose)
        {
            return new StubDataProtector();
        }
    }

    private sealed class StubDataProtector : IDataProtector
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

        public string Protect(string plaintext)
        {
            return $"enc::{plaintext}";
        }

        public string Unprotect(string protectedData)
        {
            return protectedData.Replace("enc::", string.Empty, StringComparison.Ordinal);
        }
    }
}
