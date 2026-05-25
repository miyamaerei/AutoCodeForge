<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { BarChart, PieChart, LineChart } from 'echarts/charts'
import { GridComponent, LegendComponent, TooltipComponent } from 'echarts/components'
import { dashboardApi, type DashboardOverview, type PipelineStepStat, type SystemMetrics, type AgentResponse, type TaskResponse } from '../api/dashboard.api'
import { DateUtils } from '@/lib/dateUtils'

use([
  CanvasRenderer,
  BarChart,
  PieChart,
  LineChart,
  GridComponent,
  LegendComponent,
  TooltipComponent,
])

const overview = ref<DashboardOverview | null>(null)
const pipelineStats = ref<PipelineStepStat[]>([])
const systemMetrics = ref<SystemMetrics | null>(null)
const agents = ref<AgentResponse[]>([])
const recentTasks = ref<TaskResponse[]>([])
const isFullscreen = ref(false)
const lastUpdateTime = ref('')
let refreshInterval: number | null = null

const pipelineSteps = [
  { key: 'DemandAnalyse', label: '需求分析', color: '#67c23a' },
  { key: 'QueryCurrent', label: '现状查询', color: '#409eff' },
  { key: 'MakePlan', label: '方案制定', color: '#e6a23c' },
  { key: 'Development', label: '开发实施', color: '#f56c6c' },
  { key: 'TestVerify', label: '测试验证', color: '#909399' },
  { key: 'CommitPr', label: '提交PR', color: '#b37feb' },
  { key: 'FinalAudit', label: '最终审核', color: '#73c0de' },
]

const stateColors: Record<string, string> = {
  Idle: '#67c23a',
  Handling: '#409eff',
  Learning: '#e6a23c',
  Dormant: '#909399',
}

const statusColors: Record<string, string> = {
  Pending: '#e6a23c',
  Running: '#409eff',
  Completed: '#67c23a',
  Failed: '#f56c6c',
  Paused: '#909399',
  Canceled: '#666666',
}

const stateLabels: Record<string, string> = {
  Idle: '空闲',
  Handling: '处理中',
  Learning: '学习中',
  Dormant: '休眠',
}

const statusLabels: Record<string, string> = {
  Pending: '待处理',
  Running: '进行中',
  Completed: '已完成',
  Failed: '失败',
  Paused: '已暂停',
  Canceled: '已取消',
}

const agentStatsChartOption = computed(() => ({
  tooltip: { trigger: 'item' },
  series: [
    {
      name: 'Agent状态',
      type: 'pie',
      radius: ['50%', '75%'],
      center: ['50%', '50%'],
      avoidLabelOverlap: false,
      itemStyle: { borderRadius: 8, borderColor: 'rgba(255,255,255,0.1)', borderWidth: 2 },
      label: { show: false },
      emphasis: {
        label: { show: true, fontSize: 24, fontWeight: 'bold', color: '#fff' },
      },
      labelLine: { show: false },
      data: [
        { value: overview.value?.agentStats.idle, name: '空闲', itemStyle: { color: '#67c23a' } },
        { value: overview.value?.agentStats.handling, name: '处理中', itemStyle: { color: '#409eff' } },
        { value: overview.value?.agentStats.learning, name: '学习中', itemStyle: { color: '#e6a23c' } },
        { value: overview.value?.agentStats.dormant, name: '休眠', itemStyle: { color: '#909399' } },
      ],
    },
  ],
}))

const taskStatsChartOption = computed(() => ({
  tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
  grid: { left: '5%', right: '5%', bottom: '5%', top: '10%', containLabel: true },
  xAxis: { type: 'category', data: ['待处理', '进行中', '已完成', '失败', '暂停', '取消'], axisLabel: { color: '#94a3b8', fontSize: 14 } },
  yAxis: { type: 'value', axisLabel: { color: '#94a3b8', fontSize: 14 } },
  series: [
    {
      type: 'bar',
      data: [
        { value: overview.value?.taskStats.pending, itemStyle: { color: '#e6a23c', borderRadius: [4, 4, 0, 0] } },
        { value: overview.value?.taskStats.running, itemStyle: { color: '#409eff', borderRadius: [4, 4, 0, 0] } },
        { value: overview.value?.taskStats.completed, itemStyle: { color: '#67c23a', borderRadius: [4, 4, 0, 0] } },
        { value: overview.value?.taskStats.failed, itemStyle: { color: '#f56c6c', borderRadius: [4, 4, 0, 0] } },
        { value: overview.value?.taskStats.paused, itemStyle: { color: '#909399', borderRadius: [4, 4, 0, 0] } },
        { value: overview.value?.taskStats.canceled, itemStyle: { color: '#666666', borderRadius: [4, 4, 0, 0] } },
      ],
      barWidth: '50%',
    },
  ],
}))

const pipelineHeatmapOption = computed(() => {
  const statsMap = new Map<string, PipelineStepStat>()
  pipelineStats.value.forEach((s: PipelineStepStat) => {
    statsMap.set(s.stepType, s)
  })
  
  const data = pipelineSteps.map((step) => {
    const stat = statsMap.get(step.key)
    const pendingCount = stat?.pending ?? 0
    return {
      name: step.label,
      value: pendingCount,
      itemStyle: {
        color: pendingCount === 0 ? '#67c23a' : pendingCount <= 3 ? '#e6a23c' : '#f56c6c',
      },
    }
  })

  return {
    tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
    grid: { left: '3%', right: '4%', bottom: '3%', top: '10%', containLabel: true },
    xAxis: {
      type: 'category',
      data: pipelineSteps.map((s) => s.label),
      axisLabel: { color: '#94a3b8', fontSize: 14, rotate: 30 },
    },
    yAxis: { type: 'value', name: '堆积任务', nameTextStyle: { color: '#94a3b8' }, axisLabel: { color: '#94a3b8' } },
    series: [
      {
        name: '堆积任务',
        type: 'bar',
        data: data,
        barWidth: '60%',
      },
    ],
  }
})

const uptimeDisplay = computed(() => {
  return DateUtils.formatUpTime(systemMetrics.value?.upTime || null)
})

const successRate = computed(() => {
  if (!overview.value) return 0
  const total = overview.value.taskStats.completed + overview.value.taskStats.failed
  if (total === 0) return 100
  return Math.round((overview.value.taskStats.completed / total) * 100)
})

const gateTypeLabels: Record<string, string> = {
  RequirementConfirm: '需求确认',
  PlanApproval: '方案审批',
  CodeReview: '代码审核',
  TestAcceptance: '测试验收',
  MergeApproval: '合并审批',
  FinalSignoff: '最终签收',
  Emergency: '紧急介入',
}

function getHeatLevel(pending: number): string {
  if (pending === 0) return 'normal'
  if (pending <= 3) return 'warning'
  return 'critical'
}

async function loadData() {
  try {
    const [overviewRes, pipelineRes, metricsRes, agentsRes, tasksRes] = await Promise.all([
      dashboardApi.getOverview(),
      dashboardApi.getPipelineStats(),
      dashboardApi.getSystemMetrics(),
      dashboardApi.getAgentList(),
      dashboardApi.getRecentTasks(),
    ])
    overview.value = overviewRes.data
    pipelineStats.value = pipelineRes.data
    systemMetrics.value = metricsRes.data
    agents.value = agentsRes.data
    recentTasks.value = tasksRes.data
    lastUpdateTime.value = new Date().toLocaleTimeString('zh-CN')
  } catch (error) {
    console.error('Failed to load dashboard data:', error)
  }
}

function toggleFullscreen() {
  try {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen()
      isFullscreen.value = true
    } else {
      document.exitFullscreen()
      isFullscreen.value = false
    }
  } catch {
    isFullscreen.value = !isFullscreen.value
  }
}

function refresh() {
  loadData()
}

onMounted(() => {
  loadData()
  refreshInterval = window.setInterval(loadData, 15000)
})

onUnmounted(() => {
  if (refreshInterval) {
    clearInterval(refreshInterval)
  }
})
</script>

<template>
  <div class="dashboard-screen" :class="{ fullscreen: isFullscreen }">
    <header class="screen-header">
      <div class="header-left">
        <h1 class="title">多Agent协作系统监控大屏</h1>
        <span class="update-time">更新时间: {{ lastUpdateTime }}</span>
      </div>
      <div class="header-right">
        <button class="btn-refresh" @click="refresh">
          <span class="icon">🔄</span> 刷新
        </button>
        <button class="btn-fullscreen" @click="toggleFullscreen">
          <span class="icon">⛶</span> {{ isFullscreen ? '退出全屏' : '全屏' }}
        </button>
      </div>
    </header>

    <main class="screen-main">
      <aside class="left-panel">
        <div class="panel agent-panel">
          <div class="panel-header">
            <h2 class="panel-title">👥 Agent状态</h2>
            <span class="panel-count">共 {{ overview?.agentStats.total || 0 }} 个</span>
          </div>
          <div class="stat-grid">
            <div class="stat-card idle">
              <div class="stat-value">{{ overview?.agentStats.idle || 0 }}</div>
              <div class="stat-label">空闲</div>
            </div>
            <div class="stat-card handling">
              <div class="stat-value">{{ overview?.agentStats.handling || 0 }}</div>
              <div class="stat-label">处理中</div>
            </div>
            <div class="stat-card learning">
              <div class="stat-value">{{ overview?.agentStats.learning || 0 }}</div>
              <div class="stat-label">学习中</div>
            </div>
            <div class="stat-card dormant">
              <div class="stat-value">{{ overview?.agentStats.dormant || 0 }}</div>
              <div class="stat-label">休眠</div>
            </div>
          </div>
          <div class="chart-container">
            <v-chart :option="agentStatsChartOption" autoresize />
          </div>
        </div>

        <div class="panel gate-panel">
          <div class="panel-header">
            <h2 class="panel-title">🚪 门控告警</h2>
            <span 
              class="panel-count" 
              :class="{ warning: (overview?.gateStats.pendingCount ?? 0) > 0 }"
            >
              {{ overview?.gateStats.pendingCount || 0 }} 个待处理
            </span>
          </div>
          <div class="gate-list">
            <div 
              v-for="(count, type) in overview?.gateStats.byType" 
              :key="type"
              class="gate-item"
            >
              <span class="gate-type">{{ gateTypeLabels[type as string] || type }}</span>
              <span class="gate-count-badge">{{ count }}</span>
            </div>
            <div v-if="(!overview?.gateStats.byType || Object.keys(overview.gateStats.byType).length === 0)" class="empty-state">
              暂无待处理门控
            </div>
          </div>
        </div>

        <div class="panel metrics-panel">
          <div class="panel-header">
            <h2 class="panel-title">📊 系统指标</h2>
          </div>
          <div class="metrics-grid">
            <div class="metric-item">
              <div class="metric-icon">🎯</div>
              <div class="metric-info">
                <div class="metric-label">成功率</div>
                <div class="metric-value highlight">{{ successRate }}%</div>
              </div>
            </div>
            <div class="metric-item">
              <div class="metric-icon">⏱️</div>
              <div class="metric-info">
                <div class="metric-label">运行时间</div>
                <div class="metric-value">{{ uptimeDisplay }}</div>
              </div>
            </div>
            <div class="metric-item">
              <div class="metric-icon">🧠</div>
              <div class="metric-info">
                <div class="metric-label">学习时长</div>
                <div class="metric-value">{{ systemMetrics?.totalLearningHours.toFixed(1) || 0 }}h</div>
              </div>
            </div>
            <div class="metric-item">
              <div class="metric-icon">📈</div>
              <div class="metric-info">
                <div class="metric-label">平均负载</div>
                <div class="metric-value">{{ systemMetrics?.averageLoad.toFixed(1) || 0 }}</div>
              </div>
            </div>
          </div>
        </div>
      </aside>

      <section class="center-panel">
        <div class="panel pipeline-panel">
          <div class="panel-header">
            <h2 class="panel-title">🔄 流水线热力图</h2>
          </div>
          <div class="pipeline-flow">
            <div
              v-for="step in pipelineSteps"
              :key="step.key"
              class="pipeline-step"
              :class="getHeatLevel(pipelineStats.find((s: PipelineStepStat) => s.stepType === step.key)?.pending || 0)"
            >
              <div class="step-number">{{ pipelineSteps.indexOf(step) + 1 }}</div>
              <div class="step-label">{{ step.label }}</div>
              <div class="step-count">
                {{ pipelineStats.find((s: PipelineStepStat) => s.stepType === step.key)?.pending || 0 }}
              </div>
            </div>
          </div>
          <div class="chart-container large">
            <v-chart :option="pipelineHeatmapOption" autoresize />
          </div>
        </div>

        <div class="panel task-panel">
          <div class="panel-header">
            <h2 class="panel-title">📋 任务统计</h2>
            <span class="panel-count">共 {{ overview?.taskStats.total || 0 }} 个任务</span>
          </div>
          <div class="task-summary">
            <div class="summary-item pending">
              <span class="summary-value">{{ overview?.taskStats.pending || 0 }}</span>
              <span class="summary-label">待处理</span>
            </div>
            <div class="summary-item running">
              <span class="summary-value">{{ overview?.taskStats.running || 0 }}</span>
              <span class="summary-label">进行中</span>
            </div>
            <div class="summary-item completed">
              <span class="summary-value">{{ overview?.taskStats.completed || 0 }}</span>
              <span class="summary-label">已完成</span>
            </div>
            <div class="summary-item failed">
              <span class="summary-value">{{ overview?.taskStats.failed || 0 }}</span>
              <span class="summary-label">失败</span>
            </div>
            <div class="summary-item paused">
              <span class="summary-value">{{ overview?.taskStats.paused || 0 }}</span>
              <span class="summary-label">暂停</span>
            </div>
            <div class="summary-item canceled">
              <span class="summary-value">{{ overview?.taskStats.canceled || 0 }}</span>
              <span class="summary-label">取消</span>
            </div>
          </div>
          <div class="chart-container">
            <v-chart :option="taskStatsChartOption" autoresize />
          </div>
        </div>
      </section>

      <aside class="right-panel">
        <div class="panel agent-list-panel">
          <div class="panel-header">
            <h2 class="panel-title">👤 Agent列表</h2>
          </div>
          <div class="agent-list">
            <div 
              v-for="agent in agents.slice(0, 6)" 
              :key="agent.id" 
              class="agent-item"
              :style="{ borderLeftColor: stateColors[agent.state] }"
            >
              <div class="agent-status" :style="{ backgroundColor: stateColors[agent.state] }"></div>
              <div class="agent-info">
                <div class="agent-name">{{ agent.name }}</div>
                <div class="agent-role">{{ agent.role }} · {{ stateLabels[agent.state] }}</div>
              </div>
              <div class="agent-extra">
                <span v-if="agent.currentTaskId" class="task-badge">处理中</span>
              </div>
            </div>
            <div v-if="agents.length === 0" class="empty-state">暂无Agent</div>
          </div>
        </div>

        <div class="panel task-list-panel">
          <div class="panel-header">
            <h2 class="panel-title">📝 最近任务</h2>
          </div>
          <div class="task-list">
            <div 
              v-for="task in recentTasks.slice(0, 5)" 
              :key="task.id" 
              class="task-item"
            >
              <div class="task-status" :style="{ backgroundColor: statusColors[task.status] }"></div>
              <div class="task-info">
                <div class="task-title">{{ task.title }}</div>
                <div class="task-meta">
                  {{ statusLabels[task.status] }} · {{ DateUtils.formatDateTime(task.createdAtUtc) }}
                </div>
              </div>
              <div class="task-progress">
                <div class="progress-bar">
                  <div 
                    class="progress-fill" 
                    :style="{ width: task.progress + '%', backgroundColor: statusColors[task.status] }"
                  ></div>
                </div>
                <span class="progress-text">{{ task.progress }}%</span>
              </div>
            </div>
            <div v-if="recentTasks.length === 0" class="empty-state">暂无任务</div>
          </div>
        </div>
      </aside>
    </main>
  </div>
</template>

<style scoped>
.dashboard-screen {
  min-height: 100vh;
  background: linear-gradient(135deg, #0a0e1a 0%, #131b2e 50%, #0a0e1a 100%);
  background-image: 
    radial-gradient(circle at 20% 80%, rgba(64, 158, 255, 0.05) 0%, transparent 50%),
    radial-gradient(circle at 80% 20%, rgba(103, 194, 58, 0.05) 0%, transparent 50%);
  padding: 24px;
  color: #fff;
  transition: all 0.3s ease;
}

.dashboard-screen.fullscreen {
  padding: 16px;
}

.screen-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding: 20px 24px;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 16px;
  border: 1px solid rgba(255, 255, 255, 0.05);
}

.header-left {
  display: flex;
  align-items: center;
  gap: 24px;
}

.title {
  font-size: 32px;
  font-weight: bold;
  background: linear-gradient(90deg, #60a5fa, #a78bfa);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  margin: 0;
}

.update-time {
  font-size: 16px;
  color: #64748b;
}

.header-right {
  display: flex;
  gap: 12px;
}

.btn-refresh, .btn-fullscreen {
  padding: 12px 24px;
  background: rgba(255, 255, 255, 0.08);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 10px;
  color: #fff;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 16px;
  transition: all 0.3s ease;
}

.btn-refresh:hover, .btn-fullscreen:hover {
  background: rgba(255, 255, 255, 0.15);
  border-color: rgba(255, 255, 255, 0.2);
}

.screen-main {
  display: grid;
  grid-template-columns: 320px 1fr 360px;
  gap: 20px;
  height: calc(100vh - 140px);
}

.left-panel, .right-panel {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.center-panel {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.panel {
  background: rgba(255, 255, 255, 0.03);
  border-radius: 16px;
  padding: 20px;
  border: 1px solid rgba(255, 255, 255, 0.05);
  flex-shrink: 0;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 12px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.05);
}

.panel-title {
  font-size: 20px;
  font-weight: 600;
  margin: 0;
  color: #f1f5f9;
}

.panel-count {
  font-size: 14px;
  padding: 6px 14px;
  background: rgba(255, 255, 255, 0.08);
  border-radius: 20px;
  color: #94a3b8;
}

.panel-count.warning {
  background: rgba(245, 108, 108, 0.2);
  color: #f56c6c;
}

.stat-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
  margin-bottom: 16px;
}

.stat-card {
  background: rgba(255, 255, 255, 0.03);
  border-radius: 12px;
  padding: 16px;
  text-align: center;
  border: 1px solid rgba(255, 255, 255, 0.05);
}

.stat-card.idle { border-left: 3px solid #67c23a; }
.stat-card.handling { border-left: 3px solid #409eff; }
.stat-card.learning { border-left: 3px solid #e6a23c; }
.stat-card.dormant { border-left: 3px solid #909399; }

.stat-value {
  font-size: 36px;
  font-weight: bold;
  color: #fff;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 14px;
  color: #94a3b8;
}

.chart-container {
  height: 180px;
}

.chart-container.large {
  height: 220px;
}

.gate-list {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.gate-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 14px;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 20px;
  border: 1px solid rgba(255, 255, 255, 0.05);
}

.gate-type {
  font-size: 14px;
  color: #cbd5e1;
}

.gate-count-badge {
  font-size: 14px;
  font-weight: bold;
  background: rgba(245, 108, 108, 0.2);
  color: #f56c6c;
  padding: 2px 10px;
  border-radius: 12px;
}

.empty-state {
  color: #475569;
  font-size: 14px;
  padding: 20px;
  text-align: center;
}

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 12px;
}

.metric-item {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 16px;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.05);
}

.metric-icon {
  font-size: 28px;
}

.metric-info {
  flex: 1;
}

.metric-label {
  font-size: 13px;
  color: #64748b;
  margin-bottom: 4px;
}

.metric-value {
  font-size: 20px;
  font-weight: bold;
  color: #fff;
}

.metric-value.highlight {
  color: #67c23a;
  font-size: 24px;
}

.pipeline-flow {
  display: flex;
  justify-content: space-between;
  align-items: stretch;
  margin-bottom: 20px;
  padding: 16px 0;
}

.pipeline-step {
  flex: 1;
  text-align: center;
  padding: 16px 12px;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 12px;
  margin: 0 6px;
  border: 1px solid rgba(255, 255, 255, 0.05);
  transition: all 0.3s ease;
}

.pipeline-step.normal { background: rgba(103, 194, 58, 0.1); border-color: rgba(103, 194, 58, 0.2); }
.pipeline-step.warning { background: rgba(230, 162, 60, 0.1); border-color: rgba(230, 162, 60, 0.3); }
.pipeline-step.critical { background: rgba(245, 108, 108, 0.1); border-color: rgba(245, 108, 108, 0.4); }

.step-number {
  font-size: 12px;
  color: #64748b;
  margin-bottom: 8px;
}

.step-label {
  font-size: 14px;
  font-weight: 500;
  color: #cbd5e1;
  margin-bottom: 8px;
}

.step-count {
  font-size: 24px;
  font-weight: bold;
  color: #fff;
}

.task-summary {
  display: flex;
  justify-content: space-between;
  margin-bottom: 20px;
}

.summary-item {
  flex: 1;
  text-align: center;
  padding: 16px 12px;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 12px;
  margin: 0 6px;
  border: 1px solid rgba(255, 255, 255, 0.05);
}

.summary-item.pending { border-top: 3px solid #e6a23c; }
.summary-item.running { border-top: 3px solid #409eff; }
.summary-item.completed { border-top: 3px solid #67c23a; }
.summary-item.failed { border-top: 3px solid #f56c6c; }
.summary-item.paused { border-top: 3px solid #909399; }
.summary-item.canceled { border-top: 3px solid #666666; }

.summary-value {
  font-size: 32px;
  font-weight: bold;
  color: #fff;
  display: block;
  margin-bottom: 4px;
}

.summary-label {
  font-size: 13px;
  color: #64748b;
}

.agent-list {
  max-height: 280px;
  overflow-y: auto;
}

.agent-item {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 14px;
  margin-bottom: 10px;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 12px;
  border-left: 4px solid;
}

.agent-status {
  width: 12px;
  height: 12px;
  border-radius: 50%;
}

.agent-info {
  flex: 1;
}

.agent-name {
  font-size: 15px;
  font-weight: 500;
  color: #fff;
  margin-bottom: 2px;
}

.agent-role {
  font-size: 13px;
  color: #64748b;
}

.agent-extra {
  margin-left: auto;
}

.task-badge {
  font-size: 12px;
  padding: 4px 10px;
  background: rgba(64, 158, 255, 0.2);
  color: #409eff;
  border-radius: 12px;
}

.task-list {
  max-height: 320px;
  overflow-y: auto;
}

.task-item {
  padding: 14px;
  margin-bottom: 12px;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.05);
}

.task-status {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  margin-bottom: 10px;
}

.task-title {
  font-size: 14px;
  font-weight: 500;
  color: #fff;
  margin-bottom: 6px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.task-meta {
  font-size: 12px;
  color: #64748b;
  margin-bottom: 10px;
}

.task-progress {
  display: flex;
  align-items: center;
  gap: 10px;
}

.progress-bar {
  flex: 1;
  height: 6px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 3px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  border-radius: 3px;
  transition: width 0.5s ease;
}

.progress-text {
  font-size: 13px;
  color: #94a3b8;
  min-width: 40px;
  text-align: right;
}

@media (max-width: 1600px) {
  .screen-main {
    grid-template-columns: 280px 1fr 320px;
    gap: 16px;
  }
}

@media (max-width: 1200px) {
  .screen-main {
    grid-template-columns: 1fr;
    height: auto;
  }
  .left-panel, .center-panel, .right-panel {
    flex-direction: row;
    flex-wrap: wrap;
  }
  .panel {
    min-width: 300px;
  }
  .pipeline-panel {
    min-width: 100%;
  }
}
</style>
