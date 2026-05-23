/**
 * Agent API 模块 - 提供 Agent 的 CRUD 操作和生命周期管理
 * 支持真实 API 调用和 Mock 数据切换
 */

import { request } from '@/lib/request'
import { USE_MOCK } from '@/config/runtime'
import type {
  PagedResult,
  AgentDto,
  CreateAgentDto,
  UpdateAgentDto,
  AssignTaskRequestDto,
  FailTaskRequestDto,
  EnterDormantRequestDto,
  AgentLearningRecordDto,
  AgentDormantRecordDto,
} from '@/modules/agent-center/api/agent.types'
import {
  AgentState,
  AgentRole,
  FailureCategory,
  LearningTriggerType,
} from '@/modules/agent-center/api/agent.types'

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
    state: AgentState.Idle,
    role: AgentRole.Worker,
    stateChangedAt: '2026-05-22 10:00:00',
    currentTaskId: null,
    dormantReason: null,
    skillTags: '代码审查,质量检查',
    learningProgress: 0,
    version: 1,
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
    state: AgentState.Idle,
    role: AgentRole.Manager,
    stateChangedAt: '2026-05-22 11:00:00',
    currentTaskId: null,
    dormantReason: null,
    skillTags: '架构设计,技术方案',
    learningProgress: 0,
    version: 1,
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
    state: AgentState.Handling,
    role: AgentRole.Worker,
    stateChangedAt: '2026-05-23 09:00:00',
    currentTaskId: 'task-123',
    dormantReason: null,
    skillTags: '数据库优化,SQL调优',
    learningProgress: 0,
    version: 2,
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
    state: AgentState.Dormant,
    role: AgentRole.Worker,
    stateChangedAt: '2026-05-20 18:00:00',
    currentTaskId: null,
    dormantReason: '连续学习效果不佳',
    skillTags: '前端开发,Vue,React',
    learningProgress: 0,
    version: 3,
    createdAt: '2026-05-04 14:00:00',
    updatedAt: '2026-05-18 10:00:00',
  },
]

// Mock 学习记录
const defaultLearningRecords: AgentLearningRecordDto[] = [
  {
    id: 'lr-1',
    agentId: 'agent-1',
    triggerType: LearningTriggerType.PostTaskReview,
    summary: '学习了代码审查最佳实践',
    result: '成功更新了审查规则',
    isSuccess: true,
    skillTags: '代码审查',
    createdAt: '2026-05-21 15:00:00',
  },
]

// Mock 休眠记录
const defaultDormantRecords: AgentDormantRecordDto[] = [
  {
    id: 'dr-1',
    agentId: 'agent-4',
    reason: '连续学习效果不佳',
    isWoken: false,
    wokeUpAt: null,
    wokenUpBy: null,
    createdAt: '2026-05-20 18:00:00',
  },
]

// ==================== Mock 辅助函数 ====================

let agentsData: AgentDto[] = [...defaultAgents]
let learningRecordsData: AgentLearningRecordDto[] = [...defaultLearningRecords]
let dormantRecordsData: AgentDormantRecordDto[] = [...defaultDormantRecords]

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
  state: string
  role: string
  stateChangedAtUtc: string
  currentTaskId?: string | null
  dormantReason?: string | null
  skillTags?: string | null
  learningProgress: number
  version: number
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
    state: res.state as AgentState,
    role: res.role as AgentRole,
    stateChangedAt: res.stateChangedAtUtc,
    currentTaskId: res.currentTaskId,
    dormantReason: res.dormantReason,
    skillTags: res.skillTags,
    learningProgress: res.learningProgress,
    version: res.version,
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
    role: dto.role,
    skillTags: dto.skillTags,
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
  if (dto.role !== undefined) req.role = dto.role
  if (dto.skillTags !== undefined) req.skillTags = dto.skillTags
  if (dto.version !== undefined) req.version = dto.version
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
      state: string
      role: string
      stateChangedAtUtc: string
      currentTaskId?: string | null
      dormantReason?: string | null
      skillTags?: string | null
      learningProgress: number
      version: number
      createdAtUtc: string
      updatedAtUtc: string
    }>
    totalCount: number
    page: number
    pageSize: number }
  }>('/v1/agents', { params: { page, pageSize } })
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
        state: string
        role: string
        stateChangedAtUtc: string
        currentTaskId?: string | null
        dormantReason?: string | null
        skillTags?: string | null
        learningProgress: number
        version: number
        createdAtUtc: string
        updatedAtUtc: string
      }>
      totalCount: number
      page: number
      pageSize: number
    }
  }>('/v1/agents')
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
        state: string
        role: string
        stateChangedAtUtc: string
        currentTaskId?: string | null
        dormantReason?: string | null
        skillTags?: string | null
        learningProgress: number
        version: number
        createdAtUtc: string
        updatedAtUtc: string
      }
    }>(`/v1/agents/${id}`)
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
      state: AgentState.Idle,
      stateChangedAt: now,
      currentTaskId: null,
      dormantReason: null,
      learningProgress: 0,
      version: 1,
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
      state: string
      role: string
      stateChangedAtUtc: string
      currentTaskId?: string | null
      dormantReason?: string | null
      skillTags?: string | null
      learningProgress: number
      version: number
      createdAtUtc: string
      updatedAtUtc: string
    }
  }>('/v1/agents', dtoToCreateRequest(dto))
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
      version: dto.version ?? existing.version,
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
        state: string
        role: string
        stateChangedAtUtc: string
        currentTaskId?: string | null
        dormantReason?: string | null
        skillTags?: string | null
        learningProgress: number
        version: number
        createdAtUtc: string
        updatedAtUtc: string
      }
    }>(`/v1/agents/${dto.id}`, dtoToUpdateRequest(dto))
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
  await request.delete(`/v1/agents/${id}`)
  return true
}

/** 根据消息内容自动选择最合适的 Agent */
export async function selectAgentByMessage(message: string): Promise<AgentDto | null> {
  if (USE_MOCK) {
    await wait(50)
    const enabledAgents = agentsData.filter((a) => a.enabled && a.state === AgentState.Idle)
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
        state: string
        role: string
        stateChangedAtUtc: string
        currentTaskId?: string | null
        dormantReason?: string | null
        skillTags?: string | null
        learningProgress: number
        version: number
        createdAtUtc: string
        updatedAtUtc: string
      } | null
    }>('/v1/agents/match', { params: { input: message } })
    return data.data ? agentResponseToDto(data.data) : null
  } catch {
    return null
  }
}

// ==================== 生命周期管理 API ====================

/** 根据状态获取 Agent 列表 */
export async function getAgentsByState(state: AgentState): Promise<AgentDto[]> {
  if (USE_MOCK) {
    await wait(150)
    return agentsData.filter((a) => a.state === state)
  }
  const { data } = await request.get<{
    data: Array<{
      id: string
      name: string
      description?: string | null
      keywords?: string | null
      systemPrompt?: string | null
      isEnabled: boolean
      state: string
      role: string
      stateChangedAtUtc: string
      currentTaskId?: string | null
      dormantReason?: string | null
      skillTags?: string | null
      learningProgress: number
      version: number
      createdAtUtc: string
      updatedAtUtc: string
    }>
  }>(`/v1/agents/state/${state}`)
  return data.data.map(agentResponseToDto)
}

/** 获取休眠 Agent 列表 */
export async function getDormantAgents(): Promise<AgentDto[]> {
  if (USE_MOCK) {
    await wait(150)
    return agentsData.filter((a) => a.state === AgentState.Dormant)
  }
  const { data } = await request.get<{
    data: Array<{
      id: string
      name: string
      description?: string | null
      keywords?: string | null
      systemPrompt?: string | null
      isEnabled: boolean
      state: string
      role: string
      stateChangedAtUtc: string
      currentTaskId?: string | null
      dormantReason?: string | null
      skillTags?: string | null
      learningProgress: number
      version: number
      createdAtUtc: string
      updatedAtUtc: string
    }>
  }>('/v1/agents/dormant')
  return data.data.map(agentResponseToDto)
}

/** 分配任务给 Agent */
export async function assignTask(agentId: string, dto: AssignTaskRequestDto): Promise<AgentDto> {
  if (USE_MOCK) {
    await wait(200)
    const agent = agentsData.find((a) => a.id === agentId)
    if (!agent) throw new Error('Agent not found')
    const now = new Date().toLocaleString('zh-CN', { hour12: false })
    const updated: AgentDto = {
      ...agent,
      state: AgentState.Handling,
      stateChangedAt: now,
      currentTaskId: dto.taskId,
      version: agent.version + 1,
      updatedAt: now,
    }
    const index = agentsData.findIndex((a) => a.id === agentId)
    if (index !== -1) agentsData[index] = updated
    return updated
  }
  const { data } = await request.post<{
    data: {
      id: string
      name: string
      description?: string | null
      keywords?: string | null
      systemPrompt?: string | null
      isEnabled: boolean
      state: string
      role: string
      stateChangedAtUtc: string
      currentTaskId?: string | null
      dormantReason?: string | null
      skillTags?: string | null
      learningProgress: number
      version: number
      createdAtUtc: string
      updatedAtUtc: string
    }
  }>(`/v1/agents/${agentId}/assign`, dto)
  return agentResponseToDto(data.data)
}

/** 完成 Agent 任务 */
export async function completeTask(agentId: string): Promise<AgentDto> {
  if (USE_MOCK) {
    await wait(200)
    const agent = agentsData.find((a) => a.id === agentId)
    if (!agent) throw new Error('Agent not found')
    const now = new Date().toLocaleString('zh-CN', { hour12: false })
    const updated: AgentDto = {
      ...agent,
      state: AgentState.Idle,
      stateChangedAt: now,
      currentTaskId: null,
      version: agent.version + 1,
      updatedAt: now,
    }
    const index = agentsData.findIndex((a) => a.id === agentId)
    if (index !== -1) agentsData[index] = updated
    return updated
  }
  const { data } = await request.post<{
    data: {
      id: string
      name: string
      description?: string | null
      keywords?: string | null
      systemPrompt?: string | null
      isEnabled: boolean
      state: string
      role: string
      stateChangedAtUtc: string
      currentTaskId?: string | null
      dormantReason?: string | null
      skillTags?: string | null
      learningProgress: number
      version: number
      createdAtUtc: string
      updatedAtUtc: string
    }
  }>(`/v1/agents/${agentId}/complete`)
  return agentResponseToDto(data.data)
}

/** 标记任务失败 */
export async function failTask(agentId: string, dto: FailTaskRequestDto): Promise<AgentDto> {
  if (USE_MOCK) {
    await wait(200)
    const agent = agentsData.find((a) => a.id === agentId)
    if (!agent) throw new Error('Agent not found')
    const now = new Date().toLocaleString('zh-CN', { hour12: false })
    const updated: AgentDto = {
      ...agent,
      state: AgentState.Idle,
      stateChangedAt: now,
      currentTaskId: null,
      version: agent.version + 1,
      updatedAt: now,
    }
    const index = agentsData.findIndex((a) => a.id === agentId)
    if (index !== -1) agentsData[index] = updated
    return updated
  }
  const { data } = await request.post<{
    data: {
      id: string
      name: string
      description?: string | null
      keywords?: string | null
      systemPrompt?: string | null
      isEnabled: boolean
      state: string
      role: string
      stateChangedAtUtc: string
      currentTaskId?: string | null
      dormantReason?: string | null
      skillTags?: string | null
      learningProgress: number
      version: number
      createdAtUtc: string
      updatedAtUtc: string
    }
  }>(`/v1/agents/${agentId}/fail`, dto)
  return agentResponseToDto(data.data)
}

/** 触发 Agent 学习 */
export async function startLearning(agentId: string): Promise<AgentDto> {
  if (USE_MOCK) {
    await wait(200)
    const agent = agentsData.find((a) => a.id === agentId)
    if (!agent) throw new Error('Agent not found')
    const now = new Date().toLocaleString('zh-CN', { hour12: false })
    const updated: AgentDto = {
      ...agent,
      state: AgentState.Learning,
      stateChangedAt: now,
      learningProgress: 0,
      version: agent.version + 1,
      updatedAt: now,
    }
    const index = agentsData.findIndex((a) => a.id === agentId)
    if (index !== -1) agentsData[index] = updated
    return updated
  }
  const { data } = await request.post<{
    data: {
      id: string
      name: string
      description?: string | null
      keywords?: string | null
      systemPrompt?: string | null
      isEnabled: boolean
      state: string
      role: string
      stateChangedAtUtc: string
      currentTaskId?: string | null
      dormantReason?: string | null
      skillTags?: string | null
      learningProgress: number
      version: number
      createdAtUtc: string
      updatedAtUtc: string
    }
  }>(`/v1/agents/${agentId}/learn`)
  return agentResponseToDto(data.data)
}

/** Agent 进入休眠 */
export async function enterDormant(agentId: string, dto: EnterDormantRequestDto): Promise<AgentDto> {
  if (USE_MOCK) {
    await wait(200)
    const agent = agentsData.find((a) => a.id === agentId)
    if (!agent) throw new Error('Agent not found')
    const now = new Date().toLocaleString('zh-CN', { hour12: false })
    const updated: AgentDto = {
      ...agent,
      state: AgentState.Dormant,
      stateChangedAt: now,
      dormantReason: dto.reason,
      version: agent.version + 1,
      updatedAt: now,
    }
    const index = agentsData.findIndex((a) => a.id === agentId)
    if (index !== -1) agentsData[index] = updated

    // 添加休眠记录
    const record: AgentDormantRecordDto = {
      id: `dr-${Date.now()}`,
      agentId,
      reason: dto.reason,
      isWoken: false,
      wokeUpAt: null,
      wokenUpBy: null,
      createdAt: now,
    }
    dormantRecordsData.push(record)

    return updated
  }
  const { data } = await request.post<{
    data: {
      id: string
      name: string
      description?: string | null
      keywords?: string | null
      systemPrompt?: string | null
      isEnabled: boolean
      state: string
      role: string
      stateChangedAtUtc: string
      currentTaskId?: string | null
      dormantReason?: string | null
      skillTags?: string | null
      learningProgress: number
      version: number
      createdAtUtc: string
      updatedAtUtc: string
    }
  }>(`/v1/agents/${agentId}/dormant`, dto)
  return agentResponseToDto(data.data)
}

/** 唤醒休眠的 Agent */
export async function wakeUpAgent(agentId: string): Promise<AgentDto> {
  if (USE_MOCK) {
    await wait(200)
    const agent = agentsData.find((a) => a.id === agentId)
    if (!agent) throw new Error('Agent not found')
    const now = new Date().toLocaleString('zh-CN', { hour12: false })
    const updated: AgentDto = {
      ...agent,
      state: AgentState.Idle,
      stateChangedAt: now,
      dormantReason: null,
      version: agent.version + 1,
      updatedAt: now,
    }
    const index = agentsData.findIndex((a) => a.id === agentId)
    if (index !== -1) agentsData[index] = updated

    // 更新休眠记录
    const recordIndex = dormantRecordsData.findIndex((r) => r.agentId === agentId && !r.isWoken)
    if (recordIndex !== -1) {
      const existingRecord = dormantRecordsData[recordIndex]
      if (existingRecord) {
        dormantRecordsData[recordIndex] = {
          id: existingRecord.id,
          agentId: existingRecord.agentId,
          reason: existingRecord.reason,
          isWoken: true,
          wokeUpAt: now,
          wokenUpBy: 'current-user',
          createdAt: existingRecord.createdAt,
        }
      }
    }

    return updated
  }
  const { data } = await request.post<{
    data: {
      id: string
      name: string
      description?: string | null
      keywords?: string | null
      systemPrompt?: string | null
      isEnabled: boolean
      state: string
      role: string
      stateChangedAtUtc: string
      currentTaskId?: string | null
      dormantReason?: string | null
      skillTags?: string | null
      learningProgress: number
      version: number
      createdAtUtc: string
      updatedAtUtc: string
    }
  }>(`/v1/agents/${agentId}/wake`)
  return agentResponseToDto(data.data)
}

/** 获取 Agent 学习记录 */
export async function getAgentLearningRecords(agentId: string): Promise<AgentLearningRecordDto[]> {
  if (USE_MOCK) {
    await wait(150)
    return learningRecordsData.filter((r) => r.agentId === agentId)
  }
  const { data } = await request.get<{
    data: Array<{
      id: string
      agentId: string
      triggerType: string
      summary?: string | null
      result?: string | null
      isSuccess: boolean
      skillTags?: string | null
      createdAtUtc: string
    }>
  }>(`/v1/agents/${agentId}/learning-records`)
  return data.data.map((r) => ({
    id: r.id,
    agentId: r.agentId,
    triggerType: r.triggerType as LearningTriggerType,
    summary: r.summary,
    result: r.result,
    isSuccess: r.isSuccess,
    skillTags: r.skillTags,
    createdAt: r.createdAtUtc,
  }))
}

/** 获取 Agent 休眠记录 */
export async function getAgentDormantRecords(agentId: string): Promise<AgentDormantRecordDto[]> {
  if (USE_MOCK) {
    await wait(150)
    return dormantRecordsData.filter((r) => r.agentId === agentId)
  }
  const { data } = await request.get<{
    data: Array<{
      id: string
      agentId: string
      reason: string
      isWoken: boolean
      wokeUpAtUtc?: string | null
      wokenUpBy?: string | null
      createdAtUtc: string
    }>
  }>(`/v1/agents/${agentId}/dormant-records`)
  return data.data.map((r) => ({
    id: r.id,
    agentId: r.agentId,
    reason: r.reason,
    isWoken: r.isWoken,
    wokeUpAt: r.wokeUpAtUtc,
    wokenUpBy: r.wokenUpBy,
    createdAt: r.createdAtUtc,
  }))
}
