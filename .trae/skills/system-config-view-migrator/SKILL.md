---
name: "system-config-view-migrator"
description: "Migrates system-config views from composables/localStorage to useSystemConfigStore. Invoke when user wants to migrate config views or asks to update views to use store."
---

# System Config View Migrator

批量迁移 system-config views 到使用统一的 `useSystemConfigStore`。

## 工作原理

读取 `client/src/modules/system-config/views/` 目录下的所有 `.vue` 文件，分析每个文件的当前实现状态，生成迁移报告并执行迁移。

## 迁移策略

### 简单 View（可直接迁移）

这些 views 只有简单表单，无复杂业务逻辑：
- `SystemConfigView.vue` - 已完成
- `SystemConfigPreferencesView.vue` - 已完成
- `SystemConfigDeepWikiView.vue` - 已完成
- `SystemConfigSchedulesView.vue` - 已完成
- `SystemConfigReviewView.vue` - 已完成

**迁移模式**：
```typescript
// 1. 添加 store import
import { useSystemConfigStore } from '../store/useSystemConfigStore'
import type { ConfigType } from '../api/config.types'

// 2. 在 onMounted 中加载数据
onMounted(async () => {
  loading.value = true
  try {
    await store.loadConfigs('ConfigType' as ConfigType)
    const configs = store.getConfigs('ConfigType' as ConfigType)
    // 从 configs 中恢复表单数据
  } finally {
    loading.value = false
  }
})

// 3. 修改 handleSave 使用 store
const handleSave = async () => {
  await store.saveConfig('ConfigType' as ConfigType, {
    configKey: 'your-config-key',
    configValue: JSON.stringify(form),
    isEncrypted: false,
    description: '...',
  })
}
```

### 复杂 View（需保留 composable 业务逻辑）

这些 views 使用 composables，有复杂的 scenarios、业务逻辑：
- `useSystemConfigSandbox` - Sandbox（scenarios、risk 计算）
- `useSystemConfigNotifications` - Notifications（scenarios、channels）
- `useSystemConfigWorkflow` - Workflow（tabs、agent 流程）
- `useSystemConfigIntegrations` - Integrations（集成目录、认证流程）
- `useSystemConfigKnowledge` - Knowledge（source 管理、sync 状态）
- `useSystemConfigSkills` - Skills（skill 管理、优先级）

**迁移建议**：
1. 保留现有 composable 业务逻辑不变
2. 在 composable 内部添加 `useStore` 模式：
   - composable 初始化时调用 `store.loadConfigs()`
   - 保存时同时调用 `store.saveConfig()` 和原有的 `localStorage` 持久化
3. 这样可以渐进式迁移，不破坏现有业务逻辑

## 执行步骤

### Step 1: 扫描 views 目录

```typescript
// 读取所有 vue 文件
const views = [
  'SystemConfigView.vue',        // ✅ 已完成
  'SystemConfigPreferencesView.vue',  // ✅ 已完成
  'SystemConfigSandboxView.vue',      // 🔄 待处理（复杂）
  'SystemConfigKnowledgeView.vue',     // 🔄 待处理（复杂）
  'SystemConfigSkillView.vue',        // 🔄 待处理（复杂）
  'SystemConfigNotificationsView.vue', // 🔄 待处理（复杂）
  'SystemConfigWorkflowView.vue',     // 🔄 待处理（复杂）
  'SystemConfigIntegrationsView.vue', // 🔄 待处理（复杂）
  'SystemConfigDeepWikiView.vue',     // ✅ 已完成
  'SystemConfigSchedulesView.vue',     // ✅ 已完成
  'SystemConfigReviewView.vue',       // ✅ 已完成
  'SystemConfigRepositoriesView.vue', // 🔄 待检查
  'SystemConfigApiView.vue',          // 🔄 待检查
  'SystemConfigModelsView.vue',       // 🔄 待检查
  'SystemConfigUsersView.vue',        // 🔄 待检查
]
```

### Step 2: 分析每个 view

读取 view 文件，检查：
1. 是否有 `<script setup>`
2. 是否使用了 composable
3. 是否有 `onMounted` 加载逻辑
4. `handleSave` 的实现方式

### Step 3: 分类处理

- **简单 view**：直接迁移到使用 store
- **复杂 view**：保留 composable，但添加 store 持久化

### Step 4: 执行迁移

使用 Edit 工具替换相应的代码段。

## 复杂 View 的渐进式迁移模板

对于复杂的 composables，建议在 composable 内部添加 store 支持：

```typescript
// composables/useSystemConfigSandbox.ts 修改建议
import { useSystemConfigStore } from '../store/useSystemConfigStore'
import type { ConfigType } from '../api/config.types'

export function useSystemConfigSandbox() {
  const store = useSystemConfigStore()
  const useStoreBackend = ref(true) // 可通过配置开关

  // ... 现有代码 ...

  const saveConfig = async () => {
    // 原有的 localStorage 保存
    persistConfig()

    // 新增：同时保存到后端
    if (useStoreBackend.value) {
      try {
        const dto = convertFormToSandboxConfigDto(form)
        await store.saveSandboxConfig(dto)
      } catch (err) {
        console.warn('Failed to save to backend, localStorage preserved')
      }
    }
  }

  return {
    // ... 现有返回值 ...
  }
}
```

## 完成标准

迁移完成的 view 应该：
1. 在 `onMounted` 时从 store 加载配置（或通过 composable 间接加载）
2. 保存时调用 `store.saveConfig()` 或 `store.saveSandboxConfig()`
3. 不再依赖纯 localStorage 持久化（作为 fallback 可保留）
4. TypeScript 编译通过
5. Vite build 成功

## 当前状态

| View | 状态 | 说明 |
|------|------|------|
| SystemConfigView | ✅ 完成 | 主页，简单表单 |
| SystemConfigPreferencesView | ✅ 完成 | 用户偏好 |
| SystemConfigDeepWikiView | ✅ 完成 | DeepWiki 配置 |
| SystemConfigSchedulesView | ✅ 完成 | 定时任务 |
| SystemConfigReviewView | ✅ 完成 | 代码评审 |
| SystemConfigSandboxView | 🔄 待处理 | 复杂：scenarios、risk 计算 |
| SystemConfigKnowledgeView | 🔄 待处理 | 复杂：source 管理 |
| SystemConfigSkillView | 🔄 待处理 | 复杂：skill 管理 |
| SystemConfigNotificationsView | 🔄 待处理 | 复杂：channels、scenarios |
| SystemConfigWorkflowView | 🔄 待处理 | 复杂：tabs、agent 流程 |
| SystemConfigIntegrationsView | 🔄 待处理 | 复杂：集成目录 |
| SystemConfigRepositoriesView | 🔄 待检查 | - |
| SystemConfigApiView | 🔄 待检查 | - |
| SystemConfigModelsView | 🔄 待检查 | - |
| SystemConfigUsersView | 🔄 待检查 | - |
