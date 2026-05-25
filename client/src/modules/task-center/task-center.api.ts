import { request } from '../../lib/request'
import { USE_MOCK } from '../../config/runtime'
import {
  createTask as createTaskMock,
  getTaskDetail as getTaskDetailMock,
  getTaskLogs as getTaskLogsMock,
  getTaskSummaries as getTaskSummariesMock,
} from '../../mock'
import type {
  ApiEnvelope,
  AdvanceTaskStepRequestDto,
  ApproveRequestDto,
  CreateHumanGateRequestDto,
  CreateTaskRequestDto,
  HumanGateResponseDto,
  ModifyApproveRequestDto,
  PagedResult,
  RejectRequestDto,
  SkipTaskStepRequestDto,
  TaskDetailDto,
  TaskLogDto,
  TaskLogResponseDto,
  TaskResponseDto,
  TaskStepDto,
  TaskStepResponseDto,
  TaskSummaryDto,
  UnbindTaskStepRequestDto,
  UpdateTaskRequestDto,
  UpdateTaskStepRequestDto,
} from './task-center.types'

export type {
  AdvanceTaskStepRequestDto,
  CreateTaskRequestDto,
  SkipTaskStepRequestDto,
  TaskDetailDto,
  TaskLogDto,
  TaskStepResponseDto,
  TaskSummaryDto,
  UnbindTaskStepRequestDto,
  UpdateTaskRequestDto,
  UpdateTaskStepRequestDto,
} from './task-center.types'

function normalizeState(status: string): TaskSummaryDto['state'] {
  const normalized = status.toLowerCase()
  if (normalized.includes('pause')) {
    return '已暂停'
  }
  if (normalized.includes('complete') || normalized.includes('success') || normalized.includes('done')) {
    return '已完成'
  }
  if (normalized.includes('fail') || normalized.includes('error')) {
    return '失败'
  }
  return '运行中'
}

function toLocaleDateTime(iso?: string | null): string {
  if (!iso) {
    return ''
  }
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) {
    return iso
  }
  return date.toLocaleString('zh-CN', { hour12: false })
}

function mapTaskSummary(dto: TaskResponseDto): TaskSummaryDto {
  return {
    id: dto.id,
    title: dto.title,
    state: normalizeState(dto.status),
    createdAt: toLocaleDateTime(dto.createdAtUtc),
  }
}

function buildSteps(dto: TaskResponseDto): TaskStepDto[] {
  const steps: TaskStepDto[] = [
    {
      id: 'status',
      title: `状态: ${normalizeState(dto.status)}`,
    },
  ]

  if (dto.startedAtUtc) {
    steps.push({ id: 'started', title: `开始时间: ${toLocaleDateTime(dto.startedAtUtc)}` })
  }

  if (dto.progress > 0) {
    steps.push({ id: 'progress', title: `当前进度: ${dto.progress}%` })
  }

  if (dto.completedAtUtc) {
    steps.push({ id: 'completed', title: `完成时间: ${toLocaleDateTime(dto.completedAtUtc)}` })
  }

  if (dto.errorMessage) {
    steps.push({ id: 'error', title: `错误信息: ${dto.errorMessage}` })
  }

  if (steps.length === 1) {
    steps.push({ id: 'queued', title: '任务已创建，等待执行' })
  }

  return steps
}

function mapTaskDetail(dto: TaskResponseDto): TaskDetailDto {
  return {
    id: dto.id,
    title: dto.title,
    state: normalizeState(dto.status),
    steps: buildSteps(dto),
  }
}

function mapTaskLog(dto: TaskLogResponseDto): TaskLogDto {
  const timestamp = toLocaleDateTime(dto.createdAtUtc)
  const level = dto.level?.toUpperCase() || 'INFO'
  return {
    id: dto.id,
    taskId: dto.taskId,
    message: `[${timestamp}] [${level}] ${dto.message}`,
  }
}

function buildTaskInput(payload: Pick<CreateTaskRequestDto, 'description' | 'repository' | 'branch' | 'taskType'>): string {
  return JSON.stringify(
    {
      taskType: payload.taskType,
      description: payload.description,
      repository: payload.repository,
      branch: payload.branch,
    },
    null,
    0,
  )
}

export async function fetchTaskSummaries(page = 1, pageSize = 20): Promise<TaskSummaryDto[]> {
  if (USE_MOCK) {
    return getTaskSummariesMock()
  }

  const { data } = await request.get<ApiEnvelope<PagedResult<TaskResponseDto>>>('/v1/tasks', {
    params: {
      page,
      pageSize,
    },
  })

  return data.data.items.map(mapTaskSummary)
}

export async function fetchTaskDetail(taskId: string): Promise<TaskDetailDto> {
  if (USE_MOCK) {
    return getTaskDetailMock(taskId)
  }

  const { data } = await request.get<ApiEnvelope<TaskResponseDto>>(`/v1/tasks/${taskId}`)
  return mapTaskDetail(data.data)
}

export async function fetchTaskLogs(taskId: string): Promise<TaskLogDto[]> {
  if (USE_MOCK) {
    return getTaskLogsMock(taskId)
  }

  const { data } = await request.get<ApiEnvelope<TaskLogResponseDto[]>>(`/v1/tasks/${taskId}/logs`)
  return data.data.map(mapTaskLog)
}

export async function createTask(payload: CreateTaskRequestDto): Promise<TaskSummaryDto> {
  if (USE_MOCK) {
    return createTaskMock(payload)
  }

  const { data } = await request.post<ApiEnvelope<TaskResponseDto>>('/v1/tasks', {
    title: payload.title,
    taskType: payload.taskType,
    description: payload.description,
    input: buildTaskInput(payload),
    agentId: payload.agentId,
    dueAtUtc: payload.dueAtUtc,
  })

  return mapTaskSummary(data.data)
}

export async function updateTask(taskId: string, payload: UpdateTaskRequestDto): Promise<TaskDetailDto> {
  if (USE_MOCK) {
    const mockDetail = await getTaskDetailMock(taskId)
    return {
      ...mockDetail,
      title: payload.title ?? mockDetail.title,
    }
  }

  const { data } = await request.put<ApiEnvelope<TaskResponseDto>>(`/v1/tasks/${taskId}`, {
    title: payload.title,
    description: payload.description,
    input: buildTaskInput({
      taskType: payload.taskType ?? 'requirement',
      description: payload.description ?? '',
      repository: payload.repository ?? '',
      branch: payload.branch ?? '',
    }),
    agentId: payload.agentId,
    dueAtUtc: payload.dueAtUtc,
  })

  return mapTaskDetail(data.data)
}

export async function pauseTask(taskId: string): Promise<TaskDetailDto> {
  if (USE_MOCK) {
    return getTaskDetailMock(taskId)
  }

  const { data } = await request.post<ApiEnvelope<TaskResponseDto>>(`/v1/tasks/${taskId}/pause`)
  return mapTaskDetail(data.data)
}

export async function resumeTask(taskId: string): Promise<TaskDetailDto> {
  if (USE_MOCK) {
    return getTaskDetailMock(taskId)
  }

  const { data } = await request.post<ApiEnvelope<TaskResponseDto>>(`/v1/tasks/${taskId}/resume`)
  return mapTaskDetail(data.data)
}

export async function deleteTask(taskId: string): Promise<void> {
  if (USE_MOCK) {
    return
  }

  await request.delete(`/v1/tasks/${taskId}`)
}

// 工序步骤 API
export async function fetchTaskSteps(taskId: string): Promise<TaskStepResponseDto[]> {
  const { data } = await request.get<ApiEnvelope<TaskStepResponseDto[]>>(`/v1/tasks/${taskId}/steps`)
  return data.data
}

export async function fetchTaskActiveStep(taskId: string): Promise<TaskStepResponseDto | null> {
  const { data } = await request.get<ApiEnvelope<TaskStepResponseDto | null>>(`/v1/tasks/${taskId}/steps/active`)
  return data.data
}

export async function fetchTaskStep(stepId: string): Promise<TaskStepResponseDto> {
  const { data } = await request.get<ApiEnvelope<TaskStepResponseDto>>(`/v1/tasks/steps/${stepId}`)
  return data.data
}

export async function initializeTaskSteps(taskId: string): Promise<TaskStepResponseDto[]> {
  const { data } = await request.post<ApiEnvelope<TaskStepResponseDto[]>>(`/v1/tasks/${taskId}/steps/init`)
  return data.data
}

export async function advanceTaskStep(
  taskId: string,
  stepId: string,
  payload: AdvanceTaskStepRequestDto,
): Promise<TaskStepResponseDto> {
  const { data } = await request.post<ApiEnvelope<TaskStepResponseDto>>(`/v1/tasks/${taskId}/steps/${stepId}/advance`, payload)
  return data.data
}

export async function skipTaskStep(
  taskId: string,
  stepId: string,
  payload: SkipTaskStepRequestDto,
): Promise<TaskStepResponseDto> {
  const { data } = await request.post<ApiEnvelope<TaskStepResponseDto>>(`/v1/tasks/${taskId}/steps/${stepId}/skip`, payload)
  return data.data
}

export async function unbindTaskStep(
  taskId: string,
  stepId: string,
  payload: UnbindTaskStepRequestDto,
): Promise<TaskStepResponseDto> {
  const { data } = await request.post<ApiEnvelope<TaskStepResponseDto>>(`/v1/tasks/${taskId}/steps/${stepId}/unbind`, payload)
  return data.data
}

export async function fetchTaskStepContext(
  taskId: string,
  stepId?: string,
  maxTokens?: number,
): Promise<string> {
  const { data } = await request.get<ApiEnvelope<string>>(`/v1/tasks/${taskId}/steps/context`, {
    params: { stepId, maxTokens },
  })
  return data.data
}

export async function updateTaskStep(
  stepId: string,
  payload: UpdateTaskStepRequestDto,
): Promise<TaskStepResponseDto> {
  const { data } = await request.put<ApiEnvelope<TaskStepResponseDto>>(`/v1/tasks/steps/${stepId}`, payload)
  return data.data
}

// Human Gate API
export async function fetchPendingHumanGates(): Promise<HumanGateResponseDto[]> {
  const { data } = await request.get<ApiEnvelope<HumanGateResponseDto[]>>('/v1/human-gates/pending')
  return data.data
}

export async function fetchHumanGateById(gateId: string): Promise<HumanGateResponseDto> {
  const { data } = await request.get<ApiEnvelope<HumanGateResponseDto>>(`/v1/human-gates/${gateId}`)
  return data.data
}

export async function fetchHumanGatesByTaskId(taskId: string): Promise<HumanGateResponseDto[]> {
  const { data } = await request.get<ApiEnvelope<HumanGateResponseDto[]>>(`/v1/human-gates/task/${taskId}`)
  return data.data
}

export async function createHumanGate(payload: CreateHumanGateRequestDto): Promise<HumanGateResponseDto> {
  const { data } = await request.post<ApiEnvelope<HumanGateResponseDto>>('/v1/human-gates', payload)
  return data.data
}

export async function approveHumanGate(gateId: string, payload?: ApproveRequestDto): Promise<HumanGateResponseDto> {
  const { data } = await request.post<ApiEnvelope<HumanGateResponseDto>>(`/v1/human-gates/${gateId}/approve`, payload)
  return data.data
}

export async function rejectHumanGate(gateId: string, payload?: RejectRequestDto): Promise<HumanGateResponseDto> {
  const { data } = await request.post<ApiEnvelope<HumanGateResponseDto>>(`/v1/human-gates/${gateId}/reject`, payload)
  return data.data
}

export async function modifyApproveHumanGate(gateId: string, payload?: ModifyApproveRequestDto): Promise<HumanGateResponseDto> {
  const { data } = await request.post<ApiEnvelope<HumanGateResponseDto>>(`/v1/human-gates/${gateId}/modify-approve`, payload)
  return data.data
}

export async function cancelHumanGate(gateId: string): Promise<HumanGateResponseDto> {
  const { data } = await request.post<ApiEnvelope<HumanGateResponseDto>>(`/v1/human-gates/${gateId}/cancel`)
  return data.data
}

export type {
  HumanGateResponseDto,
  CreateHumanGateRequestDto,
  ApproveRequestDto,
  RejectRequestDto,
  ModifyApproveRequestDto,
}
