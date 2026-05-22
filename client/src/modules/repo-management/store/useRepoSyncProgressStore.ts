import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import {
  cancelRepoSyncTask,
  createRepoSyncTask,
  fetchRepoSyncTaskDetail,
  type CreateRepoSyncTaskRequest,
  type RepoSyncTaskDetailDto,
} from '../api/repo-management.api'

function normalizeStatus(status: string | undefined): string {
  return (status || '').trim().toUpperCase()
}

function toProgress(status: string | undefined): number {
  const normalized = normalizeStatus(status)
  if (normalized === 'PENDING') {
    return 15
  }
  if (normalized === 'RUNNING') {
    return 65
  }
  if (normalized === 'COMPLETED') {
    return 100
  }
  if (normalized === 'FAILED' || normalized === 'CANCELED') {
    return 100
  }
  return 0
}

export const useRepoSyncProgressStore = defineStore('module.repo-sync-progress', () => {
  const currentTaskId = ref('')
  const taskDetail = ref<RepoSyncTaskDetailDto | null>(null)
  const loading = ref(false)
  const submitting = ref(false)
  const canceling = ref(false)
  const error = ref<string | null>(null)
  const pollIntervalMs = 3000
  let timerId: number | null = null

  const hasTaskDetail = computed(() => taskDetail.value !== null)
  const progressPercent = computed(() => toProgress(taskDetail.value?.status))
  const normalizedTaskStatus = computed(() => normalizeStatus(taskDetail.value?.status))
  const normalizedWorkspaceStatus = computed(() => normalizeStatus(taskDetail.value?.workspaceStatus))
  const isInFlight = computed(() => {
    const status = normalizedTaskStatus.value
    return status === 'PENDING' || status === 'RUNNING'
  })
  const canCancel = computed(() => isInFlight.value)

  function stopAutoRefresh(): void {
    if (timerId !== null) {
      window.clearInterval(timerId)
      timerId = null
    }
  }

  function startAutoRefresh(): void {
    if (timerId !== null || !currentTaskId.value) {
      return
    }
    timerId = window.setInterval(async () => {
      if (!currentTaskId.value) {
        stopAutoRefresh()
        return
      }
      try {
        await refreshTaskDetail(currentTaskId.value)
      } catch {
        stopAutoRefresh()
      }
    }, pollIntervalMs)
  }

  function ensurePollingState(): void {
    if (isInFlight.value) {
      startAutoRefresh()
      return
    }
    stopAutoRefresh()
  }

  async function createTask(payload: CreateRepoSyncTaskRequest): Promise<void> {
    submitting.value = true
    error.value = null
    try {
      const created = await createRepoSyncTask(payload)
      currentTaskId.value = created.id
      await refreshTaskDetail(created.id)
    } catch (err) {
      error.value = err instanceof Error ? err.message : '创建同步任务失败'
      throw err
    } finally {
      submitting.value = false
    }
  }

  async function refreshTaskDetail(taskId?: string): Promise<void> {
    const id = (taskId || currentTaskId.value || '').trim()
    if (!id) {
      throw new Error('请先输入任务 ID')
    }

    loading.value = true
    error.value = null
    try {
      const detail = await fetchRepoSyncTaskDetail(id)
      currentTaskId.value = id
      taskDetail.value = detail
      ensurePollingState()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '查询任务进度失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function cancelTask(): Promise<void> {
    if (!currentTaskId.value) {
      throw new Error('当前没有可取消的任务')
    }

    canceling.value = true
    error.value = null
    try {
      taskDetail.value = await cancelRepoSyncTask(currentTaskId.value)
      ensurePollingState()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '取消任务失败'
      throw err
    } finally {
      canceling.value = false
    }
  }

  return {
    currentTaskId,
    taskDetail,
    loading,
    submitting,
    canceling,
    error,
    hasTaskDetail,
    progressPercent,
    normalizedTaskStatus,
    normalizedWorkspaceStatus,
    isInFlight,
    canCancel,
    createTask,
    refreshTaskDetail,
    cancelTask,
    startAutoRefresh,
    stopAutoRefresh,
  }
})
