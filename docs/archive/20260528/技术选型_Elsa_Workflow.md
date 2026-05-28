# Elsa Workflow 技术选型与架构设计方案

**版本**: v2.0.0  
**作者**: AutoCodeForge Team  
**创建日期**: 2026-05-26  
**最后更新**: 2026-05-26

---

## 目录

1. [概述与背景](#1-概述与背景)
2. [Elsa Workflow 核心特性与兼容性](#2-elsa-workflow-核心特性与兼容性)
3. [推荐架构设计](#3-推荐架构设计)
4. [前端集成方案](#4-前端集成方案)
5. [数据库设计与数据关联](#5-数据库设计与数据关联)
6. [渐进式迁移实施路线](#6-渐进式迁移实施路线)
7. [核心实现示例](#7-核心实现示例)
8. [优势与风险分析](#8-优势与风险分析)
9. [资源需求与结论](#9-资源需求与结论)

---

## 1. 概述与背景

### 1.1 当前系统状态

AutoCodeForge 系统当前采用分层架构，包含四个核心项目：

| 项目 | 职责 | 当前技术 |
|------|------|----------|
| `AutoCodeForge.Api` | API 端点、请求处理 | .NET 10, ASP.NET Core |
| `AutoCodeForge.Application` | 业务逻辑、服务层 | .NET 10 |
| `AutoCodeForge.Core` | 实体、DTO、接口定义 | .NET 10 |
| `AutoCodeForge.Infrastructure` | 数据访问、外部集成 | .NET 10, SqlSugar |

当前工作流特点：
- ✅ 支持7步固定工序流程
- ✅ 任务状态机管理
- ✅ 人工审批介入
- ✅ 步骤重试机制
- ❌ **工作流硬编码，无法灵活配置**
- ❌ **缺少可视化设计器**
- ❌ **版本管理困难**

### 1.2 核心问题与目标

**主要问题：**
1. 工作流逻辑硬编码，变更需要重新部署
2. 缺乏可视化设计能力，业务人员无法参与流程设计
3. 无法支持自定义工作流模板
4. 工作流版本管理缺失

**改造目标：**
1. 保持现有四项目架构不变
2. 引入 Elsa Workflow 作为工作流引擎
3. 支持 Vue 3 前端深度集成
4. 采用独立数据库方案，避免与现有业务数据耦合
5. 实现渐进式迁移，降低风险

---

## 2. Elsa Workflow 核心特性与兼容性

### 2.1 .NET 10 支持确认

根据 NuGet 官方信息，**Elsa Workflow 3.6.2+ 完全支持 .NET 10.0**：

| 包名 | 支持的 .NET 版本 |
|------|-----------------|
| `Elsa.Workflows.Core` | net8.0, net9.0, **net10.0** |
| `Elsa.Workflows.Runtime` | net8.0, net9.0, **net10.0** |
| `Elsa.Studio.Workflows` | net8.0, net9.0, **net10.0** |

**结论：Elsa Workflow 与 .NET 10 完全兼容，可以安全使用。**

### 2.2 Elsa Workflow 核心特性

| 特性 | 说明 |
|------|------|
| **可视化设计器** | Blazor 或自定义 Vue 3 设计器 |
| **长期运行** | 支持工作流挂起、恢复和持久化 |
| **多数据库支持** | EF Core 支持 PostgreSQL、SQL Server、SQLite、MySQL |
| **自定义活动** | 可封装现有业务逻辑为活动 |
| **版本管理** | 工作流定义版本控制，支持灰度发布 |
| **并行执行** | 支持分支、并行、子流程等复杂流程 |
| **事件驱动** | HTTP Webhook、定时触发、事件订阅 |
| **REST API** | 完整的 API 端点供前端调用 |
| **多租户** | 支持共享数据库或独立数据库 |

---

## 3. 推荐架构设计

### 3.1 整体架构概览

**架构原则：**
- ✅ 保持现有四个项目结构不变
- ✅ Elsa 作为独立模块集成，不破坏现有架构
- ✅ 采用独立数据库方案
- ✅ 通过应用服务层桥接两个系统

```
┌─────────────────────────────────────────────────────────────────┐
│                     Vue 3 前端应用                                │
│  ┌──────────────────┐  ┌──────────────────┐  ┌─────────────────┐ │
│  │  现有业务模块   │  │ Elsa 工作流模块  │  │ 工作流设计器   │ │
│  └──────────────────┘  └──────────────────┘  └─────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                              ↓ HTTP API
┌─────────────────────────────────────────────────────────────────┐
│                   AutoCodeForge.Api                             │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │ 现有 API 端点 (保持不变)                                   │  │
│  │ - MapTaskEndpoints, MapHumanGateEndpoints, ...            │  │
│  └───────────────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │ 新增 Elsa API 端点                                        │  │
│  │ - MapElsaWorkflowEndpoints (封装 Elsa API)                │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│                AutoCodeForge.Application                        │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │ 现有服务 (保持不变)                                        │  │
│  │ - TaskService, HumanGateService, ...                      │  │
│  └───────────────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │ Elsa 集成服务 (新增)                                       │  │
│  │ - ElsaWorkflowService (桥接)                              │  │
│  │ - Custom Activities (封装现有逻辑)                        │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              ↓
        ┌─────────────────────┴─────────────────────┐
        ↓                                           ↓
┌───────────────────────────┐          ┌───────────────────────────┐
│ AutoCodeForge.Core/       │          │ Elsa Workflow 模块        │
│ Infrastructure            │          │ (独立集成)                │
│                           │          │                           │
│ ┌───────────────────────┐ │          │ ┌───────────────────────┐ │
│ │ SqlSugar ORM         │ │          │ │ EF Core               │ │
│ └───────────────────────┘ │          │ └───────────────────────┘ │
└───────────────────────────┘          └───────────────────────────┘
        ↓                                           ↓
┌───────────────────────────┐          ┌───────────────────────────┐
│  业务数据库 (SQLite)      │          │ Elsa 工作流数据库        │
│                           │          │ (独立 SQLite/PostgreSQL) │
│ - TaskEntity              │          │ - WorkflowDefinition    │
│ - TaskStepEntity          │          │ - WorkflowInstance      │
│ - HumanGateEntity         │          │ - Bookmark              │
│ - ...                     │          │ - ExecutionLog          │
└───────────────────────────┘          └───────────────────────────┘
```

### 3.2 项目集成方案

#### 3.2.1 NuGet 包依赖

在 `AutoCodeForge.Api.csproj` 中新增：

```xml
<!-- Elsa Workflow 核心包 -->
<PackageReference Include="Elsa" Version="3.6.2" />
<PackageReference Include="Elsa.Workflows.Core" Version="3.6.2" />
<PackageReference Include="Elsa.Workflows.Runtime" Version="3.6.2" />
<PackageReference Include="Elsa.Workflows.Management" Version="3.6.2" />

<!-- EF Core 持久化 (SQLite) -->
<PackageReference Include="Elsa.Persistence.EFCore.Sqlite" Version="3.6.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0" />

<!-- API 端点 -->
<PackageReference Include="Elsa.Workflows.Api" Version="3.6.2" />

<!-- HTTP 和定时触发 (可选) -->
<PackageReference Include="Elsa.Http" Version="3.6.2" />
<PackageReference Include="Elsa.Scheduling" Version="3.6.2" />
```

#### 3.2.2 分层职责分配

| 层级 | Elsa 相关职责 |
|------|--------------|
| **Api** | Elsa 服务配置、自定义 API 端点、CORS 配置 |
| **Application** | ElsaWorkflowService、自定义 Activities、工作流定义 |
| **Core** | Elsa 相关 DTO、接口定义（如需要） |
| **Infrastructure** | 无需改动 - Elsa 使用 EF Core 独立持久化 |

---

## 4. 前端集成方案

### 4.1 前端架构

Vue 3 前端与 Elsa Workflow 有三种集成方式：

| 方案 | 说明 | 适用场景 |
|------|------|---------|
| **方案 A: 嵌入式 Elsa Studio** | 在 iframe 中嵌入 Elsa 的 Blazor Studio | 快速验证、初期开发 |
| **方案 B: API 集成 + 自定义 UI** | 调用 Elsa REST API，完全自定义 Vue 3 UI | 深度集成、统一用户体验（推荐） |
| **方案 C: workflow_for_elsa 组件** | 使用第三方 Vue 3 工作流设计器组件 | 中等复杂度需求 |

### 4.2 推荐方案：方案 B（API 集成 + 自定义 Vue 3 UI）

#### 4.2.1 Elsa REST API 端点

Elsa 提供完整的 REST API，主要端点包括：

| 端点 | 方法 | 说明 |
|------|------|------|
| `/elsa/api/workflow-definitions` | GET | 获取工作流定义列表 |
| `/elsa/api/workflow-definitions` | POST | 创建工作流定义 |
| `/elsa/api/workflow-definitions/{id}` | GET | 获取单个工作流定义 |
| `/elsa/api/workflow-definitions/{id}` | PUT | 更新工作流定义 |
| `/elsa/api/workflow-instances` | GET | 获取工作流实例列表 |
| `/elsa/api/workflow-instances` | POST | 启动工作流实例 |
| `/elsa/api/workflow-instances/{id}` | GET | 获取工作流实例详情 |
| `/elsa/api/workflow-instances/{id}/execute` | POST | 执行/恢复工作流 |
| `/elsa/api/workflow-instances/{id}/cancel` | POST | 取消工作流 |

#### 4.2.2 Vue 3 模块结构建议

```
client/src/modules/workflow/
├── api/
│   ├── workflow.api.ts           # Elsa API 封装
│   └── workflow.types.ts         # 类型定义
├── composables/
│   ├── useWorkflowDesigner.ts    # 设计器逻辑
│   └── useWorkflowInstance.ts    # 实例管理逻辑
├── store/
│   └── useWorkflowStore.ts       # Pinia 状态管理
├── views/
│   ├── WorkflowDesignerView.vue  # 工作流设计器页面
│   ├── WorkflowListView.vue      # 工作流列表页面
│   └── WorkflowInstanceView.vue  # 工作流实例详情
├── components/
│   ├── WorkflowCanvas.vue        # 画布组件
│   ├── ActivityNode.vue          # 活动节点组件
│   └── ActivityPropertyPanel.vue # 属性编辑面板
└── index.ts
```

#### 4.2.3 workflow_for_elsa 组件参考

如果需要快速实现，可以使用第三方组件：

```bash
npm install workflow_for_elsa
```

```vue
<template>
  <WorkflowDesigner
    :initial-nodes="nodes"
    :initial-edges="edges"
    @workflow-change="handleWorkflowChange"
    @save-elsa="handleSaveElsa"
  />
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { WorkflowDesigner } from 'workflow_for_elsa'
import 'workflow_for_elsa/dist/style.css'

const nodes = ref([])
const edges = ref([])

const handleWorkflowChange = (nodes: any[], edges: any[]) => {
  console.log('工作流变化:', { nodes, edges })
}

const handleSaveElsa = (workflow: any) => {
  // 发送到 Elsa 服务器
  fetch('/api/elsa/workflows', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(workflow)
  })
}
</script>
```

#### 4.2.4 统一认证集成

Elsa 支持多种认证方式，推荐与现有 JWT 认证集成：

```csharp
// Program.cs 中配置
builder.Services.AddElsa(elsa =>
{
    // 配置 Elsa API 使用现有 JWT 认证
    elsa.UseWorkflowsApi(api =>
    {
        // 使用现有的 JWT 认证方案
        api.AuthenticationScheme = "Bearer";
    });
});
```

### 4.3 现有业务页面与工作流的集成

在现有任务详情页面中嵌入工作流执行视图：

```vue
<!-- TaskDetailView.vue -->
<template>
  <div class="task-detail">
    <!-- 现有任务信息 -->
    <div class="task-info">
      <h2>{{ task.title }}</h2>
      <p>{{ task.description }}</p>
    </div>

    <!-- 新增：工作流执行视图 -->
    <div class="workflow-execution" v-if="task.workflowInstanceId">
      <WorkflowExecutionView
        :workflow-instance-id="task.workflowInstanceId"
      />
    </div>
  </div>
</template>
```

---

## 5. 数据库设计与数据关联

### 5.1 双数据库方案

**原则：**
- 业务数据与工作流数据完全分离
- 通过关联字段建立逻辑关系
- 避免分布式事务，采用最终一致性

#### 5.1.1 数据库连接配置

```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=autocodeforge.db",  // 现有业务数据库
    "ElsaConnection": "Data Source=elsa-workflows.db"     // Elsa 独立数据库
  }
}
```

#### 5.1.2 现有业务数据库改动

在现有 `TaskEntity` 中新增关联字段（无需改动表结构，仅新增字段）：

```csharp
// AutoCodeForge.Core/Entities/TaskEntity.cs
public class TaskEntity : AuditableEntity
{
    // ... 现有字段 ...

    // 新增：关联的 Elsa 工作流实例 ID
    public string? WorkflowInstanceId { get; set; }

    // 新增：关联的工作流定义 ID
    public string? WorkflowDefinitionId { get; set; }

    // 新增：工作流版本
    public int? WorkflowVersion { get; set; }
}
```

### 5.2 两个数据库间的数据关联方案

#### 5.2.1 关联策略

| 场景 | 关联方式 | 说明 |
|------|---------|------|
| 任务 → 工作流实例 | `TaskEntity.WorkflowInstanceId` | 任务启动时记录关联的工作流实例 |
| 工作流活动 → 业务数据 | Activity Input/Output | 通过活动输入输出传递业务数据 ID |
| 查询关联数据 | ElsaWorkflowService 桥接 | 应用服务层联合查询两个数据库 |

#### 5.2.2 数据查询示例

```csharp
// AutoCodeForge.Application/Services/ElsaWorkflowService.cs
public class ElsaWorkflowService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IWorkflowInstanceStore _workflowInstanceStore;

    public async Task<TaskWithWorkflowDto> GetTaskWithWorkflowAsync(Guid taskId)
    {
        // 1. 从业务数据库查询任务
        var task = await _taskRepository.GetByIdAsync(taskId);

        // 2. 如果有关联的工作流实例，从 Elsa 数据库查询
        WorkflowInstance? workflowInstance = null;
        if (!string.IsNullOrEmpty(task.WorkflowInstanceId))
        {
            workflowInstance = await _workflowInstanceStore.FindAsync(
                task.WorkflowInstanceId);
        }

        // 3. 组装返回
        return new TaskWithWorkflowDto
        {
            Task = task,
            WorkflowInstance = workflowInstance
        };
    }
}
```

### 5.3 Elsa 数据库表结构

Elsa 使用 EF Core 自动创建以下表：

**Management 相关表：**
- `WorkflowDefinitions` - 工作流定义
- `WorkflowDefinitionVersions` - 工作流定义版本

**Runtime 相关表：**
- `WorkflowInstances` - 工作流实例
- `Bookmarks` - 书签（工作流挂起位置）
- `WorkflowInbox` - 工作流收件箱
- `WorkflowExecutionLogRecords` - 执行日志

### 5.4 数据一致性保障

| 一致性级别 | 实现方式 |
|-----------|---------|
| **强一致性** | 本地事务 + 补偿机制 |
| **最终一致性** | 后台任务同步 + 事件驱动 |
| **幂等性** | 操作唯一标识 + 状态检查 |

---

## 6. 渐进式迁移实施路线

### 6.1 Phase 1: Elsa 基础设施搭建（1-2 周）

**目标：** 搭建 Elsa 基础框架，与现有系统集成

**任务清单：**
- [ ] 安装 Elsa NuGet 包到 Api 项目
- [ ] 配置 Elsa 独立数据库（SQLite）
- [ ] 创建 Elsa 集成服务
- [ ] 配置 Elsa API 端点
- [ ] 实现与现有 JWT 认证集成
- [ ] 搭建基础测试工作流

**交付物：**
- 可运行的 Elsa 框架
- 基础集成文档
- 简单的测试工作流

### 6.2 Phase 2: 核心工作流迁移（2-3 周）

**目标：** 将现有的 7 步开发流程迁移到 Elsa

**任务清单：**
- [ ] 创建 TaskExecutionActivity（封装现有 TaskExecutor）
- [ ] 创建 HumanGateActivity（封装人工审批）
- [ ] 创建 AgentExecutionActivity（封装 Agent 执行）
- [ ] 定义 7 步开发工作流模板
- [ ] 修改 TaskService 集成 Elsa
- [ ] 保持现有 API 接口兼容
- [ ] 数据迁移：为现有任务关联工作流实例
- [ ] 完整的集成测试

**交付物：**
- 迁移后的 7 步工作流
- 向后兼容的 API
- 迁移文档和脚本

### 6.3 Phase 3: 前端工作流模块（2 周）

**目标：** 实现 Vue 3 前端工作流管理模块

**任务清单：**
- [ ] 创建 workflow 模块结构
- [ ] 实现工作流列表页面
- [ ] 实现工作流实例详情页面
- [ ] 集成到现有任务详情页面
- [ ] 工作流执行可视化
- [ ] 前端单元测试

**交付物：**
- 完整的工作流前端模块
- 用户操作手册

### 6.4 Phase 4: 工作流设计器（2-3 周）

**目标：** 实现可视化工作流设计器

**任务清单：**
- [ ] 实现工作流画布组件
- [ ] 实现自定义活动库
- [ ] 实现拖拽设计功能
- [ ] 实现属性编辑面板
- [ ] 工作流版本管理 UI
- [ ] 集成测试

**交付物：**
- 工作流设计器
- 设计器使用文档

### 6.5 Phase 5: 增强功能与优化（持续）

**目标：** 高级功能和性能优化

**任务清单：**
- [ ] 工作流模板库
- [ ] 工作流导入/导出
- [ ] 并行流程支持
- [ ] Pipeline 集成
- [ ] 性能优化
- [ ] 监控告警

---

## 7. 核心实现示例

### 7.1 Program.cs 配置

```csharp
// AutoCodeForge.Api/Program.cs
using Elsa.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ... 现有配置 ...

// 获取连接字符串
var elsaConnectionString = builder.Configuration.GetConnectionString("ElsaConnection")
    ?? throw new InvalidOperationException("Connection string 'ElsaConnection' not found.");

// 配置 Elsa Workflow
builder.Services.AddElsa(elsa =>
{
    // 配置工作流管理（定义、实例）
    elsa.UseWorkflowManagement(management =>
    {
        management.UseEntityFrameworkCore(ef =>
        {
            ef.UseSqlite(elsaConnectionString);
            ef.RunMigrations = true; // 开发环境自动迁移
        });
    });

    // 配置工作流运行时（书签、日志等）
    elsa.UseWorkflowRuntime(runtime =>
    {
        runtime.UseEntityFrameworkCore(ef =>
        {
            ef.UseSqlite(elsaConnectionString);
            ef.RunMigrations = true;
        });
    });

    // 启用 API 端点
    elsa.UseWorkflowsApi();

    // 启用 HTTP 活动
    elsa.UseHttp();

    // 启用定时触发
    elsa.UseScheduling();

    // 注册自定义活动
    elsa.AddActivitiesFrom(typeof(ElsaWorkflowService).Assembly);

    // 注册工作流定义
    elsa.AddWorkflow<SevenStepDevelopmentWorkflow>();
    elsa.AddWorkflow<RepoSyncWorkflow>();
    elsa.AddWorkflow<ReviewWorkflow>();
});

// 注册 Elsa 集成服务
builder.Services.AddScoped<IElsaWorkflowService, ElsaWorkflowService>();

// ... 现有配置 ...

var app = builder.Build();

// ... 现有中间件 ...

// 启用 Elsa API 端点
app.UseWorkflowsApi();

// 映射自定义 Elsa 端点
app.MapElsaWorkflowEndpoints();

// ... 现有映射 ...

app.Run();
```

### 7.2 自定义 Activity 示例

```csharp
// AutoCodeForge.Application/Activities/TaskExecutionActivity.cs
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;

namespace AutoCodeForge.Application.Activities;

/// <summary>
/// 封装现有 TaskExecutor 的活动
/// </summary>
[Activity("TaskExecution", Category = "AutoCodeForge", Description = "执行任务步骤")]
public class TaskExecutionActivity : CodeActivity
{
    /// <summary>
    /// 任务 ID
    /// </summary>
    [ActivityInput(Description = "任务 ID")]
    public Input<Guid> TaskId { get; set; } = default!;

    /// <summary>
    /// 步骤 ID（可选）
    /// </summary>
    [ActivityInput(Description = "步骤 ID")]
    public Input<Guid?> StepId { get; set; } = default!;

    /// <summary>
    /// 执行结果
    /// </summary>
    [ActivityOutput(Description = "执行结果")]
    public Output<string?> Result { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var taskExecutor = context.GetRequiredService<TaskExecutor>();
        var taskRepository = context.GetRequiredService<ITaskRepository>();

        var taskId = TaskId.Get(context);
        var task = await taskRepository.GetByIdAsync(taskId, true, context.CancellationToken);

        if (task == null)
        {
            throw new InvalidOperationException($"Task {taskId} not found");
        }

        // 执行任务
        await taskExecutor.ExecuteAsync(task, context.CancellationToken);

        // 设置输出
        Result.Set(context, task.Result);
    }
}
```

### 7.3 HumanGateActivity 示例

```csharp
// AutoCodeForge.Application/Activities/HumanGateActivity.cs
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;

namespace AutoCodeForge.Application.Activities;

/// <summary>
/// 人工审批活动 - 挂起工作流等待审批
/// </summary>
[Activity("HumanGate", Category = "AutoCodeForge", Description = "等待人工审批")]
public class HumanGateActivity : Activity
{
    /// <summary>
    /// 任务 ID
    /// </summary>
    [ActivityInput(Description = "任务 ID")]
    public Input<Guid> TaskId { get; set; } = default!;

    /// <summary>
    /// 审批类型
    /// </summary>
    [ActivityInput(Description = "审批类型")]
    public Input<string> GateType { get; set; } = new("Review");

    /// <summary>
    /// 审批结果
    /// </summary>
    [ActivityOutput(Description = "审批结果")]
    public Output<bool> IsApproved { get; set; } = default!;

    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var humanGateService = context.GetRequiredService<IHumanGateService>();
        var taskId = TaskId.Get(context);
        var gateType = GateType.Get(context);

        // 创建审批记录
        var gate = await humanGateService.CreateGateAsync(taskId, gateType, context.CancellationToken);

        // 保存审批 ID 到工作流上下文中
        context.JournalData["HumanGateId"] = gate.Id;

        // 创建书签，挂起工作流
        context.CreateBookmark(new HumanGateBookmarkPayload
        {
            GateId = gate.Id,
            TaskId = taskId
        });
    }
}

/// <summary>
/// 人工审批书签载荷
/// </summary>
public record HumanGateBookmarkPayload
{
    public Guid GateId { get; init; }
    public Guid TaskId { get; init; }
}
```

### 7.4 工作流定义示例

```csharp
// AutoCodeForge.Application/Workflows/SevenStepDevelopmentWorkflow.cs
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Contracts;

namespace AutoCodeForge.Application.Workflows;

/// <summary>
/// 七步开发工作流定义
/// </summary>
public class SevenStepDevelopmentWorkflow : IWorkflow
{
    public void Build(IWorkflowBuilder builder)
    {
        builder
            .WithId("SevenStepDevelopmentWorkflow")
            .WithVersion(1)
            .WithName("七步开发工作流")
            .WithDescription("标准的七步开发流程")
            .Root(new Sequence
            {
                Activities =
                {
                    // 步骤 1: 需求分析
                    new TaskExecutionActivity
                    {
                        Name = "需求分析",
                        TaskId = new Input<Guid>(context => context.GetInput<Guid>("TaskId"))
                    },

                    // 步骤 2: 查询当前状态
                    new TaskExecutionActivity
                    {
                        Name = "查询当前状态",
                        TaskId = new Input<Guid>(context => context.GetInput<Guid>("TaskId"))
                    },

                    // 步骤 3: 制定计划
                    new TaskExecutionActivity
                    {
                        Name = "制定计划",
                        TaskId = new Input<Guid>(context => context.GetInput<Guid>("TaskId"))
                    },

                    // 审批门 1: 计划审批
                    new HumanGateActivity
                    {
                        Name = "计划审批",
                        TaskId = new Input<Guid>(context => context.GetInput<Guid>("TaskId")),
                        GateType = new Input<string>("PlanReview")
                    },

                    // 步骤 4: 代码开发
                    new AgentExecutionActivity
                    {
                        Name = "代码开发",
                        TaskId = new Input<Guid>(context => context.GetInput<Guid>("TaskId"))
                    },

                    // 步骤 5: 测试验证
                    new TaskExecutionActivity
                    {
                        Name = "测试验证",
                        TaskId = new Input<Guid>(context => context.GetInput<Guid>("TaskId"))
                    },

                    // 审批门 2: 测试审批
                    new HumanGateActivity
                    {
                        Name = "测试审批",
                        TaskId = new Input<Guid>(context => context.GetInput<Guid>("TaskId")),
                        GateType = new Input<string>("TestReview")
                    },

                    // 步骤 6: 提交 PR
                    new TaskExecutionActivity
                    {
                        Name = "提交 PR",
                        TaskId = new Input<Guid>(context => context.GetInput<Guid>("TaskId"))
                    },

                    // 步骤 7: 最终审核
                    new HumanGateActivity
                    {
                        Name = "最终审核",
                        TaskId = new Input<Guid>(context => context.GetInput<Guid>("TaskId")),
                        GateType = new Input<string>("FinalReview")
                    },

                    // 任务完成
                    new TaskCompleteActivity
                    {
                        Name = "任务完成",
                        TaskId = new Input<Guid>(context => context.GetInput<Guid>("TaskId"))
                    }
                }
            });
    }
}
```

### 7.5 ElsaWorkflowService 集成服务

```csharp
// AutoCodeForge.Application/Services/ElsaWorkflowService.cs
using Elsa.Workflows.Contracts;
using Elsa.Workflows.Management.Contracts;
using Elsa.Workflows.Runtime.Contracts;
using Elsa.Workflows.Runtime.Models;

namespace AutoCodeForge.Application.Services;

/// <summary>
/// Elsa 工作流集成服务 - 桥接业务系统与 Elsa
/// </summary>
public interface IElsaWorkflowService
{
    /// <summary>
    /// 启动工作流
    /// </summary>
    Task<string?> StartWorkflowAsync(Guid taskId, string workflowDefinitionId);

    /// <summary>
    /// 获取工作流实例
    /// </summary>
    Task<WorkflowInstance?> GetWorkflowInstanceAsync(string workflowInstanceId);

    /// <summary>
    /// 恢复被挂起的工作流（审批后调用）
    /// </summary>
    Task ResumeWorkflowAsync(string workflowInstanceId, Guid gateId, bool isApproved);

    /// <summary>
    /// 取消工作流
    /// </summary>
    Task CancelWorkflowAsync(string workflowInstanceId);

    /// <summary>
    /// 获取可用的工作流定义
    /// </summary>
    Task<IEnumerable<WorkflowDefinition>> GetAvailableWorkflowsAsync();
}

public class ElsaWorkflowService : IElsaWorkflowService
{
    private readonly IWorkflowRegistry _workflowRegistry;
    private readonly IWorkflowInstanceStore _workflowInstanceStore;
    private readonly IWorkflowInvoker _workflowInvoker;
    private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
    private readonly ITaskRepository _taskRepository;

    public ElsaWorkflowService(
        IWorkflowRegistry workflowRegistry,
        IWorkflowInstanceStore workflowInstanceStore,
        IWorkflowInvoker workflowInvoker,
        IWorkflowDefinitionStore workflowDefinitionStore,
        ITaskRepository taskRepository)
    {
        _workflowRegistry = workflowRegistry;
        _workflowInstanceStore = workflowInstanceStore;
        _workflowInvoker = workflowInvoker;
        _workflowDefinitionStore = workflowDefinitionStore;
        _taskRepository = taskRepository;
    }

    public async Task<string?> StartWorkflowAsync(Guid taskId, string workflowDefinitionId)
    {
        // 1. 获取任务
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
            throw new InvalidOperationException($"Task {taskId} not found");

        // 2. 查找工作流定义
        var workflowBlueprint = await _workflowRegistry.FindAsync(workflowDefinitionId);
        if (workflowBlueprint == null)
            throw new InvalidOperationException($"Workflow {workflowDefinitionId} not found");

        // 3. 准备输入
        var input = new Dictionary<string, object?>
        {
            ["TaskId"] = taskId
        };

        // 4. 启动工作流
        var result = await _workflowInvoker.StartAsync(workflowBlueprint, input);
        var workflowInstanceId = result.WorkflowInstance.Id;

        // 5. 更新任务，关联工作流实例
        task.WorkflowInstanceId = workflowInstanceId;
        task.WorkflowDefinitionId = workflowDefinitionId;
        task.WorkflowVersion = workflowBlueprint.Version;
        await _taskRepository.UpdateAsync(task);

        return workflowInstanceId;
    }

    public async Task<WorkflowInstance?> GetWorkflowInstanceAsync(string workflowInstanceId)
    {
        return await _workflowInstanceStore.FindAsync(workflowInstanceId);
    }

    public async Task ResumeWorkflowAsync(string workflowInstanceId, Guid gateId, bool isApproved)
    {
        var workflowInstance = await _workflowInstanceStore.FindAsync(workflowInstanceId);
        if (workflowInstance == null)
            throw new InvalidOperationException($"Workflow instance {workflowInstanceId} not found");

        // 恢复工作流，传递审批结果
        var input = new Dictionary<string, object?>
        {
            ["GateId"] = gateId,
            ["IsApproved"] = isApproved
        };

        await _workflowInvoker.ResumeAsync(workflowInstance, input);
    }

    public async Task CancelWorkflowAsync(string workflowInstanceId)
    {
        var workflowInstance = await _workflowInstanceStore.FindAsync(workflowInstanceId);
        if (workflowInstance == null)
            return;

        await _workflowInvoker.CancelAsync(workflowInstance);
    }

    public async Task<IEnumerable<WorkflowDefinition>> GetAvailableWorkflowsAsync()
    {
        return await _workflowDefinitionStore.FindManyAsync();
    }
}
```

### 7.6 自定义 API 端点示例

```csharp
// AutoCodeForge.Api/Endpoints/ElsaWorkflowEndpoints.cs
using AutoCodeForge.Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AutoCodeForge.Api.Endpoints;

/// <summary>
/// Elsa 工作流相关 API 端点
/// </summary>
public static class ElsaWorkflowEndpoints
{
    public static IEndpointRouteBuilder MapElsaWorkflowEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/elsa")
            .WithTags("Elsa Workflow")
            .RequireAuthorization();

        // 启动工作流
        group.MapPost("/workflows/start", StartWorkflow)
            .WithName("StartWorkflow")
            .WithOpenApi();

        // 恢复工作流
        group.MapPost("/workflows/{workflowInstanceId}/resume", ResumeWorkflow)
            .WithName("ResumeWorkflow")
            .WithOpenApi();

        // 获取工作流实例
        group.MapGet("/workflows/instances/{workflowInstanceId}", GetWorkflowInstance)
            .WithName("GetWorkflowInstance")
            .WithOpenApi();

        // 获取可用工作流定义
        group.MapGet("/workflows/definitions", GetWorkflowDefinitions)
            .WithName("GetWorkflowDefinitions")
            .WithOpenApi();

        return builder;
    }

    private static async Task<Results<Ok<string>, BadRequest<string>>> StartWorkflow(
        StartWorkflowRequest request,
        IElsaWorkflowService elsaWorkflowService)
    {
        try
        {
            var workflowInstanceId = await elsaWorkflowService.StartWorkflowAsync(
                request.TaskId,
                request.WorkflowDefinitionId);

            return TypedResults.Ok(workflowInstanceId!);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    private static async Task<Results<Ok, NotFound<string>, BadRequest<string>>> ResumeWorkflow(
        string workflowInstanceId,
        ResumeWorkflowRequest request,
        IElsaWorkflowService elsaWorkflowService)
    {
        try
        {
            await elsaWorkflowService.ResumeWorkflowAsync(
                workflowInstanceId,
                request.GateId,
                request.IsApproved);

            return TypedResults.Ok();
        }
        catch (InvalidOperationException ex)
        {
            return TypedResults.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    private static async Task<Results<Ok<WorkflowInstance>, NotFound<string>>> GetWorkflowInstance(
        string workflowInstanceId,
        IElsaWorkflowService elsaWorkflowService)
    {
        var instance = await elsaWorkflowService.GetWorkflowInstanceAsync(workflowInstanceId);
        if (instance == null)
            return TypedResults.NotFound($"Workflow instance {workflowInstanceId} not found");

        return TypedResults.Ok(instance);
    }

    private static async Task<Ok<IEnumerable<WorkflowDefinition>>> GetWorkflowDefinitions(
        IElsaWorkflowService elsaWorkflowService)
    {
        var definitions = await elsaWorkflowService.GetAvailableWorkflowsAsync();
        return TypedResults.Ok(definitions);
    }
}

/// <summary>
/// 启动工作流请求
/// </summary>
public record StartWorkflowRequest(Guid TaskId, string WorkflowDefinitionId);

/// <summary>
/// 恢复工作流请求
/// </summary>
public record ResumeWorkflowRequest(Guid GateId, bool IsApproved);
```

---

## 8. 优势与风险分析

### 8.1 核心优势

| 维度 | 当前系统 | 引入 Elsa 后 |
|------|---------|-------------|
| **灵活性** | 固定7步流程 | 可配置任意工作流模板 |
| **可视化** | 无 | 可视化设计器，直观展示流程 |
| **版本管理** | 代码版本控制 | 内置工作流版本，支持灰度发布 |
| **调试能力** | 日志追踪 | 实时执行追踪，断点调试 |
| **并行流程** | 需要复杂编码 | 原生支持分支/并行 |
| **Agent 集成** | 耦合在流程中 | 独立 Activity，灵活编排 |
| **架构影响** | - | 保持现有四项目架构不变 |
| **数据库** | 单一数据库 | 独立数据库，耦合度低 |

### 8.2 风险分析与缓解措施

| 风险 | 可能性 | 影响 | 缓解措施 |
|------|--------|------|---------|
| 学习曲线陡峭 | 中 | 中 | 先做小范围试点，逐步推广 |
| 与现有代码集成复杂度 | 中 | 高 | 采用渐进式迁移，保持现有 API 兼容 |
| 双数据库一致性 | 中 | 中 | 应用服务层桥接，最终一致性 |
| 性能影响 | 低 | 中 | 性能测试，连接池优化 |
| 前端开发复杂度 | 中 | 中 | 分阶段实现，先列表后设计器 |
| Elsa 生态成熟度 | 中 | 低 | 关注社区动态，积极参与贡献 |

---

## 9. 资源需求与结论

### 9.1 人力资源需求

| 角色 | 投入时间 | 职责 |
|------|---------|------|
| Backend Developer | 1 人 × 7-9 周 | Elsa 集成、Activity 开发、迁移 |
| Frontend Developer | 0.5-1 人 × 4-5 周 | 前端工作流模块、设计器 |
| QA Engineer | 0.5 人 × 3-4 周 | 测试和验证 |

### 9.2 技术资源

| 资源 | 链接/说明 |
|------|----------|
| Elsa 官方文档 | https://elsa-workflows.github.io/elsa-core/ |
| Elsa GitHub | https://github.com/elsa-workflows/elsa-core |
| workflow_for_elsa | Vue 3 工作流组件（可选） |
| NuGet 包 | Elsa.Workflow.Core 3.6.2+ |

### 9.3 结论与建议

**✅ 推荐采用 Elsa Workflow，理由如下：**

1. **完全兼容 .NET 10** - Elsa 3.6.2+ 原生支持 net10.0
2. **保持现有架构** - 四项目结构不变，仅在 Api 和 Application 层新增集成
3. **独立数据库** - 避免与现有业务数据耦合，降低迁移风险
4. **渐进式迁移** - 分阶段实施，风险可控
5. **Vue 3 深度集成** - 完整的 REST API，支持自定义 UI
6. **提升系统能力** - 可视化设计、版本管理、灵活编排

**建议立即启动 Phase 1（基础设施搭建），验证集成方案可行性。**

---

## 附录

### A. 快速参考 - NuGet 包清单

```xml
<!-- Elsa 核心 -->
<PackageReference Include="Elsa" Version="3.6.2" />
<PackageReference Include="Elsa.Workflows.Core" Version="3.6.2" />
<PackageReference Include="Elsa.Workflows.Runtime" Version="3.6.2" />
<PackageReference Include="Elsa.Workflows.Management" Version="3.6.2" />

<!-- EF Core 持久化 -->
<PackageReference Include="Elsa.Persistence.EFCore.Sqlite" Version="3.6.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.0" />

<!-- API -->
<PackageReference Include="Elsa.Workflows.Api" Version="3.6.2" />

<!-- 可选扩展 -->
<PackageReference Include="Elsa.Http" Version="3.6.2" />
<PackageReference Include="Elsa.Scheduling" Version="3.6.2" />
```

### B. 术语表

| 术语 | 说明 |
|------|------|
| Workflow Definition | 工作流定义（模板） |
| Workflow Instance | 工作流实例（运行中的工作流） |
| Activity | 活动（工作流中的执行单元） |
| Bookmark | 书签（工作流挂起和恢复的位置） |
| Trigger | 触发器（启动工作流的条件） |
| ManagementElsaDbContext | Elsa 管理数据上下文 |
| RuntimeElsaDbContext | Elsa 运行时数据上下文 |

### C. 相关文档

- [Elsa Workflow 官方文档](https://elsa-workflows.github.io/elsa-core/)
- [.NET 10 官方文档](https://learn.microsoft.com/zh-cn/dotnet/core/whats-new/dotnet-10)
- [AutoCodeForge 架构文档](../Wiki/02-架构设计.md)
