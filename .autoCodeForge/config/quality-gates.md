# AutoCodeForge — 质量门禁

> 版本：v1.0.0 | 日期：2026-05-21  
> **变更须走 SPEC_CHANGE_REQUEST 流程**

---

## 一、门禁清单

### Q1 — 目录结构完整性

**检查内容**：`.autoCodeForge/` 必需目录是否全部存在  
**触发时机**：每次执行 bootstrap 或结构变更后  
**判定标准（full 模式）**：

```
必须存在：
  .autoCodeForge/config/
  .autoCodeForge/docs/
  .autoCodeForge/plans/active/
  .autoCodeForge/plans/archived/
  .autoCodeForge/reports/round/
  .autoCodeForge/reports/daily/
  .autoCodeForge/reports/phase/
  .autoCodeForge/specs/current/
  .autoCodeForge/specs/changes/
  .autoCodeForge/templates/
  .autoCodeForge/logs/execution/
  .autoCodeForge/logs/audits/
  .autoCodeForge/registry/
  .autoCodeForge/history/docs/
  .autoCodeForge/history/reports/
  .autoCodeForge/history/specs/
  .autoCodeForge/trash/
```

**判定标准（quick 模式）**：

```
必须存在：
  .autoCodeForge/config/
  .autoCodeForge/registry/
```

**通过条件**：所有必需目录存在  
**不通过处理**：列出缺失目录，触发创建补全

---

### Q2 — 命名规范合规率

**检查内容**：目标范围内文件名是否符合 `config/naming-rules.md` 定义的模式  
**触发时机**：运行 governance scan 时  
**计算方法**：

```
合规率 = 合规文件数 / 总文件数 × 100%
```

**通过条件**：合规率 ≥ 95%  
**豁免范围**：`history/`、`trash/`、`.gitkeep`、`README.md`、`INDEX.md`  
**不通过处理**：输出 rename-plan，按违规分级（P0/P1/P2/P3）排序

---

### Q3 — 产物索引完整性

**检查内容**：`registry/artifacts-index.md` 是否存在且包含全部已知产出物  
**触发时机**：新建/重命名/归档产出物后  
**通过条件**：
1. 文件存在且非空
2. `docs/` 下每个 `.md` 文件在索引中均有对应条目
3. 每个条目包含 `name`、`type`、`status`、`location` 四个字段

**不通过处理**：列出未登记文件，要求补充索引条目

---

### Q4 — 规则文件完整性

**检查内容**：核心规则文件是否存在且非空  
**触发时机**：bootstrap 完成后  
**通过条件**：以下文件均存在且文件大小 > 0：

```
.autoCodeForge/config/naming-rules.md
.autoCodeForge/config/archive-rules.md
.autoCodeForge/config/quality-gates.md
```

**不通过处理**：标记为 BLOCKER，要求立即补充

---

### Q5 — 当前/历史不重叠

**检查内容**：同一文件不能同时出现在当前位置和 `history/` 目录中（相同文件名，不含 history 后缀）  
**触发时机**：归档操作完成后  
**通过条件**：`history/` 中的文件名在当前目录中不存在同名文件（不含后缀差异）  
**不通过处理**：列出冲突对，要求确认哪个为 canonical

---

## 二、门禁汇总表

| 门禁 | 描述 | 级别 | 阻断发布 |
|------|------|------|---------|
| Q1 | 目录结构完整性 | BLOCKER | ✅ |
| Q2 | 命名规范合规率 ≥ 95% | WARNING（< 95% 时降为 BLOCKER） | 视情况 |
| Q3 | 产物索引完整性 | WARNING | ❌ |
| Q4 | 规则文件完整性 | BLOCKER | ✅ |
| Q5 | 当前/历史不重叠 | WARNING | ❌ |

---

## 三、运行输出格式

```
=== AutoCodeForge Quality Gate Report ===
Date: YYYY-MM-DD
Mode: full | quick

[Q1] 目录结构完整性 ............. PASS | FAIL
[Q2] 命名规范合规率 ............. PASS (98%) | FAIL (87%)
[Q3] 产物索引完整性 ............. PASS | WARN (3 files unregistered)
[Q4] 规则文件完整性 ............. PASS | FAIL
[Q5] 当前/历史不重叠 ............ PASS | WARN (1 conflict found)

Overall: PASS | FAIL (N blockers, M warnings)
==========================================
```

---

## 四、变更记录

| 版本 | 日期 | 变更说明 | 来源 |
|------|------|----------|------|
| v1.0.0 | 2026-05-21 | 初始创建 | autocodeforge-doc-governance-bootstrap |
