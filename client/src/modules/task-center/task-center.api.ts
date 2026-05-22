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
  CreateTaskRequestDto,
  PagedResult,
  TaskDetailDto,
  TaskLogDto,
  TaskLogResponseDto,
  TaskResponseDto,
  TaskStepDto,
  TaskSummaryDto,
  UpdateTaskRequestDto,
} from './task-center.types'

export type {
  CreateTaskRequestDto,
  TaskDetailDto,
  TaskLogDto,
  TaskSummaryDto,
  UpdateTaskRequestDto,
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
