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
  getAgentsByState,
  getDormantAgents,
  assignTask,
  completeTask,
  failTask,
  startLearning,
  enterDormant,
  wakeUpAgent,
  getAgentLearningRecords,
  getAgentDormantRecords,
} from '../agent.api'
import type {
  AgentDto,
  CreateAgentDto,
  UpdateAgentDto,
  AssignTaskRequestDto,
  FailTaskRequestDto,
  EnterDormantRequestDto,
  AgentLearningRecordDto,
  AgentDormantRecordDto,
} from '../api/agent.types'
import { AgentState } from '../api/agent.types'

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

  // 学习和休眠记录
  const agentLearningRecords = ref<Map<string, AgentLearningRecordDto[]>>(new Map())
  const agentDormantRecords = ref<Map<string, AgentDormantRecordDto[]>>(new Map())

  // ==================== 计算属性 ====================
  const hasAgents = computed(() => agents.value.length > 0)
  const enabledAgents = computed(() => agents.value.filter((a) => a.enabled))
  const idleAgents = computed(() => agents.value.filter((a) => a.state === AgentState.Idle))
  const handlingAgents = computed(() => agents.value.filter((a) => a.state === AgentState.Handling))
  const learningAgents = computed(() => agents.value.filter((a) => a.state === AgentState.Learning))
  const dormantAgents = computed(() => agents.value.filter((a) => a.state === AgentState.Dormant))

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
        // 同步更新列表中的 Agent
        const index = agents.value.findIndex((a) => a.id === id)
        if (index !== -1) {
          agents.value[index] = agent
        }
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
        // 清除相关记录
        agentLearningRecords.value.delete(id)
        agentDormantRecords.value.delete(id)
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

  // ==================== 生命周期管理操作 ====================

  /** 加载指定状态的 Agent */
  async function loadAgentsByState(state: AgentState): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const result = await getAgentsByState(state)
      agents.value = result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载 Agent 列表失败'
    } finally {
      loading.value = false
    }
  }

  /** 加载休眠的 Agent */
  async function loadDormantAgents(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const result = await getDormantAgents()
      agents.value = result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载休眠 Agent 失败'
    } finally {
      loading.value = false
    }
  }

  /** 分配任务给 Agent */
  async function assignAgentTask(agentId: string, dto: AssignTaskRequestDto): Promise<AgentDto | null> {
    saving.value = true
    error.value = null
    try {
      const updated = await assignTask(agentId, dto)
      updateAgentInList(updated)
      return updated
    } catch (err) {
      error.value = err instanceof Error ? err.message : '分配任务失败'
      return null
    } finally {
      saving.value = false
    }
  }

  /** 完成 Agent 任务 */
  async function completeAgentTask(agentId: string): Promise<AgentDto | null> {
    saving.value = true
    error.value = null
    try {
      const updated = await completeTask(agentId)
      updateAgentInList(updated)
      return updated
    } catch (err) {
      error.value = err instanceof Error ? err.message : '完成任务失败'
      return null
    } finally {
      saving.value = false
    }
  }

  /** 标记任务失败 */
  async function failAgentTask(agentId: string, dto: FailTaskRequestDto): Promise<AgentDto | null> {
    saving.value = true
    error.value = null
    try {
      const updated = await failTask(agentId, dto)
      updateAgentInList(updated)
      return updated
    } catch (err) {
      error.value = err instanceof Error ? err.message : '标记任务失败'
      return null
    } finally {
      saving.value = false
    }
  }

  /** 触发 Agent 学习 */
  async function startAgentLearning(agentId: string): Promise<AgentDto | null> {
    saving.value = true
    error.value = null
    try {
      const updated = await startLearning(agentId)
      updateAgentInList(updated)
      return updated
    } catch (err) {
      error.value = err instanceof Error ? err.message : '触发学习失败'
      return null
    } finally {
      saving.value = false
    }
  }

  /** Agent 进入休眠 */
  async function enterAgentDormant(agentId: string, dto: EnterDormantRequestDto): Promise<AgentDto | null> {
    saving.value = true
    error.value = null
    try {
      const updated = await enterDormant(agentId, dto)
      updateAgentInList(updated)
      // 刷新休眠记录
      await loadAgentDormantRecords(agentId)
      return updated
    } catch (err) {
      error.value = err instanceof Error ? err.message : '进入休眠失败'
      return null
    } finally {
      saving.value = false
    }
  }

  /** 唤醒休眠的 Agent */
  async function wakeUpAgentFromDormant(agentId: string): Promise<AgentDto | null> {
    saving.value = true
    error.value = null
    try {
      const updated = await wakeUpAgent(agentId)
      updateAgentInList(updated)
      // 刷新休眠记录
      await loadAgentDormantRecords(agentId)
      return updated
    } catch (err) {
      error.value = err instanceof Error ? err.message : '唤醒失败'
      return null
    } finally {
      saving.value = false
    }
  }

  /** 加载 Agent 学习记录 */
  async function loadAgentLearningRecords(agentId: string): Promise<AgentLearningRecordDto[]> {
    loading.value = true
    error.value = null
    try {
      const records = await getAgentLearningRecords(agentId)
      agentLearningRecords.value.set(agentId, records)
      return records
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载学习记录失败'
      return []
    } finally {
      loading.value = false
    }
  }

  /** 加载 Agent 休眠记录 */
  async function loadAgentDormantRecords(agentId: string): Promise<AgentDormantRecordDto[]> {
    loading.value = true
    error.value = null
    try {
      const records = await getAgentDormantRecords(agentId)
      agentDormantRecords.value.set(agentId, records)
      return records
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载休眠记录失败'
      return []
    } finally {
      loading.value = false
    }
  }

  /** 获取 Agent 的学习记录 */
  function getAgentLearningRecordsFromState(agentId: string): AgentLearningRecordDto[] {
    return agentLearningRecords.value.get(agentId) || []
  }

  /** 获取 Agent 的休眠记录 */
  function getAgentDormantRecordsFromState(agentId: string): AgentDormantRecordDto[] {
    return agentDormantRecords.value.get(agentId) || []
  }

  // ==================== 辅助函数 ====================

  /** 更新列表中的 Agent */
  function updateAgentInList(agent: AgentDto): void {
    const idx = agents.value.findIndex((a) => a.id === agent.id)
    if (idx !== -1) {
      agents.value[idx] = agent
    }
    if (selectedAgent.value?.id === agent.id) {
      selectedAgent.value = agent
    }
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
    idleAgents,
    handlingAgents,
    learningAgents,
    dormantAgents,

    // 基本操作
    loadAgents,
    loadAgent,
    submitCreate,
    submitUpdate,
    submitDelete,
    matchAgent,
    clearSelection,
    clearAutoSelected,

    // 生命周期管理操作
    loadAgentsByState,
    loadDormantAgents,
    assignAgentTask,
    completeAgentTask,
    failAgentTask,
    startAgentLearning,
    enterAgentDormant,
    wakeUpAgentFromDormant,
    loadAgentLearningRecords,
    loadAgentDormantRecords,
    getAgentLearningRecordsFromState,
    getAgentDormantRecordsFromState,
  }
})
