# 使用示例

## 示例 1：新增功能审计

**场景**：开发者新增用户注册功能，需要进行安全审计

**执行步骤**：

```markdown
/code-security-audit 新增用户注册功能
```

**Agent 会自动**：
1. 分析需求类型 → feature（新增功能）
2. 提取关键词 → "用户"、"注册"
3. 搜索相关文件 → UserService.cs、AccountController.cs、RegistrationDto.cs
4. 执行安全审计（重点：密码加密、输入验证、权限控制）
5. 生成审计报告

**预期输出**：
```markdown
## 代码安全审计报告

**需求类型**: feature
**需求描述**: 新增用户注册功能
**文件**: UserService.cs
**审计时间**: 2026-05-23 21:45
**识别风险数**: 1

### 风险清单
| # | 风险类型 | 严重级别 | 行号 | 描述 |
|---|---------|----------|------|------|
| 1 | 密码明文存储 | CRITICAL | 52 | 用户密码未加密直接存储 |

### 风险详情
#### 风险 1: 密码明文存储（CRITICAL）
**位置**: 第 52 行
**代码片段**:
```csharp
user.Password = request.Password;
```
**风险描述**: 用户密码未进行哈希加密，存在数据泄露风险。
**修复建议**: 使用 BCrypt 或 Argon2 对密码进行哈希处理。

**文件整体状态**: FAIL
```

---

## 示例 2：Bug 修复验证

**场景**：修复了订单查询的 SQL 注入漏洞，需要验证修复是否正确

**执行步骤**：

```markdown
/code-security-audit 修复订单查询的 SQL 注入漏洞
```

**Agent 会自动**：
1. 分析需求类型 → bugfix（Bug 修复）
2. 提取关键词 → "订单"、"查询"、"SQL 注入"
3. 搜索相关文件 → OrderService.cs、OrderRepository.cs
4. 执行安全审计（重点：参数化查询、修复完整性）
5. 生成修复验证报告

**预期输出**：
```markdown
## 代码安全审计报告 - Bug 修复

**需求类型**: bugfix
**需求描述**: 修复订单查询的 SQL 注入漏洞
**文件**: OrderRepository.cs
**审计时间**: 2026-05-23 22:00

### 修复验证
| 修复项 | 状态 | 修复前 | 修复后 |
|--------|------|--------|--------|
| SQL 注入修复 | ✅ | `var sql = "SELECT * FROM Orders WHERE Status = '" + status + "'";` | `await _dbContext.Orders.Where(o => o.Status == status).ToListAsync();` |

**文件整体状态**: PASS
```

---

## 示例 3：代码重构审计

**场景**：重构用户服务，抽取公共方法，需要评估重构对安全性的影响

**执行步骤**：

```markdown
/code-security-audit 重构用户服务，抽取公共方法
```

**Agent 会自动**：
1. 分析需求类型 → refactor（代码重构）
2. 提取关键词 → "用户服务"、"公共方法"
3. 搜索相关文件 → UserService.cs、UserController.cs
4. 执行安全审计（重点：安全机制完整性、依赖关系变化）
5. 生成重构影响评估报告

**预期输出**：
```markdown
## 代码安全审计报告 - 代码重构

**需求类型**: refactor
**需求描述**: 重构用户服务，抽取公共方法
**文件**: UserService.cs
**审计时间**: 2026-05-24 10:30

### 重构影响评估
| 风险项 | 状态 | 说明 |
|--------|------|------|
| 安全机制完整性 | ✅ | 权限检查逻辑保持完整 |
| 依赖关系安全性 | ✅ | 依赖注入配置正确 |
| 敏感数据处理 | ⚠️ | 抽取的公共方法包含密码处理逻辑，需关注 |

**文件整体状态**: PASS（需关注）
```

---

## 示例 4：安全专项审计

**场景**：对用户模块进行全面安全审计

**执行步骤**：

```markdown
/code-security-audit 对用户模块进行全面安全审计
```

**Agent 会自动**：
1. 分析需求类型 → security（安全专项）
2. 提取关键词 → "用户模块"
3. 搜索相关文件 → UserService.cs、UserController.cs、UserRepository.cs、IUserService.cs
4. 执行全面安全审计（L1-L4）
5. 生成综合安全评估报告

**预期输出**：
```markdown
## 代码安全审计报告 - 安全专项

**需求类型**: security
**需求描述**: 对用户模块进行全面安全审计
**审计时间**: 2026-05-24 11:00

### 综合安全评估
| 层级 | 风险数量 | 严重级别分布 |
|------|---------|-------------|
| L1 | 0 | CRITICAL: 0 |
| L2 | 1 | HIGH: 1 |
| L3 | 2 | MEDIUM: 2 |
| L4 | 0 | LOW: 0 |

### 风险摘要
- [HIGH] UserController.GetUserById 缺少权限检查 (L2-1)
- [MEDIUM] UserRepository 日志中记录敏感信息 (L3-1)
- [MEDIUM] 缺少 CSRF 防护 (L3-2)

**整体状态**: PASS（存在需要改进的项）
```

---

## 示例 5：指定文件审计

**场景**：直接指定文件进行安全审计

**执行步骤**：

```markdown
/code-security-audit 检查 server/src/Api/OrderService.cs
```

**Agent 会自动**：
1. 识别输入为文件路径
2. 直接读取该文件
3. 执行全面安全审计
4. 生成审计报告

---

## 示例 6：增量审计（修复后）

**场景**：修复了安全问题后，进行增量审计验证

**执行步骤**：

```markdown
/code-security-audit 验证用户注册功能的安全修复
```

**Agent 会自动**：
1. 读取历史证据
2. 对比文件 hash → 发现已变化
3. 重点验证上次发现的风险项是否修复
4. 生成差异报告

**预期输出**：
```markdown
### 与上次审计对比

**上次审计时间**: 2026-05-23 21:45
**文件变更**: ✅ 已修改 (hash: abc123 → def456)

**已修复项**:
- ✅ 密码明文存储已修复 (L1-1)

**新增风险**:
- 无

**趋势**: 风险数 1 → 0 (↓100%)
```

---

## 自动搜索策略示例

### 搜索过程说明

当用户输入 "新增用户登录功能" 时：

1. **关键词提取**：
   - 名词：用户、登录、功能
   - 动作：新增

2. **文件搜索**：
   ```
   搜索 "User" → UserService.cs, UserController.cs, IUserService.cs
   搜索 "Login" → LoginDto.cs, AuthService.cs
   搜索 "Account" → AccountController.cs
   ```

3. **路径推断**：
   ```
   server/src/Api/Controllers/UserController.cs
   server/src/Application/Services/UserService.cs
   server/src/Core/DTOs/LoginDto.cs
   ```

4. **依赖分析**：
   ```
   UserController → IUserService → UserService → IUserRepository
   UserService → IPasswordHasher, IJwtService
   ```

---

## 证据目录结构示例

```
.autoCodeForge/security-audit/
├── UserService/
│   ├── L1-evidence.json
│   ├── L2-evidence.json
│   ├── L3-evidence.json
│   ├── L4-evidence.json
│   └── history/
│       ├── 2026-05-23-2145.md    # 首次审计（FAIL）
│       ├── 2026-05-23-2200.md    # 修复后审计（PASS）
│       └── 2026-05-24-1030.md    # 重构后审计
└── OrderService/
    ├── L1-evidence.json
    ├── L2-evidence.json
    ├── L3-evidence.json
    ├── L4-evidence.json
    └── history/
        └── 2026-05-24-1100.md    # SQL 注入修复验证
```

---

## 常见问题

### Q1: 如何确定需求类型？

**A**: Skill 会自动分析用户输入：
- 包含"新增"、"添加"、"创建" → feature
- 包含"修复"、"bug"、"漏洞" → bugfix
- 包含"重构"、"优化"、"重构代码" → refactor
- 包含"安全"、"审计"、"合规" → security

### Q2: 自动搜索不到相关文件怎么办？

**A**: 可以直接指定文件路径：
```
/code-security-audit 检查 server/src/Api/UserService.cs
```

### Q3: 如何查看历史审计报告？

**A**: 直接查看 `.autoCodeForge/security-audit/[文件名]/history/` 目录下的 Markdown 文件。

### Q4: 测试数据被误报为敏感信息怎么办？

**A**: 在测试数据旁添加注释 `// TEST_DATA`，Skill 会识别并排除。