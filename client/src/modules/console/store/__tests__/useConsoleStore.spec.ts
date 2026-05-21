/**
 * useConsoleStore 单元测试
 */
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useConsoleStore } from '../useConsoleStore'
import * as taskMock from '../../../../mock/task'

describe('useConsoleStore', () => {
  // Spy objects
  let getDashboardStatsSpy: ReturnType<typeof vi.spyOn>
  let getRecentTasksSpy: ReturnType<typeof vi.spyOn>
  let getModuleEntriesSpy: ReturnType<typeof vi.spyOn>

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()

    // Setup spies
    getDashboardStatsSpy = vi.spyOn(taskMock, 'getDashboardStats')
    getRecentTasksSpy = vi.spyOn(taskMock, 'getRecentTasks')
    getModuleEntriesSpy = vi.spyOn(taskMock, 'getModuleEntries')
  })

  describe('初始状态', () => {
    it('should have correct initial state', () => {
      const store = useConsoleStore()

      expect(store.stats).toBe(null)
      expect(store.recentTasks).toEqual([])
      expect(store.moduleEntries).toEqual([])
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
    })

    it('should have correct computed properties', () => {
      const store = useConsoleStore()
      expect(store.hasData).toBe(false)
    })
  })

  describe('fetchConsoleData', () => {
    it('should fetch all console data successfully', async () => {
      const store = useConsoleStore()
      const mockStats = { tasksToday: 5, successRate: 0.8, runningCount: 2, alertCount: 1 }
      const mockTasks = [{ id: 'T-1', name: 'Task 1', state: '成功' as const, percent: 100 }]
      const mockEntries = [{ title: 'Entry', desc: 'desc', tagType: 'info' as const, status: 'ok', routePath: '/entry' }]

      getDashboardStatsSpy.mockResolvedValue(mockStats)
      getRecentTasksSpy.mockResolvedValue(mockTasks)
      getModuleEntriesSpy.mockResolvedValue(mockEntries)

      await store.fetchConsoleData()

      expect(store.stats).toEqual(mockStats)
      expect(store.recentTasks).toEqual(mockTasks)
      expect(store.moduleEntries).toEqual(mockEntries)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
      expect(store.hasData).toBe(true)
    })

    it('should set error on fetch failure', async () => {
      const store = useConsoleStore()
      getDashboardStatsSpy.mockRejectedValue(new Error('网络错误'))

      await store.fetchConsoleData()

      expect(store.error).toBe('网络错误')
      expect(store.loading).toBe(false)
    })

    it('should set loading state during request', async () => {
      const store = useConsoleStore()
      getDashboardStatsSpy.mockImplementation(
        () =>
          new Promise((resolve) =>
            setTimeout(() => resolve({ tasksToday: 0, successRate: 0, runningCount: 0, alertCount: 0 }), 100),
          ),
      )
      getRecentTasksSpy.mockImplementation(
        () =>
          new Promise((resolve) =>
            setTimeout(() => resolve([]), 100),
          ),
      )
      getModuleEntriesSpy.mockImplementation(
        () =>
          new Promise((resolve) =>
            setTimeout(() => resolve([]), 100),
          ),
      )

      const fetchPromise = store.fetchConsoleData()
      expect(store.loading).toBe(true)

      await fetchPromise
      expect(store.loading).toBe(false)
    })
  })
})
