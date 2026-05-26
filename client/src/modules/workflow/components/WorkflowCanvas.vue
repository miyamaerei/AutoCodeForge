/**
 * 工作流设计器画布组件
 * 基于 Vue Flow 实现工作流可视化编排
 * 支持与后端 API 的完整对接
 */
<template>
  <div class="workflow-canvas">
    <!-- 工具栏 -->
    <div class="canvas-toolbar">
      <div class="toolbar-left">
        <span class="toolbar-title">工作流设计器</span>
        <span v-if="workflowId" class="workflow-id">ID: {{ workflowId }}</span>
      </div>
      <div class="toolbar-center">
        <!-- 拖拽添加节点 -->
        <div class="node-palette">
          <div
            v-for="nodeType in nodeTypes"
            :key="nodeType.type"
            class="palette-item"
            draggable="true"
            @dragstart="onDragStart($event, nodeType)"
          >
            <span class="node-icon">{{ nodeType.icon }}</span>
            <span class="node-label">{{ nodeType.label }}</span>
          </div>
        </div>
      </div>
      <div class="toolbar-right">
        <button class="btn" @click="onSave">保存</button>
        <button class="btn" @click="onLoad">加载</button>
        <button class="btn" @click="onClear">清空</button>
        <button class="btn btn-primary" @click="onExecute">执行</button>
        <button 
          v-if="isRunning" 
          class="btn btn-warning" 
          @click="onPause"
        >暂停</button>
        <button 
          v-if="isPaused" 
          class="btn btn-success" 
          @click="onResume"
        >恢复</button>
        <button 
          v-if="isRunning || isPaused" 
          class="btn btn-danger" 
          @click="onTerminate"
        >终止</button>
      </div>
    </div>

    <!-- Vue Flow 画布 -->
    <div
      class="canvas-container"
      @drop="onDrop"
      @dragover.prevent
    >
      <VueFlow
        v-model:nodes="nodes"
        v-model:edges="edges"
        :nodes-draggable="true"
        :nodes-connectable="true"
        :elements-selectable="true"
        :default-viewport="{ x: 100, y: 100, zoom: 1 }"
        fit-view-on-init
        @node-click="onNodeClick"
        @edge-click="onEdgeClick"
        @connect="onConnect"
        @nodes-change="onNodesChange"
        @edges-change="onEdgesChange"
      >
        <!-- 背景网格 -->
        <Background pattern-color="#e5e7eb" :gap="16" />

        <!-- 缩放控制 -->
        <Controls position="bottom-left" />

        <!-- 小地图 -->
        <MiniMap position="bottom-right" />

        <!-- 自定义节点：Start -->
        <template #node-start="{ data }">
          <div class="custom-node start-node" :class="{ active: isNodeActive(data) }">
            <div class="node-icon-wrapper">
              <span class="icon">▶</span>
            </div>
            <div class="node-label">{{ data.label }}</div>
          </div>
        </template>

        <!-- 自定义节点：End -->
        <template #node-end="{ data }">
          <div class="custom-node end-node">
            <div class="node-icon-wrapper">
              <span class="icon">■</span>
            </div>
            <div class="node-label">{{ data.label }}</div>
          </div>
        </template>

        <!-- 自定义节点：Agent -->
        <template #node-agent="{ data }">
          <div class="custom-node agent-node" :class="{ active: isNodeActive(data) }">
            <div class="node-icon-wrapper">
              <span class="icon">🤖</span>
            </div>
            <div class="node-label">{{ data.label }}</div>
            <div class="node-badge">{{ data.agentRole || 'Worker' }}</div>
          </div>
        </template>

        <!-- 自定义节点：Task -->
        <template #node-task="{ data }">
          <div class="custom-node task-node" :class="{ active: isNodeActive(data) }">
            <div class="node-icon-wrapper">
              <span class="icon">📋</span>
            </div>
            <div class="node-label">{{ data.label }}</div>
            <div class="node-status" :class="data.taskStatus">
              {{ data.taskStatus || 'Pending' }}
            </div>
          </div>
        </template>

        <!-- 自定义节点：Condition -->
        <template #node-condition="{ data }">
          <div class="custom-node condition-node" :class="{ active: isNodeActive(data) }">
            <div class="node-icon-wrapper">
              <span class="icon">🔀</span>
            </div>
            <div class="node-label">{{ data.label }}</div>
            <div class="condition-branches">
              <span class="branch true">是</span>
              <span class="branch false">否</span>
            </div>
          </div>
        </template>
      </VueFlow>
    </div>

    <!-- 选中节点配置面板 -->
    <Transition name="slide">
      <div v-if="selectedNode" class="config-panel">
        <div class="panel-header">
          <h3>节点配置</h3>
          <button class="close-btn" @click="selectedNode = null">×</button>
        </div>
        <div class="panel-body">
          <div class="form-group">
            <label>名称</label>
            <input
              v-model="selectedNode.data.label"
              type="text"
              placeholder="节点名称"
              @input="onNodeUpdate"
            />
          </div>

          <!-- Agent 节点特有配置 -->
          <div v-if="selectedNode.data.nodeType === 'agent'" class="form-group">
            <label>角色</label>
            <select v-model="selectedNode.data.agentRole" @change="onNodeUpdate">
              <option value="Worker">Worker</option>
              <option value="Manager">Manager</option>
              <option value="Secretary">Secretary</option>
            </select>
          </div>

          <!-- Task 节点特有配置 -->
          <div v-if="selectedNode.data.nodeType === 'task'" class="form-group">
            <label>状态</label>
            <select v-model="selectedNode.data.taskStatus" @change="onNodeUpdate">
              <option value="Pending">待处理</option>
              <option value="Running">进行中</option>
              <option value="Completed">已完成</option>
            </select>
          </div>

          <!-- Condition 节点特有配置 -->
          <div v-if="selectedNode.data.nodeType === 'condition'" class="form-group">
            <label>条件表达式</label>
            <input
              v-model="selectedNode.data.condition"
              type="text"
              placeholder="e.g., output.hasIssues === true"
              @input="onNodeUpdate"
            />
          </div>

          <div class="form-actions">
            <button class="btn btn-danger" @click="onDeleteNode">删除</button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- 执行状态指示器 -->
    <div v-if="isRunning" class="execution-indicator">
      <div class="spinner"></div>
      <span>执行中: {{ currentExecutingNode }}</span>
      <span class="progress">{{ workflowStore.currentInstance?.progress || 0 }}%</span>
    </div>

    <!-- 执行结果弹窗 -->
    <Transition name="fade">
      <div v-if="showResult" class="result-modal">
        <div class="modal-content">
          <div class="modal-header">
            <h3>执行结果</h3>
            <button class="close-btn" @click="showResult = false">×</button>
          </div>
          <div class="modal-body">
            <div class="result-status" :class="executionResult?.status">
              <span class="result-icon">{{ getResultIcon(executionResult?.status) }}</span>
              <span class="result-text">{{ executionResult?.message }}</span>
            </div>
            <div v-if="executionResult?.output" class="result-output">
              <h4>输出结果</h4>
              <pre>{{ JSON.stringify(executionResult.output, null, 2) }}</pre>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { VueFlow, useVueFlow, MarkerType } from '@vue-flow/core'
import { Background } from '@vue-flow/background'
import { Controls } from '@vue-flow/controls'
import { MiniMap } from '@vue-flow/minimap'
import '@vue-flow/core/dist/style.css'
import '@vue-flow/core/dist/theme-default.css'
import '@vue-flow/controls/dist/style.css'
import '@vue-flow/minimap/dist/style.css'

// ============================================
// Store & API
// ============================================
import { useWorkflowStore } from '../store/useWorkflowStore'
import type { WorkflowNode, WorkflowEdge, WorkflowDefinition, ExecuteWorkflowRequest } from '../types/workflow'

const workflowStore = useWorkflowStore()
const { onConnect: onFlowConnect, screenToFlowCoordinate } = useVueFlow()

// ============================================
// 类型定义
// ============================================

interface NodeType {
  type: string
  label: string
  icon: string
}

interface CustomNodeData {
  label: string
  nodeType: 'agent' | 'task' | 'condition' | 'start' | 'end'
  agentRole?: 'Worker' | 'Manager' | 'Secretary'
  taskStatus?: string
  condition?: string
}

interface ExecutionResult {
  status: 'success' | 'error' | 'terminated'
  message: string
  output?: unknown
}

// ============================================
// 常量
// ============================================

/** 可添加的节点类型 */
const nodeTypes: NodeType[] = [
  { type: 'agent', label: 'Agent', icon: '🤖' },
  { type: 'task', label: 'Task', icon: '📋' },
  { type: 'condition', label: 'Condition', icon: '🔀' },
  { type: 'start', label: 'Start', icon: '▶' },
  { type: 'end', label: 'End', icon: '■' }
]

// ============================================
// State
// ============================================

/** 工作流 ID */
const workflowId = ref('')

/** 工作流节点 */
const nodes = ref<WorkflowNode[]>([])

/** 工作流边 */
const edges = ref<WorkflowEdge[]>([])

/** 选中的节点 */
const selectedNode = ref<WorkflowNode | null>(null)

/** 当前执行中的节点 */
const currentExecutingNode = ref('')

/** 是否正在执行 */
const isRunning = computed(() => workflowStore.currentInstance?.status === 'Running')

/** 是否暂停 */
const isPaused = computed(() => workflowStore.currentInstance?.status === 'Paused')

/** 是否显示结果弹窗 */
const showResult = ref(false)

/** 执行结果 */
const executionResult = ref<ExecutionResult | null>(null)

// ============================================
// 监听 Store 变化
// ============================================

// 监听工作流实例状态变化
watch(
  () => workflowStore.currentInstance,
  (instance) => {
    if (instance) {
      currentExecutingNode.value = instance.currentNodeId || ''
      
      // 检查执行是否完成
      if (instance.status === 'Completed') {
        executionResult.value = {
          status: 'success',
          message: '工作流执行完成',
          output: instance.outputJson
        }
        showResult.value = true
      } else if (instance.status === 'Failed') {
        executionResult.value = {
          status: 'error',
          message: instance.errorMessage || '工作流执行失败'
        }
        showResult.value = true
      } else if (instance.status === 'Terminated') {
        executionResult.value = {
          status: 'terminated',
          message: '工作流已终止'
        }
        showResult.value = true
      }
    }
  },
  { deep: true }
)

// ============================================
// 生命周期
// ============================================

onMounted(() => {
  // 加载工作流列表
  workflowStore.loadWorkflows()
})

onUnmounted(() => {
  // 清理事件订阅
  workflowStore.cleanup()
})

// ============================================
// 方法
// ============================================

/**
 * 判断节点是否正在执行
 */
function isNodeActive(data: any): boolean {
  return currentExecutingNode.value === data.label
}

/**
 * 获取结果图标
 */
function getResultIcon(status?: string): string {
  switch (status) {
    case 'success': return '✅'
    case 'error': return '❌'
    case 'terminated': return '⏹️'
    default: return 'ℹ️'
  }
}

/**
 * 节点点击
 */
function onNodeClick(event: any) {
  selectedNode.value = event.node
}

/**
 * 边点击
 */
function onEdgeClick(event: any) {
  console.log('Edge clicked:', event.edge)
}

/**
 * 节点连接
 */
function onConnect(params: any) {
  const newEdge: WorkflowEdge = {
    id: `e-${params.source}-${params.target}`,
    source: params.source,
    target: params.target,
    type: 'default',
    animated: true,
    markerEnd: MarkerType.ArrowClosed
  }
  edges.value.push(newEdge)
  workflowStore.addWorkflowEdge(newEdge)
}

/**
 * 节点变化
 */
function onNodesChange(changes: any[]) {
  changes.forEach((change: any) => {
    if (change.type === 'position' && change.position) {
      const node = nodes.value.find((n) => n.id === change.id)
      if (node) {
        node.position = change.position
      }
    }
  })
}

/**
 * 边变化
 */
function onEdgesChange(changes: any[]) {
  changes.forEach((change: any) => {
    if (change.type === 'remove') {
      edges.value = edges.value.filter((e) => e.id !== change.id)
    }
  })
}

/**
 * 拖拽开始
 */
function onDragStart(event: DragEvent, nodeType: NodeType) {
  event.dataTransfer?.setData('nodeType', nodeType.type)
  event.dataTransfer!.effectAllowed = 'copy'
}

/**
 * 放置节点
 */
function onDrop(event: DragEvent) {
  const nodeType = event.dataTransfer?.getData('nodeType')
  if (!nodeType) return

  // 获取放置位置
  const position = screenToFlowCoordinate({
    x: event.clientX,
    y: event.clientY
  })

  // 创建新节点
  const newNode: WorkflowNode = {
    id: crypto.randomUUID(),
    type: nodeType as string,
    position,
    data: {
      label: `New ${nodeType}`,
      nodeType: nodeType as 'agent' | 'task' | 'condition' | 'start' | 'end',
      ...getDefaultData(nodeType)
    }
  }

  nodes.value.push(newNode)
  workflowStore.addWorkflowNode(newNode)
}

/**
 * 获取节点默认数据
 */
function getDefaultData(nodeType: string): Partial<CustomNodeData> {
  switch (nodeType) {
    case 'agent':
      return { agentRole: 'Worker' }
    case 'task':
      return { taskStatus: 'Pending' }
    case 'condition':
      return { condition: '' }
    default:
      return {}
  }
}

/**
 * 节点更新
 */
function onNodeUpdate() {
  nodes.value = [...nodes.value]
}

/**
 * 删除节点
 */
function onDeleteNode() {
  if (!selectedNode.value) return

  nodes.value = nodes.value.filter((n) => n.id !== selectedNode.value!.id)
  edges.value = edges.value.filter(
    (e) => e.source !== selectedNode.value!.id && e.target !== selectedNode.value!.id
  )

  selectedNode.value = null
}

/**
 * 清空画布
 */
function onClear() {
  nodes.value = []
  edges.value = []
  selectedNode.value = null
  workflowId.value = ''
}

/**
 * 保存工作流到后端
 */
async function onSave() {
  if (nodes.value.length === 0) {
    alert('请先添加节点')
    return
  }

  try {
    // 构建工作流定义
    const executors = nodes.value
      .filter(n => n.type !== 'start' && n.type !== 'end')
      .map(n => ({
        id: n.id,
        type: n.type,
        label: n.data.label,
        config: n.data.config || {}
      }))

    const edgesData = edges.value.map(e => ({
      id: e.id,
      from: e.source,
      to: e.target,
      condition: e.data?.condition ? {
        type: 'expression' as const,
        value: e.data.condition
      } : undefined
    }))

    const workflowData = {
      name: '未命名工作流',
      description: '通过设计器创建的工作流',
      nodesJson: JSON.stringify(executors),
      edgesJson: JSON.stringify(edgesData),
      executorsJson: JSON.stringify(executors),
      contextProviders: []
    }

    if (workflowId.value) {
      // 更新现有工作流
      await workflowStore.updateWorkflow(workflowId.value, workflowData)
      alert('工作流更新成功')
    } else {
      // 创建新工作流
      const created = await workflowStore.createWorkflow(workflowData)
      workflowId.value = created.id
      alert('工作流保存成功')
    }
  } catch (error) {
    console.error('保存工作流失败:', error)
    alert('保存失败，请重试')
  }
}

/**
 * 从后端加载工作流
 */
async function onLoad() {
  try {
    // 获取工作流列表
    await workflowStore.loadWorkflows()
    
    if (workflowStore.workflows.length === 0) {
      alert('暂无工作流')
      return
    }

    // 选择第一个工作流
    const workflow = workflowStore.workflows[0]
    if (!workflow) {
      alert('工作流加载失败')
      return
    }

    workflowId.value = workflow.id
    
    // 解析节点数据
    let executors: any[] = []
    if (workflow.nodesJson) {
      try {
        executors = JSON.parse(workflow.nodesJson)
      } catch (e) {
        console.error('解析 nodesJson 失败:', e)
      }
    } else if (workflow.executorsJson) {
      try {
        executors = JSON.parse(workflow.executorsJson)
      } catch (e) {
        console.error('解析 executorsJson 失败:', e)
      }
    }
    
    // 解析边数据
    let edgesData: any[] = []
    if (workflow.edgesJson) {
      try {
        edgesData = JSON.parse(workflow.edgesJson)
      } catch (e) {
        console.error('解析 edgesJson 失败:', e)
      }
    }
    
    // 转换为设计器格式
    nodes.value = executors.map((executor, index) => ({
      id: executor.id,
      type: executor.type,
      position: { x: 150 + index * 200, y: 100 },
      data: {
        label: executor.label,
        nodeType: executor.type as 'agent' | 'task' | 'condition',
        config: executor.config
      }
    }))
    
    // 添加开始和结束节点
    nodes.value.unshift({
      id: 'start',
      type: 'start',
      position: { x: 50, y: 100 },
      data: { label: 'Start', nodeType: 'start' }
    })
    
    nodes.value.push({
      id: 'end',
      type: 'end',
      position: { x: 350 + (executors.length - 1) * 200, y: 100 },
      data: { label: 'End', nodeType: 'end' }
    })

    edges.value = edgesData.map(e => ({
      id: e.id,
      source: e.from,
      target: e.to,
      type: 'default',
      animated: true,
      markerEnd: MarkerType.ArrowClosed,
      data: e.condition ? { condition: e.condition.value } : undefined
    }))

    alert('工作流加载成功')
  } catch (error) {
    console.error('加载工作流失败:', error)
    alert('加载失败，请重试')
  }
}

/**
 * 执行工作流
 */
async function onExecute() {
  if (!workflowId.value) {
    alert('请先保存工作流')
    return
  }

  try {
    // 执行工作流
    const input: ExecuteWorkflowRequest = {
      type: 'DefaultInput',
      dataJson: JSON.stringify({}),
      context: {
        userId: 'current-user',
        metadata: {
          priority: 'normal'
        }
      }
    }
    
    await workflowStore.executeWorkflow(workflowId.value, input)
  } catch (error) {
    console.error('执行工作流失败:', error)
    alert('执行失败，请重试')
  }
}

/**
 * 暂停工作流
 */
async function onPause() {
  try {
    await workflowStore.pauseWorkflow()
  } catch (error) {
    console.error('暂停工作流失败:', error)
    alert('暂停失败')
  }
}

/**
 * 恢复工作流
 */
async function onResume() {
  try {
    await workflowStore.resumeWorkflow()
  } catch (error) {
    console.error('恢复工作流失败:', error)
    alert('恢复失败')
  }
}

/**
 * 终止工作流
 */
async function onTerminate() {
  if (!confirm('确定要终止工作流吗？')) {
    return
  }
  
  try {
    await workflowStore.terminateWorkflow()
  } catch (error) {
    console.error('终止工作流失败:', error)
    alert('终止失败')
  }
}
</script>

<style scoped>
.workflow-canvas {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 200px);
  min-height: 600px;
  background: var(--color-bg-secondary);
}

.canvas-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  background: var(--color-bg-primary);
  border-bottom: 1px solid var(--color-border);
}

.toolbar-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.toolbar-title {
  font-weight: 600;
  font-size: 16px;
}

.workflow-id {
  font-size: 12px;
  color: var(--color-text-secondary);
  background: var(--color-bg-secondary);
  padding: 2px 8px;
  border-radius: 4px;
}

.toolbar-center {
  flex: 1;
  display: flex;
  justify-content: center;
}

.node-palette {
  display: flex;
  gap: 8px;
}

.palette-item {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 6px 12px;
  background: var(--color-bg-secondary);
  border: 1px solid var(--color-border);
  border-radius: 6px;
  cursor: grab;
  font-size: 13px;
  transition: all 0.2s;
}

.palette-item:hover {
  border-color: var(--color-primary);
  background: var(--color-bg-tertiary);
}

.palette-item:active {
  cursor: grabbing;
}

.toolbar-right {
  display: flex;
  gap: 8px;
}

.btn {
  padding: 6px 16px;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-bg-primary);
  cursor: pointer;
  font-size: 13px;
  transition: all 0.2s;
}

.btn:hover {
  background: var(--color-bg-secondary);
}

.btn-primary {
  background: var(--color-primary);
  border-color: var(--color-primary);
  color: white;
}

.btn-primary:hover {
  opacity: 0.9;
}

.btn-warning {
  background: #f59e0b;
  border-color: #f59e0b;
  color: white;
}

.btn-success {
  background: #22c55e;
  border-color: #22c55e;
  color: white;
}

.btn-danger {
  color: var(--color-error);
  border-color: var(--color-error);
}

.canvas-container {
  flex: 1;
  position: relative;
}

/* Vue Flow 样式覆盖 */
:deep(.vue-flow) {
  background: var(--color-bg-secondary);
}

:deep(.vue-flow__minimap) {
  background: var(--color-bg-primary);
  border-radius: 8px;
}

/* 自定义节点样式 */
.custom-node {
  padding: 12px 16px;
  border-radius: 8px;
  border: 2px solid var(--color-border);
  background: var(--color-bg-primary);
  min-width: 120px;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
}

.custom-node:hover {
  border-color: var(--color-primary);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.custom-node.active {
  animation: pulse-glow 1s infinite;
}

@keyframes pulse-glow {
  0%, 100% {
    box-shadow: 0 0 0 0 rgba(59, 130, 246, 0.4);
  }
  50% {
    box-shadow: 0 0 0 8px rgba(59, 130, 246, 0);
  }
}

.node-icon-wrapper {
  font-size: 24px;
  margin-bottom: 4px;
}

.node-label {
  font-size: 13px;
  font-weight: 500;
}

.node-badge,
.node-status {
  margin-top: 4px;
  font-size: 11px;
  padding: 2px 6px;
  border-radius: 4px;
  background: var(--color-bg-tertiary);
}

.start-node {
  border-color: #22c55e;
  background: #f0fdf4;
}

.start-node .node-icon-wrapper {
  color: #22c55e;
}

.end-node {
  border-color: #ef4444;
  background: #fef2f2;
}

.end-node .node-icon-wrapper {
  color: #ef4444;
}

.agent-node {
  border-color: #8b5cf6;
  background: #f5f3ff;
}

.task-node {
  border-color: #3b82f6;
  background: #eff6ff;
}

.condition-node {
  border-color: #f59e0b;
  background: #fffbeb;
}

.condition-branches {
  display: flex;
  gap: 8px;
  margin-top: 4px;
  font-size: 11px;
}

.branch {
  padding: 2px 6px;
  border-radius: 4px;
}

.branch.true {
  background: #dcfce7;
  color: #16a34a;
}

.branch.false {
  background: #fee2e2;
  color: #dc2626;
}

/* 配置面板 */
.config-panel {
  position: absolute;
  top: 60px;
  right: 16px;
  width: 280px;
  background: var(--color-bg-primary);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  z-index: 100;
}

.panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border);
}

.panel-header h3 {
  margin: 0;
  font-size: 14px;
}

.close-btn {
  width: 24px;
  height: 24px;
  border: none;
  background: transparent;
  font-size: 18px;
  cursor: pointer;
  border-radius: 4px;
}

.close-btn:hover {
  background: var(--color-bg-secondary);
}

.panel-body {
  padding: 16px;
}

.form-group {
  margin-bottom: 12px;
}

.form-group label {
  display: block;
  margin-bottom: 4px;
  font-size: 12px;
  color: var(--color-text-secondary);
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 6px 8px;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  font-size: 13px;
}

.form-group input:focus,
.form-group select:focus {
  outline: none;
  border-color: var(--color-primary);
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 16px;
}

/* 执行状态指示器 */
.execution-indicator {
  position: absolute;
  bottom: 16px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: var(--color-primary);
  color: white;
  border-radius: 20px;
  font-size: 13px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.execution-indicator .progress {
  background: rgba(255, 255, 255, 0.2);
  padding: 2px 8px;
  border-radius: 10px;
}

.spinner {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

/* 结果弹窗 */
.result-modal {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 200;
}

.modal-content {
  background: var(--color-bg-primary);
  border-radius: 8px;
  width: 400px;
  max-width: 90%;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.2);
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px;
  border-bottom: 1px solid var(--color-border);
}

.modal-header h3 {
  margin: 0;
}

.modal-body {
  padding: 16px;
}

.result-status {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  border-radius: 8px;
  margin-bottom: 16px;
}

.result-status.success {
  background: #dcfce7;
  color: #16a34a;
}

.result-status.error {
  background: #fee2e2;
  color: #dc2626;
}

.result-status.terminated {
  background: #f3f4f6;
  color: #6b7280;
}

.result-icon {
  font-size: 24px;
}

.result-text {
  font-size: 14px;
  font-weight: 500;
}

.result-output {
  background: var(--color-bg-secondary);
  border-radius: 8px;
  padding: 12px;
}

.result-output h4 {
  margin: 0 0 8px 0;
  font-size: 13px;
}

.result-output pre {
  margin: 0;
  font-size: 12px;
  overflow-x: auto;
  max-height: 200px;
  overflow-y: auto;
}

/* 过渡动画 */
.slide-enter-active,
.slide-leave-active {
  transition: all 0.2s ease;
}

.slide-enter-from,
.slide-leave-to {
  opacity: 0;
  transform: translateX(20px);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>