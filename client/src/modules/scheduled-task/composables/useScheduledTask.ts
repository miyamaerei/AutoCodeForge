import { computed, ref } from 'vue'
import { useScheduledTaskStore } from '../store/useScheduledTaskStore'
import type { TaskTemplateDto, TriggerType } from '../api/scheduled-task.types'

export interface ScheduledTaskFormData {
  name: string
  description: string
  templateId: string
  triggerType: TriggerType
  cronExpression: string
  intervalHours: number
  intervalMinutes: number
  onceTime: string
  agentId: string
  repoId: string
  branch: string
  path: string
  params: string
  enabled: boolean
}

const defaultFormData: ScheduledTaskFormData = {
  name: '',
  description: '',
  templateId: '',
  triggerType: 'cron',
  cronExpression: '0 9 * * *',
  intervalHours: 1,
  intervalMinutes: 0,
  onceTime: '',
  agentId: '',
  repoId: '',
  branch: 'main',
  path: '',
  params: '{}',
  enabled: true,
}

const availableAgents = [
  { id: 'agent-1', name: '代码审查助手' },
  { id: 'agent-2', name: '架构设计专家' },
  { id: 'agent-3', name: '数据库专家' },
  { id: 'agent-4', name: '前端开发助手' },
  { id: 'agent-5', name: '文档撰写助手' },
]

const availableRepos = [
  { id: 'repo-1', name: 'AutoCodeForge', url: 'https://github.com/org/AutoCodeForge' },
  { id: 'repo-2', name: 'backend-service', url: 'https://github.com/org/backend-service' },
]

export function useScheduledTask() {
  const store = useScheduledTaskStore()

  const dialogVisible = ref(false)
  const dialogTitle = ref('新建定时任务')
  const formData = ref<ScheduledTaskFormData>({ ...defaultFormData })
  const selectedTaskId = ref<string | null>(null)

  const selectedTask = computed(() => {
    if (!selectedTaskId.value) return null
    return store.tasks.find((t) => t.id === selectedTaskId.value) || null
  })

  const hasTasks = computed(() => store.tasks.length > 0)
  const enabledTasks = computed(() => store.tasks.filter((t) => t.status !== 'disabled'))

  const canSave = computed(() => {
    const fd = formData.value
    return fd.name.trim() && fd.agentId && fd.repoId
  })

  async function fetchTasks(): Promise<void> {
    await store.loadTasks()
  }

  async function fetchTask(id: string): Promise<void> {
    await store.loadTask(id)
  }

  async function fetchTemplates(): Promise<void> {
    await store.loadTemplates()
  }

  async function fetchExecutions(taskId: string): Promise<void> {
    await store.loadExecutions(taskId)
  }

  function applyTemplate(template: TaskTemplateDto): void {
    formData.value.templateId = template.id
    formData.value.agentId = template.agentId
    formData.value.triggerType = template.triggerType
    if (template.cronExpression) {
      formData.value.cronExpression = template.cronExpression
    }
    if (template.intervalMs) {
      formData.value.intervalHours = Math.floor(template.intervalMs / (1000 * 60 * 60))
      formData.value.intervalMinutes = Math.floor((template.intervalMs % (1000 * 60 * 60)) / (1000 * 60))
    }
    if (template.defaultParams) {
      formData.value.params = template.defaultParams
    }
  }

  function openCreateDialog(): void {
    dialogTitle.value = '新建定时任务'
    formData.value = { ...defaultFormData }
    selectedTaskId.value = null
    dialogVisible.value = true
  }

  function openEditDialog(task: typeof store.tasks[0]): void {
    dialogTitle.value = '编辑定时任务'
    selectedTaskId.value = task.id
    const intervalMs = 0
    formData.value = {
      name: task.name,
      description: '',
      templateId: '',
      triggerType: task.triggerType,
      cronExpression: task.cronExpression,
      intervalHours: Math.floor(intervalMs / (1000 * 60 * 60)),
      intervalMinutes: Math.floor((intervalMs % (1000 * 60 * 60)) / (1000 * 60)),
      onceTime: '',
      agentId: task.agentId || '',
      repoId: '',
      branch: 'main',
      path: '',
      params: task.input,
      enabled: task.status !== 'disabled',
    }
    dialogVisible.value = true
  }

  function closeDialog(): void {
    dialogVisible.value = false
    selectedTaskId.value = null
    formData.value = { ...defaultFormData }
  }

  async function saveTask(): Promise<boolean> {
    if (!canSave.value) return false

    try {
      const fd = formData.value

      if (selectedTaskId.value) {
        await store.submitUpdate(selectedTaskId.value, {
          name: fd.name,
          cronExpression: fd.cronExpression,
          agentId: fd.agentId || undefined,
          input: fd.params,
          taskTitle: fd.name,
          status: fd.enabled ? 'pending' : 'disabled',
        })
      } else {
        await store.submitCreate({
          name: fd.name,
          cronExpression: fd.cronExpression,
          triggerType: fd.triggerType,
          agentId: fd.agentId || undefined,
          input: fd.params,
          taskTitle: fd.name,
        })
      }
      closeDialog()
      return true
    } catch {
      return false
    }
  }

  async function removeTask(id: string): Promise<boolean> {
    try {
      await store.submitDelete(id)
      if (selectedTaskId.value === id) {
        selectedTaskId.value = null
      }
      return true
    } catch {
      return false
    }
  }

  async function toggleEnabled(task: typeof store.tasks[0]): Promise<boolean> {
    try {
      if (task.status === 'disabled') {
        await store.submitResume(task.id)
      } else {
        await store.submitPause(task.id)
      }
      return true
    } catch {
      return false
    }
  }

  async function runTask(task: typeof store.tasks[0]): Promise<boolean> {
    try {
      await store.submitTrigger(task.id)
      await fetchExecutions(task.id)
      return true
    } catch {
      return false
    }
  }

  async function selectTask(task: typeof store.tasks[0]): Promise<void> {
    selectedTaskId.value = task.id
    await fetchExecutions(task.id)
  }

  function clearSelection(): void {
    selectedTaskId.value = null
    store.executions = []
  }

  return {
    tasks: computed(() => store.tasks),
    selectedTask,
    executions: computed(() => store.executions),
    templates: computed(() => store.templates),
    loading: computed(() => store.loading),
    saving: computed(() => store.saving),
    triggering: computed(() => store.triggering),
    error: computed(() => store.error),
    hasTasks,
    enabledTasks,
    canSave,
    dialogVisible,
    dialogTitle,
    formData,
    availableAgents,
    availableRepos,
    fetchTasks,
    fetchTask,
    fetchTemplates,
    fetchExecutions,
    applyTemplate,
    openCreateDialog,
    openEditDialog,
    closeDialog,
    saveTask,
    removeTask,
    toggleEnabled,
    runTask,
    selectTask,
    clearSelection,
  }
}