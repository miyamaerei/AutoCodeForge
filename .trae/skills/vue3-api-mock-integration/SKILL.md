---
name: vue3-api-mock-integration
description: 'Create Vue 3 API layer with mock/real API switching. Use for: add API functions with mock support, integrate with axios, support development with mock data.'
argument-hint: 'Describe API endpoints and data types (e.g. user API with CRUD operations)'
---

# Vue3 API Mock Integration

## When to Use
- You need to create API functions that support mock/real switching.
- You want to develop with mock data and switch to real API later.
- You need consistent API layer structure.
- You want to integrate with axios HTTP client.

## Fixed Conventions
1. API files are placed under `client/src/modules/<module>/<module>.api.ts`.
2. API uses axios instance from `client/src/lib/request.ts`.
3. Mock switching is controlled by `USE_MOCK` from `client/src/config/runtime.ts`.
4. API functions return DTOs (Data Transfer Objects).
5. API functions are async and throw errors on failure.
6. Types are exported alongside API functions.

## Required Input
1. Module name in kebab-case, example `user-management`.
2. API endpoints: base path and specific endpoints.
3. DTO types: list item, detail item, create request, update request.
4. HTTP methods: GET, POST, PUT, DELETE.
5. Mock functions to integrate (from `client/src/mock/`).

## Output Structure
- `client/src/modules/<module>/<module>.api.ts` - API functions with mock support
- Types are defined in the same file or separate `.types.ts` file

## Template Structure

```typescript
import { request } from '../../lib/request'
import { USE_MOCK } from '../../config/runtime'
import {
  get<Entity>List as get<Entity>ListMock,
  get<Entity>Detail as get<Entity>DetailMock,
  create<Entity> as create<Entity>Mock,
  update<Entity> as update<Entity>Mock,
  delete<Entity> as delete<Entity>Mock,
  type <Entity>ListItemDto,
  type <Entity>DetailDto,
  type <Entity>CreateRequestDto,
  type <Entity>UpdateRequestDto,
} from '../../mock'

// Re-export types
export type {
  <Entity>ListItemDto,
  <Entity>DetailDto,
  <Entity>CreateRequestDto,
  <Entity>UpdateRequestDto,
}

// API Base Path
const API_BASE = '/<entity>'

// API Functions
export async function fetch<Entity>List(): Promise<<Entity>ListItemDto[]> {
  if (USE_MOCK) {
    return get<Entity>ListMock()
  }
  const { data } = await request.get<<Entity>ListItemDto[]>(API_BASE)
  return data
}

export async function fetch<Entity>Detail(id: string): Promise<<Entity>DetailDto> {
  if (USE_MOCK) {
    return get<Entity>DetailMock(id)
  }
  const { data } = await request.get<<Entity>DetailDto>(`${API_BASE}/${id}`)
  return data
}

export async function create<Entity>(
  payload: <Entity>CreateRequestDto
): Promise<<Entity>ListItemDto> {
  if (USE_MOCK) {
    return create<Entity>Mock(payload)
  }
  const { data } = await request.post<<Entity>ListItemDto>(API_BASE, payload)
  return data
}

export async function update<Entity>(
  id: string,
  payload: <Entity>UpdateRequestDto
): Promise<<Entity>DetailDto> {
  if (USE_MOCK) {
    return update<Entity>Mock(id, payload)
  }
  const { data } = await request.put<<Entity>DetailDto>(`${API_BASE}/${id}`, payload)
  return data
}

export async function delete<Entity>(id: string): Promise<void> {
  if (USE_MOCK) {
    return delete<Entity>Mock(id)
  }
  await request.delete(`${API_BASE}/${id}`)
}
```

## Execution Checklist
1. Import axios instance from `client/src/lib/request.ts`.
2. Import `USE_MOCK` from `client/src/config/runtime.ts`.
3. Import mock functions and types from `client/src/mock/`.
4. Re-export DTO types for external use.
5. Define API base path constant.
6. Implement API functions with mock switching.
7. Handle errors properly (axios interceptor handles this).
8. Export all API functions.

## Mock Switching Pattern
```typescript
export async function fetchData(): Promise<DataDto> {
  if (USE_MOCK) {
    return fetchDataMock()  // Use mock function
  }
  const { data } = await request.get<DataDto>('/endpoint')  // Use real API
  return data
}
```

## Request Configuration
The axios instance from `client/src/lib/request.ts` includes:
- Base URL from environment variable `VITE_API_BASE_URL`
- Default timeout: 10000ms
- Default headers: `Content-Type: application/json`
- Request interceptor: adds `X-Requested-With` header
- Response interceptor: handles errors and extracts message

## Error Handling
Errors are handled by axios interceptor:
```typescript
// In client/src/lib/request.ts
request.interceptors.response.use(
  (response) => response,
  (error: AxiosError<{ message?: string }>) => {
    const message = error.response?.data?.message || error.message || 'Request failed'
    return Promise.reject(new Error(message))
  },
)
```

API functions can catch errors:
```typescript
try {
  const data = await fetch<Entity>List()
} catch (error) {
  // Error message is already formatted
  console.error(error.message)
}
```

## Query Parameters
For endpoints with query parameters:
```typescript
export async function fetch<Entity>List(params: {
  page?: number
  pageSize?: number
  search?: string
}): Promise<<Entity>ListItemDto[]> {
  if (USE_MOCK) {
    return get<Entity>ListMock(params)
  }
  const { data } = await request.get<<Entity>ListItemDto[]>(API_BASE, { params })
  return data
}
```

## Path Parameters
For endpoints with path parameters:
```typescript
export async function fetch<Entity>ByCategory(
  category: string
): Promise<<Entity>ListItemDto[]> {
  if (USE_MOCK) {
    return get<Entity>ByCategoryMock(category)
  }
  const { data } = await request.get<<Entity>ListItemDto[]>(
    `${API_BASE}/category/${category}`
  )
  return data
}
```

## Module Export Pattern
API functions are exported from module index:
```typescript
// client/src/modules/<module>/index.ts
export { <module>Routes } from './routes'
export {
  fetch<Entity>List,
  fetch<Entity>Detail,
  create<Entity>,
  type <Entity>ListItemDto,
  type <Entity>DetailDto,
} from './<module>.api'
export { use<Module>Store } from './store/use<Module>Store'
```

## Quality Gates
1. Mock switching is implemented correctly.
2. Types are properly imported and re-exported.
3. API functions use axios instance from `client/src/lib/request.ts`.
4. Error handling is consistent.
5. Module exports are updated.
6. API functions match mock function signatures.

## Example Prompts
- /vue3-api-mock-integration create user API with CRUD operations
- /vue3-api-mock-integration create product API with search and pagination
- /vue3-api-mock-integration create order API with status filtering
