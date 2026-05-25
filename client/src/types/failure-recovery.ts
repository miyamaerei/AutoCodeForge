export type FailureCategory = 
  | 'CodeError'
  | 'LlmException'
  | 'RequirementIssue'
  | 'ReviewRejection'
  | 'Timeout'
  | 'Unknown'

export type DegradationAction = 
  | 'LogOnly'
  | 'Fallback'
  | 'Terminate'
  | 'TriggerHumanGate'

export type RecoveryStatus = 
  | 'Success'
  | 'Retry'
  | 'Degradation'
  | 'Failure'

export interface RetryPolicy {
  maxRetries: number
  intervalMs: number
  backoffMultiplier: number
  degradationAction: DegradationAction
}

export interface RetryPolicySettings {
  retryPolicies: Record<FailureCategory, RetryPolicy>
}

export interface RecoverRequest {
  stepId: string
  failureCategory: FailureCategory
  errorMessage?: string
}

export interface RecoveryResult {
  status: RecoveryStatus
  message: string
  retryCount: number
  retryDelayMs: number
  degradationAction?: DegradationAction
}

export interface StuckStepInfo {
  id: string
  taskId: string
  step: number
  status: string
  createdAtUtc: string
  updatedAtUtc: string
}

export interface FailureHistoryItem {
  id: string
  taskId: string
  stepId: string
  failureCategory: FailureCategory
  errorMessage: string
  occurredAtUtc: string
}

export interface FailureStats {
  totalFailures: number
  byCategory: Record<FailureCategory, number>
  last24Hours: number
}

export interface ApiEnvelope<T> {
  success: boolean
  message: string
  data: T
  traceId?: string
}