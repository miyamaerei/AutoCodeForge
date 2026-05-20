---
name: vue3-test-builder
description: 'Create test cases for Vue 3 project with Vitest. Use for: add unit tests, test composables, test stores, test components, ensure code quality.'
argument-hint: 'Describe what to test (e.g. user store CRUD operations)'
---

# Vue3 Test Builder

## When to Use
- You need to create unit tests for Vue 3 code.
- You want to test composables and stores.
- You need to test Vue components.
- You want to ensure code quality and prevent regressions.

## Fixed Conventions
1. Test files are placed next to source files with `.spec.ts` or `.test.ts` suffix.
2. Tests use Vitest as the test runner.
3. Tests use `@vue/test-utils` for component testing.
4. Tests follow the pattern: Arrange → Act → Assert.
5. Tests are independent and can run in any order.
6. Tests use descriptive names and clear assertions.

## Required Input
1. What to test: composable, store, component, or utility.
2. Test scenarios: happy path, edge cases, error cases.
3. Mock requirements: API calls, external dependencies.
4. Expected outcomes: what should happen.

## Output Structure
- `client/src/**/__tests__/<name>.spec.ts` - Test file in __tests__ directory
- `client/src/**/<name>.spec.ts` - Test file next to source file

## Test Templates

### 1. Composable Test Template
```typescript
import { describe, expect, it, vi, beforeEach } from 'vitest'
import { nextTick } from 'vue'
import { use<Name> } from '../use<Name>'

/**
 * use<Name> Tests
 */

describe('use<Name>', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('initialization', () => {
    it('should initialize with default state', () => {
      const { items, loading, error } = use<Name>()

      expect(items.value).toEqual([])
      expect(loading.value).toBe(false)
      expect(error.value).toBeNull()
    })
  })

  describe('fetchItems', () => {
    it('should fetch items successfully', async () => {
      const mockItems = [
        { id: '1', name: 'Item 1' },
        { id: '2', name: 'Item 2' },
      ]

      // Mock API call
      vi.mock('../api', () => ({
        fetchItems: vi.fn().mockResolvedValue(mockItems),
      }))

      const { items, loading, error, fetchItems } = use<Name>()

      await fetchItems()
      await nextTick()

      expect(items.value).toEqual(mockItems)
      expect(loading.value).toBe(false)
      expect(error.value).toBeNull()
    })

    it('should handle fetch error', async () => {
      const mockError = new Error('Network error')

      vi.mock('../api', () => ({
        fetchItems: vi.fn().mockRejectedValue(mockError),
      }))

      const { items, loading, error, fetchItems } = use<Name>()

      await fetchItems()
      await nextTick()

      expect(items.value).toEqual([])
      expect(loading.value).toBe(false)
      expect(error.value).toBe('Network error')
    })

    it('should set loading state during fetch', async () => {
      const { loading, fetchItems } = use<Name>()

      const promise = fetchItems()
      
      expect(loading.value).toBe(true)
      
      await promise
      await nextTick()
      
      expect(loading.value).toBe(false)
    })
  })

  describe('computed properties', () => {
    it('should compute hasItems correctly', () => {
      const { items, hasItems } = use<Name>()

      expect(hasItems.value).toBe(false)

      items.value = [{ id: '1', name: 'Item 1' }]

      expect(hasItems.value).toBe(true)
    })
  })

  describe('reset', () => {
    it('should reset all state to initial values', () => {
      const { items, error, reset } = use<Name>()

      items.value = [{ id: '1', name: 'Item 1' }]
      error.value = 'Some error'

      reset()

      expect(items.value).toEqual([])
      expect(error.value).toBeNull()
    })
  })
})
```

### 2. Store Test Template
```typescript
import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { use<Module>Store } from '../use<Module>Store'

/**
 * use<Module>Store Tests
 */

describe('use<Module>Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('state', () => {
    it('should have initial state', () => {
      const store = use<Module>Store()

      expect(store.items).toEqual([])
      expect(store.selectedItem).toBeNull()
      expect(store.loading).toBe(false)
      expect(store.error).toBeNull()
    })
  })

  describe('getters', () => {
    it('should compute hasItems correctly', () => {
      const store = use<Module>Store()

      expect(store.hasItems).toBe(false)

      store.items = [{ id: '1', name: 'Item 1' }]

      expect(store.hasItems).toBe(true)
    })

    it('should compute isLoading correctly', () => {
      const store = use<Module>Store()

      expect(store.isLoading).toBe(false)

      store.loading = true

      expect(store.isLoading).toBe(true)
    })
  })

  describe('actions', () => {
    describe('fetchItems', () => {
      it('should fetch items successfully', async () => {
        const mockItems = [
          { id: '1', name: 'Item 1' },
          { id: '2', name: 'Item 2' },
        ]

        vi.mock('../module.api', () => ({
          fetchItems: vi.fn().mockResolvedValue(mockItems),
        }))

        const store = use<Module>Store()

        await store.fetchItems()

        expect(store.items).toEqual(mockItems)
        expect(store.loading).toBe(false)
        expect(store.error).toBeNull()
      })

      it('should handle fetch error', async () => {
        vi.mock('../module.api', () => ({
          fetchItems: vi.fn().mockRejectedValue(new Error('Network error')),
        }))

        const store = use<Module>Store()

        await store.fetchItems()

        expect(store.items).toEqual([])
        expect(store.error).toBe('Network error')
      })
    })

    describe('createItem', () => {
      it('should create item successfully', async () => {
        const newItem = { id: '3', name: 'Item 3' }
        const payload = { name: 'Item 3' }

        vi.mock('../module.api', () => ({
          createItem: vi.fn().mockResolvedValue(newItem),
        }))

        const store = use<Module>Store()

        const result = await store.createItem(payload)

        expect(result).toEqual(newItem)
        expect(store.items).toContainEqual(newItem)
        expect(store.successMessage).toBe('创建成功')
      })

      it('should handle create error', async () => {
        vi.mock('../module.api', () => ({
          createItem: vi.fn().mockRejectedValue(new Error('Create failed')),
        }))

        const store = use<Module>Store()

        const result = await store.createItem({ name: 'Item' })

        expect(result).toBeNull()
        expect(store.error).toBe('Create failed')
      })
    })

    describe('updateItem', () => {
      it('should update item successfully', async () => {
        const store = use<Module>Store()
        store.items = [{ id: '1', name: 'Item 1' }]

        const updatedItem = { id: '1', name: 'Updated Item' }

        vi.mock('../module.api', () => ({
          updateItem: vi.fn().mockResolvedValue(updatedItem),
        }))

        const result = await store.updateItem('1', { name: 'Updated Item' })

        expect(result).toEqual(updatedItem)
        expect(store.items[0]).toEqual(updatedItem)
      })
    })

    describe('deleteItem', () => {
      it('should delete item successfully', async () => {
        const store = use<Module>Store()
        store.items = [
          { id: '1', name: 'Item 1' },
          { id: '2', name: 'Item 2' },
        ]

        vi.mock('../module.api', () => ({
          deleteItem: vi.fn().mockResolvedValue(undefined),
        }))

        const result = await store.deleteItem('1')

        expect(result).toBe(true)
        expect(store.items).toHaveLength(1)
        expect(store.items[0].id).toBe('2')
      })
    })
  })

  describe('reset', () => {
    it('should reset all state', () => {
      const store = use<Module>Store()

      store.items = [{ id: '1', name: 'Item 1' }]
      store.error = 'Some error'
      store.loading = true

      store.reset()

      expect(store.items).toEqual([])
      expect(store.error).toBeNull()
      expect(store.loading).toBe(false)
    })
  })
})
```

### 3. Component Test Template
```typescript
import { mount } from '@vue/test-utils'
import { describe, expect, it, vi } from 'vitest'
import <ComponentName> from '../<ComponentName>.vue'

/**
 * <ComponentName> Tests
 */

describe('<ComponentName>', () => {
  describe('rendering', () => {
    it('should render with required props', () => {
      const wrapper = mount(<ComponentName>, {
        props: {
          title: 'Test Title',
        },
      })

      expect(wrapper.find('h3').text()).toBe('Test Title')
    })

    it('should render with default props', () => {
      const wrapper = mount(<ComponentName>)

      expect(wrapper.props('size')).toBe('medium')
      expect(wrapper.props('disabled')).toBe(false)
    })

    it('should apply correct classes based on props', () => {
      const wrapper = mount(<ComponentName>, {
        props: {
          size: 'large',
          disabled: true,
        },
      })

      expect(wrapper.classes()).toContain('<component>--large')
      expect(wrapper.classes()).toContain('<component>--disabled')
    })
  })

  describe('interactions', () => {
    it('should emit click event when clicked', async () => {
      const wrapper = mount(<ComponentName>, {
        props: {
          id: 'test-id',
        },
      })

      await wrapper.trigger('click')

      expect(wrapper.emitted('click')).toBeTruthy()
      expect(wrapper.emitted('click')[0]).toEqual(['test-id'])
    })

    it('should not emit click event when disabled', async () => {
      const wrapper = mount(<ComponentName>, {
        props: {
          disabled: true,
        },
      })

      await wrapper.trigger('click')

      expect(wrapper.emitted('click')).toBeFalsy()
    })
  })

  describe('slots', () => {
    it('should render default slot', () => {
      const wrapper = mount(<ComponentName>, {
        slots: {
          default: 'Default content',
        },
      })

      expect(wrapper.text()).toContain('Default content')
    })

    it('should render named slots', () => {
      const wrapper = mount(<ComponentName>, {
        slots: {
          header: 'Header content',
          footer: 'Footer content',
        },
      })

      expect(wrapper.text()).toContain('Header content')
      expect(wrapper.text()).toContain('Footer content')
    })
  })

  describe('accessibility', () => {
    it('should have correct ARIA attributes', () => {
      const wrapper = mount(<ComponentName>)

      expect(wrapper.attributes('role')).toBe('button')
      expect(wrapper.attributes('aria-label')).toBeTruthy()
    })

    it('should be keyboard accessible', async () => {
      const wrapper = mount(<ComponentName>)

      await wrapper.trigger('keydown.enter')

      expect(wrapper.emitted('click')).toBeTruthy()
    })
  })
})
```

### 4. API Test Template
```typescript
import { describe, expect, it, vi, beforeEach } from 'vitest'
import { fetchItems, createItem } from '../module.api'

/**
 * Module API Tests
 */

// Mock axios
vi.mock('../../../lib/request', () => ({
  request: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}))

describe('module.api', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('fetchItems', () => {
    it('should fetch items from API', async () => {
      const mockItems = [
        { id: '1', name: 'Item 1' },
        { id: '2', name: 'Item 2' },
      ]

      const { request } = await import('../../../lib/request')
      vi.mocked(request.get).mockResolvedValue({ data: mockItems })

      const result = await fetchItems()

      expect(request.get).toHaveBeenCalledWith('/module')
      expect(result).toEqual(mockItems)
    })

    it('should use mock when USE_MOCK is true', async () => {
      vi.mock('../../../config/runtime', () => ({
        USE_MOCK: true,
      }))

      const result = await fetchItems()

      // Should return mock data
      expect(result).toBeDefined()
    })
  })

  describe('createItem', () => {
    it('should create item via API', async () => {
      const payload = { name: 'New Item' }
      const mockResponse = { id: '3', name: 'New Item' }

      const { request } = await import('../../../lib/request')
      vi.mocked(request.post).mockResolvedValue({ data: mockResponse })

      const result = await createItem(payload)

      expect(request.post).toHaveBeenCalledWith('/module', payload)
      expect(result).toEqual(mockResponse)
    })
  })
})
```

## Test Patterns

### 1. Arrange-Act-Assert Pattern
```typescript
it('should do something', () => {
  // Arrange: Setup test data and conditions
  const input = 'test'
  
  // Act: Execute the code being tested
  const result = functionUnderTest(input)
  
  // Assert: Verify the outcome
  expect(result).toBe('expected')
})
```

### 2. Given-When-Then Pattern
```typescript
it('should do something', () => {
  // Given: Initial state
  const store = useStore()
  
  // When: Action is performed
  store.doSomething()
  
  // Then: Expected outcome
  expect(store.state).toBe('expected')
})
```

### 3. Test Each Pattern
```typescript
describe.each([
  ['small', 8],
  ['medium', 16],
  ['large', 24],
])('size %s', (size, expectedPadding) => {
  it(`should have padding ${expectedPadding}px`, () => {
    const wrapper = mount(Component, { props: { size } })
    expect(wrapper.classes()).toContain(`component--${size}`)
  })
})
```

## Best Practices

### 1. Test Organization
- Group related tests with `describe`
- Use clear, descriptive test names
- One assertion per test when possible
- Keep tests focused and small

### 2. Mocking
- Mock external dependencies
- Use `vi.fn()` for function mocks
- Use `vi.mock()` for module mocks
- Clear mocks in `beforeEach`

### 3. Assertions
- Use specific assertions
- Avoid `toBeTruthy()` when possible
- Test both success and error cases
- Test edge cases

### 4. Async Testing
- Use `async/await` for async tests
- Use `nextTick()` for Vue reactivity
- Test loading states
- Test error handling

### 5. Component Testing
- Test props rendering
- Test event emissions
- Test slot content
- Test accessibility

## Execution Checklist
1. Identify what to test.
2. Set up test file structure.
3. Mock external dependencies.
4. Write test cases for each scenario.
5. Test happy path first.
6. Test error cases.
7. Test edge cases.
8. Run tests and verify.
9. Check test coverage.

## Quality Gates
1. All tests pass.
2. Tests are independent.
3. Tests are fast.
4. Tests are maintainable.
5. Coverage meets threshold.
6. No flaky tests.

## Example Prompts
- /vue3-test-builder create tests for useUserStore with CRUD operations
- /vue3-test-builder create tests for UserCard component
- /vue3-test-builder create tests for user API with mock switching
- /vue3-test-builder create tests for useUserList composable with search
