using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Wiki;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class WikiServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly WikiService _wikiService;
    private readonly RepositoryRepository _repositoryRepository;

    public WikiServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.wiki.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(typeof(RepositoryEntity), typeof(WikiPageEntity));

        var currentUser = new TestCurrentUser("wiki.user");
        _repositoryRepository = new RepositoryRepository(_db, currentUser);
        var wikiPageRepository = new WikiPageRepository(_db, currentUser);
        _wikiService = new WikiService(wikiPageRepository, _repositoryRepository);
    }

    [Fact]
    public async Task Create_WithRepositoryAssociation_ShouldPersistAndReturnPage()
    {
        var repository = await CreateRepositoryAsync("repo-a");

        var created = await _wikiService.CreateAsync(new CreateWikiPageRequest
        {
            Title = "Wiki Home",
            Slug = "Home",
            Content = "# Welcome",
            RepositoryId = repository.Id,
        });

        var loaded = await _wikiService.GetByIdAsync(created.Id);

        Assert.Equal("Wiki Home", loaded.Title);
        Assert.Equal("home", loaded.Slug);
        Assert.Equal(repository.Id, loaded.RepositoryId);
    }

    [Fact]
    public async Task Search_WithKeywordAndRepositoryFilter_ShouldReturnMatchingItemsOnly()
    {
        var repositoryA = await CreateRepositoryAsync("repo-search-a");
        var repositoryB = await CreateRepositoryAsync("repo-search-b");

        await _wikiService.CreateAsync(new CreateWikiPageRequest
        {
            Title = "Setup Guide",
            Slug = "setup-guide",
            Content = "Install dependencies",
            RepositoryId = repositoryA.Id,
        });

        await _wikiService.CreateAsync(new CreateWikiPageRequest
        {
            Title = "Deploy Guide",
            Slug = "deploy-guide",
            Content = "Production deployment",
            RepositoryId = repositoryB.Id,
        });

        var result = await _wikiService.GetPagedAsync("setup", repositoryA.Id, 1, 20);

        Assert.Single(result.Items);
        Assert.Equal("setup-guide", result.Items[0].Slug);
    }

    [Fact]
    public async Task Create_WithDuplicateSlug_ShouldThrowValidationException()
    {
        await _wikiService.CreateAsync(new CreateWikiPageRequest
        {
            Title = "Page A",
            Slug = "duplicate",
            Content = "first",
        });

        await Assert.ThrowsAsync<ValidationException>(() => _wikiService.CreateAsync(new CreateWikiPageRequest
        {
            Title = "Page B",
            Slug = "Duplicate",
            Content = "second",
        }));
    }

    [Fact]
    public async Task Delete_ShouldHidePageFromPagedQuery()
    {
        var created = await _wikiService.CreateAsync(new CreateWikiPageRequest
        {
            Title = "To Delete",
            Slug = "to-delete",
            Content = "temporary",
        });

        await _wikiService.DeleteAsync(created.Id);
        var result = await _wikiService.GetPagedAsync(null, null, 1, 20);

        Assert.Empty(result.Items);
    }

    public void Dispose()
    {
        if (_db is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (!File.Exists(_dbPath))
        {
            return;
        }

        try
        {
            File.Delete(_dbPath);
        }
        catch (IOException)
        {
            // The OS can still hold a short-lived sqlite lock at teardown.
        }
    }

    private async Task<RepositoryEntity> CreateRepositoryAsync(string name)
    {
        return await _repositoryRepository.CreateAsync(new RepositoryEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Url = $"https://example.com/{name}.git",
            EncryptedToken = "encrypted-token",
        });
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        private readonly string? _ntId;

        public TestCurrentUser(string? ntId)
        {
            _ntId = ntId;
        }

        public string? GetCurrentNtId()
        {
            return _ntId;
        }

        public bool IsAdmin()
        {
            return false;
        }
    }
}