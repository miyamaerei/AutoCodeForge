# AutoCodeForge Template Catalog

This folder stores canonical templates used by governance skills.

## Document Management Principles
- Every document generated from templates must carry explicit governance metadata.
- Every document must be traceable by path, naming rule, keywords, and lifecycle status.
- Every live document must have index linkage and archive routing defined.

## Mandatory Metadata Checklist
All templates in this folder must include and fill a `Document Governance Metadata` section with:
- DocumentPath
- TargetFolder
- NamingRuleRef
- RecommendedFileNamePattern
- Keywords
- ArtifactType
- Lifecycle
- RegistryIndexRef
- TraceMapRef
- ArchiveRuleRef
- HistoryPathWhenArchived
- SourceTemplate

## Template List
- TEMPLATE_File_Documentation.md
- TEMPLATE_API_Config_Spec.md
- TEMPLATE_Process_Guide.md
- TEMPLATE_Config_Rule.md
- TEMPLATE_Report_Round.md
- TEMPLATE_Report_Daily.md
- TEMPLATE_Report_Phase.md
- TEMPLATE_Plan_Active.md
- TEMPLATE_Plan_Archived.md
- TEMPLATE_Spec_Current.md
- TEMPLATE_Spec_Change_Request.md
- TEMPLATE_Log_Execution.md
- TEMPLATE_Log_Audit.md
- TEMPLATE_Registry_Artifacts_Index.md
- TEMPLATE_Registry_Trace_Map.md
- TEMPLATE_History_Archive_Record.md

## Selection Rules
- If target path is .autoCodeForge/reports/round/, use TEMPLATE_Report_Round.md.
- If target path is .autoCodeForge/reports/daily/, use TEMPLATE_Report_Daily.md.
- If target path is .autoCodeForge/reports/phase/, use TEMPLATE_Report_Phase.md.
- If target path is .autoCodeForge/logs/execution/, use TEMPLATE_Log_Execution.md.
- If target path is .autoCodeForge/logs/audits/, use TEMPLATE_Log_Audit.md.
- If target path is .autoCodeForge/plans/active/, use TEMPLATE_Plan_Active.md.
- If target path is .autoCodeForge/plans/archived/, use TEMPLATE_Plan_Archived.md.
- If target path is .autoCodeForge/specs/current/, use TEMPLATE_Spec_Current.md.
- If target path is .autoCodeForge/specs/changes/, use TEMPLATE_Spec_Change_Request.md.
- If target path is .autoCodeForge/registry/artifacts-index.md, use TEMPLATE_Registry_Artifacts_Index.md.
- If target path is .autoCodeForge/registry/trace-map.md, use TEMPLATE_Registry_Trace_Map.md.
- If target path is .autoCodeForge/config/, use TEMPLATE_Config_Rule.md.
- If target is a file-level technical doc, use TEMPLATE_File_Documentation.md.
- If target is api/config contract doc, use TEMPLATE_API_Config_Spec.md.
- If target is process/how-to doc, use TEMPLATE_Process_Guide.md.

## Naming Rule Linkage
- Naming rules reference: .autoCodeForge/config/naming-rules.md
- Archive rules reference: .autoCodeForge/config/archive-rules.md
- When file name does not satisfy naming rules, generate rename-plan before publishing as live.
