import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { useRepoStore } from '../../../stores/useRepoStore'
import {
  createRepository,
  deleteRepository,
  fetchBranches,
  fetchPullRequests,
  fetchRepositories,
  updateRepository,
  type GitBranchDto,
  type GitPullRequestDto,
  type RepositoryDto,
  type CreateRepositoryRequest,
  type UpdateRepositoryRequest,
} from '../api/repo-management.api'

/**
 * 适配仓库数据，添加 UI 需要的字段
 */
function adaptRepositoryData(repo: RepositoryDto) {
  return {
    ...repo,
    branch: 'main', // 默认分支，可从其他接口获取
    lastUpdate: new Date(repo.updatedAtUtc).toLocaleString(),
  }
}

/**
 * 适配分支数据，添加 UI 需要的字段
 */
function adaptBranchData(branch: GitBranchDto) {
  return {
    ...branch,
    commit: branch.commitSha.substring(0, 7),
    author: '未知',
    lastUpdate: '-',
  }
}

/**
 * 适配 PR 数据，添加 UI 需要的字段
 */
function adaptPullRequestData(pr: GitPullRequestDto) {
  return {
    ...pr,
    status: pr.state,
    createdAt: new Date(pr.createdAtUtc).toLocaleString(),
    updatedAt: new Date(pr.updatedAtUtc).toLocaleString(),
  }
}

export const useRepoManagementStore = defineStore('module.repo-management', () => {
  const repositories = ref<RepositoryDto[]>([])
  const branches = ref<GitBranchDto[]>([])
  const pullRequests = ref<GitPullRequestDto[]>([])
  const repoGlobal = useRepoStore()

  const loading = ref(false)
  const error = ref<string | null>(null)
  const totalCount = ref(0)
  const currentPage = ref(1)

  const hasRepositories = computed(() => repositories.value.length > 0)
  const hasBranches = computed(() => branches.value.length > 0)
  const hasPullRequests = computed(() => pullRequests.value.length > 0)

  async function loadRepositories(page = 1): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const result = await fetchRepositories(page)
      repositories.value = (result.items || []).map(adaptRepositoryData)
      totalCount.value = result.totalCount || 0
      currentPage.value = result.page || page
      if (!repoGlobal.selectedRepositoryId && repositories.value.length > 0 && repositories.value[0]?.id) {
        repoGlobal.selectRepository(repositories.value[0].id)
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载仓库失败'
    } finally {
      loading.value = false
    }
  }

  async function loadBranches(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const repoId = repoGlobal.selectedRepositoryId ?? repositories.value[0]?.id ?? ''
      const rawBranches = await fetchBranches(repoId)
      branches.value = (rawBranches || []).map(adaptBranchData)
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载分支失败'
    } finally {
      loading.value = false
    }
  }

  async function loadPullRequests(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const repoId = repoGlobal.selectedRepositoryId ?? repositories.value[0]?.id ?? ''
      const rawPRs = await fetchPullRequests(repoId)
      pullRequests.value = (rawPRs || []).map(adaptPullRequestData)
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载 PR 失败'
    } finally {
      loading.value = false
    }
  }

  async function submitCreate(payload: CreateRepositoryRequest): Promise<RepositoryDto> {
    const created = await createRepository(payload)
    repositories.value.unshift(created)
    return created
  }

  async function submitUpdate(id: string, payload: UpdateRepositoryRequest): Promise<void> {
    const updated = await updateRepository(id, payload)
    const idx = repositories.value.findIndex(r => r.id === id)
    if (idx !== -1) repositories.value[idx] = updated
  }

  async function submitDelete(id: string): Promise<void> {
    await deleteRepository(id)
    repositories.value = repositories.value.filter(r => r.id !== id)
    if (repoGlobal.selectedRepositoryId === id) {
      repoGlobal.selectRepository(repositories.value[0]?.id ?? '')
    }
  }

  return {
    repositories,
    branches,
    pullRequests,
    loading,
    error,
    totalCount,
    currentPage,
    hasRepositories,
    hasBranches,
    hasPullRequests,
    repoGlobal,
    loadRepositories,
    loadBranches,
    loadPullRequests,
    submitCreate,
    submitUpdate,
    submitDelete,
  }
})