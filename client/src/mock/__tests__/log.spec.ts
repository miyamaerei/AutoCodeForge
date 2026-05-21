/**
 * Mock 函数单元测试 - log.ts
 */
import { describe, it, expect, beforeEach } from 'vitest'
import { getTaskLogs } from '../log'

describe('log mock', () => {
  beforeEach(() => {
    // Reset modules if needed
  })

  describe('getTaskLogs', () => {
    it('should return logs for known task', async () => {
      const logs = await getTaskLogs('T-1009')

      expect(Array.isArray(logs)).toBe(true)
      expect(logs.length).toBeGreaterThan(0)
      expect(logs[0]).toHaveProperty('id')
      expect(logs[0]).toHaveProperty('taskId', 'T-1009')
      expect(logs[0]).toHaveProperty('message')
    })

    it('should return empty array for unknown task', async () => {
      const logs = await getTaskLogs('unknown-task')

      expect(logs).toEqual([])
    })

    it('should have valid log structure', async () => {
      const logs = await getTaskLogs('T-1009')

      if (logs.length > 0) {
        expect(typeof logs[0].id).toBe('string')
        expect(typeof logs[0].taskId).toBe('string')
        expect(typeof logs[0].message).toBe('string')
        expect(logs[0].message).toContain('[') // Log format includes timestamp
      }
    })
  })
})
