---
name: "ai-devops-pipeline"
description: "AI研发流水线文档助手。Invoke when user asks about DevOps pipeline docs, MasterPlan, DAILY_REPORT, ROUND_REPORT, SPEC_CHANGE_REQUEST, or project specifications."
---

# AI研发流水线文档助手

本Skill用于帮助AI理解和运用AutoCodeForge项目的5个核心文档，这些文档构成了AI研发流水线的规范框架。

## 文档概览

### 1. MasterPlan（主规划文档）
**用途**：全局任务池与进度追踪

核心内容：
- **全局任务池（P0-P3分级）**：
  - P0（核心）：必须复用的组件，如`pkg/log`
  - P1（规范）：需对齐PROJECT_SPEC
  - P2（质量）：单元测试/注释补全
  - P3（优化）：技术债优化/架构调整
- **实时进度汇总**：当前轮次核心成果、累计完成任务统计
- **风险预判与应对**：Worker与Auditor冲突时的处理
- **下一轮重点任务**：核心任务、次要任务、需重点关注项

**触发场景**：当询问"当前有什么任务"、"P0任务是什么"、"进度如何"时使用。

### 2. PROJECT_SPEC（项目规范）
**用途**：项目编码规范与合规标准

核心要求：
- 编码规范（禁止裸写print/log等）
- 组件复用要求（必须使用pkg/*组件）
- 审核规则（Auditor校验依据）

**重要**：当Worker代码与Auditor校验冲突时，需查阅此文档。

### 3. DAILY_REPORT（每日日报）
**用途**：每日09:00自动生成的夜间执行汇总

核心内容：
- 夜间执行成果汇总（任务完成、代码产出、组件复用）
- 规范合规情况（审核代码行数、合规率）
- 待审批事项（SPEC_CHANGE_REQUEST列表）
- 遗留任务与次日计划

**触发场景**：当询问"昨天的成果"、"夜间执行情况"时使用。

### 4. ROUND_REPORT（单轮执行报告）
**用途**：每轮执行完毕后自动生成的执行细节

核心内容：
- 本轮任务完成明细（P0-P3）
- 本轮代码产出统计（新增代码、重构、注释、测试）
- 规范合规详情（审核行数、合规率、违规列表）
- 触发的SPEC_CHANGE_REQUEST（若有）
- 本轮未完成任务
- 下一轮临时调整建议

**命名规则**：`ROUND_REPORT_YYYYMMDD_TYPE_XXX.md`
- YYYYMMDD：日期（如20260520）
- TYPE：需求类型（DEV/BUG/REFACTOR/OPTIMIZE/DOCS/OTHER）
- XXX：当日动态编号（001/002/...）
- 示例：`ROUND_REPORT_20260520_DEV_001.md`、`ROUND_REPORT_20260520_BUG_001.md`

**触发场景**：当询问"第X轮做了什么"、"本轮执行情况"时使用；为DAILY_REPORT提供原始数据。

### 5. SPEC_CHANGE_REQUEST（规范变更申请）
**用途**：规范冲突/试探时的变更申请模板

使用场景：
- Worker与Auditor出现规范冲突时
- AI试探规范边界成功/失败时
- 申请放宽或修改现有规范

核心流程：
1. 由Worker/Auditor触发，Planner汇总
2. 生成单独文档：`SPEC_CHANGE_REQUEST_XXX.md`
3. 需明确：申请原因、变更内容、影响范围
4. 人工审批（APPROVE/REJECT）
5. 审批后自动更新PROJECT_SPEC

**触发场景**：当需要申请规范变更、查看审批状态时使用。

## 文档关系图

```
MasterPlan_YYYYMMDD.md ←→ PROJECT_SPEC_YYYYMMDD.md
              ↓                    ↓
        Worker/Auditor ←→ 冲突检测
              ↓
ROUND_REPORT_YYYYMMDD_TYPE_XXX.md（单轮执行详情）
              ↓
SPEC_CHANGE_REQUEST_XXX.md（触发变更）
              ↓
         人工审批 → PROJECT_SPEC_YYYYMMDD.md更新
              ↓
DAILY_REPORT_YYYYMMDD.md（汇总所有ROUND_REPORT）
```

## AI使用指南

### 理解任务优先级
1. 查阅`docs/reports/MasterPlan_YYYYMMDD.md`的P0-P3任务池
2. P0任务必须强制复用指定组件
3. P1任务需对齐`docs/reports/PROJECT_SPEC_YYYYMMDD.md`规范

### 处理规范冲突
1. Worker编码 → Auditor校验
2. 若冲突，查阅`docs/templates/PROJECT_SPEC.md`确认规范
3. 若需变更，生成`docs/reports/SPEC_CHANGE_REQUEST_XXX.md`
4. 等待人工审批（APPROVE/REJECT）
5. 审批通过后更新`docs/reports/PROJECT_SPEC_YYYYMMDD.md`

### 汇报进度
- **单轮执行完成后**：立即生成`docs/reports/ROUND_REPORT_YYYYMMDD_TYPE_XXX.md`
- **查看单轮详情**：查阅对应日期和类型的ROUND_REPORT
- **查看历史汇总**：查看`docs/reports/DAILY_REPORT_YYYYMMDD.md`了解夜间整体成果
- 按MasterPlan格式更新任务进度到`docs/reports/MasterPlan_YYYYMMDD.md`
- 在`docs/reports/SPEC_CHANGE_REQUEST_XXX.md`记录变更历史

## 关键文件路径

### 模板文件（只读，不可修改）
- `docs/templates/MasterPlan.md` - 主规划文档模板
- `docs/templates/PROJECT_SPEC.md` - 项目规范模板
- `docs/templates/ROUND_REPORT.md` - 单轮执行报告模板
- `docs/templates/DAILY_REPORT.md` - 每日日报模板
- `docs/templates/SPEC_CHANGE_REQUEST.md` - 变更申请模板

### 实际文件（动态生成和更新）
- `docs/reports/MasterPlan_YYYYMMDD.md` - 主规划文档（日期版本）
- `docs/reports/PROJECT_SPEC_YYYYMMDD.md` - 项目规范（日期版本）
- `docs/reports/ROUND_REPORT_YYYYMMDD_TYPE_XXX.md` - 单轮执行报告
- `docs/reports/DAILY_REPORT_YYYYMMDD.md` - 每日日报
- `docs/reports/SPEC_CHANGE_REQUEST_XXX.md` - 单次变更申请（XXX为申请ID）
