---
name: "elsa-architecture-integration"
description: "Research how Elsa Workflow integrates with AutoCodeForge's 4-layer architecture (Api/Application/Domain/Infrastructure). Invoke when planning Elsa backend integration or discussing architecture decisions."
---

# Elsa Architecture Integration

This skill researches how Elsa Workflow fits into the existing 4-layer architecture of AutoCodeForge.

## Knowledge Areas

### 1. 四层架构中的位置

```
AutoCodeForge 四层架构
├── Api            → Elsa Endpoints / API Controllers
├── Application    → ElsaWorkflowService 桥接服务
├── Domain         → Workflow Entities (if any)
└── Infrastructure → Elsa DbContext / EF Core 配置
```

**研究问题：**
- Elsa 代码应该放在哪些层？
- ElsaWorkflowService 属于 Application 层还是 Infrastructure 层？
- 是否有独立的 Workflow Domain？

### 2. 上下游交互关系

**上游（触发工作流）：**
- Task 模块如何启动工作流？
- 是否通过事件（Event）触发？
- 还是有直接的 Service 调用？

**下游（工作流完成）：**
- 工作流完成如何通知 Task？
- 使用事件总线（EventBus）？
- 还是回调机制？

**并行（Agent 模块）：**
- AgentExecutionActivity 如何调用 AgentService？
- 状态如何同步回工作流？

### 3. 双数据库隔离策略

**风险点：**
- 业务库（SqlSugar）vs Elsa 库（EF Core）
- 跨库事务一致性
- 查询逻辑分离

**研究问题：**
- ElsaDbContext 是否需要独立的 ConnectionString？
- 如何避免 SqlSugar 误操作 Elsa 表？
- 跨库关联查询是否必要？

### 4. 代码规范对齐

| 规范项 | 现有做法 | Elsa 引入后 |
|-------|---------|------------|
| 命名 | PascalCase, 业务命名 | ElsaActivity / ElsaWorkflow |
| 注释 | 中文 XMLDOC | 保持一致 |
| 异常 | 统一异常处理 | 纳入统一体系 |
| 日志 | Serilog + 结构化 | 保持一致 |

## Research Checklist

- [ ] 确认 Elsa 代码放置的层
- [ ] 梳理工作流触发链路
- [ ] 梳理工作流完成回调链路
- [ ] 确定双数据库配置方式
- [ ] 确认命名规范
- [ ] 确认日志/异常处理规范

## Output

完成后输出：
1. 架构决策文档（哪些层放什么代码）
2. 上下游交互图
3. 配置文件结构
