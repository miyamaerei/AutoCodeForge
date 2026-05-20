# ISSUE_20260520_PLAN_SYNC_DRIFT

## 标题
[REQ][P0] 对齐 Plan 事实源漂移 - RQ-MASTERPLAN-20260520-01

## 类型
REQ

## 优先级
P0

## 背景
在 validate-only 验证中，build 与 test 均通过，但 requirement mapping gate 失败。
根因是 live plan 与历史 masterplan / round report 在轮次、阶段进度、下一轮任务上出现事实不一致。

## 影响范围
- docs/MasterPlan.md
- docs/reports/MasterPlan_20260520.md
- docs/reports/ROUND_REPORT_20260520_DEV_005.md
- docs/reports/ROUND_REPORT_20260520_OTHER_002.md

## 归因与状态
- failure_state: blocked
- primary_category: requirement
- secondary_categories: documentation-drift
- failing_check_id: RG-MAP-001

## Owner
@Auditor

## 解阻条件
1. 轮次、阶段状态、下一轮任务在 live plan 与报告中一致。
2. 完成一次 validate-only 复核并产出新 round report。
3. 复核结论达到 done。

## 任务清单
1. 对齐 docs/MasterPlan.md 的基础信息与任务池事实。
2. 对齐 docs/reports/MasterPlan_20260520.md 的摘要口径。
3. 追加 validate-only 报告并附证据链接。

## 验收标准
1. RG-BUILD-001: PASS
2. RG-TEST-001: PASS
3. RG-MAP-001: PASS

## 预计工时
0.3h

## 目标截止
2026-05-21 12:00
