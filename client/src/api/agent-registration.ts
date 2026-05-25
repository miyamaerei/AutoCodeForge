import { request } from '@/lib/request'
import type {
  ApiEnvelope,
  AgentRegistrationInfo,
  RegisterAgentRequest,
  HeartbeatRequest,
} from '@/types/agent-registration'

export async function registerAgent(
  requestBody: RegisterAgentRequest,
): Promise<AgentRegistrationInfo> {
  const { data } = await request.post<ApiEnvelope<AgentRegistrationInfo>>(
    '/api/v1/agents/register',
    requestBody,
  )
  return data.data
}

export async function renewHeartbeat(requestBody: HeartbeatRequest): Promise<void> {
  await request.put('/api/v1/agents/heartbeat', requestBody)
}

export async function deregisterAgent(agentId: string): Promise<void> {
  await request.delete(`/api/v1/agents/${agentId}`)
}

export async function getAvailableAgents(): Promise<AgentRegistrationInfo[]> {
  const { data } = await request.get<ApiEnvelope<AgentRegistrationInfo[]>>(
    '/api/v1/agents/available',
  )
  return data.data
}

export async function getAgentRegistration(agentId: string): Promise<AgentRegistrationInfo> {
  const { data } = await request.get<ApiEnvelope<AgentRegistrationInfo>>(
    `/api/v1/agents/${agentId}`,
  )
  return data.data
}