/**
 * Chat 模块类型定义
 */

/** 分页结果 */
export interface PagedResult<T> {
  /** 数据项 */
  items: T[]
  /** 总数 */
  totalCount: number
  /** 当前页 */
  page: number
  /** 每页大小 */
  pageSize: number
}

/** API 响应包装器 */
export interface ApiEnvelope<T> {
  success: boolean
  message: string
  data: T
  traceId?: string
}

/** 聊天会话响应 */
export interface ChatSessionResponse {
  id: string
  title: string
  agentId?: string | null
  lastMessageAtUtc?: string | null
  createdAtUtc: string
}

/** 聊天消息响应 */
export interface ChatMessageResponse {
  id: string
  chatSessionId: string
  type: string
  content: string
  modelName?: string | null
  createdAtUtc: string
}

/** 创建会话请求 */
export interface CreateSessionRequest {
  title: string
  agentId?: string | null
}

/** 发送消息请求 */
export interface SendMessageRequest {
  message: string
  agentId?: string | null
}

/** 发送消息响应 */
export interface SendMessageResponse {
  sessionId: string
  agentId?: string | null
  userMessage: ChatMessageResponse
  assistantMessage: ChatMessageResponse
}

/** 前端聊天消息（与现有 useConsoleChat 兼容） */
export interface ChatMessage {
  id: string
  type: 'user' | 'ai'
  content: string
  timestamp: string
}

/** 前端会话记录（与现有 useConsoleChat 兼容） */
export interface SessionRecord {
  id: string
  title: string
  preview: string
  timestamp: string
  messagesCount: number
}
