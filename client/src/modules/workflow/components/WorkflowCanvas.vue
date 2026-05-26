/**
 * 极简工作流画布组件
 * 基于 Vue Flow 实现工作流可视化
 */
<template>
  <div class="workflow-canvas">
    <!-- 工具栏 -->
    <div class="canvas-toolbar">
      <div class="toolbar-left">
        <span class="toolbar-title">工作流设计器</span>
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
        <button class="btn" @click="onClear">清空</button>
        <button class="btn btn-primary" @click="onExecute">执行</button>
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
          <div class="custom-node start-node">
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
          <div class="custom-node agent-node">
            <div class="node-icon-wrapper">
              <span class="icon">🤖</span>
            </div>
            <div class="node-label">{{ data.label }}</div>
            <div class="node-badge">{{ data.agentRole || 'Worker' }}</div>
          </div>
        </template>

        <!-- 自定义节点：Task -->
        <template #node-task="{ data }">
          <div class="custom-node task-node">
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
          <div class="custom-node condition-node">
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
              placeholder="e.g., task.status === 'completed'"
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
      <span>执行中: {{ currentNode }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
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
import type { TaskStatus } from '../store/useWorkflowStore'

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

/** 工作流节点 */
const nodes = ref<any[]>([])

/** 工作流边 */
const edges = ref<any[]>([])

/** 选中的节点 */
const selectedNode = ref<any>(null)

/** 当前执行中的节点 */
const currentNode = ref('')

/** 是否正在执行 */
const isRunning = computed(() => workflowStore.runningInstance !== null)

// ============================================
// 监听 Store 变化
// ============================================

// 同步 Store 中的节点数据
watch(
  () => workflowStore.workflowNodes,
  (storeNodes) => {
    if (storeNodes.length > 0 && nodes.value.length === 0) {
      nodes.value = storeNodes
    }
  },
  { immediate: true }
)

// 同步 Store 中的边数据
watch(
  () => workflowStore.workflowEdges,
  (storeEdges) => {
    if (storeEdges.length > 0 && edges.value.length === 0) {
      edges.value = storeEdges
    }
  },
  { immediate: true }
)

// ============================================
// 方法
// ============================================

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
  // TODO: 打开边配置
  console.log('Edge clicked:', event.edge)
}

/**
 * 节点连接
 */
function onConnect(params: any) {
  const newEdge = {
    ...params,
    id: `e-${params.source}-${params.target}`,
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
  // 处理节点位置变化
  changes.forEach((change) => {
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
  // 处理边删除
  changes.forEach((change) => {
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
  const newNode = {
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
  // 触发响应式更新
  nodes.value = [...nodes.value]
}

/**
 * 删除节点
 */
function onDeleteNode() {
  if (!selectedNode.value) return

  // 从数组中移除
  nodes.value = nodes.value.filter((n) => n.id !== selectedNode.value.id)
  // 移除相关的边
  edges.value = edges.value.filter(
    (e) => e.source !== selectedNode.value.id && e.target !== selectedNode.value.id
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
}

/**
 * 执行工作流
 */
async function onExecute() {
  if (nodes.value.length === 0) {
    alert('请先添加节点')
    return
  }

  // 保存当前设计
  workflowStore.workflowNodes = nodes.value
  workflowStore.workflowEdges = edges.value

  // 执行工作流
  await workflowStore.executeWorkflow('default')

  // 模拟执行过程
  simulateExecution()
}

/**
 * 模拟执行过程
 */
async function simulateExecution() {
  const startNode = nodes.value.find((n) => n.type === 'start')
  if (!startNode) return

  let current = startNode

  while (current && current.type !== 'end') {
    currentNode.value = current.data.label

    // 模拟执行延迟
    await new Promise((resolve) => setTimeout(resolve, 1000))

    // 找到下一个节点
    const edge = edges.value.find((e) => e.source === current.id)
    if (!edge) break

    current = nodes.value.find((n) => n.id === edge.target)
  }

  currentNode.value = ''
}
</script>

<style scoped>
.workflow-canvas {
  display: flex;
  flex-direction: column;
  height: 100%;
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

.toolbar-title {
  font-weight: 600;
  font-size: 16px;
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
}

.custom-node:hover {
  border-color: var(--color-primary);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
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
</style>
