import { request } from '../../../lib/request'
import { USE_MOCK } from '../../../config/runtime'
import {
  getBranches as getBranchesMock,
  getPullRequests as getPullRequestsMock,
  getRepositories as getRepositoriesMock,
} from '../../../mock'
import type {
  CreateGitPullRequestRequest,
  CreateRepositoryRequest,
  GitBranchDto,
  GitCommitDto,
  GitPullRequestDto,
  PagedResult,
  RepositoryDto,
  UpdateRepositoryRequest,
} from './repo-management.types'

export type {
  GitBranchDto,
  GitCommitDto,
  GitPullRequestDto,
  RepositoryDto,
  CreateRepositoryRequest,
  UpdateRepositoryRequest,
}

export async function fetchRepositories(page = 1, pageSize = 20): Promise<PagedResult<RepositoryDto>> {
  if (USE_MOCK) {
    const mockData = await getRepositoriesMock()
    return {
      items: mockData as unknown as RepositoryDto[],
      totalCount: mockData.length,
      page,
      pageSize,
    }
  }
  const { data } = await request.get<PagedResult<RepositoryDto>>('/v1/repositories', {
    params: { page, pageSize },
  })
  // 确保返回的数据结构正确，防止 undefined 访问
  return {
    items: data?.items || [],
    totalCount: data?.totalCount || 0,
    page: data?.page || page,
    pageSize: data?.pageSize || pageSize,
  }
}

export async function fetchRepository(id: string): Promise<RepositoryDto> {
  const { data } = await request.get<RepositoryDto>(`/v1/repositories/${id}`)
  return data
}

export async function fetchBranches(repositoryId: string): Promise<GitBranchDto[]> {
  if (USE_MOCK) {
    return getBranchesMock() as unknown as GitBranchDto[]
  }
  if (!repositoryId) throw new Error('repositoryId is required to fetch branches')
  const { data } = await request.get<GitBranchDto[]>(`/v1/repositories/${repositoryId}/branches`)
  return data || []
}

export async function fetchCommits(repositoryId: string, branch = 'main', limit = 10): Promise<GitCommitDto[]> {
  if (!repositoryId) throw new Error('repositoryId is required to fetch commits')
  const { data } = await request.get<GitCommitDto[]>(`/v1/repositories/${repositoryId}/commits`, {
    params: { branch, limit },
  })
  return data || []
}

export async function fetchPullRequests(repositoryId: string, state = 'open', limit = 20): Promise<GitPullRequestDto[]> {
  if (USE_MOCK) {
    return getPullRequestsMock() as unknown as GitPullRequestDto[]
  }
  if (!repositoryId) throw new Error('repositoryId is required to fetch pull requests')
  const { data } = await request.get<GitPullRequestDto[]>(`/v1/repositories/${repositoryId}/pull-requests`, {
    params: { state, limit },
  })
  return data || []
}

export async function createRepository(payload: CreateRepositoryRequest): Promise<RepositoryDto> {
  const { data } = await request.post<RepositoryDto>('/v1/repositories', payload)
  return data
}

export async function createPullRequest(repositoryId: string, payload: CreateGitPullRequestRequest): Promise<GitPullRequestDto> {
  if (!repositoryId) throw new Error('repositoryId is required to create pull request')
  const { data } = await request.post<GitPullRequestDto>(`/v1/repositories/${repositoryId}/pull-requests`, payload)
  return data
}

export async function updateRepository(id: string, payload: UpdateRepositoryRequest): Promise<RepositoryDto> {
  const { data } = await request.put<RepositoryDto>(`/v1/repositories/${id}`, payload)
  return data
}

export async function deleteRepository(id: string): Promise<void> {
  await request.delete(`/v1/repositories/${id}`)
}
