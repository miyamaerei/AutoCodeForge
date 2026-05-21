# HISTORY ARCHIVE RECORD TEMPLATE

## 0. Document Governance Metadata
- DocumentPath: .autoCodeForge/history/<type>/<file>.md
- TargetFolder: .autoCodeForge/history/
- NamingRuleRef: .autoCodeForge/config/naming-rules.md
- RecommendedFileNamePattern: <original>-history-v<major>.<minor>.<patch>.md
- Keywords: history, archive, record, lifecycle
- ArtifactType: history-archive-record
- Lifecycle: history
- RegistryIndexRef: .autoCodeForge/registry/artifacts-index.md
- TraceMapRef: .autoCodeForge/registry/trace-map.md
- ArchiveRuleRef: .autoCodeForge/config/archive-rules.md
- HistoryPathWhenArchived: .autoCodeForge/history/
- SourceTemplate: TEMPLATE_History_Archive_Record.md

## 1. Meta
- ArchiveDate: <YYYY-MM-DD>
- Operator: <name>
- Trigger: new-version | duplicate | deprecated | monthly-cleanup

## 2. Source and Target
- Source path:
- Target path:
- Original file name:
- Archived file name:

## 3. Reason
- Why archived:
- Canonical live file:
- Evidence:

## 4. Validation
- Q5 non-overlap check: PASS | WARN | FAIL
- Registry updated: yes | no
- Trace map updated: yes | no
