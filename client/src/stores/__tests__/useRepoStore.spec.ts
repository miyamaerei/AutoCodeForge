/**
 * useRepoStore 单元测试
 */
import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useRepoStore } from '../useRepoStore'

describe('useRepoStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  describe('初始状态', () => {
    it('should have correct initial state', () => {
      const store = useRepoStore()

      expect(store.selectedRepositoryId).toBe(null)
    })
  })

  describe('selectRepository', () => {
    it('should select repository correctly', () => {
      const store = useRepoStore()

      store.selectRepository('repo-123')

      expect(store.selectedRepositoryId).toBe('repo-123')
    })

    it('should update selection when called again', () => {
      const store = useRepoStore()

      store.selectRepository('repo-123')
      store.selectRepository('repo-456')

      expect(store.selectedRepositoryId).toBe('repo-456')
    })

    it('should accept null to clear selection', () => {
      const store = useRepoStore()
      store.selectRepository('repo-123')

      store.selectRepository(null)

      expect(store.selectedRepositoryId).toBe(null)
    })
  })
})
