---
name: auto-developer
description: 'Run single-task development with a two-layer prompt system: Strategic Planner (planning brain) and DevOps Orchestrator (execution pipeline), including spec circuit-breaker and evolution flow.'
argument-hint: 'Describe task type, module scope, priority, acceptance criteria, component reuse constraints, and known policy risks.'
---

# Auto Developer

## When to Use
- You need one-task delivery with planning, execution, audit, and single-task reporting.
- You need a resilient workflow that can handle policy conflicts without deadlock.
- You need strict component reuse enforcement with controlled policy evolution.

## Two-Layer Prompt System
1. Strategic Planner (planning brain)
- Owns requirement decomposition, risk prediction, and plan resilience.
- Maintains docs/MasterPlan.md (the live MASTER_PLAN file for this repo).
- Shrinks or reprioritizes scope when round capacity is insufficient.
- Triggers policy-upgrade flow when implementation and policy conflict blocks delivery.

2. DevOps Orchestrator (execution layer)
- Runs the delivery pipeline using two fixed roles:
	- @Worker: implements code and must reuse approved shared components.
	- @Auditor: enforces policy gates and has veto power for non-compliant output.
- Executes only the scope approved by Strategic Planner.

## Not in Scope
- Multi-task summary or cross-round aggregation.
- Daily report generation.
- Historical file archiving or batch file cleanup.
- Component library synchronization.

## Inputs Required
1. Task type: DEV, BUG, REFACTOR, OPTIMIZE, DOCS, or OTHER.
2. Scope: target modules and files.
3. Priority: P0, P1, P2, or P3.
4. Acceptance criteria and expected behavior.
5. Reuse target: required shared components or reusable modules.
6. Known policy conflict candidates (if any).

## Workflow
1. Task Understanding
- Confirm task type, scope, priority, and acceptance criteria.
- Read current context from docs/MasterPlan.md.
- Read policy requirements from docs/templates/PROJECT_SPEC.md.

2. Strategic Planning (Planner Layer)
- Break work into executable slices that can pass auditor gates.
- Identify dependency order, fallback options, and stop-loss boundaries.
- If full scope cannot fit the round, cut to smallest valuable deliverable and record deferral in docs/MasterPlan.md.

3. Pipeline Execution (Orchestrator Layer)
- @Worker implements according to the approved plan and repository conventions.
- Mandatory reuse rule: prefer approved shared components and existing module abstractions; do not ship ad-hoc duplicate logic when reusable assets exist.
- Implement in this order when applicable: model, API layer, business logic, tests, comments/docs.

4. Self Validation
- Confirm code builds or runs.
- Run or update relevant tests.
- Check alignment with PROJECT_SPEC rules.

5. Auditor Gate
- Review maintainability, reuse, and test coverage.
- Enforce one-vote veto for policy violations.

6. Spec Circuit-Breaker and Evolution
- Conflict detection: if implementation needs a pattern rejected by policy (for example, @Worker proposes fmt.Println while @Auditor blocks it), do not deadlock.
- Circuit-breaker action:
	- Freeze only conflict-affected items.
	- Continue non-conflicting slices to keep delivery alive.
	- Create docs/reports/SPEC_CHANGE_REQUEST_XXX.md with conflict evidence, impact, and proposed policy revision.
- Approval flow:
	- APPROVE: apply the accepted rule update to docs/templates/PROJECT_SPEC.md, then resume blocked work.
	- REJECT: keep current policy, replan implementation path, and update docs/MasterPlan.md with mitigation.

7. Single-Task Output Update
- Always create docs/reports/ROUND_REPORT_YYYYMMDD_TYPE_XXX.md for every task, including small fixes and docs-only work.
- Update docs/MasterPlan.md only for this task's progress, status, and blockers.
- Do not perform cross-task summaries, archive actions, or global report merges in this skill.

## Quality Gates
1. Code is consistent with current architecture and naming.
2. Reuse-first principle is enforced; component bypass requires explicit exception.
3. Tests are added or updated for behavior changes.
4. Round report and task-scoped MasterPlan updates are completed for every task.
5. Any policy conflict is processed through SPEC_CHANGE_REQUEST and approval state is explicit.
6. Approved policy evolution is reflected in docs/templates/PROJECT_SPEC.md.

## Key Paths
- docs/templates/MasterPlan.md
- docs/templates/PROJECT_SPEC.md
- docs/templates/ROUND_REPORT.md
- docs/templates/SPEC_CHANGE_REQUEST.md
- docs/MasterPlan.md
- docs/reports/

## Example Prompts
- /auto-developer deliver DEV task with Planner decomposition and Orchestrator execution, enforce shared component reuse
- /auto-developer fix BUG in repo-management flow, trigger SPEC_CHANGE_REQUEST if auditor conflict appears, and output one round report
- /auto-developer implement P0 scope with strict auditor veto, partial-scope fallback, and task-scoped MasterPlan sync
