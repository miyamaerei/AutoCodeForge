import { request } from '@/lib/request'

export interface AgentStats {
  total: number
  idle: number
  handling: number
  learning: number
  dormant: number
}

export interface TaskStats {
  total: number
  pending: number
  running: number
  completed: number
  failed: number
  paused: number
  canceled: number
}

export interface GateStats {
  pendingCount: number
  byType: Record<string, number>
}

export interface DashboardOverview {
  agentStats: AgentStats
  taskStats: TaskStats
  gateStats: GateStats
  lastUpdated: string
}

export interface PipelineStepStat {
  stepType: string
  total: number
  pending: number
  running: number
  completed: number
  failed: number
}

export interface SystemMetrics {
  agentCount: number
  activeAgents: number
  totalLearningHours: number
  averageLoad: number
  maxLoad: number
  lastHeartbeat: string | null
  upTime: string | null
}

export interface AgentResponse {
  id: string
  name: string
  description: string | null
  keywords: string | null
  systemPrompt: string | null
  llmModelConfigId: string | null
  isEnabled: boolean
  state: string
  role: string
  stateChangedAtUtc: string
  currentTaskId: string | null
  dormantReason: string | null
  skillTags: string | null
  learningProgress: number | null
  version: number
  createdAtUtc: string
  updatedAtUtc: string
}

export interface TaskResponse {
  id: string
  title: string
  description: string | null
  status: string
  taskType: string
  progress: number
  input: string
  result: string | null
  errorMessage: string | null
  agentId: string | null
  dueAtUtc: string | null
  startedAtUtc: string | null
  completedAtUtc: string | null
  currentStep: number | null
  currentStepId: string | null
  createdAtUtc: string
  updatedAtUtc: string
}

export const dashboardApi = {
  getOverview: () =>
    request.get<DashboardOverview>('/v1/dashboard/overview'),

  getPipelineStats: () =>
    request.get<PipelineStepStat[]>('/v1/dashboard/pipeline-stats'),

  getSystemMetrics: () =>
    request.get<SystemMetrics>('/v1/dashboard/system-metrics'),

  getRecentTasks: () =>
    request.get<TaskResponse[]>('/v1/dashboard/recent-tasks'),

  getAgentList: () =>
    request.get<AgentResponse[]>('/v1/dashboard/agent-list'),
}