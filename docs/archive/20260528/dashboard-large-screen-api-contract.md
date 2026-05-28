# Dashboard Large Screen API Contract

## Scope
This document defines the field-level contract for the large-screen simulation dashboard.

- Polling cadence:
  - Core data (`snapshot`, `tasks`, `agents`): every 2 seconds.
  - Logs (`logs`): every 1 second.
- Frontend owns animation and rendering behavior.
- Backend returns state only.

## Base Path
- Stable path: `/api/dashboard`
- Legacy compatibility path: `/api/v1/dashboard`

## 1) GET /api/dashboard/snapshot
Returns aggregate system statistics for top-level KPI cards.

### Response
```json
{
  "success": true,
  "message": "OK",
  "data": {
    "agentStats": {
      "total": 0,
      "idle": 0,
      "handling": 0,
      "learning": 0,
      "dormant": 0
    },
    "taskStats": {
      "total": 0,
      "pending": 0,
      "running": 0,
      "completed": 0,
      "failed": 0,
      "paused": 0,
      "canceled": 0
    },
    "gateStats": {
      "pendingCount": 0,
      "byType": {
        "RequirementConfirm": 0,
        "PlanApproval": 0,
        "CodeReview": 0,
        "TestAcceptance": 0,
        "MergeApproval": 0,
        "FinalSignoff": 0,
        "Emergency": 0
      }
    },
    "lastUpdated": "2026-05-25T08:00:00Z"
  }
}
```

### Fields
- `agentStats.total`: number of agents.
- `agentStats.idle|handling|learning|dormant`: count by agent runtime state.
- `taskStats.*`: count by task status.
- `gateStats.pendingCount`: number of pending gates.
- `gateStats.byType`: pending gates grouped by gate type.
- `lastUpdated`: server UTC timestamp.

## 2) GET /api/dashboard/tasks
Returns live task data for track animation and alert marking.

### Response item (`DashboardTaskLive`)
```json
{
  "id": "6ef6f4ac-8a74-4f89-a3d8-2a3184db3aa8",
  "title": "Implement dashboard simulation",
  "status": "Running",
  "progress": 62,
  "currentStep": 4,
  "currentStepName": "代码开发",
  "agentId": "31ca4f85-34a4-4a13-8726-85f48956ff16",
  "agentName": "Worker-01",
  "errorMessage": null,
  "isTimeout": false,
  "hasRejectedGate": false,
  "hasEmergencyGate": true,
  "alertTags": ["emergency"],
  "alertLevel": "warning",
  "updatedAtUtc": "2026-05-25T08:00:00Z"
}
```

### Fields
- `currentStep`: normalized to 1..7.
- `currentStepName`: display name for step.
- `isTimeout`: `true` when `dueAtUtc` expired and task is still pending/running.
- `hasRejectedGate`: `true` when any related gate has `Rejected` status.
- `hasEmergencyGate`: `true` when any related gate type is `Emergency` and not cancelled.
- `alertTags`: machine-friendly alert tags, one or more of:
  - `timeout`
  - `rejected`
  - `emergency`
  - `failed`
- `alertLevel`:
  - `critical`: has `failed` or `timeout` or `rejected`.
  - `warning`: no critical tag and has `emergency`.
  - `normal`: no alert tags.

## 3) GET /api/dashboard/agents
Returns live agent data for station occupancy and workload display.

### Response item (`DashboardAgentLive`)
```json
{
  "id": "31ca4f85-34a4-4a13-8726-85f48956ff16",
  "name": "Worker-01",
  "role": "Worker",
  "state": "Handling",
  "workload": 1,
  "workstationStep": 4,
  "workstationName": "代码开发",
  "currentTaskId": "6ef6f4ac-8a74-4f89-a3d8-2a3184db3aa8",
  "dormantReason": null,
  "updatedAtUtc": "2026-05-25T08:00:00Z"
}
```

### Fields
- `role`: `Secretary | Manager | Worker`.
- `state`: `Idle | Handling | Learning | Dormant`.
- `workload`: running tasks currently assigned to this agent.
- `workstationStep`: normalized to 1..7.
- `workstationName`: display name for station.

## 4) GET /api/dashboard/logs
Returns filtered logs for the rolling log panel.

### Query params
- `type` (required): `task | agent | system`
- `taskId` (optional)
- `agentId` (optional)

### Response item (`DashboardLogItem`)
```json
{
  "time": "2026-05-25T08:00:00Z",
  "type": "system",
  "taskId": "6ef6f4ac-8a74-4f89-a3d8-2a3184db3aa8",
  "agentId": "31ca4f85-34a4-4a13-8726-85f48956ff16",
  "content": "门控事件: Emergency - Pending",
  "level": "warning"
}
```

### Field notes
- `type` indicates source domain of the event.
- `level` should be rendered with severity color:
  - `info`
  - `warning`
  - `error`

## Frontend Visual Mapping Recommendation
- Timeout (`isTimeout`): amber dashed ring.
- Rejected gate (`hasRejectedGate`): red cross overlay.
- Emergency gate (`hasEmergencyGate`): violet marker.
- Critical node (`alertLevel = critical`): red glow + pulse animation.

## Acceptance Checklist
- 2-second polling is used for `snapshot`, `tasks`, and `agents`.
- 1-second polling is used for `logs`.
- Node movement is smooth interpolation on frontend.
- Timeout/rejected/emergency have independent visual markers.
- Clicking task/agent filters logs by `taskId` or `agentId`.
