# 阶段十：测试 & 优化

**日期**: 2026-05-20  
**预估时间**: 2-3 天  
**优先级**: 🟢 P2 - 完善优化  
**前置依赖**: 所有前面的阶段

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

## 🎉 完成

恭喜！所有阶段已完成，AutoCodeForge 后端 MVP 已准备就绪！

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
