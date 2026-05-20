export type TaskStatus = 'pending' | 'running' | 'success' | 'failed' | 'disabled'
export type TriggerType = 'cron' | 'interval' | 'once'

export interface TaskRepoRef {
  repoId: string
  repoName: string
  repoUrl: string
  branch: string
  path?: string
}

export interface ScheduledTaskDto {
  id: string
  name: string
  triggerType: TriggerType
  cronExpression: string
  status: TaskStatus
  agentId?: string
  input: string
  taskTitle: string
  nextRunAtUtc?: string
  createdAtUtc: string
}

export interface CreateScheduledTaskDto {
  name: string
  cronExpression: string
  triggerType?: TriggerType
  agentId?: string
  input: string
  taskTitle: string
}

export interface UpdateScheduledTaskDto {
  name?: string
  cronExpression?: string
  agentId?: string
  input?: string
  taskTitle?: string
  status?: TaskStatus
}

export interface ScheduledTaskExecutionDto {
  id: string
  scheduledTaskId: string
  status: TaskStatus
  startedAtUtc: string
  completedAtUtc?: string
  output?: string
}

export interface TaskTemplateDto {
  id: string
  name: string
  description: string
  agentId: string
  systemPromptOverride?: string
  triggerType: TriggerType
  cronExpression?: string
  intervalMs?: number
  defaultParams: string
  isBuiltIn: boolean
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface ApiResponse<T> {
  success: boolean
  message?: string
  data: T
}