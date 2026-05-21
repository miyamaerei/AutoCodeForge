---
name: autocodeforge-skill-orchestrator
description: 'Orchestrate AutoCodeForge governance skills as one end-to-end flow. Use for intent routing, staged execution, contract alignment, and final governance handoff across bootstrap, direction, baseline, rule, opinion, and archive checks.'
argument-hint: 'Optional. Provide objective, ActionMode, depth, and outputScope. Example: objective=full-governance ActionMode=plan depth=standard outputScope=docs+governance.'
---

# AutoCodeForge Skill Orchestrator

## Core Principle
- This skill is orchestration-first.
- It does not replace domain skills; it coordinates them.
- It standardizes call order, handoff fields, and completion gates.
- It enforces one governance language across all autocodeforge-prefixed skills.

## Orchestration Target Skills
- `autocodeforge-doc-governance-bootstrap`
- `autocodeforge-project-direction-skill`
- `base-development-doc-skill`
- `code-rule-skill`
- `code-opinion-analyze-skill`
- `autocodeforge-archive-management`

## What This Skill Produces
- A deterministic orchestration plan for AutoCodeForge documentation governance.
- A routed execution path based on objective and current repository state.
- A stage-by-stage handoff contract (input, output, follow-up).
- A final governance summary with risks, unresolved items, and next actions.

## When to Use
- You want one command to drive multiple autocodeforge skills in a controlled sequence.
- You need to decide which governance/document skills to run, and in what order.
- You want parser-stable outputs and no dead-end transitions between skills.
- You need check-only, plan, or apply behavior across a full workflow.

## Required Inputs
None. All inputs are optional and can be auto-detected.

## Optional Inputs
1. `ActionMode`: `check-only` | `plan` | `apply` (default: `plan`).
2. `objective`: `bootstrap-only` | `direction-only` | `baseline-only` | `rule-only` | `opinion-only` | `archive-only` | `full-governance` (default: `full-governance`).
3. `depth`: `quick` | `standard` | `thorough` (default: `standard`).
4. `outputScope`: `docs` | `governance` | `docs+governance` (default: `docs+governance`).
5. `targetLayer`: `backend` | `frontend` | `fullstack` | `platform` (default: auto-detect).
6. `audience`: `engineering` | `onboarding` | `governance` | `leadership` | `code-review` (default: auto-detect).
7. `canonicalRule`: `latest-approved` | `first-created` (default: `latest-approved`).
8. `lifecycle`: `live` | `history` (default: `live`).
9. `sourceDocuments`: optional list of input files for focused analysis.
10. `injectPathVariables`: `true` | `false` (default: `true`).
11. `governanceRoot`: optional override for `GovernanceRoot`.
12. `docsRoot`: optional override for `DocsRoot`.
13. `skillsRoot`: optional override for `SkillsRoot`.

## Path Variables
- `GovernanceRoot`: default `.autoCodeForge`
- `DocsRoot`: default `docs`
- `SkillsRoot`: default `.github/skills`
- `ConfigRoot`: default `${GovernanceRoot}/config`
- `RegistryRoot`: default `${GovernanceRoot}/registry`
- `TemplateRoot`: default `${GovernanceRoot}/templates`
- If these variables are omitted, this skill uses the defaults above for backward compatibility.

## Variable Injection Policy
- This orchestrator resolves path variables once per run, then injects them into every downstream skill call.
- Default behavior is `injectPathVariables=true`.
- Injection payload includes: `GovernanceRoot`, `DocsRoot`, `SkillsRoot`, `ConfigRoot`, `RegistryRoot`, `TemplateRoot`.
- Precedence order:
  - user-provided overrides (`governanceRoot`, `docsRoot`, `skillsRoot`)
  - orchestrator-resolved values
  - downstream skill defaults
- If `injectPathVariables=false`, downstream skills fall back to their own defaults.
- Stage inputs must record the final injected values for traceability.

## Governance Requirements
1. Contract Uniformity
- All stage outputs must keep the canonical field order used by autocodeforge document skills.
- Handoff must include `ActionMode`, `ComplianceStatus`, `LifecycleDecision`, and `RequiredFollowUps`.

2. Deterministic Routing
- Same input profile must produce same stage order and same skill selection.
- Unknown objective falls back to `full-governance` with warning.

3. Safe Mode Behavior
- `check-only`: discovery and diagnostics only, no file mutations.
- `plan`: stage plans and target paths only, no file mutations.
- `apply`: execute selected stages and include registry/trace follow-up actions.

4. Evidence and Traceability
- Every stage decision must include evidence source (manifest, code structure, existing docs, registry data).
- Final output must aggregate unresolved gaps and ownership suggestions.

5. Automatic Path Injection
- Orchestrator must pass resolved path variables in every invoked stage when `injectPathVariables=true`.
- Downstream stage execution must not require rediscovery of path roots.
- If a downstream skill rejects an injected variable, keep execution and record warning in stage blockers.

## Stage Flow (Default: full-governance)
1. Stage A: Governance Bootstrap
- Call `autocodeforge-doc-governance-bootstrap`.
- Goal: Ensure `.autoCodeForge/` structure, naming rules, archive rules, and registry baseline exist.
- Required handoff: naming and archive governance references.
- Injected inputs: resolved path variables + shared run context.

2. Stage B: Project Direction Blueprint
- Call `autocodeforge-project-direction-skill`.
- Goal: Generate architecture and delivery direction baseline.
- Required handoff: direction evidence and placement decision.
- Injected inputs: resolved path variables + shared run context.

3. Stage C: Development Baseline Document
- Call `base-development-doc-skill`.
- Goal: Define base class, utility, exception, response, pagination, CRUD, dependencies guidance.
- Required handoff: baseline domain coverage and confidence.
- Injected inputs: resolved path variables + shared run context.

4. Stage D: Code Rule Governance
- Call `code-rule-skill`.
- Goal: Define naming/comment/format/review/tooling governance rules.
- Required handoff: rules-by-domain coverage and enforcement level.
- Injected inputs: resolved path variables + shared run context.

5. Stage E: Code Opinion Analysis
- Call `code-opinion-analyze-skill`.
- Goal: Evaluate generated standards and current codebase for risk and optimization opportunities.
- Required handoff: findings severity distribution and remediation roadmap.
- Injected inputs: resolved path variables + shared run context.

6. Stage F: Archive and Lifecycle Validation
- Call `autocodeforge-archive-management` for key generated artifacts.
- Goal: Confirm live/history routing, naming compliance, and archive readiness.
- Required handoff: canonical file decisions and archive plan.
- Injected inputs: resolved path variables + shared run context.

7. Stage G: Final Aggregation
- Merge stage outputs into one orchestration result.
- Produce consolidated follow-ups, risk summary, and next-step list.

## Objective Routing Matrix
- `bootstrap-only` -> Stage A
- `direction-only` -> Stage B
- `baseline-only` -> Stage C
- `rule-only` -> Stage D
- `opinion-only` -> Stage E
- `archive-only` -> Stage F
- `full-governance` -> Stage A -> B -> C -> D -> E -> F -> G

## Decision Logic
- If `.autoCodeForge/config/naming-rules.md` or `.autoCodeForge/config/archive-rules.md` is missing:
  - Force Stage A before any other stage.
- If `injectPathVariables=true` and overrides are provided:
  - Resolve `ConfigRoot`, `RegistryRoot`, and `TemplateRoot` from injected roots before Stage A.
- If both `client/` and `server/` show strong signals:
  - Default `targetLayer=fullstack`.
- If objective includes quality or refactoring but no rule/baseline documents are found:
  - Force Stage C and Stage D before Stage E.
- If lifecycle conflicts are detected after apply:
  - Require Stage F re-check in `plan` mode and block closure.
- If unknown inputs are provided:
  - Continue with defaults and emit warning in `DecisionEvidence`.

## Output Contract
Use this exact order to remain parser-compatible with existing AutoCodeForge skills.

- ComplianceStatus: compliant | noncompliant | warning
- LifecycleDecision: live | history-candidate | history | deprecated
- LiveStatus: live | history
- RecommendedPath: target folder path for orchestration outputs
- PlacementReason: concise reason for selected path or status
- CanonicalFile: optional, selected live file when canonical resolution is required
- TemplateType: optional, orchestration-plan | governance-summary | stage-report
- TemplateCatalogPath: .autoCodeForge/templates/README.md
- TemplatePath: optional, selected template path if used
- ArchiveRecordTemplatePath: .autoCodeForge/templates/TEMPLATE_History_Archive_Record.md
- TemplateSkeleton: optional, only when mapped template is missing
- RequiredMetadataFields: required when metadata completeness check applies
- NamingCompliance: compliant | noncompliant
- ProposedFileName: required when naming is noncompliant
- RequiredKeywords: optional metadata keywords for generated orchestration docs
- ArchiveTargetPath: required for history-candidate/history outputs
- DecisionEvidence:
  - routing evidence
  - stage gating evidence
  - naming and archive evidence
  - domain coverage evidence
  - risk evidence
- ActionMode: check-only | plan | apply
- PlannedActions: ordered list of invoke-skill/create/update-index/update-trace/verify-archive actions
- RegistryUpdatesRequired: true | false
- RequiredFollowUps:
  - naming check against .autoCodeForge/config/naming-rules.md
  - archive check against .autoCodeForge/config/archive-rules.md
  - registry update in .autoCodeForge/registry/artifacts-index.md when location/status changed
  - trace update in .autoCodeForge/registry/trace-map.md when provenance changed
  - rerun opinion analysis when standards documents change materially
- RiskLevel: low | medium | high
- ResolvedPathVariables:
  - GovernanceRoot
  - DocsRoot
  - SkillsRoot
  - ConfigRoot
  - RegistryRoot
  - TemplateRoot

## Stage Result Extension (Orchestrator-specific)
- StageResults:
  - stage: A..G
  - skill: invoked skill name
  - status: skipped | completed | blocked
  - pathVariablesInjected: true | false
  - inputsSummary: key inputs passed to that stage
  - injectedPathVariables: resolved variables for that stage
  - outputsSummary: key outputs consumed from that stage
  - blockers: optional list
- OrchestrationSummary:
  - objective
  - executedStages
  - skippedStages
  - blockers
  - topRisks
  - nextActions

## Completion Checks
1. Objective is resolved to a deterministic stage path.
2. Stage prerequisites are validated before each invocation.
3. Every invoked stage returns parser-stable output fields.
4. Final output includes unresolved blockers and ownership hints.
5. RiskLevel and RequiredFollowUps are explicit.
6. Archive/lifecycle decisions are explicit for generated artifacts.
7. ActionMode behavior is respected end-to-end.
8. Path variables are injected to all invoked stages when `injectPathVariables=true`.

## Example Prompts
- /autocodeforge-skill-orchestrator
- /autocodeforge-skill-orchestrator objective=full-governance ActionMode=plan depth=standard outputScope=docs+governance
- /autocodeforge-skill-orchestrator objective=full-governance ActionMode=apply injectPathVariables=true governanceRoot=.governance docsRoot=documentation
- /autocodeforge-skill-orchestrator objective=direction-only ActionMode=apply audience=engineering targetLayer=fullstack
- /autocodeforge-skill-orchestrator objective=opinion-only ActionMode=check-only depth=thorough sourceDocuments=docs/AutoCodeForge-ProjectOverview-v1.0.0-20260521.md
- /autocodeforge-skill-orchestrator objective=archive-only ActionMode=plan canonicalRule=latest-approved lifecycle=history
