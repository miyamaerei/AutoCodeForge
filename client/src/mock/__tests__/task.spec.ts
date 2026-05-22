/**
 * Mock 函数单元测试 - task.ts
 */
import { describe, it, expect, beforeEach } from 'vitest'
import {
  getDashboardStats,
  getRecentTasks,
  getModuleEntries,
  getTaskSummaries,
  getTaskDetail,
  createTask,
} from '../task'

describe('task mock', () => {
  beforeEach(() => {
    // Reset modules if needed
  })

  describe('getDashboardStats', () => {
    it('should return valid dashboard stats', async () => {
      const stats = await getDashboardStats()

      expect(stats).toHaveProperty('tasksToday')
      expect(stats).toHaveProperty('successRate')
      expect(stats).toHaveProperty('runningCount')
      expect(stats).toHaveProperty('alertCount')
      expect(typeof stats.tasksToday).toBe('number')
      expect(typeof stats.successRate).toBe('number')
      expect(typeof stats.runningCount).toBe('number')
      expect(typeof stats.alertCount).toBe('number')
    })
  })

  describe('getRecentTasks', () => {
    it('should return list of recent tasks', async () => {
      const tasks = await getRecentTasks()

      expect(Array.isArray(tasks)).toBe(true)
      expect(tasks.length).toBeGreaterThan(0)
      expect(tasks[0]).toHaveProperty('id')
      expect(tasks[0]).toHaveProperty('name')
      expect(tasks[0]).toHaveProperty('state')
      expect(tasks[0]).toHaveProperty('percent')
    })
  })

  describe('getModuleEntries', () => {
    it('should return list of module entries', async () => {
      const entries = await getModuleEntries()

      expect(Array.isArray(entries)).toBe(true)
      expect(entries.length).toBeGreaterThan(0)
      expect(entries[0]).toHaveProperty('title')
      expect(entries[0]).toHaveProperty('desc')
      expect(entries[0]).toHaveProperty('tagType')
      expect(entries[0]).toHaveProperty('routePath')
    })
  })

  describe('getTaskSummaries', () => {
    it('should return list of task summaries', async () => {
      const summaries = await getTaskSummaries()

      expect(Array.isArray(summaries)).toBe(true)
      expect(summaries.length).toBeGreaterThan(0)
      expect(summaries[0]).toHaveProperty('id')
      expect(summaries[0]).toHaveProperty('title')
      expect(summaries[0]).toHaveProperty('state')
      expect(summaries[0]).toHaveProperty('createdAt')
    })
  })

  describe('getTaskDetail', () => {
    it('should return task detail for known task', async () => {
      const detail = await getTaskDetail('T-1009')

      expect(detail).not.toBeNull()
      expect(detail).toHaveProperty('id', 'T-1009')
      expect(detail).toHaveProperty('title')
      expect(detail).toHaveProperty('state')
      expect(detail).toHaveProperty('steps')
      expect(Array.isArray(detail.steps)).toBe(true)
    })

    it('should throw error for unknown task', async () => {
      await expect(getTaskDetail('unknown')).rejects.toThrow()
    })
  })

  describe('createTask', () => {
    it('should create task and return summary', async () => {
      const task = await createTask({
        title: '测试任务',
        taskType: 'code-review',
        description: '描述',
        repository: 'repo-1',
        branch: 'main',
      })

      expect(task).toHaveProperty('id')
      expect(task).toHaveProperty('title', '测试任务')
      expect(task).toHaveProperty('state')
      expect(task).toHaveProperty('createdAt')
    })
  })
})
