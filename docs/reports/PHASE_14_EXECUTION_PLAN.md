# Phase 14 执行计划 - Agent 增强 Git 技能

**制定日期**: 2026-05-20  
**优先级**: 🔴 P0 - 核心能力  
**预估工时**: 4-6 天  
**前置依赖**: Phase 03 (AI) + Phase 06 (Git) + Phase 13 (Review)  
**目标**: Agent 具备"可控、可审计、可恢复"的 Git 协作能力  

---

## 一、任务复杂度评估

### 1.1 工作量分解

| 工作块 | 任务数 | 工时 | 难度 | 风险 |
|--------|--------|------|------|------|
| **数据模型与 DTO** | 2 | 3h | ⭐ 低 | 无 |
| **只读技能实现** | 2 | 6h | ⭐⭐ 中 | 中等 |
| **变更技能实现** | 2 | 8h | ⭐⭐⭐ 高 | 高 |
| **权限与审计框架** | 3 | 7h | ⭐⭐⭐ 高 | 中 |
| **集成与测试** | 1 | 4h | ⭐⭐ 中 | 中等 |
| **总计** | **10** | **28h** | **平均⭐⭐** | **整体可控** |

### 1.2 关键风险识别

| 风险 | 影响 | 缓解措施 |
|------|------|---------|
| **权限策略复杂** | 造成审批延迟 | 预定义 3 级策略（R/W/Dangerous）+ 可配置 |
| **Git 冲突处理** | 用户困惑 | GitSkillErrorMapper 提供可执行建议 |
| **审计日志量大** | 存储爆炸 | 摘要存储（InputDigest/OutputDigest） |
| **跨平台 API 差异** | 功能不一致 | 复用 IGitProvider 抽象层 |
| **Agent 误操作** | 严重后果 | 默认只读 + 高风险操作强制确认 |

---

## 二、分阶段执行方案

### 🟢 第一阶段：基础设施 (1 天)
**目标**: 搭建数据模型、权限框架和审计骨架

#### 任务清单
- **14.1** 数据模型与 DTO（3h）
  - 新增 GitReadRequest/Response DTO
  - 新增 GitWriteRequest/Response DTO
  - 新增 GitOperationPolicy 模型
  - 修改 ChatSession 添加 TaskId
  
- **14.4** GitSkillPermissionGuard（2h）
  - 用户/仓库/操作级权限检查
  - 权限矩阵与策略评估
  - 错误码与拒绝理由
  
- **14.7** AgentToolAuditLogger（2h）
  - 工具调用日志记录
  - 审计字段设计与索引
  - 异常路径处理

**交付物**: DTO、权限模型、审计基础  
**验证**: dotnet build 通过，无编译错误  

---

### 🟡 第二阶段：只读能力 (1.5 天)
**目标**: Agent 可安全查询仓库信息

#### 任务清单
- **14.2** GitReadToolset（4h）
  - 分支列表查询工具
  - 提交历史查询工具
  - Pull Request 状态查询工具
  - 文件差异/内容查询工具
  
- **14.5** GitContextHydrator（2h）
  - 自动注入仓库快照到 Agent 上下文
  - 任务与仓库的绑定关系
  - 上下文持久化与刷新

**交付物**: 4 个只读工具 + 上下文管理器  
**验证**: 
- dotnet build 通过
- 单元测试覆盖查询成功与空结果场景
- 手动验证：Agent 可输出分支列表

---

### 🔴 第三阶段：变更能力 (2 天)
**目标**: Agent 可在授权后执行 Git 变更操作

#### 任务清单
- **14.3** GitWriteToolset（6h）
  - 创建分支工具
  - 提交变更工具
  - 推送到远程工具
  - 创建 Pull Request 工具
  - 撤销变更工具（回滚）
  
- **14.9** GitSkillErrorMapper（3h）
  - 冲突异常 → 建议变更策略
  - 权限异常 → 建议授权申请
  - 网络异常 → 建议重试策略
  - 用户友好的错误消息

**交付物**: 5 个变更工具 + 异常映射器  
**验证**: 
- dotnet build 通过
- 集成测试覆盖授权成功/失败场景
- 手动验证：Agent 可创建分支并发起 PR

---

### 🟠 第四阶段：系统集成 (0.5 天)
**目标**: 所有组件聚合，端到端流程通畅

#### 任务清单
- **14.6** 扩展 AgentExecutor（2h）
  - 动态加载 GitReadToolset 和 GitWriteToolset
  - 工具调用前权限校验
  - 工具执行后审计日志记录
  
- **14.8** 技能策略 API（1h）
  - GET /api/v1/agent-skills 查询当前权限
  - PUT /api/v1/agent-skills/{skill} 配置权限
  - GET /api/v1/agent-skills/audit 查看调用历史
  
- **14.10** DI 注册与端到端联调（2h）
  - Program.cs 注册所有新组件
  - 数据表迁移与初始化
  - 从"查询分支"到"发起 PR"完整链路验证

**交付物**: 完整集成环境 + API + 数据库初始化脚本  
**验证**: 
- dotnet build 0 errors
- dotnet test 所有单元/集成测试通过
- 烟雾测试：通过 API 调用完整 Git 操作链

---

## 三、技术决策矩阵

### 权限策略设计

```csharp
public enum GitSkillLevel
{
    ReadOnly = 0,        // 仅允许查询操作
    WriteCollaborative = 1, // 允许创建分支/PR，但不允许直推 main
    WriteDangerous = 2,   // 允许直推 main、强推、删除分支（默认禁用）
}
```

### 默认授权表

| 用户类型 | 仓库权限 | 允许工具 | 备注 |
|---------|---------|----------|------|
| 普通成员 | ReadOnly | 查询分支/提交/PR/文件 | 安全模式 |
| 开发者 | WriteCollaborative | 上述 + 创建分支/提交/推送/PR | 正常模式 |
| 管理员 | WriteDangerous | 所有工具 + 强推/删除 | 受限模式（需人工审批） |

### 审计日志设计

```csharp
public class AgentToolInvocation
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? TaskId { get; set; }
    public string ToolName { get; set; } // "GitReadToolset.ListBranches"
    public string InputDigest { get; set; } // 摘要，避免日志过大
    public string OutputDigest { get; set; }
    public int HttpStatusCode { get; set; }
    public string ErrorMessage { get; set; } // 若失败
    public long DurationMs { get; set; }
    public DateTime InvokedAtUtc { get; set; }
}
```

---

## 四、依赖检查清单

### 前置条件

- ✅ Phase 03 完成（Agent 框架、LlmGateway、AgentExecutor）
- ✅ Phase 06 完成（IGitProvider、多平台提供者、凭据管理）
- ✅ Phase 13 完成（ReviewService、审查结果查询能力）
- ✅ DatabaseInitializer 框架已搭建
- ✅ 认证/授权中间件就位（JwtAuthMiddleware）

### 需要准备

- ✅ SQLite 迁移脚本（新表：GitSkillGrants、AgentToolInvocations）
- ✅ 种子数据（默认权限策略、内置工具定义）
- ✅ 测试用例框架（集成测试、权限矩阵测试）

---

## 五、优先级建议

### 关键路径（Must Do）
1. 14.1 + 14.4 + 14.7 - 基础设施（锁定架构）
2. 14.2 + 14.5 - 只读能力（降低风险）
3. 14.6 - 集成框架（连接各部分）
4. 14.10 - DI 和端到端测试

### 高价值路径（Should Do）
5. 14.3 + 14.9 - 变更能力（完整功能）

### 可优化路径（Nice to Have）
6. 14.8 - 技能策略 API（用户配置界面）

---

## 六、质量门禁

| 门禁 | 预期标准 | 验证方式 |
|------|---------|----------|
| 编译 | dotnet build 0 errors | build log |
| 测试 | 所有单元/集成测试通过 | test output |
| 审计 | 工具调用全链路可追踪 | 审计日志查询 |
| 权限 | 未授权操作100%拒绝 | 权限矩阵测试 |
| 安全 | 无明文凭据泄露 | 代码审查 |
| 文档 | 所有公开成员XML注释完整 | API文档生成 |

---

## 七、风险预案

### 高风险场景处理

| 场景 | 预防措施 | 恢复措施 |
|------|---------|----------|
| Agent 自动强推到 main | 强制权限检查 + 人工确认 | 自动回滚到上一个安全 commit |
| Git 网络失败 | 失败自动重试（3次）+ 清晰错误码 | ErrorMapper 提供建议操作 |
| 权限不足导致操作失败 | 权限预检查 + 权限申请链接 | 返回可执行的权限申请指引 |
| 审计日志存储爆炸 | 只存摘要（输入/输出哈希） + 完整日志归档 | 定期清理历史审计记录 |

---

## 八、并行执行可能性

### 可并行的任务对
- 14.1 (DTO) ⚡ 14.4 (PermissionGuard) - 无直接依赖
- 14.2 (ReadToolset) ⚡ 14.5 (ContextHydrator) - 弱依赖（可依次）
- 14.3 (WriteToolset) ⚡ 14.9 (ErrorMapper) - 无直接依赖

### 建议执行顺序
```
Day 1: [14.1 + 14.4 + 14.7] (基础设施并行)
Day 2: [14.2 + 14.5] (只读能力)
Day 3: [14.3 + 14.9] (变更能力)
Day 4: [14.6 + 14.8 + 14.10] (集成与验证)
```

---

## 九、交付物清单

### 代码交付
- 📦 GitReadToolset.cs (~300 行)
- 📦 GitWriteToolset.cs (~400 行)
- 📦 GitSkillPermissionGuard.cs (~250 行)
- 📦 GitContextHydrator.cs (~200 行)
- 📦 AgentToolAuditLogger.cs (~180 行)
- 📦 GitSkillErrorMapper.cs (~220 行)
- 📦 修改 AgentExecutor.cs (~100 行新增)
- 📦 新增 AgentSkillEndpoints.cs (~150 行)
- 📦 修改 Program.cs (~20 行注册)
- 📦 修改 DatabaseInitializer.cs (~30 行)

**总计**: ~1650 行新增 + 150 行修改

### 报告交付
- 📋 ROUND_REPORT_20260520_DEV_022.md
- 📋 MasterPlan.md 更新
- 📋 Phase 14 最终总结

### 测试交付
- 🧪 GitReadToolsetTests.cs
- 🧪 GitWriteToolsetTests.cs
- 🧪 PermissionGuardTests.cs
- 🧪 端到端集成测试

---

## 十、下一步建议

### 立即可做
1. ✅ 确认 Phase 14 的 4 天时间窗口
2. ✅ 预览需要新增的 2-3 个数据表设计
3. ✅ 确认权限策略的默认配置

### 建议顺序
1. 先完成 Phase 14 第一阶段（基础设施）
2. 确保质量门禁通过后再进入第二阶段
3. 第二、三阶段可略微并行

---

**文档完成。已准备好启动第一阶段！** 🚀
