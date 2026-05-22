/**
 * Agent 管理 Composable
 * 提供 Agent 的增删改查和自动选择功能（基于 Store）
 */
import { computed, ref, toValue } from 'vue'
import { storeToRefs } from 'pinia'
import { useAgentStore } from '../store/useAgentStore'
import type { AgentDto } from '../agent.api'

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

const genericSystemPromptTemplate =
  '你是企业研发场景下的通用 Agent。流程：1)先确认目标、范围与验收标准；2)阅读已有代码、接口与可复用组件；3)给出分步计划并标注风险与边界；4)按最小改动执行并记录关键决策；5)完成后输出结果、验证与后续建议。边界：不越权、不泄露密钥和隐私、不执行破坏性命令、未经确认不修改无关文件。质量：优先复用 store/composable，覆盖加载/错误/空状态，并进行基本回归检查。'

const defaultFormData: AgentFormData = {
  name: '',
  description: '',
  icon: '🤖',
  systemPrompt: '',
  keywordsText: '',
  enabled: true,
}

export function useAgent() {
  // 使用 Store
  const store = useAgentStore()
  const {
    agents,
    loading,
    saving,
    error,
    selectedAgent,
    autoSelectedAgent,
    hasAgents,
    enabledAgents,
  } = storeToRefs(store)

  // 弹窗状态（保留在 composable 中用于 UI 控制）
  const dialogVisible = ref(false)
  const dialogTitle = ref('新建 Agent')
  const formData = ref<AgentFormData>({ ...defaultFormData })

  // 计算属性
  const canSave = computed(() => {
    const fd = formData.value
    return fd.name.trim() && fd.systemPrompt.trim()
  })

  // 方法
  /** 加载所有 Agent */
  async function fetchAgents(): Promise<void> {
    await store.loadAgents()
  }

  /** 加载单个 Agent */
  async function fetchAgent(id: string): Promise<AgentDto | null> {
    return store.loadAgent(id)
  }

  /** 打开新建弹窗 */
  function openCreateDialog(): void {
    dialogTitle.value = '新建 Agent'
    formData.value = { ...defaultFormData }
    store.clearSelection()
    dialogVisible.value = true
  }

  /** 打开编辑弹窗 */
  function openEditDialog(agent: AgentDto): void {
    dialogTitle.value = '编辑 Agent'
    store.selectedAgent = agent
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
    store.clearSelection()
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

    const fd = formData.value
    const keywords = parseKeywords(fd.keywordsText)
    const currentSelected = toValue(selectedAgent)

    if (currentSelected) {
      // 更新
      const success = await store.submitUpdate({
        id: currentSelected.id,
        name: fd.name,
        description: fd.description,
        icon: fd.icon,
        systemPrompt: fd.systemPrompt,
        keywords,
        enabled: fd.enabled,
      })
      if (success) {
        closeDialog()
      }
      return !!success
    } else {
      // 新建
      const success = await store.submitCreate({
        name: fd.name,
        description: fd.description,
        icon: fd.icon,
        systemPrompt: fd.systemPrompt,
        keywords,
        enabled: fd.enabled,
      })
      if (success) {
        closeDialog()
      }
      return !!success
    }
  }

  /** 删除 Agent */
  async function removeAgent(id: string): Promise<boolean> {
    return store.submitDelete(id)
  }

  /** 根据消息内容自动选择 Agent */
  async function selectAgent(message: string): Promise<AgentDto | null> {
    if (!message.trim()) return null
    return store.matchAgent(message)
  }

  /** 清除自动选择的 Agent */
  function clearAutoSelected(): void {
    store.clearAutoSelected()
  }

  /** 手动设置当前会话使用的 Agent */
  function setSessionAgent(agent: AgentDto | null): void {
    store.selectedAgent = agent
  }

  return {
    // 状态（来自 store）
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
    genericSystemPromptTemplate,
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
