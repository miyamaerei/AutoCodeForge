---
name: spec-conflict-triage
description: "Triage and document policy conflicts. Invoke when implementation and audit rules disagree, or when needing SPEC_CHANGE_REQUEST drafting."
---

# Spec Conflict Triage

## When to Use
- Worker output and auditor checks disagree.
- A requirement appears to violate current PROJECT_SPEC interpretation.
- You need a structured decision trail before requesting policy change.

## Inputs Required
1. Conflict statement: what failed and why it is considered a conflict.
2. Impacted scope: modules, files, and task type.
3. Current policy references from docs/templates/PROJECT_SPEC.md.
4. Evidence: code excerpts, test outcomes, audit findings.

## Canonical Paths
- Template (read-only):
  - docs/templates/SPEC_CHANGE_REQUEST.md
- Output:
  - docs/reports/SPEC_CHANGE_REQUEST_XXX.md

## Workflow
1. Confirm this is a policy conflict, not a plain implementation bug.
2. Collect evidence bundle:
- What implementation did.
- What auditor expected.
- Which rule clause is involved.
3. Classify conflict type:
- Ambiguous rule wording.
- Rule too strict for practical implementation.
- Rule gap not covering new scenario.
4. Assess impact:
- Affected modules and tasks.
- Risk if unchanged.
- Risk if changed.
5. Produce a SPEC_CHANGE_REQUEST draft with:
- Reason.
- Proposed change.
- Impact scope.
- Rollback or mitigation plan.
6. Mark status as Awaiting Approval and stop conflict-affected execution.

## Decision Logic
1. If conflict is resolved by existing rule clarification:
- Record clarification.
- Continue work without spec change request.
2. If conflict needs rule modification:
- Create SPEC_CHANGE_REQUEST_XXX.md.
- Wait for APPROVE or REJECT.

## Completion Checks
1. Conflict source is explicitly linked to policy clause or gap.
2. Evidence is concrete and reproducible.
3. Proposed change is testable and bounded.
4. Approval state and next action are clear.
5. No silent continuation on unresolved policy conflict.

## Example Prompts
- /spec-conflict-triage analyze auditor conflict on component reuse rule and draft change request
- /spec-conflict-triage classify this PROJECT_SPEC mismatch and produce decision notes
- /spec-conflict-triage generate SPEC_CHANGE_REQUEST with impact assessment for task-center flow
