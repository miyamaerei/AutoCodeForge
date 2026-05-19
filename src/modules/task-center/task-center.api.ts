import { request } from '../../lib/request'
import { USE_MOCK } from '../../config/runtime'
import {
  createTask as createTaskMock,
  getTaskDetail as getTaskDetailMock,
  getTaskSummaries as getTaskSummariesMock,
  type TaskCreateRequestDto,
  type TaskDetailDto,
  type TaskSummaryDto,
} from '../../mock'

export type { TaskCreateRequestDto, TaskDetailDto, TaskSummaryDto }

export async function fetchTaskSummaries(): Promise<TaskSummaryDto[]> {
  if (USE_MOCK) {
    return getTaskSummariesMock()
  }
  const { data } = await request.get<TaskSummaryDto[]>('/task-center/tasks')
  return data
}

export async function fetchTaskDetail(taskId: string): Promise<TaskDetailDto> {
  if (USE_MOCK) {
    return getTaskDetailMock(taskId)
  }
  const { data } = await request.get<TaskDetailDto>(`/task-center/tasks/${taskId}`)
  return data
}

export async function createTask(payload: TaskCreateRequestDto): Promise<TaskSummaryDto> {
  if (USE_MOCK) {
    return createTaskMock(payload)
  }
  const { data } = await request.post<TaskSummaryDto>('/task-center/tasks', payload)
  return data
}
