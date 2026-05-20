# AutoCodeForge API

## Base Information

- Base path: `/api/v1`
- Auth mode: JWT Bearer token
- Response envelope: `ApiResponse<T>` for successful and business-error responses
- Interactive docs: `/swagger` in development

## Core Endpoints

### Auth

- `POST /api/v1/auth/register`: register a user and return access token
- `POST /api/v1/auth/login`: login with `ntId` and password
- `GET /api/v1/auth/me`: get current user profile

### Agents

- `GET /api/v1/agents`: list agents with paging
- `POST /api/v1/agents`: create an agent
- `GET /api/v1/agents/{id}`: get agent detail
- `PUT /api/v1/agents/{id}`: update an agent
- `DELETE /api/v1/agents/{id}`: soft-delete an agent
- `GET /api/v1/agents/match?input=...`: match an agent by input text

### Chat

- `POST /api/v1/chat/sessions`: create a session
- `GET /api/v1/chat/sessions`: list sessions
- `GET /api/v1/chat/sessions/{id}`: get session detail
- `GET /api/v1/chat/sessions/{id}/messages`: list session messages
- `POST /api/v1/chat/sessions/{id}/messages`: send a message and persist assistant reply
- `POST /api/v1/chat/sessions/{id}/stream`: stream assistant reply via Server-Sent Events
- `DELETE /api/v1/chat/sessions/{id}`: delete a session

### Tasks and Scheduling

- `GET /api/v1/tasks`
- `POST /api/v1/tasks`
- `GET /api/v1/tasks/{id}`
- `GET /api/v1/tasks/{id}/logs`
- `POST /api/v1/scheduled-tasks`
- `GET /api/v1/scheduled-tasks`

### Repository and Pipeline

- `GET /api/v1/repositories`
- `POST /api/v1/repositories`
- `GET /api/v1/repositories/{id}/branches`
- `GET /api/v1/repositories/{id}/commits`
- `POST /api/v1/repositories/{id}/pull-requests`
- `GET /api/v1/pipelines`
- `POST /api/v1/pipelines`

### Config, Health, Wiki, Admin

- `GET /health`
- `GET /health/live`
- `GET /health/ready`
- `GET /system/info`
- `GET /system/environment`
- `GET /api/v1/configs/global`
- `GET /api/v1/configs/user`
- `GET /api/v1/wiki`
- `GET /api/v1/admin/audit-logs`

## Authentication Example

```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "ntId": "demo.user",
  "userName": "Demo User",
  "email": "demo@example.com",
  "password": "Demo@123456"
}
```

Use the returned `accessToken` in the header:

```http
Authorization: Bearer <token>
```

## Streaming Example

`POST /api/v1/chat/sessions/{id}/stream` returns `text/event-stream` and emits:

- `event: start`
- `event: token`
- `event: error` when execution fails but stream remains well-formed
- `event: done` with final payload

## Validation and Error Handling

- Request DTOs use DataAnnotations validation.
- Unhandled exceptions are wrapped by `ExceptionHandlingMiddleware`.
- Auth failures return `401 Unauthorized`.
- Business validation issues return `400`-style API errors through the shared envelope.