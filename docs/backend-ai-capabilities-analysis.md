# AutoCodeForge 后端 AI 能力分析文档

---

## 文档信息

| 属性 | 值 |
|------|------|
| **文档版本** | v1.0.0 |
| **创建日期** | 2026-05-28 |
| **最后更新** | 2026-05-28 |
| **所属模块** | AI/LLM/Agent/Copilot/Skill |
| **文档状态** | 正式版 |

---

## 目录

1. [概述](#1-概述)
2. [模块架构](#2-模块架构)
3. [LLM 集成架构](#3-llm-集成架构)
4. [AI 聊天系统](#4-ai-聊天系统)
5. [Agent 系统](#5-agent-系统)
6. [Microsoft Agent Framework 集成](#6-microsoft-agent-framework-集成)
7. [GitHub Copilot 集成](#7-github-copilot-集成)
8. [Skill 调用机制](#8-skill-调用机制)
9. [依赖关系分析](#9-依赖关系分析)
10. [风险评估](#10-风险评估)
11. [回归测试范围](#11-回归测试范围)
12. [总结与建议](#12-总结与建议)

---

## 1. 概述

AutoCodeForge 后端实现了完整的 AI 能力栈，涵盖以下核心领域：

- **LLM 集成**: 支持 Azure OpenAI 和 GitHub Copilot 双引擎
- **AI 聊天系统**: 完整的会话管理、消息持久化、历史裁剪功能
- **Agent 系统**: 四状态生命周期管理（Idle/Handling/Learning/Dormant）
- **Skill 调用机制**: 可扩展的工具接口，支持 Git 操作、通知、摘要等

---

## 2. 模块架构

### 2.1 四层架构设计

```
┌─────────────────────────────────────────────────────────────────┐
│                        API 层                                   │
│  ChatEndpoints    ChatStreamEndpoints    AgentSkillEndpoints    │
│  AgentEndpoints   AgentRegistrationEndpoints                    │
├─────────────────────────────────────────────────────────────────┤
│                      Application 层                             │
│  ChatService    AgentService    AgentExecutor    LlmConfigService│
│  AgentRegistryService    GitSkillPolicyService                  │
├─────────────────────────────────────────────────────────────────┤
│                        Core 层                                  │
│  ILlmGateway    IAgentTool    IAgentSelectionStrategy           │
│  LLMModelConfigEntity    AgentEntity    ChatSessionEntity       │
├─────────────────────────────────────────────────────────────────┤
│                      Infrastructure 层                          │
│  AgentFrameworkGateway    AgentFactory    GitHubCopilotCliService│
│  ChatSessionManager       AgentMatcher    GitTools              │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 核心文件清单

| 文件类型 | 文件路径 | 状态 |
|---------|---------|------|
| LLM网关 | `server/src/AutoCodeForge.Infrastructure/AI/AgentFrameworkGateway.cs` | ✅ 存在 |
| Agent工厂 | `server/src/AutoCodeForge.Infrastructure/AI/AgentFactory.cs` | ✅ 存在 |
| Agent执行器 | `server/src/AutoCodeForge.Infrastructure/AI/AgentExecutor.cs` | ✅ 存在 |
| 聊天会话管理 | `server/src/AutoCodeForge.Infrastructure/AI/ChatSessionManager.cs` | ✅ 存在 |
| Copilot集成 | `server/src/AutoCodeForge.Infrastructure/AI/GitHubCopilotCliService.cs` | ✅ 存在 |
| 聊天服务 | `server/src/AutoCodeForge.Application/Services/ChatService.cs` | ✅ 存在 |
| Agent服务 | `server/src/AutoCodeForge.Application/Services/AgentService.cs` | ✅ 存在 |
| 工具接口 | `server/src/AutoCodeForge.Core/Interfaces/IAgentTool.cs` | ✅ 存在 |
| Agent状态机 | `server/src/AutoCodeForge.Application/StateMachines/AgentStateMachine.cs` | ✅ 存在 |

---

## 3. LLM 集成架构

### 3.1 LLM 网关设计

**核心接口**: `ILlmGateway`

```csharp
public interface ILlmGateway
{
    Task<LlmResponse> ChatAsync(LlmRequest request, CancellationToken cancellationToken);
    Task<LlmResponse> ChatWithToolsAsync(LlmRequest request, IEnumerable<IAgentTool> tools, ...);
    Task<T?> GenerateStructuredOutputAsync<T>(LlmRequest request, ...);
}
```

**实现类**: `AgentFrameworkGateway`

**技术选型**:
- 使用 **Microsoft Agent Framework** (`Microsoft.Agents.AI`) 作为核心框架
- 支持 **Azure OpenAI** 作为底层模型服务
- 支持 **GitHub Copilot** 作为替代模型源

### 3.2 模型配置管理

**实体**: `LLMModelConfigEntity`

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | Guid | 主键 |
| `ModelName` | string | 模型名称（如 gpt-4o） |
| `Endpoint` | string | 模型端点 URL |
| `ApiKey` | string | API 密钥（可选，支持 Azure AD） |
| `Provider` | LLMProvider | 提供商（AzureOpenAI/GitHubCopilot） |
| `CliExecutable` | string | Copilot CLI 路径 |
| `Organization` | string | 组织标识 |

**支持的模型提供商**:
- **Azure OpenAI**: 通过 API Key 或 Azure AD 认证
- **GitHub Copilot**: 通过 CLI 工具集成

---

## 4. AI 聊天系统

### 4.1 聊天服务架构

**ChatService** 核心职责：

| 功能 | 方法 | 说明 |
|------|------|------|
| 创建会话 | `CreateSessionAsync` | 创建聊天会话 |
| 获取会话 | `GetSessionAsync` | 获取单个会话详情 |
| 获取消息 | `GetMessagesAsync` | 获取会话消息历史 |
| 发送消息 | `SendMessageAsync` | 发送用户消息并获取响应 |
| 删除会话 | `DeleteSessionAsync` | 软删除会话 |

### 4.2 消息处理流程

```
用户消息 → AgentMatcher → AgentExecutor → LLM响应 → 消息持久化
              ↓
        Agent匹配失败
              ↓
        回退到通用聊天
```

**关键组件协作**:

1. **AgentMatcher**：根据用户输入匹配最合适的 Agent
2. **ChatSessionManager**：管理会话历史，支持 Token 限制自动裁剪
3. **AgentExecutor**：执行 Agent 请求，优先使用 Microsoft Agent Framework

### 4.3 会话历史管理

`ChatSessionManager` 实现了智能历史管理：

- **最大消息数限制**: 默认 40 条
- **Token 限制**: 默认 3000 tokens
- **自动裁剪**: 超过限制时从历史头部开始删除

---

## 5. Agent 系统

### 5.1 Agent 生命周期状态机

```
Idle ←───────────── CompleteTask ────────────── Handling
  │                   │                             │
  │ StartLearning     │ FailTask                    │
  ↓                   ↓                             │
Learning ── CompleteLearning ───→ Idle ←─────── WakeUp ←─── Dormant
                                       ↑               ↑
                                       │ EnterDormant │
                                       └─────────────┘
```

**状态说明**:

| 状态 | 说明 | 转换条件 |
|------|------|---------|
| **Idle** | 空闲状态 | 初始状态、任务完成、学习完成、被唤醒 |
| **Handling** | 处理任务中 | 分配任务 |
| **Learning** | 学习状态 | 触发学习 |
| **Dormant** | 休眠状态 | 手动进入休眠 |

### 5.2 Agent 服务核心功能

**AgentService** 提供的操作：

| 操作 | 说明 |
|------|------|
| `CreateAsync` | 创建 Agent |
| `UpdateAsync` | 更新 Agent 配置 |
| `AssignTaskAsync` | 分配任务（状态→Handling） |
| `CompleteTaskAsync` | 完成任务（状态→Idle） |
| `FailTaskAsync` | 任务失败，记录学习 |
| `StartLearningAsync` | 开始学习（状态→Learning） |
| `CompleteLearningAsync` | 完成学习，更新技能标签 |
| `EnterDormantAsync` | 进入休眠（状态→Dormant） |
| `WakeUpAsync` | 唤醒 Agent（状态→Idle） |
| `MatchByInputAsync` | 根据输入匹配最佳 Agent |

### 5.3 Agent 匹配机制

**关键词匹配算法**：

```csharp
private static int Score(string? keywords, string userInput)
{
    var split = keywords.Split(',', ...);
    return split.Count(keyword => userInput.Contains(keyword, ...));
}
```

### 5.4 Agent 学习机制

**学习触发类型**:

| 类型 | 说明 |
|------|------|
| `Manual` | 手动触发学习 |
| `Exception` | 任务失败触发学习 |

**学习记录实体**: `AgentLearningRecordEntity`

---

## 6. Microsoft Agent Framework 集成

### 6.1 集成架构

```
┌──────────────────────────────────────────────────────────────┐
│                  AgentFactory                                │
│   CreateAgentAsync(agentEntity)                             │
│           ↓                                                 │
│   CreateChatClient(model) → AzureOpenAIClient → IChatClient │
│           ↓                                                 │
│   new ChatClientAgent(chatClient, instructions, tools)      │
└──────────────────────────────────────────────────────────────┘
```

### 6.2 Agent 工厂实现

**AgentFactory** 职责：

1. **模型配置解析**: 根据 `LlmModelConfigId` 获取模型配置
2. **ChatClient 创建**: 根据配置创建 Azure OpenAI 客户端
3. **工具绑定**: 将 `IAgentTool` 转换为 `AIFunction`
4. **Agent 实例化**: 创建 `ChatClientAgent`

### 6.3 双模式执行策略

**AgentExecutor** 支持两种执行模式：

| 模式 | 优先级 | 说明 |
|------|--------|------|
| **Microsoft Agent Framework** | 高 | 使用 `ChatClientAgent.RunAsync()` |
| **传统 LLM Gateway** | 低（回退） | 使用 `ILlmGateway.ChatWithToolsAsync()` |

---

## 7. GitHub Copilot 集成

### 7.1 集成方式

**当前实现**: CLI 方式集成

```csharp
public class GitHubCopilotCliService : IGitHubCopilotService
{
    // 使用 Process 调用 copilot CLI
    // 支持参数: -p <prompt> --model <model> --allow-all-tools
}
```

**预留升级路径**: SDK 方式（待 GitHub.Copilot.SDK 稳定）

```csharp
// 未来升级方案
var copilotClient = new CopilotClient(new CopilotClientOptions { AutoStart = true });
await copilotClient.StartAsync(cancellationToken);
var agent = copilotClient.AsAIAgent(sessionConfig, ownsClient: false);
var result = await agent.RunAsync(prompt, cancellationToken);
```

### 7.2 执行流程

```
用户请求 → 判断 Provider == GitHubCopilot → GitHubCopilotCliService.ExecuteAsync
                                               ↓
                                    调用 copilot CLI
                                    参数: -p <prompt> --allow-all-tools
                                               ↓
                                    返回 CLI 输出作为响应
```

---

## 8. Skill 调用机制

### 8.1 工具接口定义

```csharp
public interface IAgentTool
{
    string Name { get; }           // 工具名称
    string Description { get; }    // 工具描述
    Task<string> ExecuteAsync(IReadOnlyDictionary<string, string> input, ...);
}
```

### 8.2 注册的工具列表

| 工具 | 路径 | 说明 |
|------|------|------|
| **GitReadToolset** | `Application/Tools/GitReadToolset.cs` | Git 读取操作 |
| **GitWriteToolset** | `Application/Tools/GitWriteToolset.cs` | Git 写入操作 |
| **GitTools** | `Application/Tools/GitTools.cs` | Git 通用操作 |
| **NotificationTool** | `Application/Tools/NotificationTool.cs` | 通知发送 |
| **SummaryTool** | `Application/Tools/SummaryTool.cs` | 摘要生成 |
| **AgentTaskCompleteTool** | `Application/Tools/AgentTaskCompleteTool.cs` | 任务完成 |

### 8.3 Skill 权限控制

**GitSkillPolicyService**: 管理 Git 技能授权策略

**GitSkillPermissionGuard**: 技能调用前的权限检查

### 8.4 工具调用记录

**AgentToolInvocationEntity**: 记录所有工具调用日志，用于审计和追溯

---

## 9. 依赖关系分析

### 9.1 模块依赖图

```
┌──────────────────┐     ┌──────────────────┐
│    ChatService   │     │   AgentService   │
│                  │     │                  │
│  ├── AgentMatcher│     │  ├── AgentRepo   │
│  ├── AgentExecutor│    │  ├── LearningRepo│
│  │   ├── ILlmGateway│   │  └── DormantRepo│
│  │   └── AgentFactory│  └──────────────────┘
│  └── ChatSessionManager┘
└──────────────────┘
         │                    ┌──────────────────┐
         └──────────────────→│ AgentFrameworkGateway │
                             │   ├── Azure OpenAI   │
                             │   └── GitHub Copilot │
                             └──────────────────┘
```

### 9.2 外部依赖

| 依赖 | 版本 | 用途 |
|------|------|------|
| `Microsoft.Agents.AI` | 最新 | Agent Framework 核心 |
| `Azure.AI.OpenAI` | 最新 | Azure OpenAI SDK |
| `Azure.Identity` | 最新 | Azure AD 认证 |

---

## 10. 风险评估

| 风险项 | 风险等级 | 影响范围 | 缓解措施 |
|-------|---------|---------|---------|
| **LLM 服务不可用** | 🟡 中 | 聊天功能全部失效 | 实现多模型回退机制 |
| **Agent 状态机异常** | 🟡 中 | Agent 任务调度失败 | 状态机单元测试覆盖 |
| **Copilot CLI 超时** | 🟢 低 | 特定模型响应慢 | 60秒超时 + 进程终止 |
| **Token 超限** | 🟢 低 | 历史消息丢失 | 自动裁剪机制 |
| **工具权限泄露** | 🔴 高 | 安全漏洞 | GitSkillPermissionGuard 强制检查 |

**整体风险评级**: 🟡 中风险

---

## 11. 回归测试范围

### 11.1 必须测试的模块

- [x] ChatService
- [x] AgentService
- [x] AgentExecutor
- [x] AgentFrameworkGateway
- [x] GitHubCopilotCliService
- [x] AgentStateMachine

### 11.2 测试类型

| 测试类型 | 测试场景 |
|---------|---------|
| **单元测试** | Agent 状态转换、工具调用、匹配算法 |
| **集成测试** | LLM 网关调用、Copilot CLI 集成 |
| **端到端测试** | 完整聊天流程：创建会话→发送消息→获取响应 |
| **API 契约测试** | ChatEndpoints、AgentSkillEndpoints |

---

## 12. 总结与建议

### 12.1 架构优势

AutoCodeForge 后端已实现完整的 AI 能力栈：

1. **模块化设计**: 各组件职责清晰，易于扩展
2. **双模式回退**: Agent Framework 失败时自动回退到传统 LLM 调用
3. **多模型支持**: 支持 Azure OpenAI 和 GitHub Copilot 切换
4. **完整生命周期**: Agent 状态管理、学习、休眠机制完整
5. **可扩展工具系统**: `IAgentTool` 接口支持灵活扩展

### 12.2 待优化项

| 优化项 | 优先级 | 说明 |
|--------|--------|------|
| GitHub Copilot SDK 集成 | 中 | 等待官方稳定版发布后升级 |
| 多模型负载均衡 | 中 | 实现故障转移和负载均衡 |
| 工具权限细粒度控制 | 高 | 增强安全防护 |
| 流式响应支持 | 中 | 支持 SSE 流式输出 |

### 12.3 未来规划

- **短期**: 完善测试覆盖，优化性能
- **中期**: 集成 GitHub Copilot SDK，实现多模型智能路由
- **长期**: 构建 Agent 协作网络，支持多 Agent 协同工作

---

**文档版本**: v1.0.0  
**创建日期**: 2026-05-28  
**作者**: AutoCodeForge 团队