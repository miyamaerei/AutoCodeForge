import { request } from '../../../lib/request'
import { USE_MOCK } from '../../../config/runtime'
import {
  getBranches as getBranchesMock,
  getPullRequests as getPullRequestsMock,
  getRepositories as getRepositoriesMock,
} from '../../../mock'
import type {
  ApiResponse,
  CreateRepoSyncTaskRequest,
  CreateGitPullRequestRequest,
  CreateRepositoryRequest,
  GitBranchDto,
  GitCommitDto,
  GitPullRequestDto,
  PagedResult,
  RemoteGitRepositoryDto,
  RepoSyncTaskDetailDto,
  RepoSyncTaskResponseDto,
  RepositoryDto,
  UpdateRepositoryRequest,
} from './repo-management.types'

export type {
  CreateRepoSyncTaskRequest,
  GitBranchDto,
  GitCommitDto,
  GitPullRequestDto,
  RepositoryDto,
  RepoSyncTaskDetailDto,
  RepoSyncTaskResponseDto,
  CreateRepositoryRequest,
  RemoteGitRepositoryDto,
  UpdateRepositoryRequest,
}

function unwrapApiResponse<T>(payload: ApiResponse<T> | T): T {
  if (payload && typeof payload === 'object' && 'data' in payload) {
    return (payload as ApiResponse<T>).data
  }
  return payload as T
}

interface GitHubRepoResponse {
  id: number
  name: string
  full_name: string
  html_url: string
  default_branch: string
  private: boolean
  owner?: {
    login?: string
  }
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
  const { data } = await request.get<ApiResponse<PagedResult<RepositoryDto>>>('/v1/repositories', {
    params: { page, pageSize },
  })
  const result = unwrapApiResponse(data)
  // 确保返回的数据结构正确，防止 undefined 访问
  return {
    items: result?.items || [],
    totalCount: result?.totalCount || 0,
    page: result?.page || page,
    pageSize: result?.pageSize || pageSize,
  }
}

export async function fetchRepository(id: string): Promise<RepositoryDto> {
  const { data } = await request.get<ApiResponse<RepositoryDto>>(`/v1/repositories/${id}`)
  return unwrapApiResponse(data)
}

export async function fetchBranches(repositoryId: string): Promise<GitBranchDto[]> {
  if (USE_MOCK) {
    return getBranchesMock() as unknown as GitBranchDto[]
  }
  if (!repositoryId) throw new Error('repositoryId is required to fetch branches')
  const { data } = await request.get<ApiResponse<GitBranchDto[]>>(`/v1/repositories/${repositoryId}/branches`)
  return unwrapApiResponse(data) || []
}

export async function fetchCommits(repositoryId: string, branch = 'main', limit = 10): Promise<GitCommitDto[]> {
  if (!repositoryId) throw new Error('repositoryId is required to fetch commits')
  const { data } = await request.get<ApiResponse<GitCommitDto[]>>(`/v1/repositories/${repositoryId}/commits`, {
    params: { branch, limit },
  })
  return unwrapApiResponse(data) || []
}

export async function fetchPullRequests(repositoryId: string, state = 'open', limit = 20): Promise<GitPullRequestDto[]> {
  if (USE_MOCK) {
    return getPullRequestsMock() as unknown as GitPullRequestDto[]
  }
  if (!repositoryId) throw new Error('repositoryId is required to fetch pull requests')
  const { data } = await request.get<ApiResponse<GitPullRequestDto[]>>(`/v1/repositories/${repositoryId}/pull-requests`, {
    params: { state, limit },
  })
  return unwrapApiResponse(data) || []
}

export async function createRepository(payload: CreateRepositoryRequest): Promise<RepositoryDto> {
  const { data } = await request.post<ApiResponse<RepositoryDto>>('/v1/repositories', payload)
  return unwrapApiResponse(data)
}

export async function createPullRequest(repositoryId: string, payload: CreateGitPullRequestRequest): Promise<GitPullRequestDto> {
  if (!repositoryId) throw new Error('repositoryId is required to create pull request')
  const requestBody = {
    Title: payload.title,
    Description: payload.description,
    SourceBranch: payload.sourceBranch,
    TargetBranch: payload.targetBranch,
  }
  const { data } = await request.post<ApiResponse<GitPullRequestDto>>(`/v1/repositories/${repositoryId}/pull-requests`, requestBody)
  return unwrapApiResponse(data)
}

export async function updateRepository(id: string, payload: UpdateRepositoryRequest): Promise<RepositoryDto> {
  const { data } = await request.put<ApiResponse<RepositoryDto>>(`/v1/repositories/${id}`, payload)
  return unwrapApiResponse(data)
}

export async function deleteRepository(id: string): Promise<void> {
  await request.delete(`/v1/repositories/${id}`)
}

export async function createRepoSyncTask(payload: CreateRepoSyncTaskRequest): Promise<RepoSyncTaskResponseDto> {
  const { data } = await request.post<ApiResponse<RepoSyncTaskResponseDto>>('/v1/repo-sync/tasks', payload)
  return unwrapApiResponse(data)
}

export async function fetchRepoSyncTaskDetail(taskId: string): Promise<RepoSyncTaskDetailDto> {
  if (!taskId) throw new Error('taskId is required to fetch repo sync detail')
  const { data } = await request.get<ApiResponse<RepoSyncTaskDetailDto>>(`/v1/repo-sync/tasks/${taskId}`)
  return unwrapApiResponse(data)
}

export async function cancelRepoSyncTask(taskId: string): Promise<RepoSyncTaskDetailDto> {
  if (!taskId) throw new Error('taskId is required to cancel repo sync task')
  const { data } = await request.post<ApiResponse<RepoSyncTaskDetailDto>>(`/v1/repo-sync/tasks/${taskId}/cancel`)
  return unwrapApiResponse(data)
}

export async function fetchGitHubRepositoriesByToken(token: string, perPage = 100): Promise<RemoteGitRepositoryDto[]> {
  const normalizedToken = token.trim()
  if (!normalizedToken) {
    throw new Error('Git token is required to fetch repositories')
  }

  const response = await fetch(`https://api.github.com/user/repos?sort=updated&per_page=${Math.min(perPage, 100)}`, {
    method: 'GET',
    headers: {
      Accept: 'application/vnd.github+json',
      Authorization: `token ${normalizedToken}`,
      'X-GitHub-Api-Version': '2022-11-28',
    },
  })

  if (!response.ok) {
    if (response.status === 401 || response.status === 403) {
      throw new Error('Git Token 无效或无权限，请检查后重试')
    }
    throw new Error(`拉取 GitHub 仓库失败，状态码 ${response.status}`)
  }

  const data = (await response.json()) as GitHubRepoResponse[]
  return (data || []).map((item) => ({
    id: item.id,
    name: item.name,
    fullName: item.full_name,
    htmlUrl: item.html_url,
    defaultBranch: item.default_branch || 'main',
    ownerLogin: item.owner?.login || item.full_name.split('/')[0] || '',
    isPrivate: Boolean(item.private),
  }))
}
