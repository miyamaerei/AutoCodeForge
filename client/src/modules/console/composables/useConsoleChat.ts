import { computed, ref, type Ref } from 'vue'
import { storeToRefs } from 'pinia'
import { useChatStore } from '../store/useChatStore'
import type { ChatMessage, SessionRecord } from '../api/chat.types'

export type ChatMode = 'ask' | 'session'

const suggestedQuestions = [
  '如何优化数据库查询性能？',
  '什么是最佳的异常处理方式？',
  '如何设计一个可扩展的 API 架构？',
  '前端性能优化有哪些方法？',
]

function nowTime(): string {
  return new Date().toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
}

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => {
    window.setTimeout(resolve, ms)
  })
}

export function useConsoleChat(mode: Ref<ChatMode>) {
  const chatStore = useChatStore()
  const {
    sessionList,
    selectedSession,
    activeMessages: sessionMessages,
    hasSessions,
    loading,
    sending: storeSending,
    error: storeError,
  } = storeToRefs(chatStore)

  const initializing = ref(false)
  const sending = ref(false)
  const error = ref<string | null>(null)
  const inputMessage = ref('')

  const askMessages = ref<ChatMessage[]>([
    {
      id: 'ask-greeting',
      type: 'ai',
      content: '你好，我是统一 Chat 助手。你可以直接提问，或切换到会话模式继续历史对话。',
      timestamp: '14:00',
    },
  ])

  const activeMessages = computed<ChatMessage[]>(() => {
    if (mode.value === 'ask') {
      return askMessages.value
    }
    return sessionMessages.value
  })

  const isEmptyState = computed(() => {
    if (initializing.value || error.value || storeError.value) {
      return false
    }
    if (mode.value === 'session') {
      return !selectedSession.value
    }
    return activeMessages.value.length === 0
  })

  async function initialize(): Promise<void> {
    initializing.value = true
    error.value = null
    try {
      if (mode.value === 'session') {
        await chatStore.loadSessions()
        if (!chatStore.selectedSessionId && sessionList.value.length > 0) {
          const firstSession = sessionList.value[0]
          if (firstSession) {
            chatStore.selectSession(firstSession.id)
          }
        }
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '初始化聊天页面失败'
    } finally {
      initializing.value = false
    }
  }

  function selectSession(id: string): void {
    chatStore.selectSession(id)
    error.value = null
  }

  async function createSession(): Promise<void> {
    error.value = null
    try {
      await chatStore.createSession({ title: `新会话 ${sessionList.value.length + 1}` })
    } catch (err) {
      error.value = err instanceof Error ? err.message : '创建会话失败'
    }
  }

  async function deleteSession(id: string): Promise<void> {
    error.value = null
    try {
      await chatStore.deleteSession(id)
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除会话失败'
    }
  }

  async function sendMessage(): Promise<void> {
    const text = inputMessage.value.trim()
    if (!text) {
      return
    }

    error.value = null
    inputMessage.value = ''
    sending.value = true

    if (mode.value === 'ask') {
      // Ask 模式使用 mock
      const userMessage: ChatMessage = {
        id: `u-${Date.now()}`,
        type: 'user',
        content: text,
        timestamp: nowTime(),
      }
      askMessages.value.push(userMessage)

      try {
        await wait(500)
        const aiMessage: ChatMessage = {
          id: `a-${Date.now()}`,
          type: 'ai',
          content: `收到你的问题：${text}。这是统一 Chat 页面返回的建议草案，可按场景继续细化。`,
          timestamp: nowTime(),
        }
        askMessages.value.push(aiMessage)
      } catch (err) {
        error.value = err instanceof Error ? err.message : '发送消息失败'
      } finally {
        sending.value = false
      }
    } else if (chatStore.selectedSessionId) {
      // Session 模式使用 store
      try {
        await chatStore.sendMessage(chatStore.selectedSessionId, { message: text })
      } catch (err) {
        error.value = err instanceof Error ? err.message : '发送消息失败'
      } finally {
        sending.value = false
      }
    } else {
      sending.value = false
    }
  }

  function applySuggestedQuestion(question: string): void {
    inputMessage.value = question
  }

  return {
    initializing,
    sending: computed(() => sending.value || storeSending.value),
    error: computed(() => error.value || storeError.value),
    inputMessage,
    selectedSession,
    sessionList,
    suggestedQuestions,
    hasSessions,
    isEmptyState,
    activeMessages,
    initialize,
    createSession,
    selectSession,
    deleteSession,
    sendMessage,
    applySuggestedQuestion,
  }
}
