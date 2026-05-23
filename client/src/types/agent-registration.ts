export type AgentRegistrationStatus = 'Online' | 'Offline' | 'Unknown'

export interface AgentRegistrationInfo {
  agentId: string
  serverId: string
  instanceId: string
  lastHeartbeat: string
  status: AgentRegistrationStatus
  registeredAt: string
}

export interface RegisterAgentRequest {
  agentId: string
  serverId: string
  instanceId: string
}

export interface HeartbeatRequest {
  agentId: string
}

export interface ApiEnvelope<T> {
  success: boolean
  message: string
  data: T
  traceId?: string
}