# Elsa Workflow 实施问答与最佳实践

**版本**: v1.0.0  
**作者**: AutoCodeForge Team  
**创建日期**: 2026-05-26  
**最后更新**: 2026-05-26

---

## 目录

1. [问题一：双数据库查询方案](#问题一双数据库查询方案)
2. [问题二：Elsa 代码文件结构组织](#问题二elsa-代码文件结构组织)
3. [问题三：最小 Demo 验证方案](#问题三最小-demo-验证方案)
4. [问题四：Vue 3 前端最佳实践](#问题四vue-3-前端最佳实践)
5. [附录：完整 Demo 代码示例](#附录完整-demo-代码示例)

---

## 问题一：双数据库查询方案

### 1.1 问题概述

当前系统使用 **SqlSugar** 操作业务数据库（SQLite），Elsa Workflow 使用 **EF Core** 操作独立数据库。如何实现跨库查询和数据关联？

### 1.2 解决方案架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                    应用服务层 (Application Layer)                │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │              ElsaWorkflowService (桥接服务)               │ │
│  │  ┌─────────────────────────────────────────────────────┐ │ │
│  │  │ 1. 查询业务数据 (SqlSugar)                          │ │ │
│  │  │ 2. 查询 Elsa 数据 (EF Core)                          │ │ │
│  │  │ 3. 数据组装 & 返回                                  │ │ │
│  │  └─────────────────────────────────────────────────────┘ │ │
│  └───────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
              ┌──────────────────────┴──────────────────────┐
              ↓                                             ↓
┌─────────────────────────────────┐      ┌─────────────────────────────────┐
│   业务数据库 (SqlSugar)          │      │  Elsa 数据库 (EF Core)          │
│  - TaskEntity                   │      │  - WorkflowDefinition          │
│  - TaskStepEntity               │      │  - WorkflowInstance            │
│  - HumanGateEntity              │      │  - Bookmark                    │
│  - ...                          │      │  - ExecutionLog                │
└─────────────────────────────────┘      └─────────────────────────────────┘
```

### 1.3 核心实现方案

#### 方案 A：应用服务层桥接（推荐）

**核心思想**：在 Application 层创建桥接服务，分别查询两个数据库，然后在内存中组装数据。

**优点**：
- 简单直观，无需复杂的分布式事务
- 符合现有架构模式
- 易于测试和维护

**实现代码**：

```csharp
// AutoCodeForge.Application/Services/ElsaWorkflowService.cs
using Elsa.Workflows.Management.Contracts;
using Elsa.Workflows.Management.Entities;
using Elsa.Workflows.Runtime.Contracts;
using Elsa.Workflows.Runtime.Entities;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Elsa 工作流桥接服务 - 处理双数据库查询
/// </summary>
public class ElsaWorkflowService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
    private readonly IWorkflowInstanceStore _workflowInstanceStore;

    public ElsaWorkflowService(
        ITaskRepository taskRepository,
        IWorkflowDefinitionStore workflowDefinitionStore,
        IWorkflowInstanceStore workflowInstanceStore)
    {
        _taskRepository = taskRepository;
        _workflowDefinitionStore = workflowDefinitionStore;
        _workflowInstanceStore = workflowInstanceStore;
    }

    /// <summary>
    /// 获取任务及其关联的工作流实例（双数据库联合查询）
    /// </summary>
    public async Task<TaskWithWorkflowDto?> GetTaskWithWorkflowAsync(Guid taskId)
    {
        // 1. 查询业务数据库（SqlSugar）
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            return null;

        // 2. 如果有工作流实例 ID，查询 Elsa 数据库（EF Core）
        WorkflowInstance? workflowInstance = null;
        if (!string.IsNullOrEmpty(task.WorkflowInstanceId))
        {
            workflowInstance = await _workflowInstanceStore.FindAsync(task.WorkflowInstanceId);
        }

        // 3. 组装数据
        return new TaskWithWorkflowDto
        {
            Task = task,
            WorkflowInstance = workflowInstance,
            // 可选：添加更多关联信息
            WorkflowStatus = workflowInstance?.Status.ToString() ?? "NotStarted"
        };
    }

    /// <summary>
    /// 获取任务列表及其工作流状态（批量查询优化）
    /// </summary>
    public async Task<PagedResult<TaskWithWorkflowDto>> GetTaskListWithWorkflowAsync(
        int page, int pageSize)
    {
        // 1. 先查询业务数据库
        var taskPage = await _taskRepository.GetPagedAsync(page, pageSize);

        // 2. 收集所有 WorkflowInstanceId
        var workflowInstanceIds = taskPage.Items
            .Where(t => !string.IsNullOrEmpty(t.WorkflowInstanceId))
            .Select(t => t.WorkflowInstanceId!)
            .ToList();

        // 3. 批量查询 Elsa 数据库（减少查询次数）
        Dictionary<string, WorkflowInstance> workflowInstanceDict = new();
        if (workflowInstanceIds.Count > 0)
        {
            var workflowInstances = await _workflowInstanceStore.FindManyAsync(
                workflowInstanceIds);
            workflowInstanceDict = workflowInstances
                .ToDictionary(wi => wi.Id);
        }

        // 4. 组装结果
        var items = taskPage.Items.Select(task => new TaskWithWorkflowDto
        {
            Task = task,
            WorkflowInstance = task.WorkflowInstanceId != null &&
                workflowInstanceDict.ContainsKey(task.WorkflowInstanceId)
                ? workflowInstanceDict[task.WorkflowInstanceId]
                : null,
            WorkflowStatus = task.WorkflowInstanceId != null &&
                workflowInstanceDict.ContainsKey(task.WorkflowInstanceId)
                ? workflowInstanceDict[task.WorkflowInstanceId].Status.ToString()
                : "NotStarted"
        }).ToList();

        return new PagedResult<TaskWithWorkflowDto>
        {
            Items = items,
            TotalCount = taskPage.TotalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// 通过工作流实例查询关联的业务任务
    /// </summary>
    public async Task<TaskEntity?> GetTaskByWorkflowInstanceIdAsync(string workflowInstanceId)
    {
        // 这里假设 TaskEntity 有 WorkflowInstanceId 字段
        // SqlSugar 查询
        return await _taskRepository.GetFirstAsync(t =>
            t.WorkflowInstanceId == workflowInstanceId);
    }
}

/// <summary>
/// 任务与工作流关联 DTO
/// </summary>
public class TaskWithWorkflowDto
{
    public TaskEntity Task { get; set; } = null!;
    public WorkflowInstance? WorkflowInstance { get; set; }
    public string WorkflowStatus { get; set; } = string.Empty;
}
```

#### 方案 B：EF Core + SqlSugar 双重注册（更灵活）

在 Program.cs 中同时配置两个数据访问框架：

```csharp
// AutoCodeForge.Api/Program.cs
using Elsa.Extensions;
using Elsa.Persistence.EntityFramework.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// 1. 现有 SqlSugar 配置（保持不变）
builder.Services.AddSqlSugar(builder.Configuration);

// 2. 新增 Elsa EF Core 配置
var elsaConnectionString = builder.Configuration.GetConnectionString("ElsaConnection")
    ?? "Data Source=elsa-workflows.db";

builder.Services.AddElsa(elsa =>
{
    elsa.UseWorkflowManagement(management =>
    {
        management.UseEntityFrameworkCore(ef =>
        {
            ef.UseSqlite(elsaConnectionString);
            ef.RunMigrations = true; // 开发环境自动迁移
        });
    });

    elsa.UseWorkflowRuntime(runtime =>
    {
        runtime.UseEntityFrameworkCore(ef =>
        {
            ef.UseSqlite(elsaConnectionString);
            ef.RunMigrations = true;
        });
    });

    elsa.UseWorkflowsApi();
});

// 注册桥接服务
builder.Services.AddScoped<ElsaWorkflowService>();

var app = builder.Build();
// ... 现有配置
app.UseWorkflowsApi(); // 启用 Elsa API
app.MapElsaWorkflowEndpoints(); // 映射自定义 Elsa 端点
app.Run();
```

配置文件更新：

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=autocodeforge.db",
    "ElsaConnection": "Data Source=elsa-workflows.db"
  }
}
```

### 1.4 TaskEntity 更新

需要在 TaskEntity 中添加工作流关联字段：

```csharp
// AutoCodeForge.Core/Entities/TaskEntity.cs
public class TaskEntity : AuditableEntity
{
    // ... 现有字段 ...

    // 新增：Elsa 工作流关联字段
    public string? WorkflowInstanceId { get; set; }
    public string? WorkflowDefinitionId { get; set; }
    public int? WorkflowVersion { get; set; }
}
```

### 1.5 查询性能优化建议

| 优化策略 | 实现方式 | 适用场景 |
|---------|---------|---------|
| **批量查询** | 先收集 ID，再批量查询 Elsa 数据库 | 列表查询场景 |
| **缓存策略** | 使用 Redis/内存缓存工作流定义 | 工作流定义不常变更 |
| **异步并行** | 同时查询两个数据库 | 单个详情查询场景 |
| **ID 索引** | 确保 WorkflowInstanceId 有索引 | 所有查询场景 |

---

## 问题二：Elsa 代码文件结构组织

### 2.1 文件结构设计方案

**原则**：保持现有四项目架构不变，在现有结构中插入 Elsa 相关代码。

```
server/src/
├── AutoCodeForge.Api/
│   ├── Endpoints/
│   │   ├── ElsaWorkflowEndpoints.cs    <-- 新增：Elsa API 端点
│   │   └── ... (现有端点保持不变)
│   ├── Extensions/
│   │   ├── ServiceCollectionExtensions.cs (更新)
│   │   └── ElsaServiceExtensions.cs     <-- 新增：Elsa 服务配置
│   ├── Program.cs                       (更新)
│   └── appsettings.json                 (更新：添加 ElsaConnection)
│
├── AutoCodeForge.Application/
│   ├── Services/
│   │   ├── ElsaWorkflowService.cs       <-- 新增：桥接服务
│   │   └── ... (现有服务保持不变)
│   ├── Activities/                     <-- 新增：自定义 Activity 目录
│   │   ├── TaskExecutionActivity.cs
│   │   ├── HumanGateActivity.cs
│   │   ├── AgentExecutionActivity.cs
│   │   └── TaskCompleteActivity.cs
│   ├── Workflows/                      <-- 新增：工作流定义目录
│   │   ├── HelloWorldWorkflow.cs
│   │   ├── SevenStepDevelopmentWorkflow.cs
│   │   ├── RepoSyncWorkflow.cs
│   │   └── ReviewWorkflow.cs
│   └── ... (现有目录保持不变)
│
├── AutoCodeForge.Core/
│   ├── DTOs/
│   │   └── Elsa/                       <-- 新增：Elsa 相关 DTO
│   │       ├── TaskWithWorkflowDto.cs
│   │       ├── StartWorkflowRequest.cs
│   │       └── ResumeWorkflowRequest.cs
│   └── ... (现有目录保持不变)
│
└── AutoCodeForge.Infrastructure/      <-- 保持不变！
    └── ... (Elsa 使用 EF Core 独立持久化)
```

### 2.2 详细代码文件组织

#### 2.2.1 Api 层新增文件

**ElsaServiceExtensions.cs**（可选，用于分离 Elsa 配置）：

```csharp
// AutoCodeForge.Api/Extensions/ElsaServiceExtensions.cs
using Elsa.Extensions;
using Elsa.Persistence.EntityFramework.Sqlite;

namespace AutoCodeForge.Api.Extensions;

public static class ElsaServiceExtensions
{
    public static IServiceCollection AddElsaWorkflow(this IServiceCollection services, IConfiguration configuration)
    {
        var elsaConnectionString = configuration.GetConnectionString("ElsaConnection")
            ?? "Data Source=elsa-workflows.db";

        services.AddElsa(elsa =>
        {
            // 工作流管理（定义、实例）
            elsa.UseWorkflowManagement(management =>
            {
                management.UseEntityFrameworkCore(ef =>
                {
                    ef.UseSqlite(elsaConnectionString);
                    ef.RunMigrations = true;
                });
            });

            // 工作流运行时（书签、日志）
            elsa.UseWorkflowRuntime(runtime =>
            {
                runtime.UseEntityFrameworkCore(ef =>
                {
                    ef.UseSqlite(elsaConnectionString);
                    ef.RunMigrations = true;
                });
            });

            // 启用 API
            elsa.UseWorkflowsApi();

            // 启用 HTTP 活动
            elsa.UseHttp();

            // 启用定时触发
            elsa.UseScheduling();

            // 注册自定义 Activity（从 Application 层）
            elsa.AddActivitiesFrom(typeof(Application.Activities.TaskExecutionActivity).Assembly);

            // 注册工作流定义
            elsa.AddWorkflow<Application.Workflows.HelloWorldWorkflow>();
        });

        // 注册桥接服务
        services.AddScoped<Application.Services.ElsaWorkflowService>();

        return services;
    }
}
```

**ElsaWorkflowEndpoints.cs**：

```csharp
// AutoCodeForge.Api/Endpoints/ElsaWorkflowEndpoints.cs
using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Elsa;

namespace AutoCodeForge.Api.Endpoints;

public static class ElsaWorkflowEndpoints
{
    public static IEndpointRouteBuilder MapElsaWorkflowEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/elsa")
            .WithTags("Elsa Workflow")
            .RequireAuthorization();

        // Demo 端点
        group.MapPost("/hello-world", StartHelloWorldWorkflow)
            .WithName("StartHelloWorldWorkflow");

        // 任务工作流端点
        group.MapPost("/task-workflow/start", StartTaskWorkflow)
            .WithName("StartTaskWorkflow");

        // 查询端点
        group.MapGet("/tasks/{taskId}/workflow", GetTaskWorkflow)
            .WithName("GetTaskWorkflow");

        return builder;
    }

    private static async Task<Results<Ok<string>, BadRequest<string>>> StartHelloWorldWorkflow(
        ElsaWorkflowService elsaWorkflowService)
    {
        var instanceId = await elsaWorkflowService.StartHelloWorldWorkflowAsync();
        return TypedResults.Ok(instanceId);
    }

    private static async Task<Results<Ok<TaskWithWorkflowDto>, NotFound<string>, BadRequest<string>>> GetTaskWorkflow(
        Guid taskId, ElsaWorkflowService elsaWorkflowService)
    {
        var result = await elsaWorkflowService.GetTaskWithWorkflowAsync(taskId);
        if (result == null)
            return TypedResults.NotFound($"Task {taskId} not found");
        return TypedResults.Ok(result);
    }

    private static async Task<Results<Ok<string>, NotFound<string>, BadRequest<string>>> StartTaskWorkflow(
        StartTaskWorkflowRequest request, ElsaWorkflowService elsaWorkflowService)
    {
        var instanceId = await elsaWorkflowService.StartTaskWorkflowAsync(request.TaskId);
        return TypedResults.Ok(instanceId);
    }
}
```

#### 2.2.2 Application 层新增文件

**Activities/HelloWorldActivity.cs**：

```csharp
// AutoCodeForge.Application/Activities/HelloWorldActivity.cs
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;

namespace AutoCodeForge.Application.Activities;

/// <summary>
/// 简单的 Hello World Activity（用于 Demo）
/// </summary>
[Activity("HelloWorld", Category = "Demo", Description = "输出 Hello World 消息")]
public class HelloWorldActivity : CodeActivity<string>
{
    [ActivityInput(Description = "要输出的消息")]
    public Input<string> Message { get; set; } = new("Hello from Elsa!");

    protected override ValueTask<string> ExecuteAsync(ActivityExecutionContext context)
    {
        var message = Message.Get(context);
        Console.WriteLine($"[HelloWorldActivity] {message}");
        return new ValueTask<string>(message);
    }
}
```

**Workflows/HelloWorldWorkflow.cs**：

```csharp
// AutoCodeForge.Application/Workflows/HelloWorldWorkflow.cs
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Contracts;

namespace AutoCodeForge.Application.Workflows;

/// <summary>
/// 简单的 Hello World 工作流（用于 Demo 验证）
/// </summary>
public class HelloWorldWorkflow : IWorkflow
{
    public void Build(IWorkflowBuilder builder)
    {
        builder
            .WithId("HelloWorldWorkflow")
            .WithVersion(1)
            .WithName("Hello World")
            .WithDescription("一个简单的测试工作流")
            .Root(new Sequence
            {
                Activities =
                {
                    new WriteLine("=== Hello World Workflow Started ==="),
                    new Activities.HelloWorldActivity
                    {
                        Message = new("Welcome to AutoCodeForge + Elsa!")
                    },
                    new WriteLine("=== Hello World Workflow Completed ===")
                }
            });
    }
}
```

**Services/ElsaWorkflowService.cs**（完整版）：

```csharp
// AutoCodeForge.Application/Services/ElsaWorkflowService.cs
using Elsa.Workflows.Contracts;
using Elsa.Workflows.Management.Contracts;
using Elsa.Workflows.Runtime.Contracts;

namespace AutoCodeForge.Application.Services;

public class ElsaWorkflowService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IWorkflowRegistry _workflowRegistry;
    private readonly IWorkflowInstanceStore _workflowInstanceStore;
    private readonly IWorkflowInvoker _workflowInvoker;
    private readonly IWorkflowDefinitionStore _workflowDefinitionStore;

    public ElsaWorkflowService(
        ITaskRepository taskRepository,
        IWorkflowRegistry workflowRegistry,
        IWorkflowInstanceStore workflowInstanceStore,
        IWorkflowInvoker workflowInvoker,
        IWorkflowDefinitionStore workflowDefinitionStore)
    {
        _taskRepository = taskRepository;
        _workflowRegistry = workflowRegistry;
        _workflowInstanceStore = workflowInstanceStore;
        _workflowInvoker = workflowInvoker;
        _workflowDefinitionStore = workflowDefinitionStore;
    }

    /// <summary>
    /// 启动 Hello World 工作流（Demo 用）
    /// </summary>
    public async Task<string> StartHelloWorldWorkflowAsync()
    {
        var workflow = await _workflowRegistry.FindAsync("HelloWorldWorkflow");
        if (workflow == null)
            throw new InvalidOperationException("HelloWorldWorkflow not found");

        var result = await _workflowInvoker.StartAsync(workflow);
        return result.WorkflowInstance.Id;
    }

    /// <summary>
    /// 启动任务工作流
    /// </summary>
    public async Task<string> StartTaskWorkflowAsync(Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new InvalidOperationException($"Task {taskId} not found");

        var workflow = await _workflowRegistry.FindAsync("HelloWorldWorkflow"); // 先用简单工作流测试
        if (workflow == null)
            throw new InvalidOperationException("Workflow not found");

        var input = new Dictionary<string, object?>
        {
            ["TaskId"] = taskId
        };

        var result = await _workflowInvoker.StartAsync(workflow, input);
        var workflowInstanceId = result.WorkflowInstance.Id;

        // 更新任务关联
        task.WorkflowInstanceId = workflowInstanceId;
        task.WorkflowDefinitionId = workflow.Identity.DefinitionId;
        task.WorkflowVersion = workflow.Identity.Version;
        await _taskRepository.UpdateAsync(task);

        return workflowInstanceId;
    }

    /// <summary>
    /// 获取任务与工作流信息
    /// </summary>
    public async Task<TaskWithWorkflowDto?> GetTaskWithWorkflowAsync(Guid taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            return null;

        WorkflowInstance? workflowInstance = null;
        if (!string.IsNullOrEmpty(task.WorkflowInstanceId))
        {
            workflowInstance = await _workflowInstanceStore.FindAsync(task.WorkflowInstanceId);
        }

        return new TaskWithWorkflowDto
        {
            Task = task,
            WorkflowInstance = workflowInstance,
            WorkflowStatus = workflowInstance?.Status.ToString() ?? "NotStarted"
        };
    }

    /// <summary>
    /// 获取所有可用的工作流定义
    /// </summary>
    public async Task<IEnumerable<WorkflowDefinitionSummary>> GetAvailableWorkflowsAsync()
    {
        var definitions = await _workflowDefinitionStore.FindManyAsync();
        return definitions.Select(d => new WorkflowDefinitionSummary
        {
            Id = d.DefinitionId,
            Name = d.Name,
            Description = d.Description,
            Version = d.Version,
            IsLatest = d.IsLatest
        }).ToList();
    }
}

public class WorkflowDefinitionSummary
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; }
    public bool IsLatest { get; set; }
}
```

#### 2.2.3 Core 层新增文件

**DTOs/Elsa/TaskWithWorkflowDto.cs**：

```csharp
// AutoCodeForge.Core/DTOs/Elsa/TaskWithWorkflowDto.cs
using AutoCodeForge.Core.Entities;
using Elsa.Workflows.Management.Entities;

namespace AutoCodeForge.Core.DTOs.Elsa;

public class TaskWithWorkflowDto
{
    public TaskEntity Task { get; set; } = null!;
    public WorkflowInstance? WorkflowInstance { get; set; }
    public string WorkflowStatus { get; set; } = string.Empty;
}

public class StartTaskWorkflowRequest
{
    public Guid TaskId { get; set; }
}
```

### 2.3 Program.cs 更新

```csharp
// AutoCodeForge.Api/Program.cs
using AutoCodeForge.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ... 现有配置保持不变 ...

// 新增：Elsa Workflow 配置
builder.Services.AddElsaWorkflow(builder.Configuration);

var app = builder.Build();

// ... 现有中间件保持不变 ...

// 新增：启用 Elsa API
app.UseWorkflowsApi();

// 新增：映射自定义 Elsa 端点
app.MapElsaWorkflowEndpoints();

app.Run();
```

---

## 问题三：最小 Demo 验证方案

### 3.1 Demo 功能设计

**目标**：用最小工作量验证 Elsa Workflow 能够正常跑通，不涉及 7 步工作流。

**Demo 功能清单**：

| 功能 | 说明 | 优先级 |
|-----|------|-------|
| **Hello World 工作流** | 简单的控制台输出工作流 | P0 |
| **API 启动工作流** | 通过 API 触发 Hello World 工作流 | P0 |
| **查询工作流实例** | 查询工作流执行状态 | P0 |
| **工作流列表页面** | Vue 3 页面显示工作流列表 | P1 |
| **启动工作流按钮** | 前端触发工作流 | P1 |

### 3.2 Demo 实施步骤

#### Phase 1：后端最小 Demo（0.5-1 天）

**步骤**：
1. 安装 Elsa NuGet 包
2. 配置 Elsa 服务（独立数据库）
3. 创建 HelloWorldActivity
4. 创建 HelloWorldWorkflow
5. 创建 ElsaWorkflowService
6. 添加 API 端点
7. 测试验证

#### Phase 2：前端最小 Demo（0.5-1 天）

**步骤**：
1. 创建 workflow-center 模块 API 文件
2. 创建简单的工作流列表页面
3. 集成启动工作流按钮
4. 显示工作流执行状态

### 3.3 Demo 完整代码（后端）

**NuGet 包安装**（AutoCodeForge.Api.csproj）：

```xml
<PackageReference Include="Elsa" Version="3.6.2" />
<PackageReference Include="Elsa.Workflows.Core" Version="3.6.2" />
<PackageReference Include="Elsa.Workflows.Runtime" Version="3.6.2" />
<PackageReference Include="Elsa.Workflows.Management" Version="3.6.2" />
<PackageReference Include="Elsa.Persistence.EntityFramework.Sqlite" Version="3.6.2" />
<PackageReference Include="Elsa.Workflows.Api" Version="3.6.2" />
```

**完整 Demo 代码**参见附录。

### 3.4 验证测试清单

- [ ] Elsa 服务能够正常启动
- [ ] Elsa 数据库能够自动创建和迁移
- [ ] 工作流定义能够正常注册
- [ ] 通过 API 能够启动工作流
- [ ] 工作流执行后能够查询状态
- [ ] 日志输出正常
- [ ] Vue 3 前端能够调用 API
- [ ] 前端能够显示工作流状态

---

## 问题四：Vue 3 前端最佳实践

### 4.1 方案对比

| 方案 | 优点 | 缺点 | 推荐度 |
|-----|------|------|-------|
| **方案 A：完全自定义** | 完全可控，统一用户体验 | 开发工作量大 | ⭐⭐⭐⭐ (推荐) |
| **方案 B：Elsa Studio 集成** | 开箱即用，功能完整 | Blazor 技术栈，与 Vue 3 风格不一致 | ⭐⭐ |
| **方案 C：混合方案** | 部分复用，逐步过渡 | 需要维护两套 UI | ⭐⭐⭐ |
| **方案 D：第三方组件** | 快速实现，节省时间 | 依赖第三方，定制性受限 | ⭐⭐⭐ |

### 4.2 推荐方案：完全自定义 Vue 3 实现

#### 4.2.1 文件结构

```
client/src/modules/workflow-center/
├── api/
│   ├── workflow.api.ts              <-- Elsa API 封装
│   └── workflow.types.ts            <-- 类型定义
├── composables/
│   ├── useWorkflowDefinition.ts     <-- 工作流定义管理
│   ├── useWorkflowInstance.ts       <-- 工作流实例管理
│   └── useWorkflowDesigner.ts       <-- 设计器逻辑
├── store/
│   └── useWorkflowStore.ts          <-- Pinia 状态管理
├── views/
│   ├── WorkflowListView.vue         <-- 工作流定义列表
│   ├── WorkflowInstanceView.vue     <-- 工作流实例列表
│   ├── WorkflowDesignerView.vue     <-- 工作流设计器
│   └── WorkflowExecutionView.vue    <-- 工作流执行详情
├── components/
│   ├── WorkflowCanvas.vue           <-- 画布组件
│   ├── ActivityNode.vue             <-- 活动节点
│   ├── ActivityPalette.vue          <-- 活动面板
│   └── WorkflowExecutionLog.vue     <-- 执行日志
└── index.ts
```

#### 4.2.2 API 层封装

**workflow.api.ts**：

```typescript
// client/src/modules/workflow-center/api/workflow.api.ts
import { request } from '@/lib/request'

export interface WorkflowDefinition {
  id: string
  name: string
  description?: string
  version: number
  isLatest: boolean
  isPublished: boolean
}

export interface WorkflowInstance {
  id: string
  definitionId: string
  status: 'Running' | 'Completed' | 'Faulted' | 'Suspended' | 'Cancelled'
  createdAt: string
  startedAt?: string
  finishedAt?: string
}

export interface StartWorkflowRequest {
  workflowDefinitionId: string
  input?: Record<string, any>
}

export const workflowApi = {
  // 获取工作流定义列表
  getDefinitions: () => {
    return request.get<WorkflowDefinition[]>('/api/elsa/workflow-definitions')
  },

  // 获取工作流实例列表
  getInstances: (params?: { definitionId?: string; status?: string }) => {
    return request.get<WorkflowInstance[]>('/api/elsa/workflow-instances', { params })
  },

  // 获取工作流实例详情
  getInstance: (instanceId: string) => {
    return request.get<WorkflowInstance>(`/api/elsa/workflow-instances/${instanceId}`)
  },

  // 启动工作流
  start: (data: StartWorkflowRequest) => {
    return request.post<string>('/api/elsa/workflow-instances', data)
  },

  // 取消工作流
  cancel: (instanceId: string) => {
    return request.post(`/api/elsa/workflow-instances/${instanceId}/cancel`)
  },

  // Demo: 启动 Hello World
  startHelloWorld: () => {
    return request.post<string>('/api/elsa/hello-world')
  },

  // Demo: 获取任务工作流
  getTaskWorkflow: (taskId: string) => {
    return request.get(`/api/elsa/tasks/${taskId}/workflow`)
  }
}
```

#### 4.2.3 Pinia Store

**useWorkflowStore.ts**：

```typescript
// client/src/modules/workflow-center/store/useWorkflowStore.ts
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { workflowApi, type WorkflowDefinition, type WorkflowInstance } from '../api/workflow.api'

export const useWorkflowStore = defineStore('workflow', () => {
  // 状态
  const definitions = ref<WorkflowDefinition[]>([])
  const instances = ref<WorkflowInstance[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  // 操作：获取工作流定义
  const fetchDefinitions = async () => {
    loading.value = true
    error.value = null
    try {
      definitions.value = await workflowApi.getDefinitions()
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch workflow definitions'
    } finally {
      loading.value = false
    }
  }

  // 操作：获取工作流实例
  const fetchInstances = async (params?: { definitionId?: string; status?: string }) => {
    loading.value = true
    error.value = null
    try {
      instances.value = await workflowApi.getInstances(params)
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch workflow instances'
    } finally {
      loading.value = false
    }
  }

  // 操作：启动工作流
  const startWorkflow = async (definitionId: string, input?: Record<string, any>) => {
    loading.value = true
    error.value = null
    try {
      const instanceId = await workflowApi.start({ workflowDefinitionId: definitionId, input })
      await fetchInstances() // 刷新列表
      return instanceId
    } catch (err: any) {
      error.value = err.message || 'Failed to start workflow'
      throw err
    } finally {
      loading.value = false
    }
  }

  // 操作：启动 Hello World (Demo)
  const startHelloWorld = async () => {
    loading.value = true
    error.value = null
    try {
      const instanceId = await workflowApi.startHelloWorld()
      await fetchInstances()
      return instanceId
    } catch (err: any) {
      error.value = err.message || 'Failed to start Hello World workflow'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    // 状态
    definitions,
    instances,
    loading,
    error,
    // 操作
    fetchDefinitions,
    fetchInstances,
    startWorkflow,
    startHelloWorld
  }
})
```

#### 4.2.4 简单的工作流列表页面（Demo）

**WorkflowListView.vue**：

```vue
<!-- client/src/modules/workflow-center/views/WorkflowListView.vue -->
<template>
  <div class="workflow-list-view">
    <div class="header">
      <h1>Workflow Center</h1>
      <el-button type="primary" @click="startHelloWorld" :loading="store.loading">
        Start Hello World Demo
      </el-button>
    </div>

    <!-- 错误提示 -->
    <el-alert v-if="store.error" type="error" :message="store.error" show-icon class="mb-4" />

    <!-- 工作流实例列表 -->
    <el-card class="mb-4">
      <template #header>
        <div class="card-header">
          <span>Workflow Instances</span>
          <el-button size="small" @click="store.fetchInstances">Refresh</el-button>
        </div>
      </template>

      <el-table :data="store.instances" v-loading="store.loading" stripe>
        <el-table-column prop="id" label="Instance ID" width="200" show-overflow-tooltip />
        <el-table-column prop="definitionId" label="Definition ID" width="200" show-overflow-tooltip />
        <el-table-column prop="status" label="Status" width="120">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="Created At" width="180">
          <template #default="{ row }">
            {{ formatDate(row.createdAt) }}
          </template>
        </el-table-column>
        <el-table-column label="Actions" width="150">
          <template #default="{ row }">
            <el-button size="small" @click="viewInstance(row.id)">View</el-button>
            <el-button size="small" type="danger" @click="cancelInstance(row.id)" v-if="row.status === 'Running'">
              Cancel
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 空状态 -->
    <el-empty v-if="!store.loading && store.instances.length === 0" description="No workflow instances yet" />
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useWorkflowStore } from '../store/useWorkflowStore'
import { ElMessage } from 'element-plus'

const store = useWorkflowStore()

// 加载数据
onMounted(async () => {
  await store.fetchInstances()
})

// 启动 Hello World Demo
const startHelloWorld = async () => {
  try {
    const instanceId = await store.startHelloWorld()
    ElMessage.success(`Started workflow: ${instanceId}`)
  } catch (err) {
    ElMessage.error('Failed to start workflow')
  }
}

// 查看实例详情
const viewInstance = (instanceId: string) => {
  console.log('View instance:', instanceId)
  // TODO: 导航到详情页
}

// 取消工作流
const cancelInstance = async (instanceId: string) => {
  try {
    await workflowApi.cancel(instanceId)
    await store.fetchInstances()
    ElMessage.success('Workflow cancelled')
  } catch (err) {
    ElMessage.error('Failed to cancel workflow')
  }
}

// 辅助函数
const getStatusType = (status: string) => {
  const map: Record<string, any> = {
    Running: 'primary',
    Completed: 'success',
    Faulted: 'danger',
    Suspended: 'warning',
    Cancelled: 'info'
  }
  return map[status] || 'info'
}

const formatDate = (dateStr: string) => {
  return new Date(dateStr).toLocaleString()
}
</script>

<style scoped>
.workflow-list-view {
  padding: 24px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>
```

#### 4.2.5 路由配置

```typescript
// client/src/modules/workflow-center/index.ts
import type { RouteRecordRaw } from 'vue-router'
import WorkflowListView from './views/WorkflowListView.vue'

const routes: RouteRecordRaw[] = [
  {
    path: '/workflow-center',
    name: 'WorkflowCenter',
    component: WorkflowListView,
    meta: { requiresAuth: true }
  }
]

export default routes
```

在主路由中导入：

```typescript
// client/src/router/index.ts
import workflowCenterRoutes from '@/modules/workflow-center'

const routes = [
  // ... 现有路由 ...
  ...workflowCenterRoutes
]
```

#### 4.2.6 工作流设计器实现建议

对于工作流设计器，建议使用以下技术栈：

| 功能 | 推荐库 | 说明 |
|-----|-------|-----|
| **画布渲染** | Vue Flow | 专门为 Vue 设计的节点图库 |
| **拖拽交互** | Vue Flow 内置 | 支持节点拖拽、连线 |
| **节点库** | 自定义组件 | 根据 Activity 类型定义 |
| **属性编辑** | Element Plus Form | 动态表单编辑属性 |

**Vue Flow 快速开始**：

```bash
npm install @vue-flow/core @vue-flow/additional-components @vue-flow/controls @vue-flow/minimap
```

简单的设计器组件：

```vue
<!-- client/src/modules/workflow-center/components/WorkflowCanvas.vue -->
<template>
  <div class="workflow-canvas">
    <VueFlow v-model:nodes="nodes" v-model:edges="edges" :default-zoom="1">
      <Background />
      <Controls />
      <MiniMap />
    </VueFlow>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { VueFlow, Background, Controls, MiniMap } from '@vue-flow/core'
import '@vue-flow/core/dist/style.css'
import '@vue-flow/controls/dist/style.css'
import '@vue-flow/minimap/dist/style.css'

const nodes = ref([
  { id: '1', data: { label: 'Start' }, position: { x: 100, y: 100 } },
  { id: '2', data: { label: 'HelloWorld' }, position: { x: 300, y: 100 } },
  { id: '3', data: { label: 'End' }, position: { x: 500, y: 100 } }
])

const edges = ref([
  { id: 'e1-2', source: '1', target: '2' },
  { id: 'e2-3', source: '2', target: '3' }
])
</script>

<style scoped>
.workflow-canvas {
  height: 600px;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
}
</style>
```

### 4.3 分阶段实施建议

**Phase 1：基础工作流管理（Demo 阶段）**
- [ ] 工作流实例列表
- [ ] 启动工作流
- [ ] 取消工作流
- [ ] 查看工作流状态

**Phase 2：工作流定义管理**
- [ ] 工作流定义列表
- [ ] 工作流定义详情
- [ ] 版本管理

**Phase 3：工作流设计器**
- [ ] 可视化画布
- [ ] 节点拖拽
- [ ] 连线编辑
- [ ] 属性编辑
- [ ] 保存工作流定义

**Phase 4：深度集成**
- [ ] 任务页面集成工作流执行视图
- [ ] 工作流执行日志查看
- [ ] 工作流监控面板

---

## 附录：完整 Demo 代码示例

### A.1 后端完整 Demo 代码

**完整项目文件清单**：

```
server/src/
├── AutoCodeForge.Api/
│   ├── Extensions/
│   │   └── ElsaServiceExtensions.cs
│   ├── Endpoints/
│   │   └── ElsaWorkflowEndpoints.cs
│   ├── Program.cs (更新)
│   └── appsettings.json (更新)
│
└── AutoCodeForge.Application/
    ├── Activities/
    │   └── HelloWorldActivity.cs
    ├── Workflows/
    │   └── HelloWorldWorkflow.cs
    └── Services/
        └── ElsaWorkflowService.cs
```

代码内容见前文。

### A.2 前端完整 Demo 代码

```
client/src/modules/workflow-center/
├── api/
│   └── workflow.api.ts
├── store/
│   └── useWorkflowStore.ts
├── views/
│   └── WorkflowListView.vue
└── index.ts
```

代码内容见前文。

### A.3 快速启动指南

**后端**：
```bash
cd server/src/AutoCodeForge.Api
dotnet restore
dotnet run
```

**前端**：
```bash
cd client
npm install
npm run dev
```

**测试**：
1. 访问 Swagger UI: `https://localhost:7123/swagger`
2. 调用 `POST /api/elsa/hello-world` 启动工作流
3. 访问前端 `http://localhost:5173/workflow-center` 查看状态

---

## 总结

### 问题解答回顾

| 问题 | 答案 |
|-----|------|
| **双数据库查询** | Application 层桥接服务，分别查询 SqlSugar 和 EF Core，内存组装 |
| **代码组织** | 保持四项目架构，在 Api 和 Application 层插入 Elsa 代码 |
| **最小 Demo** | Hello World 工作流 + 简单列表页面，1-2 天完成 |
| **前端方案** | 完全自定义 Vue 3 实现，用 Vue Flow 做设计器，分阶段实施 |

### 下一步行动

1. **立即开始**：实施最小 Demo（Hello World）
2. **验证后**：逐步迁移 7 步工作流
3. **最后**：实现工作流设计器

祝您实施顺利！如有问题，请查阅 Elsa 官方文档或联系团队。
