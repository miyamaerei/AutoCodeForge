---
name: "multi-06-failure-recovery"
description: "实现失败分类体系、差异化重试策略、Step卡死检测和应急解绑机制。Invoke when user asks for '失败重试机制'、'Step卡住自动解绑'或'不同错误不同处理'。"
---

# multi-06-failure-recovery

## 1. Skill名称与描述

- **Skill名称**: `multi-06-failure-recovery`
- **一句话描述**: 实现失败分类体系、按类别的重试策略配置、Step卡死检测和应急解绑流程。
- **使用场景触发条件**: 
  - 用户说"实现失败重试机制"
  - 用户说"Step卡住自动解绑"
  - 用户说"不同错误不同处理"

## 2. 前置条件与输入要求

- **前置条件**:
  - 已阅读需求来源文档：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-INDEX.md`
  - 已阅读需求来源文档：`e:\git\AutoFrog\AutoCodeForge\docs\多Agent分层协作系统-MVP实施研究报告-下篇.md`（§16.4、§16.8、§7）
  - TaskStepEntity已存在（由multi-01-task-pipeline生成）
  - Agent实体已存在（由entity-scaffolder生成）
  - ITaskEventPublisher接口已定义

- **输入要求**:
  - 失败分类定义（CodeError/LlmException/RequirementIssue/ReviewRejection/Timeout/Unknown）
  - 各类型重试次数配置
  - 重试间隔配置
  - Step超时阈值配置
  - 降级行为配置

## 3. 核心业务规则

### 3.1 失败分类体系

| FailureCategory | 说明 | 重试策略 | 降级行为 |
|-----------------|------|---------|----------|
| CodeError | 代码错误 | 重试3次，间隔5秒 | 降级到备用方案 |
| LlmException | LLM异常 | 重试5次，间隔10秒 | 等待后重试 |
| RequirementIssue | 需求问题 | 不重试 | 触发HumanGate |
| ReviewRejection | 审核驳回 | 重试3次 | 超过阈值终止 |
| Timeout | 超时 | 重试2次，间隔递增 | 降级或终止 |
| Unknown | 未知错误 | 重试1次 | 记录日志 |

### 3.2 重试策略配置

**配置结构**:
```json
{
    "RetryPolicies": {
        "CodeError": { "MaxRetries": 3, "IntervalMs": 5000 },
        "LlmException": { "MaxRetries": 5, "IntervalMs": 10000 },
        "Timeout": { "MaxRetries": 2, "IntervalMs": 30000, "BackoffMultiplier": 2 }
    }
}
```

### 3.3 Step卡死检测

**检测逻辑**:
1. 记录Step开始时间
2. 定期检查Step状态
3. 如果超过超时阈值且状态仍为Handling，则判定为卡死
4. 触发应急解绑流程

**超时阈值配置**:
- 默认：30分钟
- 可配置：通过GlobalConfig设置

### 3.4 应急解绑流程

```
检测到Step卡死 → 强制释放Agent → 标记Step失败 → 通知Orchestrator → 重新分配任务
```

## 4. 执行步骤（业务逻辑实现）

### Step 1: 定义 FailureCategory 枚举

**业务动作**: 创建枚举，包含6种失败类型
- CodeError、LlmException、RequirementIssue、ReviewRejection、Timeout、Unknown

**验收检查点**: 枚举定义完整

### Step 2: 创建 RetryPolicy 配置类

**业务动作**: 定义每种类别的重试次数、间隔、降级行为
- 支持差异化配置
- 从GlobalConfig读取配置

**验收检查点**: 策略配置完整

### Step 3: 创建 FailureRecoveryService 服务

**业务动作**: 实现失败处理逻辑
- 方法：`HandleFailure()`、`ExecuteRetry()`、`TriggerDegradation()`
- 集成事件发布

**验收检查点**: 服务类实现失败处理逻辑

### Step 4: 实现 Step 卡死检测

**业务动作**: 添加 `DetectStuckStep()` 方法
- 使用定时任务扫描
- 检测超时Step

**验收检查点**: 能正确检测超时Step

### Step 5: 实现应急解绑流程

**业务动作**: 添加 `EmergencyUnbind()` 方法
- 强制释放Agent
- 标记Step失败
- 通知Orchestrator重新分配

**验收检查点**: 解绑流程完整

### Step 6: 创建失败恢复API

**业务动作**: 创建 `FailureRecoveryEndpoints.cs`
- POST `/api/failure/recover`
- GET `/api/failure/history`

**验收检查点**: API能正确响应

## 5. API契约

### 5.1 失败恢复

| Method | Path | 说明 |
|--------|------|------|
| POST | /api/failure/recover | 触发失败恢复 |
| GET | /api/failure/history | 获取失败历史 |
| GET | /api/failure/stats | 获取失败统计 |

**Request: RecoverRequest**
```json
{
    "stepId": "guid",
    "failureCategory": "CodeError|LlmException|...",
    "errorMessage": "string"
}
```

## 6. 输出规范（Output Specification）

| 交付物 | 路径 | 格式 | 说明 |
|--------|------|------|------|
| FailureCategory 枚举 | `server/src/Application/Enums/FailureCategory.cs` | C# Enum | 失败分类枚举 |
| RetryPolicy 配置类 | `server/src/Application/Configuration/RetryPolicy.cs` | C# Class | 重试策略配置 |
| FailureRecoveryService | `server/src/Application/Services/FailureRecoveryService.cs` | C# Class | 失败恢复服务 |
| API端点 | `server/src/Presentation/Endpoints/FailureRecoveryEndpoints.cs` | C# Class | 失败恢复API |
| 前端类型 | `client/src/types/failure-recovery.ts` | TypeScript | 类型定义 |
| 前端API | `client/src/api/failure-recovery.ts` | TypeScript | API调用层 |

## 7. 边界与限制

- **不负责**: 通知告警发送（由multi-08-notification-integration负责）
- **不负责**: 门控审批（由multi-03-human-gate负责）
- **不负责**: 状态机状态转换（由multi-02-agent-lifecycle负责）
- **不负责**: 实体创建（由entity-scaffolder负责）
- **假设前提**: TaskStepEntity和Agent实体已存在
- **无法处理**: 永久性失败无法恢复的场景（需人工介入）

## 8. 与其他Skill的关系

| 关系 | Skill | 说明 |
|------|-------|------|
| 上游依赖 | multi-01-task-pipeline | 依赖TaskStepEntity |
| 上游依赖 | multi-02-agent-lifecycle | 依赖Agent状态机接口 |
| 上游依赖 | multi-05-agent-communication | 依赖事件发布接口 |
| 下游消费者 | multi-03-human-gate | 消费失败事件触发人工介入 |
| 下游消费者 | multi-04-task-orchestration | 使用恢复结果重新分配任务 |

## 9. 示例

**典型场景**: Step卡死应急解绑

```
场景：Step执行超过超时阈值（30分钟）无响应

检测流程：
  1. FailureRecoveryService定时扫描
  2. 发现Step状态为Handling且超过阈值
  3. 调用 EmergencyUnbind()
  4. 强制释放Agent → Idle
  5. 标记Step失败
  6. 发布 FailureEvent
  7. Orchestrator收到事件，重新分配任务
```

## 10. 验收检查清单

- [ ] `FailureCategory` 枚举包含6种失败类型
- [ ] `RetryPolicy` 支持差异化配置
- [ ] `FailureRecoveryService` 包含 `HandleFailure()` 方法
- [ ] Step卡死检测逻辑正确工作
- [ ] `EmergencyUnbind()` 方法能强制释放Agent
- [ ] POST `/api/failure/recover` API 正常工作
- [ ] 失败事件能正确发布