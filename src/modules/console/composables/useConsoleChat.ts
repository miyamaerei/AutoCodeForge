import { computed, ref, type Ref } from 'vue'

export type ChatMode = 'ask' | 'session'

type MessageType = 'user' | 'ai'

export interface ChatMessage {
  id: string
  type: MessageType
  content: string
  timestamp: string
}

interface SessionRecord {
  id: string
  title: string
  preview: string
  timestamp: string
  messagesCount: number
}

interface SessionSeed {
  id: string
  title: string
  timestamp: string
  messages: ChatMessage[]
}

const sessionSeeds: SessionSeed[] = [
  {
    id: 's1',
    title: '订单导出功能修复',
    timestamp: '2026-05-19 14:30',
    messages: [
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
    ],
  },
  {
    id: 's2',
    title: '用户认证系统升级',
    timestamp: '2026-05-18 10:15',
    messages: [
      {
        id: 'm3',
        type: 'user',
        content: '需要升级现有认证系统，支持刷新 token。',
        timestamp: '10:15',
      },
      {
        id: 'm4',
        type: 'ai',
        content: '建议引入短期 access token + 长期 refresh token，并在网关层做统一拦截。',
        timestamp: '10:16',
      },
    ],
  },
  {
    id: 's3',
    title: '数据库性能优化',
    timestamp: '2026-05-17 09:45',
    messages: [
      {
        id: 'm5',
        type: 'user',
        content: '查询性能有些慢，需要优化。',
        timestamp: '09:45',
      },
      {
        id: 'm6',
        type: 'ai',
        content: '先从慢查询日志入手，确认是否存在索引缺失或回表过多。',
        timestamp: '09:46',
      },
    ],
  },
]

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
  const initializing = ref(false)
  const sending = ref(false)
  const error = ref<string | null>(null)
  const inputMessage = ref('')
  const selectedSessionId = ref<string | null>(null)

  const askMessages = ref<ChatMessage[]>([
    {
      id: 'ask-greeting',
      type: 'ai',
      content: '你好，我是统一 Chat 助手。你可以直接提问，或切换到会话模式继续历史对话。',
      timestamp: '14:00',
    },
  ])

  const sessionList = ref<SessionRecord[]>(
    sessionSeeds.map((seed) => ({
      id: seed.id,
      title: seed.title,
      preview: seed.messages[0]?.content ?? '',
      timestamp: seed.timestamp,
      messagesCount: seed.messages.length,
    })),
  )

  const sessionMessagesMap = ref<Record<string, ChatMessage[]>>(
    sessionSeeds.reduce<Record<string, ChatMessage[]>>((acc, seed) => {
      acc[seed.id] = [...seed.messages]
      return acc
    }, {}),
  )

  const selectedSession = computed(() =>
    sessionList.value.find((session) => session.id === selectedSessionId.value) ?? null,
  )

  const activeMessages = computed<ChatMessage[]>(() => {
    if (mode.value === 'ask') {
      return askMessages.value
    }
    if (!selectedSessionId.value) {
      return []
    }
    return sessionMessagesMap.value[selectedSessionId.value] ?? []
  })

  const hasSessions = computed(() => sessionList.value.length > 0)
  const isEmptyState = computed(() => {
    if (initializing.value || error.value) {
      return false
    }
    if (mode.value === 'session') {
      return !selectedSessionId.value
    }
    return activeMessages.value.length === 0
  })

  async function initialize(): Promise<void> {
    initializing.value = true
    error.value = null
    try {
      await wait(120)
      if (mode.value === 'session' && !selectedSessionId.value && sessionList.value.length > 0) {
        const firstSession = sessionList.value[0]
        if (firstSession) {
          selectedSessionId.value = firstSession.id
        }
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '初始化聊天页面失败'
    } finally {
      initializing.value = false
    }
  }

  function selectSession(id: string): void {
    selectedSessionId.value = id
    error.value = null
  }

  function createSession(): void {
    const sessionId = `s-${Date.now()}`
    const session: SessionRecord = {
      id: sessionId,
      title: `新会话 ${sessionList.value.length + 1}`,
      preview: '新建会话，等待第一条消息...',
      timestamp: new Date().toLocaleString('zh-CN', { hour12: false }),
      messagesCount: 0,
    }
    sessionList.value.unshift(session)
    sessionMessagesMap.value[sessionId] = []
    selectedSessionId.value = sessionId
  }

  function deleteSession(id: string): void {
    sessionList.value = sessionList.value.filter((session) => session.id !== id)
    delete sessionMessagesMap.value[id]
    if (selectedSessionId.value === id) {
      selectedSessionId.value = sessionList.value[0]?.id ?? null
    }
  }

  async function sendMessage(): Promise<void> {
    const text = inputMessage.value.trim()
    if (!text) {
      return
    }

    error.value = null
    const userMessage: ChatMessage = {
      id: `u-${Date.now()}`,
      type: 'user',
      content: text,
      timestamp: nowTime(),
    }

    if (mode.value === 'ask') {
      askMessages.value.push(userMessage)
    } else if (selectedSessionId.value) {
      sessionMessagesMap.value[selectedSessionId.value] = [
        ...(sessionMessagesMap.value[selectedSessionId.value] ?? []),
        userMessage,
      ]
      sessionList.value = sessionList.value.map((session) => {
        if (session.id !== selectedSessionId.value) {
          return session
        }
        return {
          ...session,
          preview: text,
          messagesCount: (sessionMessagesMap.value[selectedSessionId.value] ?? []).length,
          timestamp: new Date().toLocaleString('zh-CN', { hour12: false }),
        }
      })
    }

    inputMessage.value = ''
    sending.value = true

    try {
      await wait(500)
      const aiMessage: ChatMessage = {
        id: `a-${Date.now()}`,
        type: 'ai',
        content: `收到你的问题：${text}。这是统一 Chat 页面返回的建议草案，可按场景继续细化。`,
        timestamp: nowTime(),
      }

      if (mode.value === 'ask') {
        askMessages.value.push(aiMessage)
      } else if (selectedSessionId.value) {
        sessionMessagesMap.value[selectedSessionId.value] = [
          ...(sessionMessagesMap.value[selectedSessionId.value] ?? []),
          aiMessage,
        ]
        sessionList.value = sessionList.value.map((session) => {
          if (session.id !== selectedSessionId.value) {
            return session
          }
          return {
            ...session,
            messagesCount: (sessionMessagesMap.value[selectedSessionId.value] ?? []).length,
          }
        })
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '发送消息失败'
    } finally {
      sending.value = false
    }
  }

  function applySuggestedQuestion(question: string): void {
    inputMessage.value = question
  }

  return {
    initializing,
    sending,
    error,
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
