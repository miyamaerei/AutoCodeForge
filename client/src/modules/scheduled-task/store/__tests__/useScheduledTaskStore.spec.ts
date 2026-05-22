/**
 * useScheduledTaskStore 单元测试
 */
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useScheduledTaskStore } from '../useScheduledTaskStore'
import * as taskApi from '../../api/scheduled-task.api'
import type {
  ScheduledTaskDto,
  CreateScheduledTaskDto,
  UpdateScheduledTaskDto,
  ScheduledTaskExecutionDto,
  TaskTemplateDto,
  PagedResult,
} from '../../api/scheduled-task.types'

describe('useScheduledTaskStore', () => {
  // 测试数据
  const mockTasks: ScheduledTaskDto[] = [
    {
      id: 'task-1',
      name: '每日备份',
      triggerType: 'cron',
      cronExpression: '0 0 2 * * ?',
      status: 'pending',
      input: '{"action":"backup"}',
      taskTitle: '每日备份',
      createdAtUtc: '2026-05-01T10:00:00Z',
      nextRunAtUtc: '2026-05-22T02:00:00Z',
    },
    {
      id: 'task-2',
      name: '数据同步',
      triggerType: 'interval',
      cronExpression: '0 0 * * * ?',
      status: 'disabled',
      input: '{"action":"sync"}',
      taskTitle: '数据同步',
      createdAtUtc: '2026-05-05T10:00:00Z',
    },
  ]
  const firstTask = mockTasks[0]!
  const secondTask = mockTasks[1]!

  const mockTemplates: TaskTemplateDto[] = [
    {
      id: 'template-1',
      name: '备份任务',
      description: '通用的备份任务模板',
      agentId: 'agent-1',
      isBuiltIn: true,
      triggerType: 'cron',
      cronExpression: '0 0 2 * * ?',
      defaultParams: '{}',
    },
  ]

  const mockExecutions: ScheduledTaskExecutionDto[] = [
    {
      id: 'exec-1',
      scheduledTaskId: 'task-1',
      status: 'success',
      startedAtUtc: '2026-05-21T02:00:00Z',
      completedAtUtc: '2026-05-21T02:05:00Z',
      output: '备份成功',
    },
  ]

  // Spy objects
  let fetchScheduledTasksSpy: ReturnType<typeof vi.spyOn>
  let fetchScheduledTaskSpy: ReturnType<typeof vi.spyOn>
  let createScheduledTaskSpy: ReturnType<typeof vi.spyOn>
  let updateScheduledTaskSpy: ReturnType<typeof vi.spyOn>
  let deleteScheduledTaskSpy: ReturnType<typeof vi.spyOn>
  let pauseScheduledTaskSpy: ReturnType<typeof vi.spyOn>
  let resumeScheduledTaskSpy: ReturnType<typeof vi.spyOn>
  let fetchExecutionsSpy: ReturnType<typeof vi.spyOn>
  let triggerTaskSpy: ReturnType<typeof vi.spyOn>
  let getTaskTemplatesSpy: ReturnType<typeof vi.spyOn>

  // Helper function to set store state
  function setStoreState(
    store: ReturnType<typeof useScheduledTaskStore>,
    state: Partial<{
      tasks: ScheduledTaskDto[]
    }>,
  ) {
    store.$patch(state)
  }

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()

    // Setup spies
    fetchScheduledTasksSpy = vi.spyOn(taskApi, 'fetchScheduledTasks')
    fetchScheduledTaskSpy = vi.spyOn(taskApi, 'fetchScheduledTask')
    createScheduledTaskSpy = vi.spyOn(taskApi, 'createScheduledTask')
    updateScheduledTaskSpy = vi.spyOn(taskApi, 'updateScheduledTask')
    deleteScheduledTaskSpy = vi.spyOn(taskApi, 'deleteScheduledTask')
    pauseScheduledTaskSpy = vi.spyOn(taskApi, 'pauseScheduledTask')
    resumeScheduledTaskSpy = vi.spyOn(taskApi, 'resumeScheduledTask')
    fetchExecutionsSpy = vi.spyOn(taskApi, 'fetchExecutions')
    triggerTaskSpy = vi.spyOn(taskApi, 'triggerTask')
    getTaskTemplatesSpy = vi.spyOn(taskApi, 'getTaskTemplates')
  })

  describe('初始状态', () => {
    it('should have correct initial state', () => {
      const store = useScheduledTaskStore()

      expect(store.tasks).toEqual([])
      expect(store.currentTask).toBe(null)
      expect(store.executions).toEqual([])
      expect(store.templates).toEqual([])
      expect(store.loading).toBe(false)
      expect(store.saving).toBe(false)
      expect(store.triggering).toBe(false)
      expect(store.error).toBe(null)
      expect(store.totalCount).toBe(0)
      expect(store.currentPage).toBe(1)
    })
  })

  describe('loadTasks', () => {
    it('should load tasks successfully', async () => {
      const store = useScheduledTaskStore()
      const pagedResult: PagedResult<ScheduledTaskDto> = {
        items: mockTasks,
        totalCount: 2,
        page: 1,
        pageSize: 20,
      }
      fetchScheduledTasksSpy.mockResolvedValue(pagedResult)

      await store.loadTasks()

      expect(store.tasks).toEqual(mockTasks)
      expect(store.totalCount).toBe(2)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
    })

    it('should set error on load failure', async () => {
      const store = useScheduledTaskStore()
      fetchScheduledTasksSpy.mockRejectedValue(new Error('加载定时任务失败'))

      await store.loadTasks()

      expect(store.error).toBe('加载定时任务失败')
      expect(store.loading).toBe(false)
    })
  })

  describe('loadTask', () => {
    it('should load single task successfully', async () => {
      const store = useScheduledTaskStore()
      fetchScheduledTaskSpy.mockResolvedValue(firstTask)
      await store.loadTask('task-1')

      expect(store.currentTask).toEqual(firstTask)
      expect(store.loading).toBe(false)
    })
  })

  describe('loadTemplates', () => {
    it('should load templates successfully', async () => {
      const store = useScheduledTaskStore()
      getTaskTemplatesSpy.mockResolvedValue(mockTemplates)

      await store.loadTemplates()

      expect(store.templates).toEqual(mockTemplates)
    })
  })

  describe('submitCreate', () => {
    it('should create task successfully', async () => {
      const store = useScheduledTaskStore()
      const newTask: ScheduledTaskDto = {
        ...firstTask,
        id: 'task-new',
        name: '新任务',
      }
      createScheduledTaskSpy.mockResolvedValue(newTask)

      const result = await store.submitCreate({
        name: '新任务',
        cronExpression: '0 0 2 * * ?',
        triggerType: 'cron',
        input: '{"action":"backup"}',
        taskTitle: '新任务',
      })

      expect(result).toEqual(newTask)
      expect(store.tasks[0]).toEqual(newTask)
      expect(store.totalCount).toBe(1)
    })
  })

  describe('submitUpdate', () => {
    it('should update task successfully', async () => {
      const store = useScheduledTaskStore()
      setStoreState(store, { tasks: [...mockTasks] })
      const updated: ScheduledTaskDto = {
        ...firstTask,
        name: '更新后的名称',
      }
      updateScheduledTaskSpy.mockResolvedValue(updated)

      await store.submitUpdate('task-1', { name: '更新后的名称' })

      expect(store.tasks[0]!.name).toBe('更新后的名称')
    })
  })

  describe('submitDelete', () => {
    it('should delete task successfully', async () => {
      const store = useScheduledTaskStore()
      setStoreState(store, { tasks: [...mockTasks] })
      deleteScheduledTaskSpy.mockResolvedValue(true)

      await store.submitDelete('task-1')

      expect(store.tasks.find((t) => t.id === 'task-1')).toBeUndefined()
    })
  })

  describe('submitPause', () => {
    it('should pause task successfully', async () => {
      const store = useScheduledTaskStore()
      setStoreState(store, { tasks: [...mockTasks] })
      const pausedTask: ScheduledTaskDto = { ...firstTask, status: 'disabled' }
      pauseScheduledTaskSpy.mockResolvedValue(pausedTask)

      await store.submitPause('task-1')

      expect(store.tasks[0]!.status).toBe('disabled')
    })
  })

  describe('submitResume', () => {
    it('should resume task successfully', async () => {
      const store = useScheduledTaskStore()
      setStoreState(store, { tasks: [...mockTasks] })
      const resumedTask: ScheduledTaskDto = { ...secondTask, status: 'pending' }
      resumeScheduledTaskSpy.mockResolvedValue(resumedTask)

      await store.submitResume('task-2')

      expect(store.tasks[1]!.status).toBe('pending')
    })
  })

  describe('submitTrigger', () => {
    it('should trigger task successfully', async () => {
      const store = useScheduledTaskStore()
      triggerTaskSpy.mockResolvedValue(undefined)

      await store.submitTrigger('task-1')

      expect(store.triggering).toBe(false)
    })
  })

  describe('loadExecutions', () => {
    it('should load executions successfully', async () => {
      const store = useScheduledTaskStore()
      const pagedResult: PagedResult<ScheduledTaskExecutionDto> = {
        items: mockExecutions,
        totalCount: 1,
        page: 1,
        pageSize: 20,
      }
      fetchExecutionsSpy.mockResolvedValue(pagedResult)

      await store.loadExecutions('task-1')

      expect(store.executions).toEqual(mockExecutions)
    })
  })
})
