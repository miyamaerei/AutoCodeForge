# 阶段六：Git 仓库集成

**日期**: 2026-05-20  
**预估时间**: 2-3 天  
**优先级**: 🟡 P1 - 重要功能  
**前置依赖**: 阶段一、二、三

---

## 我是如何考虑的

### 设计思路

Git 集成的核心是**多平台抽象**：

1. **统一接口 IGitProvider** - 定义所有 Git 操作
2. **多平台实现** - GitHubProvider/GitLabProvider/AzureDevOpsProvider
3. **工厂模式** - 根据仓库配置选择对应的 Provider
4. **凭据安全存储** - Token 加密存储在数据库
5. **Agent 工具集成** - Git 操作作为 Agent 可调用工具

### 复用设计

本阶段**大量复用阶段一、二、三的基础设施**，避免重复代码：

| 复用的组件 | 复用自 | 本阶段复用方式 |
|---------|-------|--------------|
| UserOwnedEntity | 阶段一 | RepositoryEntity 继承基类 |
| BaseRepository<T> | 阶段一 | RepositoryRepository 继承 |
| ApiResponse<T> | 阶段一 | Repository Endpoints 统一响应格式 |
| ExceptionHandlingMiddleware | 阶段一 | 全局异常处理自动生效 |
| PaginationHelper | 阶段一 | 仓库列表分页查询 |
| UserRepository | 阶段二 | 获取当前用户信息 |
| JwtAuthMiddleware | 阶段二 | 认证保护所有 Git 相关端点 |
| JsonHelper | 阶段一 | Git API 响应和请求的 JSON 序列化 |
| TimeHelper | 阶段一 | 仓库时间戳处理 |
| IAgentTool | 阶段三 | Git 工具实现 IAgentTool 接口 |

### 本阶段新增的可复用功能

| 复用组件 | 被哪些阶段复用 |
|---------|--------------|
| IGitProvider | 阶段六、七 |
| GitProviderFactory | 阶段六、七 |
| GitHubProvider | 阶段六、七 |
| GitLabProvider | 阶段六、七 |
| AzureDevOpsProvider | 阶段六、七 |
| RepositoryRepository | 阶段六、七 |
| RepositoryService | 阶段六、七 |
| GitTools | 阶段六、七 |

### 为什么这样设计？

- **扩展性** - 新增 Git 平台只需实现 IGitProvider
- **复用性** - Agent 工具和 RepositoryService 都使用同一套接口

---

## 本阶段复用的功能清单（来自其他阶段）

| 复用组件 | 文件路径 | 复用方式 | 避免重复代码 |
|---------|---------|---------|-----------|
| UserOwnedEntity | `Core/Entities/Base/UserOwnedEntity.cs` | RepositoryEntity 继承 | 3 个属性定义 |
| BaseRepository<T> | `Infrastructure/Repositories/Base/BaseRepository.cs` | RepositoryRepository 继承 | 50+ 行 CRUD/软删除/分页代码 |
| ApiResponse<T> | `Core/Models/ApiResponse.cs` | Repository Endpoints 使用 | 避免 5 个端点重复写响应格式化 |
| ExceptionHandlingMiddleware | `Api/Middleware/ExceptionHandlingMiddleware.cs` | 全局自动生效 | 避免所有端点重复写异常处理 |
| PaginationHelper | `Infrastructure/Helpers/PaginationHelper.cs` | 仓库列表分页 | 避免列表分页重复代码 |
| JsonHelper | `Infrastructure/Helpers/JsonHelper.cs` | Git API 响应和请求 JSON 序列化 | 避免 JSON 处理重复代码 |
| TimeHelper | `Infrastructure/Helpers/TimeHelper.cs` | 仓库时间戳处理 | 避免时间处理重复代码 |
| UserRepository | `Infrastructure/Repositories/UserRepository.cs` | 获取当前用户 | 避免用户查询重复代码 |
| JwtAuthMiddleware | `Api/Middleware/JwtAuthMiddleware.cs` | 认证保护 | 避免认证重复代码 |
| IAgentTool | `Core/Interfaces/IAgentTool.cs` | Git 工具实现 IAgentTool 接口 | 避免接口定义重复代码 |

---

## 本阶段新增的可复用功能清单

| 复用组件 | 文件路径 | 说明 | 被复用次数（预估） |
|---------|---------|------|------------------|
| IGitProvider | `Core/Interfaces/IGitProvider.cs` | Git 提供程序接口 | 2 次 |
| GitProviderFactory | `Infrastructure/Git/GitProviderFactory.cs` | Git 提供程序工厂 | 2 次 |
| GitHubProvider | `Infrastructure/Git/GitHubProvider.cs` | GitHub 提供程序 | 2 次 |
| GitLabProvider | `Infrastructure/Git/GitLabProvider.cs` | GitLab 提供程序 | 2 次 |
| AzureDevOpsProvider | `Infrastructure/Git/AzureDevOpsProvider.cs` | Azure DevOps 提供程序 | 2 次 |
| RepositoryRepository | `Infrastructure/Repositories/RepositoryRepository.cs` | 仓库仓储 | 2 次 |
| RepositoryService | `Application/Services/RepositoryService.cs` | 仓库服务 | 2 次 |
| GitTools | `Infrastructure/AI/Tools/GitTools.cs` | Git 工具 | 2 次 |

---

## 任务清单

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| **6.1** | 创建 Git DTO | `server/src/AutoCodeForge.Core/DTOs/Git/` | GitBranch/GitPullRequest/GitCommit 等模型 | - | ❌ - | 阶段一 | 代码编译 |
| **6.2** | 创建 IGitProvider 接口 | `server/src/AutoCodeForge.Core/Interfaces/IGitProvider.cs` | Git 操作统一接口 | - | ✅ 是 | 6.1 | 代码编译 |
| **6.3** | 创建 GitHubProvider | `server/src/AutoCodeForge.Infrastructure/Git/GitHubProvider.cs` | GitHub API 实现 | - | ✅ 是 | 6.2 | 代码编译 |
| **6.4** | 创建 GitLabProvider | `server/src/AutoCodeForge.Infrastructure/Git/GitLabProvider.cs` | GitLab API 实现 | GitHubProvider (参照) | ✅ 是 | 6.2, 6.3 | 代码编译 |
| **6.5** | 创建 AzureDevOpsProvider | `server/src/AutoCodeForge.Infrastructure/Git/AzureDevOpsProvider.cs` | Azure DevOps API 实现 | GitHubProvider (参照) | ✅ 是 | 6.2-6.4 | 代码编译 |
| **6.6** | 创建 GitProviderFactory | `server/src/AutoCodeForge.Infrastructure/Git/GitProviderFactory.cs` | Provider 工厂类 | - | ✅ 是 | 6.3-6.5 | 代码编译 |
| **6.7** | 创建 Repository DTO | `server/src/AutoCodeForge.Core/DTOs/Repository/` | CreateRepositoryRequest 等 DTO | - | ❌ - | 阶段一 | 代码编译 |
| **6.8** | 创建 RepositoryRepository | `server/src/AutoCodeForge.Infrastructure/Repositories/RepositoryRepository.cs` | RepositoryRepository 类 | 阶段一 (BaseRepository) | ✅ 是 | 阶段二 | 代码编译 |
| **6.9** | 创建 RepositoryService | `server/src/AutoCodeForge.Application/Services/RepositoryService.cs` | RepositoryService 类（CRUD+Git操作） | - | ✅ 是 | 6.6, 6.8 | 代码编译 |
| **6.10** | 创建 Repository Endpoints | `server/src/AutoCodeForge.Api/Endpoints/RepositoryEndpoints.cs` | /api/v1/repositories 相关端点 | ApiResponse | ❌ - | 6.7, 6.9 | 代码编译 |
| **6.11** | 创建 Git Agent 工具 | `server/src/AutoCodeForge.Infrastructure/AI/Tools/GitTools.cs` | GitTools 类（实现 IAgentTool） | IAgentTool (阶段三) | ✅ 是 | 6.9, 阶段三 | 代码编译 |
| **6.12** | 注册 Git 相关服务 | `server/src/AutoCodeForge.Api/Program.cs` | 注册 RepositoryService/GitProviderFactory/GitTools | - | ❌ - | 6.9-6.11 | 代码编译 |
| **6.13** | 验证 Git 集成功能 | - | 可以添加仓库、查看分支、创建 PR | - | ❌ - | 6.1-6.12 | 测试各个 Git 平台操作 |

---

## 注意事项

⚠️ **重要提醒**

1. **凭据安全** - Token 需要加密存储，不要明文存储
2. **API 限流** - 各个 Git 平台都有 API 限流，需要处理
3. **网络超时** - Git API 调用需要超时和重试机制
4. **权限验证** - Token 权限验证，确保只有授权操作
5. **错误处理** - Git API 错误要友好地转化为用户可理解的错误

✅ **验收标准**

- 可以添加和管理 Git 仓库
- 可以查看仓库分支列表
- 可以创建 Pull Request
- 可以查看提交历史
- Git 工具可以被 Agent 调用

---

## 阶段完成总结

### 复用收益

本阶段通过复用阶段一、二、三的基础设施，预计可以**避免 250+ 行重复代码**：
- 1 个实体无需重复写 CreatedAt/UpdatedAt/NtId/IsDeleted
- 1 个 Repository 无需重复写 CRUD/软删除/分页
- 5 个 API 端点无需重复写响应格式化
- 实现 IAgentTool 接口避免了接口定义重复

### 本阶段新增复用

本阶段创建了 8 个可复用组件，将被后续阶段（七）使用。

### 下一步

完成本阶段后，进入 **阶段七：流水线模块**，详见 `07-phase-seven-pipeline.md`
