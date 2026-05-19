/**
 * 定时任务管理 Composable
 * 提供定时任务的增删改查、触发执行和执行记录查询功能
 */
import { computed, ref } from 'vue'
import {
  getScheduledTasks,
  getScheduledTask,
  createScheduledTask,
  updateScheduledTask,
  deleteScheduledTask,
  getExecutionRecords,
  triggerTask,
  toggleTaskEnabled,
  getTaskTemplates,
  type ScheduledTaskDto,
  type CreateScheduledTaskDto,
  type UpdateScheduledTaskDto,
  type ExecutionRecordDto,
  type TaskTemplateDto,
  type TaskRepoRef,
  type TriggerType,
} from '../scheduled-task.api'

export interface ScheduledTaskFormData {
  /** 任务名称 */
  name: string
  /** 任务描述 */
  description: string
  /** 关联的模板 ID */
  templateId: string
  /** 触发类型 */
  triggerType: TriggerType
  /** Cron 表达式 */
  cronExpression: string
  /** 间隔（小时） */
  intervalHours: number
  /** 间隔（分钟） */
  intervalMinutes: number
  /** 一次性执行时间 */
  onceTime: string
  /** 关联的 Agent ID */
  agentId: string
  /** 关联的仓库 ID */
  repoId: string
  /** 目标分支 */
  branch: string
  /** 目标路径 */
  path: string
  /** 任务参数 */
  params: string
  /** 是否启用 */
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

// 可选的 Agent 列表（用于下拉选择）
const availableAgents = [
  { id: 'agent-1', name: '代码审查助手' },
  { id: 'agent-2', name: '架构设计专家' },
  { id: 'agent-3', name: '数据库专家' },
  { id: 'agent-4', name: '前端开发助手' },
  { id: 'agent-5', name: '文档撰写助手' },
]

// 可选的仓库列表（用于下拉选择）
const availableRepos = [
  { id: 'repo-1', name: 'AutoCodeForge', url: 'https://github.com/org/AutoCodeForge' },
  { id: 'repo-2', name: 'backend-service', url: 'https://github.com/org/backend-service' },
]

export function useScheduledTask() {
  // 状态
  const tasks = ref<ScheduledTaskDto[]>([])
  const selectedTask = ref<ScheduledTaskDto | null>(null)
  const executions = ref<ExecutionRecordDto[]>([])
  const templates = ref<TaskTemplateDto[]>([])
  const loading = ref(false)
  const saving = ref(false)
  const triggering = ref(false)
  const error = ref<string | null>(null)

  // 弹窗状态
  const dialogVisible = ref(false)
  const dialogTitle = ref('新建定时任务')
  const formData = ref<ScheduledTaskFormData>({ ...defaultFormData })

  // 计算属性
  const hasTasks = computed(() => tasks.value.length > 0)
  const enabledTasks = computed(() => tasks.value.filter((t) => t.enabled))
  const canSave = computed(() => {
    const fd = formData.value
    return fd.name.trim() && fd.agentId && fd.repoId
  })

  /** 加载所有定时任务 */
  async function fetchTasks(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      tasks.value = await getScheduledTasks()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载定时任务列表失败'
    } finally {
      loading.value = false
    }
  }

  /** 加载单个任务 */
  async function fetchTask(id: string): Promise<ScheduledTaskDto | null> {
    try {
      const task = await getScheduledTask(id)
      if (task) {
        selectedTask.value = task
      }
      return task
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载任务详情失败'
      return null
    }
  }

  /** 加载任务模板 */
  async function fetchTemplates(): Promise<void> {
    try {
      templates.value = await getTaskTemplates()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载任务模板失败'
    }
  }

  /** 加载任务的执行记录 */
  async function fetchExecutions(taskId: string): Promise<void> {
    try {
      executions.value = await getExecutionRecords(taskId)
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载执行记录失败'
    }
  }

  /** 根据模板填充表单 */
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

  /** 打开新建弹窗 */
  function openCreateDialog(): void {
    dialogTitle.value = '新建定时任务'
    formData.value = { ...defaultFormData }
    selectedTask.value = null
    dialogVisible.value = true
  }

  /** 打开编辑弹窗 */
  function openEditDialog(task: ScheduledTaskDto): void {
    dialogTitle.value = '编辑定时任务'
    selectedTask.value = task
    const intervalMs = task.intervalMs
    formData.value = {
      name: task.name,
      description: task.description,
      templateId: task.templateId || '',
      triggerType: task.triggerType,
      cronExpression: task.cronExpression,
      intervalHours: Math.floor(intervalMs / (1000 * 60 * 60)),
      intervalMinutes: Math.floor((intervalMs % (1000 * 60 * 60)) / (1000 * 60)),
      onceTime: task.onceTime,
      agentId: task.agentId,
      repoId: task.repo?.repoId || '',
      branch: task.repo?.branch || 'main',
      path: task.repo?.path || '',
      params: task.params,
      enabled: task.enabled,
    }
    dialogVisible.value = true
  }

  /** 关闭弹窗 */
  function closeDialog(): void {
    dialogVisible.value = false
    selectedTask.value = null
    formData.value = { ...defaultFormData }
  }

  /** 保存任务 */
  async function saveTask(): Promise<boolean> {
    if (!canSave.value) return false

    saving.value = true
    error.value = null
    try {
      const fd = formData.value
      const intervalMs =
        fd.triggerType === 'interval'
          ? fd.intervalHours * 60 * 60 * 1000 + fd.intervalMinutes * 60 * 1000
          : 0

      // 构建仓库信息
      const repo: TaskRepoRef | undefined = fd.repoId
        ? {
            repoId: fd.repoId,
            repoName: availableRepos.find((r) => r.id === fd.repoId)?.name || fd.repoId,
            repoUrl: availableRepos.find((r) => r.id === fd.repoId)?.url || '',
            branch: fd.branch,
            path: fd.path || undefined,
          }
        : undefined

      if (selectedTask.value) {
        // 更新
        const dto: UpdateScheduledTaskDto = {
          id: selectedTask.value.id,
          name: fd.name,
          description: fd.description,
          templateId: fd.templateId || undefined,
          triggerType: fd.triggerType,
          cronExpression: fd.cronExpression,
          intervalMs,
          onceTime: fd.onceTime,
          agentId: fd.agentId,
          repo,
          params: fd.params,
          enabled: fd.enabled,
        }
        const updated = await updateScheduledTask(dto)
        if (updated) {
          const index = tasks.value.findIndex((t) => t.id === updated.id)
          if (index !== -1) {
            tasks.value[index] = updated
          }
        }
      } else {
        // 新建
        const dto: CreateScheduledTaskDto = {
          name: fd.name,
          description: fd.description,
          templateId: fd.templateId || undefined,
          triggerType: fd.triggerType,
          cronExpression: fd.cronExpression,
          intervalMs,
          onceTime: fd.onceTime,
          agentId: fd.agentId,
          repo,
          params: fd.params,
          enabled: fd.enabled,
        }
        const created = await createScheduledTask(dto)
        tasks.value.push(created)
      }
      closeDialog()
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : '保存任务失败'
      return false
    } finally {
      saving.value = false
    }
  }

  /** 删除任务 */
  async function removeTask(id: string): Promise<boolean> {
    error.value = null
    try {
      const success = await deleteScheduledTask(id)
      if (success) {
        tasks.value = tasks.value.filter((t) => t.id !== id)
        if (selectedTask.value?.id === id) {
          selectedTask.value = null
        }
      }
      return success
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除任务失败'
      return false
    }
  }

  /** 切换启用状态 */
  async function toggleEnabled(task: ScheduledTaskDto): Promise<boolean> {
    try {
      const updated = await toggleTaskEnabled(task.id)
      if (updated) {
        const index = tasks.value.findIndex((t) => t.id === updated.id)
        if (index !== -1) {
          tasks.value[index] = updated
        }
      }
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : '切换状态失败'
      return false
    }
  }

  /** 手动触发任务 */
  async function runTask(task: ScheduledTaskDto): Promise<boolean> {
    triggering.value = true
    error.value = null
    try {
      await triggerTask(task.id)
      await fetchExecutions(task.id)
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : '触发任务失败'
      return false
    } finally {
      triggering.value = false
    }
  }

  /** 选择任务并加载执行记录 */
  async function selectTask(task: ScheduledTaskDto): Promise<void> {
    selectedTask.value = task
    await fetchExecutions(task.id)
  }

  /** 清除选择 */
  function clearSelection(): void {
    selectedTask.value = null
    executions.value = []
  }

  return {
    // 状态
    tasks,
    selectedTask,
    executions,
    templates,
    loading,
    saving,
    triggering,
    error,
    // 计算属性
    hasTasks,
    enabledTasks,
    canSave,
    // 弹窗状态
    dialogVisible,
    dialogTitle,
    formData,
    // 常量
    availableAgents,
    availableRepos,
    // 方法
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
