export type AgentRole = 'Secretary' | 'Manager' | 'Worker'

export interface OrchestrationSettings {
  maxConcurrentTasksPerManager: number
  maxConcurrentTasksPerSecretary: number
  maxConcurrentTasksPerWorker: number
  loadBalancingStrategy: string
  enableEscalation: boolean
}

export interface OrchestrationAssignRequest {
  role: AgentRole
}

export interface OrchestrationReassignRequest {
  currentAgentId: string
}

export interface OrchestrationAssignResponse {
  taskId: string
  agentId: string
  agentName: string
  usedEscalation: boolean
}

export interface ApiEnvelope<T> {
  success: boolean
  message: string
  data: T
  traceId?: string
}