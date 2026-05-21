/**
 * Project Mock 单元测试
 */
import { describe, it, expect, vi, beforeEach } from 'vitest'
import {
  getRepositories,
  getBranches,
  getPullRequests,
  getPipelines,
  getBuilds,
  type RepositoryDto,
  type BranchDto,
  type PullRequestDto,
  type PipelineDto,
  type BuildDto,
} from '../project'

// Mock window.setTimeout
const setTimeoutMock = vi.fn((callback: () => void) => {
  callback()
  return 0
})
vi.stubGlobal('setTimeout', setTimeoutMock)

describe('project mock', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('getRepositories', () => {
    it('should return list of repositories', async () => {
      const repos = await getRepositories()

      expect(Array.isArray(repos)).toBe(true)
      expect(repos.length).toBeGreaterThan(0)
    })

    it('should return repositories with correct structure', async () => {
      const repos = await getRepositories()

      repos.forEach((repo: RepositoryDto) => {
        expect(repo).toHaveProperty('id')
        expect(repo).toHaveProperty('name')
        expect(repo).toHaveProperty('url')
        expect(repo).toHaveProperty('branch')
        expect(repo).toHaveProperty('lastUpdate')
      })
    })

    it('should contain AutoCodeForge repository', async () => {
      const repos = await getRepositories()
      const autoCodeForge = repos.find((r: RepositoryDto) => r.name === 'AutoCodeForge')

      expect(autoCodeForge).toBeDefined()
      expect(autoCodeForge?.url).toBe('https://github.com/org/AutoCodeForge')
      expect(autoCodeForge?.branch).toBe('main')
    })
  })

  describe('getBranches', () => {
    it('should return list of branches', async () => {
      const branches = await getBranches()

      expect(Array.isArray(branches)).toBe(true)
      expect(branches.length).toBeGreaterThan(0)
    })

    it('should return branches with correct structure', async () => {
      const branches = await getBranches()

      branches.forEach((branch: BranchDto) => {
        expect(branch).toHaveProperty('name')
        expect(branch).toHaveProperty('commit')
        expect(branch).toHaveProperty('author')
        expect(branch).toHaveProperty('lastUpdate')
      })
    })

    it('should contain main branch', async () => {
      const branches = await getBranches()
      const main = branches.find((b: BranchDto) => b.name === 'main')

      expect(main).toBeDefined()
      expect(main?.author).toBe('Alice Chen')
    })
  })

  describe('getPullRequests', () => {
    it('should return list of pull requests', async () => {
      const prs = await getPullRequests()

      expect(Array.isArray(prs)).toBe(true)
      expect(prs.length).toBeGreaterThan(0)
    })

    it('should return PRs with correct structure', async () => {
      const prs = await getPullRequests()

      prs.forEach((pr: PullRequestDto) => {
        expect(pr).toHaveProperty('id')
        expect(pr).toHaveProperty('title')
        expect(pr).toHaveProperty('author')
        expect(pr).toHaveProperty('status')
        expect(pr).toHaveProperty('createdAt')
        expect(pr).toHaveProperty('updatedAt')
      })
    })

    it('should contain open PRs', async () => {
      const prs = await getPullRequests()
      const openPR = prs.find((pr: PullRequestDto) => pr.status === 'open')

      expect(openPR).toBeDefined()
    })

    it('should contain merged PRs', async () => {
      const prs = await getPullRequests()
      const mergedPR = prs.find((pr: PullRequestDto) => pr.status === 'merged')

      expect(mergedPR).toBeDefined()
    })
  })

  describe('getPipelines', () => {
    it('should return list of pipelines', async () => {
      const pipelines = await getPipelines()

      expect(Array.isArray(pipelines)).toBe(true)
      expect(pipelines.length).toBeGreaterThan(0)
    })

    it('should return pipelines with correct structure', async () => {
      const pipelines = await getPipelines()

      pipelines.forEach((pipeline: PipelineDto) => {
        expect(pipeline).toHaveProperty('id')
        expect(pipeline).toHaveProperty('name')
        expect(pipeline).toHaveProperty('status')
        expect(pipeline).toHaveProperty('lastRun')
        expect(pipeline).toHaveProperty('successRate')
        expect(pipeline).toHaveProperty('branch')
      })
    })

    it('should have valid status values', async () => {
      const pipelines = await getPipelines()
      const validStatuses = ['success', 'running', 'failed']

      pipelines.forEach((pipeline: PipelineDto) => {
        expect(validStatuses).toContain(pipeline.status)
      })
    })
  })

  describe('getBuilds', () => {
    it('should return list of builds', async () => {
      const builds = await getBuilds()

      expect(Array.isArray(builds)).toBe(true)
      expect(builds.length).toBeGreaterThan(0)
    })

    it('should return builds with correct structure', async () => {
      const builds = await getBuilds()

      builds.forEach((build: BuildDto) => {
        expect(build).toHaveProperty('id')
        expect(build).toHaveProperty('pipelineName')
        expect(build).toHaveProperty('buildNumber')
        expect(build).toHaveProperty('status')
        expect(build).toHaveProperty('startTime')
        expect(build).toHaveProperty('duration')
        expect(build).toHaveProperty('branch')
      })
    })

    it('should have valid status values', async () => {
      const builds = await getBuilds()
      const validStatuses = ['success', 'running', 'failed']

      builds.forEach((build: BuildDto) => {
        expect(validStatuses).toContain(build.status)
      })
    })
  })
})
