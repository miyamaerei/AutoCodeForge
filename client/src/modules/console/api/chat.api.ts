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
  const { data } = await request.get<{
    data: {
      items: ChatSessionResponse[]
      totalCount: number
      page: number
      pageSize: number
    }
  }>('/v1/chat/sessions')
  return data.data.items.map(sessionResponseToRecord)
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
