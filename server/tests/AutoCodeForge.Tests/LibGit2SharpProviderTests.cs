using AutoCodeForge.Infrastructure.Git;
using LibGit2Sharp;

namespace AutoCodeForge.Tests;

public sealed class LibGit2SharpProviderTests : IDisposable
{
    private readonly string _root;
    private readonly string _bareRemotePath;
    private readonly string _sourcePath;
    private readonly string _targetPath;

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
    }

    [Fact]
    public async Task CloneOrPullAsync_ShouldCloneAndPullLatestCommit()
    {
        var provider = new LibGit2SharpProvider();

        var firstSha = await provider.CloneOrPullAsync(_bareRemotePath, string.Empty, "main", _targetPath);

        Assert.False(string.IsNullOrWhiteSpace(firstSha));

        var latestShaAfterSecondPush = PushSecondCommitToRemote();
        var secondSha = await provider.CloneOrPullAsync(_bareRemotePath, string.Empty, "main", _targetPath);

        Assert.Equal(latestShaAfterSecondPush, secondSha);

        using var targetRepo = new Repository(_targetPath);
        Assert.Equal(latestShaAfterSecondPush, targetRepo.Head.Tip?.Sha);
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
