/**
 * 极简 Kanban 看板组件
 * 核心概念：任务拖拽流转
 */
<template>
  <div class="kanban-board">
    <!-- Kanban 列 -->
    <div
      v-for="column in columns"
      :key="column.status"
      class="kanban-column"
      @dragover.prevent="onDragOver($event, column.status)"
      @drop="onDrop($event, column.status)"
    >
      <!-- 列头 -->
      <div class="column-header">
        <span class="column-title">{{ column.title }}</span>
        <span class="column-count">{{ (tasksByStatus[column.status] || []).length }}</span>
      </div>

      <!-- 任务卡片 -->
      <div class="column-content">
        <div
          v-for="task in (tasksByStatus[column.status] || [])"
          :key="task.id"
          class="task-card"
          draggable="true"
          @dragstart="onDragStart($event, task)"
          @click="onTaskClick(task)"
        >
          <!-- 优先级标签 -->
          <span
            class="priority-tag"
            :class="task.priority"
          >
            {{ priorityLabels[task.priority] }}
          </span>

          <!-- 任务标题 -->
          <div class="task-title">{{ task.title }}</div>

          <!-- 任务描述（截断） -->
          <div v-if="task.description" class="task-desc">
            {{ truncate(task.description, 60) }}
          </div>

          <!-- 底部信息 -->
          <div class="task-footer">
            <!-- 分配的 Agent -->
            <span v-if="task.agentId" class="agent-badge">
              {{ getAgentName(task.agentId) }}
            </span>

            <!-- 创建时间 -->
            <span class="task-time">
              {{ formatTime(task.createdAt) }}
            </span>
          </div>
        </div>

        <!-- 添加任务按钮 -->
        <button
          class="add-task-btn"
          @click="onAddTask(column.status)"
        >
          + 添加任务
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useWorkflowStore } from '../store/useWorkflowStore'
import type { Task, TaskStatus } from '../types/workflow'

// ============================================
// Store
// ============================================
const workflowStore = useWorkflowStore()

// ============================================
// 常量
// ============================================

/** Kanban 列配置 */
const columns: { status: TaskStatus; title: string }[] = [
  { status: 'todo', title: '待处理' },
  { status: 'in_progress', title: '进行中' },
  { status: 'review', title: '审核中' },
  { status: 'done', title: '已完成' }
]

/** 优先级标签 */
const priorityLabels: Record<string, string> = {
  low: '低',
  medium: '中',
  high: '高'
}

// ============================================
// Computed
// ============================================

/** 按状态分组的任务 */
const tasksByStatus = computed(() => workflowStore.tasksByStatus)

// ============================================
// 方法
// ============================================

/** 获取 Agent 名称 */
function getAgentName(agentId: string): string {
  const agent = workflowStore.agents.find(a => a.id === agentId)
  return agent?.name || '未知'
}

/** 格式化时间 */
function formatTime(date: Date): string {
  const now = new Date()
  const diff = now.getTime() - new Date(date).getTime()
  const minutes = Math.floor(diff / 60000)

  if (minutes < 60) return `${minutes}分钟前`
  if (minutes < 1440) return `${Math.floor(minutes / 60)}小时前`
  return `${Math.floor(minutes / 1440)}天前`
}

/** 截断文本 */
function truncate(text: string, length: number): string {
  return text.length > length ? text.substring(0, length) + '...' : text
}

/** 拖拽开始 */
function onDragStart(event: DragEvent, task: Task) {
  event.dataTransfer?.setData('taskId', task.id)
  event.dataTransfer!.effectAllowed = 'move'
}

/** 拖拽悬停 */
function onDragOver(event: DragEvent, _status: TaskStatus) {
  event.dataTransfer!.dropEffect = 'move'
}

/** 放置任务 */
function onDrop(event: DragEvent, newStatus: TaskStatus) {
  const taskId = event.dataTransfer?.getData('taskId')
  if (taskId) {
    workflowStore.updateTaskStatus(taskId, newStatus)
  }
}

/** 点击任务 */
function onTaskClick(task: Task) {
  // TODO: 打开任务详情
  console.log('Task clicked:', task)
}

/** 添加任务 */
function onAddTask(status: TaskStatus) {
  // TODO: 打开创建任务对话框
  workflowStore.createTask({
    title: '新任务',
    status,
    priority: 'medium'
  })
}
</script>

<style scoped>
.kanban-board {
  display: flex;
  gap: 16px;
  height: 100%;
  padding: 16px;
  overflow-x: auto;
}

.kanban-column {
  flex: 1;
  min-width: 280px;
  max-width: 350px;
  background: var(--color-bg-secondary);
  border-radius: 8px;
  display: flex;
  flex-direction: column;
}

.column-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border);
}

.column-title {
  font-weight: 600;
  font-size: 14px;
}

.column-count {
  background: var(--color-bg-tertiary);
  padding: 2px 8px;
  border-radius: 10px;
  font-size: 12px;
  color: var(--color-text-secondary);
}

.column-content {
  flex: 1;
  padding: 12px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.task-card {
  background: var(--color-bg-primary);
  border: 1px solid var(--color-border);
  border-radius: 6px;
  padding: 12px;
  cursor: grab;
  transition: all 0.2s;
}

.task-card:hover {
  border-color: var(--color-primary);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.task-card:active {
  cursor: grabbing;
}

.priority-tag {
  display: inline-block;
  padding: 2px 6px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 500;
}

.priority-tag.low {
  background: #e3f2fd;
  color: #1976d2;
}

.priority-tag.medium {
  background: #fff3e0;
  color: #f57c00;
}

.priority-tag.high {
  background: #ffebee;
  color: #d32f2f;
}

.task-title {
  margin-top: 8px;
  font-size: 14px;
  font-weight: 500;
  line-height: 1.4;
}

.task-desc {
  margin-top: 4px;
  font-size: 12px;
  color: var(--color-text-secondary);
  line-height: 1.4;
}

.task-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 8px;
  font-size: 11px;
}

.agent-badge {
  background: var(--color-primary);
  color: white;
  padding: 2px 6px;
  border-radius: 4px;
}

.task-time {
  color: var(--color-text-tertiary);
}

.add-task-btn {
  margin-top: 8px;
  padding: 8px;
  border: 1px dashed var(--color-border);
  border-radius: 6px;
  background: transparent;
  color: var(--color-text-secondary);
  cursor: pointer;
  transition: all 0.2s;
}

.add-task-btn:hover {
  border-color: var(--color-primary);
  color: var(--color-primary);
}
</style>
