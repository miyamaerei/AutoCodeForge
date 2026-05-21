# AutoCodeForge — 归档规则

> 版本：v1.0.0 | 日期：2026-05-21  
> **变更须走 SPEC_CHANGE_REQUEST 流程**

---

## 一、当前 vs 历史 区分原则

| 类别 | 当前版本存放位置 | 历史版本存放位置 |
|------|----------------|----------------|
| 治理文档 | `docs/` | `.autoCodeForge/history/docs/` |
| 执行报告 | `docs/reports/` | `.autoCodeForge/history/reports/` |
| 规范文档 | `docs/` + `.autoCodeForge/specs/current/` | `.autoCodeForge/history/specs/` |
| 计划文档 | `docs/plans/` + `.autoCodeForge/plans/active/` | `.autoCodeForge/plans/archived/` |
| 模板文件 | `docs/templates/` + `.autoCodeForge/templates/` | `.autoCodeForge/history/docs/` |

**原则**：同一文件的多个版本中，**最新 approved 版本留在当前位置**，旧版本追加 `-history-v*.*.*` 后缀后移入对应 `history/` 子目录。

---

## 二、归档触发条件

| 触发条件 | 处理动作 |
|----------|----------|
| 文件发布新版本（版本号递增） | 旧版本移入 `history/`，更新 registry |
| 同一语义文件存在重复 | 保留第一个为 canonical，其余追加 `-r2`, `-r3` 并移入 `history/` |
| 文件被明确废弃（deprecated） | 移入 `.autoCodeForge/trash/`，在 registry 中标记 `status: deprecated` |
| 每月末定期清理 | 清空 `trash/`，归档或删除确认无需保留的文件 |

---

## 三、自动归档目录层级（DocArchiveManageSkill 兼容）

与需求文档 §4.2 对齐，以下目录存放对应类型产出物：

```text
docs/
  ├─ 01_init-archive/        初始化相关配置、清单、日志
  ├─ 02_planning-archive/    项目规划、框架选型文档
  ├─ 03_design-archive/      模块设计、接口设计文档
  ├─ 04_api-archive/         接口文档、接口模板
  ├─ 05_change-archive/      版本变更、流程调整相关文档
  ├─ 06_code-opinion/        CodeOpinionAnalyzeSkill 意见报告
  ├─ 07_skill-modify-guide/  SkillModifyGuideSkill 指导文档
  ├─ 08_output-summary/      AllOutputSummarySkill 归总清单
  ├─ 09_history/             历史版本文档（按类型细分子目录）
  └─ 99_trash/               废弃冗余文件暂存区
```

> 上述目录在项目根 `docs/` 下按需创建；`.autoCodeForge/` 内的 `history/` 存放的是治理工作区内部文件的历史副本。

---

## 四、版本标记规则

| 操作 | 文件名变化 |
|------|------------|
| 发布 v1.1.0，旧版为 v1.0.0 | 旧文件重命名为 `...-history-v1.0.0.md` 移入 history |
| 同日期多次发布 | 后续版本追加 `-r2`、`-r3` |
| 废弃文件 | 追加 `-deprecated` 并移入 trash |

---

## 五、registry 同步要求

归档操作完成后，必须在 **同一次提交** 中更新：
1. `registry/artifacts-index.md` — 更新 `status`、`location` 字段
2. `registry/trace-map.md` — 若产出物来源发生变化，同步更新映射

---

## 六、变更记录

| 版本 | 日期 | 变更说明 | 来源 |
|------|------|----------|------|
| v1.0.0 | 2026-05-21 | 初始创建 | autocodeforge-doc-governance-bootstrap |
