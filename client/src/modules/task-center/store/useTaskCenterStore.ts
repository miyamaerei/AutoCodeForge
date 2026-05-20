import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import {
  createTask,
  deleteTask,
  fetchTaskDetail,
  fetchTaskLogs,
  fetchTaskSummaries,
  pauseTask,
  resumeTask,
  updateTask,
} from '../task-center.api'
import type {
  CreateTaskRequestDto,
  TaskDetailDto,
  TaskLogDto,
  TaskSummaryDto,
  UpdateTaskRequestDto,
} from '../task-center.types'
import {
  getTaskChat,
  getTaskDiff,
  sendTaskChatMessage,
  type ChatMessageDto,
  type TaskDiffDto,
} from '../../../mock'

export const useTaskCenterStore = defineStore('module.task-center', () => {
  const tasks = ref<TaskSummaryDto[]>([])
  const selectedTask = ref<TaskDetailDto | null>(null)
  const logs = ref<TaskLogDto[]>([])
  const chat = ref<ChatMessageDto[]>([])
  const diff = ref<TaskDiffDto | null>(null)

  const loading = ref(false)
  const detailLoading = ref(false)
  const creating = ref(false)
  const chatSending = ref(false)
  const error = ref<string | null>(null)

  const hasTasks = computed(() => tasks.value.length > 0)
  const hasDetail = computed(() => selectedTask.value !== null)

  async function loadTasks(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      tasks.value = await fetchTaskSummaries()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载任务列表失败'
    } finally {
      loading.value = false
    }
  }

  async function loadTaskDetail(taskId: string): Promise<void> {
    detailLoading.value = true
    error.value = null
    try {
      const [detail, nextLogs, nextChat, nextDiff] = await Promise.all([
        fetchTaskDetail(taskId),
        fetchTaskLogs(taskId),
        getTaskChat(taskId),
        getTaskDiff(taskId),
      ])
      selectedTask.value = detail
      logs.value = nextLogs
      chat.value = nextChat
      diff.value = nextDiff
    } catch (err) {
      selectedTask.value = null
      logs.value = []
      chat.value = []
      diff.value = null
      error.value = err instanceof Error ? err.message : '加载任务详情失败'
    } finally {
      detailLoading.value = false
    }
  }

  async function submitTask(payload: CreateTaskRequestDto): Promise<TaskSummaryDto | null> {
    creating.value = true
    error.value = null
    try {
      const task = await createTask(payload)
      tasks.value.unshift(task)
      return task
    } catch (err) {
      error.value = err instanceof Error ? err.message : '创建任务失败'
      return null
    } finally {
      creating.value = false
    }
  }

  async function sendTaskChat(taskId: string, content: string): Promise<void> {
    const trimmed = content.trim()
    if (!trimmed) {
      return
    }
    chatSending.value = true
    error.value = null
    try {
      const nextChat = await sendTaskChatMessage({ taskId, content: trimmed })
      chat.value = nextChat
    } catch (err) {
      error.value = err instanceof Error ? err.message : '发送聊天消息失败'
    } finally {
      chatSending.value = false
    }
  }

  async function submitUpdate(taskId: string, payload: UpdateTaskRequestDto): Promise<void> {
    error.value = null
    try {
      const updated = await updateTask(taskId, payload)
      selectedTask.value = updated
      const index = tasks.value.findIndex((task) => task.id === taskId)
      if (index !== -1) {
        const existing = tasks.value[index]
        if (!existing) return
        tasks.value[index] = {
          ...existing,
          title: updated.title,
          state: updated.state,
        }
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新任务失败'
      throw err
    }
  }

  async function submitPause(taskId: string): Promise<void> {
    error.value = null
    try {
      const paused = await pauseTask(taskId)
      selectedTask.value = paused
      const index = tasks.value.findIndex((task) => task.id === taskId)
      if (index !== -1) {
        const existing = tasks.value[index]
        if (!existing) return
        tasks.value[index] = { ...existing, state: paused.state }
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '暂停任务失败'
      throw err
    }
  }

  async function submitResume(taskId: string): Promise<void> {
    error.value = null
    try {
      const resumed = await resumeTask(taskId)
      selectedTask.value = resumed
      const index = tasks.value.findIndex((task) => task.id === taskId)
      if (index !== -1) {
        const existing = tasks.value[index]
        if (!existing) return
        tasks.value[index] = { ...existing, state: resumed.state }
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '恢复任务失败'
      throw err
    }
  }

  async function submitDelete(taskId: string): Promise<void> {
    error.value = null
    try {
      await deleteTask(taskId)
      tasks.value = tasks.value.filter((task) => task.id !== taskId)
      if (selectedTask.value?.id === taskId) {
        selectedTask.value = null
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除任务失败'
      throw err
    }
  }

  return {
    tasks,
    selectedTask,
    logs,
    chat,
    diff,
    loading,
    detailLoading,
    creating,
    chatSending,
    error,
    hasTasks,
    hasDetail,
    loadTasks,
    loadTaskDetail,
    submitTask,
    submitUpdate,
    submitPause,
    submitResume,
    submitDelete,
    sendTaskChat,
  }
})
