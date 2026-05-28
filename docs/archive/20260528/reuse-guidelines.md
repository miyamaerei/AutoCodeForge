# AutoCodeForge: 复用与工程化指南

目标：把需求里提到的必须复用功能体系化、并给出实现建议与示例代码（面向 ASP.NET Core Web API + SqlSugar）。文档面向后端开发人员，用于快速搭建项目复用层与工程骨架。

---

## 目录

1. 最强复用：实体基类（BaseEntity）
2. 自动赋值复用策略（新增/修改自动填充 UserId/时间）
3. 最强复用：通用仓储基类（IBaseRepository / BaseRepository）
4. 必须全局统一复用的 10 大核心功能
5. 分层职责（Core / Entity / Repository / Service / Api）
6. 不可复用的模块（独立拆分建议）
7. 推荐文件/目录结构
8. 快速上手：实现 & 验证步骤

---

## 1. 最强复用：实体基类（BaseEntity）
说明：所有业务实体继承 `BaseEntity`，只在业务实体中声明特有字段，保证统一字段与 SqlSugar Attribute 统一。

示例：

```csharp
using SqlSugar;

public class BaseEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }

    // 对应需求中的用户归属字段（NtId / UserId），项目中统一使用 UserId
    public Guid UserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public class TaskEntity : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}
```

设计要点：
- 使用 Guid 主键，便于分布式场景。
- `UserId` 用于数据隔离（也可以在实体上保留 `NtId` 字段，依据团队命名习惯）。
- `IsDeleted` 用于软删除。
- `CreatedAt/UpdatedAt` 由框架自动赋值。

---

## 2. 自动赋值复用策略
目标：新增/修改时自动填充 `CreatedAt/UpdatedAt/UserId`，由 SqlSugar 拦截或在 Repository 层统一设置，避免每个实体重复实现。

实现建议：
- 在 `BaseRepository.AddAsync()` / `UpdateAsync()` 里统一赋值。
- 或使用 SqlSugar 的审计特性或 `Aop` 回调（OnExecuting）自动填充。

示例（Repository 内统一赋值）：

```csharp
private void FillOnCreate(BaseEntity entity, Guid currentUserId)
{
    entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
    entity.UserId = currentUserId;
    entity.CreatedAt = DateTime.UtcNow;
    entity.UpdatedAt = DateTime.UtcNow;
}

private void FillOnUpdate(BaseEntity entity)
{
    entity.UpdatedAt = DateTime.UtcNow;
}
```

权限注入：在 Service 层或 Repository 层接收 `ICurrentUser`（封装 HttpContext 中的 UserId）并传入填充方法。

---

## 3. 最强复用：通用仓储基类（IBaseRepository / BaseRepository）
目标：把 CRUD、分页、软删除、批量操作等常用操作封装在通用仓储里，所有仓储继承以消除重复代码。

接口示例：

```csharp
public interface IBaseRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetListAsync(Expression<Func<T, bool>>? predicate = null);
    Task<(List<T> Items, long Total)> GetPageListAsync(int page, int pageSize, Expression<Func<T, bool>>? predicate = null);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task SoftDeleteAsync(Guid id);
    Task BatchAddAsync(IEnumerable<T> entities);
}
```

实现要点：
- `GetListAsync`/`GetPageListAsync` 应默认加上 `IsDeleted == false` 和 `UserId == currentUserId` 过滤。
- 使用泛型和 Expression 参数保证灵活性。
- 所有 DB 操作通过注入的 `ISqlSugarClient` 执行。

---

## 4. 必须全局统一复用的 10 大核心功能（少一个都不行）
下面是每个功能的实现建议与注意点。

1) 统一 API 返回结果（ApiResponse<T>)
- 单一响应结构：Code/Message/Data/RequestId/Timestamp
- 在 Controller 层或过滤器中统一封装返回。

2) 全局 JWT 登录用户获取
- 提供 `ICurrentUser` 服务，封装解析 Token 的逻辑并提供 `GetCurrentUserId()`。
- 在中间件里验证 Token 并把用户信息写入 `HttpContext.User`。

3) 全局分页工具复用
- 提供 `PageRequest`/`PageResult<T>` 类型和扩展方法 `ApplyPaging()`。

4) 全局统一异常处理中间件
- 中间件捕获所有异常，输出 `ApiErrorResponse`，并记录 RequestId 与异常堆栈（仅 Dev 环境显示详细）。

5) 密码加密工具复用
- 提供 `IPasswordHasher`（使用 PBKDF2/Argon2），统一加盐与验证。

6) JSON 序列化 / 反序列化统一封装
- 统一使用 `System.Text.Json`（或 Newtonsoft）配置（DateTime 格式、忽略空值、CamelCase），提供 `IJsonSerializer` 包装。

7) SqlSugar 全局软删除过滤器
- 在 SqlSugar 配置里注册全局查询过滤器：`IsDeleted == false`。
- 所有查库方法默认开启该过滤器。

8) 环境配置读取复用
- 使用 `IOptions<T>` + `IConfiguration`，并封装 `IAppSettings` 提供常用配置访问（JWT/AI Keys/Git Config）。

9) SSE 流式消息推送复用
- 抽象 `ISseStreamWriter`，封装事件发送（start/chunk/tool/end/error），并提供 Controller helper。

10) 定时任务通用执行模板
- 提供 `ScheduledTaskBase : BackgroundService`，子类只实现 `ExecuteTaskAsync()`。

---

## 5. 分层职责严格划分（杜绝代码混乱）

- Core：基类、工具、接口定义（无业务）
- Entity：实体（只包含数据结构与 ORM attribute）
- Repository：数据库访问（只做 CRUD/事务）
- Service：业务逻辑（调用仓储、整合能力）
- Api：Controller 层，仅做参数接收、调用 Service、返回统一结果

实践建议：每层单元测试覆盖、禁止跨层直接调用下层以外的实现。

---

## 6. 哪些绝对不能复用（必须独立拆分）
- AI Agent 会话逻辑 → 单独 AI 服务（AgentExecutor / AgentTool）
- Git 平台对接（GitHub/GitLab/AzureDevOps）→ 单独 Git 客户端库
- 定时任务业务逻辑 → 每个定时任务独立 HostedService（或继承 ScheduledTaskBase）
- 第三方接口请求 → 独立 HttpClient 封装并注入到调用方

---

## 7. 推荐文件/目录结构
```
server/src/
├── Core/
│   ├── BaseEntity.cs
│   ├── Interfaces/
│   │   ├── IBaseRepository.cs
│   │   └── ICurrentUser.cs
│   ├── Utils/
│   └── DTOs/
├── Entities/
├── Repositories/
│   ├── BaseRepository.cs
│   └── TaskRepository.cs
├── Services/
├── Api/
│   └── Controllers/
├── AI/
├── Examples/
└── Program.cs
```

---

## 8. 快速上手：实现 & 验证步骤
1. 创建 `BaseEntity` 与 `IBaseRepository`，并实现 `BaseRepository`。
2. 在 `Program.cs` 注入 `ISqlSugarClient` 与 `IBaseRepository<>` 的 DI 映射。
3. 实现 `ICurrentUser` 从 HttpContext 读取 UserId 并在 Repository 的 Add/Update 中使用。
4. 实现全局异常处理中间件与统一 `ApiResponse<T>`。
5. 实现 SqlSugar 全局软删除过滤器并运行 CodeFirst InitTables。

验证示例：
- POST /api/v1/tasks 创建 Task（不需要填写 Id/CreateAt/UpdatedAt/UserId），返回创建成功且数据库记录有自动填充字段。
- GET /api/v1/tasks 列表接口默认只返回当前 `UserId` 的未删除记录。

---

如果你同意这个结构，我可以：
- 在 `server/src/Core` 和 `server/src/Repositories` 中生成基础代码示例（BaseEntity、IBaseRepository、BaseRepository、ICurrentUser、统一 ApiResponse、异常中间件）。
- 或者只生成 `README.md` 样板，把设计决议写在 repo 根目录以供团队讨论。

请选择要继续的下一步（例如：生成代码样例/只生成 README/生成自动化 checklist）。
