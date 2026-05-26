/**
 * Workflow Store
 * 基于 Microsoft Agent Framework 编排体系设计
 * 更新以支持新的后端 API
 */

import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { XYPosition } from '@vue-flow/core'
import { workflowApi } from '../api/workflow'
import type {
  WorkflowDefinition,
  WorkflowInstance,
  WorkflowEvent,
  WorkflowNode,
  WorkflowEdge,
  Task,
  TaskStatus,
  Agent,
  Repo,
  CreateWorkflowRequest,
  UpdateWorkflowRequest,
  ExecuteWorkflowRequest,
  WorkflowInstanceStatus
} from '../types/workflow'

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

  /** 工作流定义列表 */
  const workflows = ref<WorkflowDefinition[]>([])

  /** 当前选中的工作流 */
  const currentWorkflow = ref<WorkflowDefinition | null>(null)

  /** 工作流节点（设计器用） */
  const workflowNodes = ref<WorkflowNode[]>([])

  /** 工作流边（设计器用） */
  const workflowEdges = ref<WorkflowEdge[]>([])

  /** 工作流实例列表 */
  const instances = ref<WorkflowInstance[]>([])

  /** 当前工作流实例 */
  const currentInstance = ref<WorkflowInstance | null>(null)

  /** SSE 事件订阅 Map */
  const eventSources = ref<Map<string, EventSource>>(new Map())

  /** 加载状态 */
  const loading = ref(false)

  /** 错误信息 */
  const error = ref<string | null>(null)

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
    return groups as unknown as Record<string, Task[]>
  })

  /** 空闲的 Agent */
  const idleAgents = computed(() =>
    agents.value.filter(a => a.status === 'idle')
  )

  /** 运行中的工作流实例 */
  const runningInstances = computed(() =>
    instances.value.filter(i => i.status === 'Running')
  )

  /** 失败的工作流实例 */
  const failedInstances = computed(() =>
    instances.value.filter(i => i.status === 'Failed')
  )

  /** 当前运行中的实例 */
  const runningInstance = computed(() =>
    currentInstance.value?.status === 'Running' ? currentInstance.value : null
  )

  /** 是否有工作流 */
  const hasWorkflows = computed(() => workflows.value.length > 0)

  /** 是否有实例 */
  const hasInstances = computed(() => instances.value.length > 0)

  // ----------------------------------------
  // Actions
  // ----------------------------------------

  /**
   * 加载任务列表
   */
  async function loadTasks(repoId?: string) {
    try {
      // TODO: 实现任务 API
      // const response = await taskApi.getTasks(repoId)
      // tasks.value = response.data
    } catch (e) {
      console.error('加载任务失败:', e)
    }
  }

  /**
   * 创建任务
   */
  async function createTask(task: Partial<Task>) {
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
    try {
      // TODO: 实现 Agent API
      // const response = await agentApi.getAgents()
      // agents.value = response.data
    } catch (e) {
      console.error('加载 Agent 失败:', e)
    }
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
   * 加载工作流列表（分页）
   */
  async function loadWorkflows(page = 1, pageSize = 20) {
    loading.value = true
    error.value = null
    try {
      const response = await workflowApi.getWorkflows(page, pageSize)
      workflows.value = response.items
    } catch (e) {
      error.value = String(e)
      console.error('加载工作流列表失败:', e)
    } finally {
      loading.value = false
    }
  }

  /**
   * 加载最近工作流
   */
  async function loadRecentWorkflows(take = 10) {
    loading.value = true
    error.value = null
    try {
      workflows.value = await workflowApi.getRecentWorkflows(take)
    } catch (e) {
      error.value = String(e)
      console.error('加载最近工作流失败:', e)
    } finally {
      loading.value = false
    }
  }

  /**
   * 加载工作流详情
   */
  async function loadWorkflow(id: string) {
    loading.value = true
    error.value = null
    try {
      currentWorkflow.value = await workflowApi.getWorkflow(id)
    } catch (e) {
      error.value = String(e)
      console.error('加载工作流详情失败:', e)
    } finally {
      loading.value = false
    }
  }

  /**
   * 创建工作流
   */
  async function createWorkflow(data: CreateWorkflowRequest) {
    loading.value = true
    error.value = null
    try {
      const workflow = await workflowApi.createWorkflow(data)
      workflows.value.unshift(workflow)
      return workflow
    } catch (e) {
      error.value = String(e)
      console.error('创建工作流失败:', e)
      throw e
    } finally {
      loading.value = false
    }
  }

  /**
   * 更新工作流
   */
  async function updateWorkflow(id: string, data: UpdateWorkflowRequest) {
    loading.value = true
    error.value = null
    try {
      const updated = await workflowApi.updateWorkflow(id, data)
      const index = workflows.value.findIndex(w => w.id === id)
      if (index !== -1) {
        workflows.value[index] = updated
      }
      if (currentWorkflow.value?.id === id) {
        currentWorkflow.value = updated
      }
      return updated
    } catch (e) {
      error.value = String(e)
      console.error('更新工作流失败:', e)
      throw e
    } finally {
      loading.value = false
    }
  }

  /**
   * 删除工作流
   */
  async function deleteWorkflow(id: string) {
    loading.value = true
    error.value = null
    try {
      await workflowApi.deleteWorkflow(id)
      workflows.value = workflows.value.filter(w => w.id !== id)
      if (currentWorkflow.value?.id === id) {
        currentWorkflow.value = null
      }
    } catch (e) {
      error.value = String(e)
      console.error('删除工作流失败:', e)
      throw e
    } finally {
      loading.value = false
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
  async function executeWorkflow(workflowId: string, input?: Partial<ExecuteWorkflowRequest>) {
    loading.value = true
    error.value = null
    try {
      const workflowInput: ExecuteWorkflowRequest = {
        type: 'DefaultInput',
        dataJson: input?.dataJson,
        context: input?.context,
        agentId: input?.agentId
      }
      
      const instance = await workflowApi.executeWorkflow(workflowId, workflowInput)
      instances.value.unshift(instance)
      currentInstance.value = instance
      
      // 订阅事件
      subscribeToInstanceEvents(instance.id)
      
      return instance
    } catch (e) {
      error.value = String(e)
      console.error('执行工作流失败:', e)
      throw e
    } finally {
      loading.value = false
    }
  }

  /**
   * 加载工作流实例列表（根据工作流ID）
   */
  async function loadWorkflowInstances(workflowId: string) {
    loading.value = true
    error.value = null
    try {
      instances.value = await workflowApi.getWorkflowInstances(workflowId)
    } catch (e) {
      error.value = String(e)
      console.error('加载工作流实例失败:', e)
    } finally {
      loading.value = false
    }
  }

  /**
   * 加载最近实例列表
   */
  async function loadRecentInstances(take = 10) {
    loading.value = true
    error.value = null
    try {
      instances.value = await workflowApi.getRecentInstances(take)
    } catch (e) {
      error.value = String(e)
      console.error('加载最近实例失败:', e)
    } finally {
      loading.value = false
    }
  }

  /**
   * 加载工作流实例详情
   */
  async function loadInstance(instanceId: string) {
    loading.value = true
    error.value = null
    try {
      currentInstance.value = await workflowApi.getWorkflowInstance(instanceId)
    } catch (e) {
      error.value = String(e)
      console.error('加载工作流实例详情失败:', e)
    } finally {
      loading.value = false
    }
  }

  /**
   * 暂停工作流实例
   */
  async function pauseWorkflowInstance(instanceId: string) {
    try {
      await workflowApi.pauseWorkflow(instanceId)
      
      const instance = instances.value.find(i => i.id === instanceId)
      if (instance) {
        instance.status = 'Paused'
      }
      if (currentInstance.value?.id === instanceId) {
        currentInstance.value.status = 'Paused'
      }
    } catch (e) {
      console.error('暂停工作流失败:', e)
      throw e
    }
  }

  /**
   * 恢复工作流实例
   */
  async function resumeWorkflowInstance(instanceId: string) {
    try {
      await workflowApi.resumeWorkflow(instanceId)
      
      const instance = instances.value.find(i => i.id === instanceId)
      if (instance) {
        instance.status = 'Running'
      }
      if (currentInstance.value?.id === instanceId) {
        currentInstance.value.status = 'Running'
      }
    } catch (e) {
      console.error('恢复工作流失败:', e)
      throw e
    }
  }

  /**
   * 终止工作流实例
   */
  async function terminateWorkflowInstance(instanceId: string) {
    try {
      await workflowApi.terminateWorkflow(instanceId)
      
      const instance = instances.value.find(i => i.id === instanceId)
      if (instance) {
        instance.status = 'Terminated'
      }
      if (currentInstance.value?.id === instanceId) {
        currentInstance.value.status = 'Terminated'
      }
      
      unsubscribeFromEvents(instanceId)
    } catch (e) {
      console.error('终止工作流失败:', e)
      throw e
    }
  }

  /**
   * 暂停工作流（别名方法）
   */
  async function pauseWorkflow(instanceId?: string) {
    const id = instanceId || currentInstance.value?.id
    if (!id) {
      throw new Error('No active workflow instance')
    }
    await pauseWorkflowInstance(id)
  }

  /**
   * 恢复工作流（别名方法）
   */
  async function resumeWorkflow(instanceId?: string) {
    const id = instanceId || currentInstance.value?.id
    if (!id) {
      throw new Error('No active workflow instance')
    }
    await resumeWorkflowInstance(id)
  }

  /**
   * 终止工作流（别名方法）
   */
  async function terminateWorkflow(instanceId?: string) {
    const id = instanceId || currentInstance.value?.id
    if (!id) {
      throw new Error('No active workflow instance')
    }
    await terminateWorkflowInstance(id)
  }

  /**
   * 删除工作流实例
   */
  async function deleteWorkflowInstance(instanceId: string) {
    try {
      await workflowApi.deleteInstance(instanceId)
      instances.value = instances.value.filter(i => i.id !== instanceId)
      
      if (currentInstance.value?.id === instanceId) {
        currentInstance.value = null
      }
      
      unsubscribeFromEvents(instanceId)
    } catch (e) {
      console.error('删除实例失败:', e)
      throw e
    }
  }

  /**
   * 加载实例事件列表
   */
  async function loadInstanceEvents(instanceId: string): Promise<WorkflowEvent[]> {
    try {
      return await workflowApi.getInstanceEvents(instanceId)
    } catch (e) {
      console.error('加载实例事件失败:', e)
      return []
    }
  }

  /**
   * 订阅工作流实例事件（SSE）
   */
  function subscribeToInstanceEvents(instanceId: string) {
    // 取消之前的订阅
    const existing = eventSources.value.get(instanceId)
    if (existing) {
      existing.close()
    }

    const eventSource = workflowApi.subscribeToEvents(instanceId, (event) => {
      // 更新实例状态
      const index = instances.value.findIndex(i => i.id === instanceId)
      if (index !== -1 && instances.value[index]) {
        updateInstanceFromEvent(instances.value[index], event)
      }
      
      // 更新当前实例
      if (currentInstance.value?.id === instanceId) {
        updateInstanceFromEvent(currentInstance.value, event)
      }
    })

    eventSources.value.set(instanceId, eventSource)
  }

  /**
   * 根据事件更新实例
   */
  function updateInstanceFromEvent(instance: WorkflowInstance, event: WorkflowEvent) {
    switch (event.eventType) {
      case 'started':
        instance.status = 'Running'
        instance.progress = 10
        break
      case 'node_entered':
        instance.currentNodeId = event.nodeId
        break
      case 'node_completed':
        instance.progress = Math.min(instance.progress + 20, 90)
        break
      case 'node_failed':
        instance.status = 'Failed'
        instance.errorMessage = event.message
        break
      case 'progress_updated':
        // 可以从 event.dataJson 中解析更精确的进度
        break
      case 'paused':
        instance.status = 'Paused'
        break
      case 'resumed':
        instance.status = 'Running'
        break
      case 'completed':
        instance.status = 'Completed'
        instance.progress = 100
        break
      case 'failed':
        instance.status = 'Failed'
        instance.errorMessage = event.message
        break
      case 'terminated':
        instance.status = 'Terminated'
        break
    }
  }

  /**
   * 取消事件订阅
   */
  function unsubscribeFromEvents(instanceId: string) {
    const eventSource = eventSources.value.get(instanceId)
    if (eventSource) {
      eventSource.close()
      eventSources.value.delete(instanceId)
    }
  }

  /**
   * 清理所有订阅
   */
  function cleanup() {
    eventSources.value.forEach(source => source.close())
    eventSources.value.clear()
  }

  return {
    // State
    currentRepo,
    tasks,
    agents,
    workflows,
    currentWorkflow,
    workflowNodes,
    workflowEdges,
    instances,
    currentInstance,
    loading,
    error,

    // Getters
    tasksByStatus,
    idleAgents,
    runningInstances,
    failedInstances,
    runningInstance,
    hasWorkflows,
    hasInstances,

    // Actions
    loadTasks,
    createTask,
    updateTaskStatus,
    loadAgents,
    assignTaskToAgent,
    loadWorkflows,
    loadRecentWorkflows,
    loadWorkflow,
    createWorkflow,
    updateWorkflow,
    deleteWorkflow,
    addWorkflowNode,
    addWorkflowEdge,
    executeWorkflow,
    loadWorkflowInstances,
    loadRecentInstances,
    loadInstance,
    pauseWorkflowInstance,
    resumeWorkflowInstance,
    terminateWorkflowInstance,
    pauseWorkflow,
    resumeWorkflow,
    terminateWorkflow,
    deleteWorkflowInstance,
    loadInstanceEvents,
    subscribeToInstanceEvents,
    unsubscribeFromEvents,
    cleanup
  }
})
