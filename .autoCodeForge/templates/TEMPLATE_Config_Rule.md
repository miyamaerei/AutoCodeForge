# CONFIG RULE TEMPLATE

## 0. Document Governance Metadata
- DocumentPath: .autoCodeForge/config/<rule-file>.md
- TargetFolder: .autoCodeForge/config/
- NamingRuleRef: .autoCodeForge/config/naming-rules.md
- RecommendedFileNamePattern: <rule-name>.md
- Keywords: config, rule, policy, compliance
- ArtifactType: config-rule
- Lifecycle: live | history
- RegistryIndexRef: .autoCodeForge/registry/artifacts-index.md
- TraceMapRef: .autoCodeForge/registry/trace-map.md
- ArchiveRuleRef: .autoCodeForge/config/archive-rules.md
- HistoryPathWhenArchived: .autoCodeForge/history/specs/
- SourceTemplate: TEMPLATE_Config_Rule.md

## 1. Meta
- RuleName: <rule name>
- Version: <vMAJOR.MINOR.PATCH>
- Date: <YYYY-MM-DD>
- Owner: <team/person>

## 2. Rule Purpose
- Why this rule exists:
- Scope:
- Non-scope:

## 3. Rule Definition
- Rule item:
- Constraint:
- Example:
- Counter example:

## 4. Severity and Handling
- Severity: BLOCKER | WARNING | INFO
- Enforcement behavior:
- Exception process:

## 5. Change Control
- Change request required: yes | no
- Effective date:
- Review cycle:
