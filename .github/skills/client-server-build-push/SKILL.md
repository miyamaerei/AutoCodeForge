---
name: client-server-build-push
description: 'Build and push workflow for AutoCodeForge client and server. Use for pre-push verification, frontend build checks, .NET backend build/test checks, smoke run checks, and safe git push gating.'
argument-hint: 'Branch name and optional scope, for example: feature/task-center-ui, full-check or fast-check'
---

# Client and Server Build and Push

## When to Use
- You want a repeatable pre-push gate for this repository.
- You need to verify both frontend client and backend server before pushing.
- You want a deterministic pass or fail decision instead of pushing by habit.

## Scope and Stack
- Frontend path: `client/` (Vue 3 + Vite)
- Backend path: `server/` (.NET solution)
- Backend solution file: `server/AutoCodeForge.sln`

## Output Contract
This skill must end with one of these outcomes:
1. `PASS_READY_TO_PUSH`: all required checks passed and push is allowed.
2. `BLOCKED_DO_NOT_PUSH`: at least one required check failed.
3. `PARTIAL_WITH_RISK`: optional checks failed but required checks passed; push only with explicit risk note.

## Preflight Rules
1. Confirm working tree state:
- Run `git status --short`.
- If unexpected unrelated changes exist, stop and ask for confirmation.

2. Confirm branch target:
- Run `git branch --show-current`.
- If current branch is `main`, push is allowed, but apply the unpushed-commit threshold gate.

3. Main branch unpushed-commit threshold gate:
- Run `git rev-list --count @{u}..HEAD`.
- If count is `0-5`, direct push to `main` is allowed.
- If count is `>5`, do not push directly to `main`.
- For `>5`, create a new branch and open a PR:
  - `git switch -c feature/<topic-or-date>`
  - `git push -u origin feature/<topic-or-date>`
  - create PR from `feature/<topic-or-date>` to `main`.

4. Sync baseline:
- Run `git fetch origin`.
- Run `git pull --rebase origin <current-branch>` when branch is tracked.

## Verification Modes
1. `fast-check`
- Frontend type check + build.
- Backend restore + build.
- No tests.
- Use only for local iteration.

2. `full-check` (default)
- Frontend type check + unit tests + build.
- Frontend dev server page smoke check (required).
- Backend restore + build + tests.
- Backend API smoke run check.

## Step-by-Step Procedure
1. Frontend dependency and checks
- `cd client`
- `npm ci`
- `npm run type-check`
- For `full-check`: `npm run test:unit -- --run`
- `npm run build`

Pass criteria:
- type check exits 0
- test command exits 0 when required
- build exits 0

2. Frontend dev-server smoke check (`full-check`, required)
- `cd client`
- Start dev server: `npm run dev`
- Wait for startup log and local URL.
- Open the local page and verify it loads without a blank screen or fatal runtime error.
- Stop process cleanly.

Pass criteria:
- dev server starts successfully
- main page is reachable and renders without fatal runtime error

3. Backend dependency and checks
- `cd server`
- `dotnet restore AutoCodeForge.sln`
- `dotnet build AutoCodeForge.sln -c Release --nologo`
- For `full-check`: `dotnet test AutoCodeForge.sln --nologo`

Pass criteria:
- restore exits 0
- build exits 0
- test exits 0 when required

4. Backend smoke run check (`full-check`)
- `cd server`
- Start API: `dotnet run --project src/AutoCodeForge.Api --no-build`
- Wait for startup log that includes listening URL.
- Verify Swagger endpoint returns HTTP 200 on `https://localhost:<port>/swagger`.
- Stop process cleanly.

Pass criteria:
- API starts without unhandled exception
- Swagger endpoint is reachable

5. Git gate before push
- Return to repository root.
- Run `git status --short` and verify only intended files are changed.
- Commit with clear message:
  - `git add -A`
  - `git commit -m "chore: pass client/server build gate before push"`
- Push path decision:
  - If on `main` and unpushed commit count is `0-5`: `git push origin main`
  - If on `main` and unpushed commit count is `>5`: create feature branch and push branch, then open PR
  - If on non-main branch: `git push -u origin <current-branch>`

## Decision Points
1. If frontend check fails
- Mark `BLOCKED_DO_NOT_PUSH`.
- Report failing command and first actionable error.
- Do not continue to push.

2. If backend check fails
- Mark `BLOCKED_DO_NOT_PUSH`.
- Report failing command and first actionable error.
- Do not continue to push.

3. If smoke check fails but required build and test pass
- Mark `PARTIAL_WITH_RISK`.
- Require explicit approval before push.

4. If branch is `main` and unpushed commit count is `>5`
- Mark `BLOCKED_DO_NOT_PUSH` for direct push path.
- Switch to branch-plus-PR path.

5. If all required checks pass and branch policy is satisfied
- Mark `PASS_READY_TO_PUSH` and proceed to push.

## Completion Checklist
- Frontend required checks passed for selected mode.
- Frontend dev-server smoke check passed for `full-check`.
- Backend required checks passed for selected mode.
- Smoke check passed for `full-check`.
- Git branch and change scope validated.
- Main branch unpushed-commit threshold validated.
- Push completed successfully with remote tracking.
- Final report includes:
  - mode
  - commands executed
  - pass/fail per stage
  - final status enum

## Example Prompts
- `/client-server-build-push feature/task-center-ui full-check`
- `/client-server-build-push fast-check for quick local validation`
- `/client-server-build-push run full-check, block push on any required failure`
