/**
 * useConsoleChat Composable 单元测试
 */
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useConsoleChat } from '../useConsoleChat'
import { ref } from 'vue'

describe('useConsoleChat', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('初始状态', () => {
    it('should have correct initial state', () => {
      const mode = ref<'ask' | 'session'>('ask')
      const {
        initializing,
        sending,
        error,
        inputMessage,
      } = useConsoleChat(mode)

      expect(initializing.value).toBe(false)
      expect(sending.value).toBe(false)
      expect(error.value).toBe(null)
      expect(inputMessage.value).toBe('')
    })
  })

  describe('activeMessages computed', () => {
    it('should return array in ask mode', () => {
      const mode = ref<'ask' | 'session'>('ask')
      const { activeMessages } = useConsoleChat(mode)

      expect(Array.isArray(activeMessages.value)).toBe(true)
    })

    it('should return sessionMessages in session mode', () => {
      const mode = ref<'ask' | 'session'>('session')
      const { activeMessages } = useConsoleChat(mode)

      // In session mode, it returns sessionMessages (empty initially)
      expect(Array.isArray(activeMessages.value)).toBe(true)
    })
  })

  describe('isEmptyState computed', () => {
    it('should return boolean value', () => {
      const mode = ref<'ask' | 'session'>('ask')
      const { isEmptyState } = useConsoleChat(mode)

      expect(typeof isEmptyState.value).toBe('boolean')
    })
  })

  describe('inputMessage', () => {
    it('should allow setting input message', () => {
      const mode = ref<'ask' | 'session'>('ask')
      const { inputMessage } = useConsoleChat(mode)

      inputMessage.value = '测试消息'

      expect(inputMessage.value).toBe('测试消息')
    })
  })

  describe('suggestedQuestions', () => {
    it('should return list of suggested questions', () => {
      const mode = ref<'ask' | 'session'>('ask')
      const { suggestedQuestions } = useConsoleChat(mode)

      expect(Array.isArray(suggestedQuestions)).toBe(true)
      expect(suggestedQuestions.length).toBeGreaterThan(0)
    })
  })
})
