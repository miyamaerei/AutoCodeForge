---
name: "multi-04-task-orchestration"
description: "实现任务编排功能，包括Agent选择策略、多秘书负载均衡、Manager审核约束。Invoke when user asks for '多秘书负载均衡'、'任务分配策略'或'Manager审核并发上限'。"
---

# multi-04-task-orchestration

## 1. Skill名称与描述

- **Skill名称**: `multi-04-task-orchestration`
- **一句话描述**: 实现任务到达时的Agent选择策略、多秘书负载均衡、Manager审核约束及任务分配决策。
- **使用场景触发条件**: 
  - 用户说"实现多秘书负载均衡"
  - 用户说"Manager审核加并发上限"
  - 用户说"任务分配策略"

## 2. 前置条件与输入要求

- **前置条件**:
  - 已阅读需求来源文档：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-INDEX.md`
  - 已阅读需求来源文档：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-上篇.md`（§3.3.3）
  - 已阅读需求来源文档：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-下篇.md`（§16.2、§16.3）
  - Agent实体已创建（由entity-scaffolder生成）
  - Agent状态机已实现（由multi-02-agent-lifecycle生成）

- **输入要求**:
  - 负载均衡策略配置（如最小负载、轮询等）
  - Manager并发上限数值
  - 任务优先级配置
  - 越级兜底策略参数

## 3. 执行步骤（Step by Step）

### Step 1: 创建 IAgentSelectionStrategy 接口
- **动作描述**: 在 `server/src/` 创建 `IAgentSelectionStrategy.cs` 接口文件
- **预期输出**: 包含 `SelectAgent()`、`SelectSecretary()`、`SelectManager()`、`SelectWorker()` 方法的接口
- **验收检查点**: 接口文件存在，方法签名符合设计

### Step 2: 实现 LeastLoad 负载均衡策略
- **动作描述**: 创建 `LeastLoadAgentSelectionStrategy.cs` 实现类
- **预期输出**: 实现按当前Handling任务数选择负载最小的Agent
- **验收检查点**: 策略类能正确返回负载最低的可用Agent

### Step 3: 创建 TaskOrchestrator 服务
- **动作描述**: 创建 `TaskOrchestrator.cs` 服务类
- **预期输出**: 包含任务分配决策逻辑的服务
- **验收检查点**: 服务类包含 `AssignTask()`、`ReassignTask()` 方法

### Step 4: 实现 Manager 审核约束
- **动作描述**: 在 TaskOrchestrator 中实现并发上限、转交机制、越级兜底策略
- **预期输出**: Manager审核任务数不超过配置上限，超出时触发转交或兜底
- **验收检查点**: 超过并发上限时正确触发转交逻辑

### Step 5: 创建任务分配 API Endpoint
- **动作描述**: 在 `server/src/` 创建 `TaskOrchestrationEndpoints.cs`
- **预期输出**: POST `/api/tasks/assign` 接口，支持任务分配
- **验收检查点**: API能正确响应并返回分配结果

### Step 6: 创建前端任务分配相关类型和API
- **动作描述**: 在 `client/src/` 创建 `api/task-orchestration.ts` 和 `types/task-orchestration.ts`
- **预期输出**: 前端API调用层和类型定义
- **验收检查点**: 类型定义完整，API函数可调用

### Step 7: 创建 Pinia Store 管理任务分配状态
- **动作描述**: 创建 `stores/task-orchestration.ts`
- **预期输出**: 包含分配状态管理的Pinia store
- **验收检查点**: Store能正确管理分配状态和历史记录

## 4. 输出规范（Output Specification）

| 交付物 | 路径 | 格式 | 说明 |
|--------|------|------|------|
| IAgentSelectionStrategy 接口 | `server/src/Application/Services/IAgentSelectionStrategy.cs` | C# Interface | Agent选择策略接口 |
| LeastLoad策略实现 | `server/src/Application/Services/LeastLoadAgentSelectionStrategy.cs` | C# Class | 最小负载策略实现 |
| TaskOrchestrator服务 | `server/src/Application/Services/TaskOrchestrator.cs` | C# Class | 任务编排核心服务 |
| 配置类 | `server/src/Application/Configuration/OrchestrationSettings.cs` | C# Class | 编排配置 |
| API端点 | `server/src/Presentation/Endpoints/TaskOrchestrationEndpoints.cs` | C# Class | 任务分配API |
| 前端类型 | `client/src/types/task-orchestration.ts` | TypeScript | 类型定义 |
| 前端API | `client/src/api/task-orchestration.ts` | TypeScript | API调用层 |
| Pinia Store | `client/src/stores/task-orchestration.ts` | TypeScript | 状态管理 |

## 5. 边界与限制（Boundaries & Limitations）

- **不负责**: Agent状态机内部实现（由multi-02-agent-lifecycle负责）
- **不负责**: 工序Step流转逻辑（由multi-01-task-pipeline负责）
- **不负责**: 门控判断（由multi-03-human-gate负责）
- **假设前提**: Agent注册表已存在，Agent实体已创建
- **无法处理**: 无可用Agent时的无限等待（需配合失败恢复机制）

## 6. 与其他Skill的关系

- **上游Skill**:
  - `multi-02-agent-lifecycle`: 依赖Agent状态机接口
  - `entity-scaffolder`: 依赖Agent实体定义
- **下游Skill**:
  - `multi-03-human-gate`: 使用任务分配结果触发门控
  - `multi-05-agent-communication`: 使用分配结果进行Agent间通信
- **互斥Skill**:
  - `multi-01-task-pipeline`: 职责不重叠，一个管步骤流转，一个管Agent选择
  - `multi-06-failure-recovery`: 职责不重叠，一个管正常分配，一个管异常恢复

## 7. 示例（Example）

**典型场景**: 用户请求"实现多秘书负载均衡"

- **输入**: 配置参数：`maxConcurrentTasksPerManager = 5`, `loadBalancingStrategy = "LeastLoad"`
- **执行步骤**:
  1. 创建 `IAgentSelectionStrategy` 接口
  2. 实现 `LeastLoadAgentSelectionStrategy`
  3. 创建 `TaskOrchestrator` 服务
  4. 配置 Manager 并发上限为5
  5. 创建分配API
- **产出**: 负载均衡策略实现，任务分配API，前端调用层

**边界场景**: 所有Manager都达到并发上限

- **输入**: 10个任务同时到达，5个Manager各处理5个任务（已达上限）
- **执行步骤**:
  1. TaskOrchestrator检测到所有Manager已满
  2. 触发转交机制，尝试将任务转交给其他可用Agent
  3. 如仍无可用Agent，触发越级兜底策略
- **产出**: 任务被正确转交或进入兜底流程

## 8. 验收检查清单（Acceptance Checklist）

- [ ] `IAgentSelectionStrategy` 接口已创建，包含必要方法
- [ ] `LeastLoadAgentSelectionStrategy` 实现正确选择负载最低的Agent
- [ ] `TaskOrchestrator` 服务包含 `AssignTask()` 和 `ReassignTask()` 方法
- [ ] Manager并发上限约束正确生效
- [ ] POST `/api/tasks/assign` API 正常工作
- [ ] 前端API和类型定义完整
- [ ] Pinia Store 能正确管理分配状态
- [ ] 超过并发上限时触发转交或兜底策略

## 9. 错误处理与回退策略

- **步骤失败处理**:
  - 接口创建失败：检查命名空间和依赖，重新创建
  - 策略实现失败：参考现有策略模式，调整实现方式
  - API编译失败：检查路由注册和依赖注入
- **部分交付**: 如果某一步失败，标记未完成项并记录日志，尝试完成其他步骤
- **回退策略**: 如果核心功能无法实现，回退到简单轮询策略作为临时方案