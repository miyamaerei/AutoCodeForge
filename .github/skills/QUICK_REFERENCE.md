# Skill 模板测试文档 - 快速参考

## 概述

本目录包含 AutoCodeForge 项目中 Skill 的完整模板说明和三个实际测试用例。

## 📚 已创建的文档

### 1. **SKILL_TEMPLATE_GUIDE.md** （推荐首先阅读）
- 标准 Skill 模板的完整说明
- YAML Frontmatter 字段详解
- Markdown 各部分的编写规范
- 命名规范参考表
- 最佳实践检查清单

**快速开始**: 按照这个指南创建新 Skill
```yaml
---
name: YourSkillNameHere
description: "Brief one-line description."
argument-hint: "Hints for users."
---

# YourSkillNameHere
## Purpose
...
```

### 2. **SKILL_EXAMPLES_ANALYSIS.md** （深入学习）
- 三个测试 Skill 的详细对比
- 不同复杂度级别的 Skill 分析
- 编写模式总结
- 常见 Skill 类型快速参考

**适合**: 想要理解 Skill 设计原理的开发者

### 3. **具体 Skill 实现** （参考示例）

#### a) test-data-generator-skill/SKILL.md
```yaml
name: TestDataGeneratorSkill
类型: 数据生成类
用途: 生成测试数据、Mock 数据
复杂度: 中等
输入: 5个（Domain, Format, Count, Seed, Relationships）
步骤: 8步
输出: 生成的数据集、Mock 库、文档、清单
```
**特点**: 支持多种输出格式，包含丰富的集成示例

#### b) code-quality-validate-skill/SKILL.md
```yaml
name: CodeQualityValidateSkill
类型: 验证类
用途: 检查代码质量、覆盖率、复杂度
复杂度: 中等
输入: 5个（CodePath, Coverage, Complexity, Linters, Config）
步骤: 7步
输出: 质量报告、违规详情、趋势数据、建议
```
**特点**: 集成多个工具，包含配置示例

#### c) api-contract-validate-skill/SKILL.md
```yaml
name: APIContractValidateSkill
类型: 验证类
用途: 验证 API 合约、OpenAPI 规范
复杂度: 较高
输入: 5个（OpenAPI, BaseURL, Version, Scenarios, Rules）
步骤: 8步
输出: 验证报告、端点测试结果、breaking changes、兼容矩阵
```
**特点**: 包含测试场景和 breaking changes 检测示例

## 🎯 快速使用指南

### 场景 1: 创建一个新 Skill

1. **查看模板**
   ```
   参考: SKILL_TEMPLATE_GUIDE.md 第二部分
   ```

2. **选择复杂度级别**
   ```
   简单 (10-20行): HealthCheckSkill
   中等 (30-50行): TestDataGeneratorSkill ← 推荐起点
   复杂 (50-100+): APIContractValidateSkill
   ```

3. **复制对应示例**
   ```
   中等复杂度? → 参考 test-data-generator-skill/
   验证/检查? → 参考 code-quality-validate-skill/
   API/合约? → 参考 api-contract-validate-skill/
   ```

4. **按模板填充内容**
   ```markdown
   ---
   name: [您的SkillName]
   description: "[一行描述]"
   argument-hint: "[使用提示]"
   ---
   
   # [SkillName]
   
   ## Purpose
   - [目标1]
   - [目标2]
   ...
   ```

### 场景 2: 理解现有 Skill

1. **查看三个测试示例**
   ```
   SKILL_EXAMPLES_ANALYSIS.md → 表格对比
   ```

2. **查看对应类型**
   ```
   生成类: test-data-generator-skill/SKILL.md
   验证类: code-quality-validate-skill/ 或 api-contract-validate-skill/
   初始化: 参考 init-project-skill/ (项目中已有)
   ```

3. **学习编写模式**
   ```
   SKILL_TEMPLATE_GUIDE.md → 第四部分：完整示例对比
   SKILL_EXAMPLES_ANALYSIS.md → 第四部分：编写模式总结
   ```

### 场景 3: 扩展或修改现有 Skill

1. **检查依赖关系**
   ```markdown
   Is this Skill a prerequisite for others?
   Does it depend on other Skills?
   ```

2. **按照修改指南**
   ```
   参考: SKILL_TEMPLATE_GUIDE.md 第五部分
   或: SkillModifyGuideSkill (项目需求中提到)
   ```

3. **更新文档和清单**
   ```
   - 更新 SKILL.md
   - 更新相关的 README
   - 更新项目需求文档中的 Skill 清单
   ```

## 📊 Skill 模板结构速查表

```
SKILL.md 文件标准结构
├── YAML Frontmatter (4 行)
│   ├── name: PascalCase
│   ├── description: One-liner
│   └── argument-hint: User hints
│
└── Markdown Content
    ├── # SkillName (标题)
    ├── ## Purpose (2-5 项)
    ├── ## Inputs (3-6 项)
    ├── ## Workflow (6-10 步)
    ├── ## Outputs (2-4 项)
    ├── ## Done Checks (3-6 项)
    └── [可选] 额外部分
        ├── Configuration Examples
        ├── Integration Points
        └── Common Issues
```

## 🔍 关键数字一览

| 指标 | 推荐值 | 范围 | 备注 |
|------|------|------|------|
| 描述长度 | 50-100 字 | 30-120 | 一行完成 |
| Inputs | 4-5 个 | 2-8 | 5 个最常见 |
| Workflow | 7-8 步 | 3-15 | 平均 7 步 |
| Outputs | 3 个 | 1-5 | 至少 1-2 个 |
| Done Checks | 4-5 个 | 2-8 | 应可验证 |
| 行数 | 40-50 | 15-100+ | 含代码示例 |
| 执行时间 | 变异 | 秒-分 | 根据工具 |

## ✅ 质量检查清单

创建新 Skill 前，确保：

```
[ ] 命名: PascalCase + "Skill" 后缀
[ ] Description: 一行，30-120 字
[ ] Inputs: 3-6 个，有粗体名称
[ ] Workflow: 6-10 步，有序
[ ] Outputs: 2-4 个具体产物
[ ] Done Checks: 3-6 个可验证标准
[ ] 无拼写错误
[ ] 与已有 Skill 无冲突
[ ] 有清晰的集成点说明
[ ] 复杂 Skill 包含示例配置
```

## 📖 文档阅读顺序建议

### 第一次使用（30 分钟）
1. 本文件 (quick reference) - 5 分钟
2. SKILL_TEMPLATE_GUIDE.md - 15 分钟
3. 阅读一个简单 Skill 示例 - 10 分钟

### 深入学习（60 分钟）
1. SKILL_EXAMPLES_ANALYSIS.md - 20 分钟
2. 对比三个测试 Skill - 20 分钟
3. 查看项目中的其他 Skill - 20 分钟

### 专家级（按需参考）
1. FlowDispatchSkill (了解 Skill 编排)
2. DocArchiveManageSkill (了解 Skill 管理)
3. 项目需求文档 (了解整体架构)

## 🚀 快速命令

### 查看所有 Skill

```bash
# 列出所有现有 Skill
ls -1 .github/skills/*/SKILL.md

# 搜索特定类型的 Skill
grep -r "^name:.*GenerateSkill" .github/skills/*/SKILL.md
grep -r "^name:.*ValidateSkill" .github/skills/*/SKILL.md
```

### 创建新 Skill

```bash
# 创建目录
mkdir -p .github/skills/[your-skill-name]

# 复制模板
cp .github/skills/test-data-generator-skill/SKILL.md \
   .github/skills/[your-skill-name]/SKILL.md

# 编辑文件
code .github/skills/[your-skill-name]/SKILL.md
```

## 📝 常见问题

**Q: Skill 应该多细粒度？**
A: 一个 Skill 应该做一件事，并做好它。如果 Purpose 需要 3+ 句话，考虑拆分。

**Q: 如何处理依赖？**
A: 在 Workflow 步骤中清晰地表示依赖关系，或在 Done Checks 中验证前置条件。

**Q: Skill 名字冲突了怎么办？**
A: 使用更具体的名称。例如不要 `GenerateSkill`，用 `TestDataGeneratorSkill`。

**Q: Configuration Examples 是必需的吗？**
A: 对于复杂 Skill（50+ 行）推荐包含。简单 Skill 可选。

## 📌 文件列表

```
.github/skills/
├── SKILL_TEMPLATE_GUIDE.md              ← 标准模板指南
├── SKILL_EXAMPLES_ANALYSIS.md           ← 详细分析对比
├── QUICK_REFERENCE.md                   ← 本文件
├── test-data-generator-skill/
│   └── SKILL.md                         ← 测试 Skill 1: 数据生成
├── code-quality-validate-skill/
│   └── SKILL.md                         ← 测试 Skill 2: 质量验证
├── api-contract-validate-skill/
│   └── SKILL.md                         ← 测试 Skill 3: 合约验证
└── [其他已有 Skill 目录...]
    └── SKILL.md
```

## 🔗 相关资源

- **项目需求文档**: `docs/项目初始化AI Skill需求文档.md`
- **已有 Skill 示例**: `.github/skills/*/SKILL.md`
- **Skill 工厂**: `.github/skills/skill-factory/SKILL.md`
- **执行编排**: `.github/skills/auto-delivery-loop/SKILL.md`

---

**创建时间**: 2026-05-21  
**文档版本**: 1.0  
**维护者**: AutoCodeForge 项目  

💡 **提示**: 打开 SKILL_TEMPLATE_GUIDE.md 开始编写第一个 Skill！
