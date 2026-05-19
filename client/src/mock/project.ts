export interface RepositoryDto {
  id: string
  name: string
  url: string
  branch: string
  lastUpdate: string
}

export interface BranchDto {
  name: string
  commit: string
  author: string
  lastUpdate: string
}

export interface PullRequestDto {
  id: string
  title: string
  author: string
  status: 'open' | 'merged' | 'closed'
  createdAt: string
  updatedAt: string
}

export interface PipelineDto {
  id: string
  name: string
  status: 'success' | 'running' | 'failed'
  lastRun: string
  successRate: string
  branch: string
}

export interface BuildDto {
  id: string
  pipelineName: string
  buildNumber: string
  status: 'success' | 'running' | 'failed'
  startTime: string
  duration: string
  branch: string
}

const repositories: RepositoryDto[] = [
  {
    id: 'repo-1',
    name: 'AutoCodeForge',
    url: 'https://github.com/org/AutoCodeForge',
    branch: 'main',
    lastUpdate: '2024-05-19 10:30',
  },
  {
    id: 'repo-2',
    name: 'backend-service',
    url: 'https://github.com/org/backend-service',
    branch: 'develop',
    lastUpdate: '2024-05-18 16:45',
  },
]

const branches: BranchDto[] = [
  { name: 'main', commit: 'abc1234', author: 'Alice Chen', lastUpdate: '2024-05-19 10:30' },
  { name: 'develop', commit: 'def5678', author: 'Bob Smith', lastUpdate: '2024-05-19 09:15' },
  {
    name: 'feature/order-export',
    commit: 'ghi9101',
    author: 'Carol Wang',
    lastUpdate: '2024-05-18 16:45',
  },
]

const pullRequests: PullRequestDto[] = [
  {
    id: 'PR-145',
    title: '修复订单导出功能的空指针异常',
    author: 'Alice Chen',
    status: 'open',
    createdAt: '2024-05-19 08:00',
    updatedAt: '2024-05-19 14:30',
  },
  {
    id: 'PR-144',
    title: '优化数据库连接池配置',
    author: 'Bob Smith',
    status: 'merged',
    createdAt: '2024-05-18 10:00',
    updatedAt: '2024-05-18 18:45',
  },
]

const pipelines: PipelineDto[] = [
  {
    id: 'pipe-1',
    name: 'CI/CD - Main Branch',
    status: 'success',
    lastRun: '2024-05-19 14:30',
    successRate: '95%',
    branch: 'main',
  },
  {
    id: 'pipe-2',
    name: 'CI/CD - Develop Branch',
    status: 'running',
    lastRun: '2024-05-19 14:45',
    successRate: '88%',
    branch: 'develop',
  },
]

const builds: BuildDto[] = [
  {
    id: 'build-1',
    pipelineName: 'CI/CD - Main Branch',
    buildNumber: '#456',
    status: 'success',
    startTime: '2024-05-19 14:30',
    duration: '3m 45s',
    branch: 'main',
  },
  {
    id: 'build-2',
    pipelineName: 'CI/CD - Develop Branch',
    buildNumber: '#455',
    status: 'running',
    startTime: '2024-05-19 14:45',
    duration: '2m 30s',
    branch: 'develop',
  },
]

export async function getRepositories(): Promise<RepositoryDto[]> {
  await sleep(220)
  return repositories
}

export async function getBranches(): Promise<BranchDto[]> {
  await sleep(200)
  return branches
}

export async function getPullRequests(): Promise<PullRequestDto[]> {
  await sleep(240)
  return pullRequests
}

export async function getPipelines(): Promise<PipelineDto[]> {
  await sleep(180)
  return pipelines
}

export async function getBuilds(): Promise<BuildDto[]> {
  await sleep(180)
  return builds
}

async function sleep(ms: number): Promise<void> {
  await new Promise((resolve) => {
    setTimeout(resolve, ms)
  })
}
