/**
 * useRepoManagementStore 单元测试
 */
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useRepoManagementStore } from '../useRepoManagementStore'
import { useRepoStore } from '../../../../stores/useRepoStore'
import * as repoApi from '../../api/repo-management.api'
import type {
  RepositoryDto,
  GitBranchDto,
  GitPullRequestDto,
  PagedResult,
} from '../../api/repo-management.types'

describe('useRepoManagementStore', () => {
  // 测试数据
  const mockRepositories: RepositoryDto[] = [
    {
      id: 'repo-1',
      name: '订单服务',
      url: 'https://github.com/org/order-service',
      provider: 0,
      authType: 0,
      mergeStrategy: 1,
      branch: 'main',
      createdAtUtc: '2026-05-19T10:00:00Z',
      updatedAtUtc: '2026-05-20T10:00:00Z',
    },
    {
      id: 'repo-2',
      name: '用户服务',
      url: 'https://github.com/org/user-service',
      provider: 0,
      authType: 0,
      mergeStrategy: 1,
      branch: 'main',
      createdAtUtc: '2026-05-18T10:00:00Z',
      updatedAtUtc: '2026-05-19T10:00:00Z',
    },
  ]

  const mockBranches: GitBranchDto[] = [
    {
      name: 'main',
      commitSha: 'abc1234567890',
      isDefault: true,
    },
    {
      name: 'develop',
      commitSha: 'def1234567890',
      isDefault: false,
    },
  ]

  const mockPullRequests: GitPullRequestDto[] = [
    {
      id: 'pr-1',
      number: 1,
      title: '修复空指针异常',
      description: '修复导出流程中的空指针问题',
      state: 'open',
      author: 'developer1',
      sourceBranch: 'feature/fix-null',
      targetBranch: 'main',
      url: 'https://github.com/org/order-service/pull/1',
      createdAtUtc: '2026-05-21T10:00:00Z',
      updatedAtUtc: '2026-05-21T12:00:00Z',
    },
  ]

  // Spy objects
  let fetchRepositoriesSpy: ReturnType<typeof vi.spyOn>
  let fetchBranchesSpy: ReturnType<typeof vi.spyOn>
  let fetchPullRequestsSpy: ReturnType<typeof vi.spyOn>

  // Helper function to set store state
  function setStoreState(
    store: ReturnType<typeof useRepoManagementStore>,
    state: Partial<{
      repositories: RepositoryDto[]
      branches: GitBranchDto[]
      pullRequests: GitPullRequestDto[]
    }>,
  ) {
    store.$patch(state)
  }

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()

    fetchRepositoriesSpy = vi.spyOn(repoApi, 'fetchRepositories')
    fetchBranchesSpy = vi.spyOn(repoApi, 'fetchBranches')
    fetchPullRequestsSpy = vi.spyOn(repoApi, 'fetchPullRequests')
  })

  describe('初始状态', () => {
    it('should have correct initial state', () => {
      const store = useRepoManagementStore()

      expect(store.repositories).toEqual([])
      expect(store.branches).toEqual([])
      expect(store.pullRequests).toEqual([])
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
      expect(store.totalCount).toBe(0)
      expect(store.currentPage).toBe(1)
    })

    it('should have correct computed properties', () => {
      const store = useRepoManagementStore()
      expect(store.hasRepositories).toBe(false)
      expect(store.hasBranches).toBe(false)
      expect(store.hasPullRequests).toBe(false)
    })
  })

  describe('loadRepositories', () => {
    it('should load repositories successfully', async () => {
      const store = useRepoManagementStore()
      const pagedResult: PagedResult<RepositoryDto> = {
        items: mockRepositories,
        totalCount: 2,
        page: 1,
        pageSize: 20,
      }
      fetchRepositoriesSpy.mockResolvedValue(pagedResult)

      await store.loadRepositories()

      expect(store.repositories).toHaveLength(2)
      expect(store.totalCount).toBe(2)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
      expect(store.hasRepositories).toBe(true)
    })

    it('should set error on load failure', async () => {
      const store = useRepoManagementStore()
      fetchRepositoriesSpy.mockRejectedValue(new Error('加载仓库失败'))

      await store.loadRepositories()

      expect(store.error).toBe('加载仓库失败')
      expect(store.loading).toBe(false)
    })

    it('should use pagination parameters', async () => {
      const store = useRepoManagementStore()
      fetchRepositoriesSpy.mockResolvedValue({ items: [], totalCount: 0, page: 2, pageSize: 10 })

      await store.loadRepositories(2)

      expect(store.currentPage).toBe(2)
    })
  })

  describe('loadBranches', () => {
    it('should load branches successfully', async () => {
      const store = useRepoManagementStore()
      setStoreState(store, { repositories: mockRepositories })
      fetchBranchesSpy.mockResolvedValue(mockBranches)

      await store.loadBranches()

      expect(store.branches).toHaveLength(2)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
      expect(store.hasBranches).toBe(true)
    })

    it('should set error on load failure', async () => {
      const store = useRepoManagementStore()
      fetchBranchesSpy.mockRejectedValue(new Error('加载分支失败'))

      await store.loadBranches()

      expect(store.error).toBe('加载分支失败')
      expect(store.loading).toBe(false)
    })
  })

  describe('loadPullRequests', () => {
    it('should load pull requests successfully', async () => {
      const store = useRepoManagementStore()
      fetchPullRequestsSpy.mockResolvedValue(mockPullRequests)

      await store.loadPullRequests()

      expect(store.pullRequests).toHaveLength(1)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
      expect(store.hasPullRequests).toBe(true)
    })

    it('should set error on load failure', async () => {
      const store = useRepoManagementStore()
      fetchPullRequestsSpy.mockRejectedValue(new Error('加载PR失败'))

      await store.loadPullRequests()

      expect(store.error).toBe('加载PR失败')
      expect(store.loading).toBe(false)
    })
  })
})
