/**
 * Agent API 模块 - 提供 Agent 的 CRUD 操作
 * 支持真实 API 调用和 Mock 数据切换
 */

import { request } from '@/lib/request'
import { USE_MOCK } from '@/config/runtime'
import type { PagedResult } from '@/modules/agent-center/api/agent.types'

// ==================== 类型定义 ====================

/** 关键词权重 */
export interface AgentKeyword {
  /** 关键词 */
  keyword: string
  /** 权重 */
  weight: number
}

/** Agent 响应 DTO（对应后端 AgentResponse） */
export interface AgentDto {
  /** Agent ID */
  id: string
  /** Agent 名称 */
  name: string
  /** Agent 描述 */
  description: string
  /** 头像/图标（仅前端使用，后端不支持） */
  icon: string
  /** 系统提示词 */
  systemPrompt: string
  /** 自动选择关键词 */
  keywords: AgentKeyword[]
  /** 是否启用 */
  enabled: boolean
  /** 创建时间 */
  createdAt: string
  /** 更新时间 */
  updatedAt: string
}

/** 创建 Agent 请求 */
export interface CreateAgentDto {
  name: string
  description: string
  icon: string
  systemPrompt: string
  keywords: AgentKeyword[]
  enabled: boolean
}

/** 更新 Agent 请求 */
export interface UpdateAgentDto extends Partial<CreateAgentDto> {
  id: string
}

// ==================== Mock 数据 ====================

const defaultAgents: AgentDto[] = [
  {
    id: 'agent-1',
    name: '代码审查助手',
    description: '专注于代码质量审查，发现潜在 Bug 和安全问题',
    icon: '🔍',
    systemPrompt: '你是一个专业的代码审查助手，负责分析代码质量、发现潜在问题并提供改进建议。',
    keywords: [
      { keyword: 'review', weight: 1.0 },
      { keyword: '代码审查', weight: 1.0 },
      { keyword: 'bug', weight: 0.8 },
      { keyword: '安全', weight: 0.7 },
      { keyword: '质量', weight: 0.6 },
    ],
    enabled: true,
    createdAt: '2026-05-01 10:00:00',
    updatedAt: '2026-05-15 14:30:00',
  },
  {
    id: 'agent-2',
    name: '架构设计专家',
    description: '提供系统架构设计和技术方案建议',
    icon: '🏗️',
    systemPrompt: '你是一个资深的系统架构师，擅长设计高可用、高性能的系统架构。',
    keywords: [
      { keyword: '架构', weight: 1.0 },
      { keyword: '设计', weight: 0.8 },
      { keyword: '高可用', weight: 0.9 },
      { keyword: '微服务', weight: 0.8 },
      { keyword: '系统设计', weight: 0.9 },
    ],
    enabled: true,
    createdAt: '2026-05-02 09:00:00',
    updatedAt: '2026-05-16 11:00:00',
  },
  {
    id: 'agent-3',
    name: '数据库专家',
    description: '解决数据库性能优化和 SQL 问题',
    icon: '🗄️',
    systemPrompt: '你是一个数据库专家，精通 SQL 优化、索引设计和数据库调优。',
    keywords: [
      { keyword: '数据库', weight: 1.0 },
      { keyword: 'sql', weight: 0.9 },
      { keyword: '索引', weight: 0.8 },
      { keyword: '查询优化', weight: 0.9 },
      { keyword: '慢查询', weight: 0.8 },
    ],
    enabled: true,
    createdAt: '2026-05-03 08:30:00',
    updatedAt: '2026-05-17 16:00:00',
  },
  {
    id: 'agent-4',
    name: '前端开发助手',
    description: '帮助开发 Vue/React 组件和页面',
    icon: '🎨',
    systemPrompt: '你是一个专业的前端开发者，精通 Vue 和 React 生态。',
    keywords: [
      { keyword: '前端', weight: 1.0 },
      { keyword: 'vue', weight: 0.9 },
      { keyword: 'react', weight: 0.9 },
      { keyword: '组件', weight: 0.7 },
      { keyword: '样式', weight: 0.6 },
    ],
    enabled: true,
    createdAt: '2026-05-04 14:00:00',
    updatedAt: '2026-05-18 10:00:00',
  },
  {
    id: 'agent-5',
    name: '文档撰写助手',
    description: '帮助撰写技术文档和 API 说明',
    icon: '📝',
    systemPrompt: '你是一个技术文档专家，擅长撰写清晰、准确的技术文档。',
    keywords: [
      { keyword: '文档', weight: 1.0 },
      { keyword: '说明', weight: 0.8 },
      { keyword: 'api', weight: 0.7 },
      { keyword: 'readme', weight: 0.8 },
      { keyword: '注释', weight: 0.6 },
    ],
    enabled: false,
    createdAt: '2026-05-05 11:00:00',
    updatedAt: '2026-05-19 09:00:00',
  },
]

// ==================== Mock 辅助函数 ====================

let agentsData: AgentDto[] = [...defaultAgents]

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => window.setTimeout(resolve, ms))
}

// ==================== 类型转换函数 ====================

/** 后端 AgentResponse 转前端 AgentDto */
function agentResponseToDto(res: {
  id: string
  name: string
  description?: string | null
  keywords?: string | null
  systemPrompt?: string | null
  isEnabled: boolean
  createdAtUtc: string
  updatedAtUtc: string
}): AgentDto {
  return {
    id: res.id,
    name: res.name,
    description: res.description ?? '',
    icon: '🤖', // 后端不支持，前端默认
    systemPrompt: res.systemPrompt ?? '',
    keywords: res.keywords
      ? res.keywords.split(',').map((k) => ({ keyword: k.trim(), weight: 1.0 }))
      : [],
    enabled: res.isEnabled,
    createdAt: res.createdAtUtc,
    updatedAt: res.updatedAtUtc,
  }
}

/** 前端 CreateAgentDto 转后端 CreateAgentRequest */
function dtoToCreateRequest(dto: CreateAgentDto): Record<string, unknown> {
  return {
    name: dto.name,
    description: dto.description,
    keywords: dto.keywords.map((k) => k.keyword).join(','),
    systemPrompt: dto.systemPrompt,
    isEnabled: dto.enabled,
  }
}

/** 前端 UpdateAgentDto 转后端 UpdateAgentRequest */
function dtoToUpdateRequest(dto: UpdateAgentDto): Record<string, unknown> {
  const req: Record<string, unknown> = {
    name: dto.name,
    isEnabled: dto.enabled,
  }
  if (dto.description !== undefined) req.description = dto.description
  if (dto.keywords !== undefined) req.keywords = dto.keywords.map((k) => k.keyword).join(',')
  if (dto.systemPrompt !== undefined) req.systemPrompt = dto.systemPrompt
  return req
}

// ==================== API 函数 ====================

/** 获取 Agent 分页列表 */
export async function fetchAgents(page = 1, pageSize = 20): Promise<PagedResult<AgentDto>> {
  if (USE_MOCK) {
    await wait(200)
    return {
      items: [...agentsData],
      totalCount: agentsData.length,
      page: 1,
      pageSize,
    }
  }
  const { data } = await request.get<{
    data: { items: Array<{
      id: string
      name: string
      description?: string | null
      keywords?: string | null
      systemPrompt?: string | null
      isEnabled: boolean
      createdAtUtc: string
      updatedAtUtc: string
    }>
    totalCount: number
    page: number
    pageSize: number }
  }>('/api/v1/agents', { params: { page, pageSize } })
  return {
    items: data.data.items.map(agentResponseToDto),
    totalCount: data.data.totalCount,
    page: data.data.page,
    pageSize: data.data.pageSize,
  }
}

/** 获取所有 Agent 列表 */
export async function getAgents(): Promise<AgentDto[]> {
  if (USE_MOCK) {
    await wait(200)
    return [...agentsData]
  }
  const { data } = await request.get<{
    data: {
      items: Array<{
        id: string
        name: string
        description?: string | null
        keywords?: string | null
        systemPrompt?: string | null
        isEnabled: boolean
        createdAtUtc: string
        updatedAtUtc: string
      }>
      totalCount: number
      page: number
      pageSize: number
    }
  }>('/api/v1/agents')
  return data.data.items.map(agentResponseToDto)
}

/** 根据 ID 获取单个 Agent */
export async function getAgent(id: string): Promise<AgentDto | null> {
  if (USE_MOCK) {
    await wait(100)
    return agentsData.find((a) => a.id === id) ?? null
  }
  try {
    const { data } = await request.get<{
      data: {
        id: string
        name: string
        description?: string | null
        keywords?: string | null
        systemPrompt?: string | null
        isEnabled: boolean
        createdAtUtc: string
        updatedAtUtc: string
      }
    }>(`/api/v1/agents/${id}`)
    return agentResponseToDto(data.data)
  } catch {
    return null
  }
}

/** 创建新 Agent */
export async function createAgent(dto: CreateAgentDto): Promise<AgentDto> {
  if (USE_MOCK) {
    await wait(300)
    const now = new Date().toLocaleString('zh-CN', { hour12: false })
    const agent: AgentDto = {
      id: `agent-${Date.now()}`,
      ...dto,
      createdAt: now,
      updatedAt: now,
    }
    agentsData.push(agent)
    return agent
  }
  const { data } = await request.post<{
    data: {
      id: string
      name: string
      description?: string | null
      keywords?: string | null
      systemPrompt?: string | null
      isEnabled: boolean
      createdAtUtc: string
      updatedAtUtc: string
    }
  }>('/api/v1/agents', dtoToCreateRequest(dto))
  return agentResponseToDto(data.data)
}

/** 更新 Agent */
export async function updateAgent(dto: UpdateAgentDto): Promise<AgentDto | null> {
  if (USE_MOCK) {
    await wait(300)
    const index = agentsData.findIndex((a) => a.id === dto.id)
    if (index === -1) return null
    const existing = agentsData[index]
    if (!existing) return null
    const updated: AgentDto = {
      ...existing,
      ...dto,
      updatedAt: new Date().toLocaleString('zh-CN', { hour12: false }),
    }
    agentsData[index] = updated
    return updated
  }
  try {
    const { data } = await request.put<{
      data: {
        id: string
        name: string
        description?: string | null
        keywords?: string | null
        systemPrompt?: string | null
        isEnabled: boolean
        createdAtUtc: string
        updatedAtUtc: string
      }
    }>(`/api/v1/agents/${dto.id}`, dtoToUpdateRequest(dto))
    return agentResponseToDto(data.data)
  } catch {
    return null
  }
}

/** 删除 Agent */
export async function deleteAgent(id: string): Promise<boolean> {
  if (USE_MOCK) {
    await wait(200)
    const index = agentsData.findIndex((a) => a.id === id)
    if (index === -1) return false
    agentsData.splice(index, 1)
    return true
  }
  await request.delete(`/api/v1/agents/${id}`)
  return true
}

/** 根据消息内容自动选择最合适的 Agent */
export async function selectAgentByMessage(message: string): Promise<AgentDto | null> {
  if (USE_MOCK) {
    await wait(50)
    const enabledAgents = agentsData.filter((a) => a.enabled)
    if (enabledAgents.length === 0) return null

    const lowerMessage = message.toLowerCase()
    let bestAgent: AgentDto | null = null
    let bestScore = 0

    for (const agent of enabledAgents) {
      let score = 0
      for (const kw of agent.keywords) {
        if (lowerMessage.includes(kw.keyword.toLowerCase())) {
          score += kw.weight
        }
      }
      if (score > bestScore) {
        bestScore = score
        bestAgent = agent
      }
    }
    return bestScore >= 0.5 ? bestAgent : null
  }
  try {
    const { data } = await request.get<{
      data: {
        id: string
        name: string
        description?: string | null
        keywords?: string | null
        systemPrompt?: string | null
        isEnabled: boolean
        createdAtUtc: string
        updatedAtUtc: string
      } | null
    }>('/api/v1/agents/match', { params: { input: message } })
    return data.data ? agentResponseToDto(data.data) : null
  } catch {
    return null
  }
}
