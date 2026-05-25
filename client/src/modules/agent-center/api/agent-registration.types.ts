export interface AgentRegistrationDto {
  agentId: string
  serverId: string
  instanceId: string
  lastHeartbeat: string
  status: 'Online' | 'Offline' | 'Unknown'
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