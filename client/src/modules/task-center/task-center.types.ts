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
  currentStep?: number | null
  currentStepId?: string | null
  createdAtUtc: string
  updatedAtUtc: string
  workflowId?: string | null
  workflowInstanceId?: string | null
  workflowNodeId?: string | null
}

// 工序步骤相关类型
export interface TaskStepResponseDto {
  id: string
  taskId: string
  step: number
  stepType: string
  status: string
  workerAgentId?: string | null
  reviewerAgentId?: string | null
  input?: string | null
  output?: string | null
  skipReason?: string | null
  startedAtUtc?: string | null
  completedAtUtc?: string | null
  retryCount: number
  createdAtUtc: string
  updatedAtUtc: string
}

export interface AdvanceTaskStepRequestDto {
  output: string
}

export interface SkipTaskStepRequestDto {
  reason: string
}

export interface UnbindTaskStepRequestDto {
  reason: string
  failureCategory?: string
}

export interface UpdateTaskStepRequestDto {
  status?: string
  workerAgentId?: string
  reviewerAgentId?: string
  input?: string
  output?: string
  skipReason?: string
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
  workflowId?: string
}

export interface UpdateTaskRequestDto {
  title?: string
  taskType?: string
  description?: string
  repository?: string
  branch?: string
  agentId?: string
  dueAtUtc?: string
  workflowId?: string
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

// Human Gate 相关类型
export interface HumanGateResponseDto {
  id: string
  taskId: string
  taskStepId?: string | null
  gateType: string
  gateTypeName: string
  status: string
  reason?: string | null
  humanResponse?: string | null
  modifications?: object | null
  reviewerUserId?: string | null
  reviewerName?: string | null
  createdAtUtc: string
  respondedAtUtc?: string | null
}

export interface CreateHumanGateRequestDto {
  taskId: string
  taskStepId?: string
  gateType: string
  reason?: string
}

export interface ApproveRequestDto {
  comment?: string
}

export interface RejectRequestDto {
  reason?: string
}

export interface ModifyApproveRequestDto {
  modifications?: GateModificationsDto
}

export interface GateModificationsDto {
  input?: string
  instructions?: string
}