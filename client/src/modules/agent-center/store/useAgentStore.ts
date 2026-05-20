/**
 * Agent Center Store
 * 管理 Agent 的状态和操作
 */
import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import {
  createAgent,
  deleteAgent,
  fetchAgents,
  getAgent,
  selectAgentByMessage,
  updateAgent,
  type AgentDto,
  type CreateAgentDto,
  type UpdateAgentDto,
} from '../agent.api'

export const useAgentStore = defineStore('module.agent-center', () => {
  // ==================== 状态 ====================
  const agents = ref<AgentDto[]>([])
  const selectedAgent = ref<AgentDto | null>(null)
  const autoSelectedAgent = ref<AgentDto | null>(null)
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)
  const totalCount = ref(0)
  const currentPage = ref(1)

  // ==================== 计算属性 ====================
  const hasAgents = computed(() => agents.value.length > 0)
  const enabledAgents = computed(() => agents.value.filter((a) => a.enabled))

  // ==================== 操作 ====================

  /** 加载 Agent 列表（分页） */
  async function loadAgents(page = 1, pageSize = 20): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const result = await fetchAgents(page, pageSize)
      agents.value = result.items
      totalCount.value = result.totalCount
      currentPage.value = page
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载 Agent 列表失败'
    } finally {
      loading.value = false
    }
  }

  /** 加载单个 Agent */
  async function loadAgent(id: string): Promise<AgentDto | null> {
    loading.value = true
    error.value = null
    try {
      const agent = await getAgent(id)
      if (agent) {
        selectedAgent.value = agent
      }
      return agent
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载 Agent 详情失败'
      return null
    } finally {
      loading.value = false
    }
  }

  /** 创建 Agent */
  async function submitCreate(payload: CreateAgentDto): Promise<AgentDto | null> {
    saving.value = true
    error.value = null
    try {
      const created = await createAgent(payload)
      agents.value.unshift(created)
      return created
    } catch (err) {
      error.value = err instanceof Error ? err.message : '创建 Agent 失败'
      return null
    } finally {
      saving.value = false
    }
  }

  /** 更新 Agent */
  async function submitUpdate(payload: UpdateAgentDto): Promise<AgentDto | null> {
    saving.value = true
    error.value = null
    try {
      const updated = await updateAgent(payload)
      if (updated) {
        const idx = agents.value.findIndex((a) => a.id === updated.id)
        if (idx !== -1) agents.value[idx] = updated
        if (selectedAgent.value?.id === updated.id) {
          selectedAgent.value = updated
        }
      }
      return updated
    } catch (err) {
      error.value = err instanceof Error ? err.message : '更新 Agent 失败'
      return null
    } finally {
      saving.value = false
    }
  }

  /** 删除 Agent */
  async function submitDelete(id: string): Promise<boolean> {
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
  async function matchAgent(message: string): Promise<AgentDto | null> {
    try {
      const matched = await selectAgentByMessage(message)
      autoSelectedAgent.value = matched
      return matched
    } catch {
      autoSelectedAgent.value = null
      return null
    }
  }

  /** 清除选中状态 */
  function clearSelection(): void {
    selectedAgent.value = null
  }

  /** 清除自动选择的 Agent */
  function clearAutoSelected(): void {
    autoSelectedAgent.value = null
  }

  return {
    // 状态
    agents,
    selectedAgent,
    autoSelectedAgent,
    loading,
    saving,
    error,
    totalCount,
    currentPage,
    // 计算属性
    hasAgents,
    enabledAgents,
    // 操作
    loadAgents,
    loadAgent,
    submitCreate,
    submitUpdate,
    submitDelete,
    matchAgent,
    clearSelection,
    clearAutoSelected,
  }
})
