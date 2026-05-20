---
name: vue3-store-builder
description: 'Create Pinia store for Vue 3 with setup syntax. Use for: add state management, create store with actions/getters, manage loading/error states, implement CRUD operations.'
argument-hint: 'Describe store name and features (e.g. user store with CRUD and search)'
---

# Vue3 Store Builder

## When to Use
- You need to create a Pinia store for state management.
- You want to manage complex application state.
- You need to share state across multiple components.
- You want to implement CRUD operations with loading states.

## Fixed Conventions
1. Stores are placed under `client/src/modules/<module>/store/` or `client/src/stores/`.
2. Store names use camelCase with `use` prefix and `Store` suffix.
3. Stores use Pinia setup store syntax.
4. Store IDs follow pattern: `module.<feature>`.
5. Stores include loading, error, and success states.
6. Actions are async and handle errors properly.

## Required Input
1. Store name with `use` prefix, example `useUserStore`.
2. Module name (if module-specific), example `user-management`.
3. State shape: what data needs to be stored.
4. Actions needed: what operations to perform.
5. Getters needed: what computed values to expose.

## Output Structure
- `client/src/modules/<module>/store/use<Module>Store.ts` (for module-specific)
- `client/src/stores/use<Name>Store.ts` (for global stores)

## Template Structure

### Basic Store Template
```typescript
import { computed, ref } from 'vue'
import { defineStore } from 'pinia'

/**
 * use<Name>Store - <Description>
 * 
 * @example
 * const store = use<Name>Store()
 * await store.fetchItems()
 */

export const use<Name>Store = defineStore('module.<name>', () => {
  // State
  const items = ref<<Entity>[]>([])
  const selectedItem = ref<<Entity> | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const hasItems = computed(() => items.value.length > 0)
  const hasSelectedItem = computed(() => selectedItem.value !== null)
  const itemCount = computed(() => items.value.length)

  // Actions
  async function fetchItems(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      items.value = await api.fetchItems()
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch items'
    } finally {
      loading.value = false
    }
  }

  async function fetchItem(id: string): Promise<void> {
    loading.value = true
    error.value = null
    try {
      selectedItem.value = await api.fetchItem(id)
    } catch (err) {
      selectedItem.value = null
      error.value = err instanceof Error ? err.message : 'Failed to fetch item'
    } finally {
      loading.value = false
    }
  }

  function reset(): void {
    items.value = []
    selectedItem.value = null
    error.value = null
    loading.value = false
  }

  return {
    // State
    items,
    selectedItem,
    loading,
    error,
    // Getters
    hasItems,
    hasSelectedItem,
    itemCount,
    // Actions
    fetchItems,
    fetchItem,
    reset,
  }
})
```

### CRUD Store Template
```typescript
import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import {
  create<Entity>,
  delete<Entity>,
  fetch<Entity>Detail,
  fetch<Entity>List,
  update<Entity>,
  type <Entity>DetailDto,
  type <Entity>ListItemDto,
  type <Entity>CreateRequestDto,
  type <Entity>UpdateRequestDto,
} from '../<module>.api'

/**
 * use<Module>Store - <Description>
 */

export const use<Module>Store = defineStore('module.<module>', () => {
  // State
  const items = ref<<Entity>ListItemDto[]>([])
  const selectedItem = ref<<Entity>DetailDto | null>(null)
  
  const loading = ref(false)
  const detailLoading = ref(false)
  const creating = ref(false)
  const updating = ref(false)
  const deleting = ref(false)
  
  const error = ref<string | null>(null)
  const successMessage = ref<string | null>(null)

  // Getters
  const hasItems = computed(() => items.value.length > 0)
  const hasSelectedItem = computed(() => selectedItem.value !== null)
  const isLoading = computed(() => 
    loading.value || detailLoading.value || creating.value || updating.value || deleting.value
  )

  // Actions - Fetch List
  async function fetchItems(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      items.value = await fetch<Entity>List()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载列表失败'
    } finally {
      loading.value = false
    }
  }

  // Actions - Fetch Detail
  async function fetchItemDetail(id: string): Promise<void> {
    detailLoading.value = true
    error.value = null
    try {
      selectedItem.value = await fetch<Entity>Detail(id)
    } catch (err) {
      selectedItem.value = null
      error.value = err instanceof Error ? err.message : '加载详情失败'
    } finally {
      detailLoading.value = false
    }
  }

  // Actions - Create
  async function createItem(payload: <Entity>CreateRequestDto): Promise<<Entity>ListItemDto | null> {
    creating.value = true
    error.value = null
    successMessage.value = null
    try {
      const newItem = await create<Entity>(payload)
      items.value.unshift(newItem)
      successMessage.value = '创建成功'
      return newItem
    } catch (err) {
      error.value = err instanceof Error ? err.message : '创建失败'
      return null
    } finally {
      creating.value = false
    }
  }

  // Actions - Update
  async function updateItem(
    id: string,
    payload: <Entity>UpdateRequestDto
  ): Promise<<Entity>DetailDto | null> {
    updating.value = true
    error.value = null
    successMessage.value = null
    try {
      const updatedItem = await update<Entity>(id, payload)
      // Update in list
      const index = items.value.findIndex(item => item.id === id)
      if (index !== -1) {
        items.value[index] = updatedItem
      }
      // Update selected item if it's the same
      if (selectedItem.value?.id === id) {
        selectedItem.value = updatedItem
      }
      successMessage.value = '更新成功'
      return updatedItem
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新失败'
      return null
    } finally {
      updating.value = false
    }
  }

  // Actions - Delete
  async function deleteItem(id: string): Promise<boolean> {
    deleting.value = true
    error.value = null
    successMessage.value = null
    try {
      await delete<Entity>(id)
      // Remove from list
      items.value = items.value.filter(item => item.id !== id)
      // Clear selected item if it's the same
      if (selectedItem.value?.id === id) {
        selectedItem.value = null
      }
      successMessage.value = '删除成功'
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除失败'
      return false
    } finally {
      deleting.value = false
    }
  }

  // Actions - Reset
  function reset(): void {
    items.value = []
    selectedItem.value = null
    error.value = null
    successMessage.value = null
    loading.value = false
    detailLoading.value = false
    creating.value = false
    updating.value = false
    deleting.value = false
  }

  // Actions - Clear Messages
  function clearMessages(): void {
    error.value = null
    successMessage.value = null
  }

  return {
    // State
    items,
    selectedItem,
    loading,
    detailLoading,
    creating,
    updating,
    deleting,
    error,
    successMessage,
    // Getters
    hasItems,
    hasSelectedItem,
    isLoading,
    // Actions
    fetchItems,
    fetchItemDetail,
    createItem,
    updateItem,
    deleteItem,
    reset,
    clearMessages,
  }
})
```

### Search/Filter Store Template
```typescript
import { computed, ref } from 'vue'
import { defineStore } from 'pinia'

/**
 * use<Module>Store - <Description> with search and filter
 */

export const use<Module>Store = defineStore('module.<module>', () => {
  // State
  const items = ref<<Entity>[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  
  // Search/Filter State
  const searchQuery = ref('')
  const filters = ref<{
    status?: string
    category?: string
    dateRange?: [string, string]
  }>({})
  
  const pagination = ref({
    page: 1,
    pageSize: 10,
    total: 0,
  })

  // Getters
  const filteredItems = computed(() => {
    let result = items.value

    // Apply search
    if (searchQuery.value) {
      const query = searchQuery.value.toLowerCase()
      result = result.filter(item =>
        item.name.toLowerCase().includes(query) ||
        item.description.toLowerCase().includes(query)
      )
    }

    // Apply filters
    if (filters.value.status) {
      result = result.filter(item => item.status === filters.value.status)
    }

    if (filters.value.category) {
      result = result.filter(item => item.category === filters.value.category)
    }

    return result
  })

  const paginatedItems = computed(() => {
    const start = (pagination.value.page - 1) * pagination.value.pageSize
    const end = start + pagination.value.pageSize
    return filteredItems.value.slice(start, end)
  })

  const totalPages = computed(() =>
    Math.ceil(filteredItems.value.length / pagination.value.pageSize)
  )

  // Actions
  async function fetchItems(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      items.value = await api.fetchItems()
      pagination.value.total = items.value.length
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch items'
    } finally {
      loading.value = false
    }
  }

  function setSearchQuery(query: string): void {
    searchQuery.value = query
    pagination.value.page = 1
  }

  function setFilters(newFilters: typeof filters.value): void {
    filters.value = { ...filters.value, ...newFilters }
    pagination.value.page = 1
  }

  function clearFilters(): void {
    searchQuery.value = ''
    filters.value = {}
    pagination.value.page = 1
  }

  function setPage(page: number): void {
    pagination.value.page = page
  }

  function setPageSize(size: number): void {
    pagination.value.pageSize = size
    pagination.value.page = 1
  }

  return {
    // State
    items,
    loading,
    error,
    searchQuery,
    filters,
    pagination,
    // Getters
    filteredItems,
    paginatedItems,
    totalPages,
    // Actions
    fetchItems,
    setSearchQuery,
    setFilters,
    clearFilters,
    setPage,
    setPageSize,
  }
})
```

## Store Patterns

### 1. Basic State Management
- 简单的状态存储
- 基本的 getter 和 setter
- 适用于小型功能

### 2. CRUD Operations
- 完整的增删改查操作
- 独立的 loading 状态
- 错误和成功消息管理

### 3. Search and Filter
- 搜索功能
- 过滤功能
- 分页功能

### 4. Real-time Updates
- WebSocket 集成
- 实时数据同步
- 状态订阅

### 5. Optimistic Updates
- 即时 UI 更新
- 失败回滚
- 提升用户体验

## Best Practices

### 1. State Design
- 使用 `ref` 定义响应式状态
- 避免深层嵌套的状态结构
- 保持状态的扁平化
- 使用 TypeScript 类型约束

### 2. Actions Design
- Actions 应该是异步的
- 统一的错误处理
- 明确的 loading 状态
- 返回有意义的结果

### 3. Getters Design
- 使用 `computed` 定义 getter
- 避免在 getter 中进行副作用操作
- 保持 getter 的纯函数特性
- 利用缓存提升性能

### 4. Error Handling
- 统一的错误消息格式
- 错误状态的清理机制
- 用户友好的错误提示
- 错误日志记录

### 5. Performance
- 避免不必要的计算
- 使用 `computed` 缓存结果
- 合理使用 `watch`
- 避免深层响应式

## Execution Checklist
1. Define store name and module.
2. Design state structure.
3. Design getters for computed values.
4. Implement actions with error handling.
5. Add loading states for async operations.
6. Add error and success message states.
7. Implement reset functionality.
8. Add TypeScript types.
9. Add JSDoc comments.

## Quality Gates
1. Store uses setup syntax.
2. State is properly typed.
3. Actions handle errors properly.
4. Loading states are included.
5. Getters are computed properties.
6. Reset functionality is implemented.
7. JSDoc comments are complete.

## Example Prompts
- /vue3-store-builder create useUserStore with CRUD operations
- /vue3-store-builder create useProductStore with search and filter
- /vue3-store-builder create useCartStore with real-time updates
- /vue3-store-builder create useOrderStore with optimistic updates
