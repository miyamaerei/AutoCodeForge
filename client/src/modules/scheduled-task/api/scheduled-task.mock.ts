import type {
  ScheduledTaskDto,
  CreateScheduledTaskDto,
  UpdateScheduledTaskDto,
  ScheduledTaskExecutionDto,
  TaskTemplateDto,
  PagedResult,
} from './scheduled-task.types'

const defaultTasks: ScheduledTaskDto[] = [
  {
    id: 'st-1',
    name: '每日代码质量检查',
    triggerType: 'cron',
    cronExpression: '0 9 * * *',
    status: 'success',
    agentId: 'agent-1',
    input: '{"reportLevel":"detailed"}',
    taskTitle: '代码质量检查任务',
    nextRunAtUtc: '2026-05-20T09:00:00Z',
    createdAtUtc: '2026-05-01T10:00:00Z',
  },
  {
    id: 'st-2',
    name: '数据库备份',
    triggerType: 'interval',
    cronExpression: '',
    status: 'success',
    agentId: 'agent-3',
    input: '{"backupType":"full"}',
    taskTitle: '数据库备份任务',
    nextRunAtUtc: '2026-05-19T22:00:00Z',
    createdAtUtc: '2026-05-02T08:00:00Z',
  },
  {
    id: 'st-3',
    name: '周报自动生成',
    triggerType: 'cron',
    cronExpression: '0 18 * * 5',
    status: 'pending',
    agentId: 'agent-5',
    input: '{"template":"weekly","recipients":["team@example.com"]}',
    taskTitle: '周报生成任务',
    nextRunAtUtc: '2026-05-23T18:00:00Z',
    createdAtUtc: '2026-05-03T14:00:00Z',
  },
  {
    id: 'st-4',
    name: '性能监控报告',
    triggerType: 'interval',
    cronExpression: '',
    status: 'failed',
    agentId: 'agent-2',
    input: '{"alertThreshold":{"cpu":80,"memory":85}}',
    taskTitle: '性能监控任务',
    nextRunAtUtc: '2026-05-19T21:00:00Z',
    createdAtUtc: '2026-05-04T10:00:00Z',
  },
  {
    id: 'st-5',
    name: '临时数据清理',
    triggerType: 'once',
    cronExpression: '',
    status: 'pending',
    agentId: 'agent-3',
    input: '{"retentionDays":30,"dryRun":false}',
    taskTitle: '数据清理任务',
    nextRunAtUtc: '2026-05-25T02:00:00Z',
    createdAtUtc: '2026-05-05T11:00:00Z',
  },
]

const defaultTemplates: TaskTemplateDto[] = [
  {
    id: 'tpl-code-review',
    name: '代码审查任务',
    description: '自动化代码质量检查和安全扫描',
    agentId: 'agent-1',
    systemPromptOverride: '你是一个专业的代码审查助手，负责分析代码质量、发现潜在问题并提供改进建议。',
    triggerType: 'cron',
    cronExpression: '0 9 * * *',
    defaultParams: '{"reportLevel":"detailed","checkSecurity":true}',
    isBuiltIn: true,
  },
  {
    id: 'tpl-db-backup',
    name: '数据库备份任务',
    description: '定期执行数据库全量或增量备份',
    agentId: 'agent-3',
    triggerType: 'interval',
    intervalMs: 6 * 60 * 60 * 1000,
    defaultParams: '{"backupType":"full","compression":true}',
    isBuiltIn: true,
  },
  {
    id: 'tpl-doc-gen',
    name: '文档生成任务',
    description: '自动生成技术文档、周报、API 文档等',
    agentId: 'agent-5',
    systemPromptOverride: '你是一个技术文档专家，擅长撰写清晰、准确的技术文档。',
    triggerType: 'cron',
    cronExpression: '0 18 * * 5',
    defaultParams: '{"template":"weekly","format":"markdown"}',
    isBuiltIn: true,
  },
  {
    id: 'tpl-monitor',
    name: '系统监控任务',
    description: '监控系统性能指标，异常时发送告警',
    agentId: 'agent-2',
    triggerType: 'interval',
    intervalMs: 60 * 60 * 1000,
    defaultParams: '{"alertThreshold":{"cpu":80,"memory":85}}',
    isBuiltIn: true,
  },
  {
    id: 'tpl-refactor',
    name: '代码重构任务',
    description: '对指定代码进行重构优化',
    agentId: 'agent-4',
    systemPromptOverride: '你是一个重构专家，负责优化代码结构，提高可维护性和性能。',
    triggerType: 'once',
    defaultParams: '{"scope":"incremental","safetyLevel":"high"}',
    isBuiltIn: true,
  },
]

const defaultExecutions: ScheduledTaskExecutionDto[] = [
  {
    id: 'ex-1',
    scheduledTaskId: 'st-1',
    status: 'success',
    startedAtUtc: '2026-05-19T09:00:00Z',
    completedAtUtc: '2026-05-19T09:02:35Z',
    output: '代码质量检查完成，发现 3 个建议改进点，0 个严重问题',
  },
  {
    id: 'ex-2',
    scheduledTaskId: 'st-1',
    status: 'success',
    startedAtUtc: '2026-05-18T09:00:00Z',
    completedAtUtc: '2026-05-18T09:01:48Z',
    output: '代码质量检查完成，无问题',
  },
  {
    id: 'ex-3',
    scheduledTaskId: 'st-4',
    status: 'failed',
    startedAtUtc: '2026-05-19T20:00:00Z',
    completedAtUtc: '2026-05-19T20:00:12Z',
    output: '监控数据采集失败：连接超时',
  },
]

let tasksData: ScheduledTaskDto[] = [...defaultTasks]
let executionsData: ScheduledTaskExecutionDto[] = [...defaultExecutions]

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => window.setTimeout(resolve, ms))
}

export async function mockGetScheduledTasks(page: number, pageSize: number): Promise<PagedResult<ScheduledTaskDto>> {
  await wait(200)
  const start = (page - 1) * pageSize
  const end = start + pageSize
  return {
    items: tasksData.slice(start, end),
    totalCount: tasksData.length,
    page,
    pageSize,
  }
}

export async function mockGetScheduledTask(id: string): Promise<ScheduledTaskDto | null> {
  await wait(100)
  return tasksData.find((t) => t.id === id) ?? null
}

export async function mockCreateScheduledTask(dto: CreateScheduledTaskDto): Promise<ScheduledTaskDto> {
  await wait(300)
  const now = new Date().toISOString()
  const task: ScheduledTaskDto = {
    id: `st-${Date.now()}`,
    name: dto.name,
    triggerType: dto.triggerType || 'cron',
    cronExpression: dto.cronExpression,
    status: 'pending',
    agentId: dto.agentId,
    input: dto.input,
    taskTitle: dto.taskTitle,
    nextRunAtUtc: now,
    createdAtUtc: now,
  }
  tasksData.push(task)
  return task
}

export async function mockUpdateScheduledTask(id: string, dto: UpdateScheduledTaskDto): Promise<ScheduledTaskDto | null> {
  await wait(300)
  const index = tasksData.findIndex((t) => t.id === id)
  if (index === -1) return null
  const existing = tasksData[index]!
  const updated: ScheduledTaskDto = {
    ...existing,
    ...dto,
  }
  tasksData[index] = updated
  return updated
}

export async function mockDeleteScheduledTask(id: string): Promise<boolean> {
  await wait(200)
  const index = tasksData.findIndex((t) => t.id === id)
  if (index === -1) return false
  tasksData.splice(index, 1)
  executionsData = executionsData.filter((e) => e.scheduledTaskId !== id)
  return true
}

export async function mockGetExecutions(taskId: string, page: number, pageSize: number): Promise<PagedResult<ScheduledTaskExecutionDto>> {
  await wait(150)
  const filtered = executionsData.filter((e) => e.scheduledTaskId === taskId)
  const start = (page - 1) * pageSize
  const end = start + pageSize
  return {
    items: filtered.slice(start, end),
    totalCount: filtered.length,
    page,
    pageSize,
  }
}

export async function mockTriggerTask(taskId: string): Promise<ScheduledTaskExecutionDto> {
  await wait(500)
  const record: ScheduledTaskExecutionDto = {
    id: `ex-${Date.now()}`,
    scheduledTaskId: taskId,
    startedAtUtc: new Date().toISOString(),
    completedAtUtc: undefined,
    status: 'running',
    output: '执行中...',
  }
  executionsData.unshift(record)
  return record
}

export async function mockToggleTaskEnabled(id: string, enabled?: boolean): Promise<ScheduledTaskDto | null> {
  await wait(100)
  const task = tasksData.find((t) => t.id === id)
  if (!task) return null
  task.status = enabled === undefined ? (task.status === 'disabled' ? 'pending' : 'disabled') : (enabled ? 'pending' : 'disabled')
  return { ...task }
}

export async function mockGetTaskTemplates(): Promise<TaskTemplateDto[]> {
  await wait(100)
  return [...defaultTemplates]
}

export async function mockGetTaskTemplate(id: string): Promise<TaskTemplateDto | null> {
  await wait(50)
  return defaultTemplates.find((t) => t.id === id) ?? null
}