import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import {
  fetchBranches,
  fetchPullRequests,
  fetchRepositories,
  type BranchDto,
  type PullRequestDto,
  type RepositoryDto,
} from '../repo-management.api'

export const useRepoManagementStore = defineStore('module.repo-management', () => {
  const repositories = ref<RepositoryDto[]>([])
  const branches = ref<BranchDto[]>([])
  const pullRequests = ref<PullRequestDto[]>([])

  const loading = ref(false)
  const error = ref<string | null>(null)

  const hasRepositories = computed(() => repositories.value.length > 0)
  const hasBranches = computed(() => branches.value.length > 0)
  const hasPullRequests = computed(() => pullRequests.value.length > 0)

  async function loadRepositories(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      repositories.value = await fetchRepositories()
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
      branches.value = await fetchBranches()
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
      pullRequests.value = await fetchPullRequests()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载 PR 失败'
    } finally {
      loading.value = false
    }
  }

  return {
    repositories,
    branches,
    pullRequests,
    loading,
    error,
    hasRepositories,
    hasBranches,
    hasPullRequests,
    loadRepositories,
    loadBranches,
    loadPullRequests,
  }
})
