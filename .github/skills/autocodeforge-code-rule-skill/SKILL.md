---
name: code-rule-skill
description: 'Scan the current project and generate unified code quality governance documentation. Use for 代码命名规范, 注释规范, 格式规范, 代码审查规则, and code quality standards documentation.'
argument-hint: 'Fully optional. Call with no params for default plan mode, or override ActionMode, docPurpose, audience, targetLayer as needed. Auto-detects project signals from current folder.'
---

# CodeRuleSkill (Code Quality Governance Skill)

## Template Lineage
- This skill is created from the `autocodeforge-doc-management` template.
- Section order and output contract follow the canonical doc-management template for parser stability.
- Inherits governance patterns from `base-development-doc-skill` and adapts them for code quality rules.

## Core Principle
- This skill is documentation-first, not enforcement-first.
- It scans the current repository codebase and existing rule documents, then generates unified code quality governance documentation.
- It does not execute linting, formatting, or code transformation; it only outputs governed documentation.
- Generated documents must themselves comply with the rules they define (self-consistent governance).

## What This Skill Produces
- A comprehensive code rule governance document for the current project.
- Structured documentation sections for:
  - Naming conventions (variables, functions, classes, constants, files, etc.).
  - Comment and documentation standards (file headers, function docs, inline comments, JSDoc/XML Doc patterns).
  - Code formatting rules (indentation, spacing, line breaks, bracket placement, etc.).
  - Code review rules and checklist patterns.
  - Linting and code quality tool configurations.
  - Language-specific and framework-specific conventions.
  - Compliance audit criteria and verification methods.
- Traceable output aligned with AutoCodeForge governance contract.

## When to Use
- You need one governed document that defines code quality standards before implementation.
- You want to standardize and unify coding practices across frontend (Vue 3/TypeScript), backend (.NET/C#), or full-stack teams.
- You need onboarding and governance evidence for code quality conventions.
- You're establishing a baseline for code review and compliance checks.

## Required Inputs
None. All parameters are optional and auto-detected.

## Optional Inputs (all auto-detected if omitted)
1. `ActionMode`: `check-only` | `plan` | `apply` (default: `plan`).
2. `docPurpose`: e.g. `code-governance` | `backend-code-rules` | `frontend-code-rules` | `fullstack-code-rules` (default: `code-governance`).
3. `audience`: `engineering` | `onboarding` | `governance` | `code-review` (default: auto-detected).
4. `targetLayer`: `backend` | `frontend` | `fullstack` (default: auto-detected from project structure).
5. `lifecycle`: `live` (default) | `history`.
6. `outputFormat`: `markdown` (default) | `structured-json`.
7. `includeToolConfig`: `true` | `false` (default: `true` if tool configs exist).
8. `enforcementLevel`: `strict` | `standard` | `advisory` (default: `standard`).

## Governance Requirements

### 1. Document Placement (Reference: autocodeforge-doc-placement)

- Route output to `.autoCodeForge/docs/` for governance-centric usage.
- Placement and lifecycle decision must be explicit and traceable.

### 2. Naming and Metadata Compliance
- Product-facing naming pattern: `{ProjectName}-CodeRules-v{Version}-{Date}.md` or `code-rules-{layer}-v{Version}-{Date}.md`.
- Governance naming pattern: `CODE_RULES_{PURPOSE}_{DATE}_{SEQUENCE}.md`.
- Include metadata block: document path, purpose, keywords, lifecycle, archive target, version.
- Required keywords should include at least 5 of: `naming`, `comment`, `formatting`, `review-rule`, `typescript`, `csharp`, `vue3`, `linting`, `compliance`, `audit`.
- Generated document MUST follow the naming, comment, and formatting rules it defines (self-compliance).

### 3. Output Contract Consistency
- Keep the same field order used by initialization and governance skills.
- Ensure parser-stable field names and enumerated values.
- Each rule section must include: Rule Name, Severity, Description, Example (compliant), Example (non-compliant), Verification Method, Tool Support.

### 4. Safety and Auditability
- `check-only`: no file mutation, findings only about current project's code compliance status.
- `plan`: deterministic plan and placement decision, no file mutation.
- `apply`: create or update code rules documentation and related registry references only.
- Must not install linters, formatters, or modify business source files.
- Must not execute code transformations or apply fixes.

## Workflow

### Phase 1: Project Scanning and Language Detection
1. Auto-detect project context:
   - Scan `client/`, `server/`, root `package.json`, root `.sln`, `tsconfig.json`, `.editorconfig`, `.eslintrc.*`, etc.
   - Detect tech stack signals: Vue 3, TypeScript, .NET/C#, JavaScript, Python, etc.
   - Identify linting/formatting tools: ESLint, Prettier, Stylus, .NET code analyzers, etc.

2. Collect existing rule evidence:
   - Search for existing `.eslintrc`, `.prettierrc`, `editorconfig`, `.ruleset`, `stylecop.json` files.
   - Extract comments from existing code style documentation and code compliance checklists.
   - Identify existing naming patterns and conventions used in codebase.

### Phase 2: Rule Extraction and Analysis
3. Build rule sections by domain:

   **a) Naming Conventions**
   - Variables: camelCase, snake_case, UPPER_CASE context rules.
   - Functions/Methods: camelCase for JavaScript/TypeScript, PascalCase for C#.
   - Classes/Types: PascalCase rules, interface naming (I-prefix for C#, no prefix for TypeScript).
   - Constants: UPPER_SNAKE_CASE rules.
   - Files/Modules: naming patterns for `.ts`, `.vue`, `.cs`, `.tsx`, etc.
   - Database entities and API routes.

   **b) Comment and Documentation Standards**
   - File header format (license, purpose, author, date).
   - Function/method documentation (JSDoc for JS/TS, XML Doc for C#).
   - Parameter and return value documentation.
   - Inline comment rules and when to use them.
   - TODO/FIXME/HACK comment formats.
   - Vue 3 component and prop documentation.
   - API endpoint documentation.

   **c) Code Formatting Rules**
   - Indentation: spaces vs. tabs, size (2 or 4).
   - Line length limits.
   - Bracket placement and spacing.
   - Comma, semicolon, and operator spacing.
   - Import statement ordering and grouping.
   - Blank line rules.
   - Trailing whitespace handling.
   - Line ending (CRLF vs. LF).

   **d) Code Review Rules**
   - Mandatory review checklist items.
   - Security review criteria.
   - Performance review criteria.
   - Documentation completeness checks.
   - Test coverage requirements.
   - Breaking change detection.
   - API contract stability.

   **e) Language-Specific and Framework Conventions**
   - TypeScript: strict mode, type annotations, generics, etc.
   - Vue 3 Composition API: setup function patterns, ref vs. reactive, composables.
   - C#: async/await patterns, LINQ conventions, null handling.
   - CSS/SCSS: selector naming, nesting rules.
   - SQL: query patterns, optimization rules.

   **f) Linting and Tool Configuration**
   - ESLint rules and preset references.
   - Prettier configuration.
   - StyleLint configuration.
   - .NET Code Analyzers (Roslyn analyzers).
   - Null reference checks and warnings.
   - Suppression and exclusion rules.

### Phase 3: Governance Checks and Validation
4. Run governance checks:
   - Resolve placement path and lifecycle.
   - Validate naming against naming rules when available.
   - Build compliance and confidence assessment.
   - Check for tool config consistency across project.
   - Identify gaps in rule coverage.

### Phase 4: Mode-Specific Actions
5. Handle ActionMode:
   - `check-only`: report current project compliance status and coverage gaps only.
   - `plan`: produce section outline, placement, generation steps, and rule prioritization only.
   - `apply`: write/update document and report index update actions.

### Phase 5: Self-Compliance Validation
6. Ensure generated document is self-compliant:
   - Apply naming rules to the document filename.
   - Apply comment rules to document metadata and code examples.
   - Apply formatting rules to document structure and code blocks.
   - Validate document itself follows the rules it defines.

## Output Contract
Use this exact field order for parser stability across AutoCodeForge skills.

```yaml
ComplianceStatus: compliant | noncompliant | warning
LifecycleDecision: live | history-candidate | history | deprecated
LiveStatus: live | history
RecommendedPath: target folder path for created or validated artifacts
PlacementReason: concise reason for selected path or status
CanonicalFile: optional, selected live file when canonical resolution is required
TemplateType: optional, code-governance | rule-index | layer-specific | tool-config
TemplateCatalogPath: .autoCodeForge/templates/README.md
TemplatePath: optional, selected canonical template path
ArchiveRecordTemplatePath: .autoCodeForge/templates/TEMPLATE_History_Archive_Record.md
TemplateSkeleton: optional, only when mapped template is missing
RequiredMetadataFields: required when metadata completeness check applies
NamingCompliance: compliant | noncompliant
ProposedFileName: required when naming is noncompliant
RequiredKeywords: [naming, comment, formatting, review-rule, <language-specific>, linting, compliance, audit]
ArchiveTargetPath: required for history-candidate/history outputs
DecisionEvidence:
  - structure evidence: languages detected, layers identified, tool configs found
  - naming evidence: existing naming patterns observed, consistency assessment
  - tool evidence: linting/formatting tools detected and their configs
  - framework evidence: Vue 3, .NET, TypeScript versions, feature usage patterns
  - quality gate evidence: current compliance assessment
  - rule coverage evidence: domains covered (naming/comment/formatting/review-rules/tools)
ActionMode: check-only | plan | apply
PlannedActions: ordered list of create/validate/update-index/update-trace/align-contract actions
RulesByDomain:
  - domain: naming | comment | formatting | review-rules | language-specific | tools
    count: <number of rules in domain>
    severity: distribution of blocking/important/advisory rules
CoveragePercentage: percentage of codebase patterns with explicit rules
RegistryUpdatesRequired: true | false
RequiredFollowUps:
  - naming check against .autoCodeForge/config/naming-rules.md
  - registry update in .autoCodeForge/registry/artifacts-index.md when location/status changed
  - trace update in .autoCodeForge/registry/trace-map.md when provenance changed
  - tool config alignment check (ESLint, Prettier, code analyzers)
  - code review team alignment on enforced rules
RiskLevel: low | medium | high
```

## Completion Checks

1. ✓ Project scan completed with explicit stack and layer detection.
2. ✓ All requested domains are evaluated: naming, comment, formatting, review-rules, language-specific, tools.
3. ✓ Placement decision is explicit (`docs/` or `.autoCodeForge/docs/`).
4. ✓ Naming compliance result is explicit.
5. ✓ ComplianceStatus is explicit (`compliant | noncompliant | warning`).
6. ✓ Lifecycle is explicit (`live` or `history`).
7. ✓ Output Contract field order matches unified standard.
8. ✓ Metadata block includes path, purpose, keywords, lifecycle, archive target.
9. ✓ Generated document is self-compliant (follows its own rules).
10. ✓ In `apply` mode, target document is created/updated and registry follow-ups are listed.
11. ✓ Each rule includes Name, Severity, Description, Compliant Example, Non-Compliant Example, Verification Method.
12. ✓ Tool configurations (ESLint, Prettier, .NET Analyzers) are documented or referenced.

## Document Structure Template

The generated code rules document should follow this structure:

```markdown
# {ProjectName} Code Quality Governance

**Document Version**: v{Version}  
**Created/Updated**: {Date}  
**Applicable Project**: {ProjectName}  
**Target Layers**: {frontend|backend|fullstack}  
**Enforcement Level**: {strict|standard|advisory}

---

## 📋 Document Metadata

- **Purpose**: Unified code quality standards and governance
- **Keywords**: naming, comment, formatting, review-rule, {language-specific}
- **Lifecycle**: live
- **Archive Target**: .autoCodeForge/docs/CODE_RULES_history_{Date}_{Sequence}.md
- **Last Review**: {Date}

---

## Overview and Applicability

[Introduction explaining scope, applicability to teams, and purpose]

---

## 1. Naming Conventions

### 1.1 Variables
- Rule Name, Severity, Description, Examples, Verification

### 1.2 Functions/Methods
- Rule Name, Severity, Description, Examples, Verification

### 1.3 Classes and Types
- Rule Name, Severity, Description, Examples, Verification

### 1.4 Constants
- Rule Name, Severity, Description, Examples, Verification

### 1.5 Files and Modules
- Rule Name, Severity, Description, Examples, Verification

---

## 2. Comment and Documentation Standards

### 2.1 File Headers
- Rule Name, Severity, Description, Template, Verification

### 2.2 Function/Method Documentation
- Rule Name, Severity, Description, Template, Examples, Tool Support

### 2.3 Parameter and Return Documentation
- Rule Name, Severity, Description, Format, Verification

### 2.4 Inline Comments
- Rule Name, Severity, Description, Examples, When to Use

### 2.5 Special Comments (TODO, FIXME, HACK)
- Rule Name, Format, Severity, Verification

---

## 3. Code Formatting Rules

### 3.1 Indentation and Spacing
### 3.2 Line Length
### 3.3 Bracket and Brace Placement
### 3.4 Operator Spacing
### 3.5 Import Statement Organization
### 3.6 Blank Lines
### 3.7 Line Endings

---

## 4. Code Review Rules

### 4.1 Mandatory Review Checklist
### 4.2 Security Review Criteria
### 4.3 Performance Review Criteria
### 4.4 Documentation Completeness
### 4.5 Test Coverage Requirements
### 4.6 Breaking Change Detection

---

## 5. Language and Framework Specific Conventions

### 5.1 TypeScript Rules
### 5.2 Vue 3 Composition API Rules
### 5.3 C# and .NET Rules
### 5.4 SQL and Database Rules
### 5.5 CSS/SCSS Rules

---

## 6. Linting and Tool Configuration

### 6.1 ESLint Configuration
### 6.2 Prettier Configuration
### 6.3 StyleLint Configuration
### 6.4 .NET Code Analyzers
### 6.5 Tool Integration and CI/CD Enforcement

---

## 7. Compliance Audit and Verification

### 7.1 Audit Checklist
### 7.2 Tool-Based Verification
### 7.3 Manual Review Process
### 7.4 Remediation Workflow

---

## Appendix: Tool Configuration Examples

[Include actual tool config snippets for ESLint, Prettier, etc.]

---

## Revision History

| Version | Date | Changes |
|---------|------|---------|
| v1.0 | {Date} | Initial governance document |
```

## Example Prompts

- `/code-rule-skill`
- `/code-rule-skill ActionMode=check-only targetLayer=frontend`
- `/code-rule-skill ActionMode=plan docPurpose=backend-code-rules audience=engineering`
- `/code-rule-skill ActionMode=apply audience=code-review enforcementLevel=strict`
- `/code-rule-skill ActionMode=apply docPurpose=fullstack-code-rules outputFormat=markdown includeToolConfig=true`

## Integration with Other Skills

- **base-development-doc-skill**: Complements by adding code quality governance to development baseline.
- **autocodeforge-doc-management**: Provides template and lifecycle management.
- **init-project-skill**: Code rules can be generated as part of project initialization.
- **spec-conflict-triage**: May identify conflicts between code rules and project specifications.

## Self-Compliance Principle

Generated documents MUST:
1. Follow the naming rules they define (e.g., if defining `camelCase` for variables, use `camelCase` in examples).
2. Follow the comment rules they define (include proper documentation for all code examples).
3. Follow the formatting rules they define (indentation, line length, spacing, etc.).
4. Be grammatically correct and professionally written.
5. Include tool configuration examples that are syntactically valid.

This ensures the document serves as both governance and exemplar.
