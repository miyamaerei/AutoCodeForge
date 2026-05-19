import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import {
  getDashboardStats,
  getModuleEntries,
  getRecentTasks,
  type DashboardStatsDto,
  type ModuleEntryDto,
  type RecentTaskDto,
} from '../../../mock/task'

export const useConsoleStore = defineStore('module.console', () => {
  const stats = ref<DashboardStatsDto | null>(null)
  const recentTasks = ref<RecentTaskDto[]>([])
  const moduleEntries = ref<ModuleEntryDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const hasData = computed(() => moduleEntries.value.length > 0)

  async function fetchConsoleData(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const [nextStats, nextTasks, nextEntries] = await Promise.all([
        getDashboardStats(),
        getRecentTasks(),
        getModuleEntries(),
      ])
      stats.value = nextStats
      recentTasks.value = nextTasks
      moduleEntries.value = nextEntries
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载入口数据失败'
    } finally {
      loading.value = false
    }
  }

  return {
    stats,
    recentTasks,
    moduleEntries,
    loading,
    error,
    hasData,
    fetchConsoleData,
  }
})
