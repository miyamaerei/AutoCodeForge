---
name: "entity-scaffolder"
description: "生成从零创建实体的完整代码模板，包括后端四层架构（Entity/DTO/Service/Endpoint）和前端模块（api/types/store/test）。Invoke when user asks to create a new entity from scratch or generate entity scaffolding code."
argument-hint: "实体名称（可选，如 YourEntity）"
---

# Entity Scaffolder

从零创建一个完整实体的代码模板生成器，适用于 AutoCodeForge 项目的后端四层架构 + 前端 Vue 3 模块化结构。

## 功能说明

自动生成以下完整代码：

**后端 (.NET 四层)**

| 层级 | 生成内容 |
|-----|---------|
| Core/Entities | 实体类，继承 UserOwnedEntity |
| Core/DTOs | CreateRequest / UpdateRequest / Response |
| Infrastructure/Repositories | Repository 继承 BaseRepository |
| Application/Services | Service 含完整 CRUD |
| Api/Endpoints | MinimalAPI Endpoints |
| Tests | xUnit 单元测试（Sqlite） |

**前端 (Vue 3)**

| 类型 | 生成内容 |
|-----|---------|
| api/*.types.ts | TypeScript 类型定义 |
| api/*.api.ts | Axios API 调用函数 |
| store/useXxxStore.ts | Pinia Store |
| store/__tests__/*.spec.ts | Vitest 单元测试 |
| routes.ts | 路由配置 |
| index.ts | 模块导出 |

## 使用场景

- 用户要求从零创建一个新实体
- 用户询问如何创建实体代码模板
- 用户需要生成 Entity + API + Store + Test 的完整脚手架
- 用户输入类似：`创建一个新的实体`、`new entity template`、`实体脚手架`

## 模板内容

### 1. 后端 Core 层 - 实体

```csharp
// server/src/AutoCodeForge.Core/Entities/YourEntity.cs
using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents your entity.
/// </summary>
[SugarTable("YourTableName")]
public class YourEntity : UserOwnedEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    [SugarColumn(Length = 200, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Description { get; set; }

    public YourStatus Status { get; set; } = YourStatus.Draft;
}

/// <summary>
/// Defines your status.
/// </summary>
public enum YourStatus
{
    Draft = 0,
    Active = 1,
    Disabled = 2,
}
```

### 2. 后端 Core 层 - DTOs

```csharp
// server/src/AutoCodeForge.Core/DTOs/YourModule/CreateYourRequest.cs
using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.YourModule;

public class CreateYourRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }
}

// server/src/AutoCodeForge.Core/DTOs/YourModule/UpdateYourRequest.cs
namespace AutoCodeForge.Core.DTOs.YourModule;

public class UpdateYourRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public YourStatus? Status { get; set; }
}

// server/src/AutoCodeForge.Core/DTOs/YourModule/YourResponse.cs
namespace AutoCodeForge.Core.DTOs.YourModule;

public class YourResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
```

### 3. 后端 Infrastructure 层 - Repository

```csharp
// server/src/AutoCodeForge.Infrastructure/Repositories/YourRepository.cs
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Repository for YourEntity operations.
/// </summary>
public class YourRepository : BaseRepository<YourEntity>
{
    public YourRepository(ISqlSugarClient db, ICurrentUser currentUser) : base(db, currentUser)
    {
    }
}
```

### 4. 后端 Application 层 - Service

```csharp
// server/src/AutoCodeForge.Application/Services/YourService.cs
using AutoCodeForge.Core.DTOs.YourModule;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides your business logic.
/// </summary>
public class YourService
{
    private readonly YourRepository _yourRepository;

    public YourService(YourRepository yourRepository)
    {
        _yourRepository = yourRepository;
    }

    public async Task<YourResponse> CreateAsync(CreateYourRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new YourEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Status = YourStatus.Active,
        };

        var created = await _yourRepository.CreateAsync(entity, cancellationToken);
        return ToResponse(created);
    }

    public async Task<YourResponse> UpdateAsync(Guid id, UpdateYourRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(id, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Name))
            entity.Name = request.Name.Trim();
        if (request.Description is not null)
            entity.Description = request.Description.Trim();
        if (request.Status.HasValue)
            entity.Status = request.Status.Value;

        await _yourRepository.UpdateAsync(entity, cancellationToken);
        return ToResponse(entity);
    }

    public async Task<YourResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(id, cancellationToken);
        return ToResponse(entity);
    }

    public async Task<PagedResult<YourResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _yourRepository.GetPagedAsync(page, pageSize, cancellationToken);
        return new PagedResult<YourResponse>
        {
            Items = result.Items.Select(ToResponse).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
        };
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(id, cancellationToken);
        await _yourRepository.DeleteAsync(entity, cancellationToken);
    }

    private async Task<YourEntity> GetEntityOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _yourRepository.GetByIdAsync(id, cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"YourEntity with id {id} not found");
        return entity;
    }

    private static YourResponse ToResponse(YourEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        Status = entity.Status.ToString(),
        CreatedAtUtc = entity.CreatedAtUtc,
        UpdatedAtUtc = entity.UpdatedAtUtc,
    };
}
```

### 5. 后端 Api 层 - Endpoints

```csharp
// server/src/AutoCodeForge.Api/Endpoints/YourEndpoints.cs
using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.YourModule;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers your API endpoints.
/// </summary>
public static class YourEndpoints
{
    public static IEndpointRouteBuilder MapYourEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/yours");

        group.MapGet("/", async (int page, int pageSize, YourService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetPagedAsync(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, cancellationToken);
            return Results.Ok(ApiResponse<PagedResult<YourResponse>>.Ok(result));
        });

        group.MapGet("/{id:guid}", async (Guid id, YourService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetByIdAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<YourResponse>.Ok(result));
        });

        group.MapPost("/", async (CreateYourRequest request, YourService service, CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.CreateAsync(request, cancellationToken);
            return Results.Ok(ApiResponse<YourResponse>.Ok(result, "Created"));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateYourRequest request, YourService service, CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.UpdateAsync(id, request, cancellationToken);
            return Results.Ok(ApiResponse<YourResponse>.Ok(result, "Updated"));
        });

        group.MapDelete("/{id:guid}", async (Guid id, YourService service, CancellationToken cancellationToken) =>
        {
            await service.DeleteAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<object?>.Ok(null, "Deleted"));
        });

        return app;
    }

    private static void ValidateModel(object request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, context, results, true))
        {
            var message = string.Join("; ", results.Select(r => r.ErrorMessage));
            throw new AutoCodeForge.Core.Exceptions.ValidationException(message);
        }
    }
}
```

### 6. 后端依赖注入注册（必须手动添加）

在 `server/src/AutoCodeForge.Api/Extensions/ServiceCollectionExtensions.cs` 中：

```csharp
// AddRepositories 方法中添加：
services.AddScoped<YourRepository>();

// AddApplicationServices 方法中添加：
services.AddScoped<YourService>();
```

在 `Program.cs` 或 EndpointExtensions 中注册：
```csharp
app.MapYourEndpoints();
```

### 7. 后端单元测试

```csharp
// server/tests/AutoCodeForge.Tests/YourServiceTests.cs
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.YourModule;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using SqlSugar;

namespace AutoCodeForge.Tests;

public sealed class YourServiceTests : IDisposable
{
    private readonly string _dbPath;
    private readonly ISqlSugarClient _db;
    private readonly YourService _yourService;

    public YourServiceTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"autocodeforge.your.{Guid.NewGuid():N}.db");
        _db = new SqlSugarScope(new ConnectionConfig
        {
            ConnectionString = $"Data Source={_dbPath}",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
        });

        _db.CodeFirst.InitTables(typeof(YourEntity));

        var currentUser = new TestCurrentUser("test.user");
        var yourRepository = new YourRepository(_db, currentUser);
        _yourService = new YourService(yourRepository);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedEntity()
    {
        var request = new CreateYourRequest { Name = "Test", Description = "Desc" };
        var created = await _yourService.CreateAsync(request);

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Test", created.Name);
        Assert.Equal("Active", created.Status);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldThrowNotFoundException()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _yourService.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        var created = await _yourService.CreateAsync(new CreateYourRequest { Name = "Original" });
        var updated = await _yourService.UpdateAsync(created.Id, new UpdateYourRequest
        {
            Name = "Updated",
            Status = YourStatus.Disabled,
        });

        Assert.Equal("Updated", updated.Name);
        Assert.Equal("Disabled", updated.Status);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntity()
    {
        var created = await _yourService.CreateAsync(new CreateYourRequest { Name = "ToDelete" });
        await _yourService.DeleteAsync(created.Id);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _yourService.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResult()
    {
        for (int i = 0; i < 5; i++)
            await _yourService.CreateAsync(new CreateYourRequest { Name = $"Entity {i}" });

        var result = await _yourService.GetPagedAsync(1, 3);

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.Items.Count);
    }

    public void Dispose()
    {
        _db?.Dispose();
        if (File.Exists(_dbPath))
            try { File.Delete(_dbPath); } catch { }
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        private readonly string? _ntId;
        public TestCurrentUser(string? ntId) => _ntId = ntId;
        public string? GetCurrentNtId() => _ntId;
        public bool IsAdmin() => false;
    }
}
```

---

### 8. 前端 Types

```typescript
// client/src/modules/your-module/api/your-module.types.ts
export interface YourDto {
  id: string
  name: string
  description?: string
  status: string
  createdAtUtc: string
  updatedAtUtc: string
}

export interface CreateYourRequest {
  name: string
  description?: string
}

export interface UpdateYourRequest {
  name?: string
  description?: string
  status?: 'Draft' | 'Active' | 'Disabled'
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}
```

### 9. 前端 API

```typescript
// client/src/modules/your-module/api/your-module.api.ts
import { request } from '@/lib/request'
import type { ApiResponse, YourDto, CreateYourRequest, UpdateYourRequest, PagedResult } from './your-module.types'

function unwrapApiResponse<T>(payload: ApiResponse<T> | T): T {
  if (payload && typeof payload === 'object' && 'data' in payload) {
    return (payload as ApiResponse<T>).data
  }
  return payload as T
}

export async function fetchYours(page = 1, pageSize = 20): Promise<PagedResult<YourDto>> {
  const { data } = await request.get<ApiResponse<PagedResult<YourDto>>>('/v1/yours', {
    params: { page, pageSize },
  })
  return unwrapApiResponse(data)
}

export async function fetchYour(id: string): Promise<YourDto> {
  const { data } = await request.get<ApiResponse<YourDto>>(`/v1/yours/${id}`)
  return unwrapApiResponse(data)
}

export async function createYour(payload: CreateYourRequest): Promise<YourDto> {
  const { data } = await request.post<ApiResponse<YourDto>>('/v1/yours', payload)
  return unwrapApiResponse(data)
}

export async function updateYour(id: string, payload: UpdateYourRequest): Promise<YourDto> {
  const { data } = await request.put<ApiResponse<YourDto>>(`/v1/yours/${id}`, payload)
  return unwrapApiResponse(data)
}

export async function deleteYour(id: string): Promise<void> {
  await request.delete(`/v1/yours/${id}`)
}
```

### 10. 前端 Store

```typescript
// client/src/modules/your-module/store/useYourStore.ts
import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import {
  fetchYours,
  fetchYour,
  createYour,
  updateYour,
  deleteYour,
  type YourDto,
  type CreateYourRequest,
  type UpdateYourRequest,
} from '../api/your-module.api'

export const useYourStore = defineStore('module.your', () => {
  const yours = ref<YourDto[]>([])
  const selectedYour = ref<YourDto | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalCount = ref(0)
  const currentPage = ref(1)

  const hasYours = computed(() => yours.value.length > 0)

  async function loadYours(page = 1): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const result = await fetchYours(page)
      yours.value = result.items || []
      totalCount.value = result.totalCount || 0
      currentPage.value = result.page || page
    } catch (e: any) {
      error.value = e.message || 'Failed to load yours'
    } finally {
      loading.value = false
    }
  }

  async function loadYourDetail(id: string): Promise<void> {
    loading.value = true
    error.value = null
    try {
      selectedYour.value = await fetchYour(id)
    } catch (e: any) {
      error.value = e.message || 'Failed to load detail'
    } finally {
      loading.value = false
    }
  }

  async function create(payload: CreateYourRequest): Promise<YourDto> {
    const created = await createYour(payload)
    yours.value.unshift(created)
    return created
  }

  async function update(id: string, payload: UpdateYourRequest): Promise<YourDto> {
    const updated = await updateYour(id, payload)
    const idx = yours.value.findIndex(y => y.id === id)
    if (idx >= 0) yours.value[idx] = updated
    if (selectedYour.value?.id === id) selectedYour.value = updated
    return updated
  }

  async function remove(id: string): Promise<void> {
    await deleteYour(id)
    yours.value = yours.value.filter(y => y.id !== id)
    if (selectedYour.value?.id === id) selectedYour.value = null
  }

  return {
    yours,
    selectedYour,
    loading,
    error,
    totalCount,
    currentPage,
    hasYours,
    loadYours,
    loadYourDetail,
    create,
    update,
    remove,
  }
})
```

### 11. 前端 Store 测试

```typescript
// client/src/modules/your-module/store/__tests__/useYourStore.spec.ts
/**
 * useYourStore 单元测试
 */
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useYourStore } from '../useYourStore'
import * as yourApi from '../../api/your-module.api'
import type { YourDto, CreateYourRequest, UpdateYourRequest } from '../../api/your-module.types'

describe('useYourStore', () => {
  const mockYours: YourDto[] = [
    { id: 'y1', name: '实体 A', description: '描述 A', status: 'Active', createdAtUtc: '2026-05-01T10:00:00Z', updatedAtUtc: '2026-05-01T10:00:00Z' },
    { id: 'y2', name: '实体 B', description: '描述 B', status: 'Draft', createdAtUtc: '2026-05-02T10:00:00Z', updatedAtUtc: '2026-05-02T10:00:00Z' },
  ]

  let fetchYoursSpy: ReturnType<typeof vi.spyOn>
  let fetchYourSpy: ReturnType<typeof vi.spyOn>
  let createYourSpy: ReturnType<typeof vi.spyOn>
  let updateYourSpy: ReturnType<typeof vi.spyOn>
  let deleteYourSpy: ReturnType<typeof vi.spyOn>

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    fetchYoursSpy = vi.spyOn(yourApi, 'fetchYours')
    fetchYourSpy = vi.spyOn(yourApi, 'fetchYour')
    createYourSpy = vi.spyOn(yourApi, 'createYour')
    updateYourSpy = vi.spyOn(yourApi, 'updateYour')
    deleteYourSpy = vi.spyOn(yourApi, 'deleteYour')
  })

  it('should have correct initial state', () => {
    const store = useYourStore()
    expect(store.yours).toEqual([])
    expect(store.selectedYour).toBeNull()
    expect(store.loading).toBe(false)
    expect(store.hasYours).toBe(false)
  })

  it('should load yours successfully', async () => {
    fetchYoursSpy.mockResolvedValue({ items: mockYours, totalCount: 2, page: 1, pageSize: 20 })
    const store = useYourStore()
    await store.loadYours(1)
    expect(store.yours).toEqual(mockYours)
    expect(store.hasYours).toBe(true)
  })

  it('should handle load error', async () => {
    fetchYoursSpy.mockRejectedValue(new Error('Network error'))
    const store = useYourStore()
    await store.loadYours(1)
    expect(store.error).toBe('Network error')
  })

  it('should load your detail', async () => {
    fetchYourSpy.mockResolvedValue(mockYours[0])
    const store = useYourStore()
    await store.loadYourDetail('y1')
    expect(store.selectedYour).toEqual(mockYours[0])
  })

  it('should create new your', async () => {
    const newYour: YourDto = { id: 'y3', name: '新实体', description: '新描述', status: 'Active', createdAtUtc: '2026-05-22T10:00:00Z', updatedAtUtc: '2026-05-22T10:00:00Z' }
    createYourSpy.mockResolvedValue(newYour)
    const store = useYourStore()
    store.yours = [...mockYours]
    const result = await store.create({ name: '新实体', description: '新描述' })
    expect(store.yours[0]).toEqual(newYour)
  })

  it('should update existing your', async () => {
    const updated: YourDto = { ...mockYours[0], name: '更新名称' }
    updateYourSpy.mockResolvedValue(updated)
    const store = useYourStore()
    store.yours = [...mockYours]
    await store.update('y1', { name: '更新名称' })
    expect(store.yours[0].name).toBe('更新名称')
  })

  it('should remove your', async () => {
    deleteYourSpy.mockResolvedValue()
    const store = useYourStore()
    store.yours = [...mockYours]
    store.selectedYour = mockYours[0]
    await store.remove('y1')
    expect(store.yours.length).toBe(1)
    expect(store.selectedYour).toBeNull()
  })
})
```

### 12. 前端路由

```typescript
// client/src/modules/your-module/routes.ts
import type { RouteRecordRaw } from 'vue-router'

export const yourRoutes: RouteRecordRaw[] = [
  {
    path: '/your-module',
    name: 'your.list',
    component: () => import('./views/YourListView.vue'),
    meta: { requiresAuth: true, title: 'Your Module' },
  },
  {
    path: '/your-module/:id',
    name: 'your.detail',
    component: () => import('./views/YourDetailView.vue'),
    meta: { requiresAuth: true, title: 'Your Detail' },
  },
]
```

### 13. 前端模块入口

```typescript
// client/src/modules/your-module/index.ts
export { yourRoutes } from './routes'
export {
  fetchYours,
  fetchYour,
  createYour,
  updateYour,
  deleteYour,
  type YourDto,
  type CreateYourRequest,
  type UpdateYourRequest,
} from './api/your-module.api'
export { useYourStore } from './store/useYourStore'
```

---

## 关联实体模板

当两个实体存在外键关联时（如主实体 ParentEntity 与从实体 ChildEntity），需要创建完整的关联代码。以下以 **Task（任务）** 和 **TaskLog（任务日志）** 为例进行说明。

### 关联场景说明

| 实体 | 角色 | 外键 | 说明 |
|------|------|------|------|
| TaskEntity | 主实体 | - | 任务本身 |
| TaskLogEntity | 从实体 | TaskId | 任务的执行日志 |

### 1. Core 层 - 主实体

```csharp
// server/src/AutoCodeForge.Core/Entities/TaskEntity.cs
using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents a user task.
/// </summary>
[SugarTable("Tasks")]
public class TaskEntity : UserOwnedEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    [SugarColumn(Length = 200, IsNullable = false)]
    public string Title { get; set; } = string.Empty;

    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Description { get; set; }

    [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
    public string Input { get; set; } = string.Empty;

    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? Result { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Pending;

    public int Progress { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? StartedAtUtc { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? CompletedAtUtc { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? DueAtUtc { get; set; }

    [SugarColumn(ColumnDataType = "TEXT", IsNullable = true)]
    public string? ErrorMessage { get; set; }

    [SugarColumn(IsNullable = true)]
    public Guid? AgentId { get; set; }
}

public enum TaskStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Paused = 4,
    Canceled = 5,
}
```

### 2. Core 层 - 从实体

```csharp
// server/src/AutoCodeForge.Core/Entities/TaskLogEntity.cs
using AutoCodeForge.Core.Entities.Base;
using SqlSugar;

namespace AutoCodeForge.Core.Entities;

/// <summary>
/// Represents runtime logs for a task.
/// </summary>
[SugarTable("TaskLogs")]
public class TaskLogEntity : UserOwnedEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public new Guid Id { get; set; }

    /// <summary>
    /// 外键：指向主实体的 Id
    /// </summary>
    public Guid TaskId { get; set; }

    [SugarColumn(Length = 32, IsNullable = false)]
    public string Level { get; set; } = "Info";

    [SugarColumn(ColumnDataType = "TEXT", IsNullable = false)]
    public string Message { get; set; } = string.Empty;

    [SugarColumn(Length = 128, IsNullable = true)]
    public string? Source { get; set; }
}
```

### 3. Core 层 - DTOs

```csharp
// server/src/AutoCodeForge.Core/DTOs/Task/CreateTaskRequest.cs
using System.ComponentModel.DataAnnotations;

namespace AutoCodeForge.Core.DTOs.Task;

public class CreateTaskRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public string Input { get; set; } = string.Empty;

    public DateTime? DueAtUtc { get; set; }

    public Guid? AgentId { get; set; }
}

// server/src/AutoCodeForge.Core/DTOs/Task/UpdateTaskRequest.cs
namespace AutoCodeForge.Core.DTOs.Task;

public class UpdateTaskRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Input { get; set; }
    public DateTime? DueAtUtc { get; set; }
    public Guid? AgentId { get; set; }
}

// server/src/AutoCodeForge.Core/DTOs/Task/TaskResponse.cs
namespace AutoCodeForge.Core.DTOs.Task;

public class TaskResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string Input { get; set; } = string.Empty;
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? DueAtUtc { get; set; }
    public Guid? AgentId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

// server/src/AutoCodeForge.Core/DTOs/Task/TaskLogResponse.cs
namespace AutoCodeForge.Core.DTOs.Task;

public class TaskLogResponse
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Source { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
```

### 4. Infrastructure 层 - Repositories

```csharp
// server/src/AutoCodeForge.Infrastructure/Repositories/TaskRepository.cs
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Repository for TaskEntity operations.
/// </summary>
public class TaskRepository : BaseRepository<TaskEntity>
{
    public TaskRepository(ISqlSugarClient db, ICurrentUser currentUser) : base(db, currentUser)
    {
    }

    /// <summary>
    /// 获取待处理的任务
    /// </summary>
    public async Task<List<TaskEntity>> GetPendingAsync(int take, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(t => t.Status == Core.Entities.TaskStatus.Pending)
            .OrderBy(t => t.CreatedAtUtc)
            .Take(take)
            .ToListAsync();
    }
}

// server/src/AutoCodeForge.Infrastructure/Repositories/TaskLogRepository.cs
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Infrastructure.Repositories.Base;
using SqlSugar;

namespace AutoCodeForge.Infrastructure.Repositories;

/// <summary>
/// Repository for TaskLogEntity operations.
/// </summary>
public class TaskLogRepository : BaseRepository<TaskLogEntity>
{
    public TaskLogRepository(ISqlSugarClient db, ICurrentUser currentUser) : base(db, currentUser)
    {
    }

    /// <summary>
    /// 根据主实体 Id 获取所有关联的从实体
    /// </summary>
    public async Task<List<TaskLogEntity>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        return await Queryable
            .Where(log => log.TaskId == taskId)
            .OrderBy(log => log.CreatedAtUtc)
            .ToListAsync();
    }
}
```

### 5. Application 层 - Service

```csharp
// server/src/AutoCodeForge.Application/Services/TaskService.cs
using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Exceptions;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Provides task CRUD and related operations.
/// </summary>
public class TaskService
{
    private readonly TaskRepository _taskRepository;
    private readonly TaskLogRepository _taskLogRepository;

    public TaskService(TaskRepository taskRepository, TaskLogRepository taskLogRepository)
    {
        _taskRepository = taskRepository;
        _taskLogRepository = taskLogRepository;
    }

    /// <summary>
    /// 创建任务并添加初始化日志
    /// </summary>
    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Input = request.Input.Trim(),
            AgentId = request.AgentId,
            DueAtUtc = request.DueAtUtc,
            Status = Core.Entities.TaskStatus.Pending,
            Progress = 0,
        };

        var created = await _taskRepository.CreateAsync(entity, cancellationToken);
        await AddLogAsync(created.Id, "Info", "Task created", nameof(TaskService), cancellationToken);
        return ToResponse(created);
    }

    public async Task<TaskResponse> UpdateAsync(Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(taskId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Title))
            entity.Title = request.Title.Trim();
        if (request.Description is not null)
            entity.Description = request.Description.Trim();
        if (request.Input is not null)
            entity.Input = request.Input.Trim();

        entity.AgentId = request.AgentId;
        entity.DueAtUtc = request.DueAtUtc;

        await _taskRepository.UpdateAsync(entity, cancellationToken);
        await AddLogAsync(taskId, "Info", "Task updated", nameof(TaskService), cancellationToken);
        return ToResponse(entity);
    }

    public async Task<TaskResponse> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var entity = await GetEntityOrThrowAsync(taskId, cancellationToken);
        return ToResponse(entity);
    }

    public async Task<PagedResult<TaskResponse>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _taskRepository.GetPagedAsync(page, pageSize, cancellationToken: cancellationToken);
        return new PagedResult<TaskResponse>
        {
            Items = result.Items.Select(ToResponse).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
        };
    }

    /// <summary>
    /// 获取任务关联的日志列表
    /// </summary>
    public async Task<List<TaskLogResponse>> GetLogsAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        _ = await GetEntityOrThrowAsync(taskId, cancellationToken);
        var logs = await _taskLogRepository.GetByTaskIdAsync(taskId, cancellationToken);
        return logs.Select(ToLogResponse).ToList();
    }

    public async Task DeleteAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        await _taskRepository.SoftDeleteAsync(taskId, cancellationToken: cancellationToken);
        await AddLogAsync(taskId, "Info", "Task deleted", nameof(TaskService), cancellationToken);
    }

    /// <summary>
    /// 添加任务日志（内部方法）
    /// </summary>
    internal async Task AddLogAsync(Guid taskId, string level, string message, string source, CancellationToken cancellationToken = default)
    {
        var log = new TaskLogEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Level = string.IsNullOrWhiteSpace(level) ? "Info" : level,
            Message = message,
            Source = source,
        };

        await _taskLogRepository.CreateAsync(log, cancellationToken);
    }

    private async Task<TaskEntity> GetEntityOrThrowAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var entity = await _taskRepository.GetByIdAsync(taskId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException($"Task with id {taskId} not found");
        return entity;
    }

    private static TaskResponse ToResponse(TaskEntity entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Description = entity.Description,
        Status = entity.Status.ToString(),
        Progress = entity.Progress,
        Input = entity.Input,
        Result = entity.Result,
        ErrorMessage = entity.ErrorMessage,
        StartedAtUtc = entity.StartedAtUtc,
        CompletedAtUtc = entity.CompletedAtUtc,
        DueAtUtc = entity.DueAtUtc,
        AgentId = entity.AgentId,
        CreatedAtUtc = entity.CreatedAtUtc,
        UpdatedAtUtc = entity.UpdatedAtUtc,
    };

    private static TaskLogResponse ToLogResponse(TaskLogEntity entity) => new()
    {
        Id = entity.Id,
        TaskId = entity.TaskId,
        Level = entity.Level,
        Message = entity.Message,
        Source = entity.Source,
        CreatedAtUtc = entity.CreatedAtUtc,
    };
}
```

### 6. Api 层 - Endpoints

```csharp
// server/src/AutoCodeForge.Api/Endpoints/TaskEndpoints.cs
using System.ComponentModel.DataAnnotations;
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Registers task API endpoints.
/// </summary>
public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/tasks");

        group.MapGet("/", async (int page, int pageSize, TaskService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetPagedAsync(page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, cancellationToken);
            return Results.Ok(ApiResponse<PagedResult<TaskResponse>>.Ok(result));
        });

        group.MapGet("/{id:guid}", async (Guid id, TaskService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetByIdAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<TaskResponse>.Ok(result));
        });

        // 🔗 关联端点：获取任务的所有日志
        group.MapGet("/{id:guid}/logs", async (Guid id, TaskService service, CancellationToken cancellationToken) =>
        {
            var result = await service.GetLogsAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<List<TaskLogResponse>>.Ok(result));
        });

        group.MapPost("/", async (CreateTaskRequest request, TaskService service, CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.CreateAsync(request, cancellationToken);
            return Results.Ok(ApiResponse<TaskResponse>.Ok(result, "Task created"));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateTaskRequest request, TaskService service, CancellationToken cancellationToken) =>
        {
            ValidateModel(request);
            var result = await service.UpdateAsync(id, request, cancellationToken);
            return Results.Ok(ApiResponse<TaskResponse>.Ok(result, "Task updated"));
        });

        group.MapDelete("/{id:guid}", async (Guid id, TaskService service, CancellationToken cancellationToken) =>
        {
            await service.DeleteAsync(id, cancellationToken);
            return Results.Ok(ApiResponse<object?>.Ok(null, "Task deleted"));
        });

        return app;
    }

    private static void ValidateModel(object request)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, context, results, true))
        {
            var message = string.Join("; ", results.Select(r => r.ErrorMessage));
            throw new AutoCodeForge.Core.Exceptions.ValidationException(message);
        }
    }
}
```

### 7. 依赖注入注册

在 `server/src/AutoCodeForge.Api/Extensions/ServiceCollectionExtensions.cs` 中：

```csharp
// AddRepositories 方法中添加：
services.AddScoped<TaskRepository>();
services.AddScoped<TaskLogRepository>();

// AddApplicationServices 方法中添加：
services.AddScoped<TaskService>();
```

在 `Program.cs` 中注册端点：
```csharp
app.MapTaskEndpoints();
```

### 8. 数据库初始化

在 `server/src/AutoCodeForge.Infrastructure/Data/DatabaseInitializer.cs` 的 `InitializeAsync` 方法中：

```csharp
_db.CodeFirst.InitTables(
    typeof(TaskEntity),
    typeof(TaskLogEntity),
    // ... 其他已注册的实体
);
```

### 9. 前端 Types

```typescript
// client/src/modules/task/api/task.types.ts
export interface TaskDto {
  id: string
  title: string
  description?: string
  status: string
  progress: number
  input: string
  result?: string
  errorMessage?: string
  startedAtUtc?: string
  completedAtUtc?: string
  dueAtUtc?: string
  agentId?: string
  createdAtUtc: string
  updatedAtUtc: string
}

export interface TaskLogDto {
  id: string
  taskId: string
  level: string
  message: string
  source?: string
  createdAtUtc: string
}

export interface CreateTaskRequest {
  title: string
  description?: string
  input: string
  dueAtUtc?: string
  agentId?: string
}

export interface UpdateTaskRequest {
  title?: string
  description?: string
  input?: string
  dueAtUtc?: string
  agentId?: string
}
```

### 10. 前端 API

```typescript
// client/src/modules/task/api/task.api.ts
import { request } from '@/lib/request'
import type { ApiResponse, TaskDto, TaskLogDto, CreateTaskRequest, UpdateTaskRequest, PagedResult } from './task.types'

function unwrapApiResponse<T>(payload: ApiResponse<T> | T): T {
  if (payload && typeof payload === 'object' && 'data' in payload) {
    return (payload as ApiResponse<T>).data
  }
  return payload as T
}

export async function fetchTasks(page = 1, pageSize = 20): Promise<PagedResult<TaskDto>> {
  const { data } = await request.get<ApiResponse<PagedResult<TaskDto>>>('/v1/tasks', {
    params: { page, pageSize },
  })
  return unwrapApiResponse(data)
}

export async function fetchTask(id: string): Promise<TaskDto> {
  const { data } = await request.get<ApiResponse<TaskDto>>(`/v1/tasks/${id}`)
  return unwrapApiResponse(data)
}

// 🔗 关联接口：获取任务关联的日志
export async function fetchTaskLogs(taskId: string): Promise<TaskLogDto[]> {
  const { data } = await request.get<ApiResponse<TaskLogDto[]>>(`/v1/tasks/${taskId}/logs`)
  return unwrapApiResponse(data)
}

export async function createTask(payload: CreateTaskRequest): Promise<TaskDto> {
  const { data } = await request.post<ApiResponse<TaskDto>>('/v1/tasks', payload)
  return unwrapApiResponse(data)
}

export async function updateTask(id: string, payload: UpdateTaskRequest): Promise<TaskDto> {
  const { data } = await request.put<ApiResponse<TaskDto>>(`/v1/tasks/${id}`, payload)
  return unwrapApiResponse(data)
}

export async function deleteTask(id: string): Promise<void> {
  await request.delete(`/v1/tasks/${id}`)
}
```

### 11. 前端 Store

```typescript
// client/src/modules/task/store/useTaskStore.ts
import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import {
  fetchTasks,
  fetchTask,
  fetchTaskLogs,
  createTask,
  updateTask,
  deleteTask,
  type TaskDto,
  type TaskLogDto,
  type CreateTaskRequest,
  type UpdateTaskRequest,
} from '../api/task.api'

export const useTaskStore = defineStore('module.task', () => {
  const tasks = ref<TaskDto[]>([])
  const selectedTask = ref<TaskDto | null>(null)
  const taskLogs = ref<TaskLogDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalCount = ref(0)
  const currentPage = ref(1)

  const hasTasks = computed(() => tasks.value.length > 0)

  async function loadTasks(page = 1): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const result = await fetchTasks(page)
      tasks.value = result.items || []
      totalCount.value = result.totalCount || 0
      currentPage.value = result.page || page
    } catch (e: any) {
      error.value = e.message || 'Failed to load tasks'
    } finally {
      loading.value = false
    }
  }

  async function loadTaskDetail(id: string): Promise<void> {
    loading.value = true
    error.value = null
    try {
      selectedTask.value = await fetchTask(id)
    } catch (e: any) {
      error.value = e.message || 'Failed to load task detail'
    } finally {
      loading.value = false
    }
  }

  // 🔗 关联方法：加载任务关联的日志
  async function loadTaskLogs(taskId: string): Promise<void> {
    loading.value = true
    error.value = null
    try {
      taskLogs.value = await fetchTaskLogs(taskId)
    } catch (e: any) {
      error.value = e.message || 'Failed to load task logs'
    } finally {
      loading.value = false
    }
  }

  async function create(payload: CreateTaskRequest): Promise<TaskDto> {
    const created = await createTask(payload)
    tasks.value.unshift(created)
    return created
  }

  async function update(id: string, payload: UpdateTaskRequest): Promise<TaskDto> {
    const updated = await updateTask(id, payload)
    const idx = tasks.value.findIndex(t => t.id === id)
    if (idx >= 0) tasks.value[idx] = updated
    if (selectedTask.value?.id === id) selectedTask.value = updated
    return updated
  }

  async function remove(id: string): Promise<void> {
    await deleteTask(id)
    tasks.value = tasks.value.filter(t => t.id !== id)
    if (selectedTask.value?.id === id) selectedTask.value = null
  }

  return {
    tasks,
    selectedTask,
    taskLogs,
    loading,
    error,
    totalCount,
    currentPage,
    hasTasks,
    loadTasks,
    loadTaskDetail,
    loadTaskLogs,
    create,
    update,
    remove,
  }
})
```

---

## 关联实体 vs 独立实体对比

| 维度 | 独立实体 | 关联实体 |
|------|---------|---------|
| Entity | 1 个 | 2 个（主 + 从） |
| Repository | 1 个 | 2 个（从实体含外键查询方法） |
| Service | 注入 1 个 Repository | 注入 2 个 Repository |
| DTOs | 3 个（Create/Update/Response） | 6 个（各实体 3 个） |
| Endpoints | 标准 CRUD | 标准 CRUD + 子资源端点（如 `/tasks/{id}/logs`） |
| 前端 API | 5 个函数 | 6+ 个函数（含关联接口） |
| 前端 Store | 基础 CRUD 方法 | 基础 CRUD + 关联方法（如 `loadTaskLogs`） |

---

## 生成的文件清单

```
后端 (server/src)
├── AutoCodeForge.Core/
│   ├── Entities/
│   │   └── YourEntity.cs              ✅ 含枚举
│   └── DTOs/YourModule/
│       ├── CreateYourRequest.cs        ✅
│       ├── UpdateYourRequest.cs        ✅
│       └── YourResponse.cs             ✅
├── AutoCodeForge.Infrastructure/
│   └── Repositories/
│       └── YourRepository.cs           ✅
├── AutoCodeForge.Application/
│   └── Services/
│       └── YourService.cs             ✅
├── AutoCodeForge.Api/
│   └── Endpoints/
│       └── YourEndpoints.cs           ✅
└── tests/AutoCodeForge.Tests/
    └── YourServiceTests.cs            ✅

前端 (client/src/modules/your-module)
├── api/
│   ├── your-module.types.ts           ✅
│   └── your-module.api.ts             ✅
├── store/
│   ├── useYourStore.ts                 ✅
│   └── __tests__/
│       └── useYourStore.spec.ts        ✅
├── views/
│   ├── YourListView.vue                (需手动创建)
│   └── YourDetailView.vue              (需手动创建)
├── routes.ts                           ✅
└── index.ts                            ✅
```

## 使用方法

1. 确认实体名称（如 `YourEntity`）和业务模块名称（如 `YourModule`）
2. 将 `YourEntity` / `YourModule` / `Your` 替换为实际名称
3. 根据业务需求添加/删除实体字段
4. 在 `ServiceCollectionExtensions.cs` 中添加 Repository 和 Service 的注入注册
5. 在 `Program.cs` 中调用 `app.MapYourEndpoints()`
6. 创建 Vue 页面组件（views 目录下的 .vue 文件需手动创建）
