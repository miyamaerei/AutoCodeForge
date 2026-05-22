/**
 * Chat API 模块
 * 支持真实 API 调用和 Mock 数据切换
 */

import { request } from '@/lib/request'
import { USE_MOCK } from '@/config/runtime'
import type {
  PagedResult,
  ChatSessionResponse,
  ChatMessageResponse,
  CreateSessionRequest,
  SendMessageRequest,
  SendMessageResponse,
  ChatMessage,
  SessionRecord,
} from './chat.types'

interface StreamEventHandlers {
  onToken?: (token: string) => void
  onDone?: (result: SendMessageResponse) => void
}

// 重新导出类型
export type {
  PagedResult,
  ChatSessionResponse,
  ChatMessageResponse,
  CreateSessionRequest,
  SendMessageRequest,
  SendMessageResponse,
  ChatMessage,
  SessionRecord,
}

// ==================== Mock 数据 ====================

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

let mockSessionList: SessionRecord[] = sessionSeeds.map((seed) => ({
  id: seed.id,
  title: seed.title,
  preview: seed.messages[0]?.content ?? '',
  timestamp: seed.timestamp,
  messagesCount: seed.messages.length,
}))

const mockSessionMessagesMap: Record<string, ChatMessage[]> = sessionSeeds.reduce<Record<string, ChatMessage[]>>((acc, seed) => {
  acc[seed.id] = [...seed.messages]
  return acc
}, {})

function nowTime(): string {
  return new Date().toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
}

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => window.setTimeout(resolve, ms))
}

// ==================== 类型转换函数 ====================

/** 后端 ChatSessionResponse 转前端 SessionRecord */
function sessionResponseToRecord(res: ChatSessionResponse): SessionRecord {
  const dateStr = res.lastMessageAtUtc ?? res.createdAtUtc
  return {
    id: res.id,
    title: res.title,
    preview: '...',
    timestamp: dateStr,
    messagesCount: 0,
  }
}

/** 后端 ChatMessageResponse 转前端 ChatMessage */
function messageResponseToMessage(res: ChatMessageResponse): ChatMessage {
  const type: 'user' | 'ai' = res.type === 'user' ? 'user' : 'ai'
  return {
    id: res.id,
    type,
    content: res.content,
    timestamp: res.createdAtUtc,
  }
}

// ==================== API 函数 ====================

/** 获取会话分页列表 */
export async function fetchSessions(page = 1, pageSize = 20): Promise<PagedResult<SessionRecord>> {
  if (USE_MOCK) {
    await wait(200)
    return {
      items: [...mockSessionList],
      totalCount: mockSessionList.length,
      page: 1,
      pageSize,
    }
  }
  const { data } = await request.get<{
    data: {
      items: ChatSessionResponse[]
      totalCount: number
      page: number
      pageSize: number
    }
  }>('/v1/chat/sessions', { params: { page, pageSize } })
  return {
    items: data.data.items.map(sessionResponseToRecord),
    totalCount: data.data.totalCount,
    page: data.data.page,
    pageSize: data.data.pageSize,
  }
}

/** 获取所有会话列表 */
export async function getSessions(): Promise<SessionRecord[]> {
  if (USE_MOCK) {
    await wait(200)
    return [...mockSessionList]
  }
  const result = await fetchSessions(1, 20)
  return result.items
}

/** 创建新会话 */
export async function createSession(payload: CreateSessionRequest): Promise<SessionRecord> {
  if (USE_MOCK) {
    await wait(300)
    const sessionId = `s-${Date.now()}`
    const session: SessionRecord = {
      id: sessionId,
      title: payload.title || `新会话 ${mockSessionList.length + 1}`,
      preview: '新建会话，等待第一条消息...',
      timestamp: new Date().toLocaleString('zh-CN', { hour12: false }),
      messagesCount: 0,
    }
    mockSessionList.unshift(session)
    mockSessionMessagesMap[sessionId] = []
    return session
  }
  const { data } = await request.post<{
    data: ChatSessionResponse
  }>('/v1/chat/sessions', payload)
  return sessionResponseToRecord(data.data)
}

/** 获取单个会话 */
export async function getSession(id: string): Promise<SessionRecord | null> {
  if (USE_MOCK) {
    await wait(100)
    return mockSessionList.find((s) => s.id === id) ?? null
  }
  try {
    const { data } = await request.get<{
      data: ChatSessionResponse
    }>(`/v1/chat/sessions/${id}`)
    return sessionResponseToRecord(data.data)
  } catch {
    return null
  }
}

/** 获取会话的消息列表 */
export async function getMessages(id: string): Promise<ChatMessage[]> {
  if (USE_MOCK) {
    await wait(150)
    return [...(mockSessionMessagesMap[id] ?? [])]
  }
  const { data } = await request.get<{
    data: ChatMessageResponse[]
  }>(`/v1/chat/sessions/${id}/messages`)
  return data.data.map(messageResponseToMessage)
}

/** 发送消息 */
export async function sendMessage(sessionId: string, msg: SendMessageRequest): Promise<SendMessageResponse> {
  if (USE_MOCK) {
    await wait(500)
    const userMessageId = `m-${Date.now()}`
    const assistantMessageId = `m-${Date.now() + 1}`
    const userMessage: ChatMessage = {
      id: userMessageId,
      type: 'user',
      content: msg.message,
      timestamp: nowTime(),
    }
    const assistantMessage: ChatMessage = {
      id: assistantMessageId,
      type: 'ai',
      content: '收到您的消息，正在处理中...',
      timestamp: nowTime(),
    }
    if (mockSessionMessagesMap[sessionId]) {
      mockSessionMessagesMap[sessionId].push(userMessage)
      mockSessionMessagesMap[sessionId].push(assistantMessage)
    }
    const session = mockSessionList.find((s) => s.id === sessionId)
    if (session) {
      session.preview = msg.message
      session.timestamp = nowTime()
      session.messagesCount += 2
    }
    return {
      sessionId,
      agentId: 'agent-1',
      userMessage: {
        id: userMessageId,
        chatSessionId: sessionId,
        type: 'user',
        content: msg.message,
        createdAtUtc: new Date().toISOString(),
      },
      assistantMessage: {
        id: assistantMessageId,
        chatSessionId: sessionId,
        type: 'ai',
        content: '收到您的消息，正在处理中...',
        createdAtUtc: new Date().toISOString(),
      },
    }
  }
  const { data } = await request.post<{
    data: SendMessageResponse
  }>(`/v1/chat/sessions/${sessionId}/messages`, msg)
  return data.data
}

/** 发送消息（流式返回） */
export async function sendMessageStream(
  sessionId: string,
  msg: SendMessageRequest,
  handlers: StreamEventHandlers = {},
): Promise<SendMessageResponse> {
  if (USE_MOCK) {
    const result = await sendMessage(sessionId, msg)
    handlers.onToken?.(result.assistantMessage.content)
    handlers.onDone?.(result)
    return result
  }

  const baseURL = request.defaults.baseURL ?? ''
  const token = localStorage.getItem('auth_token')
  const response = await fetch(`${baseURL}/v1/chat/sessions/${sessionId}/stream`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Accept: 'text/event-stream',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: JSON.stringify(msg),
  })

  if (!response.ok || !response.body) {
    throw new Error(`流式请求失败: ${response.status}`)
  }

  const decoder = new TextDecoder('utf-8')
  const reader = response.body.getReader()
  let buffer = ''
  let donePayload: SendMessageResponse | null = null

  const parseEventBlock = (block: string): void => {
    const lines = block.split('\n')
    let eventName = 'message'
    const dataLines: string[] = []

    for (const line of lines) {
      if (line.startsWith('event:')) {
        eventName = line.slice(6).trim()
      } else if (line.startsWith('data:')) {
        dataLines.push(line.slice(5).trim())
      }
    }

    const dataText = dataLines.join('\n')
    if (!dataText) {
      return
    }

    if (eventName === 'token') {
      handlers.onToken?.(dataText)
      return
    }

    if (eventName === 'error') {
      try {
        const payload = JSON.parse(dataText) as { message?: string }
        throw new Error(payload.message || '流式处理失败')
      } catch (err) {
        if (err instanceof Error) {
          throw err
        }
        throw new Error('流式处理失败')
      }
    }

    if (eventName === 'done') {
      const payload = JSON.parse(dataText) as SendMessageResponse
      donePayload = payload
      handlers.onDone?.(payload)
    }
  }

  while (true) {
    const { value, done } = await reader.read()
    if (done) {
      break
    }

    buffer += decoder.decode(value, { stream: true })
    let delimiterIndex = buffer.indexOf('\n\n')
    while (delimiterIndex !== -1) {
      const rawBlock = buffer.slice(0, delimiterIndex).replace(/\r/g, '').trim()
      buffer = buffer.slice(delimiterIndex + 2)
      if (rawBlock) {
        parseEventBlock(rawBlock)
      }
      delimiterIndex = buffer.indexOf('\n\n')
    }
  }

  if (!donePayload) {
    throw new Error('流式响应未返回完成结果')
  }

  return donePayload
}

/** 删除会话 */
export async function deleteSession(id: string): Promise<void> {
  if (USE_MOCK) {
    await wait(200)
    mockSessionList = mockSessionList.filter((s) => s.id !== id)
    delete mockSessionMessagesMap[id]
    return
  }
  await request.delete(`/v1/chat/sessions/${id}`)
}
