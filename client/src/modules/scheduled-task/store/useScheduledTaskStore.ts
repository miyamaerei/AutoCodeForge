import { ref } from 'vue'
import { defineStore } from 'pinia'
import {
  fetchScheduledTasks,
  fetchScheduledTask,
  createScheduledTask,
  updateScheduledTask,
  deleteScheduledTask,
  pauseScheduledTask,
  resumeScheduledTask,
  fetchExecutions,
  triggerTask,
  getTaskTemplates,
  getTaskTemplate,
} from '../api/scheduled-task.api'
import type {
  ScheduledTaskDto,
  CreateScheduledTaskDto,
  UpdateScheduledTaskDto,
  ScheduledTaskExecutionDto,
  TaskTemplateDto,
} from '../api/scheduled-task.types'

export const useScheduledTaskStore = defineStore('module.scheduled-task', () => {
  const tasks = ref<ScheduledTaskDto[]>([])
  const currentTask = ref<ScheduledTaskDto | null>(null)
  const executions = ref<ScheduledTaskExecutionDto[]>([])
  const templates = ref<TaskTemplateDto[]>([])
  const loading = ref(false)
  const saving = ref(false)
  const triggering = ref(false)
  const error = ref<string | null>(null)
  const totalCount = ref(0)
  const currentPage = ref(1)

  async function loadTasks(page = 1, pageSize = 20): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const result = await fetchScheduledTasks(page, pageSize)
      tasks.value = result.items
      totalCount.value = result.totalCount
      currentPage.value = page
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载定时任务失败'
    } finally {
      loading.value = false
    }
  }

  async function loadTask(id: string): Promise<void> {
    loading.value = true
    error.value = null
    try {
      currentTask.value = await fetchScheduledTask(id)
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载任务详情失败'
    } finally {
      loading.value = false
    }
  }

  async function loadTemplates(): Promise<void> {
    try {
      templates.value = await getTaskTemplates()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载任务模板失败'
    }
  }

  async function loadExecutions(taskId: string, page = 1, pageSize = 20): Promise<void> {
    try {
      const result = await fetchExecutions(taskId, page, pageSize)
      executions.value = result.items
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载执行记录失败'
    }
  }

  async function submitCreate(payload: CreateScheduledTaskDto): Promise<ScheduledTaskDto> {
    saving.value = true
    error.value = null
    try {
      const created = await createScheduledTask(payload)
      tasks.value.unshift(created)
      totalCount.value++
      return created
    } catch (err) {
      error.value = err instanceof Error ? err.message : '创建任务失败'
      throw err
    } finally {
      saving.value = false
    }
  }

  async function submitUpdate(id: string, payload: UpdateScheduledTaskDto): Promise<void> {
    saving.value = true
    error.value = null
    try {
      const updated = await updateScheduledTask(id, payload)
      if (updated) {
        const idx = tasks.value.findIndex((t) => t.id === id)
        if (idx !== -1) tasks.value[idx] = updated
        if (currentTask.value?.id === id) currentTask.value = updated
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新任务失败'
      throw err
    } finally {
      saving.value = false
    }
  }

  async function submitDelete(id: string): Promise<void> {
    error.value = null
    try {
      const success = await deleteScheduledTask(id)
      if (success) {
        tasks.value = tasks.value.filter((t) => t.id !== id)
        totalCount.value--
        if (currentTask.value?.id === id) currentTask.value = null
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除任务失败'
      throw err
    }
  }

  async function submitPause(id: string): Promise<void> {
    try {
      const updated = await pauseScheduledTask(id)
      if (updated) {
        const idx = tasks.value.findIndex((t) => t.id === id)
        if (idx !== -1) tasks.value[idx] = updated
        if (currentTask.value?.id === id) currentTask.value = updated
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '暂停任务失败'
      throw err
    }
  }

  async function submitResume(id: string): Promise<void> {
    try {
      const updated = await resumeScheduledTask(id)
      if (updated) {
        const idx = tasks.value.findIndex((t) => t.id === id)
        if (idx !== -1) tasks.value[idx] = updated
        if (currentTask.value?.id === id) currentTask.value = updated
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '恢复任务失败'
      throw err
    }
  }

  async function submitTrigger(id: string): Promise<ScheduledTaskExecutionDto> {
    triggering.value = true
    error.value = null
    try {
      const record = await triggerTask(id)
      executions.value.unshift(record)
      return record
    } catch (err) {
      error.value = err instanceof Error ? err.message : '触发任务失败'
      throw err
    } finally {
      triggering.value = false
    }
  }

  function getTemplate(id: string): TaskTemplateDto | null {
    return templates.value.find((t) => t.id === id) ?? null
  }

  function clearError(): void {
    error.value = null
  }

  return {
    tasks,
    currentTask,
    executions,
    templates,
    loading,
    saving,
    triggering,
    error,
    totalCount,
    currentPage,
    loadTasks,
    loadTask,
    loadTemplates,
    loadExecutions,
    submitCreate,
    submitUpdate,
    submitDelete,
    submitPause,
    submitResume,
    submitTrigger,
    getTemplate,
    clearError,
  }
})