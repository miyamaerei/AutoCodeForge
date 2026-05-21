# AutoCodeForge — 文件命名规范

> 版本：v1.0.0 | 日期：2026-05-21  
> **变更须走 SPEC_CHANGE_REQUEST 流程，禁止直接修改本文件**

---

## 一、通用字符规则

| 规则 | 说明 |
|------|------|
| 字符集 | 仅 ASCII 字母、数字、连字符 `-`、下划线 `_`、点 `.` |
| 大小写 | 前缀固定大写（`REPORT_`、`PLAN_`、`SPEC_`）；治理文档 PascalCase segment |
| 空格 | **禁止**，使用 `-` 或 `_` 替代 |
| 混合分隔符 | **禁止**，一个文件名内只使用一种分隔符风格 |
| 日期格式 | `YYYYMMDD`（如 `20260521`）|
| 版本格式 | `vMAJOR.MINOR.PATCH`（如 `v1.0.0`）|
| 中文字符 | **禁止**（safe 模式下仅列入重命名计划，strict 模式下自动改名）|

---

## 二、核心命名模式

### 2.1 治理文档

```
<projectId>-<DocType>-v<major>.<minor>.<patch>-<YYYYMMDD>.md
```

| 段 | 说明 | 示例 |
|---|---|---|
| `projectId` | 项目标识符，PascalCase | `AutoCodeForge` |
| `DocType` | 文档类型，PascalCase | `Architecture` / `ProjectOverview` / `MasterPlan` |
| `v*.*.*` | 语义版本 | `v1.0.0` |
| `YYYYMMDD` | 生成日期 | `20260521` |

✅ 示例：`AutoCodeForge-Architecture-v1.0.0-20260521.md`  
❌ 错误：`architecture.md`、`AutoCodeForge_Architecture_v1.md`

---

### 2.2 报告文档

```
REPORT_<KIND>_<YYYYMMDD>_<SEQ3>.md
```

| 段 | 说明 | 示例 |
|---|---|---|
| `KIND` | 报告类型 SCREAMING_SNAKE | `ROUND_DEV` / `DAILY` / `PHASE` |
| `YYYYMMDD` | 报告日期 | `20260521` |
| `SEQ3` | 当日三位流水号 | `001` |

✅ 示例：`REPORT_ROUND_DEV_20260521_001.md`  

> **向后兼容说明**：现有 `ROUND_REPORT_YYYYMMDD_TYPE_XXX.md` 模式在 AutoCodeForge 项目中已大量沉积，视为**已有合规变体**，不强制迁移。新生成报告推荐使用本模式。

---

### 2.3 计划文档

```
PLAN_<SCOPE>_<YYYYMMDD>_<SEQ3>.md
```

✅ 示例：`PLAN_PHASE15_20260521_001.md`

---

### 2.4 规范变更申请

```
SPEC_CHANGE_REQUEST_<ID>.md
```

| 段 | 说明 | 示例 |
|---|---|---|
| `ID` | 唯一申请 ID | `SCR-001` / `SCR-002` |

✅ 示例：`SPEC_CHANGE_REQUEST_SCR-001.md`

---

### 2.5 代码/配置模板

```
TEMPLATE_<DOMAIN>_<PURPOSE>.<ext>
```

✅ 示例：`TEMPLATE_User_AddUser.cs`、`TEMPLATE_Pipeline_Trigger.yaml`

---

### 2.6 技术/模块设计文档

```
<module>-<stage>-<subject>.md
```

全小写 + 连字符，适用于模块层面技术文档（非治理文档）。

✅ 示例：`backend-config-architecture.md`  
✅ 示例：`user-module-design-entity.md`

---

### 2.7 历史版本文件

在原文件扩展名前追加 `-history-v<major>.<minor>.<patch>`：

```
<原始名>-history-v<major>.<minor>.<patch>.md
```

✅ 示例：`AutoCodeForge-Architecture-v1.0.0-20260521-history-v0.9.0.md`

---

### 2.8 代码意见分析文档

```
<SkillName>-Opinion-v<major>.<minor>.<patch>-<YYYYMMDD>.md
```

✅ 示例：`TemplateGenerateSkill-Opinion-v1.0.0-20260521.md`

---

### 2.9 Skill 修改指导文档

```
SkillModify-Guide-<SkillName>-v<major>.<minor>.<patch>-<YYYYMMDD>.md
```

✅ 示例：`SkillModify-Guide-InitProjectSkill-v1.0.0-20260521.md`

---

### 2.10 产出物归总清单

```
<projectId>-OutputSummary-v<major>.<minor>.<patch>-<YYYYMMDD>.md
```

✅ 示例：`AutoCodeForge-OutputSummary-v1.0.0-20260521.md`

---

## 三、违规分级

| 级别 | 场景 | 处理 |
|------|------|------|
| P0 严重 | 含中文字符、含空格 | 立即列入 rename-plan；strict 模式自动改名 |
| P1 高 | 混用分隔符 | 列入 rename-plan |
| P2 中 | 缺少版本或日期（治理文档） | 列入建议清单 |
| P3 低 | 大小写不规范 | 记录但不主动更改 |

---

## 四、变更记录

| 版本 | 日期 | 变更说明 | 来源 |
|------|------|----------|------|
| v1.0.0 | 2026-05-21 | 初始创建 | autocodeforge-doc-governance-bootstrap |
