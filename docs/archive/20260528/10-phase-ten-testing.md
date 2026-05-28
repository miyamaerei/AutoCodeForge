# 阶段十：测试 & 优化

**日期**: 2026-05-20  
**预估时间**: 2-3 天  
**优先级**: 🟢 P2 - 完善优化  
**前置依赖**: 所有前面的阶段

---

## 当前状态快照（2026-05-20）

### 已验证现状

- `server/tests/AutoCodeForge.Tests/AutoCodeForge.Tests.csproj` 已存在，测试工程可编译。
- 已落地测试 48 个：`48` 通过，`0` 失败，`0` 跳过。
- 已补齐共享测试基础设施：`TestBase.cs`、`TestDataFactory.cs`、`TestWebApplicationFactory.cs`。
- 已修复的阻断项：
	1. `RepositoryServiceTests` 改为使用真实 `GitProviderFactory` + fake `HttpMessageHandler`，不再 mock 不可覆写成员。
	2. `GitProviderTests` 改为使用可控 fake `HttpClient`，不再依赖真实外网行为。
	3. `AgentChatSmokeTests` 已解除 skip，并切换到隔离 SQLite + 移除后台服务的测试宿主。
	4. `AgentEntity.LlmModelConfigId` 与 `ChatSessionEntity` 可空字段映射已修正，消除集成测试中的 SQLite `NOT NULL` 约束错误。
- 已新增轻量性能基线测试：`server/tests/AutoCodeForge.Tests/Performance/LlmGatewayPerformanceTests.cs`。
- 已补齐后端文档：`server/docs/API.md`、`server/docs/DEPLOYMENT.md`，并同步更新 `server/README.md`。

### 阶段判断

第 10 阶段的回归阻断已经清除，`10.14` 最终验证已通过；本轮又补齐了 `BaseRepositoryTests`、`LlmGatewayTests`、`AgentServiceTests` 和正式 code review 记录，因此原始计划中剩余的核心缺口已经收口。

---

## 我是如何考虑的

### 设计思路

测试和优化是**确保质量的最后一步**：

1. **单元测试** - 核心 Service 和 Helper 的测试
2. **集成测试** - API 端点的测试
3. **代码 Review** - 整体代码质量检查
4. **性能优化** - 关键路径性能优化
5. **文档完善** - API 文档和部署文档

### 复用设计

本阶段**大量复用前面阶段开发的所有组件**进行测试：

| 复用的组件 | 复用自 | 本阶段复用方式 |
|---------|-------|--------------|
| 所有实体类 | 阶段一、二、六、七、八、九 | 测试数据创建 |
| 所有 Repository | 阶段一、二、六、七、八、九 | Repository 测试 |
| 所有 Service | 阶段二、三、四、六、七、八、九 | Service 测试 |
| ApiResponse<T> | 阶段一 | API 响应验证 |
| ExceptionHandlingMiddleware | 阶段一 | 异常处理测试 |
| JwtAuthMiddleware | 阶段二 | 认证测试 |

### 测试策略

| 测试类型 | 覆盖范围 | 工具 |
|---------|---------|------|
| 单元测试 | Services/Helpers/Repositories | xUnit + Moq |
| 集成测试 | API Endpoints | xUnit + WebApplicationFactory |

---

## 本阶段复用的功能清单（来自其他阶段）

| 复用组件 | 文件路径 | 复用方式 | 避免重复代码 |
|---------|---------|---------|-----------|
| 所有实体类 | `Core/Entities/` | 测试数据创建 | 避免测试数据重复定义 |
| 所有 Repository | `Infrastructure/Repositories/` | Repository 测试 | 直接测试实际 Repository |
| 所有 Service | `Application/Services/` | Service 测试 | 直接测试实际 Service |
| ApiResponse<T> | `Core/Models/ApiResponse.cs` | API 响应验证 | 避免响应验证重复代码 |
| ExceptionHandlingMiddleware | `Api/Middleware/ExceptionHandlingMiddleware.cs` | 异常处理测试 | 直接测试实际中间件 |
| JwtAuthMiddleware | `Api/Middleware/JwtAuthMiddleware.cs` | 认证测试 | 直接测试实际中间件 |

---

## 本阶段新增的可复用功能清单

| 复用组件 | 文件路径 | 说明 | 被复用次数（预估） |
|---------|---------|------|------------------|
| TestBase | `tests/AutoCodeForge.Tests/TestBase.cs` | 测试基类 | N 次（所有测试） |
| TestDataFactory | `tests/AutoCodeForge.Tests/TestDataFactory.cs` | 测试数据工厂 | N 次（所有测试） |

---

## 任务清单

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| **10.1** | 创建测试项目 | `server/tests/AutoCodeForge.Tests/AutoCodeForge.Tests.csproj` | 测试项目文件 | - | ❌ - | 所有阶段 | 测试项目可以编译 |
| **10.2** | 创建测试基类和数据工厂 | `server/tests/AutoCodeForge.Tests/TestBase.cs` `server/tests/AutoCodeForge.Tests/TestDataFactory.cs` | 测试基类和数据工厂 | - | ✅ 是 | 10.1 | 测试基类可用 |
| **10.3** | 编写 BaseRepository 单元测试 | `server/tests/AutoCodeForge.Tests/Repositories/BaseRepositoryTests.cs` | BaseRepository 测试 | - | ❌ - | 10.2 | 测试全部通过 |
| **10.4** | 编写 AuthService 单元测试 | `server/tests/AutoCodeForge.Tests/Services/AuthServiceTests.cs` | AuthService 测试 | - | ❌ - | 10.2 | 测试全部通过 |
| **10.5** | 编写 LlmGateway 单元测试 | `server/tests/AutoCodeForge.Tests/AI/LlmGatewayTests.cs` | LlmGateway 测试（Mock LLM） | - | ❌ - | 10.2 | 测试全部通过 |
| **10.6** | 编写 AgentService 单元测试 | `server/tests/AutoCodeForge.Tests/Services/AgentServiceTests.cs` | AgentService 测试 | - | ❌ - | 10.2 | 测试全部通过 |
| **10.7** | 编写 TaskService 单元测试 | `server/tests/AutoCodeForge.Tests/Services/TaskServiceTests.cs` | TaskService 测试 | - | ❌ - | 10.2 | 测试全部通过 |
| **10.8** | 编写 Auth Endpoints 集成测试 | `server/tests/AutoCodeForge.Tests/Endpoints/AuthEndpointsTests.cs` | Auth API 集成测试 | - | ❌ - | 10.2 | 测试全部通过 |
| **10.9** | 编写 Chat Endpoints 集成测试 | `server/tests/AutoCodeForge.Tests/Endpoints/ChatEndpointsTests.cs` | Chat API 集成测试 | - | ❌ - | 10.2 | 测试全部通过 |
| **10.10** | 代码质量 Review | `server/src/` | 代码 Review 记录 | - | ❌ - | 所有阶段 | 代码符合规范，没有明显 Bug |
| **10.11** | 性能基准测试 | `server/tests/AutoCodeForge.Tests/Performance/` | 性能测试报告 | - | ❌ - | 10.2 | 关键路径性能达标 |
| **10.12** | 完善 API 文档 | `server/README.md` `server/docs/API.md` | 完整的 API 文档 | - | ❌ - | 所有阶段 | 文档清晰完整 |
| **10.13** | 编写部署文档 | `server/docs/DEPLOYMENT.md` | 部署指南 | - | ❌ - | 所有阶段 | 文档清晰可操作 |
| **10.14** | 最终验证测试 | - | 完整 MVP 功能验证 | - | ❌ - | 所有阶段 | 所有核心功能正常工作 |

---

## 任务状态对照（2026-05-20）

| 编号 | 当前状态 | 证据/备注 |
|------|---------|-----------|
| **10.1** | ✅ 已完成 | 测试项目 `server/tests/AutoCodeForge.Tests/AutoCodeForge.Tests.csproj` 已存在，`dotnet test` 可完成发现与执行。 |
| **10.2** | ✅ 已完成 | 已新增 `TestBase.cs`、`TestDataFactory.cs`、`TestWebApplicationFactory.cs`，集成测试宿主支持独立 SQLite 和关闭后台服务。 |
| **10.3** | ✅ 已完成 | 已新增 `server/tests/AutoCodeForge.Tests/BaseRepositoryTests.cs`，覆盖审计字段、用户隔离和软删除行为。 |
| **10.4** | ✅ 已完成 | `AuthServiceTests.cs` 已存在，且纳入本次 `dotnet test` 通过集合。 |
| **10.5** | ✅ 已完成 | 已新增 `server/tests/AutoCodeForge.Tests/LlmGatewayTests.cs`，覆盖请求校验、模型选择、工具结果拼接和错误拼接。 |
| **10.6** | ✅ 已完成 | 已新增 `server/tests/AutoCodeForge.Tests/AgentServiceTests.cs`，覆盖创建、未命中更新和关键词匹配逻辑。 |
| **10.7** | ✅ 已完成 | `TaskServiceTests.cs` 已存在，基础任务状态流转已覆盖。 |
| **10.8** | ✅ 已完成 | `AuthEndpointsTests.cs` 已存在，当前 smoke 也已在主计划中登记通过。 |
| **10.9** | ✅ 已完成 | 仍未使用 `ChatEndpointsTests.cs` 命名，但 `AgentChatSmokeTests.cs` 已解除 skip 并覆盖聊天与流式集成路径。 |
| **10.10** | ✅ 已完成 | 已归档 code review 记录 `docs/reports/CODE_REVIEW_20260520_PHASE10.md`，当前未发现新的阻断级问题。 |
| **10.11** | ✅ 已完成 | 已新增 `server/tests/AutoCodeForge.Tests/Performance/LlmGatewayPerformanceTests.cs`，验证 mock LLM 关键路径 < 500ms。 |
| **10.12** | ✅ 已完成 | 已新增 `server/docs/API.md` 并同步更新 `server/README.md`。 |
| **10.13** | ✅ 已完成 | 已新增 `server/docs/DEPLOYMENT.md`。 |
| **10.14** | ✅ 已完成 | 已执行完整测试验证，结果为 `48` 总数中 `48` 通过、`0` 失败、`0` 跳过。 |

---

## 注意事项

⚠️ **重要提醒**

1. **测试数据隔离** - 使用单独的测试数据库，不要影响开发数据
2. **Mock 外部依赖** - LLM 调用、Git API 等外部依赖使用 Mock
3. **测试覆盖度** - 核心 Service 争取 80%+ 覆盖度
4. **性能基准** - 建立性能基准，方便后续优化对比

✅ **验收标准**

- 所有单元测试通过
- 所有集成测试通过
- 代码 Review 通过
- 性能测试达标
- 文档完整清晰
- MVP 所有核心功能正常工作

---

## 当前结论

阶段十的关键阻断项和原始计划缺口都已收口，当前测试基线、性能基线、后端文档和 review 记录都已可用，`10.14` 最终验证已跑通。按当前计划口径，本阶段可以视为完成。

### 当前优先收口项

1. 后续若继续增强阶段十，可再补更细的故障注入测试，例如 `LlmGateway` 的熔断与重试边界。
2. 若进入长期维护阶段，建议把 SQLite 并发边界验证从测试宿主扩展到接近生产的环境验证。

### 复用收益总览

通过整个 10 个阶段的复用设计，预计可以**避免 3000+ 行重复代码**：
- 16+ 实体无需重复写 CreatedAt/UpdatedAt/NtId/IsDeleted
- 12+ Repository 无需重复写 CRUD/软删除/分页
- 50+ API 端点无需重复写响应格式化和异常处理
- 大量复用服务和中间件

### 后续迭代方向（可选）

1. SignalR 实时通知
2. Redis 缓存
3. 接口限流
4. 多租户改造
5. 升级到 PostgreSQL
6. 容器化部署 (Docker + Kubernetes)
