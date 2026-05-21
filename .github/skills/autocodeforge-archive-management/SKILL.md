---
name: autocodeforge-archive-management
description: 'Check one .autoCodeForge file against archive rules and decide live/history routing. Use for 文档归档检查, 日期归档判断, 当前 live 文档判定, and archive compliance verification.'
argument-hint: 'Provide file path, document date/version, and check mode (check-only/plan/apply). 可附中文场景。'
---

# AutoCodeForge Archive Management

## Core Principle
- This skill is archive governance, not only file movement.
- Every archive decision must be evidence-based, naming-compliant, reversible, and traceable in registry.

## What This Skill Produces
- A compliance result for a specific file under .autoCodeForge or docs.
- A live or history decision for that file.
- A date/version-based archive plan when archival is required.
- A precise action list: keep, move, rename, or mark deprecated.
- A governance output with evidence, risk level, and required follow-up updates.

## When to Use
- 你要检查某个文件是否按 .autoCodeForge 归档规范执行。
- 你要判断一个带日期的文档是否应该进入 history。
- 你要确认哪个文件是当前 live 文档。
- 你要在执行归档前先获得可审计的 plan。

## Required Inputs
1. filePath: target file to check.
2. checkMode: check-only, plan, or apply.
3. dateOrVersion: document date and/or version token if available.
4. siblingScope: optional sibling folder for canonical comparison.
5. canonicalRule: latest-approved (default) or first-created.

## Governance Requirements
1. Rule Compliance
- Must validate against .autoCodeForge/config/archive-rules.md.
- Must validate naming against .autoCodeForge/config/naming-rules.md.

1.1 Metadata Completeness
- If target file is governed by templates, require `Document Governance Metadata` completeness.
- Required fields should align with doc placement skill: DocumentPath, TargetFolder, NamingRuleRef, RecommendedFileNamePattern, Keywords, ArtifactType, Lifecycle, RegistryIndexRef, TraceMapRef, ArchiveRuleRef, HistoryPathWhenArchived, SourceTemplate.

2. Evidence First
- Every live/history decision must include evidence.
- Evidence sources: sibling versions, date tokens, version tokens, approval state, current path vs history path.

3. Safety by Mode
- check-only: no write operation.
- plan: no write operation, deterministic action list only.
- apply: execute only after conflict-free decision and explicit actionable targets.

4. Traceability
- Registry updates are mandatory in same change set when apply mode changes path or status.
- artifacts-index and trace-map must stay consistent with final file state.

5. Archive Readiness
- Archive target path must be explicit before apply.
- Naming normalization must be completed before moving files to history.

6. Cross-Skill Consistency
- Output terms should stay compatible with autocodeforge-doc-placement.
- Use shared naming: NamingCompliance, ProposedFileName, ArchiveTargetPath, RequiredFollowUps.

## Workflow
1. Rule Load
- Load .autoCodeForge/config/archive-rules.md and .autoCodeForge/config/naming-rules.md.
- Identify required current path and history path for this file type.

2. Single File Compliance Check
- Check whether the file is currently in correct folder for its lifecycle.
- Check naming pattern and date/version tokens.
- Check whether a conflicting same-semantic file exists.

2.1 Metadata and Registry Baseline Check
- Check whether artifact exists in .autoCodeForge/registry/artifacts-index.md.
- Check whether provenance is recorded in .autoCodeForge/registry/trace-map.md when applicable.
- If missing baseline data, mark as governance warning before apply.

3. Live Document Decision
- If file is latest approved and currently active, mark as live.
- If file is replaced by higher version/newer date, mark as history-candidate.
- If ambiguous, output required evidence to decide live owner.

3.1 Canonical Decision Matrix
- Prefer latest-approved over first-created when approval evidence exists.
- If approvals are missing for all candidates, fall back to canonicalRule.
- If date and version conflict, version has higher priority; date is tie-breaker.

4. Date/Version Archive Decision
- If newer sibling exists, older file should archive.
- Archive naming follows -history-v<major>.<minor>.<patch> when versioned.
- If same day duplicate release, append -r2, -r3.

4.1 Naming Normalization Before Archive
- If file name violates naming-rules, create rename-plan first.
- Only compliant name can be accepted as archive target in apply mode.

5. Action Generation by Mode
- check-only: output compliance result and recommended actions only.
- plan: output exact move/rename plan with target paths.
- apply: execute approved move/rename and then update registry.

5.1 Archive Record Template
- For archive action record, use .autoCodeForge/templates/TEMPLATE_History_Archive_Record.md.
- Output includes source path, target path, trigger reason, and canonical live file.

6. Post-Action Validation
- Verify current/history non-overlap (Q5).
- Verify artifacts-index location and status are updated.
- Verify trace-map update when provenance changed.

6.1 Final Governance Gate
- Validate no unresolved conflicts remain.
- Validate resulting lifecycle state is explicit: live, history, or deprecated.
- Validate archive result is reversible by recorded source-target mapping.

## Decision Logic
- file already in correct current location and is latest approved:
  - status = live
  - action = keep
- file not latest or explicitly superseded:
  - status = history-candidate
  - action = move to corresponding history path
- naming/date token missing:
  - status = noncompliant
  - action = rename-plan before archive apply
- apply mode with unresolved canonical conflict:
  - stop apply and output manual resolution list

## Output Contract
Use this exact field order for parser stability across doc-placement and archive-management skills.

- ComplianceStatus: compliant | noncompliant | warning
- LifecycleDecision: live | history-candidate | history | deprecated
- LiveStatus: live | history (compatibility alias)
- RecommendedPath: final target folder path for resulting lifecycle state
- PlacementReason: optional, concise reason for selected resulting path
- CanonicalFile: selected live file path
- TemplateType: optional, use history-record when archive record output is required
- TemplateCatalogPath: .autoCodeForge/templates/README.md
- TemplatePath: optional, set when a content template is selected for generated output
- ArchiveRecordTemplatePath: .autoCodeForge/templates/TEMPLATE_History_Archive_Record.md
- TemplateSkeleton: optional, only when template file is missing or inline output is requested
- RequiredMetadataFields: required when metadata completeness check applies
- NamingCompliance: compliant | noncompliant
- ProposedFileName: required when naming is noncompliant
- RequiredKeywords: optional, provide when generating or rewriting document metadata
- ArchiveTargetPath: required when lifecycle is history-candidate/history
- DecisionEvidence:
  - path evidence
  - version evidence
  - date evidence
  - approval evidence
- ActionMode: check-only | plan | apply
- PlannedActions: ordered list of keep/move/rename/update-index/update-trace
- RegistryUpdatesRequired: true | false
- RequiredFollowUps:
  - naming check against .autoCodeForge/config/naming-rules.md
  - registry update in .autoCodeForge/registry/artifacts-index.md when location/status changed
  - trace update in .autoCodeForge/registry/trace-map.md when provenance changed
- RiskLevel: low | medium | high

## Completion Checks
1. The target file gets a clear compliance status.
2. Live vs history decision is explicit and evidence-based.
3. Date/version archive rule is applied consistently.
4. If action is needed, target path and filename are deterministic.
5. Q5 non-overlap and registry consistency are validated.
6. Naming compliance is explicit and rename-plan exists when needed.
7. Apply mode never proceeds with unresolved canonical conflict.
8. Archive record fields are complete for audit traceability.
9. Output field order follows the unified Output Contract.
10. Output fields remain compatible with autocodeforge-doc-placement handoff.

## Example Prompts
- /autocodeforge-archive-management ActionMode=check-only filePath=.autoCodeForge/specs/current/AutoCodeForge-ProjectOverview-v1.0.0-20260521.md checkMode=check-only canonicalRule=latest-approved dateOrVersion=v1.0.0 siblingScope=.autoCodeForge/specs/current/
- /autocodeforge-archive-management ActionMode=plan filePath=.autoCodeForge/reports/daily/REPORT_DAILY_20260521_001.md checkMode=plan canonicalRule=latest-approved dateOrVersion=20260521 siblingScope=.autoCodeForge/reports/daily/
- /autocodeforge-archive-management ActionMode=apply filePath=.autoCodeForge/reports/round/REPORT_ROUND_DEV_20260520_001.md checkMode=apply canonicalRule=latest-approved dateOrVersion=20260520 siblingScope=.autoCodeForge/reports/round/
- /autocodeforge-archive-management ActionMode=plan filePath=.autoCodeForge/logs/audits/bad name.md checkMode=plan canonicalRule=latest-approved dateOrVersion=20260521 siblingScope=.autoCodeForge/logs/audits/
