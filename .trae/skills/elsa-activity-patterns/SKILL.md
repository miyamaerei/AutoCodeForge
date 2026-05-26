---
name: "elsa-activity-patterns"
description: "Research design patterns for custom Elsa Activities, including bookmark design, event integration, and testability. Invoke when designing custom activities or workflow components."
---

# Elsa Activity Patterns

This skill researches how to design and implement custom Activities that integrate properly with existing services.

## Knowledge Areas

### 1. Activity 类型选择

| 类型 | 适用场景 | 阻塞行为 |
|-----|---------|---------|
| `IActivity` | 简单同步操作 | 不阻塞 |
| `AsyncTaskActivity` | 需异步执行 | 可阻塞 |
| `CodeActivity` | 纯计算逻辑 | 不阻塞 |

**研究问题：**
- TaskExecutionActivity 应该用哪种？
- HumanGateActivity 需要阻塞等待，如何实现？

### 2. 书签（Bookmark）设计

**核心概念：**
- 书签 = 工作流暂停点
- 恢复时从书签继续执行

**设计规范：**
```
CreateBookmark(
    name: "WaitForHumanApproval-{taskId}",
    callback: OnHumanApproved,
    resumeProperty: new { TaskId = taskId }
)
```

**研究问题：**
- 书签名称命名规范？
- 如何从外部触发书签恢复？
- 书签超时如何处理？

### 3. 与现有服务的解耦

**原则：Activity 不直接依赖 Entity**

```csharp
// ❌ 不推荐：直接依赖
public class TaskExecutionActivity : Activity
{
    private readonly TaskRepository _taskRepo; // 强依赖
}

// ✅ 推荐：通过 DTO/接口
public class TaskExecutionActivity : Activity
{
    private readonly ITaskService _taskService; // 依赖抽象
}
```

**研究问题：**
- Activity 如何调用 AgentService？
- Activity 如何通知 TaskStateChanged？
- 事件总线是否适用？

### 4. 输入输出属性

```csharp
// 输入属性：工作流启动时传入
public TextBlock InputTaskName { get; set; }

// 输出属性：工作流完成后输出
public Output<string> OutputResult { get; set; }

// 变量：在工作流实例间传递
public string WorkflowVariable { get; set; }
```

**研究问题：**
- 何时用 Input / Output / Variable？
- 复杂对象如何序列化？

### 5. 可测试性

**测试策略：**
- Activity 逻辑与 Elsa 容器分离
- 通过依赖注入 Mock 外部服务
- 使用集成测试验证工作流执行

## Research Checklist

- [ ] 确定 Activity 基类选择
- [ ] 设计书签命名规范
- [ ] 设计服务调用解耦方案
- [ ] 确定输入输出属性设计
- [ ] 设计单元测试策略

## Output

完成后输出：
1. Activity 基类设计
2. 书签设计规范
3. 服务调用接口定义
4. 测试策略文档
