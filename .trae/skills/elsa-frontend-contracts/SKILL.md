---
name: "elsa-frontend-contracts"
description: "Research API contract design between Vue 3 frontend and Elsa backend, including endpoints, DTOs, and state management. Invoke when designing Elsa frontend integration or API contracts."
---

# Elsa Frontend Contracts

This skill researches how to design API contracts and frontend integration for Elsa Workflow.

## Knowledge Areas

### 1. API 路径规范

**现有规范（参考）：**
```
/api/{module}/{entity}
/api/task/executions
/api/agent/tasks
```

**Elsa 集成后：**
```
/api/elsa/workflows           → 工作流定义 CRUD
/api/elsa/instances           → 工作流实例
/api/elsa/execute/{workflowId} → 触发执行
/api/elsa/bookmark/{id}/resume → 恢复书签
```

**研究问题：**
- 是否需要 `/api/elsa/` 前缀？
- 与现有 API 模块的路径一致性？

### 2. DTO 设计规范

**命名规范：**
```
ElsaWorkflowDto        → 工作流定义
ElsaInstanceDto        → 工作流实例
ElsaExecutionRequest   → 执行请求
ElsaBookmarkResumeDto   → 书签恢复
```

**通用字段：**
```csharp
public class ElsaDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

**研究问题：**
- 是否复用现有的 ApiResult<T> 包装？
- 分页、排序规范？
- 错误码如何统一？

### 3. 前端状态管理

**选项 A：纳入现有 Store**
```typescript
// stores/task.ts - 扩展
export const useTaskStore = defineStore('task', () => {
  const elsaWorkflows = ref<Workflow[]>([])
  // ...
})
```

**选项 B：独立 Elsa Store**
```typescript
// stores/elsa.ts
export const useElsaStore = defineStore('elsa', () => {
  const workflows = ref<Workflow[]>([])
  const instances = ref<Instance[]>([])
})
```

**研究问题：**
- Elsa 状态是否独立管理？
- 如何与现有 Store 交互？

### 4. Vue Flow / LogicFlow 集成

**节点与 Activity 映射：**
```
Vue Flow Node Type    → Elsa Activity
─────────────────────────────────────
task-node            → TaskExecutionActivity
human-gate-node      → HumanGateActivity
agent-node           → AgentExecutionActivity
```

**JSON 序列化：**
```json
{
  "id": "node-1",
  "type": "task-node",
  "data": {
    "activityType": "TaskExecutionActivity",
    "properties": {
      "taskName": "执行任务"
    }
  },
  "position": { "x": 100, "y": 200 }
}
```

**研究问题：**
- 设计器 JSON 格式与 Elsa 定义格式的转换？
- 节点属性面板设计？
- 拖拽操作如何转换为工作流定义？

### 5. 错误处理

**前后端错误码对照：**
```typescript
// 前端
enum ElsaError {
  WORKFLOW_NOT_FOUND = 40401,
  INSTANCE_NOT_RUNNING = 40001,
  BOOKMARK_EXPIRED = 40002
}
```

**研究问题：**
- 是否需要独立的错误码区间？
- 错误提示本地化？

## Research Checklist

- [ ] 确定 API 路径规范
- [ ] 设计 DTO 命名和结构
- [ ] 确定前端 Store 策略
- [ ] 设计器 JSON 格式规范
- [ ] 确定错误处理规范

## Output

完成后输出：
1. API 接口设计文档
2. DTO 定义文件
3. 前端 Store 结构
4. 设计器 JSON 格式规范
