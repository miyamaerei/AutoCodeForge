---
name: auto-developer
description: '使用双层提示系统执行单任务开发：Strategic Planner 负责规划，DevOps Orchestrator 负责执行，并包含规范断路器与演进流程。'
argument-hint: '请说明任务类型、模块范围、优先级、验收标准、组件复用约束，以及已知的策略风险。'
---

# 自动开发

## 适用场景
- 需要包含规划、执行、审计和单任务报告的一次性交付。
- 需要能够处理策略冲突且不发生死锁的稳健工作流。
- 需要严格执行组件复用，并支持受控的策略演进。

## 双层提示系统
1. Strategic Planner（规划层）
- 负责需求拆解、风险预判和方案韧性。
- 维护 docs/MasterPlan.md（本仓库的实时 MASTER_PLAN 文件）。
- 当本轮容量不足时，缩小范围或重新排序优先级。
- 当实现与策略冲突并阻塞交付时，触发策略升级流程。

2. DevOps Orchestrator（执行层）
- 使用两个固定角色运行交付流水线：
	- @Worker：负责实现代码，且必须复用已批准的共享组件。
	- @Auditor：负责执行策略门禁，并对不符合规范的输出拥有否决权。
- 仅执行 Strategic Planner 批准的范围。

## 不在范围内
- 多任务汇总或跨轮聚合。
- 日报生成。
- 历史文件归档或批量清理文件。
- 组件库同步。

## 需要的输入
1. 任务类型：DEV、BUG、REFACTOR、OPTIMIZE、DOCS 或 OTHER。
2. 范围：目标模块和文件。
3. 优先级：P0、P1、P2 或 P3。
4. 验收标准和期望行为。
5. 复用目标：需要使用的共享组件或可复用模块。
6. 已知的策略冲突候选项（如有）。

## 工作流程
1. 任务理解
- 确认任务类型、范围、优先级和验收标准。
- 从 docs/MasterPlan.md 读取当前上下文。
- 从 docs/templates/PROJECT_SPEC.md 读取策略要求。

2. 战略规划（规划层）
- 将工作拆分为可执行、可通过审计门禁的切片。
- 确定依赖顺序、回退方案和止损边界。
- 如果完整范围无法放入本轮，则裁剪为最小有价值交付，并在 docs/MasterPlan.md 中记录延期项。

3. 流水线执行（编排层）
- @Worker 按照批准的方案和仓库约定进行实现。
- 强制复用规则：优先使用已批准的共享组件和现有模块抽象；只要存在可复用资产，就不要交付临时拼凑的重复逻辑。
- 如适用，按以下顺序实现：模型、API 层、业务逻辑、测试、注释/文档。

4. 自检验证
- 确认代码可以构建或运行。
- 运行或更新相关测试。
- 检查是否符合 PROJECT_SPEC 规则。

5. 审计门禁
- 审查可维护性、复用情况和测试覆盖率。
- 对违反策略的内容执行一票否决。

6. 规范断路器与演进
- 冲突检测：如果实现需要采用被策略拒绝的模式（例如 @Worker 提议使用 fmt.Println，而 @Auditor 阻止），不要陷入死锁。
- 断路器动作：
	- 只冻结受冲突影响的项。
	- 继续推进不冲突的切片，保持交付不断线。
	- 创建 docs/reports/SPEC_CHANGE_REQUEST_XXX.md，写入冲突证据、影响范围和建议的策略修订。
- 审批流程：
	- APPROVE：将已接受的规则更新应用到 docs/templates/PROJECT_SPEC.md，然后恢复被阻塞的工作。
	- REJECT：保持当前策略，重新规划实现路径，并在 docs/MasterPlan.md 中更新缓解方案。

7. 单任务输出更新
- 每个任务都必须创建 docs/reports/ROUND_REPORT_YYYYMMDD_TYPE_XXX.md，包括小修复和仅文档工作。
- 只在 docs/MasterPlan.md 中更新本任务的进度、状态和阻塞项。
- 在此 skill 中不要执行跨任务汇总、归档动作或全局报告合并。

## 质量门槛
1. 代码与现有架构和命名保持一致。
2. 严格执行优先复用原则；绕过组件必须有明确例外。
3. 行为变更必须补充或更新测试。
4. 每个任务都完成轮次报告和任务范围内的 MasterPlan 更新。
5. 任何策略冲突都必须通过 SPEC_CHANGE_REQUEST 处理，并明确审批状态。
6. 已批准的策略演进必须反映到 docs/templates/PROJECT_SPEC.md 中。

## 关键路径
- docs/templates/MasterPlan.md
- docs/templates/PROJECT_SPEC.md
- docs/templates/ROUND_REPORT.md
- docs/templates/SPEC_CHANGE_REQUEST.md
- docs/MasterPlan.md
- docs/reports/

## 示例提示
- /auto-developer 交付 DEV 任务，要求 Planner 拆解和 Orchestrator 执行，并强制复用共享组件
- /auto-developer 修复仓库管理流程中的 BUG，若出现审计冲突则触发 SPEC_CHANGE_REQUEST，并输出一份轮次报告
- /auto-developer 实现 P0 范围，启用严格审计否决、部分范围回退，以及任务级 MasterPlan 同步
