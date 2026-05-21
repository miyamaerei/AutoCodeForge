---
name: base-development-doc-skill
description: 'Scan the current project and generate a development baseline document. Use for 基类封装, 通用工具类, 异常处理机制, 统一返回体, 分页逻辑, CRUD 基础方法, and dependency inventory documentation.'
argument-hint: 'Fully optional. Call with no params for default plan mode, or override ActionMode, audience, docPurpose, targetLayer as needed. Auto-detects project signals from current folder.'
---

# BaseDevelopmentDocSkill (Development Baseline Documentation Skill)

## Template Lineage
- This skill is created from the `autocodeforge-doc-management` template.
- Section order and output contract follow the canonical doc-management template for parser stability.

## Core Principle
- This skill is documentation-first, not implementation-first.
- It scans the current repository and existing artifacts, then generates or plans development baseline documents.
- It does not create runtime base classes, utilities, or API code directly; it only outputs governed documentation.

## What This Skill Produces
- A development baseline guidance document for the current project.
- Structured documentation sections for:
  - Project base class encapsulation strategy.
  - Shared utility class strategy.
  - Exception handling mechanism.
  - Unified response body contract.
  - Pagination model and query conventions.
  - Generic CRUD base method patterns.
  - Dependency inventory and version baseline.
- Traceable output aligned with AutoCodeForge governance contract.

## When to Use
- You need one governed document that defines reusable development foundations before implementation.
- You want to standardize backend and/or full-stack scaffolding rules based on current repository reality.
- You need onboarding and governance evidence for base-layer coding conventions.

## Required Inputs
None. All parameters are optional and auto-detected.

## Optional Inputs (all auto-detected if omitted)
1. `ActionMode`: `check-only` | `plan` | `apply` (default: `plan`).
2. `docPurpose`: e.g. `development-baseline` | `backend-foundation` | `fullstack-foundation` (default: `development-baseline`).
3. `audience`: `engineering` | `onboarding` | `governance` (default: auto-detected).
4. `targetLayer`: `backend` | `frontend` | `fullstack` (default: `backend` if server signals are strong, otherwise auto).
5. `lifecycle`: `live` (default) | `history`.
6. `outputFormat`: `markdown` (default) | `structured-json`.

## Governance Requirements
1. Document Placement (Reference: autocodeforge-doc-placement)
- Route output to `docs/` for engineering/onboarding usage.
- Route output to `.autoCodeForge/docs/` for governance-centric usage.
- Placement and lifecycle decision must be explicit and traceable.

2. Naming and Metadata Compliance
- Product-facing naming pattern: `{ProjectName}-DevBaseline-v{Version}-{Date}.md`.
- Governance naming pattern: `DEV_BASELINE_{PURPOSE}_{DATE}_{SEQUENCE}.md`.
- Include metadata block: document path, purpose, keywords, lifecycle, archive target.
- Required keywords should include at least 3 of: `base-class`, `exception-handling`, `response-contract`, `pagination`, `crud`, `dependencies`.

3. Output Contract Consistency
- Keep the same field order used by initialization and governance skills.
- Ensure parser-stable field names and enumerated values.

4. Safety and Auditability
- `check-only`: no file mutation, findings only.
- `plan`: deterministic plan and placement decision, no file mutation.
- `apply`: create or update baseline documentation and related registry references only.
- Must not install dependencies, scaffold runtime code, or modify business source files.

## Workflow
1. Auto-detect project context:
   - Scan `client/`, `server/`, root `package.json`, `tsconfig.json`, solution files, and docs.
   - Detect tech stack signals (Vue 3, TypeScript, .NET, test stack, package managers).
2. Collect foundation evidence:
   - Search for existing base abstractions, shared utility modules, exception middleware/filters, DTO wrappers, pagination patterns, generic repository/service methods.
   - Extract dependency versions from package manifests and project files.
3. Build documentation sections:
   - Baseline architecture scope and target layer.
   - Base class encapsulation design and responsibilities.
   - Utility taxonomy and boundary rules.
   - Exception flow, error code model, and logging contract.
   - Unified response schema and API output conventions.
   - Pagination request/response fields, sorting, filtering constraints.
   - Generic CRUD method template and extension points.
   - Dependency inventory grouped by runtime, framework, infrastructure, and dev/test.
4. Run governance checks:
   - Resolve placement path and lifecycle.
   - Validate naming against naming rules when available.
   - Build compliance and confidence assessment.
5. Handle ActionMode:
   - `check-only`: report coverage and missing evidence only.
   - `plan`: produce section outline, placement, and generation steps only.
   - `apply`: write/update document and report index update actions.
6. Emit unified output contract and follow-up actions.

## Decision Logic
- **Target Layer Detection**:
  - If `server/` and .sln/.csproj signals exist, prioritize `backend` baseline.
  - If both `client/` and `server/` are substantial and requested, use `fullstack`.
- **Evidence Priority**:
  - Highest: source files and manifests.
  - Medium: existing architecture/design docs.
  - Lowest: README narrative only.
- **Completeness Threshold**:
  - High confidence: all 7 requested domains have concrete evidence.
  - Medium confidence: 4-6 domains have evidence.
  - Low confidence: fewer than 4 domains; mark `ComplianceStatus=warning`.
- **Lifecycle Assignment**:
  - First authoritative baseline or latest approved update -> `live`.
  - Superseded baseline -> `history` or `history-candidate`.

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
  - baseline evidence (base-class/utility/exception/response/pagination/crud/dependencies)
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
1. Project scan completed with explicit stack and layer detection.
2. All requested domains are evaluated: base class, utility class, exception, response, pagination, CRUD, dependencies.
3. Placement decision is explicit (`docs/` or `.autoCodeForge/docs/`).
4. Naming compliance result is explicit.
5. ComplianceStatus is explicit (`compliant | noncompliant | warning`).
6. Lifecycle is explicit (`live` or `history`).
7. Output Contract field order matches unified standard.
8. Metadata block includes path, purpose, keywords, lifecycle, archive target.
9. In `apply` mode, target document is created/updated and registry follow-ups are listed.

## Example Prompts
- /base-development-doc-skill
- /base-development-doc-skill ActionMode=check-only targetLayer=backend
- /base-development-doc-skill ActionMode=plan docPurpose=fullstack-foundation audience=engineering
- /base-development-doc-skill ActionMode=apply audience=governance lifecycle=live
- /base-development-doc-skill ActionMode=apply docPurpose=backend-foundation outputFormat=markdown
