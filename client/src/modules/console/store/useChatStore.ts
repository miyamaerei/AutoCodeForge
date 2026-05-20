/**
 * Chat Store
 * 管理聊天会话和消息状态
 */

import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import {
  fetchSessions,
  getSessions,
  getSession,
  getMessages,
  sendMessage as sendMessageApi,
  createSession as createSessionApi,
  deleteSession as deleteSessionApi,
  type ChatMessage,
  type SessionRecord,
  type CreateSessionRequest,
  type SendMessageRequest,
} from '../api/chat.api'

export const useChatStore = defineStore('module.console-chat', () => {
  // ==================== 状态 ====================
  const sessionList = ref<SessionRecord[]>([])
  const selectedSessionId = ref<string | null>(null)
  const messagesMap = ref<Record<string, ChatMessage[]>>({})
  const loading = ref(false)
  const sending = ref(false)
  const error = ref<string | null>(null)

  // ==================== 计算属性 ====================
  const selectedSession = computed(() =>
    sessionList.value.find((session: SessionRecord) => session.id === selectedSessionId.value) ?? null,
  )

  const activeMessages = computed<ChatMessage[]>(() => {
    if (!selectedSessionId.value) {
      return []
    }
    return messagesMap.value[selectedSessionId.value] ?? []
  })

  const hasSessions = computed(() => sessionList.value.length > 0)

  // ==================== 操作 ====================

  /** 加载会话列表 */
  async function loadSessions(page = 1, pageSize = 20): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const result = await fetchSessions(page, pageSize)
      sessionList.value = result.items
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载会话列表失败'
    } finally {
      loading.value = false
    }
  }

  /** 加载会话消息 */
  async function loadMessages(sessionId: string): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const messages = await getMessages(sessionId)
      messagesMap.value[sessionId] = messages
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载消息失败'
    } finally {
      loading.value = false
    }
  }

  /** 选择会话 */
  function selectSession(id: string): void {
    selectedSessionId.value = id
    error.value = null
    if (!messagesMap.value[id]) {
      loadMessages(id).catch(() => {})
    }
  }

  /** 创建会话 */
  async function createSession(payload: CreateSessionRequest): Promise<SessionRecord> {
    loading.value = true
    error.value = null
    try {
      const session = await createSessionApi(payload)
      sessionList.value.unshift(session)
      messagesMap.value[session.id] = []
      selectedSessionId.value = session.id
      return session
    } catch (err) {
      error.value = err instanceof Error ? err.message : '创建会话失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** 发送消息 */
  async function sendMessage(sessionId: string, msg: SendMessageRequest): Promise<void> {
    sending.value = true
    error.value = null
    try {
      const result = await sendMessageApi(sessionId, msg)
      const userMsg = {
        id: result.userMessage.id,
        type: 'user' as const,
        content: result.userMessage.content,
        timestamp: result.userMessage.createdAtUtc,
      }
      const aiMsg = {
        id: result.assistantMessage.id,
        type: 'ai' as const,
        content: result.assistantMessage.content,
        timestamp: result.assistantMessage.createdAtUtc,
      }
      if (!messagesMap.value[sessionId]) {
        messagesMap.value[sessionId] = []
      }
      messagesMap.value[sessionId].push(userMsg)
      messagesMap.value[sessionId].push(aiMsg)
      const session = sessionList.value.find((s: SessionRecord) => s.id === sessionId)
      if (session) {
        session.preview = msg.message
        session.timestamp = new Date().toLocaleString('zh-CN', { hour12: false })
        session.messagesCount += 2
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '发送消息失败'
      throw err
    } finally {
      sending.value = false
    }
  }

  /** 删除会话 */
  async function deleteSession(id: string): Promise<void> {
    loading.value = true
    error.value = null
    try {
      await deleteSessionApi(id)
      sessionList.value = sessionList.value.filter((s: SessionRecord) => s.id !== id)
      delete messagesMap.value[id]
      if (selectedSessionId.value === id) {
        selectedSessionId.value = null
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除会话失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  /** 清除选中状态 */
  function clearSelection(): void {
    selectedSessionId.value = null
  }

  /** 重置状态 */
  function reset(): void {
    sessionList.value = []
    selectedSessionId.value = null
    messagesMap.value = {}
    loading.value = false
    sending.value = false
    error.value = null
  }

  return {
    // 状态
    sessionList,
    selectedSessionId,
    messagesMap,
    loading,
    sending,
    error,
    // 计算属性
    selectedSession,
    activeMessages,
    hasSessions,
    // 操作
    loadSessions,
    loadMessages,
    selectSession,
    createSession,
    sendMessage,
    deleteSession,
    clearSelection,
    reset,
  }
})
