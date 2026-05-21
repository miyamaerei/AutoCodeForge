# AutoCodeForge 治理规则与规范宪法

**版本**: v1.0.0  
**生成时间**: 2026-05-21  
**生效日期**: 2026-05-22  
**更新触发**: 仅通过 SPEC_CHANGE_REQUEST 流程  

---

## 一、核心治理原则

### 1.1 规范宪法的地位
本文档是 AutoCodeForge AI 研发流水线的**唯一规范基准**，所有开发、审核、变更动作必须遵循本规范。规范禁止随意修改，仅可通过 SPEC_CHANGE_REQUEST 审批后由 AI 自动更新。

### 1.2 四大治理维度
1. **组件复用规范**: 强制复用预设基础类和服务，禁止重复轮子
2. **编码规范**: 统一命名、风格、注释、异常处理标准
3. **合规校验**: @Auditor 进行代码审查与规范检查
4. **流程约束**: Git 分支、提交、PR 流程严格把控

---

## 二、组件复用规范（核心强制要求）

### 2.1 后端强制复用清单

#### 2.1.1 基础实体类
| 组件 | 位置 | 强制复用范围 | 说明 |
|------|------|------------|------|
| **AuditableEntity** | Core/Entities/ | 所有需要追踪 CreatedAt/UpdatedAt/IsDeleted 的实体 | 基础审计字段自动管理 |
| **UserOwnedEntity** | Core/Entities/ | 所有用户关联的数据（Task, Pipeline, Repository） | 多租户隔离 + 审计 |
| **BaseRepository<T>** | Core/Repositories/ | 所有仓储实现 | 通用 CRUD + QueryFilter |
| **IRepository<T>** | Core/Repositories/ | 所有 Service 依赖的仓储接口 | 接口抽象，便于 Mock |

**使用规范**:
```csharp
// ✅ 正确做法
public class Task : UserOwnedEntity { } // 继承基类自动获得 OwnerId、审计字段

public class TaskService
{
    private readonly IRepository<Task> _taskRepository;
    
    public async Task<Task> GetTaskAsync(string taskId)
    {
        return await _taskRepository.GetAsync(t => t.Id == taskId);
        // 自动应用 IsDeleted=false 与 OwnerId=currentUser.Id 过滤
    }
}

// ❌ 禁止
public class CustomTask : DbContext { } // 违反继承规范
```

#### 2.1.2 服务与工具类
| 组件 | 位置 | 强制复用范围 | 说明 |
|------|------|------------|------|
| **ILlmGateway** | Infrastructure/AI/ | 所有 LLM 调用 | 统一网关，支持多模型 |
| **IGitProviderFactory** | Infrastructure/Git/ | 所有 Git 操作 | 工厂模式隔离不同平台 |
| **TaskQueue** | Infrastructure/Queue/ | 所有异步任务入队 | 内存队列 + 持久化基础 |
| **Logger** | Infrastructure/Logging/ | 所有日志输出 | 统一格式与采集 |
| **ApiResponse<T>** | Api/Models/ | 所有 HTTP 响应 | 统一格式，便于前端处理 |

**使用规范**:
```csharp
// ✅ 正确做法
public class ChatService
{
    private readonly ILlmGateway _llmGateway;
    
    public async Task<string> GenerateResponseAsync(string userMessage)
    {
        return await _llmGateway.CallAsync(userMessage);
    }
}

// ❌ 禁止 - 直接调用 OpenAI API
var response = await new OpenAIClient(apiKey).CreateChatCompletionAsync(...);
```

#### 2.1.3 中间件与过滤器
| 组件 | 位置 | 强制复用范围 | 说明 |
|------|------|------------|------|
| **JwtAuthMiddleware** | Api/Middleware/ | 所有需要认证的端点 | 自动 Bearer Token 验证 |
| **GlobalExceptionMiddleware** | Api/Middleware/ | 全局异常处理 | 统一错误响应格式 |
| **ValidateModelStateAttribute** | Api/Filters/ | 所有参数验证 | 自动返回 400 Bad Request |

### 2.2 前端强制复用清单

#### 2.2.1 基础组件与工具

| 组件 | 位置 | 强制复用范围 | 说明 |
|------|------|------------|------|
| **API 层** | src/modules/*/[module-name].api.ts | 所有 HTTP 调用 | 禁止在组件中直接使用 axios |
| **Store 层** | src/modules/*/store/ | 所有状态管理 | 禁止在组件中定义本地状态（除组件 UI 状态） |
| **Mapper 函数** | src/modules/*/ | DTO ↔ Model 转换 | 数据必须经过转换再入 store |
| **Composables** | src/composables/ | 通用逻辑复用 | 页面表单逻辑、API 绑定 |

#### 2.2.2 Element Plus 组件复用

| 使用场景 | 组件 | 规范 |
|---------|------|------|
| **表单** | el-form + el-form-item | 禁止手写 input 标签 |
| **表格** | el-table + el-table-column | 禁止手写 <table> |
| **对话框** | el-dialog | 禁止手写 modal/popover |
| **消息提示** | ElMessage / ElNotification | 所有用户反馈统一使用 |
| **分页** | el-pagination | 所有列表必须分页 |

**使用规范**:
```vue
<!-- ✅ 正确做法 -->
<el-form :model="formData" @submit="handleSubmit">
  <el-form-item label="任务名" prop="title">
    <el-input v-model="formData.title" />
  </el-form-item>
</el-form>

<!-- ❌ 禁止 - 裸 HTML -->
<form @submit="handleSubmit">
  <label>任务名: <input v-model="formData.title" /></label>
</form>
```

### 2.3 违规处理机制

**@Auditor 检查职责**:
1. 每轮代码审查 ≥20% 代码，重点检查组件复用合规性
2. 发现违规立即**一票否决**，标注违规编号并打回整改
3. 同一类违规累计 3 次，触发 SPEC_CHANGE_REQUEST 评估规范强化

**违规等级**:
- **高**: 直接调用 axios / 绕过 Repository 接口 / 不使用 ApiResponse (一票否决)
- **中**: 缺少组件复用、代码重复 >30% (打回整改)
- **低**: 命名不规范、注释缺失 (建议改进)

**处理流程**:
```
代码提交 → @Auditor 审查 → 违规? 
  ├─ YES → 标注违规编号 → 打回整改 → 再审
  └─ NO → 合规通过 → 入库
```

---

## 三、编码规范（统一执行标准）

### 3.1 后端编码规范

#### 3.1.1 命名规范
```
包/命名空间: PascalCase (AutoCodeForge.Application.Services)
类名: PascalCase (UserService, TaskRepository)
方法名: PascalCase (GetUserAsync, CreateTaskAsync)
属性名: PascalCase (Id, CreatedAt, UserId)
私有字段: _camelCase (_repository, _logger)
常量: UPPER_SNAKE_CASE (MAX_RETRY_COUNT, DEFAULT_TIMEOUT_MS)
```

**示例**:
```csharp
namespace AutoCodeForge.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly ILogger<UserService> _logger;
        
        private const int MAX_RETRY_COUNT = 3;
        
        public async Task<UserResponseDto> GetUserAsync(string userId)
        {
            // 实现
        }
    }
}
```

#### 3.1.2 注释要求 (XML Documentation)
所有 public 成员必须有 XML 注释，包含：
- 功能说明
- 参数描述
- 返回值描述
- 异常说明
- 使用示例（必要时）

**示例**:
```csharp
/// <summary>
/// 创建新任务并异步执行
/// </summary>
/// <param name="dto">任务创建 DTO，包含 Title、Description</param>
/// <returns>创建后的任务实体</returns>
/// <exception cref="ValidationException">DTO 验证失败时抛出</exception>
/// <remarks>
/// 该方法将任务入队并由 BackgroundService 异步执行
/// </remarks>
public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto)
{
    // 实现
}
```

#### 3.1.3 异常处理规范
所有可能抛出异常的操作必须明确处理，禁止裸抛：

**示例**:
```csharp
// ✅ 正确做法
try
{
    var user = await _userRepository.GetAsync(u => u.Id == userId);
    if (user == null)
        throw new NotFoundException($"User {userId} not found");
    return user;
}
catch (NotFoundException)
{
    throw;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to get user {UserId}", userId);
    throw new ApplicationException("Failed to retrieve user", ex);
}

// ❌ 禁止 - 裸抛异常
var user = await _userRepository.GetAsync(u => u.Id == userId);
return user ?? throw new Exception("User not found");
```

#### 3.1.4 异步操作规范
- 所有 I/O 操作（数据库、网络）必须使用 async/await
- 方法名必须以 Async 后缀结尾
- 禁止使用 .Result 或 .Wait()（死锁风险）

**示例**:
```csharp
// ✅ 正确
public async Task<User> GetUserAsync(string userId)
{
    return await _userRepository.GetAsync(u => u.Id == userId);
}

// ❌ 禁止 - 死锁风险
public User GetUser(string userId)
{
    return _userRepository.GetAsync(u => u.Id == userId).Result;
}
```

### 3.2 前端编码规范

#### 3.2.1 命名规范
```
文件夹: kebab-case (src/modules/task-center)
Vue 组件文件: PascalCase (TaskListView.vue)
TypeScript 文件: 类型文件 XxxDto.ts / XxxModel.ts
存储: useXxxStore.ts (Pinia convention)
API 文件: xxxApi.ts
类型定义: xxxTypes.ts 或 xxxDto.ts
函数/变量: camelCase (getUserName, taskListRef)
常量: UPPER_SNAKE_CASE (MAX_PAGE_SIZE, DEFAULT_SORT_ORDER)
```

**示例**:
```typescript
// ✅ 正确
// src/modules/task-center/task-center.api.ts
export const taskApi = {
  async getTaskList(params: GetTaskListDto): Promise<TaskModel[]> {
    // 实现
  }
}

// src/modules/task-center/store/useTaskCenterStore.ts
export const useTaskCenterStore = defineStore('task-center', () => {
  const tasks = ref<TaskModel[]>([])
  // 实现
})

// ❌ 禁止
// taskApi.js (错误后缀)
// src/modules/task_center (下划线不规范)
```

#### 3.2.2 TypeScript 类型规范
所有变量、参数、返回值必须有类型注释，禁止 `any`：

**示例**:
```typescript
// ✅ 正确
const user: UserModel = {
  id: '123',
  name: 'John'
}

function handleTaskSelect(task: TaskModel): void {
  // 实现
}

// ❌ 禁止
const user: any = { ... }
function handleTaskSelect(task): void { }
```

#### 3.2.3 Vue 组件规范
每个组件必须包含：
- `<template>`: UI 结构
- `<script setup lang="ts">`: 逻辑（使用 setup 语法糖）
- `<style scoped>`: 组件样式（scoped 防止样式污染）

**示例**:
```vue
<template>
  <div class="task-list">
    <el-table :data="tasks" @row-click="handleRowClick">
      <el-table-column prop="title" label="任务名" />
    </el-table>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { taskApi } from '../task-center.api'
import type { TaskModel } from '../task-center.types'

const tasks = ref<TaskModel[]>([])
const loading = ref(false)

onMounted(async () => {
  loading.value = true
  try {
    tasks.value = await taskApi.getTaskList({ page: 1, size: 20 })
  } finally {
    loading.value = false
  }
})

const handleRowClick = (row: TaskModel) => {
  // 处理行选择
}
</script>

<style scoped>
.task-list {
  min-width: 1280px;
  padding: 20px;
}
</style>
```

#### 3.2.4 Store 状态规范
Pinia setup store 必须包含：
- 状态 (`ref/reactive`): 组件所需数据
- 计算属性 (`computed`): 派生数据
- 方法 (`action`): 业务逻辑与 API 调用

**示例**:
```typescript
export const useTaskCenterStore = defineStore('task-center', () => {
  // 状态
  const tasks = ref<TaskModel[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  
  // 计算属性
  const hasError = computed(() => error.value !== null)
  const taskCount = computed(() => tasks.value.length)
  
  // 异步方法（必须有 try/catch/finally）
  async function fetchTasks() {
    loading.value = true
    error.value = null
    try {
      tasks.value = await taskApi.getTaskList()
    } catch (err) {
      error.value = (err as Error).message
    } finally {
      loading.value = false
    }
  }
  
  return {
    tasks,
    loading,
    error,
    hasError,
    taskCount,
    fetchTasks
  }
})
```

---

## 四、Git 工作流与提交规范

### 4.1 分支策略 (GitFlow 变种)
```
main (生产分支)
  ├─ hotfix/* (线上紧急修复)
  │   └─ hotfix/critical-bug-fix
  │
staging (预发布分支)
  └─ develop (开发主分支)
      ├─ feature/* (功能分支)
      │   ├─ feature/task-center-v1
      │   ├─ feature/wiki-module
      │   └─ feature/git-integration-v2
      │
      └─ bugfix/* (bug 修复分支)
          ├─ bugfix/auth-token-expired
          └─ bugfix/task-execution-race-condition
```

### 4.2 分支命名规范
```
功能分支:     feature/<module>-<feature-name>
    例如:     feature/task-center-v1, feature/console-chat-sse
    
缺陷分支:     bugfix/<bug-id>-<description>
    例如:     bugfix/td-001-http-auth-header, bugfix/jwt-expiry-handle
    
热修复:       hotfix/<issue>-<description>
    例如:     hotfix/critical-db-connection-leak

发布分支:     release/v<version>
    例如:     release/v1.2.0
```

### 4.3 提交消息规范 (Conventional Commits)
```
<type>(<scope>): <subject>

<body>

<footer>
```

**类型**:
- `feat`: 新功能
- `fix`: 缺陷修复
- `refactor`: 代码重构（非功能变更）
- `chore`: 依赖更新、配置调整
- `test`: 测试相关
- `docs`: 文档更新

**示例**:
```
✅ 正确:
feat(task-center): add task execution status polling
- Implement 30-second polling interval
- Add loading state to task detail view
- Closes issue #42

fix(auth): prevent stale jwt token in request header
- Clear authorization header on logout
- Add token expiry check in interceptor
- Fixes #38

chore(deps): upgrade vue to 3.5.32
- Update pinia to 3.0.4
- No breaking changes

❌ 错误:
fixed bug
Updated files
任务中心做了修改
```

### 4.4 提交规范
- **原子提交**: 每个提交对应一个逻辑变更（不要把多个功能混在一起）
- **频率**: 每个完整功能点至少一个提交，避免超过 500 行代码差异
- **消息长度**: subject 不超过 50 字符，body 不超过 72 字符

---

## 五、Pull Request 规范

### 5.1 PR 命名规范
```
[<类型>] <描述> - <发起人轮次>
[feature] Task center module MVP - Round 15
[fix] Auth token refresh race condition - Round 14
[refactor] Simplify repository query builder - Round 16
```

### 5.2 PR 检查清单 (自检)
发起 PR 前，必须勾选以下项目：

- [ ] 代码已本地编译通过 (`npm run build` / `dotnet build`)
- [ ] 所有相关单元测试通过 (`npm run test` / `dotnet test`)
- [ ] 代码符合命名与风格规范
- [ ] 组件复用检查 (是否使用了已有基类/服务)
- [ ] 异常处理完整 (try/catch/finally)
- [ ] 新增 API 在 Swagger 中文档完整
- [ ] 前后端类型对齐 (DTO/Model 一致)
- [ ] 已更新相关文档 (README, API docs)
- [ ] 无硬编码凭据或密钥
- [ ] 提交消息符合 Conventional Commits

### 5.3 PR 审查职责 (@Auditor)

**强制审查项**:
1. ✅ 组件复用合规性 (是否违反 2.1/2.2 清单)
2. ✅ 编码规范符合性 (命名、注释、异常处理)
3. ✅ 测试覆盖率 (新增代码是否有单元测试)
4. ✅ 安全检查 (是否有输入验证、权限检查)
5. ✅ 性能影响 (是否有 N+1 查询、大数据操作)

**审查结论**:
- 🟢 **APPROVED**: 代码完全符合规范，可入库
- 🟡 **CHANGES_REQUESTED**: 存在 1+ 不符合项，需整改后重新审查
- 🔴 **COMMENTED**: 存在争议或建议改进项，可选择修改

### 5.4 合并流程
```
PR 发起 → @Auditor 审查 → APPROVED? 
  ├─ YES → Squash & Merge → main/staging
  └─ NO → 整改 → 重新审查
```

---

## 六、版本控制与发版规范

### 6.1 版本号管理 (Semantic Versioning)
```
v<MAJOR>.<MINOR>.<PATCH>
v1.2.3
├─ 1: MAJOR (breaking changes: API 重构、数据迁移)
├─ 2: MINOR (new features: 新模块、新端点)
└─ 3: PATCH (bug fixes: 缺陷修复、性能优化)
```

### 6.2 发版前检查清单
- [ ] 所有单元测试通过 (100% 相关测试)
- [ ] 集成测试验证 (关键流程端到端)
- [ ] 代码安全扫描 (SonarQube / Snyk)
- [ ] 性能基准测试 (关键接口响应时间)
- [ ] 文档更新 (API 变更、迁移指南)
- [ ] 数据库迁移脚本 (如有结构变更)
- [ ] 灰度部署计划 (验证生产环境)

---

## 七、规范更新记录与变更流程

### 7.1 规范版本控制
| 版本 | 发布日期 | 更新内容 | 触发方式 |
|------|----------|--------|--------|
| v1.0.0 | 2026-05-21 | 初始治理规范（基础四大维度） | AI 治理初始化 |
| | | | |

### 7.2 SPEC_CHANGE_REQUEST 流程
任何对本规范的变更必须通过正式的 SPEC_CHANGE_REQUEST 流程：

**触发条件**:
- 发现规范过时或不可达成
- 多个开发者反馈规范冲突
- 技术栈升级导致规范失效
- 同一类违规累计 3+ 次

**流程步骤**:
1. **发起**: 提交 SPEC_CHANGE_REQUEST 文件，说明变更理由、影响范围、建议方案
2. **评审**: Strategic Planner 与 @Auditor 联合评审（24 小时内）
3. **决议**: 批准/驳回/修改后批准
4. **更新**: AI 根据审批结果自动更新规范，并更新版本号与变更记录

---

## 八、规范执行监管机制

### 8.1 日常监管
- **代码审查**: 每个 PR 由 @Auditor 进行规范检查
- **周期性抽查**: 每周抽查 10% 已提交代码
- **自动化检查**: 集成 ESLint (前端) + SonarQube (后端) 到 CI/CD

### 8.2 违规上报
| 违规次数 | 处理措施 | 触发机制 |
|----------|--------|--------|
| 第 1 次 | 一票否决 + 打回整改 + 记录 | @Auditor 审查时发现 |
| 第 2 次 | 同上 + 发出警告 | 同类违规再次出现 |
| 第 3 次 | 同上 + 触发 SPEC_CHANGE_REQUEST | 同类违规累计达到 3 |
| 第 4+ 次 | 规范升级为 CI/CD 强制检查 | 自动化工具集成 |

### 8.3 规范执行 SLA
- **PR 审查**: 24 小时内完成
- **违规整改**: 发现后 24 小时内完成
- **SPEC_CHANGE_REQUEST**: 72 小时内给出决议

---

## 九、规范相关文档与工具

### 9.1 快速参考卡
- 命名速查表 (快速参考规范中的命名部分)
- 组件复用清单 (后端/前端强制复用组件列表)
- Git 提交模板 (.gitmessage)

### 9.2 工具配置
- ESLint 配置文件 (.eslintrc.js)
- Prettier 配置文件 (.prettierrc)
- SonarQube 项目配置 (sonar-project.properties)
- Git Hooks (pre-commit, commit-msg 校验)

### 9.3 培训与宣传
- 规范宣讲 (新成员入职 checklist)
- 常见违规案例分析
- 规范 Q&A 文档

---

## 十、附录：规范符号速查

| 符号 | 含义 | 示例 |
|------|------|------|
| ✅ | 正确做法，必须遵循 | ✅ 使用 BaseRepository |
| ❌ | 错误做法，禁止使用 | ❌ 直接调用 axios |
| ⚠️ | 警告，需要特别注意 | ⚠️ 异常处理可能不完整 |
| 🟢 | 良好实践，推荐采用 | 🟢 使用 Composables 共享逻辑 |
| 🔴 | 严格禁止，一票否决 | 🔴 裸异常抛出 |
| P0 | 最高优先级，立即处理 | P0 组件复用违规 |
| P1 | 高优先级，本轮必须完成 | P1 缺失单元测试 |
| P2 | 中优先级，两轮内完成 | P2 代码重复 >30% |
| P3 | 低优先级，三轮内优化 | P3 注释不完整 |

---

## 总结

本规范宪法通过**强制复用**、**统一编码**、**流程约束**、**监管机制**四大维度，确保 AutoCodeForge 的长期质量与可维护性。所有开发者必须严格遵循，任何变更只能通过 SPEC_CHANGE_REQUEST 流程进行。

