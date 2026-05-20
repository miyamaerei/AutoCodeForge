---
name: fe-be-contract-map
description: 'Reference skill: full contract map between Vue 3 frontend modules and .NET backend endpoints. Use for: gap analysis, path alignment audit, integration planning, deciding which module to wire next.'
argument-hint: 'Specify scope filter if needed (e.g. "task-center only" or "all modules"). Defaults to full map.'
---

# Frontend ↔ Backend Contract Map

## When to Use
- You need to know which frontend modules are mock-only and need real API wiring.
- You need to audit path mismatches between frontend API calls and backend routes.
- You are planning a new integration round and need to pick the next module.
- You want a single authoritative reference before touching any API file.

## Key Facts About This Project
- Frontend: Vue 3 + TypeScript + Axios + Pinia. Lives in `client/src/modules/<module>/`.
- Backend: .NET 9 Minimal API. Base prefix `/api/v1/`. Lives in `server/src/AutoCodeForge.Api/Endpoints/`.
- Mock switch: `client/src/config/runtime.ts` exports `USE_MOCK`. When `true`, all API files return local mock data and never call the backend.
- All real HTTP calls go through the shared axios instance in `client/src/lib/request.ts` (or equivalent).
- Auth: JWT Bearer. Backend supports passwordless login and issues tokens at `POST /api/v1/auth/windows-login` (preferred) and `POST /api/v1/auth/login` (ntId-only fallback). Frontend must attach `Authorization: Bearer <token>` header for protected routes.

---

## Contract Map — All Modules

### Status Legend
| Symbol | Meaning |
|--------|---------|
| ✅ | Frontend API file exists and makes real HTTP calls (path may still need alignment) |
| ⚠️ | Frontend API file exists but path does not match backend canonical route |
| ❌ | No frontend API file — backend is complete but frontend has no wiring at all |
| 🔒 | Mock-only — API file exists but always returns mock data, no HTTP branch |

---

### Module: Auth
| Frontend Route | Backend Canonical | Status | Notes |
|---------------|-------------------|--------|-------|
| — | `POST /api/v1/auth/windows-login` | ❌ | Preferred login path. Backend resolves Windows identity (or `X-Windows-User` / `X-NtId`) and auto-provisions user when missing |
| — | `POST /api/v1/auth/login` | ❌ | Passwordless fallback path (ntId-based), no password required |
| — | `POST /api/v1/auth/register` | ❌ | Optional manual bootstrap path, no password required |
| — | `GET /api/v1/auth/me` | ❌ | No current-user fetch wired |

**Gap**: No auth module exists in frontend. JWT token handling (store, attach to requests, redirect on 401) must be built. Login UI should call `windows-login` first, then use `login` only as fallback when Windows identity is unavailable.

---

### Module: Task Center (`/task-center`)
Frontend module path: `client/src/modules/task-center/`

| Function | Frontend Path Used | Backend Canonical | Status |
|----------|--------------------|-------------------|--------|
| `fetchTaskSummaries()` | `GET /task-center/tasks` | `GET /api/v1/tasks` | ⚠️ Path mismatch |
| `fetchTaskDetail(id)` | `GET /task-center/tasks/{id}` | `GET /api/v1/tasks/{id}` | ⚠️ Path mismatch |
| `createTask(payload)` | `POST /task-center/tasks` | `POST /api/v1/tasks` | ⚠️ Path mismatch |
| `fetchTaskLogs(id)` | — | `GET /api/v1/tasks/{id}/logs` | ❌ Not wired |
| `updateTask(id)` | — | `PUT /api/v1/tasks/{id}` | ❌ Not wired |
| `pauseTask(id)` | — | `POST /api/v1/tasks/{id}/pause` | ❌ Not wired |
| `resumeTask(id)` | — | `POST /api/v1/tasks/{id}/resume` | ❌ Not wired |
| `deleteTask(id)` | — | `DELETE /api/v1/tasks/{id}` | ❌ Not wired |

**Priority**: HIGH — Core feature. Fix paths, add missing functions.

---

### Module: Agent Center (`/agent`)
Frontend module path: `client/src/modules/agent-center/`

| Function | Frontend Path Used | Backend Canonical | Status |
|----------|--------------------|-------------------|--------|
| `fetchAgents()` | mock only | `GET /api/v1/agents` | 🔒 Mock-only |
| `fetchAgent(id)` | mock only | `GET /api/v1/agents/{id}` | 🔒 Mock-only |
| `matchAgent(input)` | mock only | `GET /api/v1/agents/match?input=` | 🔒 Mock-only |
| `createAgent(payload)` | mock only | `POST /api/v1/agents` | 🔒 Mock-only |
| `updateAgent(id, payload)` | mock only | `PUT /api/v1/agents/{id}` | 🔒 Mock-only |
| `deleteAgent(id)` | mock only | `DELETE /api/v1/agents/{id}` | 🔒 Mock-only |

**Priority**: HIGH — Agent selection used by Scheduled Task and Chat.

---

### Module: Console / Chat (`/session`, `/ask`)
Frontend module path: `client/src/modules/console/`

| Function | Frontend Path Used | Backend Canonical | Status |
|----------|--------------------|-------------------|--------|
| `fetchSessions()` | — | `GET /api/v1/chat/sessions` | ❌ Not wired |
| `createSession()` | — | `POST /api/v1/chat/sessions` | ❌ Not wired |
| `getSession(id)` | — | `GET /api/v1/chat/sessions/{id}` | ❌ Not wired |
| `getMessages(id)` | — | `GET /api/v1/chat/sessions/{id}/messages` | ❌ Not wired |
| `sendMessage(id, msg)` | — | `POST /api/v1/chat/sessions/{id}/messages` | ❌ Not wired |
| `deleteSession(id)` | — | `DELETE /api/v1/chat/sessions/{id}` | ❌ Not wired |

**Priority**: HIGH — Primary user-facing feature.

---

### Module: Repo Management (`/repo-management`)
Frontend module path: `client/src/modules/repo-management/`

| Function | Frontend Path Used | Backend Canonical | Status |
|----------|--------------------|-------------------|--------|
| `fetchRepositories()` | `GET /repo-management/repositories` | `GET /api/v1/repositories` | ⚠️ Path mismatch |
| `fetchBranches()` | `GET /repo-management/branches` | `GET /api/v1/repositories/{id}/branches` | ⚠️ Path mismatch + missing `id` param |
| `fetchPullRequests()` | `GET /repo-management/pull-requests` | `GET /api/v1/repositories/{id}/pull-requests` | ⚠️ Path mismatch + missing `id` param |
| `fetchCommits(id, branch)` | — | `GET /api/v1/repositories/{id}/commits` | ❌ Not wired |
| `createRepository(payload)` | — | `POST /api/v1/repositories` | ❌ Not wired |
| `createPullRequest(id, payload)` | — | `POST /api/v1/repositories/{id}/pull-requests` | ❌ Not wired |
| `updateRepository(id, payload)` | — | `PUT /api/v1/repositories/{id}` | ❌ Not wired |
| `deleteRepository(id)` | — | `DELETE /api/v1/repositories/{id}` | ❌ Not wired |

**Priority**: MEDIUM

---

### Module: Pipeline Center (`/pipeline-center`)
Frontend module path: `client/src/modules/pipeline-center/`

| Function | Frontend Path Used | Backend Canonical | Status |
|----------|--------------------|-------------------|--------|
| `fetchPipelines()` | `GET /pipeline-center/pipelines` | `GET /api/v1/pipelines` | ⚠️ Path mismatch |
| `fetchBuilds()` | `GET /pipeline-center/builds` | `GET /api/v1/pipelines/{id}/builds` | ⚠️ Path mismatch + missing `id` param |
| `fetchPipeline(id)` | — | `GET /api/v1/pipelines/{id}` | ❌ Not wired |
| `createPipeline(payload)` | — | `POST /api/v1/pipelines` | ❌ Not wired |
| `triggerPipeline(id)` | — | `POST /api/v1/pipelines/{id}/trigger` | ❌ Not wired |
| `updatePipeline(id)` | — | `PUT /api/v1/pipelines/{id}` | ❌ Not wired |
| `deletePipeline(id)` | — | `DELETE /api/v1/pipelines/{id}` | ❌ Not wired |

**Priority**: MEDIUM

---

### Module: Scheduled Task (`/scheduled-task`)
Frontend module path: `client/src/modules/scheduled-task/`

| Function | Frontend Path Used | Backend Canonical | Status |
|----------|--------------------|-------------------|--------|
| `fetchScheduledTasks()` | mock only | `GET /api/v1/scheduled-tasks` | 🔒 Mock-only |
| `fetchScheduledTask(id)` | mock only | `GET /api/v1/scheduled-tasks/{id}` | 🔒 Mock-only |
| `fetchExecutions(id)` | mock only | `GET /api/v1/scheduled-tasks/{id}/executions` | 🔒 Mock-only |
| `createScheduledTask(payload)` | mock only | `POST /api/v1/scheduled-tasks` | 🔒 Mock-only |
| `updateScheduledTask(id)` | mock only | `PUT /api/v1/scheduled-tasks/{id}` | 🔒 Mock-only |
| `pauseScheduledTask(id)` | mock only | `POST /api/v1/scheduled-tasks/{id}/pause` | 🔒 Mock-only |
| `resumeScheduledTask(id)` | mock only | `POST /api/v1/scheduled-tasks/{id}/resume` | 🔒 Mock-only |
| `deleteScheduledTask(id)` | mock only | `DELETE /api/v1/scheduled-tasks/{id}` | 🔒 Mock-only |

**Priority**: MEDIUM

---

### Module: Wiki (`/wiki`)
Frontend module path: `client/src/modules/console/` (wiki tab) or dedicated module

| Function | Frontend Path Used | Backend Canonical | Status |
|----------|--------------------|-------------------|--------|
| `fetchWikiPages()` | — | `GET /api/v1/wiki` | ❌ Not wired |
| `fetchWikiPage(id)` | — | `GET /api/v1/wiki/{id}` | ❌ Not wired |
| `createWikiPage(payload)` | — | `POST /api/v1/wiki` | ❌ Not wired |
| `updateWikiPage(id)` | — | `PUT /api/v1/wiki/{id}` | ❌ Not wired |
| `deleteWikiPage(id)` | — | `DELETE /api/v1/wiki/{id}` | ❌ Not wired |

**Priority**: MEDIUM

---

### Module: Review (`/review`)
Frontend module path: `client/src/modules/console/` (review tab) or dedicated module

| Function | Frontend Path Used | Backend Canonical | Status |
|----------|--------------------|-------------------|--------|
| — | — | `GET /api/v1/reviews/rule-sets` | ❌ Not wired |
| — | — | `POST /api/v1/reviews/rule-sets` | ❌ Not wired |
| — | — | `PUT /api/v1/reviews/rule-sets/{id}` | ❌ Not wired |
| — | — | `DELETE /api/v1/reviews/rule-sets/{id}` | ❌ Not wired |
| — | — | `POST /api/v1/reviews/tasks` | ❌ Not wired |
| — | — | `GET /api/v1/reviews/tasks/{id}` | ❌ Not wired |
| — | — | `GET /api/v1/reviews/tasks/{id}/findings` | ❌ Not wired |
| — | — | `GET /api/v1/reviews/repositories/{id}/tasks` | ❌ Not wired |

**Priority**: LOW (after core modules)

---

### Module: System Config / Settings (`/settings`, `/system-config`)
Frontend module path: `client/src/modules/system-config/`

| Function | Frontend Path Used | Backend Canonical | Status |
|----------|--------------------|-------------------|--------|
| — | — | `GET /api/v1/configs/global` | ❌ Not wired (Admin only) |
| — | — | `POST/PUT /api/v1/configs/global/{key}` | ❌ Not wired |
| — | — | `GET /api/v1/configs/user` | ❌ Not wired |
| — | — | `POST/PUT /api/v1/configs/user/{key}` | ❌ Not wired |
| — | — | `GET /api/v1/configs/user/sandbox` | ❌ Not wired |
| — | — | `PUT /api/v1/configs/user/sandbox` | ❌ Not wired |

**Priority**: LOW

---

## Integration Priority Ranking

| Rank | Module | Reason |
|------|--------|--------|
| 1 | **Auth** | Blocks everything — Windows/passwordless JWT bootstrap must work before any protected call |
| 2 | **Task Center** | Core feature; paths just need fixing |
| 3 | **Console / Chat** | Primary UX; fully missing API wiring |
| 4 | **Agent Center** | Drives auto-selection in Chat and Scheduled Tasks |
| 5 | **Repo Management** | Needed for Task and Review flows |
| 6 | **Scheduled Task** | Depends on Agent wiring |
| 7 | **Pipeline Center** | Depends on Repo wiring |
| 8 | **Wiki** | Stand-alone; lower urgency |
| 9 | **Review** | Depends on Repo and Rule Sets |
| 10 | **System Config** | Admin surface; lowest urgency |

---

## Common Path Fix Pattern

All existing frontend API files use wrong base prefixes. The fix is always the same:

```typescript
// Before (wrong):
const { data } = await request.get('/task-center/tasks')

// After (correct):
const { data } = await request.get('/api/v1/tasks')
```

If a reverse-proxy rewrites `/api/v1` to `/`, document that in `client/src/config/runtime.ts` and align accordingly. Do not guess — check `vite.config.ts` proxy settings first.

---

## Auth Bootstrap Pattern (Windows + Passwordless)

Use this sequence when wiring frontend auth:

```typescript
// 1) Preferred: Windows identity bootstrap
const loginRes = await request.post('/api/v1/auth/windows-login', {})

// 2) Fallback: explicit ntId login without password
// const loginRes = await request.post('/api/v1/auth/login', { ntId })

const token = loginRes.data?.data?.accessToken
if (token) {
	localStorage.setItem('acf.accessToken', token)
	request.defaults.headers.common.Authorization = `Bearer ${token}`
}
```

Notes:
- `windows-login` is anonymous and designed for automatic sign-in.
- Backend auto-creates user records on first login when user does not exist.
- No password field should appear in frontend auth DTOs or forms.

---

## Frontend File Structure per Module (Required)

```
client/src/modules/<module>/
├── api/
│   ├── <module>.api.ts      ← HTTP functions (one per operation)
│   └── <module>.types.ts    ← Request/response DTOs (match backend)
├── models/
│   ├── <module>.model.ts    ← Domain model (UI-facing shape)
│   └── <module>.mapper.ts   ← DTO → Model converter
├── store/
│   └── use<Module>Store.ts  ← Pinia setup store
├── routes.ts
├── views/
│   └── *.vue
└── index.ts                 ← Re-exports api, store, routes
```

---

## DTO Alignment Rules

1. Frontend DTO property names must match backend JSON field names exactly (camelCase).
2. Use `string` for IDs (backend uses `Guid` serialized as string).
3. Use `string` for dates (ISO 8601 format from backend).
4. Paged responses follow shape: `{ items: T[], totalCount: number, page: number, pageSize: number }`.
5. Error responses follow shape: `{ message: string, errors?: Record<string, string[]> }`.
