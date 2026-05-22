export interface RepositoryDto {
  id: string
  name: string
  url: string
  provider: GitProvider
  authType: AuthenticationType
  mergeStrategy: MergeStrategy
  defaultReviewRuleSetId?: string | null
  branch?: string | null
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
  branch?: string
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

export interface CreateRepoSyncTaskRequest {
  repositoryId: string
  branch?: string
  title?: string
  description?: string
}

export interface RepoSyncTaskResponseDto {
  id: string
  title: string
  description?: string
  status: string
  taskType: string
  progress: number
  input?: string
  result?: string
  errorMessage?: string
  agentId?: string
  dueAtUtc?: string
  startedAtUtc?: string
  completedAtUtc?: string
  createdAtUtc: string
  updatedAtUtc: string
}

export interface RepoSyncTaskDetailDto {
  taskId: string
  status: string
  workspaceStatus: string
  effectiveSandboxPath: string
  branch?: string
  commitSha?: string
  errorMessage?: string
}

export interface ApiResponse<T> {
  success: boolean
  message: string
  data: T
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export enum GitProvider {
  GitHub = 0,
  GitLab = 1,
  AzureDevOps = 2,
  Bitbucket = 3,
}

export enum AuthenticationType {
  Token = 0,
  SshKey = 1,
  UsernamePassword = 2,
}

export enum MergeStrategy {
  MergeCommit = 0,
  Squash = 1,
  Rebase = 2,
}