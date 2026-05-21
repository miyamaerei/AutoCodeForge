export interface RepositoryDto {
  id: string
  name: string
  url: string
  provider: GitProvider
  authType: AuthenticationType
  mergeStrategy: MergeStrategy
  defaultReviewRuleSetId?: string
  createdAtUtc: string
  updatedAtUtc: string
}

export interface GitBranchDto {
  name: string
  commitSha: string
  isDefault: boolean
}

export interface GitCommitDto {
  sha: string
  message: string
  author: string
  createdAtUtc: string
}

export interface GitPullRequestDto {
  id: string
  number: number
  title: string
  description?: string
  sourceBranch: string
  targetBranch: string
  state: string
  author: string
  url: string
  createdAtUtc: string
  updatedAtUtc: string
}

export interface CreateRepositoryRequest {
  name: string
  url: string
  provider?: GitProvider
  authType?: AuthenticationType
  token: string
  mergeStrategy?: MergeStrategy
}

export interface UpdateRepositoryRequest {
  name?: string
  mergeStrategy?: MergeStrategy
}

export interface RemoteGitRepositoryDto {
  id: number
  name: string
  fullName: string
  htmlUrl: string
  defaultBranch: string
  ownerLogin: string
  isPrivate: boolean
}

export interface CreateGitPullRequestRequest {
  title: string
  description?: string
  sourceBranch: string
  targetBranch: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export type GitProvider = 'GitHub' | 'GitLab' | 'AzureDevOps' | 'Local'

export type AuthenticationType = 'Token' | 'UsernamePassword' | 'SSH'

export type MergeStrategy = 'Squash' | 'Merge' | 'Rebase'