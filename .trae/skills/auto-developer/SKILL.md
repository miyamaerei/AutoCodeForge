---
name: "auto-developer"
description: "AI自动开发助手。Invoke when user asks to develop a feature, fix a bug, refactor code, or optimize performance."
---

# AI自动开发助手

本Skill用于指导AI执行开发任务，遵循AutoCodeForge项目的完整开发流程和规范要求。

## 一、任务理解阶段

### 1.1 需求确认
当用户下达开发任务时，首先确认：
- 任务类型：DEV（开发）/ BUG（修复）/ REFACTOR（重构）/ OPTIMIZE（优化）/ DOCS（文档）
- 功能/问题描述
- 涉及的模块/文件
- 验收标准
- 优先级（P0/P1/P2/P3）

### 1.2 查阅文档
执行以下检查：
1. 查阅 `docs/reports/MasterPlan_YYYYMMDD.md` - 了解当前项目进度和任务池
2. 查阅 `docs/templates/PROJECT_SPEC.md` - 确认编码规范和审核规则
3. 查阅相关代码 - 理解现有架构和实现方式

## 二、任务规划阶段

### 2.1 分解任务
将大任务拆解为可执行的小步骤，例如：
- 设计API接口
- 创建数据模型
- 实现业务逻辑
- 添加单元测试
- 补充文档注释

### 2.2 预估工时
为每个子任务预估工时（小时），用于MasterPlan_YYYYMMDD.md更新。

### 2.3 确定依赖
识别任务间的依赖关系，确定执行顺序。

## 三、编码执行阶段

### 3.1 遵循规范
严格遵守 `docs/templates/PROJECT_SPEC.md` 的要求：
- 组件复用：优先使用 `pkg/*` 组件，禁止裸写重复代码
- 代码风格：遵循项目现有代码风格
- 注释规范：必须添加函数级/类级注释

### 3.2 执行顺序
1. 先修改/创建数据模型（若需要）
2. 实现API层接口
3. 编写业务逻辑
4. 添加单元测试
5. 补充文档注释

### 3.3 自检查
每完成一个模块，执行自我检查：
- 代码是否可编译/运行
- 是否通过现有测试
- 是否符合规范要求
- 注释是否完整

## 四、规范审核阶段

### 4.1 Auditor校验
作为Auditor角色，检查代码：
- 是否符合 `docs/templates/PROJECT_SPEC.md` 规范
- 组件复用情况
- 代码质量和可维护性
- 测试覆盖度

### 4.2 冲突处理
若发现规范冲突：
1. 暂停后续开发
2. 查阅 `docs/templates/SPEC_CHANGE_REQUEST.md` 模板
3. 生成 `docs/reports/SPEC_CHANGE_REQUEST_XXX.md`
4. 等待人工审批（APPROVE/REJECT）
5. 根据审批结果继续执行

## 五、报告生成阶段

### 5.1 生成ROUND_REPORT
任务完成后，立即生成 `docs/reports/ROUND_REPORT_YYYYMMDD_TYPE_XXX.md`：
- YYYYMMDD：当前日期
- TYPE：任务类型（DEV/BUG/REFACTOR/OPTIMIZE/DOCS/OTHER）
- XXX：当日动态编号
- 填写完整的任务完成明细、代码产出统计、规范合规详情

### 5.2 更新MasterPlan
将任务进度更新到 `docs/reports/MasterPlan_YYYYMMDD.md`：
- 完成度更新
- 已用工时记录
- 状态变更（进行中→已完成）

## 六、交付收尾阶段

### 6.1 最终检查
- 所有代码已提交（git）
- 所有测试通过
- ROUND_REPORT已生成
- MasterPlan已更新

### 6.2 总结汇报
向用户汇报：
- 任务完成情况
- 代码产出统计
- 遇到的问题及解决方案
- 后续建议（若有）

## 关键文件路径

### 模板文件（只读，不可修改）
- `docs/templates/MasterPlan.md` - 主规划文档模板
- `docs/templates/PROJECT_SPEC.md` - 项目规范模板
- `docs/templates/ROUND_REPORT.md` - 单轮执行报告模板
- `docs/templates/DAILY_REPORT.md` - 每日日报模板
- `docs/templates/SPEC_CHANGE_REQUEST.md` - 规范变更申请模板

### 实际文件（动态生成和更新）
- `docs/reports/MasterPlan_YYYYMMDD.md` - 主规划文档（更新进度）
- `docs/reports/PROJECT_SPEC_YYYYMMDD.md` - 项目规范（遵循）
- `docs/reports/ROUND_REPORT_YYYYMMDD_TYPE_XXX.md` - 生成执行报告
- `docs/reports/DAILY_REPORT_YYYYMMDD.md` - 每日日报
- `docs/reports/SPEC_CHANGE_REQUEST_XXX.md` - 生成变更申请（必要时）

## 流程图

```
用户下达任务 → 理解需求 → 查阅文档 → 分解任务
    ↓
编码执行 → 自检查 → Auditor校验 → 冲突？→ 是→生成SPEC_CHANGE_REQUEST
    ↓否
生成ROUND_REPORT → 更新MasterPlan → 最终检查 → 汇报交付
```

## 注意事项

1. **优先复用组件**：P0任务必须强制复用pkg/*组件
2. **规范第一**：遇到规范冲突，暂停开发，走变更流程
3. **及时报告**：任务完成立即生成ROUND_REPORT
4. **保留痕迹**：所有修改都要有git提交记录
5. **测试优先**：新功能必须配套单元测试
