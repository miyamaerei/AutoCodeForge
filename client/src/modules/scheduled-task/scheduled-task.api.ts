/**
 * 定时任务 API 模块 - 提供定时任务的 CRUD 操作 Mock 实现
 */

export type TaskStatus = 'pending' | 'running' | 'success' | 'failed' | 'disabled'
export type TriggerType = 'cron' | 'interval' | 'once'

/** 关联的仓库信息 */
export interface TaskRepoRef {
  /** 仓库 ID */
  repoId: string
  /** 仓库名称 */
  repoName: string
  /** 仓库 URL */
  repoUrl: string
  /** 目标分支 */
  branch: string
  /** 目标路径（可选，例如 src/utils） */
  path?: string
}

export interface ScheduledTaskDto {
  /** 任务 ID */
  id: string
  /** 任务名称 */
  name: string
  /** 任务描述 */
  description: string
  /** 关联的模板 ID（可选） */
  templateId?: string
  /** 触发类型 */
  triggerType: TriggerType
  /** Cron 表达式（triggerType 为 cron 时） */
  cronExpression: string
  /** 间隔毫秒（triggerType 为 interval 时） */
  intervalMs: number
  /** 一次性执行时间（triggerType 为 once 时） */
  onceTime: string
  /** 关联的 Agent ID */
  agentId: string
  /** 关联的 Agent 名称 */
  agentName: string
  /** 关联的仓库 */
  repo?: TaskRepoRef
  /** 任务参数（JSON 字符串） */
  params: string
  /** 任务状态 */
  status: TaskStatus
  /** 下次执行时间 */
  nextRunTime: string
  /** 上次执行时间 */
  lastRunTime: string
  /** 总执行次数 */
  totalRuns: number
  /** 成功次数 */
  successRuns: number
  /** 失败次数 */
  failedRuns: number
  /** 是否启用 */
  enabled: boolean
  /** 创建时间 */
  createdAt: string
  /** 更新时间 */
  updatedAt: string
}

export interface CreateScheduledTaskDto {
  name: string
  description: string
  templateId?: string
  triggerType: TriggerType
  cronExpression: string
  intervalMs: number
  onceTime: string
  agentId: string
  repo?: TaskRepoRef
  params: string
  enabled: boolean
}

export interface UpdateScheduledTaskDto extends Partial<CreateScheduledTaskDto> {
  id: string
}

/** 任务模板 */
export interface TaskTemplateDto {
  /** 模板 ID */
  id: string
  /** 模板名称 */
  name: string
  /** 模板描述 */
  description: string
  /** 预设的 Agent ID */
  agentId: string
  /** 预设的系统提示词（部分覆盖） */
  systemPromptOverride?: string
  /** 预设的触发类型 */
  triggerType: TriggerType
  /** 预设的 Cron 表达式 */
  cronExpression?: string
  /** 预设的间隔（毫秒） */
  intervalMs?: number
  /** 预设参数 */
  defaultParams: string
  /** 是否为系统内置模板 */
  isBuiltIn: boolean
}

export interface ExecutionRecordDto {
  /** 执行记录 ID */
  id: string
  /** 任务 ID */
  taskId: string
  /** 执行开始时间 */
  startTime: string
  /** 执行结束时间 */
  endTime: string
  /** 执行状态 */
  status: TaskStatus
  /** 执行耗时（毫秒） */
  duration: number
  /** 执行结果/错误信息 */
  result: string
}

const defaultTasks: ScheduledTaskDto[] = [
  {
    id: 'st-1',
    name: '每日代码质量检查',
    description: '每天早上 9 点自动执行代码质量检查，生成报告',
    templateId: 'tpl-code-review',
    triggerType: 'cron',
    cronExpression: '0 9 * * *',
    intervalMs: 0,
    onceTime: '',
    agentId: 'agent-1',
    agentName: '代码审查助手',
    repo: {
      repoId: 'repo-1',
      repoName: 'AutoCodeForge',
      repoUrl: 'https://github.com/org/AutoCodeForge',
      branch: 'main',
    },
    params: '{"reportLevel":"detailed"}',
    status: 'success',
    nextRunTime: '2026-05-20 09:00:00',
    lastRunTime: '2026-05-19 09:00:00',
    totalRuns: 19,
    successRuns: 18,
    failedRuns: 1,
    enabled: true,
    createdAt: '2026-05-01 10:00:00',
    updatedAt: '2026-05-19 09:00:00',
  },
  {
    id: 'st-2',
    name: '数据库备份',
    description: '每 6 小时执行一次数据库全量备份',
    templateId: 'tpl-db-backup',
    triggerType: 'interval',
    cronExpression: '',
    intervalMs: 6 * 60 * 60 * 1000,
    onceTime: '',
    agentId: 'agent-3',
    agentName: '数据库专家',
    repo: {
      repoId: 'repo-2',
      repoName: 'backend-service',
      repoUrl: 'https://github.com/org/backend-service',
      branch: 'develop',
    },
    params: '{"backupType":"full"}',
    status: 'success',
    nextRunTime: '2026-05-19 22:00:00',
    lastRunTime: '2026-05-19 16:00:00',
    totalRuns: 76,
    successRuns: 75,
    failedRuns: 1,
    enabled: true,
    createdAt: '2026-05-02 08:00:00',
    updatedAt: '2026-05-19 16:00:00',
  },
  {
    id: 'st-3',
    name: '周报自动生成',
    description: '每周五下午 6 点自动生成项目周报',
    templateId: 'tpl-doc-gen',
    triggerType: 'cron',
    cronExpression: '0 18 * * 5',
    intervalMs: 0,
    onceTime: '',
    agentId: 'agent-5',
    agentName: '文档撰写助手',
    repo: {
      repoId: 'repo-1',
      repoName: 'AutoCodeForge',
      repoUrl: 'https://github.com/org/AutoCodeForge',
      branch: 'main',
      path: 'docs',
    },
    params: '{"template":"weekly","recipients":["team@example.com"]}',
    status: 'pending',
    nextRunTime: '2026-05-23 18:00:00',
    lastRunTime: '2026-05-16 18:00:00',
    totalRuns: 4,
    successRuns: 4,
    failedRuns: 0,
    enabled: true,
    createdAt: '2026-05-03 14:00:00',
    updatedAt: '2026-05-16 18:00:00',
  },
  {
    id: 'st-4',
    name: '性能监控报告',
    description: '每小时检查系统性能指标，异常时告警',
    templateId: 'tpl-monitor',
    triggerType: 'interval',
    cronExpression: '',
    intervalMs: 60 * 60 * 1000,
    onceTime: '',
    agentId: 'agent-2',
    agentName: '架构设计专家',
    repo: {
      repoId: 'repo-2',
      repoName: 'backend-service',
      repoUrl: 'https://github.com/org/backend-service',
      branch: 'main',
    },
    params: '{"alertThreshold":{"cpu":80,"memory":85}}',
    status: 'failed',
    nextRunTime: '2026-05-19 21:00:00',
    lastRunTime: '2026-05-19 20:00:00',
    totalRuns: 200,
    successRuns: 195,
    failedRuns: 5,
    enabled: false,
    createdAt: '2026-05-04 10:00:00',
    updatedAt: '2026-05-19 20:00:00',
  },
  {
    id: 'st-5',
    name: '临时数据清理',
    description: '一次性任务：清理 30 天前的临时文件',
    triggerType: 'once',
    cronExpression: '',
    intervalMs: 0,
    onceTime: '2026-05-25 02:00:00',
    agentId: 'agent-3',
    agentName: '数据库专家',
    repo: {
      repoId: 'repo-2',
      repoName: 'backend-service',
      repoUrl: 'https://github.com/org/backend-service',
      branch: 'develop',
      path: 'temp',
    },
    params: '{"retentionDays":30,"dryRun":false}',
    status: 'pending',
    nextRunTime: '2026-05-25 02:00:00',
    lastRunTime: '',
    totalRuns: 0,
    successRuns: 0,
    failedRuns: 0,
    enabled: true,
    createdAt: '2026-05-05 11:00:00',
    updatedAt: '2026-05-19 15:00:00',
  },
]

/** 默认任务模板 */
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

const defaultExecutions: ExecutionRecordDto[] = [
  {
    id: 'ex-1',
    taskId: 'st-1',
    startTime: '2026-05-19 09:00:00',
    endTime: '2026-05-19 09:02:35',
    status: 'success',
    duration: 155000,
    result: '代码质量检查完成，发现 3 个建议改进点，0 个严重问题。',
  },
  {
    id: 'ex-2',
    taskId: 'st-1',
    startTime: '2026-05-18 09:00:00',
    endTime: '2026-05-18 09:01:48',
    status: 'success',
    duration: 108000,
    result: '代码质量检查完成，无问题。',
  },
  {
    id: 'ex-3',
    taskId: 'st-4',
    startTime: '2026-05-19 20:00:00',
    endTime: '2026-05-19 20:00:12',
    status: 'failed',
    duration: 12000,
    result: '监控数据采集失败：连接超时。',
  },
]

let tasksData: ScheduledTaskDto[] = [...defaultTasks]
let templatesData: TaskTemplateDto[] = [...defaultTemplates]
let executionsData: ExecutionRecordDto[] = [...defaultExecutions]

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => window.setTimeout(resolve, ms))
}

/** 获取所有定时任务 */
export async function getScheduledTasks(): Promise<ScheduledTaskDto[]> {
  await wait(200)
  return [...tasksData]
}

/** 根据 ID 获取单个定时任务 */
export async function getScheduledTask(id: string): Promise<ScheduledTaskDto | null> {
  await wait(100)
  return tasksData.find((t) => t.id === id) ?? null
}

/** 创建新的定时任务 */
export async function createScheduledTask(dto: CreateScheduledTaskDto): Promise<ScheduledTaskDto> {
  await wait(300)
  const now = new Date().toLocaleString('zh-CN', { hour12: false })
  const task: ScheduledTaskDto = {
    id: `st-${Date.now()}`,
    ...dto,
    agentName: getAgentName(dto.agentId),
    status: 'pending',
    nextRunTime: calculateNextRunTime(dto),
    lastRunTime: '',
    totalRuns: 0,
    successRuns: 0,
    failedRuns: 0,
    enabled: dto.enabled,
    createdAt: now,
    updatedAt: now,
  }
  tasksData.push(task)
  return task
}

/** 更新定时任务 */
export async function updateScheduledTask(dto: UpdateScheduledTaskDto): Promise<ScheduledTaskDto | null> {
  await wait(300)
  const index = tasksData.findIndex((t) => t.id === dto.id)
  if (index === -1) return null
  const existing = tasksData[index]
  if (!existing) return null
  const updated: ScheduledTaskDto = {
    ...existing,
    ...dto,
    agentName: dto.agentId ? getAgentName(dto.agentId) : existing.agentName,
    updatedAt: new Date().toLocaleString('zh-CN', { hour12: false }),
  }
  if (dto.triggerType || dto.cronExpression || dto.intervalMs || dto.onceTime) {
    updated.nextRunTime = calculateNextRunTime(updated)
  }
  tasksData[index] = updated
  return updated
}

/** 删除定时任务 */
export async function deleteScheduledTask(id: string): Promise<boolean> {
  await wait(200)
  const index = tasksData.findIndex((t) => t.id === id)
  if (index === -1) return false
  tasksData.splice(index, 1)
  executionsData = executionsData.filter((e) => e.taskId !== id)
  return true
}

/** 获取任务执行记录 */
export async function getExecutionRecords(taskId: string): Promise<ExecutionRecordDto[]> {
  await wait(150)
  return executionsData.filter((e) => e.taskId === taskId)
}

/** 手动触发任务执行 */
export async function triggerTask(taskId: string): Promise<ExecutionRecordDto> {
  await wait(500)
  const task = tasksData.find((t) => t.id === taskId)
  if (!task) throw new Error('任务不存在')

  const record: ExecutionRecordDto = {
    id: `ex-${Date.now()}`,
    taskId,
    startTime: new Date().toLocaleString('zh-CN', { hour12: false }),
    endTime: '',
    status: 'running',
    duration: 0,
    result: '执行中...',
  }
  executionsData.unshift(record)
  return record
}

/** 切换任务启用状态 */
export async function toggleTaskEnabled(id: string): Promise<ScheduledTaskDto | null> {
  await wait(100)
  const task = tasksData.find((t) => t.id === id)
  if (!task) return null
  task.enabled = !task.enabled
  task.status = task.enabled ? 'pending' : 'disabled'
  task.updatedAt = new Date().toLocaleString('zh-CN', { hour12: false })
  return { ...task }
}

/** 根据 Agent ID 获取 Agent 名称 */
function getAgentName(agentId: string): string {
  const agentNames: Record<string, string> = {
    'agent-1': '代码审查助手',
    'agent-2': '架构设计专家',
    'agent-3': '数据库专家',
    'agent-4': '前端开发助手',
    'agent-5': '文档撰写助手',
  }
  return agentNames[agentId] ?? '未知 Agent'
}

/** 根据触发配置计算下次执行时间 */
function calculateNextRunTime(task: CreateScheduledTaskDto | ScheduledTaskDto): string {
  const now = new Date()
  if (task.triggerType === 'once') {
    return task.onceTime || ''
  }
  if (task.triggerType === 'interval' && task.intervalMs > 0) {
    const next = new Date(now.getTime() + task.intervalMs)
    return next.toLocaleString('zh-CN', { hour12: false })
  }
  // 对于 cron 表达式，简单返回“待计算”
  return '待计算'
}

/** 获取所有任务模板 */
export async function getTaskTemplates(): Promise<TaskTemplateDto[]> {
  await wait(100)
  return [...templatesData]
}

/** 根据 ID 获取单个任务模板 */
export async function getTaskTemplate(id: string): Promise<TaskTemplateDto | null> {
  await wait(50)
  return templatesData.find((t) => t.id === id) ?? null
}
