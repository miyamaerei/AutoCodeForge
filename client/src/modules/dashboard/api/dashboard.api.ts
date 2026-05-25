import { API_BASE_URL, request } from '@/lib/request'

interface ApiEnvelopeDto<T> {
  success: boolean
  message: string
  data: T
}

export interface AgentStatsDto {
  total: number
  idle: number
  handling: number
  learning: number
  dormant: number
}

export interface TaskStatsDto {
  total: number
  pending: number
  running: number
  completed: number
  failed: number
  paused: number
  canceled: number
}

export interface GateStatsDto {
  pendingCount: number
  byType: Record<string, number>
}

export interface DashboardSnapshotDto {
  agentStats: AgentStatsDto
  taskStats: TaskStatsDto
  gateStats: GateStatsDto
  lastUpdated: string
}

export interface DashboardTaskLiveDto {
  id: string
  title: string
  status: string
  progress: number
  currentStep: number
  currentStepName: string
  agentId: string | null
  agentName: string | null
  errorMessage: string | null
  isTimeout: boolean
  hasRejectedGate: boolean
  hasEmergencyGate: boolean
  alertTags: string[]
  alertLevel: 'normal' | 'warning' | 'critical'
  updatedAtUtc: string
}

export interface DashboardAgentLiveDto {
  id: string
  name: string
  role: string
  state: string
  workload: number
  workstationStep: number
  workstationName: string
  currentTaskId: string | null
  dormantReason: string | null
  updatedAtUtc: string
}

export interface DashboardLogItemDto {
  time: string
  type: 'task' | 'agent' | 'system'
  taskId: string | null
  agentId: string | null
  content: string
  level: 'info' | 'warning' | 'error'
}

export interface DashboardLivePayloadDto {
  snapshot: DashboardSnapshotDto
  tasks: DashboardTaskLiveDto[]
  agents: DashboardAgentLiveDto[]
  generatedAtUtc: string
}

interface DashboardLiveStreamOptions {
  intervalMs?: number
  onOpen?: () => void
  onLive: (payload: DashboardLivePayloadDto) => void
  onError?: (event: Event) => void
}

interface DashboardLogsStreamOptions {
  type: 'task' | 'agent' | 'system'
  taskId?: string
  agentId?: string
  intervalMs?: number
  onOpen?: () => void
  onLogs: (payload: DashboardLogItemDto[]) => void
  onError?: (event: Event) => void
}

function unwrapEnvelope<T>(responseData: ApiEnvelopeDto<T> | T): T {
  if (
    typeof responseData === 'object'
    && responseData !== null
    && 'success' in responseData
    && 'data' in responseData
  ) {
    return (responseData as ApiEnvelopeDto<T>).data
  }

  return responseData as T
}

async function getPayload<T>(url: string, params?: Record<string, string>): Promise<T> {
  const response = await request.get<ApiEnvelopeDto<T> | T>(url, { params })
  return unwrapEnvelope<T>(response.data)
}

function buildLiveStreamUrl(intervalMs?: number): string {
  const normalizedBase = API_BASE_URL.replace(/\/+$/, '')
  const streamUrl = new URL(`${normalizedBase}/dashboard/live/stream`, window.location.origin)

  if (intervalMs) {
    streamUrl.searchParams.set('intervalMs', String(intervalMs))
  }

  return streamUrl.toString()
}

function buildLogsStreamUrl(options: DashboardLogsStreamOptions): string {
  const normalizedBase = API_BASE_URL.replace(/\/+$/, '')
  const streamUrl = new URL(`${normalizedBase}/dashboard/logs/stream`, window.location.origin)
  streamUrl.searchParams.set('type', options.type)

  if (options.taskId) {
    streamUrl.searchParams.set('taskId', options.taskId)
  }

  if (options.agentId) {
    streamUrl.searchParams.set('agentId', options.agentId)
  }

  if (options.intervalMs) {
    streamUrl.searchParams.set('intervalMs', String(options.intervalMs))
  }

  return streamUrl.toString()
}

export const dashboardApi = {
  getSnapshot: () => getPayload<DashboardSnapshotDto>('/dashboard/snapshot'),
  getTasks: () => getPayload<DashboardTaskLiveDto[]>('/dashboard/tasks'),
  getAgents: () => getPayload<DashboardAgentLiveDto[]>('/dashboard/agents'),
  getLogs: (params: {
    type: 'task' | 'agent' | 'system'
    taskId?: string
    agentId?: string
  }) => getPayload<DashboardLogItemDto[]>('/dashboard/logs', {
    type: params.type,
    ...(params.taskId ? { taskId: params.taskId } : {}),
    ...(params.agentId ? { agentId: params.agentId } : {}),
  }),
  connectLiveStream: (options: DashboardLiveStreamOptions): (() => void) => {
    const source = new EventSource(buildLiveStreamUrl(options.intervalMs))

    const handleLive = (event: Event) => {
      const message = event as MessageEvent<string>
      if (!message.data) {
        return
      }

      try {
        const payload = JSON.parse(message.data) as DashboardLivePayloadDto
        options.onLive(payload)
      } catch (error) {
        console.error('Failed to parse dashboard live stream payload:', error)
      }
    }

    source.onopen = () => {
      options.onOpen?.()
    }

    source.onerror = (event) => {
      options.onError?.(event)
    }

    source.addEventListener('live', handleLive)

    return () => {
      source.removeEventListener('live', handleLive)
      source.close()
    }
  },
  connectLogsStream: (options: DashboardLogsStreamOptions): (() => void) => {
    const source = new EventSource(buildLogsStreamUrl(options))

    const handleLogs = (event: Event) => {
      const message = event as MessageEvent<string>
      if (!message.data) {
        return
      }

      try {
        const payload = JSON.parse(message.data) as DashboardLogItemDto[]
        options.onLogs(payload)
      } catch (error) {
        console.error('Failed to parse dashboard logs stream payload:', error)
      }
    }

    source.onopen = () => {
      options.onOpen?.()
    }

    source.onerror = (event) => {
      options.onError?.(event)
    }

    source.addEventListener('logs', handleLogs)

    return () => {
      source.removeEventListener('logs', handleLogs)
      source.close()
    }
  },
}
