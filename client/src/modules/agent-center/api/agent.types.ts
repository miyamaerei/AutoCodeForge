/**
 * Agent 模块类型定义
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

/** API 错误 */
export interface ApiError {
  message: string
  errors?: Record<string, string[]>
}

// ==================== Agent 生命周期类型 ====================

/** Agent 状态枚举 */
export enum AgentState {
  /** 空闲 */
  Idle = 'Idle',
  /** 处理中 */
  Handling = 'Handling',
  /** 学习中 */
  Learning = 'Learning',
  /** 休眠 */
  Dormant = 'Dormant',
}

/** Agent 角色枚举 */
export enum AgentRole {
  /** 秘书 */
  Secretary = 'Secretary',
  /** 经理 */
  Manager = 'Manager',
  /** 工人 */
  Worker = 'Worker',
}

/** 失败类别枚举 */
export enum FailureCategory {
  /** 代码错误 */
  CodeError = 'CodeError',
  /** LLM 异常 */
  LLMException = 'LLMException',
  /** 需求问题 */
  RequirementIssue = 'RequirementIssue',
  /** 审查拒绝 */
  ReviewRejection = 'ReviewRejection',
  /** 超时 */
  Timeout = 'Timeout',
  /** 未知 */
  Unknown = 'Unknown',
}

/** 学习触发类型枚举 */
export enum LearningTriggerType {
  /** 空闲超时 */
  IdleTimeout = 'IdleTimeout',
  /** 任务后复盘 */
  PostTaskReview = 'PostTaskReview',
  /** 异常触发 */
  ExceptionTriggered = 'ExceptionTriggered',
  /** 手动触发 */
  ManualTriggered = 'ManualTriggered',
}

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
  /** Agent 状态 */
  state: AgentState
  /** Agent 角色 */
  role: AgentRole
  /** 状态变更时间 */
  stateChangedAt: string
  /** 当前任务 ID */
  currentTaskId?: string | null
  /** 休眠原因 */
  dormantReason?: string | null
  /** 技能标签 */
  skillTags?: string | null
  /** 学习进度 */
  learningProgress: number
  /** 版本号（乐观锁） */
  version: number
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
  role: AgentRole
  skillTags?: string | null
}

/** 更新 Agent 请求 */
export interface UpdateAgentDto extends Partial<Omit<CreateAgentDto, 'icon'>> {
  id: string
  version?: number | null
}

// ==================== Agent 状态操作请求 ====================

/** 分配任务请求 */
export interface AssignTaskRequestDto {
  taskId: string
}

/** 任务失败请求 */
export interface FailTaskRequestDto {
  reason: string
  failureCategory: FailureCategory
}

/** 进入休眠请求 */
export interface EnterDormantRequestDto {
  reason: string
}

// ==================== Agent 记录类型 ====================

/** Agent 学习记录 DTO */
export interface AgentLearningRecordDto {
  /** 记录 ID */
  id: string
  /** Agent ID */
  agentId: string
  /** 触发类型 */
  triggerType: LearningTriggerType
  /** 学习摘要 */
  summary?: string | null
  /** 学习结果 */
  result?: string | null
  /** 成功与否 */
  isSuccess: boolean
  /** 技能标签 */
  skillTags?: string | null
  /** 创建时间 */
  createdAt: string
}

/** Agent 休眠记录 DTO */
export interface AgentDormantRecordDto {
  /** 记录 ID */
  id: string
  /** Agent ID */
  agentId: string
  /** 休眠原因 */
  reason: string
  /** 是否已唤醒 */
  isWoken: boolean
  /** 唤醒时间 */
  wokeUpAt?: string | null
  /** 唤醒人 ID */
  wokenUpBy?: string | null
  /** 创建时间 */
  createdAt: string
}
