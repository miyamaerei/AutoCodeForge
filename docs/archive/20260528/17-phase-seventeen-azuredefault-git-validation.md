# 阶段十七：AzureDefault 配置与 Git/PR 验证

## 我是如何考虑的

### 设计思路
本阶段目标是补齐一个可执行、可追踪的后端配置与验证闭环：
1. 在后端开发配置中维护 `azureDefault` 的可用真实值，确保 Azure DevOps 拉取链路具备可运行基础。
2. 通过 test 目录中的单元测试验证 Git Provider、仓库拉取（clone/pull）和 PR 创建/查询行为，避免仅靠手工验证。
3. 在文档层固化验证入口与结果，减少后续重复沟通成本。

### 复用设计
- 复用阶段六的 Git 仓库集成能力（Provider 抽象、RepositoryService、Git 工具链）。
- 复用阶段十的测试基础设施（MSTest/xUnit 混合测试工程）。
- 新增最小范围测试补充，不引入新的运行依赖。

---

## 本阶段复用的功能清单（来自其他阶段）

| 复用组件 | 文件路径 | 复用方式 | 避免重复代码 |
|---------|---------|---------|-----------|
| Git Provider 工厂 | `server/src/AutoCodeForge.Infrastructure/Git/GitProviderFactory.cs` | 统一创建 GitHub/GitLab/AzureDevOps Provider | 避免重复实例化逻辑 |
| 仓库服务层 | `server/src/AutoCodeForge.Application/Services/RepositoryService.cs` | 统一分支、提交、PR 操作入口 | 避免重复鉴权和 token 解密 |
| Repo 同步处理器 | `server/src/AutoCodeForge.Infrastructure/BackgroundServices/Handlers/RepoSyncTaskHandler.cs` | 复用拉取任务执行链路 | 避免重复任务执行与状态更新 |
| 测试工程框架 | `server/tests/AutoCodeForge.Tests/AutoCodeForge.Tests.csproj` | 复用现有单测框架 | 避免新建测试宿主 |

---

## 本阶段新增的可复用功能清单

| 复用组件 | 文件路径 | 说明 | 被复用次数 |
|---------|---------|------|----------|
| Azure DevOps PR 解析健壮性增强 | `server/src/AutoCodeForge.Infrastructure/Git/AzureDevOpsProvider.cs` | 兼容 `pullRequestId` 数值类型与可选 `closedDate` 字段 | 2+（创建 PR 与查询 PR） |
| 本地 clone/pull 回归测试 | `server/tests/AutoCodeForge.Tests/LibGit2SharpProviderTests.cs` | 基于本地 bare repo 验证拉取链路 | 1+ |
| Azure DevOps PR Provider 测试 | `server/tests/AutoCodeForge.Tests/GitProviderTests.cs` | 验证 PR 查询与创建行为 | 2+ |

---

## 任务清单

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| 17-01 | 写入可用 azureDefault 配置 | `server/src/AutoCodeForge.Api/appsettings.Development.json` | 可用 Azure DevOps 配置节 | 阶段六 | 否 | 无 | JSON 校验通过 |
| 17-02 | 修复 Azure DevOps PR 解析兼容性 | `server/src/AutoCodeForge.Infrastructure/Git/AzureDevOpsProvider.cs` | PR 创建/查询鲁棒性增强 | 阶段六 | 是 | 17-01 | 单元测试通过 |
| 17-03 | 增加 clone/pull 单测 | `server/tests/AutoCodeForge.Tests/LibGit2SharpProviderTests.cs` | 拉取链路回归测试 | 阶段十 | 是 | 无 | 用例通过 |
| 17-04 | 增加 PR 查询/创建单测 | `server/tests/AutoCodeForge.Tests/GitProviderTests.cs` | PR 行为测试 | 阶段十 | 是 | 17-02 | 用例通过 |
| 17-05 | 计划文档同步 | `docs/plans/17-phase-seventeen-azuredefault-git-validation.md` | 阶段计划文档 | 阶段十一 | 否 | 17-01~17-04 | 文档索引可追踪 |

---

## 注意事项

1. `Token` 属于敏感信息，建议后续迁移到安全密钥存储或环境变量，不在生产配置中明文保存。
2. `RepoSyncTaskHandler` 实际拉取使用的是数据库中仓库实体保存的 token；`azureDefault` 主要用于默认配置和联调起步。
3. 若执行真实远端 PR 创建，需确保 source 分支已存在且具备目标仓库写权限。

---

## 阶段完成总结

### 复用收益
通过复用既有 Git 抽象和测试基座，本阶段以最小增量实现配置落地与验证闭环，避免重复搭建测试环境和重复实现 Provider 行为校验。

### 下一步
建议进入阶段十五与阶段十六联动：将 `azureDefault` 的敏感字段统一迁移到受控配置来源，并通过配置治理模块提供审计与轮换能力。
