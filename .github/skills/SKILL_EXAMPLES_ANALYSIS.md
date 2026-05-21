# Skill 模板对比与分析

本文档展示三个测试 Skill 的对比，帮助理解不同类型 Skill 的设计差异。

## 一、三个示例 Skill 对比表

| 维度 | TestDataGeneratorSkill | CodeQualityValidateSkill | APIContractValidateSkill |
|------|----------------------|-------------------------|--------------------------|
| **类型** | 数据生成类 | 验证类 | 验证类 |
| **主要功能** | 生成测试数据 | 检查代码质量 | 验证 API 合约 |
| **输入数量** | 5个 | 5个 | 5个 |
| **工作流步数** | 8步 | 7步 | 8步 |
| **复杂度** | 中等 | 中等 | 较高 |
| **依赖工具** | faker, 序列化库 | ESLint, Coverage工具 | OpenAPI parser, HTTP client |
| **输出产物数** | 3-5个 | 3-4个 | 4-5个 |
| **典型执行时间** | 30-60秒 | 1-5分钟 | 10-30秒 |
| **适用场景** | 单元/集成测试 | 代码审查/CI | 前后端对接 |

## 二、结构对比

### 2.1 Purpose 部分

```markdown
# TestDataGeneratorSkill
## Purpose
- Generate realistic and comprehensive test data (4个目标)

# CodeQualityValidateSkill  
## Purpose
- Enforce consistent code quality standards
- Verify test coverage meets minimum thresholds
- Detect code complexity violations early
- Validate style and linting compliance automatically (4个目标)

# APIContractValidateSkill
## Purpose
- Ensure API contracts are consistent between frontend and backend
- Verify OpenAPI specifications match actual implementations
- Validate request/response payloads against defined schemas
- Detect breaking changes in API contracts before deployment
- Support contract-first development practices (5个目标)
```

**观察**：Purpose 部分通常 2-5 个目标，每个目标一行。

### 2.2 Inputs 部分

```markdown
# TestDataGeneratorSkill
## Inputs
1. **Data Domain**: Specify which business entities (加粗 + 冒号 + 说明)
2. **Target Format**: Output format preference
3. **Fixture Count**: Number of records per entity type
4. **Seed Value**: Optional random seed
5. **Relationships**: Define entity relationships

# CodeQualityValidateSkill
## Inputs
1. **Code path**: Directory or file patterns (加粗 + 冒号 + 说明)
2. **Coverage threshold**: Minimum line coverage percentage
3. **Complexity limit**: Maximum cyclomatic complexity per function
4. **Enabled linters**: List of linters to run
5. **Config files**: Path to linter configs

# APIContractValidateSkill
## Inputs
1. **OpenAPI spec path**: URL or file path
2. **Backend service URL**: Target service to validate against
3. **Contract version**: Contract version identifier
4. **Test scenarios**: Optional: Custom request/response pairs
5. **Breaking change rules**: Optional: Define acceptable changes
```

**观察**：
- 输入通常 3-5 个
- 使用 **粗体** 作为参数名
- 可选参数用 "Optional:" 前缀标记
- 参数说明包含默认值或示例

### 2.3 Workflow 部分

```markdown
# TestDataGeneratorSkill
## Workflow
1. Validate inputs (验证)
2. Build data schema (构建)
3. Generate fixtures (生成)
4. Apply constraints (应用规则)
5. Format output (格式化)
6. Create mock repositories (创建)
7. Verify data integrity (验证)
8. Export or inject (导出)

# CodeQualityValidateSkill
## Workflow
1. Load configuration (加载)
2. Run linters (执行)
3. Collect coverage metrics (收集)
4. Analyze complexity (分析)
5. Compare against thresholds (比较)
6. Generate detailed report (生成)
7. Export metrics (导出)

# APIContractValidateSkill
## Workflow
1. Load OpenAPI specification (加载)
2. Parse schema definitions (解析)
3. Connect to backend service (连接)
4. Execute test requests (执行)
5. Validate schema compliance (验证)
6. Detect breaking changes (检测)
7. Generate compatibility matrix (生成)
8. Report violations (报告)
```

**模式**：
- 通常 6-10 步
- 每步用动词开头（Validate, Load, Run, Connect, Execute, Generate）
- 步骤按执行顺序排列
- 括号内可添加简短说明

## 三、Skill 复杂度分级

### 简单 Skill（10-20 行）
- **特征**: 单一功能，少量输入，线性工作流
- **示例**: HealthCheckSkill, InitProjectSkill
- **范例**:
  ```markdown
  Inputs: 2-3 个
  Workflow: 3-4 步
  Outputs: 1-2 个
  Done Checks: 2-3 个
  ```

### 中等 Skill（30-50 行）
- **特征**: 多步骤，5 个输入，集成多个工具
- **示例**: TestDataGeneratorSkill, CodeQualityValidateSkill
- **范例**:
  ```markdown
  Inputs: 4-6 个
  Workflow: 6-8 步
  Outputs: 3-4 个
  Done Checks: 4-6 个
  ```

### 复杂 Skill（50-100+ 行）
- **特征**: 多个集成点，条件逻辑，配置示例
- **示例**: APIContractValidateSkill, FlowDispatchSkill
- **范例**:
  ```markdown
  Inputs: 5+ 个
  Workflow: 8+ 步
  Outputs: 4+ 个
  Done Checks: 5+ 个
  包含: 配置示例, 集成点, 常见问题
  ```

## 四、编写模式总结

### 4.1 Inputs 编写模式

**模式 1: 强制参数**
```markdown
1. **Parameter Name**: Description with examples or valid values.
```

**模式 2: 可选参数**
```markdown
5. **Optional Config**: Optional: Description of when to use this.
```

**模式 3: 条件参数**
```markdown
4. **Test scenarios**: Optional: Custom request/response pairs to validate.
   - Required if: Using custom test cases
   - Ignored if: Using auto-generated scenarios
```

### 4.2 Workflow 编写模式

**直线流程** (7-8 步):
```
1. Load X
2. Parse Y
3. Connect to Z
4. Execute operations
5. Validate results
6. Generate report
7. Export data
```

**分支流程** (需要在步骤中注明):
```
3. Execute test requests:
   - If GET: Validate response structure
   - If POST: Validate creation payload
   - If DELETE: Verify resource removed
```

**并行流程** (可通过括号注明):
```
2. Run linters (parallel execution):
   - ESLint for JavaScript
   - Pylint for Python
   - StyleCop for C#
3. Collect coverage metrics (parallel execution)
```

### 4.3 Done Checks 编写模式

**必需检查**:
```markdown
- Core output file generated successfully.
- No unhandled errors occurred.
- Schema validation passed.
```

**质量检查**:
```markdown
- Data passes integrity validation.
- All thresholds met or exceeded.
- No violations remain.
```

**完整性检查**:
```markdown
- All expected files present.
- Documentation is accurate.
- Trend data updated.
```

## 五、Skill 质量检查清单

创建新 Skill 时，检查以下项目：

### 内容完整性
- [ ] YAML Frontmatter 三个字段都有
- [ ] Purpose 有 2-5 个清晰的目标
- [ ] Inputs 有 3-6 个命名参数
- [ ] Workflow 有 6-10 个有序步骤
- [ ] Outputs 有 2-4 个具体产物
- [ ] Done Checks 有 3-6 个可验证标准

### 可读性
- [ ] 长度适中（30-80 行 Markdown）
- [ ] 无拼写错误或语法错误
- [ ] 参数名使用 **粗体**
- [ ] 代码块/示例正确缩进
- [ ] 链接格式正确

### 技术准确性
- [ ] 工作流步骤顺序合理
- [ ] Inputs 与 Workflow 一致
- [ ] Outputs 与 Done Checks 一致
- [ ] 没有不可实现的要求
- [ ] 依赖关系清晰

### 集成就绪
- [ ] 明确定义了与其他 Skill 的关系
- [ ] 输入输出格式明确
- [ ] 错误处理方式说明
- [ ] 集成点已列出

## 六、示例配置对比

### TestDataGeneratorSkill 配置
```yaml
Domain: Simple, single-level
Format: Supports multiple (JSON/CSV/SQL)
Count: Numeric parameter
```

### CodeQualityValidateSkill 配置
```yaml
paths: Array of glob patterns
coverage:
  threshold: Numeric (percentage)
  reportFormat: Enum (html/json)
complexity:
  limit: Numeric
  calculator: String (tool name)
```

### APIContractValidateSkill 配置
```yaml
scenarios:
  - endpoint: String
    request: Object (with nested structure)
    expectedResponse:
      status: Numeric
      body:
        schema: String
breakingChanges:
  - type: Enum
    severity: Enum
    example: String
```

## 七、最佳实践建议

1. **参数命名**: 使用 CamelCase 或 Kebab-case，保持一致
2. **步骤粒度**: 每步应该能在 1-2 句话说清
3. **Done Checks**: 应该能够自动化验证（至少部分）
4. **配置示例**: 提供完整的、可运行的示例
5. **错误处理**: 明确说明失败情况的处理方式

## 八、常见 Skill 类型快速参考

| 类型 | 典型 Inputs 数 | 典型 Workflow 步数 | 主要挑战 |
|------|--|--| -- |
| 初始化类 | 2-3 | 4-6 | 幂等性、依赖管理 |
| 生成类 | 4-5 | 7-9 | 性能、覆盖率 |
| 验证类 | 3-5 | 6-8 | 规则复杂性 |
| 部署类 | 5-6 | 8-10 | 容错性、回滚 |
| 集成类 | 6+ | 10+ | 协调性、一致性 |

---

**更新时间**: 2026-05-21  
**相关文件**: 
- [SKILL_TEMPLATE_GUIDE.md](./SKILL_TEMPLATE_GUIDE.md)
- [test-data-generator-skill/SKILL.md](./test-data-generator-skill/SKILL.md)
- [code-quality-validate-skill/SKILL.md](./code-quality-validate-skill/SKILL.md)
- [api-contract-validate-skill/SKILL.md](./api-contract-validate-skill/SKILL.md)
