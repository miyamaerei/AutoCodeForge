export interface ApiEnvelope<T> {
  success: boolean
  message: string
  data: T
  traceId?: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface ApiError {
  message: string
  errors?: Record<string, string[]>
}

export interface TaskResponseDto {
  id: string
  title: string
  description?: string | null
  status: string
  taskType: string
  progress: number
  input: string
  result?: string | null
  errorMessage?: string | null
  agentId?: string | null
  dueAtUtc?: string | null
  startedAtUtc?: string | null
  completedAtUtc?: string | null
  createdAtUtc: string
  updatedAtUtc: string
}

export interface TaskLogResponseDto {
  id: string
  taskId: string
  level: string
  message: string
  source?: string | null
  createdAtUtc: string
}

export interface CreateTaskRequestDto {
  title: string
  taskType: string
  description: string
  repository: string
  branch: string
  agentId?: string
  dueAtUtc?: string
}

export interface UpdateTaskRequestDto {
  title?: string
  taskType?: string
  description?: string
  repository?: string
  branch?: string
  agentId?: string
  dueAtUtc?: string
}

export interface TaskSummaryDto {
  id: string
  title: string
  state: '运行中' | '已完成' | '已暂停' | '失败'
  createdAt: string
}

export interface TaskStepDto {
  id: string
  title: string
}

export interface TaskDetailDto {
  id: string
  title: string
  state: '运行中' | '已完成' | '已暂停' | '失败'
  steps: TaskStepDto[]
}

export interface TaskLogDto {
  id: string
  taskId: string
  message: string
}