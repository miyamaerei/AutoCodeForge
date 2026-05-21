# AutoCodeForge — 产出物索引

> 版本：v1.0.0 | 日期：2026-05-21  
> 每次新建/重命名/归档产出物后，必须同步更新本文件

---

## 更新说明

- `status` 可选值：`active`（当前有效）、`archived`（已归档至 history）、`deprecated`（废弃）
- `type` 对应 naming-rules.md 中的文档分类
- `location` 使用相对于项目根目录的路径

---

## 一、治理文档（Governance Docs）

| 文件名 | 类型 | 版本 | 状态 | 位置 | 备注 |
|--------|------|------|------|------|------|
| AutoCodeForge-Architecture-v1.0.0-20260521.md | governance | v1.0.0 | active | docs/ | ✅ 命名合规 |
| AutoCodeForge-CodeOpinion-v1.0.0-20260521.md | governance | v1.0.0 | active | docs/ | ✅ 命名合规 |
| AutoCodeForge-GovernanceInit-v1.0.0-20260521.md | governance | v1.0.0 | active | docs/ | ✅ 命名合规 |
| AutoCodeForge-OutputSummary-v1.0.0-20260521.md | governance | v1.0.0 | active | docs/ | ✅ 命名合规 |
| AutoCodeForge-ProjectOverview-v1.0.0-20260521.md | governance | v1.0.0 | active | docs/ | ✅ 命名合规 |
| AutoCodeForge-ProjectSkill-v1.0.0-20260521.md | governance | v1.0.0 | active | docs/ | ✅ 命名合规 |

---

## 二、技术/模块文档（Technical Docs）

| 文件名 | 类型 | 状态 | 位置 | 备注 |
|--------|------|------|------|------|
| backend-config-api.md | technical | active | docs/ | ⚠️ 缺版本/日期（P2） |
| backend-config-architecture.md | technical | active | docs/ | ⚠️ 缺版本/日期（P2） |
| backend-development-requirements.md | technical | active | docs/ | ⚠️ 缺版本/日期（P2） |
| backend-priority.md | technical | active | docs/ | ⚠️ 缺版本/日期（P2） |
| backend-roadmap.md | technical | active | docs/ | ⚠️ 缺版本/日期（P2） |
| backend-skills-autocodeforge.md | technical | active | docs/ | ⚠️ 缺版本/日期（P2） |
| code-compliance-checklist.md | spec | active | docs/ | ⚠️ 建议改名为 SPEC_ 前缀或治理文档格式（P2） |
| config-management-optimization.md | technical | active | docs/ | ⚠️ 缺版本/日期（P2） |
| frontend-system-config-analysis.md | technical | active | docs/ | ⚠️ 缺版本/日期（P2） |
| MasterPlan.md | governance | active | docs/ | ⚠️ 建议改名为 AutoCodeForge-MasterPlan-v*.*.*.md（P2） |
| project-features.md | technical | active | docs/ | ⚠️ 缺版本/日期（P2） |
| PROJECT_SPEC.md | spec | active | docs/ | ✅ 符合 SPEC_ 前缀规范 |
| requirement.txt | other | active | docs/ | ℹ️ 非 .md 格式，不适用命名规则 |
| reuse-guidelines.md | technical | active | docs/ | ⚠️ 缺版本/日期（P2） |
| settings-config-requirements.md | technical | active | docs/ | ⚠️ 缺版本/日期（P2） |
| user-guide.md | technical | active | docs/ | ⚠️ 缺版本/日期（P2） |
| 文档结构与标准-v1.0.0-20260521.md | governance | active | docs/ | ❌ 含中文字符（P0），见重命名计划 |

---

## 三、报告文档（Reports）

| 文件名 | 类型 | 状态 | 位置 | 备注 |
|--------|------|------|------|------|
| ROUND_REPORT_20260520_DEV_001.md ~ DEV_022.md | report-round | active | docs/reports/ | ✅ 符合项目既有报告命名约定 |
| ROUND_REPORT_20260520_DOCS_001.md | report-round | active | docs/reports/ | ✅ 合规 |
| ROUND_REPORT_20260520_OTHER_001.md ~ OTHER_004.md | report-round | active | docs/reports/ | ✅ 合规 |
| CODE_REVIEW_20260520_PHASE10.md | report-review | active | docs/reports/ | ✅ 合规 |
| ISSUE_20260520_PLAN_SYNC_DRIFT.md | report-issue | active | docs/reports/ | ✅ 合规 |
| MasterPlan_20260520.md | snapshot | active | docs/reports/ | ⚠️ 使用下划线，与主 MasterPlan.md 命名体系混用（P1） |
| PHASE_07_DELIVERY_SUMMARY.md | report-phase | active | docs/reports/ | ✅ 合规 |
| PHASE_14_EXECUTION_PLAN.md | plan | active | docs/reports/ | ⚠️ 计划文件存放在 reports/ 目录下（归档位置建议迁移） |
| PHASE_14_START_CHECKLIST.md | checklist | active | docs/reports/ | ⚠️ 同上 |

---

## 四、模板文档（Templates）

| 文件名 | 类型 | 状态 | 位置 | 备注 |
|--------|------|------|------|------|
| DAILY_REPORT.md | template | active | docs/templates/ | ✅ 合规 |
| MasterPlan.md | template | active | docs/templates/ | ✅ 合规（模板语义） |
| PROJECT_SPEC.md | template | active | docs/templates/ | ✅ 合规 |
| ROUND_REPORT.md | template | active | docs/templates/ | ✅ 合规 |
| SPEC_CHANGE_REQUEST.md | template | active | docs/templates/ | ✅ 合规 |

---

## 五、治理工作区文件（.autoCodeForge）

| 文件名 | 类型 | 版本 | 状态 | 位置 |
|--------|------|------|------|------|
| README.md | governance | v1.0.0 | active | .autoCodeForge/ |
| INDEX.md | governance | v1.0.0 | active | .autoCodeForge/ |
| config/naming-rules.md | config | v1.0.0 | active | .autoCodeForge/config/ |
| config/archive-rules.md | config | v1.0.0 | active | .autoCodeForge/config/ |
| config/quality-gates.md | config | v1.0.0 | active | .autoCodeForge/config/ |
| registry/artifacts-index.md | registry | v1.0.0 | active | .autoCodeForge/registry/ |
| registry/trace-map.md | registry | v1.0.0 | active | .autoCodeForge/registry/ |

---

> 本文件由 `autocodeforge-doc-governance-bootstrap` Skill 自动生成  
> 最后同步：2026-05-21
