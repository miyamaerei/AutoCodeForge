# 阶段十二文档索引：Sandbox 继承与 Repo 同步

**日期**: 2026-05-20  
**用途**: 统一导航阶段12的需求依据、设计方案、数据变更和执行顺序。

---

## 1. 核心入口

| 文档 | 作用 | 使用时机 |
|------|------|---------|
| docs/plans/12-phase-twelve-sandbox-repo-sync.md | 阶段12主计划（架构、任务、验收、数据表变更清单） | 开发与评审主依据 |
| docs/plans/00-execution-overview.md | 全局阶段总览与优先级 | 跨阶段排期与汇报 |

---

## 2. 需求与约束来源

| 文档 | 关键内容 | 本阶段引用点 |
|------|---------|------------|
| docs/settings-config-requirements.md | SandboxConfig 字段与校验规则 | 本地路径、超时、写路径策略 |
| docs/backend-development-requirements.md | Task/Repository/UserConfig 等后端实体与 API 约定 | 任务静态关联 + 用户隔离 |
| docs/project-features.md | 前端设置页与任务中心流程 | Settings/Sandbox + Repositories + Task Center 闭环 |

---

## 3. 数据库变更导航

阶段12数据表变更清单已并入主计划文档，包含：

1. 现有表增量字段（Tasks）。
2. 新增运行态关联表（RepoSandboxWorkspaces）。
3. 索引设计（查询与关联性能）。
4. 唯一约束与外键策略。
5. 迁移与回滚建议。

对应章节：
- docs/plans/12-phase-twelve-sandbox-repo-sync.md

---

## 4. 推荐实施顺序

1. 数据结构变更：先执行表结构迁移（字段、表、索引、约束）。
2. 配置能力增强：补 Sandbox 配置校验与用户隔离策略。
3. 路径能力实现：完成 SandboxPathResolver。
4. Git 执行能力：接入 LibGit2Sharp + GitCloneService。
5. 任务链路打通：RepoSyncTaskHandler + Endpoints。
6. 验证与联调：前端配置到本地落盘全链路验收。

---

## 5. 验收检查清单

1. 同一仓库不同用户任务，落盘目录互相隔离。
2. 单用户多任务目录不覆盖，可追溯到 TaskId。
3. RepoSandboxWorkspaces 与 Tasks/Repositories 关联完整。
4. 日志无明文凭据。
5. 失败场景可定位（认证失败、分支不存在、路径不可写）。

---

## 6. 维护约定

1. 阶段12若新增子方案，统一在本索引追加入口。
2. 变更数据库结构时，同步更新主计划中的“数据表变更清单”。
3. 里程碑汇报优先引用主计划，再引用本索引进行导航。
