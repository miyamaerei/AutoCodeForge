---
name: auto-summary-archive
description: 'Run manual summary and archive workflow. Use for multi-task aggregation, DAILY_REPORT generation, historical file archival, and component library synchronization.'
argument-hint: 'Describe date range, source round reports, archive scope, and component library sync targets.'
---

# Auto Summary Archive

## When to Use
- You need to aggregate outputs from multiple completed tasks.
- You need to generate a global DAILY_REPORT.
- You need to archive historical planning and report files.
- You need to synchronize component library assets after task delivery.

## Trigger Mode
- Manual on-demand invocation only.
- Not auto-triggered by date and not part of daily mandatory execution.
- Run only when user explicitly requests summary or archival work.

## Inputs Required
1. Date or date range: YYYYMMDD or YYYYMMDD-YYYYMMDD.
2. Source rounds: one or more ROUND_REPORT file names or patterns.
3. Archive scope: MasterPlan working file, ROUND_REPORT files, and temporary files to archive.
4. Daily report target date and coverage scope.
5. Component library sync target and expected sync rule.

## Canonical Paths
- Templates:
  - docs/templates/DAILY_REPORT.md
- Sources:
  - docs/MasterPlan.md
  - docs/reports/ROUND_REPORT_YYYYMMDD_TYPE_XXX.md
- Outputs:
  - docs/reports/DAILY_REPORT_YYYYMMDD.md
  - docs/reports/MasterPlan_YYYYMMDD.md (archive snapshot from docs/MasterPlan.md)
  - archive destination defined by repository policy or user input

## Workflow
1. Collect Sources
- Resolve all required ROUND_REPORT files for the requested scope.
- Load the current working MasterPlan and required temporary artifacts.
- Check for missing source files and record gaps before writing outputs.

2. Multi-Task Aggregation
- Extract completion facts, blockers, risk notes, and compliance outcomes from each source.
- Merge overlapping facts with source traceability.
- Mark unresolved conflicts for manual review.

3. Daily Report Generation
- Use docs/templates/DAILY_REPORT.md as the only daily summary template.
- Use docs/MasterPlan.md as the primary planning/progress source and reconcile with ROUND_REPORT details.
- Generate or update docs/reports/DAILY_REPORT_YYYYMMDD.md.
- Declare coverage: complete, partial, or blocked.

4. Historical Archive
- Create a dated snapshot docs/reports/MasterPlan_YYYYMMDD.md from docs/MasterPlan.md before or during archive.
- Archive selected MasterPlan snapshots, round reports, and temporary files.
- Preserve trace links between archived items and daily summary records.
- Do not delete files when archive destination or policy is ambiguous.

5. Component Library Sync
- Apply requested synchronization for shared components.
- Validate component version and mapping after sync.
- Record sync scope and outcome in the daily summary or archive log.

## Completion Checks
1. Daily report content matches aggregated source facts.
2. Every aggregated task record has at least one source reference.
3. Archive actions are traceable and reversible by path history.
4. Missing inputs and unresolved conflicts are explicit.
5. Component library sync result is verified and documented.

## Example Prompts
- /auto-summary-archive aggregate all 20260520 rounds and generate DAILY_REPORT
- /auto-summary-archive archive finished MasterPlan and ROUND_REPORT files for 202605
- /auto-summary-archive summarize multiple rounds, sync component library, and update DAILY_REPORT
