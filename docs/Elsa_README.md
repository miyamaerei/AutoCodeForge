# Elsa Workflow 集成文档

**版本**: v1.0.0  
**创建日期**: 2026-05-26

---

## 📚 文档索引

欢迎使用 Elsa Workflow 集成文档系列！这里有完整的技术选型、实施问答和快速开始指南。

### 📄 文档列表

| 文档 | 版本 | 说明 | 阅读顺序 |
|-----|------|------|---------|
| [技术选型 Elsa Workflow](./技术选型_Elsa_Workflow.md) | v2.0.0 | 技术选型分析、架构设计、迁移路线图 | 1️⃣ |
| [Elsa Workflow 实施问答与最佳实践](./Elsa_Workflow_实施问答与最佳实践.md) | v1.0.0 | 常见问题解答、双数据库方案、文件结构 | 2️⃣ |
| [Elsa 集成快速开始](./Elsa_集成_快速开始.md) | v1.0.0 | 快速集成指南、最小 Demo 代码 | 3️⃣ |

---

## 🚀 快速开始

### 1. 先读技术选型
了解为什么选择 Elsa Workflow，以及整体架构设计。

### 2. 再读实施问答
了解双数据库方案、文件结构、前端设计器选项。

### 3. 最后开始实施
按照快速开始文档，一步步搭建最小 Demo。

---

## 🔑 核心要点

### Elsa Workflow 重要说明

| 特性 | 说明 |
|-----|-----|
| **持久化** | 只支持 **EF Core**（不支持 SqlSugar） |
| **数据库** | 支持 SQLite、SQL Server、PostgreSQL、MySQL |
| **.NET 10** | ✅ 完全支持（Elsa 3.6.2+） |

### 推荐架构

```
应用层 (Application Layer)
├── ElsaWorkflowService (桥接服务)
├── Activities (自定义活动)
└── Workflows (工作流定义)

数据层
├── 业务数据库 (SqlSugar)
└── Elsa 数据库 (EF Core)
```

---

## 📋 实施路线图

### Phase 1: 最小 Demo 验证（1-2天）
- [ ] 安装 Elsa NuGet 包
- [ ] 配置 Elsa 服务
- [ ] 创建 HelloWorld Workflow
- [ ] 搭建简单前端页面
- [ ] 验证完整流程

### Phase 2: 基础集成（1周）
- [ ] 实现双数据库桥接服务
- [ ] 创建 TaskExecutionActivity
- [ ] 创建 HumanGateActivity
- [ ] 创建 AgentExecutionActivity
- [ ] 实现工作流列表和详情页面

### Phase 3: 七步工作流迁移（2-3周）
- [ ] 将现有七步工作流迁移到 Elsa
- [ ] 实现书签暂停/恢复
- [ ] 集成到任务中心
- [ ] 测试完整流程

### Phase 4: 工作流设计器（2-4周）
- [ ] 选择前端框架（Vue Flow / LogicFlow）
- [ ] 实现拖拽设计
- [ ] 实现节点属性编辑
- [ ] 实现工作流保存/发布

---

## 🎨 Vue 3 设计器选型

| 框架 | 优点 | 缺点 | 开发时间 |
|-----|------|------|---------|
| **Vue Flow** | 轻量、灵活、Vue 3 原生、社区活跃 | 需要自己构建设计器 | 2-4周 |
| **LogicFlow** | 功能完整、企业级、中文文档 | 稍重，需要学习 | 3-5周 |
| **@xiaoxiao6.0/flow-designer-vue** | 开箱即用、基于 Vue Flow | 相对较新 | 1-2周 |

**推荐：Vue Flow** - 轻量灵活，符合我们的技术栈！

---

## 📖 详细内容索引

### 技术选型文档

- [技术选型 Elsa Workflow - 概述](./技术选型_Elsa_Workflow.md#概述)
- [推荐架构](./技术选型_Elsa_Workflow.md#推荐架构)
- [前端集成方案](./技术选型_Elsa_Workflow.md#前端集成方案)
- [数据库设计](./技术选型_Elsa_Workflow.md#数据库设计)
- [实施计划](./技术选型_Elsa_Workflow.md#实施计划)
- [风险与挑战](./技术选型_Elsa_Workflow.md#风险与挑战)

### 实施问答文档

- [双数据库查询方案](./Elsa_Workflow_实施问答与最佳实践.md#问题一双数据库查询方案)
- [文件结构组织](./Elsa_Workflow_实施问答与最佳实践.md#问题二elsa-代码文件结构组织)
- [最小 Demo 验证](./Elsa_Workflow_实施问答与最佳实践.md#问题三最小-demo-验证方案)
- [Vue 3 前端最佳实践](./Elsa_Workflow_实施问答与最佳实践.md#问题四vue-3-前端最佳实践)
- [完整 Demo 代码](./Elsa_Workflow_实施问答与最佳实践.md#附录完整-demo-代码示例)

### 快速开始文档

- [后端快速集成](./Elsa_集成_快速开始.md#后端快速集成)
- [前端快速集成](./Elsa_集成_快速开始.md#前端快速集成)
- [验证 Demo](./Elsa_集成_快速开始.md#验证-demo)

---

## 💡 常见问题

### Q1: 可以用 SqlSugar 操作 Elsa 数据库吗？
**A:** 不行。Elsa Workflow 只支持 EF Core。我们采用双数据库方案：
- 业务数据库 → SqlSugar
- Elsa 数据库 → EF Core

### Q2: 前端设计器需要从零开始吗？
**A:** 不需要！推荐使用 **Vue Flow** 或 **LogicFlow**，1-4周就能完成基础设计器。

### Q3: 需要新建代码库吗？
**A:** 不需要。保持现有四项目架构，在 Api 和 Application 层插入 Elsa 代码。

### Q4: Demo 验证需要多久？
**A:** 1-2天就能完成最小 Demo（HelloWorld 工作流）。

### Q5: Elsa 服务如何注册？
**A:** 使用标准依赖注入，在 `Program.cs` 中配置：

```csharp
// 添加 Elsa 服务
builder.Services.AddElsa(elsa => elsa
    .UseEntityFrameworkCore(ef => ef.UseSqlite(connectionString))
    .AddWorkflow<HelloWorldWorkflow>()
    .AddActivitiesFrom<Program>());  // 扫描当前程序集中的 Activity

// 添加 Elsa API 端点
builder.Services.AddElsaApiEndpoints();
```

### Q6: 配置文件放在哪里？
**A:** 使用标准的 `appsettings.json`，在连接字符串区域添加：

```json
{
  "ConnectionStrings": {
    "Elsa": "Data Source=elsa.db"
  },
  "Elsa": {
    "ServerOptions": {
      "BaseUrl": "http://localhost:5000"
    }
  }
}
```

---

## 🤝 贡献指南

如果您发现文档有问题或需要补充内容：

1. 请先阅读现有文档
2. 创建一个新的文档或更新现有文档
3. 提交 PR 时请简要说明变更内容

---

## 📞 帮助

如果您有问题或需要帮助：

1. 先查阅文档索引，找到相关文档
2. 检查常见问题是否有解答
3. 联系团队成员讨论

---

## 📝 更新日志

| 版本 | 日期 | 说明 |
|-----|------|------|
| v1.0.0 | 2026-05-26 | 初始版本，创建文档索引和系列文档 |
