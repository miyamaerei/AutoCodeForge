# AUDIT LOG TEMPLATE

## 0. Document Governance Metadata
- DocumentPath: .autoCodeForge/logs/audits/<file>.md
- TargetFolder: .autoCodeForge/logs/audits/
- NamingRuleRef: .autoCodeForge/config/naming-rules.md
- RecommendedFileNamePattern: AUDIT_<YYYYMMDD>_<SEQ3>.md
- Keywords: log, audit, compliance, finding
- ArtifactType: log-audit
- Lifecycle: live | history
- RegistryIndexRef: .autoCodeForge/registry/artifacts-index.md
- TraceMapRef: .autoCodeForge/registry/trace-map.md
- ArchiveRuleRef: .autoCodeForge/config/archive-rules.md
- HistoryPathWhenArchived: .autoCodeForge/history/docs/
- SourceTemplate: TEMPLATE_Log_Audit.md

## 1. Meta
- LogName: AUDIT_<YYYYMMDD>_<SEQ3>.md
- DateTime: <YYYY-MM-DD HH:mm:ss>
- Auditor: <name>
- Scope: <folders/files>

## 2. Audit Checklist
- Structure integrity:
- Naming compliance:
- Registry consistency:
- Archive consistency:

## 3. Findings
- Severity: P0 | P1 | P2 | P3
- Finding:
- Evidence:
- Recommended fix:

## 4. Compliance Decision
- Overall: PASS | WARN | FAIL
- Blockers:
- Warnings:

## 5. Corrective Actions
- Action:
- Owner:
- Due date:
- Status:
