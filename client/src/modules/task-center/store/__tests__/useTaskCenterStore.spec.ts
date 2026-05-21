/**
 * useTaskCenterStore 单元测试
 */
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useTaskCenterStore } from '../useTaskCenterStore'
import * as taskApi from '../../task-center.api'
import * as mockModule from '../../../../mock'
import type {
  TaskSummaryDto,
  TaskDetailDto,
  TaskLogDto,
  CreateTaskRequestDto,
} from '../../task-center.types'
import type { ChatMessageDto, TaskDiffDto } from '../../../../mock'

describe('useTaskCenterStore', () => {
  // 测试数据
  const mockTaskSummaries: TaskSummaryDto[] = [
    { id: 'T-1009', title: '订单导出异常修复', state: '运行中', createdAt: '2024-05-19 14:30' },
    { id: 'T-1008', title: '会员模块接口重构', state: '已完成', createdAt: '2024-05-18 10:15' },
  ]

  const mockTaskDetail: TaskDetailDto = {
    id: 'T-1009',
    title: '订单导出异常修复',
    state: '运行中',
    steps: [
      { id: 'step-1', title: '拉取代码' },
      { id: 'step-2', title: '修改代码' },
    ],
  }

  const mockLogs: TaskLogDto[] = [
    { id: 'log-1', taskId: 'T-1009', message: '[14:31:22] clone repository success' },
    { id: 'log-2', taskId: 'T-1009', message: '[14:31:24] apply patch to OrderService' },
  ]

  const mockChat: ChatMessageDto[] = [
    { id: 'chat-1', taskId: 'T-1009', role: 'user', content: '请帮我修复订单导出空指针', timestamp: '14:30' },
    { id: 'chat-2', taskId: 'T-1009', role: 'ai', content: '已分析问题', timestamp: '14:31' },
  ]

  const mockDiff: TaskDiffDto = {
    taskId: 'T-1009',
    fileName: 'OrderService.cs',
    oldCode: 'return exportOrder(data)',
    newCode: 'return exportOrder(data ?? [])',
  }

  // Spy objects
  let fetchTaskSummariesSpy: ReturnType<typeof vi.spyOn>
  let fetchTaskDetailSpy: ReturnType<typeof vi.spyOn>
  let fetchTaskLogsSpy: ReturnType<typeof vi.spyOn>
  let createTaskSpy: ReturnType<typeof vi.spyOn>
  let getTaskChatSpy: ReturnType<typeof vi.spyOn>
  let getTaskDiffSpy: ReturnType<typeof vi.spyOn>
  let sendTaskChatMessageSpy: ReturnType<typeof vi.spyOn>
  let pauseTaskSpy: ReturnType<typeof vi.spyOn>
  let resumeTaskSpy: ReturnType<typeof vi.spyOn>

  // Helper function to set store state
  function setStoreState(
    store: ReturnType<typeof useTaskCenterStore>,
    state: Partial<{
      tasks: TaskSummaryDto[]
      selectedTask: TaskDetailDto | null
      logs: TaskLogDto[]
      chat: ChatMessageDto[]
      diff: TaskDiffDto | null
    }>,
  ) {
    store.$patch(state)
  }

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()

    // Setup spies
    fetchTaskSummariesSpy = vi.spyOn(taskApi, 'fetchTaskSummaries')
    fetchTaskDetailSpy = vi.spyOn(taskApi, 'fetchTaskDetail')
    fetchTaskLogsSpy = vi.spyOn(taskApi, 'fetchTaskLogs')
    createTaskSpy = vi.spyOn(taskApi, 'createTask')
    getTaskChatSpy = vi.spyOn(mockModule, 'getTaskChat')
    getTaskDiffSpy = vi.spyOn(mockModule, 'getTaskDiff')
    sendTaskChatMessageSpy = vi.spyOn(mockModule, 'sendTaskChatMessage')
    pauseTaskSpy = vi.spyOn(taskApi, 'pauseTask')
    resumeTaskSpy = vi.spyOn(taskApi, 'resumeTask')
  })

  describe('初始状态', () => {
    it('should have correct initial state', () => {
      const store = useTaskCenterStore()

      expect(store.tasks).toEqual([])
      expect(store.selectedTask).toBe(null)
      expect(store.logs).toEqual([])
      expect(store.chat).toEqual([])
      expect(store.diff).toBe(null)
      expect(store.loading).toBe(false)
      expect(store.detailLoading).toBe(false)
      expect(store.creating).toBe(false)
      expect(store.chatSending).toBe(false)
      expect(store.error).toBe(null)
    })

    it('should have correct computed properties', () => {
      const store = useTaskCenterStore()
      expect(store.hasTasks).toBe(false)
      expect(store.hasDetail).toBe(false)
    })
  })

  describe('loadTasks', () => {
    it('should load tasks successfully', async () => {
      const store = useTaskCenterStore()
      fetchTaskSummariesSpy.mockResolvedValue(mockTaskSummaries)

      await store.loadTasks()

      expect(store.tasks).toEqual(mockTaskSummaries)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
      expect(store.hasTasks).toBe(true)
    })

    it('should set error on load failure', async () => {
      const store = useTaskCenterStore()
      fetchTaskSummariesSpy.mockRejectedValue(new Error('加载任务列表失败'))

      await store.loadTasks()

      expect(store.error).toBe('加载任务列表失败')
      expect(store.loading).toBe(false)
    })
  })

  describe('loadTaskDetail', () => {
    it('should load task detail with all related data', async () => {
      const store = useTaskCenterStore()
      fetchTaskDetailSpy.mockResolvedValue(mockTaskDetail)
      fetchTaskLogsSpy.mockResolvedValue(mockLogs)
      getTaskChatSpy.mockResolvedValue(mockChat)
      getTaskDiffSpy.mockResolvedValue(mockDiff)

      await store.loadTaskDetail('T-1009')

      expect(store.selectedTask).toEqual(mockTaskDetail)
      expect(store.logs).toEqual(mockLogs)
      expect(store.chat).toEqual(mockChat)
      expect(store.diff).toEqual(mockDiff)
      expect(store.detailLoading).toBe(false)
    })

    it('should set error on load failure', async () => {
      const store = useTaskCenterStore()
      fetchTaskDetailSpy.mockRejectedValue(new Error('加载任务详情失败'))

      await store.loadTaskDetail('T-1009')

      expect(store.selectedTask).toBe(null)
      expect(store.logs).toEqual([])
      expect(store.chat).toEqual([])
      expect(store.diff).toBe(null)
      expect(store.error).toBe('加载任务详情失败')
    })
  })

  describe('submitTask', () => {
    it('should create task successfully', async () => {
      const store = useTaskCenterStore()
      const newTask: TaskSummaryDto = {
        id: 'T-1010',
        title: '新任务',
        state: '运行中',
        createdAt: '2026-05-21 10:00:00',
      }
      createTaskSpy.mockResolvedValue(newTask)

      const result = await store.submitTask({
        title: '新任务',
        description: '描述',
        repository: 'repo-1',
        branch: 'main',
      })

      expect(result).toEqual(newTask)
      expect(store.tasks[0]).toEqual(newTask)
      expect(store.creating).toBe(false)
    })
  })

  describe('sendTaskChat', () => {
    it('should send chat message successfully', async () => {
      const store = useTaskCenterStore()
      setStoreState(store, { chat: [...mockChat] })
      const updatedChat = [
        ...mockChat,
        { id: 'chat-3', taskId: 'T-1009', role: 'user', content: '新消息', timestamp: '14:35' },
        { id: 'chat-4', taskId: 'T-1009', role: 'ai', content: '回复', timestamp: '14:36' },
      ]
      sendTaskChatMessageSpy.mockResolvedValue(updatedChat)

      await store.sendTaskChat('T-1009', '新消息')

      expect(store.chat).toEqual(updatedChat)
      expect(store.chatSending).toBe(false)
    })

    it('should not send empty message', async () => {
      const store = useTaskCenterStore()

      await store.sendTaskChat('T-1009', '   ')

      expect(sendTaskChatMessageSpy).not.toHaveBeenCalled()
    })
  })

  describe('submitPause', () => {
    it('should pause task successfully', async () => {
      const store = useTaskCenterStore()
      setStoreState(store, { tasks: [...mockTaskSummaries] })
      const pausedTask = { ...mockTaskSummaries[0], state: '已暂停' as const }
      pauseTaskSpy.mockResolvedValue(pausedTask)

      await store.submitPause('T-1009')

      expect(store.tasks[0].state).toBe('已暂停')
    })
  })

  describe('submitResume', () => {
    it('should resume task successfully', async () => {
      const store = useTaskCenterStore()
      const pausedTasks: TaskSummaryDto[] = [
        { ...mockTaskSummaries[0], state: '已暂停' as const },
      ]
      setStoreState(store, { tasks: pausedTasks })
      const resumedTask = { ...pausedTasks[0], state: '运行中' as const }
      resumeTaskSpy.mockResolvedValue(resumedTask)

      await store.submitResume('T-1009')

      expect(store.tasks[0].state).toBe('运行中')
    })
  })
})
