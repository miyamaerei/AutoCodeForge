---
name: vue3-composable-builder
description: 'Create Vue 3 composables with TypeScript. Use for: add reusable logic functions, encapsulate state and methods, create composables with loading/error states.'
argument-hint: 'Describe composable purpose and features (e.g. user list with pagination and search)'
---

# Vue3 Composable Builder

## When to Use
- You need to create a reusable composable function.
- You want to encapsulate state, computed properties, and methods.
- You need consistent composable structure with loading/error states.
- You want to follow Vue 3 Composition API best practices.

## Fixed Conventions
1. Composables are placed under `client/src/modules/<module>/composables/` or `client/src/composables/`.
2. Composable names start with `use` prefix, example `useUserList`.
3. Composables use TypeScript with explicit type definitions.
4. Composables return reactive state and methods.
5. Loading and error states are included by default.
6. Composables are pure logic, no direct DOM manipulation.

## Required Input
1. Composable name with `use` prefix, example `useUserList`.
2. Module name (if module-specific), example `user-management`.
3. Features needed: data fetching, pagination, search, filtering, etc.
4. State shape: what data needs to be managed.
5. Methods needed: what actions should be exposed.

## Output Structure
- `client/src/modules/<module>/composables/use<Name>.ts` (for module-specific)
- `client/src/composables/use<Name>.ts` (for shared composables)

## Template Structure

```typescript
import { computed, ref } from 'vue'

export interface <Entity>Item {
  id: string
  // ... other fields
}

export interface <Entity>Filter {
  // ... filter fields
}

export function use<Name>() {
  // State
  const items = ref<<Entity>Item[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const filter = ref<<Entity>Filter>({})

  // Computed
  const hasItems = computed(() => items.value.length > 0)
  const filteredItems = computed(() => {
    // ... filtering logic
    return items.value
  })

  // Methods
  async function fetchItems(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      // ... fetch logic
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch items'
    } finally {
      loading.value = false
    }
  }

  function reset(): void {
    items.value = []
    error.value = null
    filter.value = {}
  }

  return {
    // State
    items,
    loading,
    error,
    filter,
    // Computed
    hasItems,
    filteredItems,
    // Methods
    fetchItems,
    reset,
  }
}
```

## Execution Checklist
1. Define TypeScript interfaces for data types.
2. Create reactive state with `ref` or `reactive`.
3. Add computed properties for derived state.
4. Implement methods with proper error handling.
5. Include loading and error states.
6. Return all state and methods.
7. Add JSDoc comments for exported functions and types.

## Best Practices
1. Keep composables focused on a single responsibility.
2. Use TypeScript for all type definitions.
3. Handle errors gracefully with try-catch.
4. Reset state when needed.
5. Use computed properties for derived state.
6. Avoid side effects outside of methods.
7. Make composables testable.

## Common Patterns

### Data Fetching with Cache
```typescript
const cache = ref<Map<string, Data>>(new Map())

async function fetchData(id: string): Promise<Data> {
  if (cache.value.has(id)) {
    return cache.value.get(id)!
  }
  const data = await api.fetch(id)
  cache.value.set(id, data)
  return data
}
```

### Pagination
```typescript
const page = ref(1)
const pageSize = ref(10)
const total = ref(0)

const totalPages = computed(() => Math.ceil(total.value / pageSize.value))

async function loadPage(p: number): Promise<void> {
  page.value = p
  await fetchItems()
}
```

### Search with Debounce
```typescript
import { watchDebounced } from '@vueuse/core'

const searchQuery = ref('')

watchDebounced(searchQuery, async (query) => {
  await fetchItems(query)
}, { debounce: 300 })
```

## Quality Gates
1. TypeScript types are properly defined.
2. Loading and error states are included.
3. Methods have proper error handling.
4. Composable returns all necessary state and methods.
5. Code follows project conventions.
6. JSDoc comments are added.

## Example Prompts
- /vue3-composable-builder create useUserList with pagination and search
- /vue3-composable-builder create useProductFilter for product module with filtering and sorting
- /vue3-composable-builder create useChatSession with message history and send functionality
