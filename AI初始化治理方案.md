
# 已有项目 AI 初始化治理 Skill 需求文档

## 一、文档概述

本文档定义一种面向“已有项目”的 AI 初始化治理机制。

通过在项目中引入一组结构化 AI Skill，使 AI 能够：

- 自动理解项目结构与技术栈
- 对现有代码进行分析与评估
- 建立统一的文档体系
- 注入项目规范（命名、流程等）
- 输出完整初始化治理结果

> 本方案不涉及项目创建或脚手架生成，仅针对已有代码仓库。

---

## 二、AI Skill 体系

### 1. ProjectUnderstandSkill

**职责：**
- 扫描代码仓库
- 识别技术栈
- 分析目录与模块
- 判断架构类型

**输出：**
- 技术栈
- 架构说明
- 模块结构

---

### 2. CodeOpinionAnalyzeSkill

**职责：**
- 分析代码质量
- 识别问题（冗余、命名、结构问题）

**输出：**
- 分析报告
- 优化建议

---

### 3. ProjectPlanSkill

**职责：**
- 抽象项目结构
- 输出架构认知

**输出：**
- 架构设计文档
- 模块划分说明

---

### 4. DocStructureDesignSkill

**职责：**
- 设计 docs 结构

**输出结构：**

```

用户可以自定义foldername,后续所有文档都统一
docs/
 ├─ 01_项目总览
 ├─ 02_架构设计
 ├─ 03_模块设计
 ├─ 04_接口文档
 ├─ 05_开发规范
 ├─ 06_流程管理
```

---

### 5. TemplateGenerateSkill

**职责：**
- 生成文档模板（非代码）

**类型：**
- README
- API文档
- 模块设计文档

---

### 6. GovernanceInitSkill

**职责：**
- 注入项目规范

包括：
- 命名规范
- Git分支策略
- PR规范

---

### 7. OutputSummarySkill

**职责：**
- 汇总初始化结果

输出：
- 项目现状
- 文档列表
- 问题与建议

---

### 8. FlowDispatchSkill

**职责：**
- 控制执行流程
- 记录执行状态

---

## 三、执行流程

```
1. ProjectUnderstand
2. CodeOpinionAnalyze
3. ProjectPlan
4. DocStructureDesign
5. TemplateGenerate
6. GovernanceInit
7. OutputSummary
```

---

## 四、文档规范

### 命名规则

- 项目文档：
  [项目]-[类型]-[版本]-[日期].md

- 模块文档：
  [模块]-Design.md

---

## 五、方案定位

本系统为：

✅ AI驱动的项目初始化治理体系

而不是：

❌ 项目脚手架
❌ 代码生成工具

---

## 六、总结

本方案通过 Skill 文档驱动 AI：

- 理解已有项目
- 建立文档体系
- 注入规范
- 输出治理结果

实现项目标准化与可维护性提升。
