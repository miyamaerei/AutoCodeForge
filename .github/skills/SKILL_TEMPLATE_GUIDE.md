# Skill 模板完整指南

## 一、Skill 模板标准结构

AutoCodeForge 项目中的每个 Skill 都遵循统一的 YAML + Markdown 混合格式。

### 1.1 文件组织

```
.github/skills/[skill-name]/
├── SKILL.md                  # 必需：Skill定义文件
├── README.md                 # 可选：详细文档
└── examples/                 # 可选：使用示例
```

### 1.2 SKILL.md 文件结构

每个 SKILL.md 包含两部分：

#### Part 1: YAML Frontmatter（元数据）

```yaml
---
name: SkillNameInPascalCase
description: "One-line concise description of what this skill does."
argument-hint: "Hints for user on what inputs to provide."
---
```

**字段说明：**
- `name`: Skill 唯一标识，用 PascalCase（例：ConfigInitSkill）
- `description`: 一行简短描述，30-100 字符
- `argument-hint`: 使用提示，告诉用户需要提供哪些参数（可选）

#### Part 2: Markdown 内容

```markdown
# SkillName

## Purpose
- 主要目标 1
- 主要目标 2

## Inputs
1. 输入参数1 说明
2. 输入参数2 说明
3. 输入参数3 说明

## Workflow
1. 第一步工作流程
2. 第二步工作流程
3. 第三步工作流程
...

## Outputs
- 输出产物1
- 输出产物2

## Done Checks
- 完成检查项1
- 完成检查项2
- 完成检查项3
```

## 二、各部分详细说明

### 2.1 Purpose 部分
- **用途**: 明确此 Skill 要解决什么问题
- **格式**: 2-5 个无序列表项，每项一个核心目标
- **示例**:
  ```markdown
  ## Purpose
  - Establish baseline configuration for all runtime environments.
  - Validate mandatory configuration keys before startup.
  - Provide fallback values for non-critical settings.
  ```

### 2.2 Inputs 部分
- **用途**: 列出此 Skill 需要的输入参数
- **格式**: 有序列表，包含参数名和简短说明
- **示例**:
  ```markdown
  ## Inputs
  1. Environment profiles (dev, staging, prod).
  2. Database connection target.
  3. Logging level configuration.
  4. Optional: Custom CORS origins list.
  ```

### 2.3 Workflow 部分
- **用途**: 描述 Skill 执行的步骤顺序
- **格式**: 有序列表，每步一个清晰的动作
- **示例**:
  ```markdown
  ## Workflow
  1. Validate input parameters and check environment availability.
  2. Load configuration templates from baseline.
  3. Materialize config files for each environment.
  4. Verify all mandatory keys are present.
  5. Generate fallback values for missing optional keys.
  6. Emit validation report.
  ```

### 2.4 Outputs 部分
- **用途**: 说明此 Skill 生成的产物
- **格式**: 无序列表，产物类型 + 简短说明
- **示例**:
  ```markdown
  ## Outputs
  - Baseline configuration files (config.json per environment).
  - Configuration validation report (missing keys, type mismatches).
  - Fallback values spreadsheet (for manual review).
  ```

### 2.5 Done Checks 部分
- **用途**: 定义完成此 Skill 的验收标准
- **格式**: 无序列表，可验证的检查项
- **示例**:
  ```markdown
  ## Done Checks
  - All config files present for required environments.
  - No hardcoded secrets in configuration.
  - All mandatory keys validated and non-empty.
  - Validation report generated successfully.
  - Fallback values applied without errors.
  ```

## 三、Skill 命名规范

| 类别 | 模式 | 示例 |
|------|------|------|
| 初始化类 | `[Domain]InitSkill` | `ProjectInitSkill`, `ConfigInitSkill` |
| 生成类 | `[Type]GenerateSkill` | `CodeGenerateSkill`, `TemplateGenerateSkill` |
| 分析类 | `[Target]AnalyzeSkill` | `CodeOpinionAnalyzeSkill`, `DependencyAnalyzeSkill` |
| 验证类 | `[Target]ValidateSkill` | `ConfigValidateSkill`, `ContractValidateSkill` |
| 部署类 | `[Target]DeploySkill` | `DockerDeploySkill`, `K8sDeploySkill` |
| 工具类 | `[Function]UtilSkill` | `FileUtilSkill`, `DataTransformUtilSkill` |

## 四、完整示例对比

### 4.1 简单 Skill（约 15 行）

```yaml
---
name: HealthCheckSkill
description: "Perform health checks on configured services and endpoints."
argument-hint: "Provide service endpoints list and timeout in milliseconds."
---

# HealthCheckSkill

## Purpose
- Verify service availability before workflow execution.

## Inputs
1. Service endpoints list (URLs, gRPC addresses).
2. Health check timeout (milliseconds).

## Workflow
1. Iterate through endpoints.
2. Send health check request (HTTP GET, gRPC healthz).
3. Collect response status.
4. Generate summary report.

## Outputs
- Health check report (JSON format).
- Service availability matrix.

## Done Checks
- All services respond within timeout.
- Report is valid JSON.
- No network errors occurred.
```

### 4.2 复杂 Skill（约 60+ 行）

参考本仓库已创建的 [test-data-generator-skill/SKILL.md](./test-data-generator-skill/SKILL.md)

## 五、最佳实践

### 5.1 编写检查清单

- [ ] 使用清晰的 PascalCase 命名
- [ ] Description 一行完成，简洁准确
- [ ] Inputs 列出所有必需和可选参数
- [ ] Workflow 按执行顺序排列（1, 2, 3...）
- [ ] Outputs 包含具体的产物类型
- [ ] Done Checks 包含 3-5 个可验证的标准
- [ ] 没有拼写或语法错误
- [ ] 与其他 Skill 没有名字冲突
- [ ] 相关文档已完成（如有 README）

### 5.2 Skill 之间的依赖关系

定义 Skill 依赖时：
```markdown
## Prerequisites
- InitProjectSkill must complete first.
- ConfigInitSkill outputs are required.

## Downstream Consumers
- BaseDevelopSkill depends on outputs.
- TemplateGenerateSkill uses config baseline.
```

### 5.3 常见问题

**Q: Skill 应该多细粒度？**
A: 一个 Skill 应该做一件事，做好它。如果需要 3+ 分钟解释目的，考虑拆分。

**Q: Done Checks 需要自动化吗？**
A: 不一定。Done Checks 是验收标准。自动化检查是可选的扩展。

**Q: 如何处理 Skill 失败？**
A: 在 FlowDispatchSkill 中定义重试/回滚策略。

## 六、技能工厂集成

当新 Skill 创建完成后，需要在以下地方注册：

1. `.github/skills/[skill-name]/SKILL.md` ✓（已完成）
2. 项目文档：补充到需求文档的 Skill 清单
3. FlowDispatchSkill：添加到调用顺序

## 七、参考资源

- 已有 Skill 示例: `.github/skills/*/SKILL.md`
- 项目需求文档: `docs/项目初始化AI Skill需求文档.md`
- 技能工厂: `.github/skills/skill-factory/SKILL.md`

---

**最后更新**: 2026-05-21  
**维护者**: AutoCodeForge 团队
