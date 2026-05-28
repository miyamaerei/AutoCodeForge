# Elsa Workflow 集成快速开始

**版本**: v1.0.0  
**作者**: AutoCodeForge Team  
**创建日期**: 2026-05-26

## 前置条件

- .NET 10 SDK
- Node.js v20+
- Vue 3.3+

## 目录

1. [概述](#概述)
2. [后端快速集成](#后端快速集成)
3. [前端快速集成](#前端快速集成)
4. [验证 Demo](#验证-demo)

---

## 概述

Elsa Workflow 是一个强大的工作流引擎，但需要注意：

| 特性 | 说明 |
|-----|-----|
| **持久化** | 只支持 **EF Core**（不支持 SqlSugar） |
| **数据库** | 支持 SQLite、SQL Server、PostgreSQL、MySQL |
| **.NET 10** | ✅ 完全支持（Elsa 3.6.2+） |

**我们的架构方案：**
- 业务数据库：SqlSugar（保持现有不变）
- Elsa 数据库：EF Core（独立数据库）
- 桥接服务：Application 层关联两个数据库

---

## 后端快速集成

### Step 1: 安装 NuGet 包

在 `AutoCodeForge.Api.csproj` 中添加：

```xml
<PackageReference Include="Elsa" Version="3.6.2" />
<PackageReference Include="Elsa.Workflows.Core" Version="3.6.2" />
<PackageReference Include="Elsa.Workflows.Runtime" Version="3.6.2" />
<PackageReference Include="Elsa.Workflows.Management" Version="3.6.2" />
<PackageReference Include="Elsa.Persistence.EntityFramework.Sqlite" Version="3.6.2" />
<PackageReference Include="Elsa.Workflows.Api" Version="3.6.2" />
```

### Step 2: 配置连接字符串

在 `appsettings.json` 中添加：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=autocodeforge.db",
    "ElsaConnection": "Data Source=elsa-workflows.db"
  }
}
```

### Step 3: 创建 Elsa 扩展配置

新建 `AutoCodeForge.Api/Extensions/ElsaServiceExtensions.cs`：

```csharp
using Elsa.Extensions;
using Elsa.Persistence.EntityFramework.Sqlite;
using AutoCodeForge.Application.Services;

namespace AutoCodeForge.Api.Extensions;

public static class ElsaServiceExtensions
{
    public static IServiceCollection AddElsaWorkflow(this IServiceCollection services, IConfiguration configuration)
    {
        var elsaConnectionString = configuration.GetConnectionString("ElsaConnection")
            ?? "Data Source=elsa-workflows.db";

        services.AddElsa(elsa =>
        {
            // 工作流管理
            elsa.UseWorkflowManagement(management =>
            {
                management.UseEntityFrameworkCore(ef =>
                {
                    ef.UseSqlite(elsaConnectionString);
                    ef.RunMigrations = true; // 开发环境自动迁移
                });
            });

            // 工作流运行时
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

            // 注册自定义 Activity
            elsa.AddActivitiesFrom(typeof(Application.Activities.HelloWorldActivity).Assembly);

            // 注册工作流
            elsa.AddWorkflow<Application.Workflows.HelloWorldWorkflow>();
        });

        // 注册桥接服务
        services.AddScoped<ElsaWorkflowService>();

        return services;
    }
}
```

### Step 4: 创建 HelloWorld Activity

新建 `AutoCodeForge.Application/Activities/HelloWorldActivity.cs`：

```csharp
using Elsa.Workflows;
using Elsa.Workflows.Attributes;

namespace AutoCodeForge.Application.Activities;

/// <summary>
/// HelloWorld Activity - 用于 Demo
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

### Step 5: 创建 HelloWorld Workflow

新建 `AutoCodeForge.Application/Workflows/HelloWorldWorkflow.cs`：

```csharp
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Contracts;
using AutoCodeForge.Application.Activities;

namespace AutoCodeForge.Application.Workflows;

/// <summary>
/// HelloWorld Workflow - 最小 Demo
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
                    new HelloWorldActivity
                    {
                        Message = new("Welcome to AutoCodeForge + Elsa!")
                    },
                    new WriteLine("=== Hello World Workflow Completed ===")
                }
            });
    }
}
```

### Step 6: 创建 Elsa 桥接服务

新建 `AutoCodeForge.Application/Services/ElsaWorkflowService.cs`：

```csharp
using Elsa.Workflows.Contracts;
using Elsa.Workflows.Management.Contracts;
using Elsa.Workflows.Runtime.Contracts;

namespace AutoCodeForge.Application.Services;

public class ElsaWorkflowService
{
    private readonly IWorkflowRegistry _workflowRegistry;
    private readonly IWorkflowInvoker _workflowInvoker;
    private readonly IWorkflowInstanceStore _workflowInstanceStore;

    public ElsaWorkflowService(
        IWorkflowRegistry workflowRegistry,
        IWorkflowInvoker workflowInvoker,
        IWorkflowInstanceStore workflowInstanceStore)
    {
        _workflowRegistry = workflowRegistry;
        _workflowInvoker = workflowInvoker;
        _workflowInstanceStore = workflowInstanceStore;
    }

    /// <summary>
    /// 启动 HelloWorld 工作流
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
    /// 获取所有工作流实例
    /// </summary>
    public async Task<IEnumerable<WorkflowInstance>> GetAllInstancesAsync()
    {
        return await _workflowInstanceStore.FindManyAsync();
    }
}
```

### Step 7: 创建 API 端点

新建 `AutoCodeForge.Api/Endpoints/ElsaWorkflowEndpoints.cs`：

```csharp
using AutoCodeForge.Application.Services;

namespace AutoCodeForge.Api.Endpoints;

public static class ElsaWorkflowEndpoints
{
    public static IEndpointRouteBuilder MapElsaWorkflowEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/elsa")
            .WithTags("Elsa Workflow")
            .RequireAuthorization();

        // 启动 HelloWorld
        group.MapPost("/hello-world", StartHelloWorld)
            .WithName("StartHelloWorld");

        // 获取工作流实例
        group.MapGet("/instances", GetAllInstances)
            .WithName("GetAllInstances");

        return builder;
    }

    private static async Task<Results<Ok<string>, BadRequest<string>>> StartHelloWorld(
        ElsaWorkflowService elsaWorkflowService)
    {
        try
        {
            var instanceId = await elsaWorkflowService.StartHelloWorldWorkflowAsync();
            return TypedResults.Ok(instanceId);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    private static async Task<Results<Ok<IEnumerable<WorkflowInstance>>, BadRequest<string>>> GetAllInstances(
        ElsaWorkflowService elsaWorkflowService)
    {
        try
        {
            var instances = await elsaWorkflowService.GetAllInstancesAsync();
            return TypedResults.Ok(instances);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }
}
```

### Step 8: 更新 Program.cs

```csharp
using AutoCodeForge.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ... 现有配置保持不变 ...

// 新增：Elsa Workflow
builder.Services.AddElsaWorkflow(builder.Configuration);

var app = builder.Build();

// ... 现有中间件保持不变 ...

// 新增：Elsa API
app.UseWorkflowsApi();
app.MapElsaWorkflowEndpoints();

app.Run();
```

---

## 前端快速集成

### Step 1: 创建 API 层

新建 `client/src/modules/workflow-center/api/workflow.api.ts`：

```typescript
import { request } from '@/lib/request'

export interface WorkflowInstance {
  id: string
  status: string
  createdAt: string
  startedAt?: string
  finishedAt?: string
}

export const workflowApi = {
  startHelloWorld: () => request.post<string>('/api/elsa/hello-world'),
  getInstances: () => request.get<WorkflowInstance[]>('/api/elsa/instances')
}
```

### Step 2: 创建 Pinia Store

新建 `client/src/modules/workflow-center/store/useWorkflowStore.ts`：

```typescript
import { defineStore } from 'pinia'
import { ref } from 'vue'
import { workflowApi, type WorkflowInstance } from '../api/workflow.api'

export const useWorkflowStore = defineStore('workflow', () => {
  const instances = ref<WorkflowInstance[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const fetchInstances = async () => {
    loading.value = true
    error.value = null
    try {
      instances.value = await workflowApi.getInstances()
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch instances'
    } finally {
      loading.value = false
    }
  }

  const startHelloWorld = async () => {
    loading.value = true
    error.value = null
    try {
      const instanceId = await workflowApi.startHelloWorld()
      await fetchInstances()
      return instanceId
    } catch (err: any) {
      error.value = err.message || 'Failed to start workflow'
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    instances,
    loading,
    error,
    fetchInstances,
    startHelloWorld
  }
})
```

### Step 3: 创建工作流列表页面

新建 `client/src/modules/workflow-center/views/WorkflowListView.vue`：

```vue
<template>
  <div class="workflow-list-view">
    <el-card class="header-card">
      <template #header>
        <div class="card-header">
          <h2>Elsa Workflow Demo</h2>
          <el-button type="primary" @click="handleStart" :loading="store.loading">
            启动 HelloWorld
          </el-button>
        </div>
      </template>
      <p class="description">最小 Demo - 测试 Elsa Workflow 是否正常工作</p>
    </el-card>

    <el-alert v-if="store.error" type="error" :message="store.error" show-icon class="alert" />

    <el-card class="list-card">
      <template #header>
        <div class="card-header">
          <span>工作流实例</span>
          <el-button size="small" @click="store.fetchInstances">刷新</el-button>
        </div>
      </template>
      <el-table :data="store.instances" v-loading="store.loading" stripe>
        <el-table-column prop="id" label="实例 ID" width="250" show-overflow-tooltip />
        <el-table-column prop="status" label="状态" width="120">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)">{{ row.status }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间">
          <template #default="{ row }">{{ formatDate(row.createdAt) }}</template>
        </el-table-column>
      </el-table>
      <el-empty v-if="!store.loading && store.instances.length === 0" description="暂无工作流实例" />
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useWorkflowStore } from '../store/useWorkflowStore'
import { ElMessage } from 'element-plus'

const store = useWorkflowStore()

onMounted(() => {
  store.fetchInstances()
})

const handleStart = async () => {
  try {
    const instanceId = await store.startHelloWorld()
    ElMessage.success(`工作流已启动: ${instanceId}`)
  } catch (err) {
    ElMessage.error('启动失败')
  }
}

const getStatusType = (status: string) => {
  const map: Record<string, any> = {
    Running: 'primary',
    Completed: 'success',
    Faulted: 'danger',
    Suspended: 'warning'
  }
  return map[status] || 'info'
}

const formatDate = (dateStr: string) => new Date(dateStr).toLocaleString()
</script>

<style scoped>
.workflow-list-view {
  padding: 24px;
}

.header-card {
  margin-bottom: 24px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.description {
  margin: 0;
  color: #666;
}

.alert {
  margin-bottom: 24px;
}

.list-card {
  margin-bottom: 24px;
}
</style>
```

### Step 4: 配置路由

新建 `client/src/modules/workflow-center/index.ts`：

```typescript
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

---

## 验证 Demo

### 1. 启动后端

```bash
cd server/src/AutoCodeForge.Api
dotnet restore
dotnet run
```

### 2. 启动前端

```bash
cd client
npm install
npm run dev
```

### 3. 验证步骤

1. **访问 Swagger UI**：`https://localhost:7123/swagger`
2. **调用 API**：执行 `POST /api/elsa/hello-world`
3. **访问前端**：`http://localhost:5173/workflow-center`
4. **点击按钮**："启动 HelloWorld"
5. **查看结果**：工作流实例列表应该显示已完成的实例

---

## 下一步

Demo 验证成功后，请参考：
- [Elsa Workflow 实施问答与最佳实践](./Elsa_Workflow_实施问答与最佳实践.md)
- [技术选型 Elsa Workflow](./技术选型_Elsa_Workflow.md)
