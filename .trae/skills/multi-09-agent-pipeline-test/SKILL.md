---
name: "multi-09-agent-pipeline-test"
description: "定义Agent流水线专属测试场景、测试策略与数据构造模式、复杂流程的测试编排方式。Invoke when user asks for '给Agent流水线写测试'、'怎么测HumanGate场景'或'状态机转换怎么验证'。"
---

# multi-09-agent-pipeline-test

## 1. Skill名称与描述

- **Skill名称**: `multi-09-agent-pipeline-test`
- **一句话描述**: 定义Agent流水线专属测试场景、测试策略与数据构造模式、复杂流程的测试编排方式。
- **使用场景触发条件**: 
  - 用户说"给Agent流水线写测试"
  - 用户说"怎么测HumanGate场景"
  - 用户说"状态机转换怎么验证"

## 2. 前置条件与输入要求

- **前置条件**:
  - 已阅读需求来源文档：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-INDEX.md`
  - 已阅读需求来源文档：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-下篇.md`（§14、§7）
  - 核心Skill已实现（multi-01~multi-06）
  - 测试框架已配置（xUnit/NUnit）

- **输入要求**:
  - 测试场景定义
  - 测试数据构造需求
  - 测试覆盖率目标
  - 测试环境配置

## 3. 测试策略与场景定义

### 3.1 测试金字塔映射

| 层级 | 类型 | 占比 | 说明 |
|------|------|------|------|
| 单元测试 | Unit | 70% | 单个组件/方法 |
| 集成测试 | Integration | 20% | 组件间协作 |
| 端到端测试 | E2E | 10% | 完整流程 |

### 3.2 核心测试场景矩阵

#### 3.2.1 Agent状态机测试场景

| 场景 | 测试目的 | 验证点 |
|------|---------|--------|
| 状态转换正常路径 | 验证状态机基本转换 | Idle→Handling→Idle |
| 状态转换非法路径 | 验证状态机保护 | Idle→Learning（非法） |
| 空闲超时触发Learning | 验证IdleMonitor | Idle超时→Learning |
| Learning完成返回Idle | 验证Learning流程 | Learning→Idle |
| Dormant冻结与恢复 | 验证休眠机制 | Idle→Dormant→Idle |
| 角色差异化超时 | 验证不同角色配置 | Secretary/Manager/Worker不同超时 |

#### 3.2.2 工序流转测试场景

| 场景 | 测试目的 | 验证点 |
|------|---------|--------|
| 正常7步流转 | 验证完整流程 | Step1→Step2→...→Step7→Completed |
| Step失败重试 | 验证重试机制 | Step失败→重试→成功 |
| Step跳过 | 验证条件跳过 | 条件满足→跳过Step |
| 上下文传递 | 验证数据流转 | Step.Output→NextStep.Input |
| 上下文截断 | 验证Token限制 | 超过阈值→截断 |

#### 3.2.3 HumanGate门控测试场景

| 场景 | 测试目的 | 验证点 |
|------|---------|--------|
| 门控创建 | 验证触发条件 | Step完成→创建HumanGate |
| 门控批准 | 验证批准流程 | Approved→下一步 |
| 门控驳回 | 验证驳回流程 | Rejected→Step重置 |
| 修改后批准 | 验证修改注入 | 修改内容→NextStep.Input |
| 门控策略判断 | 验证条件触发 | conditional策略正确判断 |
| 暂停任务 | 验证贯穿式介入 | Paused状态正确 |
| 恢复任务 | 验证恢复流程 | 从暂停点继续 |
| 紧急终止 | 验证终止流程 | 所有Step取消，Agent释放 |
| 需求变更 | 验证变更传播 | 新需求→相关Step重做 |

#### 3.2.4 任务编排测试场景

| 场景 | 测试目的 | 验证点 |
|------|---------|--------|
| Agent选择策略 | 验证负载均衡 | LeastLoad策略正确选择 |
| Manager并发上限 | 验证约束 | 超过上限→触发转交 |
| 转交机制 | 验证降级 | 转交失败→越级兜底 |
| 跨服务器分配 | 验证多服务器 | 不同ServerId的Agent分配 |

#### 3.2.5 失败恢复测试场景

| 场景 | 测试目的 | 验证点 |
|------|---------|--------|
| 失败分类 | 验证分类体系 | 6种FailureCategory正确识别 |
| 差异化重试 | 验证策略 | 不同类别不同重试次数 |
| Step卡死检测 | 验证超时 | 超时→检测到卡死 |
| 应急解绑 | 验证释放 | 卡死→强制释放Agent |
| 降级行为 | 验证降级 | 重试耗尽→降级/终止 |

#### 3.2.6 Agent通信测试场景

| 场景 | 测试目的 | 验证点 |
|------|---------|--------|
| 事件发布订阅 | 验证通信机制 | 发布→订阅者收到 |
| 产出物存储 | 验证标准化 | 存储→检索一致 |
| 事件演进预留 | 验证扩展性 | 接口预留MassTransit/Kafka |

## 4. 测试数据构造模式

### 4.1 测试数据工厂

| 工厂类 | 用途 | 关键方法 |
|--------|------|---------|
| FakeAgentFactory | 创建模拟Agent | CreateSecretary()、CreateManager()、CreateWorker() |
| FakeTaskFactory | 创建模拟任务 | CreateTask()、CreateTaskWithSteps() |
| FakeStepFactory | 创建模拟步骤 | CreateStep()、CreateStepsForTask() |
| FakeGateFactory | 创建模拟门控 | CreateHumanGate()、CreatePendingGate() |

### 4.2 测试数据特征

| 特征 | 说明 | 示例 |
|------|------|------|
| 有效数据 | 正常场景 | 有效的Agent、Task、Step |
| 边界数据 | 边界条件 | 最小/最大超时时间 |
| 无效数据 | 异常场景 | 空ID、无效状态 |
| 并发数据 | 并发场景 | 同时创建多个门控 |

## 5. 测试编排方式

### 5.1 测试套件组织

```
tests/
├── Unit/                    # 单元测试
│   ├── AgentStateMachineTests.cs
│   ├── TaskOrchestratorTests.cs
│   └── ContextChainServiceTests.cs
├── Integration/             # 集成测试
│   ├── TaskPipelineIntegrationTests.cs
│   ├── HumanGateIntegrationTests.cs
│   └── FailureRecoveryIntegrationTests.cs
└── Utils/                   # 测试工具
    ├── TestDataFactory.cs
    └── TestUtils.cs
```

### 5.2 测试执行顺序

| 阶段 | 测试类型 | 目的 |
|------|---------|------|
| 单元测试 | Unit | 验证单个组件 |
| 集成测试 | Integration | 验证组件协作 |
| 回归测试 | All | 验证修改不破坏现有功能 |

### 5.3 测试环境配置

| 配置项 | 值 | 说明 |
|--------|------|------|
| 数据库 | SQLite In-Memory | 测试隔离 |
| 超时时间 | 缩短（10秒） | 加速测试 |
| 日志级别 | Debug | 便于调试 |

## 6. 输出规范（测试指导）

| 交付物 | 路径 | 格式 | 说明 |
|--------|------|------|------|
| 测试场景文档 | `docs/test-scenarios.md` | Markdown | 测试场景矩阵 |
| 测试数据工厂 | `server/tests/Utils/TestDataFactory.cs` | C# Class | 测试数据构造 |
| 测试工具类 | `server/tests/Utils/TestUtils.cs` | C# Class | 测试辅助方法 |
| 测试套件规划 | `server/tests/TestPlan.md` | Markdown | 测试套件组织 |

## 7. 边界与限制

- **不负责**: 基础单元测试代码生成（由csharp-unit-test-generator负责）
- **不负责**: 通用测试基础设施搭建
- **不负责**: 前端测试（本Skill专注后端测试）
- **假设前提**: 核心业务代码已实现，测试框架已配置
- **无法处理**: 大规模性能测试（需专门性能测试工具）

## 8. 与其他Skill的关系

| 关系 | Skill | 说明 |
|------|-------|------|
| 上游依赖 | multi-01-task-pipeline | 测试流水线实现 |
| 上游依赖 | multi-02-agent-lifecycle | 测试状态机实现 |
| 上游依赖 | multi-03-human-gate | 测试门控实现 |
| 参考 | csharp-unit-test-generator | 测试代码生成模式 |

## 9. 示例

**典型场景**: Agent状态机转换测试

```
场景：测试Idle→Handling→Idle的正常转换

测试步骤：
  1. 使用FakeAgentFactory创建Idle状态的Agent
  2. 调用HandleTask()方法
  3. 验证状态变为Handling
  4. 调用CompleteTask()方法
  5. 验证状态变为Idle

验证点：
  - State == Idle（初始）
  - State == Handling（任务中）
  - State == Idle（完成后）
```

**边界场景**: Step卡死应急解绑测试

```
场景：测试Step执行超时触发应急解绑

测试步骤：
  1. 创建Task和Step，设置短超时时间（如1秒）
  2. 启动Step执行（模拟长时间运行）
  3. 等待超时
  4. 验证EmergencyUnbind被调用
  5. 验证Agent状态变为Idle
  6. 验证Step状态变为Failed

验证点：
  - Agent被释放
  - Step标记为失败
  - FailureEvent被发布
```

## 10. 验收检查清单

- [ ] 测试场景矩阵覆盖所有核心功能
- [ ] 测试数据工厂能构造各种测试场景
- [ ] 状态机测试覆盖所有状态转换路径
- [ ] HumanGate测试覆盖7种门控类型
- [ ] 失败恢复测试覆盖6种失败分类
- [ ] 集成测试覆盖完整流程
- [ ] 测试工具类提供必要辅助方法
- [ ] 测试环境配置正确隔离