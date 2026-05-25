import { request } from '@/lib/request'
import type {
  ApiEnvelope,
  RecoverRequest,
  RecoveryResult,
  StuckStepInfo,
  FailureHistoryItem,
  FailureStats,
} from '@/types/failure-recovery'

export async function recoverFailure(
  requestBody: RecoverRequest,
): Promise<RecoveryResult> {
  const { data } = await request.post<ApiEnvelope<RecoveryResult>>(
    '/api/v1/failure/recover',
    requestBody,
  )
  return data.data
}

export async function detectStuckSteps(
  timeoutMinutes?: number,
): Promise<StuckStepInfo[]> {
  const params = timeoutMinutes ? { timeoutMinutes } : {}
  const { data } = await request.get<ApiEnvelope<StuckStepInfo[]>>(
    '/api/v1/failure/stuck-steps',
    { params },
  )
  return data.data
}

export async function emergencyUnbind(stepId: string): Promise<RecoveryResult> {
  const { data } = await request.post<ApiEnvelope<RecoveryResult>>(
    `/api/v1/failure/emergency-unbind/${stepId}`,
  )
  return data.data
}

export async function getFailureHistory(): Promise<FailureHistoryItem[]> {
  const { data } = await request.get<ApiEnvelope<FailureHistoryItem[]>>(
    '/api/v1/failure/history',
  )
  return data.data
}

export async function getFailureStats(): Promise<FailureStats> {
  const { data } = await request.get<ApiEnvelope<FailureStats>>(
    '/api/v1/failure/stats',
  )
  return data.data
}