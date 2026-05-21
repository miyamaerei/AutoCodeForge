---
name: autocodeforge-doc-governance-bootstrap
description: 'Initialize .autoCodeForge documentation governance in a project. Use for creating folder taxonomy, naming conventions, archive strategy, and validation checks for project docs and code artifacts.'
argument-hint: 'Provide project identifier, mode (quick/full), language (zh/en), and whether to auto-rename noncompliant files.'
---

# AutoCodeForge Doc Governance Bootstrap

## Orchestration Role
- This skill is the governance orchestrator for initialization and policy alignment.
- It manages and aligns downstream execution with:
  - .github/skills/autocodeforge-doc-placement/SKILL.md
  - .github/skills/autocodeforge-archive-management/SKILL.md
- It ensures both downstream skills use the same governance vocabulary, metadata model, and output field order.

## What This Skill Produces
- A standardized `.autoCodeForge/` governance workspace in the current project.
- A clear, enforceable naming convention for docs and generated artifacts.
- A version-aware archive layout that separates current vs history artifacts.
- A validation checklist and index files to keep outputs traceable.
- A cross-skill contract baseline so placement and archive outputs are parser-compatible.

## When to Use
- You want to initialize project documentation governance from scratch.
- You need consistent file naming, archiving, and lifecycle management.
- You want a repeatable structure that supports AI-assisted delivery workflows.

## Design Principles (Condensed)
- Structure by reader intent (Diataxis): tutorial, how-to, reference, explanation.
- Keep docs skimmable and current (Write the Docs): short sections, explicit ownership, version awareness.
- Keep names machine-sortable and human-readable: stable prefix + semantic token + date/version token.

## Required Inputs
1. `projectId`: short identifier, example `AutoCodeForge`.
2. `mode`: `quick` or `full` (default `full`).
3. `language`: `zh` or `en`.
4. `renamePolicy`: `strict` (auto-rename) or `safe` (report-only, default).
5. `date`: optional, default `YYYYMMDD` today.

## Folder Blueprint

Create this structure under project root:

```text
.autoCodeForge/
  README.md
  INDEX.md
  config/
    naming-rules.md
    archive-rules.md
    quality-gates.md
  docs/
    tutorial/
    how-to/
    reference/
    explanation/
  plans/
    active/
    archived/
  reports/
    round/
    daily/
    phase/
  specs/
    current/
    changes/
  templates/
  logs/
    execution/
    audits/
  registry/
    artifacts-index.md
    trace-map.md
  history/
    docs/
    reports/
    specs/
  trash/
```

## Naming Convention

### 1) Core Patterns
- Governance doc: `<projectId>-<docType>-v<major>.<minor>.<patch>-<YYYYMMDD>.md`
- Report doc: `REPORT_<kind>_<YYYYMMDD>_<seq3>.md`
- Plan doc: `PLAN_<scope>_<YYYYMMDD>_<seq3>.md`
- Spec change: `SPEC_CHANGE_REQUEST_<id>.md`
- Template file: `TEMPLATE_<domain>_<purpose>.<ext>`

### 2) History Suffix
- Historical version appends: `-history-v<major>.<minor>.<patch>` before extension.

### 3) Character Rules
- Use lowercase for fixed prefixes (`report`, `plan`, `spec`) only when pattern requires lowercase.
- Use ASCII letters, digits, hyphen, underscore (default policy).
- No spaces, no mixed separators in one filename.
- Date must be `YYYYMMDD`.

## Workflow

1. Discovery
- Detect whether `.autoCodeForge/` already exists.
- Inventory existing docs/reports/spec files and naming patterns.

2. Mode Selection
- `quick`: create minimal governance skeleton (`README`, `INDEX`, `config/naming-rules.md`, `registry/artifacts-index.md`).
- `full`: create full blueprint and initialize all governance files.

3. Structure Initialization
- Create missing folders only.
- Do not delete existing files.

4. Rules Initialization
- Generate `config/naming-rules.md` with patterns and examples.
- Generate `config/archive-rules.md` for current/history split policy.
- Generate `config/quality-gates.md` with compliance checks.

5. Optional Rename Pass (Decision Branch)
- If `renamePolicy = strict`, normalize noncompliant names and record mapping.
- If `renamePolicy = safe`, produce a rename plan only, no file rename.

6. Registry Sync
- Update `registry/artifacts-index.md` with artifact name, type, owner, status, location.
- Update `registry/trace-map.md` linking output to source skill/process.

7. Final Validation
- Run quality gates and output pass/fail summary.

8. Cross-Skill Alignment
- Validate that doc-placement and archive-management both reference:
  - .autoCodeForge/config/naming-rules.md
  - .autoCodeForge/config/archive-rules.md
  - .autoCodeForge/templates/README.md
- Validate both skills use unified output field ordering for parser stability.
- Validate both skills require `Document Governance Metadata` when applicable.

## Decision Logic
- If existing naming conflicts with current team convention:
  - Preserve original files.
  - Add mapped target names in rename plan.
  - Require explicit confirmation before destructive moves.
- If duplicate semantic artifact exists (same type + same date + same scope):
  - Keep first as canonical.
  - Add suffix `-r2`, `-r3` for alternates.
- If both current and history directories contain same version file:
  - Current keeps latest approved.
  - Older copy moved to `history/` and indexed.

## Quality Gates
- Q1: Required `.autoCodeForge/` folders exist for selected mode.
- Q2: Naming pattern compliance >= 95% for target scope.
- Q3: `registry/artifacts-index.md` exists and includes all known artifacts.
- Q4: `config/naming-rules.md` and `config/archive-rules.md` are present and non-empty.
- Q5: No file simultaneously marked current and history.
- Q6: doc-placement and archive-management output contracts are field-order aligned.
- Q7: templates catalog and skill mappings are consistent.

## Output Contract
Use this exact field order for parser stability across governance, doc-placement, and archive-management skills.

- ComplianceStatus: compliant | noncompliant | warning
- LifecycleDecision: live | history-candidate | history | deprecated
- LiveStatus: live | history
- RecommendedPath: target folder path for created or validated governance artifacts
- PlacementReason: concise reason for selected path or status
- CanonicalFile: optional, selected live file when canonical resolution is required
- TemplateType: optional, folder-mapped | history-record | file-level | api-config | process-guide
- TemplateCatalogPath: .autoCodeForge/templates/README.md
- TemplatePath: optional, selected canonical template path
- ArchiveRecordTemplatePath: .autoCodeForge/templates/TEMPLATE_History_Archive_Record.md
- TemplateSkeleton: optional, only when mapped template is missing
- RequiredMetadataFields: required when metadata completeness check applies
- NamingCompliance: compliant | noncompliant
- ProposedFileName: required when naming is noncompliant
- RequiredKeywords: optional, provide when generating metadata-bearing docs
- ArchiveTargetPath: required for history-candidate/history outputs
- DecisionEvidence:
  - structure evidence
  - naming evidence
  - mapping evidence
  - quality gate evidence
- ActionMode: check-only | plan | apply
- PlannedActions: ordered list of create/rename/move/update-index/update-trace/align-contract
- RegistryUpdatesRequired: true | false
- RequiredFollowUps:
  - naming check against .autoCodeForge/config/naming-rules.md
  - registry update in .autoCodeForge/registry/artifacts-index.md when location/status changed
  - trace update in .autoCodeForge/registry/trace-map.md when provenance changed
  - cross-skill output order verification
- RiskLevel: low | medium | high

## Example Prompts
- /autocodeforge-doc-governance-bootstrap ActionMode=plan projectId=AutoCodeForge mode=full language=zh renamePolicy=safe canonicalRule=latest-approved filePath=.autoCodeForge/ docName=governance-bootstrap.md dateOrVersion=20260521
- /autocodeforge-doc-governance-bootstrap ActionMode=plan projectId=AutoCodeForge mode=quick language=zh renamePolicy=safe canonicalRule=latest-approved filePath=.autoCodeForge/config/ docName=naming-rules.md dateOrVersion=20260521
- /autocodeforge-doc-governance-bootstrap ActionMode=check-only projectId=AutoCodeForge mode=full language=zh renamePolicy=strict canonicalRule=latest-approved filePath=docs/ docName=scan-target.md dateOrVersion=20260521

## Notes
- Prefer adding rules and mappings over mutating historical artifacts directly.
- Keep index files up to date in the same change as file creation/rename.
- Treat this skill as the contract owner for doc-placement and archive-management alignment.