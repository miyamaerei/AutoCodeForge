# 阶段三：AI 核心模块

**日期**: 2026-05-20  
**预估时间**: 3-4 天  
**优先级**: 🔴 P0 - 项目核心  
**前置依赖**: 阶段一、阶段二

---

## 我是如何考虑的

### 设计思路

这是**项目最核心的模块**，所有 AI 能力都基于 Microsoft Agent Framework：

1. **LlmGateway 先行** - 统一的 LLM 调用封装，多模型切换、重试、熔断
2. **Agent 管理** - Agent CRUD、关键词匹配
3. **AgentExecutor** - Microsoft Agent Framework 封装，工具调用
4. **会话管理** - ChatSession + ChatMessage，上下文维护
5. **SSE 流式输出** - 实时打字效果，对接前端聊天界面
6. **智能匹配** - 根据用户输入自动选择合适的 Agent

### 复用设计

本阶段**大量复用阶段一、二的基础设施**，避免重复代码：

| 复用的组件 | 复用自 | 本阶段复用方式 |
|---------|-------|--------------|
| AuditableEntity / UserOwnedEntity | 阶段一 | ChatSession、ChatMessage、AgentEntity 继承基类 |
| BaseRepository<T> | 阶段一 | AgentRepository、ChatSessionRepository、ChatMessageRepository 继承 |
| ApiResponse<T> | 阶段一 | Agent Endpoints、Chat Endpoints 统一响应格式 |
| ExceptionHandlingMiddleware | 阶段一 | 全局异常处理自动生效 |
| PaginationHelper | 阶段一 | Agent 列表、会话列表分页查询 |
| UserRepository | 阶段二 | 获取当前用户信息 |
| LLMModelConfigRepository | 阶段二 | LlmGateway 读取模型配置 |
| JwtAuthMiddleware | 阶段二 | 认证保护所有 AI 相关端点 |
| JsonHelper | 阶段一 | LLM 消息和配置的 JSON 序列化 |
| TimeHelper | 阶段一 | 会话时间戳处理 |

### 本阶段新增的可复用功能

| 复用组件 | 被哪些阶段复用 |
|---------|--------------|
| LlmGateway | 阶段三、四、六、七 |
| ILlmGateway | 阶段三、四、六、七 |
| AgentExecutor | 阶段三、四 |
| ChatSessionManager | 阶段三、四 |
| AgentMatcher | 阶段三、四 |
| IAgentTool | 阶段三、六 |
| AgentService | 阶段三、四 |
| ChatService | 阶段三、四 |
| ChatSessionRepository | 阶段三、四 |
| ChatMessageRepository | 阶段三、四 |
| AgentRepository | 阶段三、四 |

### 为什么这样安排？

- **LlmGateway 是基础** - 所有 AI 调用都经过它，便于统一管理
- **Agent 先于会话** - 会话需要绑定 Agent
- **SSE 最后实现** - 依赖前面的所有组件

### Microsoft Agent Framework 集成策略

| 组件 | 职责 |
|------|------|
| ChatClient | 初始化，配置模型 |
| AIAgent | 定义 Agent，配置 System Prompt |
| AgentThread | 管理会话上下文 |
| Tools | 注册可调用工具（Git 操作等） |

---

## 本阶段复用的功能清单（来自其他阶段）

| 复用组件 | 文件路径 | 复用方式 | 避免重复代码 |
|---------|---------|---------|-----------|
| AuditableEntity | `Core/Entities/Base/AuditableEntity.cs` | AgentEntity、ChatSessionEntity、ChatMessageEntity 继承 | 6 个属性定义（每个实体 2 个） |
| UserOwnedEntity | `Core/Entities/Base/UserOwnedEntity.cs` | AgentEntity、ChatSessionEntity、ChatMessageEntity 继承 | 9 个属性定义（每个实体 3 个） |
| BaseRepository<T> | `Infrastructure/Repositories/Base/BaseRepository.cs` | AgentRepository、ChatSessionRepository、ChatMessageRepository 继承 | 150+ 行 CRUD/软删除/分页代码 |
| ApiResponse<T> | `Core/Models/ApiResponse.cs` | Agent Endpoints、Chat Endpoints 使用 | 避免 8 个端点重复写响应格式化 |
| ExceptionHandlingMiddleware | `Api/Middleware/ExceptionHandlingMiddleware.cs` | 全局自动生效 | 避免所有端点重复写异常处理 |
| PaginationHelper | `Infrastructure/Helpers/PaginationHelper.cs` | Agent 列表、会话列表分页 | 避免列表分页重复代码 |
| JsonHelper | `Infrastructure/Helpers/JsonHelper.cs` | LLM 消息 JSON 序列化 | 避免 JSON 处理重复代码 |
| TimeHelper | `Infrastructure/Helpers/TimeHelper.cs` | 会话时间戳处理 | 避免时间处理重复代码 |
| UserRepository | `Infrastructure/Repositories/UserRepository.cs` | 获取当前用户 | 避免用户查询重复代码 |
| LLMModelConfigRepository | `Infrastructure/Repositories/LLMModelConfigRepository.cs` | LlmGateway 读取模型配置 | 避免配置查询重复代码 |
| JwtAuthMiddleware | `Api/Middleware/JwtAuthMiddleware.cs` | 认证保护 | 避免认证重复代码 |

---

## 本阶段新增的可复用功能清单

| 复用组件 | 文件路径 | 说明 | 被复用次数（预估） |
|---------|---------|------|------------------|
| LlmGateway | `Infrastructure/AI/LlmGateway.cs` | LLM 网关（多模型+重试+熔断） | 4 次 |
| ILlmGateway | `Core/Interfaces/ILlmGateway.cs` | LLM 网关接口 | 4 次 |
| AgentExecutor | `Infrastructure/AI/AgentExecutor.cs` | Agent 执行器 | 3 次 |
| ChatSessionManager | `Infrastructure/AI/ChatSessionManager.cs` | 会话管理器 | 3 次 |
| AgentMatcher | `Infrastructure/AI/AgentMatcher.cs` | Agent 智能匹配器 | 3 次 |
| IAgentTool | `Core/Interfaces/IAgentTool.cs` | Agent 工具接口 | 2 次 |
| AgentService | `Application/Services/AgentService.cs` | Agent 服务 | 3 次 |
| ChatService | `Application/Services/ChatService.cs` | 聊天服务 | 3 次 |
| AgentRepository | `Infrastructure/Repositories/AgentRepository.cs` | Agent 仓储 | 3 次 |
| ChatSessionRepository | `Infrastructure/Repositories/ChatSessionRepository.cs` | 会话仓储 | 3 次 |
| ChatMessageRepository | `Infrastructure/Repositories/ChatMessageRepository.cs` | 消息仓储 | 3 次 |

---

## 任务清单

| 编号 | 任务名称 | 文件路径 | 产出物 | 复用自 | 是否为复用功能 | 前置依赖 | 验证方式 |
|------|---------|---------|-------|------|-------------|---------|---------|
| **3.1** | 安装 Microsoft Agent Framework NuGet | `server/src/AutoCodeForge.Infrastructure/AutoCodeForge.Infrastructure.csproj` | 添加 Microsoft.Agents.AI 等包 | - | ❌ - | 阶段一 | 项目文件包含正确的 PackageReference |
| **3.2** | 创建 LLM Request/Response 模型 | `server/src/AutoCodeForge.Core/DTOs/AI/` | LlmRequest/LlmResponse/ChatMessage 模型 | - | ❌ - | 阶段一 | 代码编译，模型定义完整 |
| **3.3** | 创建 ILlmGateway 接口 | `server/src/AutoCodeForge.Core/Interfaces/ILlmGateway.cs` | LLM 网关接口定义 | - | ✅ 是 | 3.2 | 代码编译，包含 ChatAsync/ChatWithToolsAsync |
| **3.4** | 实现 LlmGateway | `server/src/AutoCodeForge.Infrastructure/AI/LlmGateway.cs` | LlmGateway 类（多模型切换+重试+熔断） | JsonHelper/TimeHelper | ✅ 是 | 3.3, 阶段二 | 代码编译，包含完整实现 |
| **3.5** | 创建 Agent DTO | `server/src/AutoCodeForge.Core/DTOs/Agent/` | CreateAgentRequest/UpdateAgentRequest/AgentResponse 等 | - | ❌ - | 阶段一 | 代码编译，DTO 定义完整 |
| **3.6** | 创建 AgentRepository | `server/src/AutoCodeForge.Infrastructure/Repositories/AgentRepository.cs` | AgentRepository 类（继承 BaseRepository<AgentEntity>） | 阶段一 (BaseRepository) | ✅ 是 | 阶段二 | 代码编译 |
| **3.7** | 创建 AgentService | `server/src/AutoCodeForge.Application/Services/AgentService.cs` | AgentService 类（CRUD+关键词匹配） | - | ✅ 是 | 3.6, 阶段二 | 代码编译，包含完整的 Agent 管理方法 |
| **3.8** | 创建 Agent Endpoints | `server/src/AutoCodeForge.Api/Endpoints/AgentEndpoints.cs` | /api/v1/agents 相关端点 | ApiResponse | ❌ - | 3.5, 3.7 | 代码编译，包含 CRUD 端点 |
| **3.9** | 创建 Chat DTO | `server/src/AutoCodeForge.Core/DTOs/Chat/` | CreateSessionRequest/SendMessageRequest/ChatMessageResponse 等 | - | ❌ - | 阶段一 | 代码编译，DTO 定义完整 |
| **3.10** | 创建 ChatSessionRepository | `server/src/AutoCodeForge.Infrastructure/Repositories/ChatSessionRepository.cs` | ChatSessionRepository 类 | 阶段一 (BaseRepository) | ✅ 是 | 阶段二 | 代码编译 |
| **3.11** | 创建 ChatMessageRepository | `server/src/AutoCodeForge.Infrastructure/Repositories/ChatMessageRepository.cs` | ChatMessageRepository 类 | 阶段一 (BaseRepository) | ✅ 是 | 阶段二 | 代码编译 |
| **3.12** | 创建 ChatSessionManager | `server/src/AutoCodeForge.Infrastructure/AI/ChatSessionManager.cs` | ChatSessionManager 类（会话历史+Token 管理） | TimeHelper | ✅ 是 | 3.10, 3.11 | 代码编译，包含会话管理逻辑 |
| **3.13** | 创建 Agent 工具接口 | `server/src/AutoCodeForge.Core/Interfaces/IAgentTool.cs` | IAgentTool 接口定义 | - | ✅ 是 | 阶段一 | 代码编译，包含工具元数据和 ExecuteAsync |
| **3.14** | 创建 AgentExecutor | `server/src/AutoCodeForge.Infrastructure/AI/AgentExecutor.cs` | AgentExecutor 类（基于 Microsoft Agent Framework） | LlmGateway | ✅ 是 | 3.1, 3.4, 3.13 | 代码编译，包含 Agent 初始化和执行逻辑 |
| **3.15** | 创建智能 Agent 匹配逻辑 | `server/src/AutoCodeForge.Infrastructure/AI/AgentMatcher.cs` | AgentMatcher 类（关键词权重匹配） | - | ✅ 是 | 3.7, 3.4 | 代码编译，包含 MatchAgentAsync 方法 |
| **3.16** | 创建 ChatService | `server/src/AutoCodeForge.Application/Services/ChatService.cs` | ChatService 类（会话管理+消息发送） | ChatSessionManager/AgentExecutor/AgentMatcher | ✅ 是 | 3.12, 3.14, 3.15 | 代码编译，包含完整聊天逻辑 |
| **3.17** | 创建 Chat Endpoints（非流式） | `server/src/AutoCodeForge.Api/Endpoints/ChatEndpoints.cs` | /api/v1/chat/sessions 相关端点 | ApiResponse | ❌ - | 3.9, 3.16 | 代码编译，包含会话 CRUD 和发送消息端点 |
| **3.18** | 创建 SSE 流式输出 Endpoint | `server/src/AutoCodeForge.Api/Endpoints/ChatStreamEndpoints.cs` | /api/v1/chat/sessions/{id}/stream 端点 | ApiResponse | ❌ - | 3.16, 3.17 | 代码编译，实现 Server-Sent Events 流式输出 |
| **3.19** | 注册 AI 相关服务 | `server/src/AutoCodeForge.Api/Program.cs` | 注册 LlmGateway/AgentService/ChatService/AgentExecutor | - | ❌ - | 3.4, 3.7, 3.14, 3.16 | 代码编译，所有服务已注册 |
| **3.20** | 验证 AI 功能 | `server/src/AutoCodeForge.Api/` | 可以创建 Agent、创建会话、发送消息（含流式） | - | ❌ - | 3.1-3.19 | 运行应用，使用 Swagger 测试完整聊天流程 |

---

## 关键文件内容预览

### LlmGateway.cs 核心结构

```csharp
// server/src/AutoCodeForge.Infrastructure/AI/LlmGateway.cs
public class LlmGateway : ILlmGateway
{
    private readonly LLMModelConfigRepository _configRepository;
    private readonly ILogger<LlmGateway> _logger;

    public async Task<LlmResponse> ChatAsync(LlmRequest request)
    {
        var model = await SelectModelAsync(request.PreferredModelId);
        return await ExecuteWithRetryAsync(async () => 
            await CallModelAsync(model, request));
    }

    public async Task<LlmResponse> ChatWithToolsAsync(LlmRequest request, IEnumerable<IAgentTool> tools)
    {
        // 带工具调用的实现
    }
}
```

### AgentExecutor.cs 核心结构

```csharp
// server/src/AutoCodeForge.Infrastructure/AI/AgentExecutor.cs
public class AgentExecutor
{
    private readonly ILlmGateway _llmGateway;
    private readonly IEnumerable<IAgentTool> _tools;

    public async Task<string> ExecuteAsync(
        AgentEntity agent, 
        string userInput, 
        List<ChatMessageEntity> history)
    {
        // 初始化 Microsoft Agent Framework
        // 创建 AIAgent
        // 创建 AgentThread
        // 注册工具
        // 执行并返回结果
    }
}
```

---

## 注意事项

⚠️ **重要提醒**

1. **Microsoft Agent Framework 版本** - 使用最新稳定预览版，注意 API 变动
2. **Token 管理** - 注意会话历史 Token 数量，避免超限
3. **SSE 连接管理** - 处理连接断开、超时等情况
4. **工具调用安全** - Agent 工具调用需要权限验证
5. **LLM 调用超时** - 设置合理的超时时间，避免长时间阻塞
6. **异常重试** - LlmGateway 需要有完善的重试策略

✅ **验收标准**

- 可以创建和管理 Agent
- 可以创建聊天会话
- 可以发送消息并收到 AI 响应
- SSE 流式输出正常工作
- Agent 关键词匹配功能正常
- 对话历史正确保存

---

## 阶段完成总结

### 复用收益

本阶段通过复用阶段一、二的基础设施，预计可以**避免 400+ 行重复代码**：
- 3 个实体无需重复写 CreatedAt/UpdatedAt/NtId/IsDeleted
- 3 个 Repository 无需重复写 CRUD/软删除/分页
- 8 个 API 端点无需重复写响应格式化

### 本阶段新增复用

本阶段创建了 11 个可复用组件，将被后续阶段（四、六、七）使用。

### 下一步

完成本阶段后，进入 **阶段四：任务中心模块**，详见 `04-phase-four-task-center.md`
