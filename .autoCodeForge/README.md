# AutoCodeForge — .autoCodeForge 治理工作区

**projectId**: AutoCodeForge  
**language**: zh  
**初始化日期**: 2026-05-21  
**初始化模式**: full  
**命名策略**: safe（仅生成重命名计划，不自动改名）

---

## 一、用途说明

`.autoCodeForge/` 是项目文档治理的根目录，承载以下职责：

- 统一管控命名规范、归档规则与质量门禁
- 维护全量产出物索引与追踪映射
- 存放活跃计划、执行报告、规范变更的治理副本
- 区隔当前有效版本与历史归档

**本目录不存放业务代码，不参与编译，不包含运行时依赖。**

---

## 二、目录结构说明

| 目录 | 用途 |
|------|------|
| `config/` | 命名规则、归档规则、质量门禁定义 |
| `docs/` | 按 Diataxis 分层的治理文档（tutorial / how-to / reference / explanation） |
| `plans/` | 执行计划（active 活跃 / archived 已归档） |
| `reports/` | 执行报告（round 轮次 / daily 日报 / phase 阶段） |
| `specs/` | 规范文档（current 当前版本 / changes 变更申请） |
| `templates/` | 文档模板 |
| `logs/` | 执行日志与审计日志 |
| `registry/` | 产物索引与追踪映射 |
| `history/` | 历史版本归档（docs / reports / specs 细分子目录） |
| `trash/` | 废弃与冗余文件暂存区 |

---

## 三、快速入口

- 命名规则 → [config/naming-rules.md](./config/naming-rules.md)
- 归档规则 → [config/archive-rules.md](./config/archive-rules.md)
- 质量门禁 → [config/quality-gates.md](./config/quality-gates.md)
- 产物索引 → [registry/artifacts-index.md](./registry/artifacts-index.md)
- 追踪映射 → [registry/trace-map.md](./registry/trace-map.md)
- 全局索引 → [INDEX.md](./INDEX.md)

---

## 四、维护规范

1. 每次新建产出物后同步更新 `registry/artifacts-index.md`
2. 版本迭代时将旧版本移入 `history/` 对应子目录并更新索引
3. 修改命名规则前先在 `specs/changes/` 提交变更申请
4. `trash/` 目录在每月最后一个工作日统一清理

---

> 本文件由 `autocodeforge-doc-governance-bootstrap` Skill 自动生成  
> 如需修改规则，请走 SPEC_CHANGE_REQUEST 流程
