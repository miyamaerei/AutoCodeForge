# 阶段八：Wiki 模块

**日期**: 2026-05-20  
**预估时间**: 1 天  
**优先级**: 🟢 P2 - 一般功能  
**前置依赖**: 阶段一、二、六

---

## 我是如何考虑的

### 设计思路

Wiki 模块是**知识库管理**，相对简单独立：

1. **继承基类** - WikiPageEntity 继承 UserOwnedEntity
2. **CRUD 基础功能** - 页面增删改查
3. **可选的全文检索** - MVP 阶段可以先用简单的 LIKE 查询
4. **关联仓库** - Wiki 页面可以关联到 Git 仓库

### 复用设计

本阶段**大量复用阶段一、二、六的基础设施**，避免重复代码：

| 复用的组件 | 复用自 | 本阶段复用方式 |
|---------|-------|--------------|
| UserOwnedEntity | 阶段一 | WikiPageEntity 继承基类 |
| BaseRepository<T> | 阶段一 | WikiPageRepository 继承 |
| ApiResponse<T> | 阶段一 | Wiki Endpoints 统一响应格式 |
| ExceptionHandlingMiddleware | 阶段一 | 全局异常处理自动生效 |
| PaginationHelper | 阶段一 | Wiki 页面列表分页查询 |
| UserRepository | 阶段二 | 获取当前用户信息 |
| JwtAuthMiddleware | 阶段二 | 认证保护所有 Wiki 相关端点 |
| TimeHelper | 阶段一 | Wiki 时间戳处理 |
| RepositoryRepository | 阶段六 | 复用仓库仓储 |

### 本阶段新增的可复用功能

| 复用组件 | 被哪些阶段复用 |
|---------|--------------|
| WikiPageRepository | 阶段八 |
| WikiService | 阶段八 |

---

## 本阶段复用的功能清单（来自其他阶段）

| 复用组件 | 文件路径 | 复用方式 | 避免重复代码 |
|---------|---------|---------|-----------|
| UserOwnedEntity | `Core/Entities/Base/UserOwnedEntity.cs` | WikiPageEntity 继承 | 3 个属性定义 |
| BaseRepository<T> | `Infrastructure/Repositories/Base/BaseRepository.cs` | WikiPageRepository 继承 | 50+ 行 CRUD/软删除/分页代码 |
| ApiResponse<T> | `Core/Models/ApiResponse.cs` | Wiki Endpoints 使用 | 避免 5 个端点重复写响应格式化 |
| ExceptionHandlingMiddleware | `Api/Middleware/ExceptionHandlingMiddleware.cs` | 全局自动生效 | 避免所有端点重复写异常处理 |
| PaginationHelper | `Infrastructure/Helpers/PaginationHelper.cs` | Wiki 页面列表分页 | 避免列表分页重复代码 |
| TimeHelper | `Infrastructure/Helpers/TimeHelper.cs` | Wiki 时间戳处理 | 避免时间处理重复代码 |
| UserRepository | `Infrastructure/Repositories/UserRepository.cs` | 获取当前用户 | 避免用户查询重复代码 |
| JwtAuthMiddleware | `Api/Middleware/JwtAuthMiddleware.cs` | 认证保护 | 避免认证重复代码 |
| RepositoryRepository | `Infrastructure/Repositories/RepositoryRepository.cs` | 复用仓库仓储 | 避免仓库查询重复代码 |

---

## 本阶段新增的可复用功能清单

| 复用组件 | 文件路径 | 说明 | 被复用次数（预估） |
|---------|---------|------|------------------|
| WikiPageRepository | `Infrastructure/Repositories/WikiPageRepository.cs` | Wiki 页面仓储 | 1 次 |
| WikiService | `Application/Services/WikiService.cs` | Wiki 服务 | 1 次 |

---

## 任务清单

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| **8.1** | 创建 Wiki DTO | `server/src/AutoCodeForge.Core/DTOs/Wiki/` | CreateWikiPageRequest 等 DTO | - | ❌ - | 阶段一 | 代码编译 |
| **8.2** | 创建 WikiPageRepository | `server/src/AutoCodeForge.Infrastructure/Repositories/WikiPageRepository.cs` | WikiPageRepository 类 | 阶段一 (BaseRepository) | ✅ 是 | 阶段二 | 代码编译 |
| **8.3** | 创建 WikiService | `server/src/AutoCodeForge.Application/Services/WikiService.cs` | WikiService 类（CRUD+搜索） | - | ✅ 是 | 8.2 | 代码编译 |
| **8.4** | 创建 Wiki Endpoints | `server/src/AutoCodeForge.Api/Endpoints/WikiEndpoints.cs` | /api/v1/wiki 相关端点 | ApiResponse | ❌ - | 8.1, 8.3 | 代码编译 |
| **8.5** | 注册 Wiki 相关服务 | `server/src/AutoCodeForge.Api/Program.cs` | 注册 WikiService | - | ❌ - | 8.3 | 代码编译 |
| **8.6** | 验证 Wiki 功能 | - | 可以创建/编辑/删除 Wiki 页面，搜索功能正常 | - | ❌ - | 8.1-8.5 | 测试 Wiki 完整功能 |

---

## 注意事项

⚠️ **重要提醒**

1. **搜索性能** - LIKE 查询性能一般，后期可以考虑全文检索引擎
2. **版本历史** - MVP 阶段不需要版本历史，后期可以考虑
3. **Markdown 渲染** - 前端负责渲染，后端只存储原始 Markdown
4. **权限控制** - Wiki 页面可以关联到仓库，需要考虑仓库权限
5. **图片附件** - MVP 阶段可以不支持，后期考虑

✅ **验收标准**

- 可以创建 Wiki 页面
- 可以编辑 Wiki 页面
- 可以删除 Wiki 页面
- 可以搜索 Wiki 页面
- 可以关联 Wiki 页面到仓库

---

## 阶段完成总结

### 复用收益

本阶段通过复用阶段一、二、六的基础设施，预计可以**避免 200+ 行重复代码**：
- 1 个实体无需重复写 CreatedAt/UpdatedAt/NtId/IsDeleted
- 1 个 Repository 无需重复写 CRUD/软删除/分页
- 5 个 API 端点无需重复写响应格式化

### 本阶段新增复用

本阶段创建了 2 个可复用组件。

### 下一步

完成本阶段后，进入 **阶段九：系统配置 & 健康检查**，详见 `09-phase-nine-config-health.md`
