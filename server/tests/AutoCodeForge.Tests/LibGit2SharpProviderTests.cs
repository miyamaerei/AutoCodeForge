using AutoCodeForge.Infrastructure.Git;
using AutoCodeForge.Core.Models;
using LibGit2Sharp;

namespace AutoCodeForge.Tests;

public sealed class LibGit2SharpProviderTests : IDisposable
{
    private readonly string _root;
    private readonly string _bareRemotePath;
    private readonly string _sourcePath;
    private readonly string _targetPath;
    private readonly GitOptions _defaultGitOptions;

    public LibGit2SharpProviderTests()
    {
        _root = Path.Combine(Path.GetTempPath(), $"autocodeforge.gitclone.{Guid.NewGuid():N}");
        _bareRemotePath = Path.Combine(_root, "remote.git");
        _sourcePath = Path.Combine(_root, "source");
        _targetPath = Path.Combine(_root, "target");

        Directory.CreateDirectory(_root);
        Directory.CreateDirectory(_sourcePath);

        Repository.Init(_bareRemotePath, isBare: true);
        Repository.Init(_sourcePath);

        using var sourceRepo = new Repository(_sourcePath);
        var author = new Signature("tester", "tester@example.com", DateTimeOffset.UtcNow);

        File.WriteAllText(Path.Combine(_sourcePath, "README.md"), "first\n");
        Commands.Stage(sourceRepo, "README.md");
        sourceRepo.Commit("initial commit", author, author);

        var mainBranch = sourceRepo.CreateBranch("main");
        Commands.Checkout(sourceRepo, mainBranch);

        sourceRepo.Network.Remotes.Add("origin", _bareRemotePath);
        var origin = sourceRepo.Network.Remotes["origin"] ?? throw new InvalidOperationException("origin remote missing");
        sourceRepo.Network.Push(origin, "refs/heads/main:refs/heads/main", new PushOptions());

        _defaultGitOptions = new GitOptions
        {
            AzureDevOps = new AzureDevOpsOptions
            {
                DomainPatterns = new List<string> { "dev.azure.com", "visualstudio.com" },
                Username = string.Empty,
                EnableUrlEncoding = true,
            },
            StringHandling = new StringHandlingOptions
            {
                GitHubUsername = "x-access-token",
                GitLabUsername = "oauth2",
                SpecialCharacters = new List<string> { " ", "@", "#", "$", "%", "^", "&", "*", "(", ")", "+", ",", ";", "=", "?", "/" },
                NormalizeWhitespace = true,
                WhitespaceReplacement = "_",
            },
            Path = new PathOptions
            {
                MaxPathLength = 260,
                TempPathTemplate = "C:\\temp\\repo-{guid}",
                AutoShortenPaths = true,
                ShortPathIdLength = 3,
            },
            Providers = new ProviderOptions
            {
                DefaultUsername = "git",
            },
        };
    }

    [Fact]
    public async Task CloneOrPullAsync_ShouldCloneAndPullLatestCommit()
    {
        var provider = new LibGit2SharpProvider(_defaultGitOptions);

        var firstSha = await provider.CloneOrPullAsync(_bareRemotePath, string.Empty, "main", _targetPath);

        Assert.False(string.IsNullOrWhiteSpace(firstSha));

        // 验证文件已检出
        Assert.True(File.Exists(Path.Combine(_targetPath, "README.md")), "README.md should exist after clone");

        var latestShaAfterSecondPush = PushSecondCommitToRemote();
        var secondSha = await provider.CloneOrPullAsync(_bareRemotePath, string.Empty, "main", _targetPath);

        // 验证拉取后 SHA 已更新
        Assert.False(string.IsNullOrWhiteSpace(secondSha));
        
        using var targetRepo = new Repository(_targetPath);
        Assert.Equal(latestShaAfterSecondPush, targetRepo.Head.Tip?.Sha);
    }

    [Fact]
    public void DetermineProviderType_ShouldIdentifyAzureDevOps()
    {
        var provider = new LibGit2SharpProvider(_defaultGitOptions);

        var result = provider.DetermineProviderType("https://dev.azure.com/org/project/_git/repo");
        Assert.Equal("AzureDevOps", result);

        result = provider.DetermineProviderType("https://myorg.visualstudio.com/project/_git/repo");
        Assert.Equal("AzureDevOps", result);
    }

    [Fact]
    public void DetermineProviderType_ShouldIdentifyGitHub()
    {
        var provider = new LibGit2SharpProvider(_defaultGitOptions);

        var result = provider.DetermineProviderType("https://github.com/user/repo.git");
        Assert.Equal("GitHub", result);
    }

    [Fact]
    public void DetermineProviderType_ShouldIdentifyGitLab()
    {
        var provider = new LibGit2SharpProvider(_defaultGitOptions);

        var result = provider.DetermineProviderType("https://gitlab.com/user/repo.git");
        Assert.Equal("GitLab", result);
    }

    [Fact]
    public void DetermineProviderType_ShouldReturnGenericForUnknownProviders()
    {
        var provider = new LibGit2SharpProvider(_defaultGitOptions);

        var result = provider.DetermineProviderType("https://git.example.com/user/repo.git");
        Assert.Equal("Generic", result);
    }

    [Fact]
    public void GetUsernameForProvider_ShouldReturnCorrectUsernames()
    {
        var provider = new LibGit2SharpProvider(_defaultGitOptions);

        Assert.Equal(string.Empty, provider.GetUsernameForProvider("AzureDevOps"));
        Assert.Equal("x-access-token", provider.GetUsernameForProvider("GitHub"));
        Assert.Equal("oauth2", provider.GetUsernameForProvider("GitLab"));
        Assert.Equal("git", provider.GetUsernameForProvider("Generic"));
    }

    [Fact]
    public void EncodePathSegment_ShouldEncodeSpecialCharacters()
    {
        var provider = new LibGit2SharpProvider(_defaultGitOptions);

        var result = provider.EncodePathSegment("JGP Applications");

        Assert.Equal("JGP_Applications", result);
    }

    [Fact]
    public void EncodePathSegment_ShouldNotEncodeWhenDisabled()
    {
        var options = new GitOptions
        {
            AzureDevOps = new AzureDevOpsOptions
            {
                EnableUrlEncoding = false,
            },
            StringHandling = new StringHandlingOptions
            {
                NormalizeWhitespace = true,
                WhitespaceReplacement = "_",
                SpecialCharacters = new List<string> { " " },
            },
        };

        var provider = new LibGit2SharpProvider(options);

        var result = provider.EncodePathSegment("JGP Applications");

        Assert.Equal("JGP Applications", result);
    }

    [Fact]
    public void EncodePathSegment_ShouldHandleEmptyString()
    {
        var provider = new LibGit2SharpProvider(_defaultGitOptions);

        var result = provider.EncodePathSegment(string.Empty);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void EncodePathSegment_ShouldHandleNull()
    {
        var provider = new LibGit2SharpProvider(_defaultGitOptions);

        var result = provider.EncodePathSegment(null!);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetUsernameForProvider_WithCustomAzureDevOpsUsername()
    {
        var options = new GitOptions
        {
            AzureDevOps = new AzureDevOpsOptions
            {
                Username = "custom-user",
            },
            StringHandling = new StringHandlingOptions
            {
                GitHubUsername = "x-access-token",
                GitLabUsername = "oauth2",
            },
            Providers = new ProviderOptions
            {
                DefaultUsername = "git",
            },
        };

        var provider = new LibGit2SharpProvider(options);

        Assert.Equal("custom-user", provider.GetUsernameForProvider("AzureDevOps"));
    }

    private string PushSecondCommitToRemote()
    {
        using var sourceRepo = new Repository(_sourcePath);
        var author = new Signature("tester", "tester@example.com", DateTimeOffset.UtcNow);

        File.WriteAllText(Path.Combine(_sourcePath, "README.md"), "second\n");
        Commands.Stage(sourceRepo, "README.md");
        var commit = sourceRepo.Commit("second commit", author, author);

        var origin = sourceRepo.Network.Remotes["origin"] ?? throw new InvalidOperationException("origin remote missing");
        sourceRepo.Network.Push(origin, "refs/heads/main:refs/heads/main", new PushOptions());
        return commit.Sha;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup issues on transient file lock.
        }
    }
}