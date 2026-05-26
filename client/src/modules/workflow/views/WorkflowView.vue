/**
 * 极简工作流视图
 * 整合工作流设计器 + Kanban 看板
 */
<template>
  <div class="workflow-view">
    <!-- 顶部导航 -->
    <div class="view-header">
      <div class="header-left">
        <h1 class="view-title">工作流中心</h1>
        <span class="view-subtitle">Agent 驱动的任务编排</span>
      </div>

      <div class="header-center">
        <!-- 视图切换标签 -->
        <div class="view-tabs">
          <button
            v-for="tab in tabs"
            :key="tab.id"
            class="tab-btn"
            :class="{ active: currentView === tab.id }"
            @click="currentView = tab.id"
          >
            <span class="tab-icon">{{ tab.icon }}</span>
            <span class="tab-label">{{ tab.label }}</span>
          </button>
        </div>
      </div>

      <div class="header-right">
        <!-- 当前 Repo -->
        <div v-if="workflowStore.currentRepo" class="repo-badge">
          <span class="repo-icon">📁</span>
          <span class="repo-name">{{ workflowStore.currentRepo.name }}</span>
        </div>

        <!-- 设置按钮 -->
        <button class="icon-btn" title="设置">
          ⚙️
        </button>
      </div>
    </div>

    <!-- 主内容区 -->
    <div class="view-content">
      <!-- 工作流设计器视图 -->
      <div v-show="currentView === 'designer'" class="view-panel">
        <WorkflowCanvas />
      </div>

      <!-- Kanban 看板视图 -->
      <div v-show="currentView === 'kanban'" class="view-panel">
        <KanbanBoard />
      </div>

      <!-- Agent 监控视图 -->
      <div v-show="currentView === 'agents'" class="view-panel">
        <AgentMonitor />
      </div>

      <!-- 实时日志视图 -->
      <div v-show="currentView === 'logs'" class="view-panel">
        <RealtimeLogs />
      </div>
    </div>

    <!-- 底部状态栏 -->
    <div class="view-footer">
      <div class="footer-left">
        <span class="status-indicator" :class="connectionStatus"></span>
        <span>{{ statusText }}</span>
      </div>

      <div class="footer-center">
        <span v-if="workflowStore.currentInstance">
          执行进度: {{ workflowStore.currentInstance.progress }}%
        </span>
      </div>

      <div class="footer-right">
        <span>{{ currentTime }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useWorkflowStore } from '../store/useWorkflowStore'
import KanbanBoard from '../components/KanbanBoard.vue'
import WorkflowCanvas from '../components/WorkflowCanvas.vue'

// ============================================
// Store
// ============================================
const workflowStore = useWorkflowStore()

// ============================================
// 常量
// ============================================

/** 视图标签 */
const tabs = [
  { id: 'designer', label: '设计器', icon: '🎨' },
  { id: 'kanban', label: '看板', icon: '📋' },
  { id: 'agents', label: 'Agent', icon: '🤖' },
  { id: 'logs', label: '日志', icon: '📝' }
]

// ============================================
// State
// ============================================

/** 当前视图 */
const currentView = ref('kanban')

/** 连接状态 */
const connectionStatus = ref<'connected' | 'disconnected' | 'connecting'>('connected')

/** 当前时间 */
const currentTime = ref('')

// ============================================
// Computed
// ============================================

/** 状态文本 */
const statusText = computed(() => {
  switch (connectionStatus.value) {
    case 'connected':
      return '已连接'
    case 'connecting':
      return '连接中...'
    case 'disconnected':
      return '未连接'
  }
})

// ============================================
// 组件
// ============================================

/**
 * Agent 监控组件（占位）
 */
const AgentMonitor = {
  template: `
    <div class="agent-monitor">
      <div class="empty-state">
        <span class="empty-icon">🤖</span>
        <h3>Agent 监控</h3>
        <p>展示所有 Agent 的实时状态</p>
        <div class="agent-list">
          <div
            v-for="agent in agents"
            :key="agent.id"
            class="agent-card"
            :class="agent.status"
          >
            <div class="agent-avatar">
              {{ agent.name.charAt(0) }}
            </div>
            <div class="agent-info">
              <div class="agent-name">{{ agent.name }}</div>
              <div class="agent-role">{{ agent.role }}</div>
            </div>
            <div class="agent-status">
              <span class="status-dot"></span>
              {{ agent.status }}
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  setup() {
    const agents = computed(() => workflowStore.agents)
    return { agents }
  }
}

/**
 * 实时日志组件（占位）
 */
const RealtimeLogs = {
  template: `
    <div class="realtime-logs">
      <div class="log-container">
        <div
          v-for="(log, index) in logs"
          :key="index"
          class="log-entry"
          :class="log.type"
        >
          <span class="log-time">{{ log.time }}</span>
          <span class="log-level">{{ log.level }}</span>
          <span class="log-message">{{ log.message }}</span>
        </div>
      </div>
    </div>
  `,
  setup() {
    const logs = ref([
      { time: '10:23:45', level: 'INFO', type: 'info', message: '工作流已启动' },
      { time: '10:23:46', level: 'INFO', type: 'info', message: 'Agent-1 开始处理任务' },
      { time: '10:23:50', level: 'WARN', type: 'warn', message: '检测到代码风格问题' },
      { time: '10:23:55', level: 'INFO', type: 'info', message: '任务完成，等待审核' }
    ])
    return { logs }
  }
}

// ============================================
// 生命周期
// ============================================

/** 定时更新时钟 */
let timeInterval: number | null = null

onMounted(() => {
  // 初始化数据
  workflowStore.loadTasks()
  workflowStore.loadAgents()

  // 更新时钟
  updateTime()
  timeInterval = window.setInterval(updateTime, 1000)
})

onUnmounted(() => {
  // 清理
  workflowStore.cleanup()
  if (timeInterval) {
    clearInterval(timeInterval)
  }
})

// ============================================
// 方法
// ============================================

/**
 * 更新当前时间
 */
function updateTime() {
  const now = new Date()
  currentTime.value = now.toLocaleTimeString('zh-CN', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  })
}
</script>

<style scoped>
.workflow-view {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background: var(--color-bg-secondary);
}

/* 顶部导航 */
.view-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 24px;
  background: var(--color-bg-primary);
  border-bottom: 1px solid var(--color-border);
}

.header-left {
  display: flex;
  align-items: baseline;
  gap: 12px;
}

.view-title {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
}

.view-subtitle {
  font-size: 13px;
  color: var(--color-text-secondary);
}

.header-center {
  flex: 1;
  display: flex;
  justify-content: center;
}

.view-tabs {
  display: flex;
  gap: 4px;
  padding: 4px;
  background: var(--color-bg-secondary);
  border-radius: 8px;
}

.tab-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  background: transparent;
  cursor: pointer;
  font-size: 13px;
  color: var(--color-text-secondary);
  transition: all 0.2s;
}

.tab-btn:hover {
  background: var(--color-bg-tertiary);
  color: var(--color-text-primary);
}

.tab-btn.active {
  background: var(--color-bg-primary);
  color: var(--color-primary);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.tab-icon {
  font-size: 16px;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.repo-badge {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  background: var(--color-bg-secondary);
  border-radius: 6px;
  font-size: 13px;
}

.repo-icon {
  font-size: 14px;
}

.icon-btn {
  width: 36px;
  height: 36px;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  background: var(--color-bg-primary);
  cursor: pointer;
  font-size: 16px;
  transition: all 0.2s;
}

.icon-btn:hover {
  background: var(--color-bg-secondary);
}

/* 主内容区 */
.view-content {
  flex: 1;
  overflow: hidden;
}

.view-panel {
  height: 100%;
}

/* Agent 监控 */
.agent-monitor {
  padding: 24px;
  height: 100%;
  overflow-y: auto;
}

.empty-state {
  text-align: center;
  padding: 48px;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.empty-state h3 {
  margin: 0 0 8px 0;
  font-size: 18px;
}

.empty-state p {
  margin: 0 0 24px 0;
  color: var(--color-text-secondary);
}

.agent-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 16px;
}

.agent-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  background: var(--color-bg-primary);
  border: 1px solid var(--color-border);
  border-radius: 8px;
}

.agent-card.idle {
  border-left: 3px solid #22c55e;
}

.agent-card.busy {
  border-left: 3px solid #f59e0b;
}

.agent-card.offline {
  border-left: 3px solid #6b7280;
}

.agent-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: var(--color-primary);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
}

.agent-info {
  flex: 1;
}

.agent-name {
  font-weight: 500;
}

.agent-role {
  font-size: 12px;
  color: var(--color-text-secondary);
}

.agent-status {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: var(--color-text-secondary);
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: currentColor;
}

.agent-card.idle .status-dot {
  background: #22c55e;
}

.agent-card.busy .status-dot {
  background: #f59e0b;
}

.agent-card.offline .status-dot {
  background: #6b7280;
}

/* 实时日志 */
.realtime-logs {
  padding: 16px;
  height: 100%;
  overflow: hidden;
}

.log-container {
  height: 100%;
  overflow-y: auto;
  background: var(--color-bg-primary);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 12px;
  font-family: 'Monaco', 'Menlo', monospace;
  font-size: 12px;
}

.log-entry {
  display: flex;
  gap: 12px;
  padding: 4px 0;
  border-bottom: 1px solid var(--color-border);
}

.log-entry:last-child {
  border-bottom: none;
}

.log-time {
  color: var(--color-text-tertiary);
}

.log-level {
  font-weight: 600;
  min-width: 50px;
}

.log-entry.info .log-level {
  color: #3b82f6;
}

.log-entry.warn .log-level {
  color: #f59e0b;
}

.log-entry.error .log-level {
  color: #ef4444;
}

.log-message {
  flex: 1;
}

/* 底部状态栏 */
.view-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 24px;
  background: var(--color-bg-primary);
  border-top: 1px solid var(--color-border);
  font-size: 12px;
  color: var(--color-text-secondary);
}

.footer-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.status-indicator {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.status-indicator.connected {
  background: #22c55e;
}

.status-indicator.connecting {
  background: #f59e0b;
  animation: pulse 1s infinite;
}

.status-indicator.disconnected {
  background: #ef4444;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.footer-center {
  font-weight: 500;
  color: var(--color-primary);
}
</style>
