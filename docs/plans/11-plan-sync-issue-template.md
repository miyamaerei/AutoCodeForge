# Plan 对齐治理：Issue 模板与派单草案

**日期**: 2026-05-20  
**目的**: 解决 live plan 与历史 masterplan/report 事实不一致，建立可复用的任务分配模板。

---

## 1. 我是如何考虑的

1. 当前问题不是代码回归失败，而是文档事实源漂移。
2. 漂移会导致闭环验证被 requirement gate 阻塞，即使 build/test 全绿也不能 closure。
3. 需要一个固定 issue 模板，确保每次分配都包含 owner、解阻条件、验收口径与证据链接。

---

## 2. 复用设计

1. 复用闭环分类口径：code / test-case / environment / requirement。
2. 复用报告字段：failure_state、primary_category、evidence、owner、unblock_condition。
3. 复用计划目录编排：采用分阶段编号文件，便于轮次追踪。

---

## 3. 可复用 Issue 模板

### 3.1 模板名称
Plan-Sync Drift Issue Template

### 3.2 模板正文（复制后直接填空）

标题：
[REQ][P0] 对齐 Plan 事实源漂移 - <RequirementId 或日期>

类型：
REQ

优先级：
P0

背景：
发现 live plan 与历史 masterplan / round report 出现事实不一致，导致 requirement mapping gate 未通过。

影响范围：
- docs/MasterPlan.md
- docs/reports/MasterPlan_YYYYMMDD.md
- docs/reports/ROUND_REPORT_YYYYMMDD_TYPE_XXX.md

失败归因：
- primary_category: requirement
- secondary_categories: documentation-drift
- failure_state: blocked
- failing_check_id: RG-MAP-001

错误签名：
<一句话描述冲突，例如：轮次、阶段完成度、下一轮任务冲突>

Owner：
<@Worker 或 @Auditor 或具体负责人>

解阻条件（必须可验证）：
1. 当前轮次字段与最新 round report 一致。
2. 阶段完成度与任务状态一致。
3. 下一轮任务与最新里程碑一致。
4. 完成一次 validate-only 复核并产出 round report。

任务拆解：
1. 对齐 live plan 的基础信息与全局任务池。
2. 对齐进度汇总与风险项。
3. 追加验证轮次报告并标注 done 或 blocked。
4. 回填证据链接。

验收标准：
1. RG-BUILD-001 通过。
2. RG-TEST-001 通过。
3. RG-MAP-001 通过。
4. 报告中明确记录 no re-development executed（若为 validate-only）。

证据链接：
- <round report path>
- <masterplan path>
- <log / diff path>

截止时间：
<YYYY-MM-DD HH:mm>

---

## 4. 本次可直接分配的 Issue 草案

标题：
[REQ][P0] 对齐 Plan 事实源漂移 - RQ-MASTERPLAN-20260520-01

类型：
REQ

优先级：
P0

背景：
validate-only 轮次中，回归门禁通过，但 requirement mapping 未通过，原因是 live plan 与历史报告事实不一致。

影响范围：
- docs/MasterPlan.md
- docs/reports/MasterPlan_20260520.md
- docs/reports/ROUND_REPORT_20260520_DEV_005.md
- docs/reports/ROUND_REPORT_20260520_OTHER_002.md

Owner：
@Auditor

解阻条件：
1. 轮次字段、阶段三状态、下一轮任务完成统一对齐。
2. 复核后新增一份 validate-only round report，状态为 done。

建议排期：
- 预计工时：0.3h
- 截止时间：2026-05-21 12:00

---

## 5. 文件路径

- docs/plans/11-plan-sync-issue-template.md

## 6. 产出物

1. 可复用 issue 模板。
2. 一条可立即派发的 REQ issue 草案。

## 7. 验证方式

1. 抽样检查新 issue 是否含 owner、解阻条件、验收标准。
2. 检查是否可直接映射到 docs/MasterPlan.md 回流记录字段。
3. 执行一次 validate-only 复核，确认流程可闭环。
