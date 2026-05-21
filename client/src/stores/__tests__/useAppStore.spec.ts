/**
 * useAppStore 单元测试
 */
import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAppStore } from '../app'

describe('useAppStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  describe('初始状态', () => {
    it('should have correct initial state', () => {
      const store = useAppStore()

      expect(store.lastSubmission).toBe(null)
      expect(store.hasSubmission).toBe(false)
    })
  })

  describe('setLastSubmission', () => {
    it('should set submission correctly', () => {
      const store = useAppStore()
      const submission = {
        name: '张三',
        email: 'zhangsan@example.com',
        message: '测试消息',
      }

      store.setLastSubmission(submission)

      expect(store.lastSubmission).toEqual(submission)
      expect(store.hasSubmission).toBe(true)
    })

    it('should update submission when called again', () => {
      const store = useAppStore()
      store.setLastSubmission({
        name: '张三',
        email: 'zhangsan@example.com',
        message: '第一条消息',
      })

      store.setLastSubmission({
        name: '李四',
        email: 'lisi@example.com',
        message: '第二条消息',
      })

      expect(store.lastSubmission?.name).toBe('李四')
      expect(store.lastSubmission?.message).toBe('第二条消息')
    })
  })

  describe('hasSubmission', () => {
    it('should return false when no submission', () => {
      const store = useAppStore()
      expect(store.hasSubmission).toBe(false)
    })

    it('should return true after setLastSubmission', () => {
      const store = useAppStore()
      store.setLastSubmission({
        name: '测试',
        email: 'test@example.com',
        message: '测试',
      })
      expect(store.hasSubmission).toBe(true)
    })
  })
})
