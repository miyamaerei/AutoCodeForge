/**
 * useAgent Composable 单元测试
 */
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAgent } from '../useAgent'
import * as agentApi from '../../agent.api'
import type { AgentDto } from '../../agent.api'

describe('useAgent', () => {
  // 测试数据
  const mockAgent: AgentDto = {
    id: 'agent-1',
    name: '测试助手',
    description: '一个测试用的助手',
    icon: '🤖',
    systemPrompt: '你是一个测试助手',
    keywords: [{ keyword: '测试', weight: 1.0 }],
    enabled: true,
    createdAt: '2026-05-01 10:00:00',
    updatedAt: '2026-05-15 14:30:00',
  }

  // Spy objects
  let loadAgentsSpy: ReturnType<typeof vi.spyOn>
  let loadAgentSpy: ReturnType<typeof vi.spyOn>

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()

    loadAgentsSpy = vi.spyOn(agentApi, 'fetchAgents')
    loadAgentSpy = vi.spyOn(agentApi, 'getAgent')
  })

  describe('初始状态', () => {
    it('should have correct initial state', () => {
      const {
        agents,
        loading,
        saving,
        error,
        selectedAgent,
        autoSelectedAgent,
        dialogVisible,
        formData,
      } = useAgent()

      expect(agents.value).toEqual([])
      expect(loading.value).toBe(false)
      expect(saving.value).toBe(false)
      expect(error.value).toBe(null)
      expect(selectedAgent.value).toBe(null)
      expect(autoSelectedAgent.value).toBe(null)
      expect(dialogVisible.value).toBe(false)
      expect(formData.value.name).toBe('')
      expect(formData.value.icon).toBe('🤖')
    })
  })

  describe('openCreateDialog', () => {
    it('should open create dialog with default values', () => {
      const { openCreateDialog, dialogVisible, dialogTitle, formData } = useAgent()

      openCreateDialog()

      expect(dialogVisible.value).toBe(true)
      expect(dialogTitle.value).toBe('新建 Agent')
      expect(formData.value.name).toBe('')
      expect(formData.value.enabled).toBe(true)
    })
  })

  describe('openEditDialog', () => {
    it('should open edit dialog with agent data', () => {
      const { openEditDialog, dialogVisible, dialogTitle, formData } = useAgent()

      openEditDialog(mockAgent)

      expect(dialogVisible.value).toBe(true)
      expect(dialogTitle.value).toBe('编辑 Agent')
      expect(formData.value.name).toBe(mockAgent.name)
      expect(formData.value.description).toBe(mockAgent.description)
      expect(formData.value.icon).toBe(mockAgent.icon)
    })
  })

  describe('canSave computed', () => {
    it('should return falsy when name is empty', () => {
      const { canSave, formData } = useAgent()
      formData.value.name = ''
      formData.value.systemPrompt = 'some prompt'

      expect(canSave.value).toBeFalsy()
    })

    it('should return falsy when systemPrompt is empty', () => {
      const { canSave, formData } = useAgent()
      formData.value.name = 'Test Agent'
      formData.value.systemPrompt = ''

      expect(canSave.value).toBeFalsy()
    })

    it('should return truthy when name and systemPrompt are filled', () => {
      const { canSave, formData } = useAgent()
      formData.value.name = 'Test Agent'
      formData.value.systemPrompt = 'You are a helpful assistant'

      expect(canSave.value).toBeTruthy()
    })
  })
})
