# MAF + Copilot CLI 集成要点（脱水版）

**文档版本**：V1.0  
**更新时间**：2026-05-22  
**目标**：提炼 Microsoft Agent Framework（MAF）与 GitHub Copilot CLI/SDK 集成的关键实现信息，仅保留可直接落地内容。

---

## 1. 一句话架构

- 编排层：Microsoft Agent Framework（MAF）
- 模型接入层：GitHub Copilot SDK（底层通过 Copilot CLI）
- 核心桥接：`CopilotClient.AsAIAgent(...)`

---

## 2. 为什么这样组合

- 用 MAF 统一 Agent 生命周期、会话、多 Agent 工作流、工具调用与中间件。
- 用 Copilot SDK 作为 LLM Provider，减少自建模型接入复杂度。
- 通过 `AsAIAgent` 保持 MAF 标准接口，后续可复用 MAF 能力（Workflow/Tools/Middleware/Telemetry）。

---

## 3. 最小依赖（.NET）

```bash
dotnet add package Microsoft.Agents.AI
dotnet add package GitHub.Copilot.SDK
```

可选（需要 Foundry 托管或 Azure 侧能力时再加）：

```bash
dotnet add package Microsoft.Agents.AI.Foundry
dotnet add package Azure.AI.Projects
dotnet add package Azure.Identity
```

---

## 4. 最小可运行集成示例

```csharp
using GitHub.Copilot.SDK;
using Microsoft.Agents.AI;

public sealed class MafCopilotService : IAsyncDisposable
{
    private readonly CopilotClient _copilotClient;
    private readonly AIAgent _agent;

    public MafCopilotService()
    {
        _copilotClient = new CopilotClient(new CopilotClientOptions
        {
            AutoStart = true,
            LogLevel = "info"
        });

        var sessionConfig = new SessionConfig
        {
            Model = "gpt-5",
            Streaming = false,
            OnPermissionRequest = (_, _) => Task.FromResult(new PermissionRequestResult
            {
                Kind = PermissionRequestResultKind.Approved
            })
        };

        _agent = _copilotClient.AsAIAgent(
            sessionConfig: sessionConfig,
            ownsClient: false,
            id: "default-agent",
            name: "Default Agent",
            description: "MAF agent backed by Copilot"
        );
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _copilotClient.StartAsync(cancellationToken);
    }

    public async Task<string> RunAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var result = await _agent.RunAsync(prompt, cancellationToken);
        return result.ToString();
    }

    public async ValueTask DisposeAsync()
    {
        await _copilotClient.StopAsync();
    }
}
```

---

## 5. 常用能力映射

- 单轮调用：`_agent.RunAsync(prompt)`
- 流式输出：`_agent.RunStreamingAsync(prompt)`
- 多轮上下文：`var conversation = _agent.CreateConversation(); conversation.SendAsync(...)`
- 工具调用：在 Agent 上注册 Tools（函数）
- 流程编排：MAF Workflow（顺序/并发/条件分支/交接）
- 横切能力：Middleware（日志、审计、限流、拦截）

---

## 6. 模型与权限配置建议

模型建议：
- 默认：`gpt-5`
- 代码导向：`codex`
- 低成本场景：使用轻量模型（按可用模型列表选择）

权限建议：
- 开发环境：可临时自动批准（提高联调效率）
- 生产环境：按 `request.Kind` 做白名单审批（shell/write/mcp 等）

---

## 7. 生产落地关键点

- 会话隔离：按用户/会话 ID 管理 `Conversation` 实例。
- 超时与重试：为 Run/Tool 调用配置取消令牌与重试策略。
- 审计日志：记录提示词、工具调用、权限审批与错误栈。
- 可观测性：接入 OpenTelemetry，跟踪时延、错误率、Token 使用。
- 限制面：对危险工具（文件写入、命令执行）做显式拦截。

---

## 8. 典型风险与规避

- 风险：权限策略过宽导致越权操作。
  - 规避：按工具类型和参数做细粒度审批与审计。
- 风险：长会话上下文膨胀导致成本和延迟上升。
  - 规避：截断历史、摘要归档、分段会话。
- 风险：模型切换造成行为漂移。
  - 规避：固定关键场景模型版本并回归测试。

---

## 9. 实施检查清单

- 已安装并可执行 Copilot CLI，且完成登录。
- 服务启动时 `CopilotClient.StartAsync()` 成功。
- `AsAIAgent` 初始化成功，可返回基础对话结果。
- 会话模式可保持上下文，多轮响应正确。
- 至少 1 个 Tool 调用链路验证通过。
- 权限审批、日志、异常处理已接入。
- 流式输出（如需要）已完成前后端联调。

---

## 10. 结论

MAF + Copilot CLI/SDK 的组合适合“需要 Agent 编排能力 + 希望统一调用接口”的项目：
- MAF 负责编排与工程化能力。
- Copilot SDK 负责模型连接。
- `AsAIAgent` 是两者集成的关键接口。

该方案可先以最小闭环上线（单 Agent + 单会话 + 基础权限），再逐步扩展到 Workflow、Tools、可观测性与治理能力。
