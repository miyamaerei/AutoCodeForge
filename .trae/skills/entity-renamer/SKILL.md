---
name: "entity-renamer"
description: "Renames Task-related entities to WorkItem/Workflow naming convention. Invoke when user asks to rename entities, refactor entity names, or reorganize naming structure. For full-stack rename including DTO/Service/Repo/Endpoint/Frontend, use full-stack-renamer skill instead."
---

# Entity Renamer

> **Note**: For complete full-stack rename (Entity + DTO + Service + Repository + Endpoint + Frontend), use the **`full-stack-renamer`** skill instead.
>
> This skill only covers entity-layer rename.

This skill renames Task-related entities to a cleaner naming convention.

## Rename Mapping

| Current Name | New Name |
|-------------|----------|
| `TaskEntity` | `WorkItemEntity` |
| `TaskStepEntity` | `WorkflowStepEntity` |
| `TaskReviewEntity` | `WorkflowStepAuditEntity` |
| `ReviewTaskEntity` | `CodeReviewStepEntity` |
| `ScheduledTaskExecutionEntity` | `ScheduledRunEntity` |
| `TaskLogEntity` | `WorkItemLogEntity` |

## File Rename Mapping

Rename both class name AND file name for each entity:

| Old File Name | New File Name | Old Class | New Class |
|--------------|---------------|-----------|-----------|
| `TaskEntity.cs` | `WorkItemEntity.cs` | `TaskEntity` | `WorkItemEntity` |
| `TaskStepEntity.cs` | `WorkflowStepEntity.cs` | `TaskStepEntity` | `WorkflowStepEntity` |
| `TaskReviewEntity.cs` | `WorkflowStepAuditEntity.cs` | `TaskReviewEntity` | `WorkflowStepAuditEntity` |
| `ReviewTaskEntity.cs` | `CodeReviewStepEntity.cs` | `ReviewTaskEntity` | `CodeReviewStepEntity` |
| `ScheduledTaskExecutionEntity.cs` | `ScheduledRunEntity.cs` | `ScheduledTaskExecutionEntity` | `ScheduledRunEntity` |
| `TaskLogEntity.cs` | `WorkItemLogEntity.cs` | `TaskLogEntity` | `WorkItemLogEntity` |

## Execution Steps

### Step 1: Find All Related Files
Search for files containing each old entity name:
```powershell
grep -r "TaskEntity" --include="*.cs"
grep -r "TaskStepEntity" --include="*.cs"
grep -r "TaskReviewEntity" --include="*.cs"
grep -r "ReviewTaskEntity" --include="*.cs"
grep -r "ScheduledTaskExecutionEntity" --include="*.cs"
grep -r "TaskLogEntity" --include="*.cs"
```

### Step 2: Rename Physical Files
Rename each entity file using git mv to preserve history:
```powershell
git mv TaskEntity.cs WorkItemEntity.cs
git mv TaskStepEntity.cs WorkflowStepEntity.cs
git mv TaskReviewEntity.cs WorkflowStepAuditEntity.cs
git mv ReviewTaskEntity.cs CodeReviewStepEntity.cs
git mv ScheduledTaskExecutionEntity.cs ScheduledRunEntity.cs
git mv TaskLogEntity.cs WorkItemLogEntity.cs
```

### Step 3: Update Class Names and Table Attributes
For each renamed file:
1. Rename the class declaration
2. Update `[SugarTable("...")]` to new table name
3. Update namespace if following folder structure

### Step 4: Update All References
For each file containing old names:
1. Replace class name references (type declarations, method parameters, variables)
2. Update using statements/imports to new namespaces
3. Update file path references in any configuration files

### Step 5: Update Related DTO/Service/Controller Files
If DTOs or services share the old name, rename them too:
| Old DTO/Service Name | New DTO/Service Name |
|---------------------|---------------------|
| `TaskStepResponse.cs` | `WorkflowStepResponse.cs` |
| `TaskReviewResponse.cs` | `WorkflowStepAuditResponse.cs` |
| `ReviewTaskResponse.cs` | `CodeReviewStepResponse.cs` |
| ... (add as needed based on Step 1 search results) |

### Step 6: Verify Consistency
After renaming:
1. Search for any remaining old names: `grep -r "TaskEntity\|TaskStepEntity\|TaskReviewEntity\|ReviewTaskEntity\|ScheduledTaskExecutionEntity\|TaskLogEntity" --include="*.cs"`
2. Verify all namespaces are correct
3. Build project to check for compilation errors

## Example: Renaming TaskEntity to WorkItemEntity

### Step 1: Rename File
```powershell
git mv TaskEntity.cs WorkItemEntity.cs
```

### Step 2: Update Class
```csharp
// File: WorkItemEntity.cs (renamed from TaskEntity.cs)
[SugarTable("WorkItems")]  // Table name also changed
public class WorkItemEntity : UserOwnedEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; }
}
```

### Step 3: Update All References
Search and replace:
- `TaskEntity` → `WorkItemEntity`
- `"Tasks"` → `"WorkItems"` (in `[SugarTable]`)
- File path `TaskEntity.cs` → `WorkItemEntity.cs`

## Important Notes

- **Database Migration**: If database already exists, consider creating a migration script to rename tables
- **Backup**: Ensure code is committed before starting rename refactoring
- **Incremental**: Rename one entity at a time to reduce risk
- **Test**: Build and run tests after each entity rename to catch issues early
