import { request } from '@/lib/request'
import type {
  AgentRole,
  ApiEnvelope,
  OrchestrationAssignRequest,
  OrchestrationAssignResponse,
  OrchestrationReassignRequest,
  OrchestrationSettings,
} from '@/types/task-orchestration'

export async function assignTask(
  taskId: string,
  role: AgentRole,
): Promise<OrchestrationAssignResponse> {
  const payload: OrchestrationAssignRequest = { role }
  const { data } = await request.post<ApiEnvelope<OrchestrationAssignResponse>>(
    `/v1/orchestration/tasks/${taskId}/assign`,
    payload,
  )
  return data.data
}

export async function reassignTask(
  taskId: string,
  currentAgentId: string,
): Promise<OrchestrationAssignResponse> {
  const payload: OrchestrationReassignRequest = { currentAgentId }
  const { data } = await request.post<ApiEnvelope<OrchestrationAssignResponse>>(
    `/v1/orchestration/tasks/${taskId}/reassign`,
    payload,
  )
  return data.data
}

export async function getOrchestrationSettings(): Promise<OrchestrationSettings> {
  const { data } = await request.get<ApiEnvelope<OrchestrationSettings>>('/v1/orchestration/settings')
  return data.data
}