import { request } from '../../lib/request'
import { USE_MOCK } from '../../config/runtime'
import {
  getBranches as getBranchesMock,
  getPullRequests as getPullRequestsMock,
  getRepositories as getRepositoriesMock,
  type BranchDto,
  type PullRequestDto,
  type RepositoryDto,
} from '../../mock'

export type { BranchDto, PullRequestDto, RepositoryDto }

export async function fetchRepositories(): Promise<RepositoryDto[]> {
  if (USE_MOCK) {
    return getRepositoriesMock()
  }
  const { data } = await request.get<RepositoryDto[]>('/repo-management/repositories')
  return data
}

export async function fetchBranches(): Promise<BranchDto[]> {
  if (USE_MOCK) {
    return getBranchesMock()
  }
  const { data } = await request.get<BranchDto[]>('/repo-management/branches')
  return data
}

export async function fetchPullRequests(): Promise<PullRequestDto[]> {
  if (USE_MOCK) {
    return getPullRequestsMock()
  }
  const { data } = await request.get<PullRequestDto[]>('/repo-management/pull-requests')
  return data
}
