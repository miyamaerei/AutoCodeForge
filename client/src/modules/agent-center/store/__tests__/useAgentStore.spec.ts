/**
 * useAgentStore 单元测试
 */
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAgentStore } from '../useAgentStore'
import * as agentApi from '../../agent.api'
import type { AgentDto, CreateAgentDto, UpdateAgentDto, PagedResult } from '../../api/agent.types'
import { AgentState, AgentRole } from '../../api/agent.types'

describe('useAgentStore', () => {
  // 测试数据
  const mockAgents: AgentDto[] = [
    {
      id: 'agent-1',
      name: '代码审查助手',
      description: '专注于代码质量审查',
      icon: '🔍',
      systemPrompt: '你是一个专业的代码审查助手',
      keywords: [{ keyword: 'review', weight: 1.0 }],
      enabled: true,
      state: AgentState.Idle,
      role: AgentRole.Worker,
      stateChangedAt: '2026-05-22 10:00:00',
      learningProgress: 0,
      version: 1,
      createdAt: '2026-05-01 10:00:00',
      updatedAt: '2026-05-15 14:30:00',
    },
    {
      id: 'agent-2',
      name: '架构设计专家',
      description: '提供系统架构设计建议',
      icon: '🏗️',
      systemPrompt: '你是一个资深的系统架构师',
      keywords: [{ keyword: '架构', weight: 1.0 }],
      enabled: true,
      state: AgentState.Idle,
      role: AgentRole.Manager,
      stateChangedAt: '2026-05-22 11:00:00',
      learningProgress: 0,
      version: 1,
      createdAt: '2026-05-02 09:00:00',
      updatedAt: '2026-05-16 11:00:00',
    },
    {
      id: 'agent-3',
      name: '禁用的助手',
      description: '这是一个禁用的助手',
      icon: '❌',
      systemPrompt: '我被禁用了',
      keywords: [{ keyword: '禁用', weight: 1.0 }],
      enabled: false,
      state: AgentState.Idle,
      role: AgentRole.Worker,
      stateChangedAt: '2026-05-22 12:00:00',
      learningProgress: 0,
      version: 1,
      createdAt: '2026-05-03 08:30:00',
      updatedAt: '2026-05-17 16:00:00',
    },
  ]
  const firstAgent = mockAgents[0]!
  const secondAgent = mockAgents[1]!

  // Spy objects
  let fetchAgentsSpy: ReturnType<typeof vi.spyOn>
  let getAgentSpy: ReturnType<typeof vi.spyOn>
  let createAgentSpy: ReturnType<typeof vi.spyOn>
  let updateAgentSpy: ReturnType<typeof vi.spyOn>
  let deleteAgentSpy: ReturnType<typeof vi.spyOn>
  let selectAgentByMessageSpy: ReturnType<typeof vi.spyOn>

  // Helper function to set store state using $patch
  function setStoreAgents(store: ReturnType<typeof useAgentStore>, agents: AgentDto[]) {
    store.$patch({ agents })
  }

  function setStoreSelectedAgent(
    store: ReturnType<typeof useAgentStore>,
    agent: AgentDto | null,
  ) {
    store.$patch({ selectedAgent: agent })
  }

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()

    // Setup spies for API functions
    fetchAgentsSpy = vi.spyOn(agentApi, 'fetchAgents')
    getAgentSpy = vi.spyOn(agentApi, 'getAgent')
    createAgentSpy = vi.spyOn(agentApi, 'createAgent')
    updateAgentSpy = vi.spyOn(agentApi, 'updateAgent')
    deleteAgentSpy = vi.spyOn(agentApi, 'deleteAgent')
    selectAgentByMessageSpy = vi.spyOn(agentApi, 'selectAgentByMessage')
  })

  describe('初始状态', () => {
    it('should have correct initial state', () => {
      const store = useAgentStore()

      expect(store.agents).toEqual([])
      expect(store.selectedAgent).toBe(null)
      expect(store.autoSelectedAgent).toBe(null)
      expect(store.loading).toBe(false)
      expect(store.saving).toBe(false)
      expect(store.error).toBe(null)
      expect(store.totalCount).toBe(0)
      expect(store.currentPage).toBe(1)
    })

    it('should have correct computed properties', () => {
      const store = useAgentStore()

      expect(store.hasAgents).toBe(false)
      expect(store.enabledAgents).toEqual([])
    })
  })

  describe('loadAgents', () => {
    it('should load agents successfully', async () => {
      const store = useAgentStore()
      const pagedResult: PagedResult<AgentDto> = {
        items: [firstAgent, secondAgent],
        totalCount: 2,
        page: 1,
        pageSize: 20,
      }

      fetchAgentsSpy.mockResolvedValue(pagedResult)

      await store.loadAgents()

      expect(store.agents).toEqual([firstAgent, secondAgent])
      expect(store.totalCount).toBe(2)
      expect(store.currentPage).toBe(1)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
    })

    it('should set error on load failure', async () => {
      const store = useAgentStore()
      fetchAgentsSpy.mockRejectedValue(new Error('网络错误'))

      await store.loadAgents()

      expect(store.error).toBe('网络错误')
      expect(store.loading).toBe(false)
    })

    it('should set loading state during request', async () => {
      const store = useAgentStore()
      fetchAgentsSpy.mockImplementation(
        () =>
          new Promise((resolve) =>
            setTimeout(() => resolve({ items: [], totalCount: 0, page: 1, pageSize: 20 }), 100),
          ),
      )

      const loadPromise = store.loadAgents()
      expect(store.loading).toBe(true)

      await loadPromise
      expect(store.loading).toBe(false)
    })

    it('should use correct pagination parameters', async () => {
      const store = useAgentStore()
      fetchAgentsSpy.mockResolvedValue({ items: [], totalCount: 0, page: 2, pageSize: 10 })

      await store.loadAgents(2, 10)

      expect(fetchAgentsSpy).toHaveBeenCalledWith(2, 10)
      expect(store.currentPage).toBe(2)
    })
  })

  describe('loadAgent', () => {
    it('should load single agent successfully', async () => {
      const store = useAgentStore()
      getAgentSpy.mockResolvedValue(firstAgent)

      const agent = await store.loadAgent('agent-1')

      expect(agent).toEqual(mockAgents[0])
      expect(store.selectedAgent).toEqual(mockAgents[0])
      expect(store.loading).toBe(false)
    })

    it('should return null when agent not found', async () => {
      const store = useAgentStore()
      getAgentSpy.mockResolvedValue(null)

      const agent = await store.loadAgent('nonexistent')

      expect(agent).toBe(null)
      expect(store.selectedAgent).toBe(null)
    })
  })

  describe('submitCreate', () => {
    it('should create agent successfully', async () => {
      const store = useAgentStore()
      const newAgent: CreateAgentDto = {
        name: '新助手',
        description: '新创建的助手',
        icon: '🆕',
        systemPrompt: '我是新助手',
        keywords: [{ keyword: '新', weight: 1.0 }],
        enabled: true,
        role: AgentRole.Worker,
      }
      const createdAgent: AgentDto = {
        ...newAgent,
        id: 'agent-new',
        state: AgentState.Idle,
        stateChangedAt: '2026-05-21 10:00:00',
        learningProgress: 0,
        version: 1,
        createdAt: '2026-05-21 10:00:00',
        updatedAt: '2026-05-21 10:00:00',
      }
      createAgentSpy.mockResolvedValue(createdAgent)

      const result = await store.submitCreate(newAgent)

      expect(result).toEqual(createdAgent)
      expect(store.agents[0]).toEqual(createdAgent)
      expect(store.saving).toBe(false)
    })

    it('should set error on create failure', async () => {
      const store = useAgentStore()
      createAgentSpy.mockRejectedValue(new Error('创建失败'))

      const result = await store.submitCreate({
        name: '新助手',
        description: '',
        icon: '🆕',
        systemPrompt: '',
        keywords: [],
        enabled: true,
        role: AgentRole.Worker,
      })

      expect(result).toBe(null)
      expect(store.error).toBe('创建失败')
    })
  })

  describe('submitUpdate', () => {
    it('should update agent successfully', async () => {
      const store = useAgentStore()
      setStoreAgents(store, [...mockAgents])
      const updateDto: UpdateAgentDto = {
        id: 'agent-1',
        name: '更新的名称',
      }
      const updatedAgent: AgentDto = {
        ...firstAgent,
        name: '更新的名称',
      }
      updateAgentSpy.mockResolvedValue(updatedAgent)

      const result = await store.submitUpdate(updateDto)

      expect(result).toEqual(updatedAgent)
      expect(store.agents[0]!.name).toBe('更新的名称')
      expect(store.saving).toBe(false)
    })

    it('should update selectedAgent when updating selected one', async () => {
      const store = useAgentStore()
      setStoreAgents(store, [...mockAgents])
      setStoreSelectedAgent(store, firstAgent)
      const updateDto: UpdateAgentDto = {
        id: 'agent-1',
        name: '更新的名称',
      }
      const updatedAgent: AgentDto = {
        ...firstAgent,
        name: '更新的名称',
      }
      updateAgentSpy.mockResolvedValue(updatedAgent)

      await store.submitUpdate(updateDto)

      expect(store.selectedAgent?.name).toBe('更新的名称')
    })

    it('should set error on update failure', async () => {
      const store = useAgentStore()
      setStoreAgents(store, [...mockAgents])
      updateAgentSpy.mockRejectedValue(new Error('更新失败'))

      await store.submitUpdate({ id: 'agent-1', name: 'test' })

      expect(store.error).toBe('更新失败')
    })
  })

  describe('submitDelete', () => {
    it('should delete agent successfully', async () => {
      const store = useAgentStore()
      setStoreAgents(store, [...mockAgents])
      setStoreSelectedAgent(store, firstAgent)
      deleteAgentSpy.mockResolvedValue(true)

      const result = await store.submitDelete('agent-1')

      expect(result).toBe(true)
      expect(store.agents.find((a) => a.id === 'agent-1')).toBeUndefined()
      expect(store.selectedAgent).toBe(null)
    })

    it('should set error on delete failure', async () => {
      const store = useAgentStore()
      setStoreAgents(store, [...mockAgents])
      deleteAgentSpy.mockRejectedValue(new Error('删除失败'))

      const result = await store.submitDelete('agent-1')

      expect(result).toBe(false)
      expect(store.error).toBe('删除失败')
    })

    it('should not clear selectedAgent if deleting different agent', async () => {
      const store = useAgentStore()
      setStoreAgents(store, [...mockAgents])
      setStoreSelectedAgent(store, firstAgent)
      deleteAgentSpy.mockResolvedValue(true)

      await store.submitDelete('agent-2')

      expect(store.selectedAgent).toEqual(mockAgents[0])
    })
  })

  describe('matchAgent', () => {
    it('should match agent by message', async () => {
      const store = useAgentStore()
      selectAgentByMessageSpy.mockResolvedValue(mockAgents[0])

      const result = await store.matchAgent('帮我审查代码')

      expect(result).toEqual(mockAgents[0])
      expect(store.autoSelectedAgent).toEqual(mockAgents[0])
    })

    it('should return null when no match found', async () => {
      const store = useAgentStore()
      selectAgentByMessageSpy.mockResolvedValue(null)

      const result = await store.matchAgent('未知内容')

      expect(result).toBe(null)
      expect(store.autoSelectedAgent).toBe(null)
    })
  })

  describe('clearSelection', () => {
    it('should clear selectedAgent', () => {
      const store = useAgentStore()
      setStoreSelectedAgent(store, firstAgent)

      store.clearSelection()

      expect(store.selectedAgent).toBe(null)
    })
  })

  describe('clearAutoSelected', () => {
    it('should clear autoSelectedAgent', () => {
      const store = useAgentStore()
      store.$patch({ autoSelectedAgent: mockAgents[0] })

      store.clearAutoSelected()

      expect(store.autoSelectedAgent).toBe(null)
    })
  })

  describe('computed properties', () => {
    it('should calculate hasAgents correctly', () => {
      const store = useAgentStore()
      expect(store.hasAgents).toBe(false)

      setStoreAgents(store, [firstAgent])
      expect(store.hasAgents).toBe(true)

      setStoreAgents(store, [])
      expect(store.hasAgents).toBe(false)
    })

    it('should filter enabled agents', () => {
      const store = useAgentStore()
      setStoreAgents(store, mockAgents)

      expect(store.enabledAgents).toHaveLength(2)
      expect(store.enabledAgents.every((a) => a.enabled)).toBe(true)
    })
  })
})
