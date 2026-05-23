import { ref } from 'vue'
import { defineStore } from 'pinia'
import { assignTask, getOrchestrationSettings, reassignTask } from '@/api/task-orchestration'
import type { AgentRole, OrchestrationAssignResponse, OrchestrationSettings } from '@/types/task-orchestration'

export const useTaskOrchestrationStore = defineStore('task-orchestration', () => {
  const settings = ref<OrchestrationSettings | null>(null)
  const lastAssignment = ref<OrchestrationAssignResponse | null>(null)
  const assignmentHistory = ref<OrchestrationAssignResponse[]>([])
  const loading = ref(false)
  const assigning = ref(false)
  const error = ref<string | null>(null)

  async function loadSettings(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      settings.value = await getOrchestrationSettings()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载编排设置失败'
    } finally {
      loading.value = false
    }
  }

  async function submitAssignTask(taskId: string, role: AgentRole): Promise<OrchestrationAssignResponse | null> {
    assigning.value = true
    error.value = null
    try {
      const result = await assignTask(taskId, role)
      lastAssignment.value = result
      assignmentHistory.value.unshift(result)
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '任务分配失败'
      return null
    } finally {
      assigning.value = false
    }
  }

  async function submitReassignTask(taskId: string, currentAgentId: string): Promise<OrchestrationAssignResponse | null> {
    assigning.value = true
    error.value = null
    try {
      const result = await reassignTask(taskId, currentAgentId)
      lastAssignment.value = result
      assignmentHistory.value.unshift(result)
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '任务重新分配失败'
      return null
    } finally {
      assigning.value = false
    }
  }

  function clearError(): void {
    error.value = null
  }

  function clearHistory(): void {
    assignmentHistory.value = []
  }

  return {
    settings,
    lastAssignment,
    assignmentHistory,
    loading,
    assigning,
    error,
    loadSettings,
    submitAssignTask,
    submitReassignTask,
    clearError,
    clearHistory,
  }
})