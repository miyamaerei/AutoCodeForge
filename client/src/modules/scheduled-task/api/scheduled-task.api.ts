import type {
  ScheduledTaskDto,
  CreateScheduledTaskDto,
  UpdateScheduledTaskDto,
  ScheduledTaskExecutionDto,
  TaskTemplateDto,
  PagedResult,
  ApiResponse,
} from './scheduled-task.types'
import { request } from '@/lib/request'
import { USE_MOCK } from '@/config/runtime'
import {
  mockGetScheduledTasks,
  mockGetScheduledTask,
  mockCreateScheduledTask,
  mockUpdateScheduledTask,
  mockDeleteScheduledTask,
  mockGetExecutions,
  mockTriggerTask,
  mockToggleTaskEnabled,
  mockGetTaskTemplates,
  mockGetTaskTemplate,
} from './scheduled-task.mock'

export async function fetchScheduledTasks(page = 1, pageSize = 20): Promise<PagedResult<ScheduledTaskDto>> {
  if (USE_MOCK) return mockGetScheduledTasks(page, pageSize)
  const { data } = await request.get<ApiResponse<PagedResult<ScheduledTaskDto>>>('/v1/scheduled-tasks', {
    params: { page, pageSize },
  })
  return data.data
}

export async function fetchScheduledTask(id: string): Promise<ScheduledTaskDto | null> {
  if (USE_MOCK) return mockGetScheduledTask(id)
  try {
    const { data } = await request.get<ApiResponse<ScheduledTaskDto>>(`/v1/scheduled-tasks/${id}`)
    return data.data
  } catch {
    return null
  }
}

export async function createScheduledTask(dto: CreateScheduledTaskDto): Promise<ScheduledTaskDto> {
  if (USE_MOCK) return mockCreateScheduledTask(dto)
  const { data } = await request.post<ApiResponse<ScheduledTaskDto>>('/v1/scheduled-tasks', dto)
  return data.data
}

export async function updateScheduledTask(id: string, dto: UpdateScheduledTaskDto): Promise<ScheduledTaskDto | null> {
  if (USE_MOCK) return mockUpdateScheduledTask(id, dto)
  try {
    const { data } = await request.put<ApiResponse<ScheduledTaskDto>>(`/v1/scheduled-tasks/${id}`, dto)
    return data.data
  } catch {
    return null
  }
}

export async function deleteScheduledTask(id: string): Promise<boolean> {
  if (USE_MOCK) return mockDeleteScheduledTask(id)
  try {
    await request.delete(`/v1/scheduled-tasks/${id}`)
    return true
  } catch {
    return false
  }
}

export async function pauseScheduledTask(id: string): Promise<ScheduledTaskDto | null> {
  if (USE_MOCK) return mockToggleTaskEnabled(id, false)
  try {
    const { data } = await request.post<ApiResponse<ScheduledTaskDto>>(`/v1/scheduled-tasks/${id}/pause`)
    return data.data
  } catch {
    return null
  }
}

export async function resumeScheduledTask(id: string): Promise<ScheduledTaskDto | null> {
  if (USE_MOCK) return mockToggleTaskEnabled(id, true)
  try {
    const { data } = await request.post<ApiResponse<ScheduledTaskDto>>(`/v1/scheduled-tasks/${id}/resume`)
    return data.data
  } catch {
    return null
  }
}

export async function fetchExecutions(taskId: string, page = 1, pageSize = 20): Promise<PagedResult<ScheduledTaskExecutionDto>> {
  if (USE_MOCK) return mockGetExecutions(taskId, page, pageSize)
  const { data } = await request.get<ApiResponse<PagedResult<ScheduledTaskExecutionDto>>>(`/v1/scheduled-tasks/${taskId}/executions`, {
    params: { page, pageSize },
  })
  return data.data
}

export async function triggerTask(taskId: string): Promise<ScheduledTaskExecutionDto> {
  if (USE_MOCK) return mockTriggerTask(taskId)
  const { data } = await request.post<ApiResponse<ScheduledTaskExecutionDto>>(`/v1/scheduled-tasks/${taskId}/trigger`)
  return data.data
}

export async function toggleTaskEnabled(id: string): Promise<ScheduledTaskDto | null> {
  if (USE_MOCK) return mockToggleTaskEnabled(id)
  const task = await fetchScheduledTask(id)
  if (!task) return null
  if (task.status === 'disabled') {
    return resumeScheduledTask(id)
  }
  return pauseScheduledTask(id)
}

export async function getTaskTemplates(): Promise<TaskTemplateDto[]> {
  return mockGetTaskTemplates()
}

export async function getTaskTemplate(id: string): Promise<TaskTemplateDto | null> {
  return mockGetTaskTemplate(id)
}
