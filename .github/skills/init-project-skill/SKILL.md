---
name: init-project-skill
description: 'Generate initialization guidance files by analyzing the current project. Use for environment readiness summary, framework identification, naming rule definition, repository URL capture, branch convention baseline, and project structure overview documentation.'
argument-hint: 'Fully optional. Call with no params for default plan mode, or override ActionMode, audience, docPurpose as needed. Auto-detects everything from current folder.'
---

# InitProjectSkill (Project Initialization Skill)

## Core Principle
- This skill is documentation-first, not scaffold execution.
- It analyzes the current project state and existing documents, then generates initialization guidance artifacts.
- It standardizes outputs for environment checks, framework baseline, naming conventions, repository references, branch rules, and project structure summaries.

## What This Skill Produces
- A project initialization guidance document for the current repository.
- Structured analysis of environment readiness and framework baseline.
- Defined naming and branch convention recommendations with evidence.
- Repository metadata summary (remote URL, branch baseline, repository mode).
- Project structure overview for the existing codebase.
- Traceable governance output aligned with the unified output contract.

## When to Use
- You need to assess an existing project and generate a standard initialization guide.
- You want project basic information documented without changing runtime code.
- You need a repeatable analysis artifact for governance, onboarding, or audit.

## Required Inputs
None. All parameters are optional with smart defaults.

## Optional Inputs (all auto-detected if omitted)
1. `ActionMode`: `check-only` | `plan` | `apply` (default: `plan`).
2. `docPurpose`: what the guidance document is for (default: `initialization-guide`).
3. `audience`: who uses the guide, e.g. `onboarding` | `governance` (default: auto-detected).
4. `lifecycle`: `live` (default) | `history`.
5. `outputFormat`: `markdown` (default) | `structured-json`.

All other information (projectId, framework, branch rules, naming conventions, project structure) is auto-detected from the current folder by scanning:
- package.json, server/ folder, client/ folder, .git/config
- existing docs in docs/ and .autoCodeForge/
- README files for repository metadata
- Folder structure for architecture signals

## Governance Requirements
1. Document Placement (Reference: autocodeforge-doc-placement)
- Route output to docs/ (for onboarding audience) or .autoCodeForge/docs/ (for governance).
- Follow naming conventions from .autoCodeForge/config/naming-rules.md.
- Auto-detect lifecycle: if first generation or latest approved, mark as `live`.

2. Naming and Metadata Compliance
- Use pattern: `{ProjectName}-InitGuide-v{Version}-{Date}.md` for product docs.
- Use pattern: `INIT_GUIDE_{PURPOSE}_{DATE}_{SEQUENCE}.md` for governance docs.
- Include Document Governance Metadata block (path, keywords, lifecycle, archive target).
- Keywords: minimum 3 (e.g. initialization, framework, naming-rules).

3. Output Contract Consistency
- Use unified field order shared with autocodeforge-doc-placement and other governance skills.
- ComplianceStatus, LifecycleDecision, LiveStatus, RecommendedPath, PlacementReason, etc.

4. Safety and Auditability
- In `check-only` mode, do not change files; report findings only.
- In `plan` mode, generate deterministic document generation steps and placement decision.
- In `apply` mode, only create or update guidance documents and registry references.
- This skill must not perform project scaffold creation, dependency installation, or repository mutation.

## Workflow
1. Auto-detect current folder context:
   - Read package.json, .git/config, README.md, existing docs.
   - Extract projectId, framework baseline, branch strategy, naming patterns.
   
2. Run governance checks:
   - Determine output placement using autocodeforge-doc-placement rules.
   - Validate naming compliance against .autoCodeForge/config/naming-rules.md.
   - Build required metadata fields.

3. Generate guidance sections:
   - Environment readiness summary (Node.js, Python, Git, Docker signals).
   - Framework baseline (Vue 3 + .NET, monorepo structure, tooling).
   - Naming rule definition (inferred from existing artifacts).
   - Repository metadata (remote URL, default branch, repository mode).
   - Branch convention baseline (main, develop, feature branches).
   - Project structure overview (folder taxonomy, key entry points).

4. Handle ActionMode:
   - `check-only`: Report findings, confidence level, and missing evidence only.
   - `plan`: Generate outline, placement decision, and document generation plan with no mutations.
   - `apply`: Write or update guidance document and update registry links.

5. Emit output contract and follow-up actions.

## Decision Logic
- **Output Placement**: Auto-detect audience (onboarding → docs/how-to/ | governance → .autoCodeForge/docs/reference/), then invoke autocodeforge-doc-placement logic for final path.

- **Naming Compliance**: Check `docName` against naming-rules.md. If noncompliant, propose rename and mark `ComplianceStatus=noncompliant`.

- **Framework Priority**: Multiple signals rank as: package.json + folder structure (highest) > existing docs (medium) > README inference (lowest).

- **Evidence Confidence**: 
  - High: package.json + git config exist and are readable.
  - Medium: docs exist with framework indicators.
  - Low: only README or sparse folder signals.
  - Mark `ComplianceStatus=warning` if evidence is incomplete.

- **Lifecycle Assignment**: First generation or latest approved → `live`. Explicitly marked as historical → `history`.

## Output Contract
Use this exact field order for parser stability across AutoCodeForge skills.

- ComplianceStatus: compliant | noncompliant | warning
- LifecycleDecision: live | history-candidate | history | deprecated
- LiveStatus: live | history
- RecommendedPath: target folder path for created or validated artifacts
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

## Completion Checks
1. Auto-detection succeeds for projectId, framework, and branch strategy.
2. Output placement decision is made (docs/ or .autoCodeForge/docs/).
3. Document naming is compliant with naming-rules.md.
4. ComplianceStatus is explicit (compliant | noncompliant | warning).
5. Lifecycle is assigned (live or history).
6. Output Contract fields follow unified order (ComplianceStatus, LifecycleDecision, LiveStatus, RecommendedPath, etc.).
7. Metadata block includes path, keywords, archive target.
8. PlannedActions list is ordered and actionable.
9. For apply mode: document is created or updated, registry links are refreshed.

## Example Prompts
- /init-project-skill
- /init-project-skill ActionMode=check-only
- /init-project-skill ActionMode=apply audience=onboarding lifecycle=live
- /init-project-skill ActionMode=apply audience=governance docPurpose=framework-baseline
- /init-project-skill audience=engineering outputFormat=structured-json
