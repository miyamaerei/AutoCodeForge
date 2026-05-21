# AutoCodeForge — .autoCodeForge 全局索引

> 最后更新：2026-05-21 | 初始化版本

---

## 一、治理文件

| 文件 | 类型 | 版本 | 说明 |
|------|------|------|------|
| [config/naming-rules.md](./config/naming-rules.md) | 规则 | v1.0.0 | 文件命名强制规范 |
| [config/archive-rules.md](./config/archive-rules.md) | 规则 | v1.0.0 | 当前 vs 历史归档拆分策略 |
| [config/quality-gates.md](./config/quality-gates.md) | 规则 | v1.0.0 | 治理质量门禁定义 |

---

## 二、注册表

| 文件 | 类型 | 说明 |
|------|------|------|
| [registry/artifacts-index.md](./registry/artifacts-index.md) | 索引 | 全量产出物登记表 |
| [registry/trace-map.md](./registry/trace-map.md) | 映射 | 产出物 → 来源 Skill/流程 映射 |

---

## 三、活跃计划

> 路径：`plans/active/`  
> 当前无条目，请在创建计划时同步在此处登记。

| 文件 | 范围 | 状态 |
|------|------|------|
| — | — | — |

---

## 四、活跃报告（最近 10 条）

> 路径：`reports/round/` `reports/daily/` `reports/phase/`  
> 当前无条目，请在生成报告时同步在此处登记。

| 文件 | 类型 | 日期 |
|------|------|------|
| — | — | — |

---

## 五、当前规范

> 路径：`specs/current/`  
> 请在引入新规范时登记。

| 文件 | 版本 | 变更来源 |
|------|------|----------|
| — | — | — |

---

## 六、目录结构快照

```text
.autoCodeForge/
  README.md
  INDEX.md
  config/
    naming-rules.md
    archive-rules.md
    quality-gates.md
  docs/
    tutorial/
    how-to/
    reference/
    explanation/
  plans/
    active/
    archived/
  reports/
    round/
    daily/
    phase/
  specs/
    current/
    changes/
  templates/
  logs/
    execution/
    audits/
  registry/
    artifacts-index.md
    trace-map.md
  history/
    docs/
    reports/
    specs/
  trash/
```

---

> 本文件由 `autocodeforge-doc-governance-bootstrap` Skill 自动生成
