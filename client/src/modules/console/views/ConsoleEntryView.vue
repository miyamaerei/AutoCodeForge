<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useConsoleStore } from '../store/useConsoleStore'
import { useWorkflowCenterStore } from '../../workflow-center/store/useWorkflowCenterStore'

const router = useRouter()
const store = useConsoleStore()
const workflowStore = useWorkflowCenterStore()
const { stats, recentTasks, loading, error, hasData } = storeToRefs(store)
const {
  stats: workflowStats,
  filteredNotifications,
  filteredApprovals,
  filteredHumanLoopCases,
} = storeToRefs(workflowStore)
const hasRecentTasks = computed(() => recentTasks.value.length > 0)
const pendingOpsNotifications = computed(
  () => filteredNotifications.value.filter((item) => item.priority === 'P0' && item.state === '待处理').length,
)

// 功能模块
const features = ref([
  {
    id: 'session',
    icon: '💬',
    title: 'Session',
    desc: '查看历史对话记录，继续上次的任务或对话',
    color: '#667eea',
    route: '/session',
  },
  {
    id: 'ask',
    icon: '❓',
    title: 'Ask',
    desc: '直接向AI提问，获取编程建议和帮助',
    color: '#f093fb',
    route: '/ask',
  },
  {
    id: 'wiki',
    icon: '📚',
    title: 'Wiki',
    desc: '查看和管理仓库Wiki文档',
    color: '#4facfe',
    route: '/wiki',
  },
  {
    id: 'review',
    icon: '👁️',
    title: 'Review',
    desc: '代码审查和PR管理',
    color: '#fa709a',
    route: '/review',
  },
  {
    id: 'automations',
    icon: '⚙️',
    title: 'Automations',
    desc: '创建和管理自动化任务',
    color: '#30b0fe',
    route: '/automations',
  },
  {
    id: 'workflow-ops',
    icon: '🛟',
    title: 'Workflow OPS',
    desc: '查看通知、审批与人工确认队列',
    color: '#34d399',
    route: '/workflow-center/ops',
  },
])

onMounted(async () => {
  if (!hasData.value) {
    await store.fetchConsoleData()
  }
  await workflowStore.loadWorkflowData()
})

const navigateTo = (route: string) => {
  router.push(route)
}
</script>

<template>
  <section class="console-entry">
    <el-skeleton :rows="4" animated v-if="loading" class="skeleton" />
    <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />

    <div v-else class="desktop-shell">
      <main class="center-panel">
        <!-- 欢迎信息 -->
        <div class="welcome-section">
          <h2>欢迎使用 AutoCodeForge</h2>
          <p>AI 自主编程助手平台 - 让AI帮助你完成编程任务</p>
        </div>

        <!-- 统计信息 -->
        <div class="stats-grid">
          <div class="stat-card">
            <div class="stat-icon">📊</div>
            <div class="stat-content">
              <div class="stat-label">今日任务</div>
              <div class="stat-value">{{ stats?.tasksToday ?? 0 }}</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">✅</div>
            <div class="stat-content">
              <div class="stat-label">成功率</div>
              <div class="stat-value">{{ stats?.successRate ?? 0 }}%</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">⚡</div>
            <div class="stat-content">
              <div class="stat-label">执行中</div>
              <div class="stat-value">{{ stats?.runningCount ?? 0 }}</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">⚠️</div>
            <div class="stat-content">
              <div class="stat-label">告警数</div>
              <div class="stat-value">{{ stats?.alertCount ?? 0 }}</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">🧑‍⚖️</div>
            <div class="stat-content">
              <div class="stat-label">待人工审批</div>
              <div class="stat-value">{{ workflowStats?.approvalPending ?? 0 }}</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon">🧭</div>
            <div class="stat-content">
              <div class="stat-label">待人工介入</div>
              <div class="stat-value">{{ workflowStats?.humanLoopOpen ?? 0 }}</div>
            </div>
          </div>
        </div>

        <!-- 主要功能块 -->
        <div class="features-section">
          <h3>主要功能</h3>
          <div class="features-grid">
            <div
              v-for="feature in features"
              :key="feature.id"
              class="feature-card"
              :style="{ '--feature-color': feature.color }"
              @click="navigateTo(feature.route)"
            >
              <div class="feature-icon">{{ feature.icon }}</div>
              <h4>{{ feature.title }}</h4>
              <p>{{ feature.desc }}</p>
              <div class="feature-arrow">→</div>
            </div>
          </div>
        </div>
      </main>

      <!-- 右侧面板 -->
      <aside class="right-panel">
        <!-- 最近任务 -->
        <div class="panel-section">
          <h4>最近任务</h4>
          <template v-if="hasRecentTasks">
            <div v-for="task in recentTasks.slice(0, 3)" :key="task.id" class="task-item">
              <div class="task-header">
                <strong>{{ task.name }}</strong>
                <span class="task-status">{{ task.state }}</span>
              </div>
              <div class="task-progress">
                <el-progress :percentage="task.percent" :stroke-width="6" />
              </div>
            </div>
          </template>
          <div v-else class="empty-state">暂无任务</div>
        </div>

        <!-- 快速操作 -->
        <div class="panel-section">
          <h4>快速操作</h4>
          <div class="quick-actions">
            <button class="action-btn" @click="navigateTo('/task-center/create')">
              <span>✨ 新建任务</span>
            </button>
            <button class="action-btn" @click="navigateTo('/ask')">
              <span>💡 提问</span>
            </button>
            <button class="action-btn" @click="navigateTo('/session')">
              <span>📝 历史记录</span>
            </button>
            <button class="action-btn" @click="navigateTo('/workflow-center/ops')">
              <span>🛟 进入OPS运营台</span>
            </button>
          </div>
        </div>

        <div class="panel-section">
          <h4>OPS 待确认</h4>
          <div class="ops-summary">
            <div class="ops-line">
              <span>待审批</span>
              <strong>{{ filteredApprovals.filter((item) => item.status !== '已完成').length }}</strong>
            </div>
            <div class="ops-line">
              <span>待人工介入</span>
              <strong>{{ filteredHumanLoopCases.filter((item) => item.status !== '已闭环').length }}</strong>
            </div>
            <div class="ops-line ops-line-danger">
              <span>P0待处理通知</span>
              <strong>{{ pendingOpsNotifications }}</strong>
            </div>
          </div>
        </div>
      </aside>
    </div>
  </section>
</template>

<style scoped>
.console-entry {
  width: 100%;
  min-height: 100%;
  margin-top: 0;
  overflow-x: auto;
}

.skeleton {
  padding: 1.5rem 0.5rem;
}

.desktop-shell {
  display: grid;
  grid-template-columns: minmax(760px, 1fr) 300px;
  gap: 24px;
  width: 100%;
  min-height: 100%;
  min-width: 1280px;
  padding: 24px;
}

.center-panel {
  min-width: 0;
}

.welcome-section {
  margin-bottom: 32px;
}

.welcome-section h2 {
  font-size: 28px;
  font-weight: 700;
  color: #0f172a;
  margin: 0 0 8px;
  letter-spacing: -0.5px;
}

.welcome-section p {
  font-size: 16px;
  color: #64748b;
  margin: 0;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 16px;
  margin-bottom: 32px;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.stat-card:hover {
  border-color: #cbd5e1;
  box-shadow: 0 4px 12px rgba(15, 23, 42, 0.08);
  transform: translateY(-2px);
}

.stat-icon {
  font-size: 24px;
}

.stat-content {
  display: flex;
  flex-direction: column;
}

.stat-label {
  font-size: 12px;
  color: #64748b;
  font-weight: 500;
}

.stat-value {
  font-size: 24px;
  font-weight: 700;
  color: #0f172a;
}

.features-section {
  margin-bottom: 24px;
}

.features-section h3 {
  font-size: 20px;
  font-weight: 700;
  color: #0f172a;
  margin: 0 0 16px;
}

.features-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
}

.feature-card {
  position: relative;
  padding: 24px;
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  border: 1px solid #e2e8f0;
  border-radius: 14px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  overflow: hidden;
}

.feature-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: var(--feature-color, #667eea);
}

.feature-card:hover {
  border-color: var(--feature-color, #667eea);
  box-shadow: 0 8px 24px rgba(102, 126, 234, 0.15);
  transform: translateY(-4px);
}

.feature-icon {
  font-size: 40px;
  margin-bottom: 12px;
}

.feature-card h4 {
  font-size: 16px;
  font-weight: 700;
  color: #0f172a;
  margin: 0 0 8px;
}

.feature-card p {
  font-size: 13px;
  color: #64748b;
  margin: 0;
  line-height: 1.5;
}

.feature-arrow {
  position: absolute;
  bottom: 16px;
  right: 16px;
  font-size: 24px;
  color: var(--feature-color, #667eea);
  opacity: 0;
  transform: translateX(-8px);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.feature-card:hover .feature-arrow {
  opacity: 1;
  transform: translateX(0);
}

.right-panel {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.panel-section {
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  padding: 16px;
}

.panel-section h4 {
  font-size: 14px;
  font-weight: 700;
  color: #0f172a;
  margin: 0 0 12px;
}

.task-item {
  margin-bottom: 12px;
  padding: 12px;
  background: white;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
}

.task-item:last-child {
  margin-bottom: 0;
}

.task-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.task-header strong {
  font-size: 13px;
  color: #0f172a;
  flex: 1;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.task-status {
  font-size: 11px;
  color: #ffffff;
  background: #667eea;
  padding: 2px 8px;
  border-radius: 12px;
  font-weight: 600;
  margin-left: 8px;
  white-space: nowrap;
}

.task-progress :deep(.el-progress__bar) {
  background-color: #667eea;
}

.empty-state {
  font-size: 13px;
  color: #94a3b8;
  text-align: center;
  padding: 20px 0;
}

.quick-actions {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.ops-summary {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.ops-line {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 13px;
  color: #334155;
}

.ops-line strong {
  font-size: 15px;
  color: #0f172a;
}

.ops-line-danger strong {
  color: #b91c1c;
}

.action-btn {
  padding: 12px 16px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  font-size: 13px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.action-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 16px rgba(102, 126, 234, 0.4);
}

.action-btn span {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
}
</style>
