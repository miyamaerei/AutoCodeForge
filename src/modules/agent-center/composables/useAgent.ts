/**
 * Agent 管理 Composable
 * 提供 Agent 的增删改查和自动选择功能
 */
import { computed, ref } from 'vue'
import {
  getAgents,
  getAgent,
  createAgent,
  updateAgent,
  deleteAgent,
  selectAgentByMessage,
  type AgentDto,
  type CreateAgentDto,
  type UpdateAgentDto,
} from '../agent.api'

export interface AgentFormData {
  /** Agent 名称 */
  name: string
  /** Agent 描述 */
  description: string
  /** 图标 */
  icon: string
  /** 系统提示词 */
  systemPrompt: string
  /** 关键词列表（逗号分隔） */
  keywordsText: string
  /** 是否启用 */
  enabled: boolean
}

const defaultFormData: AgentFormData = {
  name: '',
  description: '',
  icon: '🤖',
  systemPrompt: '',
  keywordsText: '',
  enabled: true,
}

export function useAgent() {
  // 状态
  const agents = ref<AgentDto[]>([])
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)
  const selectedAgent = ref<AgentDto | null>(null)
  const autoSelectedAgent = ref<AgentDto | null>(null)

  // 弹窗状态
  const dialogVisible = ref(false)
  const dialogTitle = ref('新建 Agent')
  const formData = ref<AgentFormData>({ ...defaultFormData })

  // 计算属性
  const hasAgents = computed(() => agents.value.length > 0)
  const enabledAgents = computed(() => agents.value.filter((a) => a.enabled))
  const canSave = computed(() => {
    const fd = formData.value
    return fd.name.trim() && fd.systemPrompt.trim()
  })

  // 方法
  /** 加载所有 Agent */
  async function fetchAgents(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      agents.value = await getAgents()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载 Agent 列表失败'
    } finally {
      loading.value = false
    }
  }

  /** 加载单个 Agent */
  async function fetchAgent(id: string): Promise<AgentDto | null> {
    try {
      const agent = await getAgent(id)
      if (agent) {
        selectedAgent.value = agent
      }
      return agent
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载 Agent 详情失败'
      return null
    }
  }

  /** 打开新建弹窗 */
  function openCreateDialog(): void {
    dialogTitle.value = '新建 Agent'
    formData.value = { ...defaultFormData }
    selectedAgent.value = null
    dialogVisible.value = true
  }

  /** 打开编辑弹窗 */
  function openEditDialog(agent: AgentDto): void {
    dialogTitle.value = '编辑 Agent'
    selectedAgent.value = agent
    formData.value = {
      name: agent.name,
      description: agent.description,
      icon: agent.icon,
      systemPrompt: agent.systemPrompt,
      keywordsText: agent.keywords.map((k) => k.keyword).join(', '),
      enabled: agent.enabled,
    }
    dialogVisible.value = true
  }

  /** 关闭弹窗 */
  function closeDialog(): void {
    dialogVisible.value = false
    selectedAgent.value = null
    formData.value = { ...defaultFormData }
  }

  /** 解析关键词文本为关键词数组 */
  function parseKeywords(text: string): { keyword: string; weight: number }[] {
    return text
      .split(/[,，;；]/)
      .map((k) => k.trim())
      .filter((k) => k)
      .map((keyword) => ({ keyword, weight: 1.0 }))
  }

  /** 保存 Agent（新建或更新） */
  async function saveAgent(): Promise<boolean> {
    if (!canSave.value) return false

    saving.value = true
    error.value = null
    try {
      const fd = formData.value
      const keywords = parseKeywords(fd.keywordsText)

      if (selectedAgent.value) {
        // 更新
        const dto: UpdateAgentDto = {
          id: selectedAgent.value.id,
          name: fd.name,
          description: fd.description,
          icon: fd.icon,
          systemPrompt: fd.systemPrompt,
          keywords,
          enabled: fd.enabled,
        }
        const updated = await updateAgent(dto)
        if (updated) {
          const index = agents.value.findIndex((a) => a.id === updated.id)
          if (index !== -1) {
            agents.value[index] = updated
          }
        }
      } else {
        // 新建
        const dto: CreateAgentDto = {
          name: fd.name,
          description: fd.description,
          icon: fd.icon,
          systemPrompt: fd.systemPrompt,
          keywords,
          enabled: fd.enabled,
        }
        const created = await createAgent(dto)
        agents.value.push(created)
      }
      closeDialog()
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : '保存 Agent 失败'
      return false
    } finally {
      saving.value = false
    }
  }

  /** 删除 Agent */
  async function removeAgent(id: string): Promise<boolean> {
    error.value = null
    try {
      const success = await deleteAgent(id)
      if (success) {
        agents.value = agents.value.filter((a) => a.id !== id)
        if (selectedAgent.value?.id === id) {
          selectedAgent.value = null
        }
      }
      return success
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除 Agent 失败'
      return false
    }
  }

  /** 根据消息内容自动选择 Agent */
  async function selectAgent(message: string): Promise<AgentDto | null> {
    autoSelectedAgent.value = null
    if (!message.trim()) return null

    try {
      const agent = await selectAgentByMessage(message)
      autoSelectedAgent.value = agent
      return agent
    } catch (err) {
      return null
    }
  }

  /** 清除自动选择的 Agent */
  function clearAutoSelected(): void {
    autoSelectedAgent.value = null
  }

  /** 手动设置当前会话使用的 Agent */
  function setSessionAgent(agent: AgentDto | null): void {
    selectedAgent.value = agent
  }

  return {
    // 状态
    agents,
    loading,
    saving,
    error,
    selectedAgent,
    autoSelectedAgent,
    // 计算属性
    hasAgents,
    enabledAgents,
    canSave,
    // 弹窗状态
    dialogVisible,
    dialogTitle,
    formData,
    // 方法
    fetchAgents,
    fetchAgent,
    openCreateDialog,
    openEditDialog,
    closeDialog,
    saveAgent,
    removeAgent,
    selectAgent,
    clearAutoSelected,
    setSessionAgent,
  }
}
