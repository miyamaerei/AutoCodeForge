# Microsoft Agent Framework 编排体系与前端对接指南

## 一、核心概念概述

### 1.1 Agent（智能代理）

**定义**：AI 驱动的组件，具有特定角色、指令集和可选工具。

**核心特性**：
- 专注于单一任务领域
- 具备特定领域知识
- 可调用外部工具增强能力

**示例**：
```csharp
var agent = new ChatClientAgent(chatClient, "你是一个代码审查专家，专注于C#代码质量检查。");
```

### 1.2 Executor（执行器）

**定义**：包装 Agent 的处理单元，将类型化输入转换为类型化输出。

**核心职责**：
- 接收结构化输入
- 调用 Agent 处理
- 返回结构化输出
- 处理错误和异常

**类型系统**：
```csharp
public interface IExecutor<in TInput, out TOutput>
{
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken);
}
```

### 1.3 Workflow（工作流）

**定义**：由边连接的 Executor 有向图，决定执行顺序和路由。

**工作流类型**：

| 类型 | 适用场景 | 特点 |
|------|----------|------|
| **顺序编排** | 文档审核、多阶段推理 | 线性执行，步骤依赖 |
| **并发编排** | 头脑风暴、集成推理 | 并行执行，结果聚合 |
| **条件编排** | 决策分支、动态路由 | 根据条件选择路径 |

**构建方式**：
```csharp
// 顺序工作流
var sequentialWorkflow = AgentWorkflowBuilder.BuildSequential(executors);

// 并发工作流  
var concurrentWorkflow = AgentWorkflowBuilder.BuildConcurrent(executors);

// 自定义工作流
var customWorkflow = AgentWorkflowBuilder.Create()
    .AddExecutor("step1", executor1)
    .AddExecutor("step2", executor2)
    .AddEdge("step1", "step2", condition)
    .Build();
```

### 1.4 Edge（边）

**定义**：两个 Executor 之间的连接，支持无条件或条件路由。

**路由类型**：

| 类型 | 说明 | 示例 |
|------|------|------|
| **无条件路由** | 直接传递到下一个节点 | `AddEdge("A", "B")` |
| **条件路由** | 根据输出值决定路径 | `AddEdge("A", "B", ctx => ctx.Output.Score > 0.8)` |
| **Switch路由** | 多分支选择 | `AddSwitchEdge("A", cases)` |

**条件路由示例**：
```csharp
workflow.AddEdge("analyzer", "reviewer", context => 
{
    var result = context.GetOutput<AnalysisResult>("analyzer");
    return result.HasIssues; // 有问题才进入审核
});

workflow.AddEdge("analyzer", "completion", context => 
{
    var result = context.GetOutput<AnalysisResult>("analyzer");
    return !result.HasIssues; // 无问题直接完成
});
```

### 1.5 Context Provider（上下文提供者）

**定义**：在运行时将上下文信息注入 Agent 提示词。

**上下文类型**：

| 类型 | 来源 | 用途 |
|------|------|------|
| **RAG 检索结果** | 知识库 | 提供领域知识 |
| **历史对话** | 对话记录 | 保持对话连贯性 |
| **用户信息** | 身份服务 | 个性化响应 |
| **工具结果** | 工具调用 | 补充实时数据 |

**实现方式**：
```csharp
public class RagContextProvider : IContextProvider
{
    private readonly IKnowledgeBase _knowledgeBase;
    
    public async Task<ContextData> ProvideAsync(WorkflowContext context)
    {
        var query = context.CurrentInput.ToString();
        var documents = await _knowledgeBase.SearchAsync(query);
        
        return new ContextData
        {
            Key = "rag_results",
            Content = string.Join("\n", documents.Select(d => d.Content))
        };
    }
}
```

---

## 二、前端对接架构

### 2.1 整体架构设计

```
┌─────────────────────────────────────────────────────────────────┐
│                        Frontend Layer                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐    │
│  │ Workflow    │  │ Agent       │  │ Task Monitor        │    │
│  │ Designer    │  │ Console     │  │ Dashboard           │    │
│  └──────┬──────┘  └──────┬──────┘  └──────────┬──────────┘    │
│         │                │                     │                │
└─────────┼────────────────┼─────────────────────┼────────────────┘
          │                │                     │
          ▼                ▼                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                      API Gateway                               │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ /api/workflows    /api/executors    /api/tasks         │   │
│  └─────────────────────────────────────────────────────────┘   │
└───────────────────────────┬─────────────────────────────────────┘
                            │
          ┌─────────────────┼─────────────────┐
          ▼                 ▼                 ▼
┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│ Workflow         │ │ Agent            │ │ Task             │
│ Orchestrator     │ │ Service          │ │ Manager          │
│ Service          │ │                  │ │                  │
└──────────────────┘ └──────────────────┘ └──────────────────┘
```

### 2.2 核心 API 接口设计

#### 2.2.1 Workflow 管理接口

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | `/api/workflows` | 获取所有工作流列表 |
| GET | `/api/workflows/{id}` | 获取工作流详情 |
| POST | `/api/workflows` | 创建新工作流 |
| PUT | `/api/workflows/{id}` | 更新工作流 |
| DELETE | `/api/workflows/{id}` | 删除工作流 |

**创建工作流请求体**：
```json
{
  "name": "代码审查工作流",
  "description": "自动化代码审查流程",
  "executors": [
    {
      "id": "analyzer",
      "type": "CodeAnalyzer",
      "config": {
        "rules": ["style", "security", "performance"]
      }
    },
    {
      "id": "reviewer", 
      "type": "HumanReview",
      "config": {
        "required": true
      }
    }
  ],
  "edges": [
    {
      "from": "analyzer",
      "to": "reviewer",
      "condition": {
        "type": "expression",
        "value": "output.hasIssues === true"
      }
    },
    {
      "from": "analyzer",
      "to": "completion",
      "condition": {
        "type": "expression",
        "value": "output.hasIssues === false"
      }
    }
  ],
  "contextProviders": ["RagContextProvider", "UserContextProvider"]
}
```

#### 2.2.2 工作流执行接口

| 方法 | 路径 | 描述 |
|------|------|------|
| POST | `/api/workflows/{id}/execute` | 执行工作流 |
| GET | `/api/workflows/{id}/instances` | 获取工作流实例列表 |
| GET | `/api/workflows/{id}/instances/{instanceId}` | 获取工作流实例详情 |
| POST | `/api/workflows/instances/{instanceId}/pause` | 暂停工作流 |
| POST | `/api/workflows/instances/{instanceId}/resume` | 恢复工作流 |
| POST | `/api/workflows/instances/{instanceId}/terminate` | 终止工作流 |

**执行工作流请求体**：
```json
{
  "input": {
    "type": "CodeReviewRequest",
    "data": {
      "repositoryUrl": "https://github.com/example/repo",
      "commitHash": "a1b2c3d",
      "reviewerId": "user-123"
    }
  },
  "context": {
    "userId": "user-123",
    "metadata": {
      "priority": "high",
      "deadline": "2024-12-31T23:59:59Z"
    }
  }
}
```

**执行工作流响应**：
```json
{
  "instanceId": "workflow-instance-123",
  "workflowId": "code-review-workflow",
  "status": "Running",
  "currentStep": "analyzer",
  "startedAt": "2024-01-15T10:00:00Z",
  "input": { /* ... */ },
  "output": null,
  "events": []
}
```

#### 2.2.3 实时事件接口（SSE）

| 方法 | 路径 | 描述 |
|------|------|------|
| GET | `/api/workflows/instances/{instanceId}/events` | 订阅工作流事件（SSE） |

**事件类型**：

| 事件类型 | 描述 | 数据结构 |
|----------|------|----------|
| `StepStarted` | 步骤开始执行 | `{ stepId, timestamp }` |
| `StepCompleted` | 步骤执行完成 | `{ stepId, output, duration }` |
| `StepFailed` | 步骤执行失败 | `{ stepId, error, retryCount }` |
| `WorkflowPaused` | 工作流暂停 | `{ reason }` |
| `WorkflowResumed` | 工作流恢复 | `{ timestamp }` |
| `WorkflowCompleted` | 工作流完成 | `{ output, totalDuration }` |
| `WorkflowTerminated` | 工作流终止 | `{ reason }` |
| `HumanApprovalRequired` | 需要人工审批 | `{ stepId, taskId, deadline }` |

---

## 三、前端组件设计

### 3.1 Workflow Designer（工作流设计器）

**功能特性**：
- 可视化拖拽设计工作流
- 支持 Executor 节点添加/删除/编辑
- 支持 Edge 连接创建
- 条件路由配置
- 工作流预览和验证

**组件结构**：
```typescript
interface WorkflowDesignerProps {
  workflow: WorkflowDefinition;
  onSave: (workflow: WorkflowDefinition) => void;
  onExecute: (workflowId: string) => void;
}

interface ExecutorNode {
  id: string;
  type: string;
  label: string;
  position: { x: number; y: number };
  config: Record<string, unknown>;
}

interface EdgeConnection {
  id: string;
  from: string;
  to: string;
  condition?: ConditionConfig;
}
```

**设计器状态管理**：
```typescript
const [nodes, setNodes] = useState<ExecutorNode[]>([]);
const [edges, setEdges] = useState<EdgeConnection[]>([]);
const [selectedNode, setSelectedNode] = useState<string | null>(null);
const [draggingNode, setDraggingNode] = useState<string | null>(null);
```

### 3.2 Agent Console（Agent 控制台）

**功能特性**：
- 实时对话界面
- 支持工具调用展示
- 上下文信息展示
- 多轮对话历史

**组件结构**：
```typescript
interface AgentConsoleProps {
  workflowInstanceId: string;
  onComplete: () => void;
}

interface ChatMessage {
  id: string;
  role: 'user' | 'agent' | 'system' | 'tool';
  content: string;
  timestamp: Date;
  toolCall?: ToolCallInfo;
}

interface ToolCallInfo {
  toolName: string;
  parameters: Record<string, unknown>;
  result?: unknown;
}
```

### 3.3 Task Monitor（任务监控面板）

**功能特性**：
- 工作流实例列表
- 实时状态更新
- 执行进度可视化
- 错误告警

**组件结构**：
```typescript
interface TaskMonitorProps {
  filters?: FilterOptions;
}

interface WorkflowInstance {
  id: string;
  workflowId: string;
  workflowName: string;
  status: 'Pending' | 'Running' | 'Paused' | 'Completed' | 'Failed' | 'Terminated';
  currentStep: string;
  progress: number;
  startedAt: Date;
  completedAt?: Date;
  errorMessage?: string;
}
```

---

## 四、前端对接实现方案

### 4.1 API 层封装

**创建统一的 API 服务**：
```typescript
// src/api/workflow.ts
import axios from 'axios';

const api = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

export const workflowApi = {
  // 获取工作流列表
  async getWorkflows(): Promise<WorkflowDefinition[]> {
    const response = await api.get('/workflows');
    return response.data;
  },

  // 获取工作流详情
  async getWorkflow(id: string): Promise<WorkflowDefinition> {
    const response = await api.get(`/workflows/${id}`);
    return response.data;
  },

  // 创建工作流
  async createWorkflow(data: WorkflowDefinition): Promise<WorkflowDefinition> {
    const response = await api.post('/workflows', data);
    return response.data;
  },

  // 更新工作流
  async updateWorkflow(id: string, data: Partial<WorkflowDefinition>): Promise<WorkflowDefinition> {
    const response = await api.put(`/workflows/${id}`, data);
    return response.data;
  },

  // 删除工作流
  async deleteWorkflow(id: string): Promise<void> {
    await api.delete(`/workflows/${id}`);
  },

  // 执行工作流
  async executeWorkflow(id: string, input: WorkflowInput): Promise<WorkflowInstance> {
    const response = await api.post(`/workflows/${id}/execute`, input);
    return response.data;
  },

  // 获取工作流实例
  async getWorkflowInstances(workflowId?: string): Promise<WorkflowInstance[]> {
    const params = workflowId ? { workflowId } : {};
    const response = await api.get('/workflows/instances', { params });
    return response.data;
  },

  // 获取工作流实例详情
  async getWorkflowInstance(instanceId: string): Promise<WorkflowInstance> {
    const response = await api.get(`/workflows/instances/${instanceId}`);
    return response.data;
  },

  // 订阅工作流事件
  subscribeToEvents(instanceId: string, callback: (event: WorkflowEvent) => void): EventSource {
    const eventSource = new EventSource(`/api/workflows/instances/${instanceId}/events`);
    
    eventSource.onmessage = (event) => {
      const workflowEvent = JSON.parse(event.data) as WorkflowEvent;
      callback(workflowEvent);
    };

    return eventSource;
  },
};
```

### 4.2 状态管理（Pinia Store）

**创建工作流状态管理**：
```typescript
// src/stores/workflowStore.ts
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { workflowApi } from '../api/workflow';
import type { WorkflowDefinition, WorkflowInstance, WorkflowEvent } from '../types';

export const useWorkflowStore = defineStore('workflow', () => {
  // 工作流定义列表
  const workflows = ref<WorkflowDefinition[]>([]);
  
  // 当前选中的工作流
  const currentWorkflow = ref<WorkflowDefinition | null>(null);
  
  // 工作流实例列表
  const instances = ref<WorkflowInstance[]>([]);
  
  // 当前执行的工作流实例
  const currentInstance = ref<WorkflowInstance | null>(null);
  
  // 事件订阅
  const eventSources = ref<Map<string, EventSource>>(new Map());

  // 获取运行中的实例
  const runningInstances = computed(() => 
    instances.value.filter(i => i.status === 'Running')
  );

  // 获取失败的实例
  const failedInstances = computed(() => 
    instances.value.filter(i => i.status === 'Failed')
  );

  // 加载工作流列表
  async function loadWorkflows() {
    workflows.value = await workflowApi.getWorkflows();
  }

  // 加载工作流详情
  async function loadWorkflow(id: string) {
    currentWorkflow.value = await workflowApi.getWorkflow(id);
  }

  // 创建工作流
  async function createWorkflow(data: WorkflowDefinition) {
    const workflow = await workflowApi.createWorkflow(data);
    workflows.value.push(workflow);
    return workflow;
  }

  // 更新工作流
  async function updateWorkflow(id: string, data: Partial<WorkflowDefinition>) {
    const updated = await workflowApi.updateWorkflow(id, data);
    const index = workflows.value.findIndex(w => w.id === id);
    if (index !== -1) {
      workflows.value[index] = updated;
    }
    if (currentWorkflow.value?.id === id) {
      currentWorkflow.value = updated;
    }
    return updated;
  }

  // 删除工作流
  async function deleteWorkflow(id: string) {
    await workflowApi.deleteWorkflow(id);
    workflows.value = workflows.value.filter(w => w.id !== id);
    if (currentWorkflow.value?.id === id) {
      currentWorkflow.value = null;
    }
  }

  // 执行工作流
  async function executeWorkflow(workflowId: string, input: WorkflowInput) {
    const instance = await workflowApi.executeWorkflow(workflowId, input);
    instances.value.unshift(instance);
    currentInstance.value = instance;
    
    // 订阅事件
    subscribeToInstanceEvents(instance.id);
    
    return instance;
  }

  // 加载工作流实例
  async function loadInstances(workflowId?: string) {
    instances.value = await workflowApi.getWorkflowInstances(workflowId);
  }

  // 加载工作流实例详情
  async function loadInstance(instanceId: string) {
    currentInstance.value = await workflowApi.getWorkflowInstance(instanceId);
  }

  // 订阅工作流实例事件
  function subscribeToInstanceEvents(instanceId: string) {
    // 取消之前的订阅
    const existing = eventSources.value.get(instanceId);
    if (existing) {
      existing.close();
    }

    const eventSource = workflowApi.subscribeToEvents(instanceId, (event) => {
      // 更新实例状态
      const index = instances.value.findIndex(i => i.id === instanceId);
      if (index !== -1) {
        updateInstanceFromEvent(instances.value[index], event);
      }
      
      // 更新当前实例
      if (currentInstance.value?.id === instanceId) {
        updateInstanceFromEvent(currentInstance.value, event);
      }
    });

    eventSources.value.set(instanceId, eventSource);
  }

  // 根据事件更新实例
  function updateInstanceFromEvent(instance: WorkflowInstance, event: WorkflowEvent) {
    switch (event.type) {
      case 'StepStarted':
        instance.currentStep = event.data.stepId;
        break;
      case 'StepCompleted':
        instance.progress = calculateProgress(instance);
        break;
      case 'StepFailed':
        instance.status = 'Failed';
        instance.errorMessage = event.data.error;
        break;
      case 'WorkflowPaused':
        instance.status = 'Paused';
        break;
      case 'WorkflowResumed':
        instance.status = 'Running';
        break;
      case 'WorkflowCompleted':
        instance.status = 'Completed';
        instance.completedAt = new Date();
        break;
      case 'WorkflowTerminated':
        instance.status = 'Terminated';
        instance.completedAt = new Date();
        break;
    }
  }

  // 取消事件订阅
  function unsubscribeFromEvents(instanceId: string) {
    const eventSource = eventSources.value.get(instanceId);
    if (eventSource) {
      eventSource.close();
      eventSources.value.delete(instanceId);
    }
  }

  // 清理所有订阅
  function cleanup() {
    eventSources.value.forEach(source => source.close());
    eventSources.value.clear();
  }

  return {
    // State
    workflows,
    currentWorkflow,
    instances,
    currentInstance,
    
    // Computed
    runningInstances,
    failedInstances,
    
    // Actions
    loadWorkflows,
    loadWorkflow,
    createWorkflow,
    updateWorkflow,
    deleteWorkflow,
    executeWorkflow,
    loadInstances,
    loadInstance,
    subscribeToInstanceEvents,
    unsubscribeFromEvents,
    cleanup,
  };
});
```

### 4.3 前端页面路由配置

**路由结构**：
```typescript
// src/router/index.ts
import { createRouter, createWebHistory } from 'vue-router';
import type { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/workflows',
    name: 'WorkflowList',
    component: () => import('../views/WorkflowList.vue'),
  },
  {
    path: '/workflows/create',
    name: 'WorkflowCreate',
    component: () => import('../views/WorkflowCreate.vue'),
  },
  {
    path: '/workflows/:id',
    name: 'WorkflowDetail',
    component: () => import('../views/WorkflowDetail.vue'),
  },
  {
    path: '/workflows/:id/designer',
    name: 'WorkflowDesigner',
    component: () => import('../views/WorkflowDesigner.vue'),
  },
  {
    path: '/instances',
    name: 'InstanceList',
    component: () => import('../views/InstanceList.vue'),
  },
  {
    path: '/instances/:id',
    name: 'InstanceDetail',
    component: () => import('../views/InstanceDetail.vue'),
  },
  {
    path: '/instances/:id/console',
    name: 'InstanceConsole',
    component: () => import('../views/InstanceConsole.vue'),
  },
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
```

---

## 五、类型定义

### 5.1 核心类型

```typescript
// src/types/workflow.ts

// 工作流定义
export interface WorkflowDefinition {
  id: string;
  name: string;
  description?: string;
  executors: ExecutorDefinition[];
  edges: EdgeDefinition[];
  contextProviders: string[];
  createdAt: Date;
  updatedAt: Date;
}

// Executor 定义
export interface ExecutorDefinition {
  id: string;
  type: string;
  label: string;
  config: Record<string, unknown>;
}

// Edge 定义
export interface EdgeDefinition {
  id: string;
  from: string;
  to: string;
  condition?: ConditionConfig;
}

// 条件配置
export interface ConditionConfig {
  type: 'expression' | 'switch';
  value: string;
  cases?: SwitchCase[];
}

// Switch 分支
export interface SwitchCase {
  value: string;
  target: string;
}

// 工作流输入
export interface WorkflowInput {
  type: string;
  data: Record<string, unknown>;
  context?: WorkflowContext;
}

// 工作流上下文
export interface WorkflowContext {
  userId: string;
  metadata?: Record<string, unknown>;
}

// 工作流实例
export interface WorkflowInstance {
  id: string;
  workflowId: string;
  workflowName: string;
  status: WorkflowStatus;
  currentStep: string;
  progress: number;
  input: WorkflowInput;
  output?: unknown;
  startedAt: Date;
  completedAt?: Date;
  errorMessage?: string;
}

// 工作流状态
export type WorkflowStatus = 'Pending' | 'Running' | 'Paused' | 'Completed' | 'Failed' | 'Terminated';

// 工作流事件
export interface WorkflowEvent {
  id: string;
  instanceId: string;
  type: WorkflowEventType;
  data: Record<string, unknown>;
  timestamp: Date;
}

// 工作流事件类型
export type WorkflowEventType = 
  | 'StepStarted'
  | 'StepCompleted'
  | 'StepFailed'
  | 'WorkflowPaused'
  | 'WorkflowResumed'
  | 'WorkflowCompleted'
  | 'WorkflowTerminated'
  | 'HumanApprovalRequired';
```

---

## 六、部署与集成建议

### 6.1 开发环境配置

**环境变量**：
```env
# API 配置
VITE_API_BASE_URL=http://localhost:5000/api

# 认证配置
VITE_AUTH_ENABLED=true
VITE_AUTH_PROVIDER=azure

# 实时通信
VITE_SSE_ENABLED=true
```

### 6.2 CORS 配置

**后端 CORS 设置**：
```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition");
    });
});
```

### 6.3 性能优化建议

1. **缓存策略**：
   - 缓存工作流定义（不常变化）
   - 缓存 Agent 配置
   - 使用 CDN 加速静态资源

2. **批量加载**：
   - 工作流列表分页
   - 实例列表按需加载
   - 事件订阅延迟连接

3. **异步处理**：
   - 工作流执行异步化
   - 长时间运行任务后台处理
   - 使用 WebSocket/SSE 推送状态

---

## 七、总结

Microsoft Agent Framework 提供了强大的编排能力，通过 Agent、Executor、Workflow、Edge 和 Context Provider 的组合，可以构建复杂的多阶段 AI 工作流。

**前端对接关键点**：

1. **API 层**：使用 REST API 进行工作流管理和执行控制
2. **实时通信**：使用 SSE 订阅工作流事件，实现实时状态更新
3. **状态管理**：使用 Pinia 管理全局状态，确保数据一致性
4. **可视化**：提供工作流设计器、执行控制台和监控面板

**推荐实践**：

- 使用 TypeScript 确保类型安全
- 实现错误边界和重试机制
- 提供清晰的用户反馈
- 支持人工介入和审批流程

---

## 八、推荐前端库

基于 Microsoft Agent Framework 编排体系，以下是推荐的 Vue 3 工作流编排前端库：

### 8.1 Vue Flow（推荐 ⭐⭐⭐⭐⭐）

**Vue Flow** 是目前最成熟、最活跃的 Vue 3 流程图编辑器库。

| 特性 | 说明 |
|------|------|
| **GitHub Stars** | 6.8k+ |
| **维护状态** | 活跃维护 |
| **核心能力** | 拖拽节点、连线编辑、缩放平移、自定义节点、边 |
| **官方插件** | Background、Minimap、Controls、EdgeTypes |
| **Vue 版本** | 仅支持 Vue 3 |

**安装**：

```bash
npm install @vue-flow/core @vue-flow/background @vue-flow/controls @vue-flow/minimap
```

**快速开始**：

```vue
<template>
  <VueFlow v-model:nodes="nodes" v-model:edges="edges">
    <Background pattern-color="#aaa" :gap="16" />
    <MiniMap />
    <Controls />
  </VueFlow>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { VueFlow } from '@vue-flow/core'
import { Background } from '@vue-flow/background'
import { MiniMap } from '@vue-flow/minimap'
import { Controls } from '@vue-flow/controls'

const nodes = ref([
  { id: '1', type: 'input', label: 'Start', position: { x: 250, y: 5 } },
  { id: '2', label: 'Agent Executor', position: { x: 100, y: 100 } },
  { id: '3', label: 'Next Step', position: { x: 400, y: 100 } },
])

const edges = ref([
  { id: 'e1-2', source: '1', target: '2', animated: true },
  { id: 'e1-3', source: '1', target: '3' },
])
</script>

<style>
@import "@vue-flow/core/dist/style.css";
@import "@vue-flow/core/dist/theme-default.css";
</style>
```

**与 Elsa 集成包**：

```bash
npm install workflow_for_elsa
```

专门为 Elsa Workflow 设计的 Vue Flow 封装，支持双向格式转换。

### 8.2 Drawflow（推荐 ⭐⭐⭐⭐）

**Drawflow** 是一个轻量级的零依赖流程图库，支持 Vue 3。

| 特性 | 说明 |
|------|------|
| **依赖数量** | 零依赖（Vanilla JS） |
| **移动端支持** | ✅ 原生支持 |
| **模块化** | 支持多个独立工作流模块 |
| **API 丰富** | 完整的事件和方法体系 |

**安装**：

```bash
npm install drawflow
npm install -D @types/drawflow
```

**Vue 3 集成**：

```vue
<template>
  <div ref="editorRef"></div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import Drawflow from 'drawflow'
import styleDrawflow from 'drawflow/dist/drawflow.min.css'

const editorRef = ref<HTMLElement>()

onMounted(() => {
  const editor = new Drawflow(editorRef.value, {
    position_x: 0,
    position_y: 0,
    editor_mode: 'edit'
  })
  
  editor.start()
  
  // 添加节点
  editor.addNode('task', 0, 1, 150, 100, 'task', {}, 'Task Node')
})
</script>
```

### 8.3 vue3-super-flow（推荐 ⭐⭐⭐）

**vue3-super-flow** 是 vue-super-flow 的 Vue 3 移植版。

| 特性 | 说明 |
|------|------|
| **GitHub** | Fork 自经典 vue-super-flow |
| **右键菜单** | 支持节点、连线、画布右键菜单 |
| **辅助线** | 拖拽对齐辅助线 |
| **节点连线** | 灵活的单节点多输入/输出 |

**安装**：

```bash
npm install vue3-super-flow
```

### 8.4 LogicFlow（推荐 ⭐⭐⭐⭐）

**LogicFlow** 是一款由滴滴开源的流程图编辑框架，提供 Vue 和 React 双版本。

| 特性 | 说明 |
|------|------|
| **开源背景** | 滴滴出品 |
| **自定义能力** | 强大的节点、边样式自定义 |
| **插件体系** | 拖拽面板、迷你地图等插件 |
| **执行引擎** | 支持浏览器端执行流程图逻辑 |

**安装**：

```bash
npm install @logicflow/core
npm install @logicflow/extension  # 扩展包
```

**Vue 3 集成**：

```vue
<template>
  <div ref="containerRef" class="container"></div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import LogicFlow from '@logicflow/core'
import '@logicflow/core/es/index.css'
import { DndPanel } from '@logicflow/extension'

const containerRef = ref<HTMLElement>()

onMounted(() => {
  const lf = new LogicFlow({
    container: containerRef.value,
    grid: true,
    plugins: [DndPanel]
  })
  
  lf.render()
})
</script>
```

### 8.5 AI Agent 管理仪表盘

针对 AI Agent 工作流编排，以下是两个值得关注的专用仪表盘：

#### 8.5.1 agent-board

**agent-board** 是一个 AI 编码 Agent 的可视化管理系统。

| 特性 | 说明 |
|------|------|
| **任务看板** | Kanban 风格任务管理 |
| **实时聊天** | 与 Agent 实时对话 |
| **Diff 查看** | 代码变更对比 |
| **一键 PR** | 直接从仪表盘创建 PR |
| **MCP 集成** | 支持 Model Context Protocol |

**快速开始**：

```bash
npx ai-agent-board
```

#### 8.5.2 nats-agent-dashboard

**nats-agent-dashboard** 是基于 NATS 的 AI Agent 网络统一仪表盘。

| 特性 | 说明 |
|------|------|
| **通信协议** | NATS over WebSocket |
| **无后端** | 纯静态 Vue 3 应用 |
| **实时流** | Agent 发现、检测、提示 |
| **会话控制** | 创建、停止 Agent 会话 |

**快速开始**：

```bash
npx @m64/nats-agent-dashboard
```

### 8.6 库对比总结

| 库 | 适用场景 | 依赖大小 | 学习曲线 | 维护状态 |
|----|----------|----------|----------|----------|
| **Vue Flow** | 专业工作流设计器 | 中等 | 中等 | ⭐⭐⭐⭐⭐ |
| **workflow_for_elsa** | Elsa 专用 | 依赖 Vue Flow | 低 | ⭐⭐⭐⭐ |
| **LogicFlow** | 企业级流程图 | 中等 | 中等 | ⭐⭐⭐⭐ |
| **Drawflow** | 轻量级编辑器 | 零依赖 | 低 | ⭐⭐⭐ |
| **vue3-super-flow** | 快速原型 | 小 | 低 | ⭐⭐⭐ |

---

## 九、集成建议

### 9.1 推荐技术栈

**Microsoft Agent Framework + Vue 3 编排前端**：

```
前端技术栈：
├── Vue 3 (Composition API + TypeScript)
├── Pinia (状态管理)
├── Vue Router (路由管理)
├── Vue Flow (工作流设计器) ← 推荐
├── Element Plus / Naive UI (UI 组件库)
└── Axios (HTTP 客户端)

后端技术栈：
├── ASP.NET Core 8.0
├── Microsoft.Agents.AI
├── Microsoft.Agents.AI.Workflows
└── Entity Framework Core (SQL Server)
```

### 9.2 项目结构建议

```
src/
├── components/
│   ├── workflow/
│   │   ├── WorkflowDesigner.vue      # 工作流设计器
│   │   ├── nodes/
│   │   │   ├── AgentNode.vue         # Agent 节点
│   │   │   ├── ExecutorNode.vue      # Executor 节点
│   │   │   ├── ConditionNode.vue     # 条件节点
│   │   │   └── StartEndNode.vue      # 开始/结束节点
│   │   ├── edges/
│   │   │   ├── DefaultEdge.vue       # 默认连线
│   │   │   └── ConditionalEdge.vue   # 条件连线
│   │   └── TaskMonitor.vue           # 任务监控
│   ├── agent/
│   │   ├── AgentConsole.vue           # Agent 控制台
│   │   └── ChatPanel.vue             # 实时对话
│   └── common/
│       ├── NodeConfig.vue            # 节点配置面板
│       └── EdgeEditor.vue            # 边编辑器
├── stores/
│   ├── workflowStore.ts              # 工作流状态
│   ├── agentStore.ts                 # Agent 状态
│   └── taskStore.ts                  # 任务状态
├── api/
│   ├── workflow.ts                   # 工作流 API
│   ├── agent.ts                      # Agent API
│   └── task.ts                       # 任务 API
├── types/
│   ├── workflow.ts                   # 工作流类型
│   ├── agent.ts                      # Agent 类型
│   └── task.ts                       # 任务类型
└── views/
    ├── WorkflowList.vue              # 工作流列表
    ├── WorkflowDesigner.vue          # 设计器视图
    ├── InstanceList.vue             # 实例列表
    └── InstanceConsole.vue          # 实例控制台
```

### 9.3 关键实现要点

1. **数据双向绑定**：Vue Flow 的 `v-model:nodes` 和 `v-model:edges` 实现设计器与应用状态同步

2. **实时事件订阅**：使用 EventSource（SSE）订阅工作流实例事件流

3. **持久化策略**：设计器配置 → JSON → 后端存储，执行时加载完整工作流定义

4. **权限控制**：工作流设计器仅管理员可用，执行监控所有用户可访问