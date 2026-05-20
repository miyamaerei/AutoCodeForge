---
name: fe-be-contract-map
description: "Reference skill for Vue 3 frontend and .NET backend contract map. Invoke for gap analysis, path alignment audit, or integration planning."
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
| `/login` (auth.login) | `POST /api/v1/auth/windows-login` | ✅ | Implemented as `login()` primary branch; frontend calls `/v1/auth/windows-login` with axios `baseURL=/api` |
| `/login` (fallback action) | `POST /api/v1/auth/login` | ✅ | Implemented fallback in `login(payload)` when `ntId` is provided |
| `/login` (register path) | `POST /api/v1/auth/register` | ✅ | Implemented `register(payload)` |
| app bootstrap / guarded routes | `GET /api/v1/auth/me` | ✅ | Implemented `getMe()` and Pinia auth store `fetchMe()` |

**Status**: Auth module exists in frontend (`api/types/store/routes/index` + login view). JWT token is stored as `auth_token`, attached in request interceptor, and cleared with redirect to `/login` on 401.

---

### Module: Task Center (`/task-center`)
Frontend module path: `client/src/modules/task-center/`

| Function | Frontend Path Used | Backend Canonical | Status |
|----------|--------------------|-------------------|--------|
| `fetchTaskSummaries(page, pageSize)` | `GET /api/v1/tasks?page=&pageSize=` | `GET /api/v1/tasks` | ✅ |
| `fetchTaskDetail(id)` | `GET /api/v1/tasks/{id}` | `GET /api/v1/tasks/{id}` | ✅ |
| `createTask(payload)` | `POST /api/v1/tasks` | `POST /api/v1/tasks` | ✅ |
| `fetchTaskLogs(id)` | `GET /api/v1/tasks/{id}/logs` | `GET /api/v1/tasks/{id}/logs` | ✅ |
| `updateTask(id, payload)` | `PUT /api/v1/tasks/{id}` | `PUT /api/v1/tasks/{id}` | ✅ |
| `pauseTask(id)` | `POST /api/v1/tasks/{id}/pause` | `POST /api/v1/tasks/{id}/pause` | ✅ |
| `resumeTask(id)` | `POST /api/v1/tasks/{id}/resume` | `POST /api/v1/tasks/{id}/resume` | ✅ |
| `deleteTask(id)` | `DELETE /api/v1/tasks/{id}` | `DELETE /api/v1/tasks/{id}` | ✅ |

**Status**: Task Center API 已完成路径对齐与能力补齐。Store 已改为通过 API 加载任务详情与日志；聊天与 diff 暂保留 mock。

---

### Module: Agent Center (`/agent`)
Frontend module path: `client/src/modules/agent-center/`

| Function | Frontend Path Used | Backend Canonical | Status |
|----------|--------------------|-------------------|--------|
| `fetchAgents(page, pageSize)` | `GET /api/v1/agents?page=&pageSize=` | `GET /api/v1/agents` | ✅ |
| `getAgents()` | `GET /api/v1/agents` | `GET /api/v1/agents` | ✅ |
| `getAgent(id)` | `GET /api/v1/agents/{id}` | `GET /api/v1/agents/{id}` | ✅ |
| `selectAgentByMessage(input)` | `GET /api/v1/agents/match?input=` | `GET /api/v1/agents/match?input=` | ✅ |
| `createAgent(payload)` | `POST /api/v1/agents` | `POST /api/v1/agents` | ✅ |
| `updateAgent(id, payload)` | `PUT /api/v1/agents/{id}` | `PUT /api/v1/agents/{id}` | ✅ |
| `deleteAgent(id)` | `DELETE /api/v1/agents/{id}` | `DELETE /api/v1/agents/{id}` | ✅ |

**Status**: ✅ Agent Center API 已完成集成。所有函数支持真实 API 调用和 Mock 切换（USE_MOCK=true 时使用 mock）。Store (`useAgentStore`) 已创建，Composable (`useAgent`) 已更新使用 Store。注意：后端 `AgentResponse` 不包含 `icon` 字段，前端自动适配（默认 🤖）；关键词格式为逗号分隔字符串而非数组。

**Priority**: HIGH — Agent selection used by Scheduled Task and Chat.

---

### Module: Console / Chat (`/session`, `/ask`)
Frontend module path: `client/src/modules/console/`

| Function | Frontend Path Used | Backend Canonical | Status |
|----------|--------------------|-------------------|--------|
| `fetchSessions(page, pageSize)` | `GET /api/v1/chat/sessions?page=&pageSize=` | `GET /api/v1/chat/sessions` | ✅ |
| `createSession(payload)` | `POST /api/v1/chat/sessions` | `POST /api/v1/chat/sessions` | ✅ |
| `getMessages(id)` | `GET /api/v1/chat/sessions/{id}/messages` | `GET /api/v1/chat/sessions/{id}/messages` | ✅ |
| `sendMessage(id, msg)` | `POST /api/v1/chat/sessions/{id}/messages` | `POST /api/v1/chat/sessions/{id}/messages` | ✅ |
| `deleteSession(id)` | `DELETE /api/v1/chat/sessions/{id}` | `DELETE /api/v1/chat/sessions/{id}` | ✅ |

**Status**: ✅ Console/Chat API 已完成集成。所有函数支持真实 API 调用和 Mock 切换（USE_MOCK=true 时使用 mock）。Store (`useChatStore`) 已创建，Composable (`useConsoleChat`) 已更新使用 Store。注意：Ask 模式暂时保留 Mock（后端暂无对应接口），Session 模式已全量接入真实 API。

**Priority**: HIGH — Primary user-facing feature.

---

### Module: Repo Management (`/repo-management`)
Frontend module path: `client/src/modules/repo-management/`

| Function | Frontend Path Used | Backend Canonical | Status |
|----------|--------------------|-------------------|--------|
| `fetchRepositories(page, pageSize)` | `GET /api/v1/repositories?page=&pageSize=` | `GET /api/v1/repositories` | ✅ |
| `fetchRepository(id)` | `GET /api/v1/repositories/{id}` | `GET /api/v1/repositories/{id}` | ✅ |
| `fetchBranches(repositoryId)` | `GET /api/v1/repositories/{id}/branches` | `GET /api/v1/repositories/{id}/branches` | ✅ |
| `fetchCommits(repositoryId, branch, limit)` | `GET /api/v1/repositories/{id}/commits` | `GET /api/v1/repositories/{id}/commits` | ✅ |
| `fetchPullRequests(repositoryId, state, limit)` | `GET /api/v1/repositories/{id}/pull-requests` | `GET /api/v1/repositories/{id}/pull-requests` | ✅ |
| `createRepository(payload)` | `POST /api/v1/repositories` | `POST /api/v1/repositories` | ✅ |
| `createPullRequest(repositoryId, payload)` | `POST /api/v1/repositories/{id}/pull-requests` | `POST /api/v1/repositories/{id}/pull-requests` | ✅ |
| `updateRepository(id, payload)` | `PUT /api/v1/repositories/{id}` | `PUT /api/v1/repositories/{id}` | ✅ |
| `deleteRepository(id)` | `DELETE /api/v1/repositories/{id}` | `DELETE /api/v1/repositories/{id}` | ✅ |

**Status**: ✅ Repo Management API 已完成集成。所有函数支持真实 API 调用和 Mock 切换（USE_MOCK=true 时使用 mock）。Store (`useRepoManagementStore`) 已更新使用 API 函数，并添加了 `submitCreate`, `submitUpdate`, `submitDelete` 操作。

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
| `fetchScheduledTasks(page, pageSize)` | `GET /api/v1/scheduled-tasks?page=&pageSize=` | `GET /api/v1/scheduled-tasks` | ✅ |
| `fetchScheduledTask(id)` | `GET /api/v1/scheduled-tasks/{id}` | `GET /api/v1/scheduled-tasks/{id}` | ✅ |
| `fetchExecutions(id)` | `GET /api/v1/scheduled-tasks/{id}/executions` | `GET /api/v1/scheduled-tasks/{id}/executions` | ✅ |
| `createScheduledTask(payload)` | `POST /api/v1/scheduled-tasks` | `POST /api/v1/scheduled-tasks` | ✅ |
| `updateScheduledTask(id, payload)` | `PUT /api/v1/scheduled-tasks/{id}` | `PUT /api/v1/scheduled-tasks/{id}` | ✅ |
| `triggerScheduledTask(id)` | `POST /api/v1/scheduled-tasks/{id}/trigger` | `POST /api/v1/scheduled-tasks/{id}/trigger` | ✅ |
| `deleteScheduledTask(id)` | `DELETE /api/v1/scheduled-tasks/{id}` | `DELETE /api/v1/scheduled-tasks/{id}` | ✅ |
| `fetchTemplates()` | `GET /api/v1/scheduled-tasks/templates` | `GET /api/v1/scheduled-tasks/templates` | ✅ |

**Status**: ✅ Scheduled Task API 已完成集成。所有函数支持真实 API 调用和 Mock 切换（USE_MOCK=true 时使用 mock）。Store (`useScheduledTaskStore`) 已创建，Composable (`useScheduledTask`) 已更新使用 Store。DTO 已对齐后端字段（`nextRunAtUtc`, `input` 等）。

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
