import { request } from '../../lib/request'
import { USE_MOCK } from '../../config/runtime'
import {
  getBuilds as getBuildsMock,
  getPipelines as getPipelinesMock,
  type BuildDto,
  type PipelineDto,
} from '../../mock'

export type { BuildDto, PipelineDto }

export async function fetchPipelines(): Promise<PipelineDto[]> {
  if (USE_MOCK) {
    return getPipelinesMock()
  }
  const { data } = await request.get<PipelineDto[]>('/pipeline-center/pipelines')
  return data
}

export async function fetchBuilds(): Promise<BuildDto[]> {
  if (USE_MOCK) {
    return getBuildsMock()
  }
  const { data } = await request.get<BuildDto[]>('/pipeline-center/builds')
  return data
}
