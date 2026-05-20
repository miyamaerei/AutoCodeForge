---
name: auto-delivery-loop
description: 'Run closed-loop delivery orchestration in two modes: implement-and-verify or verify-only. Chain requirement matching, regression gate, failure feedback, MasterPlan sync, and report outputs without dead ends.'
argument-hint: 'Describe mode (implement|validate-only), requirement id or text, priority, scope, regression baseline, and report requirements.'
---

# Auto Delivery Loop

## When to Use
- You need end-to-end delivery with mandatory regression before completion.
- You need to prevent one-way execution that only writes round output.
- You need deterministic feedback when verification fails.
- You need task progress, risk, and next actions synchronized back to MasterPlan.

## Core Goal
Transform linear task execution into a closed loop:
1. Plan from live MasterPlan.
2. Execute one task slice.
3. Run regression gate.
4. If regression fails, feed failure back into plan as actionable work.
5. Re-run until acceptance criteria and regression are both green.
6. Publish round output and update plan state.

## Modes
1. implement
- Use when the requirement still needs implementation work.
- Runs full loop including coding via auto-developer.

2. validate-only
- Use when the same requirement was already implemented and you only want closed-loop verification.
- Reuses existing implementation outputs and does not trigger re-development by default.
- Focuses on requirement traceability, regression evidence, and plan/report consistency.

## Orchestration Dependencies
- Execution unit skill:
  - .github/skills/auto-developer/SKILL.md
- Management and output skill:
  - .github/skills/auto-summary-archive/SKILL.md

## Inputs Required
1. Task type: DEV, BUG, REFACTOR, OPTIMIZE, DOCS, or OTHER.
2. Priority: P0, P1, P2, or P3.
3. Scope: target modules, files, and out-of-scope boundaries.
4. Acceptance criteria: behavior and quality gates.
5. Regression baseline: required tests, smoke paths, and impact area checks.
6. Reuse constraints: mandatory shared components and allowed exceptions.
7. Reporting mode: round-only or round plus daily aggregation.
8. Mode: implement or validate-only.
9. Existing evidence for validate-only mode: prior round report ids, changed files, test logs, commit range, or task references.
10. Failure classification policy: enable layered root-cause tagging (code, test-case, environment, requirement).

## Canonical Paths
- Live plan:
  - docs/MasterPlan.md
- Plan and report templates:
  - docs/templates/MasterPlan.md
  - docs/templates/PROJECT_SPEC.md
  - docs/templates/ROUND_REPORT.md
  - docs/templates/DAILY_REPORT.md
  - docs/templates/SPEC_CHANGE_REQUEST.md
- Outputs:
  - docs/reports/ROUND_REPORT_YYYYMMDD_TYPE_XXX.md
  - docs/reports/DAILY_REPORT_YYYYMMDD.md
  - docs/reports/SPEC_CHANGE_REQUEST_XXX.md

## Closed-Loop Workflow
1. Intake and Constraint Lock
- Read live plan and policy baseline.
- Freeze requirement text or requirement id, acceptance criteria, and regression baseline.
- Refuse to start if acceptance criteria or regression baseline is missing.

2. Mode Routing
- implement: run development loop.
- validate-only: run verification loop against existing outputs.

3. Implement Loop (mode=implement)
- Select smallest deliverable slice for one loop.
- Invoke auto-developer for implementation.
- Enforce reuse-first policy and repository conventions.

4. Verification Loop (all modes, mandatory)
- Run required regression baseline.
- Minimum gate:
  - Build or run sanity check.
  - Affected-module tests.
  - Core user path smoke checks.
- Requirement mapping gate:
  - Verify each acceptance criterion has evidence in code changes and test results.
  - Verify this round does not drift from the original requirement intent.
- Do not mark completion when regression is not executed.

5. Failure Feedback Loop
- If regression or requirement mapping fails:
  - Mark current requirement state as blocked or failed with evidence.
  - Classify root cause before creating follow-up actions.
  - Use layered tags:
    - code: implementation defect, integration break, logic regression, or contract mismatch.
    - test-case: missing case, wrong assertion, flaky case, stale fixture, or weak coverage.
    - environment: dependency outage, config drift, secrets/runtime mismatch, CI agent issue, or infrastructure instability.
    - requirement: ambiguous acceptance criteria, conflicting scope statement, or requirement-policy mismatch.
  - Generate category-specific feedback actions in docs/MasterPlan.md instead of one generic todo:
    - code -> BUG fix action with impacted module and suspected commit range.
    - test-case -> TEST action with missing scenario and expected assertion target.
    - environment -> OPS action with environment fingerprint and reproducibility steps.
    - requirement -> REQ action with clarification owner and decision deadline.
  - Record unblock condition and owner for each category action.
  - Keep loop active without claiming completion.
- If policy conflict causes block:
  - Open docs/reports/SPEC_CHANGE_REQUEST_XXX.md.
  - Continue non-conflicting slices when possible.

### Failure Classification and Attribution Rules
1. Classification is mandatory for every blocked or failed result.
2. At least one primary category must be assigned; multiple secondary categories are allowed.
3. Evidence fields required for attribution:
- failing check id or test name
- observed error signature
- first detected timestamp
- impacted scope (module, endpoint, workflow, or dataset)
- reproducibility status (always, intermittent, unknown)
4. If classification confidence is low, mark `needs-triage` and create a short triage action rather than forcing a wrong category.
5. No closure is allowed while a primary failure item lacks owner, unblock condition, or evidence.

### Suggested Feedback Record Schema
- requirement_id
- loop_mode
- failure_state (blocked|failed)
- primary_category (code|test-case|environment|requirement)
- secondary_categories
- evidence_refs
- owner
- unblock_condition
- next_action_type (BUG|TEST|OPS|REQ)
- next_action_priority

6. Plan Synchronization
- Apply status, risk, and effort updates to docs/MasterPlan.md.
- Keep P0 visibility explicit and do not silently downgrade priority.
- In validate-only mode, update verification result and evidence links without creating fake implementation progress.
- Use auto-summary-archive as the canonical path for global progress alignment and historical snapshot consistency.

7. Reporting
- Always generate one round report for the loop outcome.
- Mark report state as:
  - done: acceptance and regression passed.
  - partial: scope reduced with documented deferral.
  - blocked: regression, requirement mismatch, or policy gate unresolved.
- In validate-only mode, report type remains round report but execution type should be VERIFY or OTHER with explicit note "no re-development executed".
- Use auto-summary-archive to produce standardized report outputs and optional daily aggregation.

## Decision Matrix
1. Regression passed and acceptance met:
- Set task state to done.
- Sync MasterPlan and publish round report.

2. Acceptance met but regression failed:
- Set task state to blocked.
- Open failure feedback item and keep loop active.

3. Partial delivery only:
- Set task state to partial.
- Log deferred scope and next-round priority.

4. Spec conflict detected:
- Freeze only conflict-affected work.
- Open SPEC_CHANGE_REQUEST and proceed with non-conflicting scope.

## Hard Rules
1. No regression, no completion.
2. Every failure must create a traceable feedback action in MasterPlan.
3. Every status transition must be backed by execution evidence.
4. Blocked tasks must include unblock condition and owner.
5. Do not edit files under docs/templates directly.
6. In validate-only mode, do not invoke auto-developer unless user explicitly requests re-implementation.

## Completion Checks
1. Acceptance criteria and regression baseline are both satisfied.
2. docs/MasterPlan.md is synchronized with current loop state.
3. One round report exists with explicit done/partial/blocked state.
4. Failure cases include clear follow-up actions and ownership.
5. Policy conflicts are documented in SPEC_CHANGE_REQUEST when needed.
6. Validate-only runs clearly state whether closure is confirmed or rejected for the same requirement.
7. Every failed or blocked run includes root-cause category and attribution evidence.

## Example Prompts
- /auto-delivery-loop deliver P0 DEV task in task-center with mandatory regression and failure feedback, then align MasterPlan via auto-summary-archive
- /auto-delivery-loop run closed loop for BUG fix in repo-management, keep looping until regression gate passes, and output standardized report via auto-summary-archive
- /auto-delivery-loop execute REFACTOR with closed-loop verification and global progress alignment through auto-summary-archive
- /auto-delivery-loop ship partial scope safely, record deferrals, and prepare next-round actions with auto-summary-archive output
- /auto-delivery-loop validate-only for requirement RQ-20260520-01 using existing DEV_003 outputs and regression baseline, no re-development, sync result via auto-summary-archive
- /auto-delivery-loop validate-only same requirement text as previous auto-developer run, confirm closure and update evidence links with auto-summary-archive
