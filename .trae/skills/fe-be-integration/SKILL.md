---
name: fe-be-integration
description: "Wire Vue 3 frontend modules to .NET backend endpoints. Invoke when integrating frontend with backend, fixing path mismatches, or migrating mock to real API."
---

# Frontend ↔ Backend Integration Skill

## When to Use
- You want to replace mock data with real API calls for one module.
- You need to fix path mismatches between frontend API functions and backend routes.
- You want a complete, verifiable integration pass: DTO alignment + API wiring + store update + smoke test.
- You are running a continuous integration loop across all modules until all are wired.

## Prerequisite Skill
Before executing, read the contract map:
- `.trae/skills/fe-be-contract-map/SKILL.md`

The contract map is the single source of truth for:
- Which module to pick next (priority ranking).
- What backend paths to use.
- Which frontend functions are missing.
- What DTO shapes are expected.

---

## Execution Loop

Repeat this loop for each module until all modules reach ✅ status in the contract map.

```
┌─────────────────────────────────────────────────────────────────┐
│  1. Select Module (from contract map priority)                  │
│  2. Audit Current State (read existing api file if any)         │
│  3. Align DTOs (match backend response shapes)                  │
│  4. Fix or Create API Functions (correct paths, add missing)    │
│  5. Update Store (wire store actions to real API functions)     │
│  6. Wire Auth (ensure token is attached)                        │
│  7. Disable Mock Branch (or set USE_MOCK = false scope)         │
│  8. Smoke Verify (build check + type check)                     │
│  9. Update Contract Map status for this module                  │
│  10. Pick next module → repeat                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Step 1 — Select Module

If a module name is provided as argument, use it.  
Otherwise, read the contract map's **Integration Priority Ranking** table and pick the first module whose status is not ✅.

**Default order**: Auth → Task Center → Console/Chat → Agent Center → Repo Management → Scheduled Task → Pipeline Center → Wiki → Review → System Config

---

## Step 2 — Audit Current State

Read these files for the selected module:
- `client/src/modules/<module>/api/<module>.api.ts` (if exists)
- `client/src/modules/<module>/api/<module>.types.ts` (if exists)
- `client/src/modules/<module>/store/use<Module>Store.ts`
- `client/src/config/runtime.ts` — confirm `USE_MOCK` switch
- `client/vite.config.ts` — check proxy rules for `/api` prefix

Record:
- Which functions exist vs. which are missing.
- Current base path used vs. backend canonical path.
- Whether mock branch is inside the same function or in a separate mock file.

---

## Step 3 — Align DTOs

Read the backend endpoint file:
- `server/src/AutoCodeForge.Api/Endpoints/<Module>Endpoints.cs`

Extract every request and response type. Then check or create:
- `client/src/modules/<module>/api/<module>.types.ts`

### DTO Alignment Rules
1. Property names: camelCase to match backend JSON serialization.
2. IDs: always `string` (backend `Guid` → JSON `string`).
3. Dates: always `string` (ISO 8601).
4. Paged response shape:
   ```typescript
   interface PagedResult<T> {
     items: T[]
     totalCount: number
     page: number
     pageSize: number
   }
   ```
5. Error shape:
   ```typescript
   interface ApiError {
     message: string
     errors?: Record<string, string[]>
   }
   ```

If the types file already exists, compare each property. Add missing fields. Do not remove existing fields without confirming the backend no longer returns them.

---

## Step 4 — Fix or Create API Functions

Edit `client/src/modules/<module>/api/<module>.api.ts`.

### Path Fix Pattern
```typescript
// Wrong — old frontend-relative path:
const { data } = await request.get('/task-center/tasks')

// Correct — backend canonical path:
const { data } = await request.get('/api/v1/tasks')
```

### Standard Function Template
```typescript
import type { PagedResult, MyDto, CreateMyDto } from './my-module.types'
import { request } from '@/lib/request'   // adjust import path as needed
import { USE_MOCK } from '@/config/runtime'
import { getMyMock, getMyListMock } from '@/mock/my-module.mock'   // if mock file exists

export async function fetchItems(page = 1, pageSize = 20): Promise<PagedResult<MyDto>> {
  if (USE_MOCK) return getMyListMock()
  const { data } = await request.get<PagedResult<MyDto>>('/api/v1/my-module', { params: { page, pageSize } })
  return data
}

export async function fetchItem(id: string): Promise<MyDto> {
  if (USE_MOCK) return getMyMock(id)
  const { data } = await request.get<MyDto>(`/api/v1/my-module/${id}`)
  return data
}

export async function createItem(payload: CreateMyDto): Promise<MyDto> {
  if (USE_MOCK) return createMyMock(payload)
  const { data } = await request.post<MyDto>('/api/v1/my-module', payload)
  return data
}

export async function updateItem(id: string, payload: Partial<CreateMyDto>): Promise<MyDto> {
  if (USE_MOCK) return updateMyMock(id, payload)
  const { data } = await request.put<MyDto>(`/api/v1/my-module/${id}`, payload)
  return data
}

export async function deleteItem(id: string): Promise<void> {
  if (USE_MOCK) return deleteMyMock(id)
  await request.delete(`/api/v1/my-module/${id}`)
}
```

### Rules
- Keep the `if (USE_MOCK)` branch — do not delete mock support.
- Each function is responsible for exactly one HTTP operation.
- Do not add retry logic, caching, or interceptors in the API file — that belongs in the axios instance.
- Do not throw custom errors in API functions — let the axios interceptor handle 4xx/5xx.

---

## Step 5 — Update Store

Edit `client/src/modules/<module>/store/use<Module>Store.ts`.

### Store Template (Pinia Setup Store)
```typescript
import { ref } from 'vue'
import { defineStore } from 'pinia'
import { fetchItems, fetchItem, createItem, updateItem, deleteItem } from '../api/<module>.api'
import type { MyDto, CreateMyDto } from '../api/<module>.types'

export const useMyModuleStore = defineStore('module.<module>', () => {
  const items = ref<MyDto[]>([])
  const currentItem = ref<MyDto | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalCount = ref(0)
  const currentPage = ref(1)

  async function loadItems(page = 1): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const result = await fetchItems(page)
      items.value = result.items
      totalCount.value = result.totalCount
      currentPage.value = page
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载失败'
    } finally {
      loading.value = false
    }
  }

  async function loadItem(id: string): Promise<void> {
    loading.value = true
    error.value = null
    try {
      currentItem.value = await fetchItem(id)
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载详情失败'
    } finally {
      loading.value = false
    }
  }

  async function submitCreate(payload: CreateMyDto): Promise<MyDto> {
    const created = await createItem(payload)
    items.value.unshift(created)
    return created
  }

  async function submitUpdate(id: string, payload: Partial<CreateMyDto>): Promise<void> {
    const updated = await updateItem(id, payload)
    const idx = items.value.findIndex(i => i.id === id)
    if (idx !== -1) items.value[idx] = updated
    if (currentItem.value?.id === id) currentItem.value = updated
  }

  async function submitDelete(id: string): Promise<void> {
    await deleteItem(id)
    items.value = items.value.filter(i => i.id !== id)
    if (currentItem.value?.id === id) currentItem.value = null
  }

  return {
    items, currentItem, loading, error, totalCount, currentPage,
    loadItems, loadItem, submitCreate, submitUpdate, submitDelete,
  }
})
```

### Store Update Rules
- Only expose actions that match real backend capabilities (from contract map).
- Keep store names consistent: `defineStore('module.<module-kebab>', ...)`.
- If a store action already exists but calls the mock directly (not via API), refactor to call the API function instead.
- Do not duplicate loading/error state across multiple stores — each store owns its slice.

---

## Step 6 — Wire Auth

Check `client/src/lib/request.ts` (or the axios instance file).

### Required Axios Interceptor
```typescript
import axios from 'axios'

export const request = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '',
  timeout: 30_000,
})

// Attach JWT on every request
request.interceptors.request.use((config) => {
  const token = localStorage.getItem('auth_token')   // or from Pinia auth store
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Handle 401 globally
request.interceptors.response.use(
  (res) => res,
  (err) => {
    if (err.response?.status === 401) {
      localStorage.removeItem('auth_token')
      window.location.href = '/login'   // or use router.push if available
    }
    return Promise.reject(err)
  },
)
```

If the interceptor already exists, verify the token key and redirect path match the project conventions. Do not add a second interceptor.

### Auth Module (if not yet built)
Create `client/src/modules/auth/` with:
- `auth.types.ts` — `LoginRequest`, `RegisterRequest`, `AuthResponse`
- `auth.api.ts` — `login(payload)`, `register(payload)`, `getMe()`
- `store/useAuthStore.ts` — `token`, `user`, `login()`, `logout()`, `fetchMe()`
- `routes.ts` — `/login` route with `meta.requiresAuth: false`
- `index.ts`

Store the JWT in `localStorage` under key `auth_token`. Set it on login, clear it on logout.

---

## Step 7 — Disable Mock Branch (Optional)

By default, keep the `if (USE_MOCK)` branch in every API function.  
Only disable mock mode globally if explicitly instructed.

To disable for a single module during testing, temporarily set `USE_MOCK = false` in `client/src/config/runtime.ts` and restore after verification.

Do not remove mock branches permanently unless all modules are fully wired and tested against a running backend.

---

## Step 8 — Smoke Verify

Run these checks in order. Stop on first failure and fix before proceeding.

### Check 1: TypeScript Compile
```bash
cd client && npx tsc --noEmit
```
Expected: zero errors.

### Check 2: Vite Build
```bash
cd client && npm run build
```
Expected: build completes without errors.

### Check 3: Unit Tests (if exist for this module)
```bash
cd client && npm run test:unit -- --reporter=verbose <module>
```
Expected: all tests pass.

### Check 4: Runtime Smoke (manual, USE_MOCK=false)
1. Start backend: `cd server && dotnet run --project src/AutoCodeForge.Api`
2. Start frontend: `cd client && npm run dev`
3. Navigate to the module's main route.
4. Confirm: data loads, no 401/404/500 errors in the browser console.

---

## Step 9 — Update Contract Map

After successful verification, update `.trae/skills/fe-be-contract-map/SKILL.md`:
- Change the module's status from 🔒/❌/⚠️ to ✅.
- Update the function table to reflect the correct path used.
- Note any deviations (e.g., backend returned different field names — document the fix).

---

## Step 10 — Pick Next Module

Return to Step 1 and select the next module from the priority ranking.  
Continue until all modules show ✅ in the contract map.

---

## Module-Specific Notes

### Auth Module
- Must be done first. All other modules depend on JWT being attached.
- Backend issues `accessToken` in `AuthResponse`. Store as `auth_token` in localStorage.
- Add a Vue Router navigation guard in `client/src/router/index.ts`:
  ```typescript
  router.beforeEach((to) => {
    const token = localStorage.getItem('auth_token')
    if (to.meta.requiresAuth && !token) return '/login'
  })
  ```

### Console / Chat Module
- Sessions and messages are two separate resource groups.
- Message sending returns immediately (non-streaming). If streaming is needed, that is handled by `ChatStreamEndpoints.cs` — treat as a separate task.
- Create two sub-sections in `chat.api.ts`: session operations and message operations.

### Repo Management Module
- `fetchBranches` and `fetchPullRequests` require a `repositoryId` parameter. The current frontend mock ignores this. Add the param to the function signature and update the store to pass `currentRepo.id`.

### Scheduled Task Module
- The template feature (`TaskTemplateDto`) has no backend endpoint — it is client-side only. Keep template list in a local constant file. Do not create a backend endpoint for it.

### Pipeline Center Module
- `fetchBuilds` needs `pipelineId` to call `/api/v1/pipelines/{id}/builds`. Update function signature from `fetchBuilds()` to `fetchBuilds(pipelineId: string)`.

---

## Error Cases and Recovery

| Error | Likely Cause | Fix |
|-------|-------------|-----|
| `401 Unauthorized` | Token missing or expired | Check auth interceptor; re-login flow |
| `404 Not Found` | Path mismatch | Cross-check with contract map Step 2 |
| TypeScript type error on `data` | DTO mismatch | Re-read backend endpoint and align types |
| `CORS` error in browser | Backend not configured for frontend origin | Add `AllowedOrigins` in `appsettings.Development.json` |
| `net::ERR_CONNECTION_REFUSED` | Backend not running | Start backend first |
| `USE_MOCK` still true | Runtime config not read | Check `client/src/config/runtime.ts` value |

---

## Completion Criteria

A module integration is complete when:
1. All functions in the contract map table show ✅.
2. TypeScript compiles without errors.
3. Vite build succeeds.
4. No runtime 4xx/5xx errors when USE_MOCK is false and backend is running.
5. Contract map is updated for this module.
