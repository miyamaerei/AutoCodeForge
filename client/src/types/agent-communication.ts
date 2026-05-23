export type ArtifactType = 'code' | 'test' | 'document' | 'other'

export type ArtifactFormat = 'json' | 'text' | 'binary'

export interface ArtifactItem {
  type: ArtifactType
  name: string
  content: string
  format: ArtifactFormat
}

export interface ArtifactMetrics {
  executionTimeMs: number
  tokenUsage: number
}

export interface ArtifactContract {
  id: string
  taskId: string
  stepId: string
  agentId: string
  stepName: string
  artifacts: ArtifactItem[]
  summary: string
  issues: string[]
  metrics: ArtifactMetrics
  createdAtUtc: string
}

export interface EventPublishRequest {
  taskId: string
  stepId?: string
  message?: string
}

export interface StoreArtifactRequest {
  taskId: string
  stepId: string
  agentId: string
  stepName?: string
  artifacts?: ArtifactItem[]
  summary?: string
  issues?: string[]
  metrics?: ArtifactMetrics
}

export interface ApiEnvelope<T> {
  success: boolean
  message: string
  data: T
  traceId?: string
}