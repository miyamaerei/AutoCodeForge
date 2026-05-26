/**
 * 极简 Workflow Store
 * 核心概念：Workflow = Agent 的可视化编排 + Kanban 任务视图
 */
import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Node, Edge, XYPosition } from '@vue-flow/core'

// ============================================
// 类型定义
// ============================================

/** 任务状态（Kanban 列） */
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

/** Agent */
export interface Agent {
  id: string
  name: string
  role: 'worker' | 'manager' | 'secretary'
  status: 'idle' | 'busy' | 'offline'
  currentTaskId?: string
  capabilities: string[]
}

/** Repo */
export interface Repo {
  id: string
  name: string
  url: string
  branch: string
}

/** Workflow 节点数据 */
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

/** Workflow 节点 */
export interface WorkflowNode {
  id: string
  type: string
  position: XYPosition
  data: WorkflowNodeData
}

/** Workflow 边 */
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

/** 工作流实例 */
export interface WorkflowInstance {
  id: string
  workflowId: string
  status: 'pending' | 'running' | 'paused' | 'completed' | 'failed'
  currentNodeId: string
  progress: number
  startedAt: Date
  completedAt?: Date
  errorMessage?: string
}

// ============================================
// Store 定义
// ============================================

export const useWorkflowStore = defineStore('workflow', () => {
  // ----------------------------------------
  // State
  // ----------------------------------------

  /** 当前选中的 Repo */
  const currentRepo = ref<Repo | null>(null)

  /** 任务列表 */
  const tasks = ref<Task[]>([])

  /** Agent 列表 */
  const agents = ref<Agent[]>([])

  /** 工作流节点 */
  const workflowNodes = ref<WorkflowNode[]>([])

  /** 工作流边 */
  const workflowEdges = ref<WorkflowEdge[]>([])

  /** 当前工作流实例 */
  const currentInstance = ref<WorkflowInstance | null>(null)

  /** SSE 事件订阅 */
  let eventSource: EventSource | null = null

  // ----------------------------------------
  // Getters
  // ----------------------------------------

  /** 按状态分组的任务（Kanban 用） */
  const tasksByStatus = computed(() => {
    const groups: Record<TaskStatus, Task[]> = {
      todo: [],
      in_progress: [],
      review: [],
      done: []
    }
    tasks.value.forEach(task => {
      groups[task.status].push(task)
    })
    return groups
  })

  /** 空闲的 Agent */
  const idleAgents = computed(() =>
    agents.value.filter(a => a.status === 'idle')
  )

  /** 运行中的工作流实例 */
  const runningInstance = computed(() =>
    currentInstance.value?.status === 'running' ? currentInstance.value : null
  )

  // ----------------------------------------
  // Actions
  // ----------------------------------------

  /**
   * 加载任务列表
   */
  async function loadTasks(repoId?: string) {
    // TODO: 调用 API
    // const response = await api.getTasks(repoId)
    // tasks.value = response.data
  }

  /**
   * 创建任务
   */
  async function createTask(task: Partial<Task>) {
    // TODO: 调用 API
    const newTask: Task = {
      id: crypto.randomUUID(),
      title: task.title || '新任务',
      description: task.description || '',
      status: task.status || 'todo',
      repoId: task.repoId || currentRepo.value?.id,
      priority: task.priority || 'medium',
      createdAt: new Date(),
      updatedAt: new Date()
    }
    tasks.value.push(newTask)
    return newTask
  }

  /**
   * 更新任务状态（拖拽到 Kanban 新列）
   */
  async function updateTaskStatus(taskId: string, newStatus: TaskStatus) {
    const task = tasks.value.find(t => t.id === taskId)
    if (task) {
      task.status = newStatus
      task.updatedAt = new Date()
    }
  }

  /**
   * 加载 Agent 列表
   */
  async function loadAgents() {
    // TODO: 调用 API
  }

  /**
   * 分配任务给 Agent
   */
  async function assignTaskToAgent(taskId: string, agentId: string) {
    const task = tasks.value.find(t => t.id === taskId)
    const agent = agents.value.find(a => a.id === agentId)
    if (task && agent) {
      task.agentId = agentId
      task.status = 'in_progress'
      agent.currentTaskId = taskId
      agent.status = 'busy'
    }
  }

  /**
   * 添加工作流节点
   */
  function addWorkflowNode(node: Omit<WorkflowNode, 'id'> & { position: XYPosition }) {
    workflowNodes.value.push({
      ...node,
      id: crypto.randomUUID()
    } as WorkflowNode)
  }

  /**
   * 添加工作流边
   */
  function addWorkflowEdge(edge: Omit<WorkflowEdge, 'id'> & { source: string; target: string }) {
    workflowEdges.value.push({
      ...edge,
      id: `e-${edge.source}-${edge.target}`
    } as WorkflowEdge)
  }

  /**
   * 执行工作流
   */
  async function executeWorkflow(workflowId: string) {
    // TODO: 调用 API 启动工作流
    // const response = await api.executeWorkflow(workflowId)
    currentInstance.value = {
      id: crypto.randomUUID(),
      workflowId,
      status: 'running',
      currentNodeId: 'start',
      progress: 0,
      startedAt: new Date()
    }

    // 订阅实时事件
    subscribeToEvents(currentInstance.value.id)
  }

  /**
   * 订阅工作流事件（SSE）
   */
  function subscribeToEvents(instanceId: string) {
    eventSource?.close()

    eventSource = new EventSource(`/api/workflows/instances/${instanceId}/events`)

    eventSource.onmessage = (event) => {
      const data = JSON.parse(event.data)

      switch (data.type) {
        case 'StepStarted':
          if (currentInstance.value) {
            currentInstance.value.currentNodeId = data.stepId
          }
          break
        case 'StepCompleted':
          if (currentInstance.value) {
            currentInstance.value.progress += 25 // 假设 4 个步骤
          }
          break
        case 'WorkflowCompleted':
          if (currentInstance.value) {
            currentInstance.value.status = 'completed'
            currentInstance.value.completedAt = new Date()
            currentInstance.value.progress = 100
          }
          break
        case 'WorkflowFailed':
          if (currentInstance.value) {
            currentInstance.value.status = 'failed'
            currentInstance.value.errorMessage = data.error
          }
          break
      }
    }
  }

  /**
   * 暂停工作流
   */
  async function pauseWorkflow() {
    if (currentInstance.value) {
      currentInstance.value.status = 'paused'
      // TODO: 调用 API
    }
  }

  /**
   * 恢复工作流
   */
  async function resumeWorkflow() {
    if (currentInstance.value) {
      currentInstance.value.status = 'running'
      // TODO: 调用 API
    }
  }

  /**
   * 终止工作流
   */
  async function terminateWorkflow() {
    if (currentInstance.value) {
      currentInstance.value.status = 'failed'
      currentInstance.value.completedAt = new Date()
      // TODO: 调用 API
    }
    eventSource?.close()
  }

  /**
   * 清理
   */
  function cleanup() {
    eventSource?.close()
    eventSource = null
  }

  return {
    // State
    currentRepo,
    tasks,
    agents,
    workflowNodes,
    workflowEdges,
    currentInstance,

    // Getters
    tasksByStatus,
    idleAgents,
    runningInstance,

    // Actions
    loadTasks,
    createTask,
    updateTaskStatus,
    loadAgents,
    assignTaskToAgent,
    addWorkflowNode,
    addWorkflowEdge,
    executeWorkflow,
    pauseWorkflow,
    resumeWorkflow,
    terminateWorkflow,
    cleanup
  }
})
