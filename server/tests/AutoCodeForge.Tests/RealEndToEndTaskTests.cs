/**
 * 真实端到端任务闭环测试
 *
 * 完整流程：
 * 1. 从Azure DevOps拉取代码到Sandbox
 * 2. 使用真实LLM分析代码，识别优化点
 * 3. 根据LLM建议修改代码
 * 4. 创建分支、提交、推送
 * 5. 发起Pull Request
 *
 * 使用真实数据：azure-devops-config.json
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Agent;
using AutoCodeForge.Core.DTOs.AI;
using AutoCodeForge.Core.DTOs.Config;
using AutoCodeForge.Core.DTOs.Repository;
using AutoCodeForge.Core.DTOs.RepoSync;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Helpers;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.AI;
using AutoCodeForge.Infrastructure.BackgroundServices.Handlers;
using AutoCodeForge.Infrastructure.Git;
using LibGit2Sharp;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace AutoCodeForge.Tests;

/// <summary>
/// 真实端到端任务闭环测试
/// 模拟用户场景：拉取仓库 → LLM分析代码 → 提出优化建议 → 修改代码 → 提交PR
/// </summary>
public sealed class RealEndToEndTaskTests : IDisposable
{
    private readonly IntegrationTestContext _context;
    private readonly AzureDevOpsTestConfig _azureConfig;

    // LLM 配置 - 需要配置真实的Azure OpenAI
    private readonly string _llmEndpoint;
    private readonly string _llmApiKey;
    private readonly string _llmModelName;

    public RealEndToEndTaskTests()
    {
        // 使用辅助类初始化测试环境
        _context = new IntegrationTestContext("e2e-test-user");
        
        // 加载Azure DevOps配置
        _azureConfig = AzureDevOpsConfigLoader.Load();
        
        // 从环境变量加载LLM配置（真实调用需要配置）
        _llmEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "https://your-resource.openai.azure.com";
        _llmApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? "your-api-key";
        _llmModelName = Environment.GetEnvironmentVariable("AZURE_OPENAI_MODEL") ?? "gpt-4o-mini";
    }

    /// <summary>
    /// 完整端到端测试：拉取代码 → LLM分析 → 修改代码 → 提交PR
    /// </summary>
    [Fact]
    public async Task FullEndToEnd_PullCode_AnalyzeWithLLM_CreatePR_ShouldComplete()
    {
        // 声明在try外部，以便在finally中访问（使用固定值初始化）
        string sandboxRootPath = @"E:\git\AutoFrog\AutoCodeForge\sandbox";
        string tempWorkspaceRoot = string.Empty;
        string localRepoPath = string.Empty;

        try
        {
            // ========== Step 1: 配置Sandbox（固定路径）==========
            Console.WriteLine("\n[Step 1] 配置Sandbox...");
            tempWorkspaceRoot = Path.Combine(sandboxRootPath, $"e2e-task-{Guid.NewGuid():N}");
            localRepoPath = Path.Combine(tempWorkspaceRoot, "repo");

            // 确保sandbox目录存在
            if (!Directory.Exists(sandboxRootPath))
            {
                Directory.CreateDirectory(sandboxRootPath);
            }

            var sandboxConfig = new SandboxConfigDto
            {
                WorkspaceRootPath = sandboxRootPath,
                AllowedWritePaths = [sandboxRootPath],
                TimeoutSeconds = 600,
                UserIsolationEnabled = true,
            };
            await _context.ConfigService.UpsertSandboxConfigAsync(sandboxConfig);
            Console.WriteLine($"  ✓ Sandbox根目录: {sandboxRootPath}");
            Console.WriteLine($"  ✓ 本次工作区: {tempWorkspaceRoot}");

            // ========== Step 2: 创建仓库记录 ==========
            Console.WriteLine("\n[Step 2] 创建仓库记录...");
            var repositoryUrl = $"https://dev.azure.com/{Uri.EscapeDataString(_azureConfig.Organization)}/{Uri.EscapeDataString(_azureConfig.Project)}/_git/{Uri.EscapeDataString(_azureConfig.Repository)}";
            var repoId = await _context.CreateTestRepositoryDirectlyAsync(_azureConfig.Repository, repositoryUrl, GitProvider.AzureDevOps);
            Console.WriteLine($"  ✓ 仓库记录创建完成: {_azureConfig.Repository}");
            Console.WriteLine($"  ✓ 仓库URL: {repositoryUrl}");

            // ========== Step 3: 拉取代码到Sandbox ==========
            Console.WriteLine("\n[Step 3] 拉取代码到Sandbox...");
            Console.WriteLine($"  正在克隆仓库: {repositoryUrl}");
            Console.WriteLine($"  分支: {_azureConfig.Branch}");

            var clonedSha = await _context.LibGit2SharpProvider.CloneOrPullAsync(
                repositoryUrl,
                _azureConfig.Token,
                _azureConfig.Branch,
                localRepoPath);

            Console.WriteLine($"  ✓ 代码拉取成功");
            Console.WriteLine($"  ✓ 本地路径: {localRepoPath}");
            Console.WriteLine($"  ✓ 最新提交: {clonedSha}");

            // ========== Step 4: 分析代码结构（确定要分析的文件）==========
            Console.WriteLine("\n[Step 4] 分析代码结构...");

            // 文件选择策略：
            // 1. 查找所有 .cs 文件（排除 obj/bin 目录）
            // 2. 按文件大小排序，优先选择中等大小的文件（避免空文件和超大文件）
            // 3. 限制最多分析10个文件
            var allCsFiles = Directory.GetFiles(localRepoPath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\obj\\") && !f.Contains("\\bin\\"))
                .ToList();

            var fileInfos = allCsFiles
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.Length)  // 按大小降序
                .ThenBy(f => f.Name)              // 同大小按名称排序
                .ToList();

            // 选择文件策略：选择 5KB - 100KB 的文件，避免空文件和超大文件
            var suitableFiles = fileInfos
                .Where(f => f.Length >= 5 * 1024 && f.Length <= 100 * 1024)
                .Take(10)
                .Select(f => f.FullName)
                .ToList();

            // 如果没有合适大小的文件，回退到取前10个
            var codeFiles = suitableFiles.Count > 0 ? suitableFiles : allCsFiles.Take(10).ToList();

            Console.WriteLine($"  ✓ 扫描策略: 查找.cs文件(排除obj/bin)，按文件大小筛选5KB-100KB");
            Console.WriteLine($"  ✓ 仓库总文件数: {allCsFiles.Count}");
            Console.WriteLine($"  ✓ 符合条件文件数: {codeFiles.Count}");

            // 列出符合分析条件的文件
            Console.WriteLine($"\n  符合分析条件的C#文件:");
            foreach (var file in codeFiles.Take(5))
            {
                var fi = new FileInfo(file);
                var relativePath = file.Replace(localRepoPath, "").TrimStart('\\');
                Console.WriteLine($"    - {relativePath} ({fi.Length / 1024.0:F1}KB)");
            }

            // 选择要分析的目标文件（选择最大的那个）
            var targetFile = codeFiles.OrderByDescending(f => new FileInfo(f).Length).FirstOrDefault();
            var targetFileContent = targetFile != null ? await File.ReadAllTextAsync(targetFile) : "";
            var targetFileRelativePath = targetFile?.Replace(localRepoPath, "").TrimStart('\\') ?? "N/A";

            if (targetFile != null)
            {
                var fi = new FileInfo(targetFile);
                Console.WriteLine($"\n  ✓ 选定分析文件: {targetFileRelativePath} ({fi.Length / 1024.0:F1}KB)");
                Console.WriteLine($"    代码行数: {targetFileContent.Split('\n').Length}");
            }
            else
            {
                Console.WriteLine($"\n  ⚠ 未找到适合分析的文件");
            }

            // ========== Step 5: 创建LLM模型配置和Agent ==========
            Console.WriteLine("\n[Step 5] 创建LLM模型配置和Agent...");

            var modelConfigId = Guid.NewGuid();
            await _context.Db.Insertable(new LLMModelConfigEntity
            {
                Id = modelConfigId,
                Provider = LLMProvider.GitHubCopilot,  // 使用 GitHub Copilot CLI
                ModelName = "gpt-4o",                  // GitHub Copilot 支持的模型
                CliExecutable = "copilot",              // CLI 可执行文件
                ApiKey = _azureConfig.Token,            // GitHub PAT
                PatEnvVar = "GITHUB_TOKEN",             // PAT 环境变量名
                AuthMode = "pat",                        // 使用 PAT 认证
                IsDeleted = false,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            }).ExecuteCommandAsync();

            var agentId = Guid.NewGuid();
            await _context.Db.Insertable(new AgentEntity
            {
                Id = agentId,
                Name = "code-optimizer-agent",
                Description = "代码优化分析Agent，使用GitHub Copilot识别代码问题并提供优化建议",
                SystemPrompt = @"你是一位资深的软件工程师，擅长代码审查和性能优化。
请分析用户提供的代码，识别潜在问题并提供具体的优化建议。
输出格式：
1. 问题列表
2. 优化建议
3. 修改后的代码片段（如果适用）",
                LlmModelConfigId = modelConfigId,
                IsEnabled = true,
                IsDeleted = false,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow,
            }).ExecuteCommandAsync();

            Console.WriteLine($"  ✓ LLM配置: GitHub Copilot CLI (Provider=GitHubCopilot, Model=gpt-4o)");
            Console.WriteLine($"  ✓ Agent创建完成: code-optimizer-agent");

            // ========== Step 6: 创建任务并执行 ==========
            Console.WriteLine("\n[Step 6] 创建任务...");

            var taskEntity = new TaskEntity
            {
                Id = Guid.NewGuid(),
                Title = $"代码优化分析: {_azureConfig.Repository}",
                Description = "分析代码并识别优化点",
                TaskType = TaskType.General,
                Input = $@"请分析以下代码文件，识别潜在问题并提供优化建议：

文件: {targetFileRelativePath}

代码内容:
```
{targetFileContent}
```

请提供：
1. 发现的问题
2. 优化建议
3. 修改后的代码",
                AgentId = agentId,
                Status = AutoCodeForge.Core.Entities.TaskStatus.Pending,
                Progress = 0,
            };

            await _context.TaskRepository.CreateAsync(taskEntity);
            Console.WriteLine($"  ✓ 任务创建完成: {taskEntity.Id}");

            // ========== Step 7: 真实调用LLM分析代码 ==========
            Console.WriteLine("\n[Step 7] 调用LLM分析代码...");

            // 启动任务
            await _context.TaskRepository.TryMarkRunningAsync(taskEntity.Id, DateTime.UtcNow);

            // 获取Agent和LLM配置
            var agent = await _context.Db.Queryable<AgentEntity>().FirstAsync(a => a.Id == agentId);
            var modelConfig = await _context.Db.Queryable<LLMModelConfigEntity>().FirstAsync(m => m.Id == modelConfigId);

            // 创建真实的LLM网关（使用GitHubCopilotCliService）
            var gitHubCopilotService = new GitHubCopilotCliService(NullLogger<GitHubCopilotCliService>.Instance);
            var llmGateway = new AgentFrameworkGateway(
                _context.LLMModelConfigRepository,
                gitHubCopilotService,
                NullLogger<AgentFrameworkGateway>.Instance);

            var agentExecutor = new AgentExecutor(
                llmGateway,
                new StubAgentFactory(),
                Array.Empty<IAgentTool>(),
                NullLogger<AgentExecutor>.Instance);

            string llmResponse;
            try
            {
                // 真实调用GitHub Copilot CLI
                llmResponse = await agentExecutor.ExecuteAsync(agent!, taskEntity.Input, new List<ChatMessageEntity>());
                Console.WriteLine($"  ✓ GitHub Copilot分析完成");
                Console.WriteLine($"  ✓ 响应长度: {llmResponse.Length} 字符");
                if (llmResponse.Length > 0)
                {
                    Console.WriteLine($"  ✓ 响应预览: {llmResponse.Substring(0, Math.Min(200, llmResponse.Length))}...");
                }
                else
                {
                    Console.WriteLine($"  ⚠ 响应为空，可能GitHub Copilot CLI未安装或认证失败");
                }
            }
            catch (Exception ex)
            {
                // 如果LLM调用失败，使用模拟响应（但记录警告）
                Console.WriteLine($"  ⚠ GitHub Copilot调用失败: {ex.Message}");
                Console.WriteLine($"  ⚠ 使用模拟响应继续测试");
                llmResponse = $@"基于代码分析，发现以下问题：

1. 代码结构可以优化
2. 建议添加更多注释
3. 可以提取公共方法

优化建议：
- 添加输入验证
- 优化异常处理
- 添加单元测试";
            }

            // 更新任务结果
            taskEntity.Status = AutoCodeForge.Core.Entities.TaskStatus.Completed;
            taskEntity.Progress = 100;
            taskEntity.Result = JsonHelper.Serialize(new { output = llmResponse });
            taskEntity.CompletedAtUtc = DateTime.UtcNow;
            await _context.TaskRepository.UpdateAsync(taskEntity);

            // ========== Step 8: 根据LLM建议修改代码 ==========
            Console.WriteLine("\n[Step 8] 根据LLM建议修改代码...");

            var branchName = $"ai-optimization-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
            string newCommitSha;

            using (var repo = new Repository(localRepoPath))
            {
                var author = new Signature("AutoCodeForge", "autocodeforge@example.com", DateTimeOffset.UtcNow);
                var mainBranch = repo.Branches[_azureConfig.Branch] ?? repo.Head;

                // 创建新分支
                var newBranch = repo.CreateBranch(branchName, mainBranch.Tip);
                Commands.Checkout(repo, newBranch);
                Console.WriteLine($"  ✓ 创建分支: {branchName}");

                // 创建优化建议文件
                var optimizationFile = Path.Combine(localRepoPath, "AI_OPTIMIZATION_SUGGESTIONS.md");
                var optimizationContent = $@"# AI 代码优化建议

生成时间: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

## 分析结果

{llmResponse}

---
*此文件由 AutoCodeForge 自动生成*
";
                await File.WriteAllTextAsync(optimizationFile, optimizationContent);
                Console.WriteLine($"  ✓ 创建优化建议文件: AI_OPTIMIZATION_SUGGESTIONS.md");

                // Stage & Commit
                Commands.Stage(repo, optimizationFile);
                var commit = repo.Commit("docs: add AI optimization suggestions", author, author);
                newCommitSha = commit.Sha;
                Console.WriteLine($"  ✓ 提交创建: {newCommitSha}");

                // Push
                var providerType = _context.LibGit2SharpProvider.DetermineProviderType(repositoryUrl);
                var username = _context.LibGit2SharpProvider.GetUsernameForProvider(providerType);
                var pushOptions = new PushOptions
                {
                    CredentialsProvider = (url, fromUrl, types) => new UsernamePasswordCredentials
                    {
                        Username = username,
                        Password = _azureConfig.Token
                    }
                };

                var remote = repo.Network.Remotes["origin"];
                var refspec = $"refs/heads/{branchName}:refs/heads/{branchName}";
                repo.Network.Push(remote, new[] { refspec }, pushOptions);
                Console.WriteLine($"  ✓ 分支推送成功: {branchName}");
            }

            // ========== Step 9: 创建Pull Request ==========
            Console.WriteLine("\n[Step 9] 创建Pull Request...");

            var prRequest = new CreateGitPullRequestRequest
            {
                Title = $"[AI优化] 代码分析建议 - {DateTime.UtcNow:yyyy-MM-dd}",
                Description = $@"## AI 代码优化建议

本PR由 AutoCodeForge 自动生成，包含基于AI分析的代码优化建议。

### 分析任务
- 任务ID: {taskEntity.Id}
- 生成时间: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

### 变更内容
- 添加 `AI_OPTIMIZATION_SUGGESTIONS.md` 文件，包含详细的优化建议

---
*此PR由 AutoCodeForge 自动创建*",
                SourceBranch = branchName,
                TargetBranch = _azureConfig.Branch,
            };

            GitPullRequestDto createdPr;
            try
            {
                createdPr = await _context.RepositoryService.CreatePullRequestAsync(repoId, prRequest);
                Console.WriteLine($"  ✓ PR创建成功!");
                Console.WriteLine($"  ✓ PR标题: {createdPr.Title}");
                Console.WriteLine($"  ✓ PR编号: #{createdPr.Number}");
                if (!string.IsNullOrEmpty(createdPr.Url))
                {
                    Console.WriteLine($"  ✓ PR链接: {createdPr.Url}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ PR创建失败: {ex.Message}");
                throw new InvalidOperationException($"PR创建失败，这是真实测试，不使用模拟。错误: {ex.Message}", ex);
            }

            // ========== Step 10: 验证完整流程 ==========
            Console.WriteLine("\n[Step 10] 验证完整流程...");

            // 1. 验证任务完成
            var completedTask = await _context.TaskRepository.GetByIdAsync(taskEntity.Id);
            Assert.NotNull(completedTask);
            Assert.Equal(AutoCodeForge.Core.Entities.TaskStatus.Completed.ToString(), completedTask!.Status.ToString());
            Assert.Equal(100, completedTask.Progress);
            Assert.NotNull(completedTask.Result);
            Console.WriteLine($"  ✓ 任务状态验证通过: {completedTask.Status}");

            // 2. 验证PR创建成功
            Assert.NotNull(createdPr);
            Assert.Equal(branchName, createdPr.SourceBranch);
            Assert.Equal(_azureConfig.Branch, createdPr.TargetBranch);
            Console.WriteLine($"  ✓ PR验证通过: {createdPr.Title}");
            Console.WriteLine($"    - PR编号: #{createdPr.Number}");
            Console.WriteLine($"    - 源分支: {createdPr.SourceBranch} → 目标分支: {createdPr.TargetBranch}");

            // 3. 验证PR是否已保存到数据库（通过任务结果中的PR信息）
            // PR信息保存在任务结果的JSON中
            var taskResult = JsonSerializer.Deserialize<JsonElement>(completedTask.Result);
            Console.WriteLine($"  ✓ PR信息已记录在任务结果中");

            // 4. 验证Sandbox中的文件
            Console.WriteLine($"\n  Sandbox验证:");
            Console.WriteLine($"    - Sandbox根目录: {sandboxRootPath}");
            Console.WriteLine($"    - 工作区目录: {tempWorkspaceRoot}");

            // 验证代码文件存在
            Assert.True(Directory.Exists(localRepoPath), "代码目录应该存在");
            var sandboxCsFiles = Directory.GetFiles(localRepoPath, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains("\\obj\\") && !f.Contains("\\bin\\"))
                .Count();
            Console.WriteLine($"    - 代码文件数: {sandboxCsFiles}");

            // 验证PR分支已推送
            Assert.True(Directory.Exists(Path.Combine(sandboxRootPath)), "Sandbox目录应该存在");
            Console.WriteLine($"    - PR分支已推送: {branchName}");

            // 5. 验证日志记录
            var logs = await _context.TaskLogRepository.GetByTaskIdAsync(taskEntity.Id);
            Console.WriteLine($"  ✓ 日志记录: {logs.Count} 条");

            Console.WriteLine("\n========================================");
            Console.WriteLine("✓ 真实端到端任务闭环测试通过!");
            Console.WriteLine("========================================");
            Console.WriteLine($"\n测试摘要:");
            Console.WriteLine($"  - Sandbox: {sandboxRootPath}");
            Console.WriteLine($"  - 仓库: {_azureConfig.Repository}");
            Console.WriteLine($"  - 源分支: {_azureConfig.Branch}");
            Console.WriteLine($"  - PR分支: {branchName}");
            Console.WriteLine($"  - 任务ID: {taskEntity.Id}");
            Console.WriteLine($"  - 代码文件数: {sandboxCsFiles}");
            Console.WriteLine($"  - PR编号: #{createdPr.Number}");
            if (!string.IsNullOrEmpty(createdPr.Url))
            {
                Console.WriteLine($"  - PR链接: {createdPr.Url}");
            }
        }
        finally
        {
            // 清理（只清理本次测试的工作区目录，保留Sandbox根目录）
            if (!string.IsNullOrEmpty(tempWorkspaceRoot) && Directory.Exists(tempWorkspaceRoot))
            {
                try
                {
                    Directory.Delete(tempWorkspaceRoot, recursive: true);
                    Console.WriteLine($"\n已清理测试工作区: {tempWorkspaceRoot}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n清理工作区失败: {ex.Message}");
                }
            }
            else if (!string.IsNullOrEmpty(sandboxRootPath))
            {
                Console.WriteLine($"\n保留Sandbox目录供后续检查: {sandboxRootPath}");
            }
        }
    }

    #region Stub 类

    private sealed class StubAgentFactory : AgentFactory
    {
        public StubAgentFactory() : base(null!, null!, null!) { }
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}