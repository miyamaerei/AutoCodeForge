/**
 * useChatStore 单元测试
 */
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useChatStore } from '../useChatStore'
import * as chatApi from '../../api/chat.api'
import type { ChatMessage, SessionRecord } from '../../api/chat.api'

describe('useChatStore', () => {
  // 测试数据
  const mockSessions: SessionRecord[] = [
    {
      id: 's1',
      title: '订单导出功能修复',
      preview: '帮我修复订单导出的空指针异常。',
      timestamp: '2026-05-19 14:30',
      messagesCount: 2,
    },
    {
      id: 's2',
      title: '用户认证系统升级',
      preview: '需要升级现有认证系统，支持刷新 token。',
      timestamp: '2026-05-18 10:15',
      messagesCount: 2,
    },
  ]

  const mockMessages: ChatMessage[] = [
    {
      id: 'm1',
      type: 'user',
      content: '帮我修复订单导出的空指针异常。',
      timestamp: '14:30',
    },
    {
      id: 'm2',
      type: 'ai',
      content: '先定位导出服务里可能为 null 的订单字段，再加上空值防护和日志。',
      timestamp: '14:31',
    },
  ]

  // Spy objects
  let fetchSessionsSpy: ReturnType<typeof vi.spyOn>
  let getMessagesSpy: ReturnType<typeof vi.spyOn>
  let sendMessageSpy: ReturnType<typeof vi.spyOn>
  let createSessionSpy: ReturnType<typeof vi.spyOn>
  let deleteSessionSpy: ReturnType<typeof vi.spyOn>

  // Helper function to set store state
  function setStoreState(
    store: ReturnType<typeof useChatStore>,
    state: Partial<{
      sessionList: SessionRecord[]
      selectedSessionId: string | null
      messagesMap: Record<string, ChatMessage[]>
    }>,
  ) {
    store.$patch(state)
  }

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()

    // Setup spies for API functions
    fetchSessionsSpy = vi.spyOn(chatApi, 'fetchSessions')
    getMessagesSpy = vi.spyOn(chatApi, 'getMessages')
    sendMessageSpy = vi.spyOn(chatApi, 'sendMessage')
    createSessionSpy = vi.spyOn(chatApi, 'createSession')
    deleteSessionSpy = vi.spyOn(chatApi, 'deleteSession')
  })

  describe('初始状态', () => {
    it('should have correct initial state', () => {
      const store = useChatStore()

      expect(store.sessionList).toEqual([])
      expect(store.selectedSessionId).toBe(null)
      expect(store.messagesMap).toEqual({})
      expect(store.loading).toBe(false)
      expect(store.sending).toBe(false)
      expect(store.error).toBe(null)
    })
  })

  describe('computed properties', () => {
    it('should return null when no session selected', () => {
      const store = useChatStore()
      expect(store.selectedSession).toBe(null)
    })

    it('should return selected session when selected', () => {
      const store = useChatStore()
      setStoreState(store, { sessionList: mockSessions, selectedSessionId: 's1' })

      expect(store.selectedSession).toEqual(mockSessions[0])
    })

    it('should return empty messages when no session selected', () => {
      const store = useChatStore()
      expect(store.activeMessages).toEqual([])
    })

    it('should return messages for selected session', () => {
      const store = useChatStore()
      setStoreState(store, { messagesMap: { s1: mockMessages }, selectedSessionId: 's1' })

      expect(store.activeMessages).toEqual(mockMessages)
    })

    it('should return hasSessions correctly', () => {
      const store = useChatStore()
      expect(store.hasSessions).toBe(false)

      setStoreState(store, { sessionList: mockSessions })
      expect(store.hasSessions).toBe(true)
    })
  })

  describe('loadSessions', () => {
    it('should load sessions successfully', async () => {
      const store = useChatStore()
      fetchSessionsSpy.mockResolvedValue({
        items: mockSessions,
        totalCount: 2,
        page: 1,
        pageSize: 20,
      })

      await store.loadSessions()

      expect(store.sessionList).toEqual(mockSessions)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
    })

    it('should set error on load failure', async () => {
      const store = useChatStore()
      fetchSessionsSpy.mockRejectedValue(new Error('网络错误'))

      await store.loadSessions()

      expect(store.error).toBe('网络错误')
      expect(store.loading).toBe(false)
    })

    it('should use pagination parameters', async () => {
      const store = useChatStore()
      fetchSessionsSpy.mockResolvedValue({ items: [], totalCount: 0, page: 2, pageSize: 10 })

      await store.loadSessions(2, 10)

      expect(fetchSessionsSpy).toHaveBeenCalledWith(2, 10)
    })
  })

  describe('loadMessages', () => {
    it('should load messages successfully', async () => {
      const store = useChatStore()
      getMessagesSpy.mockResolvedValue(mockMessages)

      await store.loadMessages('s1')

      expect(store.messagesMap['s1']).toEqual(mockMessages)
      expect(store.loading).toBe(false)
    })

    it('should set error on load failure', async () => {
      const store = useChatStore()
      getMessagesSpy.mockRejectedValue(new Error('加载消息失败'))

      await store.loadMessages('s1')

      expect(store.error).toBe('加载消息失败')
      expect(store.loading).toBe(false)
    })
  })

  describe('selectSession', () => {
    it('should select session and load messages', async () => {
      const store = useChatStore()
      getMessagesSpy.mockResolvedValue(mockMessages)

      store.selectSession('s1')

      expect(store.selectedSessionId).toBe('s1')
      // Should trigger loadMessages
      await vi.waitFor(() => {
        expect(getMessagesSpy).toHaveBeenCalledWith('s1')
      })
    })

    it('should clear error on select', () => {
      const store = useChatStore()
      store.$patch({ error: 'previous error' })

      store.selectSession('s1')

      expect(store.error).toBe(null)
    })
  })

  describe('createSession', () => {
    it('should create session successfully', async () => {
      const store = useChatStore()
      const newSession: SessionRecord = {
        id: 's-new',
        title: '新会话',
        preview: '',
        timestamp: '2026-05-21 10:00',
        messagesCount: 0,
      }
      createSessionSpy.mockResolvedValue(newSession)

      const result = await store.createSession({ title: '新会话' })

      expect(result).toEqual(newSession)
      expect(store.sessionList[0]).toEqual(newSession)
      expect(store.selectedSessionId).toBe('s-new')
      expect(store.messagesMap['s-new']).toEqual([])
      expect(store.loading).toBe(false)
    })

    it('should set error on create failure', async () => {
      const store = useChatStore()
      createSessionSpy.mockRejectedValue(new Error('创建会话失败'))

      await expect(store.createSession({ title: '新会话' })).rejects.toThrow()
      expect(store.error).toBe('创建会话失败')
    })
  })

  describe('sendMessage', () => {
    it('should send message successfully', async () => {
      const store = useChatStore()
      setStoreState(store, {
        sessionList: mockSessions,
        selectedSessionId: 's1',
        messagesMap: { s1: [] },
      })

      const sendResult = {
        sessionId: 's1',
        agentId: 'agent-1',
        userMessage: {
          id: 'm-new-1',
          chatSessionId: 's1',
          type: 'user',
          content: '新消息',
          createdAtUtc: '10:00',
        },
        assistantMessage: {
          id: 'm-new-2',
          chatSessionId: 's1',
          type: 'ai',
          content: 'AI 回复',
          createdAtUtc: '10:01',
        },
      }
      sendMessageSpy.mockResolvedValue(sendResult)

      await store.sendMessage('s1', { message: '新消息' })

      const messages = store.messagesMap['s1'] ?? []
      expect(messages).toHaveLength(2)
      expect(messages[0]!.type).toBe('user')
      expect(messages[1]!.type).toBe('ai')
      expect(store.sending).toBe(false)
    })

    it('should update session preview after send', async () => {
      const store = useChatStore()
      // Create fresh session with 0 messagesCount
      const sessionCopy = { id: 's1', title: '新会话', preview: '', timestamp: '2026-05-21 10:00', messagesCount: 0 }
      setStoreState(store, {
        sessionList: [sessionCopy],
        selectedSessionId: 's1',
        messagesMap: { s1: [] },
      })

      sendMessageSpy.mockResolvedValue({
        sessionId: 's1',
        agentId: 'agent-1',
        userMessage: {
          id: 'm-new-1',
          chatSessionId: 's1',
          type: 'user',
          content: '新消息内容',
          createdAtUtc: '10:00',
        },
        assistantMessage: {
          id: 'm-new-2',
          chatSessionId: 's1',
          type: 'ai',
          content: '回复',
          createdAtUtc: '10:01',
        },
      })

      await store.sendMessage('s1', { message: '新消息内容' })

      expect(store.sessionList[0]!.preview).toBe('新消息内容')
      expect(store.sessionList[0]!.messagesCount).toBe(2)
    })
  })

  describe('deleteSession', () => {
    it('should delete session successfully', async () => {
      const store = useChatStore()
      setStoreState(store, {
        sessionList: [...mockSessions],
        messagesMap: { s1: mockMessages },
        selectedSessionId: 's1',
      })
      deleteSessionSpy.mockResolvedValue(undefined)

      await store.deleteSession('s1')

      expect(store.sessionList.find((s) => s.id === 's1')).toBeUndefined()
      expect(store.messagesMap['s1']).toBeUndefined()
      expect(store.selectedSessionId).toBe(null)
    })

    it('should clear selectedSessionId only if deleted session was selected', async () => {
      const store = useChatStore()
      setStoreState(store, {
        sessionList: [...mockSessions],
        selectedSessionId: 's1',
      })
      deleteSessionSpy.mockResolvedValue(undefined)

      await store.deleteSession('s2')

      expect(store.selectedSessionId).toBe('s1')
    })

    it('should set error on delete failure', async () => {
      const store = useChatStore()
      setStoreState(store, { sessionList: [...mockSessions] })
      deleteSessionSpy.mockRejectedValue(new Error('删除失败'))

      try {
        await store.deleteSession('s1')
      } catch (e) {
        // Expected - store throws after setting error
      }

      expect(store.error).toBe('删除失败')
    })
  })
})
