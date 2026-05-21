---
name: autocodeforge-doc-management
description: 'Canonical template skill for AutoCodeForge document generation workflows. Use for 文档管理模板, 统一输出契约, 命名治理, 生命周期判定, and registry follow-up requirements.'
argument-hint: 'Use as a template baseline. Provide target domain skill name, docPurpose, audience, and ActionMode expectations when deriving new skills.'
---

# AutoCodeForge Doc Management Template Skill

## Core Principle
- This skill defines the canonical template for document-generation skills in AutoCodeForge.
- Derived skills must be documentation-first and governance-aware.
- Derived skills must preserve parser-stable output contract ordering.

## What This Template Produces
- A standardized skill structure for governed documentation workflows.
- Unified field semantics for placement, naming, lifecycle, and traceability.
- Reusable ActionMode behavior definitions (`check-only`, `plan`, `apply`).

## When to Use
- You are creating a new documentation-oriented skill.
- You need a consistent contract across governance skills.
- You need deterministic outputs for downstream parsers and audit flows.

## Required Inputs
None. This template can be instantiated with domain-specific defaults.

## Optional Inputs
1. `ActionMode`: `check-only` | `plan` | `apply`.
2. `docPurpose`: domain purpose token for the generated document.
3. `audience`: `engineering` | `onboarding` | `governance`.
4. `lifecycle`: `live` | `history`.
5. `outputFormat`: `markdown` | `structured-json`.

## Path Variables
- `GovernanceRoot`: default `.autoCodeForge`
- `DocsRoot`: default `docs`
- `SkillsRoot`: default `.github/skills`
- `ConfigRoot`: default `${GovernanceRoot}/config`
- `RegistryRoot`: default `${GovernanceRoot}/registry`
- `TemplateRoot`: default `${GovernanceRoot}/templates`
- If these variables are omitted, this skill uses the defaults above for backward compatibility.

## Governance Requirements
1. Placement governance must map target output to `docs/` or `.autoCodeForge/docs/` with explicit reason.
2. Naming governance must evaluate compliance against `.autoCodeForge/config/naming-rules.md`.
3. Metadata governance must require document path, purpose, keywords, lifecycle, archive target.
4. Traceability governance must define registry and trace-map follow-ups.

## Workflow Template
1. Detect project and domain context.
2. Gather source evidence from codebase and existing docs.
3. Build domain document sections.
4. Run placement and naming checks.
5. Apply ActionMode-specific behavior.
6. Emit unified output contract.

## Decision Logic Template
- Evidence priority: source/manifests > existing docs > README-only statements.
- Confidence threshold: high/medium/low based on evidence coverage.
- Lifecycle default: newest approved output is `live`; superseded outputs move to `history-candidate/history`.
- Compliance downgrade: missing governance evidence sets `ComplianceStatus=warning`.

## Output Contract
Use this exact field order for parser stability across AutoCodeForge documentation skills.

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
1. Generated skill sections follow template order.
2. Output contract field order is unchanged.
3. ActionMode behavior is explicitly defined.
4. Placement, naming, metadata, and traceability checks are present.

## Example Prompts
- /autocodeforge-doc-management ActionMode=plan docPurpose=template-baseline audience=governance outputFormat=markdown
- /autocodeforge-doc-management ActionMode=check-only docPurpose=template-audit audience=engineering outputFormat=structured-json
