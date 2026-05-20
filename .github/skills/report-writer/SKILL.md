---
name: report-writer
description: 'Generate and update AutoCodeForge execution reports. Use for ROUND_REPORT and DAILY_REPORT drafting, completion checks, data reconciliation, and traceable report output.'
argument-hint: 'Describe report type, date, round type, and source facts.'
---

# Report Writer

## When to Use
- You need to create or update ROUND_REPORT for a completed task round.
- You need to generate DAILY_REPORT from one or more round reports.
- You need a consistent report structure aligned with project templates.
- You need to reconcile missing fields before report finalization.

## Inputs Required
1. Report type: ROUND_REPORT or DAILY_REPORT.
2. Date: YYYYMMDD.
3. Task type for round report: DEV, BUG, REFACTOR, OPTIMIZE, DOCS, or OTHER.
4. Source facts: tasks completed, files changed, tests run, compliance notes, unresolved items.

## Canonical Paths
- Templates (read-only):
  - docs/templates/ROUND_REPORT.md
  - docs/templates/DAILY_REPORT.md
- Outputs:
  - docs/reports/ROUND_REPORT_YYYYMMDD_TYPE_XXX.md
  - docs/reports/DAILY_REPORT_YYYYMMDD.md

## Workflow
1. Select report type and resolve target output file name.
2. Load matching template and extract required sections.
3. Map source facts to sections:
- Task completion details.
- Code output statistics.
- Compliance and audit observations.
- Open issues and next actions.
4. Validate completeness:
- No required section left blank.
- Naming pattern is correct.
- Facts are internally consistent.
5. Write report and include explicit references to related round IDs when applicable.
6. For DAILY_REPORT, aggregate all rounds from the date and mark missing rounds explicitly.

## Completion Checks
1. File name strictly follows naming conventions.
2. Every required section is filled with verifiable content.
3. Metrics and counts match the provided source facts.
4. Risks, blockers, and follow-up items are explicit.
5. For DAILY_REPORT, source round coverage is declared as complete or partial.

## Error Handling
1. Missing source data:
- Mark report as partial.
- Add a Missing Inputs section with exact gaps.
2. Conflicting data:
- Preserve both values with source attribution.
- Add a Reconciliation Needed note.

## Example Prompts
- /report-writer create ROUND_REPORT for 20260520 BUG round 003 using latest execution facts
- /report-writer generate DAILY_REPORT for 20260520 from all available round reports
- /report-writer validate and repair missing sections in ROUND_REPORT_20260520_DEV_001
