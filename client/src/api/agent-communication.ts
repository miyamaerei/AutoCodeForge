import { request } from '@/lib/request'
import type {
  ApiEnvelope,
  ArtifactContract,
  EventPublishRequest,
  StoreArtifactRequest,
} from '@/types/agent-communication'

export async function publishEvent(requestBody: EventPublishRequest): Promise<void> {
  await request.post<ApiEnvelope<void>>(
    '/api/v1/communication/events',
    requestBody,
  )
}

export async function storeArtifact(
  requestBody: StoreArtifactRequest,
): Promise<ArtifactContract> {
  const { data } = await request.post<ApiEnvelope<ArtifactContract>>(
    '/api/v1/artifacts',
    requestBody,
  )
  return data.data
}

export async function getArtifact(artifactId: string): Promise<ArtifactContract> {
  const { data } = await request.get<ApiEnvelope<ArtifactContract>>(
    `/api/v1/artifacts/${artifactId}`,
  )
  return data.data
}

export async function listArtifactsByTask(taskId: string): Promise<ArtifactContract[]> {
  const { data } = await request.get<ApiEnvelope<ArtifactContract[]>>(
    `/api/v1/artifacts/task/${taskId}`,
  )
  return data.data
}

export async function deleteArtifact(artifactId: string): Promise<void> {
  await request.delete(`/api/v1/artifacts/${artifactId}`)
}