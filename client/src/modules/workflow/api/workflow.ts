/**
 * Workflow API 层封装
 * 基于 Microsoft Agent Framework 编排体系设计
 * 对接后端 /api/v1/workflows 和 /api/v1/workflow-instances
 */

import axios from 'axios'
import type {
  WorkflowDefinition,
  WorkflowInstance,
  CreateWorkflowRequest,
  UpdateWorkflowRequest,
  ExecuteWorkflowRequest,
  WorkflowEvent
} from '../types/workflow'

const api = axios.create({
  baseURL: '/api/v1',
  headers: {
    'Content-Type': 'application/json'
  }
})

/**
 * 工作流 API 服务
 */
export const workflowApi = {
  // ========== Workflow Definition ==========

  /**
   * 获取工作流列表（分页）
   */
  async getWorkflows(page = 1, pageSize = 20): Promise<{ items: WorkflowDefinition[]; totalCount: number }> {
    const response = await api.get('/workflows', { params: { page, pageSize } })
    return response.data.data
  },

  /**
   * 获取最近工作流
   */
  async getRecentWorkflows(take = 10): Promise<WorkflowDefinition[]> {
    const response = await api.get('/workflows/recent', { params: { take } })
    return response.data.data
  },

  /**
   * 获取工作流详情
   */
  async getWorkflow(id: string): Promise<WorkflowDefinition> {
    const response = await api.get(`/workflows/${id}`)
    return response.data.data
  },

  /**
   * 创建工作流
   */
  async createWorkflow(data: CreateWorkflowRequest): Promise<WorkflowDefinition> {
    const response = await api.post('/workflows', data)
    return response.data.data
  },

  /**
   * 更新工作流
   */
  async updateWorkflow(id: string, data: UpdateWorkflowRequest): Promise<WorkflowDefinition> {
    const response = await api.put(`/workflows/${id}`, data)
    return response.data.data
  },

  /**
   * 删除工作流
   */
  async deleteWorkflow(id: string): Promise<void> {
    await api.delete(`/workflows/${id}`)
  },

  /**
   * 执行工作流
   */
  async executeWorkflow(id: string, input: ExecuteWorkflowRequest): Promise<WorkflowInstance> {
    const response = await api.post(`/workflows/${id}/execute`, input)
    return response.data.data
  },

  /**
   * 获取工作流的所有实例
   */
  async getWorkflowInstances(workflowId: string): Promise<WorkflowInstance[]> {
    const response = await api.get(`/workflows/${workflowId}/instances`)
    return response.data.data
  },

  // ========== Workflow Instance ==========

  /**
   * 获取最近实例
   */
  async getRecentInstances(take = 10): Promise<WorkflowInstance[]> {
    const response = await api.get('/workflow-instances/recent', { params: { take } })
    return response.data.data
  },

  /**
   * 获取实例详情
   */
  async getWorkflowInstance(instanceId: string): Promise<WorkflowInstance> {
    const response = await api.get(`/workflow-instances/${instanceId}`)
    return response.data.data
  },

  /**
   * 暂停实例
   */
  async pauseWorkflow(instanceId: string): Promise<void> {
    await api.post(`/workflow-instances/${instanceId}/pause`)
  },

  /**
   * 恢复实例
   */
  async resumeWorkflow(instanceId: string): Promise<void> {
    await api.post(`/workflow-instances/${instanceId}/resume`)
  },

  /**
   * 终止实例
   */
  async terminateWorkflow(instanceId: string): Promise<void> {
    await api.post(`/workflow-instances/${instanceId}/terminate`)
  },

  /**
   * 删除实例
   */
  async deleteInstance(instanceId: string): Promise<void> {
    await api.delete(`/workflow-instances/${instanceId}`)
  },

  /**
   * 获取实例事件列表
   */
  async getInstanceEvents(instanceId: string): Promise<WorkflowEvent[]> {
    const response = await api.get(`/workflow-instances/${instanceId}/events`)
    return response.data.data
  },

  // ========== SSE Event Stream ==========

  /**
   * 订阅实例实时事件（SSE）
   * @param instanceId 实例ID
   * @param callback 事件回调
   * @returns EventSource 实例，需在组件卸载时调用 close()
   */
  subscribeToEvents(instanceId: string, callback: (event: WorkflowEvent) => void): EventSource {
    const eventSource = new EventSource(`/api/v1/workflow-instances/${instanceId}/events/stream`)

    eventSource.onmessage = (event) => {
      try {
        const workflowEvent = JSON.parse(event.data) as WorkflowEvent
        callback(workflowEvent)
      } catch (e) {
        console.error('Failed to parse workflow event:', e)
      }
    }

    eventSource.onerror = (error) => {
      console.error('Workflow SSE error:', error)
    }

    return eventSource
  }
}
