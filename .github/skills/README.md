# Skill 模板完整文档体系

> 这个目录包含 AutoCodeForge 项目中 Skill 系统的完整文档和测试示例。

## 📂 文档结构

```
.github/skills/
├── README.md                             ← 👈 您在这里
├── QUICK_REFERENCE.md                   ← ⭐ 快速参考（推荐先读）
├── SKILL_TEMPLATE_GUIDE.md              ← 详细模板指南
├── SKILL_EXAMPLES_ANALYSIS.md           ← 深入分析对比
│
├── 🧪 测试 Skill 示例
│   ├── test-data-generator-skill/
│   │   └── SKILL.md                     ← 数据生成类示例
│   ├── code-quality-validate-skill/
│   │   └── SKILL.md                     ← 验证类示例 (代码质量)
│   └── api-contract-validate-skill/
│       └── SKILL.md                     ← 验证类示例 (API 合约)
│
└── 📦 已有的 Skill 实现
    ├── init-project-skill/
    ├── config-init-skill/
    ├── folder-struct-skill/
    ├── base-develop-skill/
    ├── dependency-install-skill/
    ├── template-generate-skill/
    ├── code-rule-skill/
    ├── git-flow-skill/
    └── ...
```

## 🎯 快速开始

### 我想创建一个新 Skill

**第一步**：阅读快速参考（5 分钟）
```bash
# 打开这个文件
code .github/skills/QUICK_REFERENCE.md
```

**第二步**：学习模板标准（15 分钟）
```bash
# 参考完整模板指南
code .github/skills/SKILL_TEMPLATE_GUIDE.md
```

**第三步**：选择合适的示例（5 分钟）
```bash
# 查看与你的 Skill 类型相似的示例
# 数据生成类? → 看 test-data-generator-skill/
# 验证/检查类? → 看 code-quality-validate-skill/
```

**第四步**：创建你的 Skill

```bash
mkdir -p .github/skills/[your-skill-name]
# 复制最接近的示例并修改
```

### 我想理解 Skill 的设计原理

**快速路线（30 分钟）**：
1. 读本文件 - 5 分钟
2. 读 QUICK_REFERENCE.md - 10 分钟  
3. 对比三个测试 Skill - 15 分钟

**深入路线（60 分钟）**：
1. 读 SKILL_TEMPLATE_GUIDE.md - 20 分钟
2. 读 SKILL_EXAMPLES_ANALYSIS.md - 25 分钟
3. 阅读项目中的其他 Skill - 15 分钟

### 我想看实际例子

**最简单的例子**：
```bash
# 查看项目中已有的简单 Skill
code init-project-skill/SKILL.md
code config-init-skill/SKILL.md
```

**中等复杂度的例子**：
```bash
# 查看我们创建的测试 Skill
code test-data-generator-skill/SKILL.md
code code-quality-validate-skill/SKILL.md
```

**复杂的例子**：
```bash
# 查看高级 Skill
code api-contract-validate-skill/SKILL.md
code auto-delivery-loop/SKILL.md
```

## 📚 文档导航

| 文档 | 长度 | 难度 | 用途 | 何时阅读 |
|------|------|------|------|---------|
| **QUICK_REFERENCE.md** | 10 min | ⭐ | 快速参考 | 🔴 首次使用 |
| **SKILL_TEMPLATE_GUIDE.md** | 20 min | ⭐⭐ | 详细指南 | 🔴 创建新 Skill |
| **SKILL_EXAMPLES_ANALYSIS.md** | 25 min | ⭐⭐⭐ | 深入分析 | 🟡 提升理解 |
| **test-data-generator-skill** | 5 min | ⭐⭐ | 代码示例 | 🟢 参考实现 |
| **code-quality-validate-skill** | 5 min | ⭐⭐ | 代码示例 | 🟢 参考实现 |
| **api-contract-validate-skill** | 8 min | ⭐⭐⭐ | 代码示例 | 🟢 参考实现 |

**图例**: 🔴 必读 / 🟡 推荐 / 🟢 参考

## 🎓 学习路径

### 路径 A: 快速上手（适合时间紧张的人）
```
1. QUICK_REFERENCE.md (10 min)
   ↓
2. 选择相似示例 (2 min)
   ↓
3. 开始编写你的 Skill (30 min)
```

### 路径 B: 标准学习（推荐）
```
1. QUICK_REFERENCE.md (10 min)
   ↓
2. SKILL_TEMPLATE_GUIDE.md (20 min)
   ↓
3. 研究一个示例 (10 min)
   ↓
4. 创建你的 Skill (45 min)
   ↓
5. 参考 DONE_CHECKS 完成质量检查 (10 min)
```

### 路径 C: 深度学习（追求完全理解）
```
1. QUICK_REFERENCE.md (10 min)
   ↓
2. SKILL_TEMPLATE_GUIDE.md (20 min)
   ↓
3. SKILL_EXAMPLES_ANALYSIS.md (25 min)
   ↓
4. 研究所有 3 个测试示例 (15 min)
   ↓
5. 阅读项目中的其他 Skill (30 min)
   ↓
6. 创建高质量 Skill (60 min)
```

## 📋 Skill 标准模板

最小化版本（快速参考）：

```yaml
---
name: YourSkillNameHere
description: "What this skill does in one sentence."
argument-hint: "What inputs user should provide."
---

# YourSkillNameHere

## Purpose
- Primary objective 1
- Primary objective 2

## Inputs
1. **Input 1**: Description
2. **Input 2**: Description
3. **Input 3**: Description (optional: explain when optional)

## Workflow
1. First step description
2. Second step description
3. Third step description
4. Fourth step description
5. Fifth step description
6. Sixth step description

## Outputs
- Output artifact 1
- Output artifact 2
- Output artifact 3

## Done Checks
- Completion check 1
- Completion check 2
- Completion check 3
- Completion check 4
```

## 🔑 关键概念

### Skill 是什么？
一个**自包含的、可重复使用的**能力或工作单元，用于完成项目初始化、开发或部署中的特定任务。

### Skill 的特点
- ✅ **单一职责**: 一个 Skill 做一件事
- ✅ **明确输入**: 清晰定义需要什么
- ✅ **明确输出**: 清晰定义产生什么
- ✅ **可验证**: 完成状态可以检查
- ✅ **可重用**: 多个项目可以使用
- ✅ **可组合**: 可以与其他 Skill 组合

### Skill 生命周期

```
┌─────────────┐
│   创建      │
│ Create new  │
└──────┬──────┘
       │
       ↓
┌─────────────┐     ┌──────────────┐
│   注册      │────→│  添加到流程  │
│ Register    │     │ Add to flow  │
└──────┬──────┘     └──────────────┘
       │
       ↓
┌─────────────┐
│   测试      │
│   Test      │
└──────┬──────┘
       │
       ↓
┌─────────────┐
│   部署      │
│   Deploy    │
└──────┬──────┘
       │
       ↓
┌─────────────┐
│   维护/更新 │
│ Maintain    │
└─────────────┘
```

## 🛠️ 常用工具和命令

### 查找 Skill

```bash
# 列出所有 Skill
ls -1d .github/skills/*/

# 按名字搜索
ls .github/skills/ | grep -i "generate"

# 查看 Skill 元数据
grep "^name:" .github/skills/*/SKILL.md
```

### 创建新 Skill

```bash
# 1. 创建目录
mkdir -p .github/skills/my-new-skill

# 2. 复制模板
cp .github/skills/test-data-generator-skill/SKILL.md \
   .github/skills/my-new-skill/SKILL.md

# 3. 编辑
code .github/skills/my-new-skill/SKILL.md

# 4. 验证
grep "^name:" .github/skills/my-new-skill/SKILL.md
```

### 验证 Skill 质量

```bash
# 检查必需部分
grep -E "^## (Purpose|Inputs|Workflow|Outputs|Done Checks)" \
     .github/skills/[skill-name]/SKILL.md

# 检查 YAML 格式
head -5 .github/skills/[skill-name]/SKILL.md
```

## ✨ 特点和优势

### 📖 完整的文档体系
- ✅ 快速参考指南
- ✅ 详细模板说明
- ✅ 深入分析文档
- ✅ 3 个实际示例

### 🎯 多个示例
- ✅ 简单 Skill（10-20 行）
- ✅ 中等 Skill（30-50 行）
- ✅ 复杂 Skill（50-100+ 行）

### 📊 详细对比
- ✅ 表格对比
- ✅ 结构分析
- ✅ 编写模式
- ✅ 最佳实践

### ✅ 质量保证
- ✅ 检查清单
- ✅ Done Checks 指导
- ✅ 常见错误说明

## 🚀 下一步行动

### 如果你是新手
```
1. 打开 QUICK_REFERENCE.md
2. 查看一个测试示例
3. 按照模板创建你的第一个 Skill
```

### 如果你想深入学习
```
1. 阅读 SKILL_TEMPLATE_GUIDE.md
2. 阅读 SKILL_EXAMPLES_ANALYSIS.md
3. 对比三个测试示例
4. 创建一个高级 Skill
```

### 如果你要修改现有 Skill
```
1. 了解 Skill 之间的依赖关系
2. 参考修改指南（在项目需求文档中）
3. 更新相关文档
4. 更新清单
```

## 📞 常见问题

**Q: 我应该从哪里开始？**
A: 打开 `QUICK_REFERENCE.md`，然后根据你的情况选择学习路径。

**Q: 我需要了解所有内容吗？**
A: 不需要。`QUICK_REFERENCE.md` 就足以创建一个基本 Skill。如果需要更高级的功能，再读详细文档。

**Q: 已有的三个示例有什么区别？**
A: 看 `SKILL_EXAMPLES_ANALYSIS.md` 的第一部分，有详细的对比表。

**Q: 我的 Skill 应该有多长？**
A: 通常 30-50 行（中等复杂度）。参考 `QUICK_REFERENCE.md` 中的"关键数字一览"。

**Q: Done Checks 和 Workflow 的区别是什么？**
A: Workflow 是"怎么做"，Done Checks 是"怎么验证做好了"。

## 🔗 相关文件

- 项目需求文档: `docs/项目初始化AI Skill需求文档.md`
- 执行编排: `.github/skills/auto-delivery-loop/SKILL.md`
- Skill 工厂: `.github/skills/skill-factory/SKILL.md`

## 📝 版本历史

| 版本 | 日期 | 变更 |
|------|------|------|
| 1.0 | 2026-05-21 | 初始版本，包含 4 个文档 + 3 个测试示例 |

## 👥 贡献指南

在创建新 Skill 时：
1. 参考这些文档
2. 遵循标准模板
3. 在适当的地方添加集成示例
4. 更新项目需求文档中的 Skill 清单

## 📄 许可证

本文档是 AutoCodeForge 项目的一部分。

---

**💡 提示**: 不确定从哪开始？打开 `QUICK_REFERENCE.md` ⭐
