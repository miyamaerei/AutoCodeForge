---
name: code-opinion-analyze-skill
description: 'Post-analysis skill that evaluates generated code, templates, and dependency configurations from prior skills (base-development-doc-skill, code-rule-skill). Identifies non-ideal code patterns, redundancy, potential bugs, specification violations, and dependency issues. Outputs targeted optimization recommendations, problem diagnosis, and improvement suggestions as a structured analysis report.'
argument-hint: 'Fully optional. Call with no params for default plan mode, or override ActionMode, docPurpose, audience, analysisDepth. Auto-detects base/rule documents from docs/ and .autoCodeForge/docs/. Specify sourceDocuments to analyze specific prior skill outputs; omit to scan all available.'
---

# CodeOpinionAnalyzeSkill (Code Opinion Analysis Skill)

## Template Lineage
- This skill is created from the `autocodeforge-doc-management` template.
- Section order and output contract follow the canonical doc-management template for parser stability.
- Extends analysis patterns from `base-development-doc-skill` and `code-rule-skill` to perform post-generation quality assessment.
- Designed as a post-processor that works after baseline and rule documentation skills complete.

## Core Principle
- This skill is analysis-first, not implementation-first.
- It scans generated documentation (from base-development-doc-skill, code-rule-skill, and related outputs) and codebase artifacts, then produces actionable quality analysis.
- It does not generate code, apply fixes, or modify documents directly; it only outputs governed analysis reports.
- Analysis output identifies patterns that deviate from stated rules, detects redundancy/anti-patterns, and recommends targeted improvements without implementation.

## What This Skill Produces
- A comprehensive code opinion analysis report for the current project.
- Structured analysis sections for:
  - **Code Redundancy Analysis**: Identifies duplicate utility functions, repeated patterns, unused code paths, and shared logic opportunities.
  - **Potential Bug Detection**: Identifies error handling gaps, missing null/undefined checks, type safety violations, race conditions, and lifecycle issues.
  - **Specification Compliance Audit**: Cross-references generated code against defined naming rules, comment standards, formatting conventions, and architectural principles.
  - **Dependency Configuration Review**: Analyzes package manifests for version conflicts, transitive dependency overhead, security vulnerabilities, and unused dependencies.
  - **Project Structure Assessment**: Evaluates module organization, layer coherence, naming hierarchy consistency, and boundary clarity against declared standards.
  - **Performance and Scalability Issues**: Flags inefficient patterns, unoptimized queries, memory leaks, excessive bundling, and architectural bottlenecks.
  - **Test Coverage Gaps**: Identifies untested code paths, missing unit/integration tests, and insufficient edge case coverage.
  - **Security and Compliance Violations**: Detects hardcoded secrets, missing input validation, insufficient access controls, and cryptographic weaknesses.
- Traceable output aligned with AutoCodeForge governance contract.
- Problem severity classification (critical, high, medium, low).
- Actionable remediation steps for each identified issue.

## When to Use
- You have generated baseline and rule documentation and now need to validate code quality.
- You want a structured assessment of whether implementation aligns with declared standards.
- You need to identify technical debt, refactoring priorities, and optimization opportunities.
- You're performing pre-review quality gates before code promotion or team onboarding.
- You want to generate governance evidence for compliance and audit workflows.

## Required Inputs
None. All parameters are optional and auto-detected.

## Optional Inputs (all auto-detected if omitted)
1. `ActionMode`: `check-only` | `plan` | `apply` (default: `plan`).
   - `check-only`: Analyze and report findings only; no output file generation.
   - `plan`: Generate analysis plan, findings summary, and placement decision; no report write.
   - `apply`: Create full analysis report and update governance registry.

2. `docPurpose`: e.g. `code-quality-analysis` | `code-review-gate` | `refactoring-roadmap` | `compliance-audit` (default: `code-quality-analysis`).

3. `audience`: `engineering` | `onboarding` | `governance` | `code-review` (default: auto-detected).

4. `analysisDepth`: `quick` | `standard` | `thorough` (default: `standard`).
   - `quick`: Surface-level pattern scanning, high-level structural assessment.
   - `standard`: Pattern detection + dependency analysis + selective code inspection.
   - `thorough`: Deep inspection of all code paths, security audit, performance profiling recommendations.

5. `analysisLayers`: `backend` | `frontend` | `fullstack` | comma-separated list (default: auto-detected from project signals).

6. `sourceDocuments`: Comma-separated list of base documentation file paths to analyze (optional; default: auto-scan `docs/` and `.autoCodeForge/docs/`).

7. `lifecycle`: `live` (default) | `history`.

8. `outputFormat`: `markdown` (default) | `structured-json`.

9. `reportStyle`: `detailed` | `executive-summary` | `checklist` (default: `detailed`).

## Governance Requirements

### 1. Document Placement (Reference: autocodeforge-doc-placement)
- Route output to `docs/` for engineering/code-review usage (e.g., `docs/CodeOpinion-Analysis-v{Version}-{Date}.md`).
- Route output to `.autoCodeForge/docs/` for governance-centric usage.
- Placement and lifecycle decision must be explicit and traceable.
- Analysis reports should reference source documents for traceability.

### 2. Naming and Metadata Compliance
- Product-facing naming pattern: `{ProjectName}-CodeOpinion-v{Version}-{Date}.md` or `code-opinion-analysis-{layer}-v{Version}-{Date}.md`.
- Governance naming pattern: `CODE_OPINION_{PURPOSE}_{DATE}_{SEQUENCE}.md`.
- Include metadata block: document path, purpose, keywords, lifecycle, archive target, version, source documents analyzed.
- Required keywords should include at least 5 of: `code-quality`, `redundancy`, `bug-detection`, `compliance-audit`, `dependency-review`, `security`, `performance`, `refactoring`, `technical-debt`, `code-review`.

### 3. Output Contract Consistency
- Keep the same field order used by initialization and governance skills.
- Ensure parser-stable field names and enumerated values.
- Each finding section must include: Finding ID, Severity, Category, Description, Evidence (code location/document reference), Remediation Steps, Priority.

### 4. Safety and Auditability
- `check-only`: no file mutation, findings only.
- `plan`: deterministic analysis plan and placement decision, no file mutation.
- `apply`: create or update analysis report and related registry references only.
- Must not modify source code, dependencies, or configuration files.
- Must not execute code transformations or apply fixes directly.
- All recommendations must be non-breaking and testable.

## Workflow

### Phase 1: Context Detection and Source Document Discovery
1. Auto-detect analysis context:
   - Scan `client/`, `server/`, root `package.json`, `.sln`, `tsconfig.json`, and existing rule/baseline docs.
   - Detect tech stack signals: Vue 3, TypeScript, .NET/C#, test frameworks.
   - Identify prior skill outputs: base-development-doc.md, code-rules.md, project-overview.md.

2. Locate and parse source documents:
   - Search `docs/` and `.autoCodeForge/docs/` for baseline and rule documentation.
   - Extract declared standards, conventions, and architectural principles.
   - Parse defined patterns: naming rules, exception handling contracts, CRUD methods, dependency constraints.

### Phase 2: Code and Configuration Scanning
3. Perform codebase snapshot:
   - For backend: scan .NET project files, service/repository layers, DTOs, exception handlers.
   - For frontend: scan Vue components, stores (Pinia), API modules, composables, utilities.
   - Extract dependency inventories from package.json, .csproj, and lock files.

4. Build evidence collection:
   - Identify utility function implementations and detect duplicates/near-duplicates.
   - Extract error handling patterns and identify missing catch blocks or null checks.
   - Scan type definitions for incomplete or conflicting schemas.
   - Analyze dependency trees for version conflicts, CVE flags, or orphaned packages.
   - Map code structure against declared module organization.

### Phase 3: Analysis and Finding Generation
5. Execute analysis rules:

   **a) Code Redundancy Detection**
   - Identify functions/methods with >80% code similarity to existing utilities.
   - Flag repeated error handling patterns that should be extracted.
   - Detect near-duplicate component/template logic.
   - Recommend utility extraction or base class consolidation.

   **b) Potential Bug Detection**
   - Scan for missing error handling in async/await chains.
   - Identify uninitialized state or reactive variables.
   - Detect race conditions in concurrent operations or state mutations.
   - Flag type mismatches between declared and inferred types.
   - Identify potential null/undefined access paths.

   **c) Specification Compliance Audit**
   - Cross-reference naming conventions against code-rules document.
   - Verify comment/doc compliance for functions, classes, complex logic.
   - Check formatting compliance (indentation, line length, bracket placement).
   - Validate exception handling against declared error contract.
   - Verify response schema alignment with unified response body definition.

   **d) Dependency Configuration Review**
   - Detect version conflicts and transitive dependency overlaps.
   - Identify vulnerable packages (cross-reference against CVE feeds if available).
   - Flag unused or rarely-used dependencies.
   - Recommend consolidation or removal.
   - Assess bundle size impact of included packages.

   **e) Project Structure Assessment**
   - Evaluate module naming consistency against declared conventions.
   - Verify module boundaries and layer separation.
   - Identify orphaned or misplaced modules.
   - Flag circular dependencies or cross-layer violations.

   **f) Performance and Scalability Issues**
   - Identify inefficient algorithms or query patterns.
   - Flag excessive DOM manipulation or re-renders in Vue components.
   - Detect memory leaks or resource leaks in lifecycle hooks.
   - Recommend pagination or lazy-loading patterns.

   **g) Test Coverage Assessment**
   - Identify critical functions without unit tests.
   - Flag integration points without tests.
   - Recommend edge case coverage.

   **h) Security and Compliance Violations**
   - Scan for hardcoded credentials, API keys, or secrets.
   - Identify missing input validation or XSS prevention.
   - Flag insufficient RBAC checks or missing authz attributes.
   - Detect unencrypted sensitive data storage.

### Phase 4: Finding Classification and Prioritization
6. Classify findings:
   - **Severity**: critical (blocks deployment), high (major risk), medium (optimization), low (nice-to-have).
   - **Category**: code-redundancy, bug-risk, compliance-gap, dependency-issue, structure-issue, performance, security, test-gap.
   - **Effort**: quick-fix (< 1 hour), standard (1-4 hours), complex (> 4 hours).
   - **Priority**: high (fix before release), medium (fix in current sprint), low (backlog).

7. Cross-reference with baseline/rule docs:
   - For each finding, cite which rule/standard it violates.
   - Provide specific remediation steps aligned with declared patterns.

### Phase 5: Report Generation and Governance Integration
8. Generate analysis report:
   - Organize findings by category and severity.
   - Include executive summary (top 5-10 issues, refactoring roadmap).
   - Provide detailed finding sections with evidence, impact, and remediation.
   - Include metrics: finding count by severity/category, coverage gaps, refactoring effort estimate.
   - Generate action items and priority roadmap.

9. Handle ActionMode:
   - `check-only`: Report findings only; no artifact creation.
   - `plan`: Produce finding summary, severity distribution, placement, and generation steps only.
   - `apply`: Write full analysis report and update registry references.

10. Emit unified output contract and follow-up actions.

## Decision Logic

### Severity Classification
- **Critical**: Security vulnerabilities, data corruption risks, crashes, broken compliance.
- **High**: Major architectural violations, missing error handling in critical paths, dependency conflicts.
- **Medium**: Code redundancy, performance inefficiencies, incomplete test coverage, refactoring opportunities.
- **Low**: Code style, documentation gaps, optimization suggestions.

### Confidence and Evidence Priority
- **High Confidence**: Direct evidence from source code inspection, duplicate detection by structure matching, explicit type mismatches.
- **Medium Confidence**: Inferred from patterns, heuristic analysis, incomplete type information.
- **Low Confidence**: Speculative issues, unverified warnings, context-dependent findings.

### Remediation Recommendation Selection
- Align recommendations with declared standards (from code-rules document).
- Prioritize refactoring that consolidates utilities or extracts patterns.
- Suggest architectural changes only when clear violations of declared design are present.
- Provide template examples aligned with project style.

## Output Contract
Use this exact field order for parser stability across AutoCodeForge analysis skills.

```
ComplianceStatus: compliant | noncompliant | warning
AnalysisScope: backend | frontend | fullstack
AnalysisDepth: quick | standard | thorough
SourceDocumentsAnalyzed: [list of base/rule documents parsed]
FindingCount: { critical: int, high: int, medium: int, low: int, total: int }
CoverageMetrics: { codeInspected: percentage, dependenciesReviewed: percentage, testCoverageGap: percentage }
RefactoringEffortEstimate: { hours: int, priority: [list of high-priority items] }

LifecycleDecision: live | history-candidate | history | deprecated
LiveStatus: live | history
RecommendedPath: target folder path for created analysis report
PlacementReason: concise reason for selected path or status
CanonicalFile: optional, selected live analysis file when canonical resolution is required
TemplateType: analysis-report | governance-audit | code-review-gate | refactoring-roadmap
TemplateCatalogPath: .autoCodeForge/templates/README.md
ArchiveRecordTemplatePath: .autoCodeForge/templates/TEMPLATE_History_Archive_Record.md
ArchiveTargetPath: required for history-candidate/history outputs

NamingCompliance: compliant | noncompliant
ProposedFileName: required when naming is noncompliant
RequiredKeywords: list of metadata keywords for the analysis report
RequiredMetadataFields: required when metadata completeness check applies

DecisionEvidence:
  - analysis-scope-evidence: signals from project structure and configuration
  - source-document-discovery: which base/rule documents were located and parsed
  - code-inspection-summary: overview of code paths analyzed
  - finding-classification: methodology for severity/category assignment

ActionMode: check-only | plan | apply
PlannedActions: ordered list of analyze/report-generate/registry-update/trace-update actions
RegistryUpdatesRequired: true | false
RequiredFollowUps:
  - registry update in .autoCodeForge/registry/artifacts-index.md when location/status changed
  - trace update in .autoCodeForge/registry/trace-map.md when provenance changed
  - source document versioning when base/rule docs should be archived
  - priority alignment with MasterPlan if refactoring roadmap is included

RiskLevel: low | medium | high

FindingsReport: [structured findings section with fields below for each finding]
  - FindingID: unique identifier (e.g., OPINION-001)
  - Severity: critical | high | medium | low
  - Category: code-redundancy | bug-risk | compliance-gap | dependency-issue | structure-issue | performance | security | test-gap
  - Title: concise finding title
  - Description: detailed description of the issue
  - Evidence: specific code locations, document references, or patterns that support finding
  - SourceStandard: reference to rule or baseline doc that this violates (if applicable)
  - RemediationSteps: numbered list of specific action steps to address issue
  - RemediationTemplate: code example or pattern to follow for remediation
  - PriorityLevel: high | medium | low
  - EstimatedEffort: quick-fix | standard | complex
  - BlocksDeploy: true | false
  - DependentIssues: list of related FindingIDs that should be addressed together
  - References: links to inline documentation, patterns, or examples

SummaryRecommendations: executive summary of top 5-10 issues and refactoring roadmap
RefactoringRoadmap: priority-ordered list of recommended refactoring initiatives
```

## Completion Checks
1. Source documents are discovered and parsed successfully.
2. Analysis findings are classified with severity, category, and evidence.
3. Each finding includes remediation steps aligned with declared standards.
4. Output contract field order is unchanged.
5. ActionMode behavior is explicitly defined.
6. Analysis is traceable to source documents and code locations.
7. Registry and trace updates are specified in RequiredFollowUps.

## Example Prompts

### Scenario 1: Standard Code Quality Audit (Default)
```
/code-opinion-analyze-skill
```
- Analyzes auto-detected base-development-doc and code-rule outputs.
- Reports findings in standard detail with medium depth.
- Outputs full analysis report to `docs/CodeOpinion-Analysis-v1.0.0-{Date}.md`.
- Includes compliance audit, redundancy detection, and refactoring roadmap.

### Scenario 2: Pre-Review Code Gate (Executive Summary)
```
/code-opinion-analyze-skill ActionMode=check-only analysisDepth=thorough reportStyle=executive-summary audience=code-review
```
- Performs thorough inspection of all code layers.
- Reports top findings only (critical + high severity).
- No output file; findings displayed in chat for review gate decision.

### Scenario 3: Backend Refactoring Roadmap (Governance Archive)
```
/code-opinion-analyze-skill ActionMode=apply docPurpose=refactoring-roadmap analysisLayers=backend audience=governance lifecycle=history
```
- Focuses analysis on backend layer (.NET, C#, services, repositories).
- Generates refactoring roadmap with effort estimates and priority sequencing.
- Archives report to `.autoCodeForge/docs/CODE_OPINION_REFACTORING_BACKEND_{Date}.md`.
- Updates registry and trace maps.

### Scenario 4: Security and Compliance Deep Audit
```
/code-opinion-analyze-skill ActionMode=plan analysisDepth=thorough docPurpose=compliance-audit audience=governance
```
- Performs security scanning, CVE detection, RBAC audit, input validation checks.
- Identifies compliance violations (hardcoded secrets, missing authz, XSS risks).
- Produces audit-grade report with evidence collection and remediation blocking.
- Plans governance registry updates without implementation.

### Scenario 5: Dependency Configuration Review (Quick Scan)
```
/code-opinion-analyze-skill analysisDepth=quick docPurpose=dependency-review sourceDocuments=docs/BaseDevelopment-DevBaseline-v1.0.0-{Date}.md
```
- Analyzes only package manifests and dependency inventory from specified baseline doc.
- Detects version conflicts, vulnerable packages, and unused dependencies.
- Recommends consolidation and version pinning.
- Outputs quick findings in checklist format.

### Scenario 6: Test Coverage Gap Analysis (Team Onboarding)
```
/code-opinion-analyze-skill docPurpose=test-coverage-analysis audience=onboarding reportStyle=checklist analysisLayers=fullstack
```
- Identifies untested code paths, integration points, and edge cases.
- Generates checklist of tests to implement as onboarding task.
- Provides template test cases aligned with project patterns.
- Routes to `docs/TestCoverage-Action-Items-v1.0.0-{Date}.md`.

---

## Integration with AutoCodeForge Workflow

### Post-Skill Dependency Chain
1. **base-development-doc-skill** generates development baseline and declares CRUD patterns, exception handling contract.
2. **code-rule-skill** generates code quality rules and defines naming, comment, formatting standards.
3. **code-opinion-analyze-skill** executes post-analysis to validate code and configurations against declared standards (from steps 1-2), identifies gaps, and recommends remediation.

### Governance Registry Integration
- Findings reference source documents (base-development-doc, code-rule).
- Analysis report is registered in `.autoCodeForge/registry/artifacts-index.md`.
- Trace map updated in `.autoCodeForge/registry/trace-map.md` to show provenance: analysis → findings → source standards.
- Priority issues are surfaced to MasterPlan sync for roadmap alignment.

### Feedback Loop for Spec Evolution
- If analysis finds patterns that violate stated rules, flag as `SpecViolation` in findings.
- If analysis finds undeclared patterns in code that should be standardized, flag as `MissingStandard`.
- Provide summary of `SpecViolation` and `MissingStandard` counts for input to spec-conflict-triage workflow.

---

## Notes for Implementors

### Analysis Scope Boundaries
- Focus analysis on user-written code; exclude generated boilerplate, vendored code, and test utilities by default.
- Provide option to include/exclude specific paths or patterns.

### Remediation Template Quality
- Remediation templates should be copy-paste ready and aligned with project style.
- Include inline comments explaining why the change is needed.

### Performance Considerations
- For large codebases, consider incremental analysis (analyze changed files only) and cached results.
- Provide option to parallelize analysis across code layers/modules.

### Actionability
- Every finding must have a clear, implementable remediation step.
- Findings with low actionability (speculative, context-dependent) should be flagged with lower confidence.
