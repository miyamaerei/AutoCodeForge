import { request } from '@/lib/request'
import type { AgentRegistrationDto, RegisterAgentRequest, HeartbeatRequest } from './agent-registration.types'

export async function registerAgent(payload: RegisterAgentRequest): Promise<AgentRegistrationDto> {
  const { data } = await request.post<AgentRegistrationDto>('/v1/agents/register', payload)
  return data
}

export async function renewHeartbeat(payload: HeartbeatRequest): Promise<void> {
  await request.put('/v1/agents/heartbeat', payload)
}

export async function deregisterAgent(agentId: string): Promise<void> {
  await request.delete(`/v1/agents/${agentId}`)
}

export async function getAvailableAgents(): Promise<AgentRegistrationDto[]> {
  const { data } = await request.get<AgentRegistrationDto[]>('/v1/agents/available')
  return data
}

export async function getAgentRegistration(agentId: string): Promise<AgentRegistrationDto> {
  const { data } = await request.get<AgentRegistrationDto>(`/v1/agents/${agentId}`)
  return data
}