# Settings Configuration Requirements

This document defines what should be filled in each setting section for the console configuration pages.

## Preferences

Purpose: Define workspace-level defaults used by UI and agent runtime.

Required fields:
- Language and locale (example: zh-CN, en-US)
- Timezone (IANA format, example: Asia/Shanghai)
- UI theme mode (light, dark, auto)
- Date and time display format
- Number format and separator rules
- Default workspace landing page
- Notification defaults (email, in-app, webhook)
- Safe mode flags (confirm before destructive operations)

Validation rules:
- Locale must be from supported locale list.
- Timezone must be valid IANA timezone.
- Theme mode must be one of light, dark, auto.

## Repositories

Purpose: Manage source code repository integrations and governance defaults.

Required fields:
- Repository identity (owner, repo name)
- Hosting provider (GitHub, Azure DevOps, GitLab)
- Authentication method (token, app, oauth)
- Default branch (example: main)
- Protected branch policy settings
- Pull request template selection
- Required status checks list
- Allowed merge strategies (merge, squash, rebase)

Validation rules:
- Owner and repo are required and unique per provider.
- Default branch cannot be empty.
- Merge strategies must include at least one option.

## Knowledge

Purpose: Configure knowledge sources used for retrieval and context assembly.

Required fields:
- Knowledge source name and type
- Source location (path, URL, or connector identifier)
- Content format (markdown, docs, wiki, code)
- Refresh policy (manual, hourly, daily)
- Chunking strategy (size, overlap)
- Indexing scope (branches, folders, tags)
- Access control level (public, internal, restricted)
- Freshness SLA and last sync timestamp

Validation rules:
- Source location must be reachable or testable.
- Refresh policy must define cadence if not manual.
- Chunk size and overlap must be positive integers.

## Skill

Purpose: Configure skill behavior, safety bounds, and execution controls.

Required fields:
- Skill name and version
- Enable flag per skill
- Runtime mode (strict, balanced, exploratory)
- Timeout in seconds
- Max retries and backoff policy
- Allowed tools list
- Forbidden operations list
- Prompt template or instruction reference

Validation rules:
- Timeout must be within approved range (for example 10-300 seconds).
- Retry count must be non-negative.
- Allowed tools must be explicit for restricted mode.

## Schedules

Purpose: Define recurring jobs for sync, reports, and maintenance automation.

Required fields:
- Schedule name
- Cron expression
- Timezone
- Job target (workflow, pipeline, sync task)
- Enable flag
- Failure retry policy
- Alert channel on failure
- Last run and next run timestamps

Validation rules:
- Cron expression must parse correctly.
- Timezone must be valid.
- Job target must map to an existing executable task.

## DeepWiki

Purpose: Configure DeepWiki indexing and retrieval behavior.

Required fields:
- Workspace or project identifier
- Index name
- Embedding model
- Vector dimension and distance metric
- Retrieval top-k default
- Source include and exclude patterns
- Re-index trigger policy
- Retention period and cleanup policy

Validation rules:
- Index name must be unique within workspace.
- Embedding model must be available in current environment.
- Top-k must be positive integer and within system limits.

## Review

Purpose: Define code review quality gates and pull request guardrails.

Required fields:
- Minimum required approvals
- Required reviewer roles
- Block merge on failing checks (true or false)
- Required checks list (CI, test, lint, security)
- Code owner enforcement (true or false)
- SLA target for first review response
- Auto-assignment rules
- Exception process and approver roles

Validation rules:
- Minimum approvals must be integer >= 0.
- If blocking on checks is true, required checks list cannot be empty.
- Exception process must identify at least one approver role.

## Suggested Common Metadata For All Sections

- Environment scope (dev, test, prod)
- Updated by and updated at
- Change ticket or audit reference
- Rollback guidance
- Data sensitivity classification
