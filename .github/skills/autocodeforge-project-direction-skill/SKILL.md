---
name: autocodeforge-project-direction-skill
description: 'Analyze the current repository and produce a governed project-direction blueprint covering technology choices, architecture, initialization structure, database and connection strategy, core capabilities, API design, auth/user system, scalability, CI/CD, and API documentation baseline.'
argument-hint: 'Fully optional. Call with no params for default plan mode, or override ActionMode, audience, docPurpose, targetLayer as needed. Auto-detects project signals from current folder.'
---

# ProjectDirectionSkill (Project Direction Blueprint Skill)

## Template Lineage
- This skill is created from the `autocodeforge-doc-management` template family.
- Section order and output contract follow parser-stable governance conventions used by existing AutoCodeForge skills.

## Core Principle
- This skill is documentation-first, not implementation-first.
- It inspects the repository as-is and generates a project-direction blueprint, not runtime feature code.
- It provides a unified north-star document for architecture and delivery alignment before deep implementation.

## What This Skill Produces
- A project-direction blueprint document for the current repository.
- Structured decisions and rationale for:
  - Technology stack and toolchain selection.
  - Architecture style and module boundaries.
  - Project structure initialization and baseline conventions.
  - Database strategy, schema evolution approach, and connection configuration pattern.
  - Core platform capabilities and reusable foundation services.
  - API design standards (contract shape, versioning, error model).
  - Permission and user system direction (authN/authZ model).
  - Scalability and extensibility strategy.
  - CI/CD pipeline baseline and quality gates.
  - API documentation strategy and governance.
- Traceable output aligned with AutoCodeForge governance contract.

## When to Use
- You need a single governed document to define project direction before large-scale implementation.
- You want to align frontend/backend/database/devops decisions using evidence from the current repo.
- You need onboarding and governance evidence for architecture and delivery strategy.

## Required Inputs
None. All parameters are optional and auto-detected.

## Optional Inputs (all auto-detected if omitted)
1. `ActionMode`: `check-only` | `plan` | `apply` (default: `plan`).
2. `docPurpose`: e.g. `project-direction` | `architecture-blueprint` | `delivery-baseline` (default: `project-direction`).
3. `audience`: `engineering` | `onboarding` | `governance` | `leadership` (default: auto-detected).
4. `targetLayer`: `backend` | `frontend` | `fullstack` | `platform` (default: auto-detected, prefer `fullstack` when client/ and server/ both exist).
5. `lifecycle`: `live` (default) | `history`.
6. `outputFormat`: `markdown` (default) | `structured-json`.
7. `scopeProfile`: `core` | `extended` | `enterprise` (default: `core`).
8. `includeExtensions`: comma-separated optional domains, e.g. `security,observability,cost,dr,multi-tenant,data-governance,test-strategy,migration,adr`.

## Extension Domains (Cross-Project Reuse)
- This skill supports optional extension sections so the same framework can be reused beyond the current repository.
- In `scopeProfile=extended` or `scopeProfile=enterprise`, include relevant domains based on project context:
  - Security and compliance baseline (threat model, secrets, policy mapping).
  - SLO/SLI and observability target model (metrics, logs, traces, alerting ownership).
  - Cost and FinOps strategy (budget guardrails, cost attribution, optimization checkpoints).
  - Reliability and disaster recovery baseline (RTO/RPO, backup, failover drills).
  - Multi-tenancy and data isolation strategy (tenant model, sharding/partitioning boundaries).
  - Data governance and lifecycle (classification, retention, masking, lineage).
  - Test strategy blueprint (unit/integration/contract/e2e/performance/security).
  - Migration and rollout strategy (strangler, phased rollout, rollback, compatibility window).
  - Architectural Decision Record (ADR) policy and traceability.
  - Internationalization and localization readiness (locale strategy, content governance).

## Governance Requirements
1. Document Placement (Reference: autocodeforge-doc-placement)
- Route output to `docs/` for engineering/onboarding usage.
- Route output to `.autoCodeForge/docs/` for governance-centric usage.
- Placement and lifecycle decisions must be explicit and traceable.

2. Naming and Metadata Compliance
- Product-facing naming pattern: `{ProjectName}-ProjectDirection-v{Version}-{Date}.md`.
- Governance naming pattern: `PROJECT_DIRECTION_{PURPOSE}_{DATE}_{SEQUENCE}.md`.
- Include metadata block: document path, purpose, keywords, lifecycle, archive target.
- Required keywords should include at least 3 of: `tech-stack`, `architecture`, `database`, `api-design`, `auth`, `scalability`, `cicd`, `api-docs`.

3. Output Contract Consistency
- Keep the exact field order shared by governed AutoCodeForge documentation skills.
- Ensure parser-stable field names and enumerated values.

4. Safety and Auditability
- `check-only`: no file mutation, findings only.
- `plan`: deterministic plan and placement decision, no file mutation.
- `apply`: create or update direction documentation and related registry references only.
- Must not install dependencies, create runtime services, or mutate business source files.

## Workflow
1. Auto-detect project context:
   - Scan `client/`, `server/`, root manifests (`package.json`, `tsconfig.json`, solution files), and existing docs.
   - Detect stack signals (Vue 3 + TypeScript + Vite, .NET solution, testing and tooling signals).

2. Collect direction evidence:
   - Technology and architecture signals from code structure and manifests.
   - Initialization and project-structure signals from folder taxonomy and bootstrapping docs.
   - Database and connection signals from server config, docs, and infrastructure artifacts.
   - API design and auth signals from route/controller/DTO/middleware patterns.
   - CI/CD and documentation signals from workflow files, scripts, and docs.

3. Build blueprint sections:
   - Technology selection baseline (current and recommended).
   - Architecture design baseline (module boundaries, layering, integration points).
   - Project structure initialization baseline.
   - Database and connectivity strategy.
   - Core capability baseline (logging, config, health checks, observability, caching).
   - API design standards and lifecycle.
   - Permission and user-system strategy.
   - Scalability and extensibility model.
   - CI/CD baseline and quality gates.
   - API documentation and contract-governance approach.
  - Optional extension appendices selected by `scopeProfile` and `includeExtensions`.

4. Run governance checks:
   - Resolve placement path and lifecycle.
   - Validate naming against naming rules when available.
   - Produce compliance and confidence assessment.

5. Handle ActionMode:
   - `check-only`: report coverage, conflicts, and missing evidence only.
   - `plan`: produce section outline, placement, and generation plan only.
   - `apply`: write/update document and report index update actions.

6. Emit unified output contract and follow-up actions.

## Decision Logic
- **Target Layer Detection**:
  - If `client/` and `server/` both have strong signals, default to `fullstack`.
  - If only backend signals are strong, prioritize `backend`.
  - If delivery/infrastructure files dominate, allow `platform` orientation.

- **Evidence Priority**:
  - Highest: source files, manifests, pipeline configs, infrastructure descriptors.
  - Medium: existing architecture/product docs.
  - Lowest: README narrative only.

- **Completeness Threshold**:
  - High confidence: at least 8 of 10 direction domains have concrete evidence.
  - Medium confidence: 5-7 domains have evidence.
  - Low confidence: fewer than 5 domains; mark `ComplianceStatus=warning`.

- **Extension Activation Rules**:
  - `core`: evaluate the default direction domains only.
  - `extended`: add high-value optional domains with available evidence.
  - `enterprise`: require security/compliance, reliability/DR, observability/SLO, and migration strategy as mandatory extension checks.
  - If `includeExtensions` is explicitly set, it overrides profile defaults where feasible.

- **Lifecycle Assignment**:
  - First authoritative direction document or latest approved update -> `live`.
  - Superseded direction baseline -> `history` or `history-candidate`.

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
  - direction evidence (tech/architecture/init-structure/database/db-connect/core-capabilities/api/auth/scalability/cicd/api-docs)
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
1. Project scan is complete with explicit stack and layer detection.
2. All direction domains are evaluated:
   - technology selection
   - architecture design
   - project structure initialization
   - database strategy
   - database connection strategy
   - core capabilities
   - API design
   - permissions and user system
   - extensibility and scalability
   - CI/CD baseline
   - API documentation strategy
3. Extension domains are evaluated when `scopeProfile` is `extended` or `enterprise`, and missing evidence is explicitly marked.
4. Placement decision is explicit (`docs/` or `.autoCodeForge/docs/`).
5. Naming compliance result is explicit.
6. ComplianceStatus is explicit (`compliant | noncompliant | warning`).
7. Lifecycle is explicit (`live` or `history`).
8. Output Contract field order matches unified standard.
9. Metadata block includes path, purpose, keywords, lifecycle, archive target.
10. In `apply` mode, target document is created/updated and registry follow-ups are listed.

## Example Prompts
- /autocodeforge-project-direction-skill
- /autocodeforge-project-direction-skill ActionMode=check-only targetLayer=fullstack
- /autocodeforge-project-direction-skill ActionMode=plan docPurpose=architecture-blueprint audience=engineering
- /autocodeforge-project-direction-skill ActionMode=apply audience=governance lifecycle=live
- /autocodeforge-project-direction-skill ActionMode=apply docPurpose=delivery-baseline outputFormat=markdown
- /autocodeforge-project-direction-skill ActionMode=plan scopeProfile=extended includeExtensions=security,observability,test-strategy,migration
- /autocodeforge-project-direction-skill ActionMode=apply scopeProfile=enterprise docPurpose=project-direction