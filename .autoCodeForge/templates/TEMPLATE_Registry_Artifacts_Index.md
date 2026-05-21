# ARTIFACTS INDEX TEMPLATE

## 0. Document Governance Metadata
- DocumentPath: .autoCodeForge/registry/artifacts-index.md
- TargetFolder: .autoCodeForge/registry/
- NamingRuleRef: .autoCodeForge/config/naming-rules.md
- RecommendedFileNamePattern: artifacts-index.md
- Keywords: registry, artifacts, index, governance
- ArtifactType: registry-index
- Lifecycle: live
- RegistryIndexRef: self
- TraceMapRef: .autoCodeForge/registry/trace-map.md
- ArchiveRuleRef: .autoCodeForge/config/archive-rules.md
- HistoryPathWhenArchived: .autoCodeForge/history/docs/
- SourceTemplate: TEMPLATE_Registry_Artifacts_Index.md

| name | type | status | location | owner | version | date |
|------|------|--------|----------|-------|---------|------|
| <artifact-name> | <report/plan/spec/log/template/doc> | <live/history/deprecated> | <relative path> | <owner> | <version> | <YYYY-MM-DD> |

## Notes
- Keep this table updated in the same change set as file creation, move, or archive.
- location must point to the current real path.
