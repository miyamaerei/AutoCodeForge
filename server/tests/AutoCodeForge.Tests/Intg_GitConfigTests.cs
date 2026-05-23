/**
 * Git 配置集成测试
 *
 * 使用 git-config.json 配置测试 GitHub/GitLab 仓库操作
 * 
 * 测试覆盖：
 * 1. 配置加载
 * 2. 仓库克隆
 * 3. 分支操作
 * 4. 文件读取
 */

using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Git;
using LibGit2Sharp;

namespace AutoCodeForge.Tests;

/// <summary>
/// Git 配置集成测试
/// </summary>
public sealed class GitConfigIntegrationTests : IDisposable
{
    private readonly IntegrationTestContext _context;
    private readonly GitTestConfig? _gitConfig;
    private readonly string _sandboxRoot = @"E:\git\AutoFrog\AutoCodeForge\sandbox";

    public GitConfigIntegrationTests()
    {
        _context = new IntegrationTestContext("git-test-user");
        _gitConfig = GitTestConfigLoader.TryLoad();
    }

    /// <summary>
    /// 测试配置加载
    /// </summary>
    [Fact]
    public void LoadGitConfig_ShouldReturnValidConfig()
    {
        // Arrange & Act
        var config = GitTestConfigLoader.Load();

        // Assert
        Assert.NotNull(config);
        Assert.False(string.IsNullOrEmpty(config.Token));
        Assert.False(string.IsNullOrEmpty(config.Repo));
        Assert.NotNull(config.Provider);

        Console.WriteLine($"[配置信息]");
        Console.WriteLine($"  仓库: {config.Repo}");
        Console.WriteLine($"  分支: {config.Branch}");
        Console.WriteLine($"  Provider: {config.Provider}");
    }

    /// <summary>
    /// 测试克隆 GitHub 仓库
    /// </summary>
    [Fact]
    public async Task CloneGitHubRepo_WithGitConfig_ShouldSucceed()
    {
        // Skip if config not available
        if (_gitConfig == null)
        {
            Console.WriteLine("[跳过] git-config.json 不存在");
            return;
        }

        // Arrange
        var tempWorkspace = Path.Combine(_sandboxRoot, $"git-test-{Guid.NewGuid():N}");
        var localRepoPath = Path.Combine(tempWorkspace, "repo");

        try
        {
            // 配置 Sandbox
            await _context.CreateAndSaveTestSandboxConfigAsync(_sandboxRoot);

            // 创建仓库记录
            var repoId = await _context.CreateTestRepositoryDirectlyAsync(
                "test-git-repo",
                _gitConfig.Repo,
                _gitConfig.Provider);

            Console.WriteLine($"[Step 1] 创建仓库记录: {repoId}");

            // Act - 克隆仓库
            var cloneOptions = new CloneOptions
            {
                Checkout = true
            };

            cloneOptions.FetchOptions.CredentialsProvider = (url, user, cred) => new UsernamePasswordCredentials
            {
                Username = _gitConfig.Provider == GitProvider.GitHub ? "x-access-token" : _gitConfig.Username,
                Password = _gitConfig.Token,
            };

            Repository.Clone(_gitConfig.Repo, localRepoPath, cloneOptions);

            // Assert
            Assert.True(Directory.Exists(localRepoPath));
            Assert.True(Directory.Exists(Path.Combine(localRepoPath, ".git")));

            Console.WriteLine($"[Step 2] 克隆成功: {localRepoPath}");

            // 验证分支
            using var repo = new Repository(localRepoPath);
            var branches = repo.Branches.ToList();
            Assert.NotEmpty(branches);

            Console.WriteLine($"[Step 3] 分支列表 ({branches.Count} 个):");
            foreach (var branch in branches.Take(5))
            {
                Console.WriteLine($"  - {branch.FriendlyName}");
            }
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempWorkspace))
            {
                TryDeleteDirectory(tempWorkspace);
            }
        }
    }

    /// <summary>
    /// 测试读取仓库文件
    /// </summary>
    [Fact]
    public async Task ReadRepoFiles_WithGitConfig_ShouldSucceed()
    {
        if (_gitConfig == null)
        {
            Console.WriteLine("[跳过] git-config.json 不存在");
            return;
        }

        // Arrange
        var tempWorkspace = Path.Combine(_sandboxRoot, $"git-read-{Guid.NewGuid():N}");
        var localRepoPath = Path.Combine(tempWorkspace, "repo");

        try
        {
            await _context.CreateAndSaveTestSandboxConfigAsync(_sandboxRoot);

            // 克隆仓库
            var cloneOptions = new CloneOptions
            {
                Checkout = true
            };

            cloneOptions.FetchOptions.CredentialsProvider = (url, user, cred) => new UsernamePasswordCredentials
            {
                Username = _gitConfig.Provider == GitProvider.GitHub ? "x-access-token" : _gitConfig.Username,
                Password = _gitConfig.Token,
            };

            Repository.Clone(_gitConfig.Repo, localRepoPath, cloneOptions);

            // Act - 读取文件
            var files = Directory.GetFiles(localRepoPath, "*", SearchOption.TopDirectoryOnly);

            // Assert
            Assert.NotEmpty(files);

            Console.WriteLine($"[文件列表] 共 {files.Length} 个文件:");
            foreach (var file in files.Take(10))
            {
                var fileName = Path.GetFileName(file);
                var fileSize = new FileInfo(file).Length;
                Console.WriteLine($"  - {fileName} ({fileSize} bytes)");
            }

            // 尝试读取 README
            var readmeFiles = files.Where(f => 
                f.EndsWith("README.md", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith("readme.md", StringComparison.OrdinalIgnoreCase)).ToList();

            if (readmeFiles.Any())
            {
                var readmeContent = await File.ReadAllTextAsync(readmeFiles.First());
                Console.WriteLine($"\n[README 内容预览]:\n{readmeContent.Take(200).ToArray()}");
            }
        }
        finally
        {
            if (Directory.Exists(tempWorkspace))
            {
                TryDeleteDirectory(tempWorkspace);
            }
        }
    }

    /// <summary>
    /// 测试切换分支
    /// </summary>
    [Fact]
    public async Task SwitchBranch_WithGitConfig_ShouldSucceed()
    {
        if (_gitConfig == null)
        {
            Console.WriteLine("[跳过] git-config.json 不存在");
            return;
        }

        // Arrange
        var tempWorkspace = Path.Combine(_sandboxRoot, $"git-branch-{Guid.NewGuid():N}");
        var localRepoPath = Path.Combine(tempWorkspace, "repo");

        try
        {
            await _context.CreateAndSaveTestSandboxConfigAsync(_sandboxRoot);

            var cloneOptions = new CloneOptions
            {
                BranchName = _gitConfig.Branch,
                Checkout = true
            };

            cloneOptions.FetchOptions.CredentialsProvider = (url, user, cred) => new UsernamePasswordCredentials
            {
                Username = _gitConfig.Provider == GitProvider.GitHub ? "x-access-token" : _gitConfig.Username,
                Password = _gitConfig.Token,
            };

            Repository.Clone(_gitConfig.Repo, localRepoPath, cloneOptions);

            using var repo = new Repository(localRepoPath);

            // Act - 获取当前分支
            var currentBranch = repo.Head.FriendlyName;

            // Assert
            Assert.Contains(_gitConfig.Branch, currentBranch);

            Console.WriteLine($"[当前分支] {currentBranch}");

            // 列出所有远程分支
            var remoteBranches = repo.Branches
                .Where(b => b.IsRemote)
                .Select(b => b.FriendlyName)
                .Distinct()
                .ToList();

            Console.WriteLine($"[远程分支列表] 共 {remoteBranches.Count} 个:");
            foreach (var branch in remoteBranches.Take(10))
            {
                Console.WriteLine($"  - {branch}");
            }
        }
        finally
        {
            if (Directory.Exists(tempWorkspace))
            {
                TryDeleteDirectory(tempWorkspace);
            }
        }
    }

    /// <summary>
    /// 测试使用 LibGit2SharpProvider
    /// </summary>
    [Fact]
    public async Task CloneWithLibGit2SharpProvider_WithGitConfig_ShouldSucceed()
    {
        if (_gitConfig == null)
        {
            Console.WriteLine("[跳过] git-config.json 不存在");
            return;
        }

        // Arrange
        var tempWorkspace = Path.Combine(_sandboxRoot, $"git-provider-{Guid.NewGuid():N}");
        var localRepoPath = Path.Combine(tempWorkspace, "repo");

        try
        {
            await _context.CreateAndSaveTestSandboxConfigAsync(_sandboxRoot);

            var provider = _context.LibGit2SharpProvider;

            // Act - 使用 Provider 克隆
            await Task.Run(() =>
            {
                var cloneOptions = new CloneOptions
                {
                    Checkout = true
                };

                cloneOptions.FetchOptions.CredentialsProvider = (url, user, cred) => new UsernamePasswordCredentials
                {
                    Username = _gitConfig.Provider == GitProvider.GitHub ? "x-access-token" : _gitConfig.Username,
                    Password = _gitConfig.Token,
                };

                Repository.Clone(_gitConfig.Repo, localRepoPath, cloneOptions);
            });

            // Assert
            Assert.True(Directory.Exists(localRepoPath));

            Console.WriteLine($"[LibGit2SharpProvider] 克隆成功: {localRepoPath}");
        }
        finally
        {
            if (Directory.Exists(tempWorkspace))
            {
                TryDeleteDirectory(tempWorkspace);
            }
        }
    }

    #region Helper Methods

    /// <summary>
    /// 尝试删除目录（带重试）
    /// </summary>
    private static void TryDeleteDirectory(string path, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                Directory.Delete(path, recursive: true);
                return;
            }
            catch (Exception ex)
            {
                if (i == maxRetries - 1)
                {
                    Console.WriteLine($"[警告] 无法删除目录: {path}, 错误: {ex.Message}");
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}
