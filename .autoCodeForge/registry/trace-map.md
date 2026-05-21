# AutoCodeForge — 产出物追踪映射

> 版本：v1.0.0 | 日期：2026-05-21  
> 记录每个产出物的来源 Skill/流程，支持产出物可追溯

---

## 映射说明

| 字段 | 含义 |
|------|------|
| `artifact` | 产出物文件名（相对路径） |
| `source-skill` | 生成该产出物的 Skill 或流程名称 |
| `trigger` | 触发方式 |
| `created` | 创建日期 |
| `last-updated` | 最近更新日期 |

---

## 一、治理工作区产出物

| artifact | source-skill | trigger | created | last-updated |
|----------|-------------|---------|---------|--------------|
| .autoCodeForge/README.md | autocodeforge-doc-governance-bootstrap | 初始化 full | 2026-05-21 | 2026-05-21 |
| .autoCodeForge/INDEX.md | autocodeforge-doc-governance-bootstrap | 初始化 full | 2026-05-21 | 2026-05-21 |
| .autoCodeForge/config/naming-rules.md | autocodeforge-doc-governance-bootstrap | 初始化 full | 2026-05-21 | 2026-05-21 |
| .autoCodeForge/config/archive-rules.md | autocodeforge-doc-governance-bootstrap | 初始化 full | 2026-05-21 | 2026-05-21 |
| .autoCodeForge/config/quality-gates.md | autocodeforge-doc-governance-bootstrap | 初始化 full | 2026-05-21 | 2026-05-21 |
| .autoCodeForge/registry/artifacts-index.md | autocodeforge-doc-governance-bootstrap | 初始化 full | 2026-05-21 | 2026-05-21 |
| .autoCodeForge/registry/trace-map.md | autocodeforge-doc-governance-bootstrap | 初始化 full | 2026-05-21 | 2026-05-21 |

---

## 二、已有文档（项目交付阶段产出）

| artifact | source-skill | trigger | created | last-updated |
|----------|-------------|---------|---------|--------------|
| docs/AutoCodeForge-Architecture-v1.0.0-20260521.md | autocodeforge-doc-governance-bootstrap | 治理初始化 | 2026-05-21 | 2026-05-21 |
| docs/AutoCodeForge-CodeOpinion-v1.0.0-20260521.md | CodeOpinionAnalyzeSkill（推测） | 代码意见分析 | 2026-05-21 | 2026-05-21 |
| docs/AutoCodeForge-GovernanceInit-v1.0.0-20260521.md | autocodeforge-doc-governance-bootstrap | 治理初始化 | 2026-05-21 | 2026-05-21 |
| docs/AutoCodeForge-OutputSummary-v1.0.0-20260521.md | AllOutputSummarySkill（推测） | 产出物归总 | 2026-05-21 | 2026-05-21 |
| docs/AutoCodeForge-ProjectOverview-v1.0.0-20260521.md | 人工编写 | 项目概述 | 2026-05-21 | 2026-05-21 |
| docs/AutoCodeForge-ProjectSkill-v1.0.0-20260521.md | 人工编写 | 技能清单 | 2026-05-21 | 2026-05-21 |
| docs/MasterPlan.md | auto-developer / masterplan-sync | 持续更新 | — | — |
| docs/PROJECT_SPEC.md | autocodeforge-doc-governance-bootstrap | 规范宪法 | — | — |
| docs/reports/ROUND_REPORT_20260520_*.md | auto-developer | 每轮自动生成 | 2026-05-20 | 2026-05-20 |
| docs/reports/CODE_REVIEW_20260520_PHASE10.md | auto-delivery-loop / report-writer | 代码审查 | 2026-05-20 | 2026-05-20 |
| docs/reports/PHASE_07_DELIVERY_SUMMARY.md | auto-delivery-loop | 阶段交付 | 2026-05-20 | 2026-05-20 |
| docs/templates/*.md | 人工编写 | 模板初始化 | — | — |

---

## 三、更新流程

新增产出物时，在本文件末尾对应分类下追加一行，格式示例：

```
| docs/reports/REPORT_ROUND_DEV_20260522_001.md | auto-developer | DEV 轮次完成 | 2026-05-22 | 2026-05-22 |
```

---

> 本文件由 `autocodeforge-doc-governance-bootstrap` Skill 自动生成  
> 最后同步：2026-05-21
