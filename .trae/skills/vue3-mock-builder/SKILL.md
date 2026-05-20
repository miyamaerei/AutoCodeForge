---
name: vue3-mock-builder
description: 'Create mock data and functions for Vue 3 API layer. Use for: add mock DTOs, create mock API functions, support mock/real API switching.'
argument-hint: 'Describe entity and mock data requirements (e.g. user list with 10 items)'
---

# Vue3 Mock Builder

## When to Use
- You need to create mock data for development or testing.
- You want to support mock/real API switching.
- You need consistent mock data structure.
- You want to simulate API delays and responses.

## Fixed Conventions
1. Mock files are placed under `client/src/mock/` directory.
2. Mock data uses TypeScript with explicit DTO types.
3. Mock functions simulate async behavior with delays.
4. Mock data is exported from `client/src/mock/index.ts`.
5. Mock functions follow the same signature as real API functions.
6. Mock delay is typically 200-500ms to simulate network latency.

## Required Input
1. Entity name in kebab-case, example `user-profiles`.
2. DTO types needed: list item, detail item, create request, update request.
3. Mock data count and sample values.
4. Mock functions needed: get list, get detail, create, update, delete.
5. Error scenarios to mock: not found, validation error, etc.

## Output Structure
- `client/src/mock/<entity>.ts` - Mock data and functions
- Update `client/src/mock/index.ts` - Export new mock functions

## Template Structure

```typescript
// DTO Types
export interface <Entity>ListItemDto {
  id: string
  // ... other fields
  createdAt: string
}

export interface <Entity>DetailDto {
  id: string
  // ... other fields
}

export interface <Entity>CreateRequestDto {
  // ... create fields
}

export interface <Entity>UpdateRequestDto {
  // ... update fields
}

// Mock Data
const <entity>List: <Entity>ListItemDto[] = [
  {
    id: '1',
    // ... sample data
    createdAt: '2024-05-19 14:30',
  },
  // ... more items
]

const <entity>Details: Record<string, <Entity>DetailDto> = {
  '1': {
    id: '1',
    // ... sample data
  },
}

// Helper function for delay
async function sleep(ms: number): Promise<void> {
  await new Promise((resolve) => setTimeout(resolve, ms))
}

// Mock Functions
export async function get<Entity>List(): Promise<<Entity>ListItemDto[]> {
  await sleep(220)
  return <entity>List
}

export async function get<Entity>Detail(id: string): Promise<<Entity>DetailDto> {
  await sleep(260)
  const detail = <entity>Details[id]
  if (!detail) {
    throw new Error('<Entity> not found')
  }
  return detail
}

export async function create<Entity>(
  payload: <Entity>CreateRequestDto
): Promise<<Entity>ListItemDto> {
  await sleep(240)
  const now = Date.now().toString().slice(-4)
  const item: <Entity>ListItemDto = {
    id: `<entity>-${now}`,
    // ... map payload to item
    createdAt: new Date().toLocaleString('zh-CN', { hour12: false }),
  }
  <entity>List.unshift(item)
  <entity>Details[item.id] = {
    id: item.id,
    // ... map item to detail
  }
  return item
}

export async function update<Entity>(
  id: string,
  payload: <Entity>UpdateRequestDto
): Promise<<Entity>DetailDto> {
  await sleep(240)
  const detail = <entity>Details[id]
  if (!detail) {
    throw new Error('<Entity> not found')
  }
  // ... update logic
  return detail
}

export async function delete<Entity>(id: string): Promise<void> {
  await sleep(200)
  const index = <entity>List.findIndex((item) => item.id === id)
  if (index === -1) {
    throw new Error('<Entity> not found')
  }
  <entity>List.splice(index, 1)
  delete <entity>Details[id]
}
```

## Execution Checklist
1. Define DTO types with TypeScript.
2. Create mock data arrays and objects.
3. Implement mock functions with delays.
4. Add error handling for not found scenarios.
5. Export all types and functions.
6. Update `client/src/mock/index.ts` to export new mocks.
7. Ensure mock functions match real API signatures.

## Mock Data Guidelines
1. Use realistic sample data.
2. Include various states: active, inactive, pending, etc.
3. Include edge cases: empty strings, long text, special characters.
4. Use consistent date formats: `YYYY-MM-DD HH:mm` or ISO format.
5. Include Chinese text for localized fields.
6. Generate unique IDs using timestamp or random strings.

## Delay Simulation
- List operations: 200-300ms
- Detail operations: 250-350ms
- Create/Update operations: 200-300ms
- Delete operations: 150-250ms

## Error Scenarios
1. **Not Found**: Throw error when entity doesn't exist.
2. **Validation Error**: Throw error for invalid input.
3. **Network Error**: Randomly fail to simulate network issues (optional).

## Integration with API Layer
Mock functions are used in API layer with USE_MOCK flag:

```typescript
// client/src/modules/<module>/<module>.api.ts
import { USE_MOCK } from '../../config/runtime'
import {
  get<Entity>List as get<Entity>ListMock,
  // ... other mock imports
} from '../../mock'

export async function fetch<Entity>List(): Promise<<Entity>ListItemDto[]> {
  if (USE_MOCK) {
    return get<Entity>ListMock()
  }
  const { data } = await request.get<<Entity>ListItemDto[]>('/<entity>')
  return data
}
```

## Quality Gates
1. DTO types are properly defined.
2. Mock data is realistic and comprehensive.
3. Mock functions have proper delays.
4. Error scenarios are handled.
5. Exports are added to `client/src/mock/index.ts`.
6. Mock functions match real API signatures.

## Example Prompts
- /vue3-mock-builder create user mock with list detail create update delete
- /vue3-mock-builder create product mock with 20 items and search functionality
- /vue3-mock-builder create order mock with various states: pending, processing, completed, cancelled
