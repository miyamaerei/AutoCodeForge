/**
 * Workflow 模块类型定义
 * 基于 Microsoft Agent Framework 编排体系设计
 * 与后端 DTO 保持一致
 */

// ============================================
// 核心类型定义（与后端 DTO 对应）
// ============================================

/** 工作流状态 */
export type WorkflowStatus = 'Draft' | 'Published' | 'Archived'

/** 工作流定义（对应 WorkflowResponse） */
export interface WorkflowDefinition {
  id: string
  name: string
  description?: string
  status: WorkflowStatus
  version: number
  nodesJson?: string
  edgesJson?: string
  executorsJson?: string
  contextProviders?: string[]
  createdAtUtc: string
  updatedAtUtc: string
}

/** 工作流实例状态 */
export type WorkflowInstanceStatus = 'Pending' | 'Running' | 'Paused' | 'Completed' | 'Failed' | 'Terminated'

/** 工作流实例（对应 WorkflowInstanceResponse） */
export interface WorkflowInstance {
  id: string
  workflowId: string
  workflowName: string
  currentNodeId?: string
  status: WorkflowInstanceStatus
  progress: number
  inputJson?: string
  outputJson?: string
  errorMessage?: string
  startedAtUtc?: string
  completedAtUtc?: string
  agentId?: string
  createdAtUtc: string
  updatedAtUtc: string
}

/** 工作流事件（对应 WorkflowEventResponse） */
export interface WorkflowEvent {
  id: string
  instanceId: string
  eventType: WorkflowEventType
  message?: string
  dataJson?: string
  nodeId?: string
  level: 'Info' | 'Warning' | 'Error'
  timestamp: string
}

/** 工作流事件类型（与后端常量一致） */
export type WorkflowEventType = 
  | 'started'
  | 'node_entered'
  | 'node_completed'
  | 'node_failed'
  | 'progress_updated'
  | 'paused'
  | 'resumed'
  | 'completed'
  | 'failed'
  | 'terminated'

/** 创建工作流请求（对应 CreateWorkflowRequest） */
export interface CreateWorkflowRequest {
  name: string
  description?: string
  nodesJson?: string
  edgesJson?: string
  executorsJson?: string
  contextProviders?: string[]
}

/** 更新工作流请求（对应 UpdateWorkflowRequest） */
export interface UpdateWorkflowRequest {
  name?: string
  description?: string
  nodesJson?: string
  edgesJson?: string
  executorsJson?: string
  contextProviders?: string[]
  status?: WorkflowStatus
}

/** 执行工作流请求（对应 ExecuteWorkflowRequest） */
export interface ExecuteWorkflowRequest {
  type: string
  dataJson?: string
  context?: WorkflowContext
  agentId?: string
}

/** 工作流上下文 */
export interface WorkflowContext {
  userId?: string
  metadata?: Record<string, unknown>
}

// ============================================
// 工作流设计器相关类型
// ============================================

/** 工作流节点数据 */
export interface WorkflowNodeData {
  label: string
  nodeType: 'agent' | 'task' | 'condition' | 'start' | 'end'
  agentId?: string
  taskId?: string
  agentRole?: 'Worker' | 'Manager' | 'Secretary'
  taskStatus?: string
  condition?: string
  config?: Record<string, unknown>
}

/** 工作流节点 */
export interface WorkflowNode {
  id: string
  type: string
  position: { x: number; y: number }
  data: WorkflowNodeData
}

/** 工作流边 */
export interface WorkflowEdge {
  id: string
  source: string
  target: string
  type?: string
  animated?: boolean
  markerEnd?: any
  data?: {
    condition?: string
  }
}

// ============================================
// 旧类型兼容（保留以便迁移）
// ============================================

/** Executor 定义（旧类型，用于兼容） */
export interface ExecutorDefinition {
  id: string
  type: string
  label: string
  config: Record<string, unknown>
}

/** Edge 定义（旧类型，用于兼容） */
export interface EdgeDefinition {
  id: string
  from: string
  to: string
  condition?: ConditionConfig
}

/** 条件配置（旧类型，用于兼容） */
export interface ConditionConfig {
  type: 'expression' | 'switch'
  value: string
  cases?: SwitchCase[]
}

/** Switch 分支（旧类型，用于兼容） */
export interface SwitchCase {
  value: string
  target: string
}

/** 工作流输入（旧类型，用于兼容） */
export interface WorkflowInput {
  type: string
  data: Record<string, unknown>
  context?: WorkflowContext
}

// ============================================
// 任务相关类型（保留）
// ============================================

/** 任务状态 */
export type TaskStatus = 'todo' | 'in_progress' | 'review' | 'done'

/** 任务 */
export interface Task {
  id: string
  title: string
  description: string
  status: TaskStatus
  repoId?: string
  agentId?: string
  priority: 'low' | 'medium' | 'high'
  createdAt: Date
  updatedAt: Date
}

// ============================================
// Agent 相关类型（保留）
// ============================================

/** Agent */
export interface Agent {
  id: string
  name: string
  role: 'worker' | 'manager' | 'secretary'
  status: 'idle' | 'busy' | 'offline'
  currentTaskId?: string
  capabilities: string[]
}

// ============================================
// Repo 相关类型（保留）
// ============================================

/** Repo */
export interface Repo {
  id: string
  name: string
  url: string
  branch: string
}