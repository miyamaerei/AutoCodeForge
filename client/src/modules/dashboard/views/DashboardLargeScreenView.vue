<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { Stage, Layer, Group, Rect, Circle, Line, Text } from 'vue-konva'
import {
  dashboardApi,
  type DashboardAgentLiveDto,
  type DashboardLivePayloadDto,
  type DashboardLogItemDto,
  type DashboardSnapshotDto,
  type DashboardTaskLiveDto,
} from '../api/dashboard.api'
import { DateUtils } from '@/lib/dateUtils'

interface AnimatedTaskNode extends DashboardTaskLiveDto {
  x: number
  y: number
  targetX: number
  targetY: number
  lane: number
}

interface AnimatedAgentNode extends DashboardAgentLiveDto {
  x: number
  y: number
  targetX: number
  targetY: number
}

const snapshot = ref<DashboardSnapshotDto | null>(null)
const tasks = ref<DashboardTaskLiveDto[]>([])
const agents = ref<DashboardAgentLiveDto[]>([])
const logs = ref<DashboardLogItemDto[]>([])
const selectedLogType = ref<'task' | 'agent' | 'system'>('system')
const selectedTaskId = ref<string | null>(null)
const selectedAgentId = ref<string | null>(null)
const isFullscreen = ref(false)
const lastUpdateTime = ref('')
const viewportWidth = ref(1920)
const viewportHeight = ref(1080)

const stageHost = ref<HTMLElement | null>(null)
const stageWidth = ref(1280)
const stageHeight = ref(330)

const animatedTasks = ref<AnimatedTaskNode[]>([])
const animatedAgents = ref<AnimatedAgentNode[]>([])

let coreRefreshInterval: number | null = null
let animationFrameId: number | null = null
let resizeObserver: ResizeObserver | null = null
let liveStreamDisposer: (() => void) | null = null
let liveStreamGuardTimeout: number | null = null
let logsStreamDisposer: (() => void) | null = null
let hasLiveStreamPayload = false

const pipelineSteps = [
  { step: 1, key: 'DemandAnalyse', label: '需求梳理' },
  { step: 2, key: 'QueryCurrent', label: '信息查询' },
  { step: 3, key: 'MakePlan', label: '方案制定' },
  { step: 4, key: 'Development', label: '代码开发' },
  { step: 5, key: 'TestVerify', label: '测试校验' },
  { step: 6, key: 'CommitPr', label: '版本提交' },
  { step: 7, key: 'FinalAudit', label: '最终审核' },
]

const stateColors: Record<string, string> = {
  Idle: '#6ee7b7',
  Handling: '#60a5fa',
  Learning: '#fbbf24',
  Dormant: '#f87171',
}

const roleLabels: Record<string, string> = {
  Secretary: '秘书',
  Manager: '管理者',
  Worker: '工人',
}

const statusLabels: Record<string, string> = {
  Pending: '待处理',
  Running: '进行中',
  Completed: '已完成',
  Failed: '失败',
  Paused: '已暂停',
  Canceled: '已取消',
}

const totalTasks = computed(() => snapshot.value?.taskStats.total ?? 0)
const runningTasks = computed(() => snapshot.value?.taskStats.running ?? 0)
const exceptionTasks = computed(() => (snapshot.value?.taskStats.failed ?? 0) + (snapshot.value?.taskStats.canceled ?? 0))
const completedTasks = computed(() => snapshot.value?.taskStats.completed ?? 0)
const filteredLogs = computed(() => logs.value.slice(0, 120))
const isStrictLargeScreen = computed(() => {
  const ratio = viewportWidth.value / Math.max(viewportHeight.value, 1)
  return viewportHeight.value <= 1080 && ratio >= 1.6
})

const stageConfig = computed(() => ({
  width: stageWidth.value,
  height: stageHeight.value,
}))

const stepStartX = computed(() => 90)
const stepGap = computed(() => (stageWidth.value - 180) / (pipelineSteps.length - 1))
const laneStartY = computed(() => 74)
const laneGap = computed(() => 33)

const stepLabels = computed(() => {
  return pipelineSteps.map((step) => ({
    ...step,
    x: stepStartX.value + (step.step - 1) * stepGap.value,
    y: 22,
  }))
})

const laneGuides = computed(() => {
  return Array.from({ length: 6 }, (_, index) => ({
    y: laneStartY.value + index * laneGap.value,
  }))
})

const taskNodeTargets = computed(() => {
  return tasks.value.slice(0, 36).map((task, index) => {
    const step = Math.max(1, Math.min(7, task.currentStep || 1))
    const lane = index % 6
    const progressRatio = Math.min(1, Math.max(0, task.progress / 100))
    const targetX = stepStartX.value + (step - 1) * stepGap.value + progressRatio * stepGap.value * 0.72
    const targetY = laneStartY.value + lane * laneGap.value

    return {
      ...task,
      lane,
      targetX,
      targetY,
    }
  })
})

const agentNodeTargets = computed(() => {
  return agents.value.slice(0, 18).map((agent, index) => {
    const step = Math.max(1, Math.min(7, agent.workstationStep || 1))
    const stack = index % 3
    const targetX = stepStartX.value + (step - 1) * stepGap.value
    const targetY = stageHeight.value - 78 + stack * 22

    return {
      ...agent,
      targetX,
      targetY,
    }
  })
})

function getTaskStatusText(status: string): string {
  return statusLabels[status] || status
}

function syncAnimatedTasks() {
  const previous = new Map(animatedTasks.value.map((task) => [task.id, task]))

  animatedTasks.value = taskNodeTargets.value.map((task) => {
    const oldTask = previous.get(task.id)
    return {
      ...task,
      x: oldTask?.x ?? task.targetX,
      y: oldTask?.y ?? task.targetY,
    }
  })
}

function syncAnimatedAgents() {
  const previous = new Map(animatedAgents.value.map((agent) => [agent.id, agent]))

  animatedAgents.value = agentNodeTargets.value.map((agent) => {
    const oldAgent = previous.get(agent.id)
    return {
      ...agent,
      x: oldAgent?.x ?? agent.targetX,
      y: oldAgent?.y ?? agent.targetY,
    }
  })
}

function animateFrame() {
  animatedTasks.value = animatedTasks.value.map((task) => ({
    ...task,
    x: task.x + (task.targetX - task.x) * 0.18,
    y: task.y + (task.targetY - task.y) * 0.2,
  }))

  animatedAgents.value = animatedAgents.value.map((agent) => ({
    ...agent,
    x: agent.x + (agent.targetX - agent.x) * 0.2,
    y: agent.y + (agent.targetY - agent.y) * 0.2,
  }))

  animationFrameId = window.requestAnimationFrame(animateFrame)
}

function setupStageResize() {
  if (!stageHost.value) {
    return
  }

  const updateStageSize = () => {
    if (!stageHost.value) {
      return
    }

    stageWidth.value = Math.max(1000, stageHost.value.clientWidth - 2)
    stageHeight.value = Math.max(240, stageHost.value.clientHeight - 2)
  }

  updateStageSize()
  resizeObserver = new ResizeObserver(() => updateStageSize())
  resizeObserver.observe(stageHost.value)
}

function updateViewportMetrics() {
  viewportWidth.value = window.innerWidth
  viewportHeight.value = window.innerHeight
}

async function refreshCoreData() {
  try {
    const [snapshotData, taskData, agentData] = await Promise.all([
      dashboardApi.getSnapshot(),
      dashboardApi.getTasks(),
      dashboardApi.getAgents(),
    ])

    snapshot.value = snapshotData
    tasks.value = taskData
    agents.value = agentData
    lastUpdateTime.value = new Date().toLocaleTimeString('zh-CN')
  } catch (error) {
    console.error('Failed to refresh dashboard snapshot/tasks/agents:', error)
  }
}

function applyLivePayload(payload: DashboardLivePayloadDto) {
  snapshot.value = payload.snapshot
  tasks.value = payload.tasks
  agents.value = payload.agents
  const timestamp = payload.generatedAtUtc || new Date().toISOString()
  lastUpdateTime.value = new Date(timestamp).toLocaleTimeString('zh-CN')
}

function stopCoreFallbackPolling() {
  if (!coreRefreshInterval) {
    return
  }

  clearInterval(coreRefreshInterval)
  coreRefreshInterval = null
}

function startCoreFallbackPolling() {
  if (coreRefreshInterval) {
    return
  }

  refreshCoreData()
  coreRefreshInterval = window.setInterval(refreshCoreData, 5000)
}

function stopCoreLiveStream() {
  if (liveStreamGuardTimeout) {
    clearTimeout(liveStreamGuardTimeout)
    liveStreamGuardTimeout = null
  }

  if (!liveStreamDisposer) {
    return
  }

  liveStreamDisposer()
  liveStreamDisposer = null
}

function startCoreLiveStream() {
  stopCoreLiveStream()
  hasLiveStreamPayload = false
  let consecutiveErrors = 0

  liveStreamDisposer = dashboardApi.connectLiveStream({
    intervalMs: 4000,
    onOpen: () => {
      consecutiveErrors = 0
    },
    onLive: (payload) => {
      hasLiveStreamPayload = true
      consecutiveErrors = 0
      stopCoreFallbackPolling()
      applyLivePayload(payload)
    },
    onError: () => {
      consecutiveErrors += 1
      if (!hasLiveStreamPayload || consecutiveErrors >= 2) {
        startCoreFallbackPolling()
      }
    },
  })

  liveStreamGuardTimeout = window.setTimeout(() => {
    if (!hasLiveStreamPayload) {
      startCoreFallbackPolling()
    }
  }, 7000)
}

function stopLogsLiveStream() {
  if (!logsStreamDisposer) {
    return
  }

  logsStreamDisposer()
  logsStreamDisposer = null
}

function startLogsLiveStream() {
  stopLogsLiveStream()

  logsStreamDisposer = dashboardApi.connectLogsStream({
    type: selectedLogType.value,
    ...(selectedTaskId.value ? { taskId: selectedTaskId.value } : {}),
    ...(selectedAgentId.value ? { agentId: selectedAgentId.value } : {}),
    intervalMs: 2500,
    onLogs: (payload) => {
      logs.value = payload
    },
    onError: () => {
      // EventSource auto-reconnect handles transient network interruptions.
    },
  })
}

function reconnectLogsLiveStream() {
  startLogsLiveStream()
}

function resetSelection(type: 'task' | 'agent' | 'system') {
  selectedLogType.value = type
  if (type !== 'task') {
    selectedTaskId.value = null
  }
  if (type !== 'agent') {
    selectedAgentId.value = null
  }
}

function selectTask(taskId: string) {
  selectedTaskId.value = taskId
  selectedAgentId.value = null
  selectedLogType.value = 'task'
}

function selectAgent(agentId: string) {
  selectedTaskId.value = null
  selectedAgentId.value = agentId
  selectedLogType.value = 'agent'
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

function getTaskBaseColor(task: DashboardTaskLiveDto): string {
  if (task.alertLevel === 'critical') {
    return '#ef4444'
  }

  if (task.alertLevel === 'warning') {
    return '#f59e0b'
  }

  if (task.status === 'Completed') {
    return '#22c55e'
  }

  if (task.status === 'Running') {
    return '#60a5fa'
  }

  return '#94a3b8'
}

watch([taskNodeTargets, stageWidth], () => {
  syncAnimatedTasks()
}, { deep: true })

watch([agentNodeTargets, stageWidth], () => {
  syncAnimatedAgents()
}, { deep: true })

watch([selectedLogType, selectedTaskId, selectedAgentId], () => {
  startLogsLiveStream()
})

onMounted(() => {
  updateViewportMetrics()
  window.addEventListener('resize', updateViewportMetrics)
  setupStageResize()
  startCoreLiveStream()
  startLogsLiveStream()
  syncAnimatedTasks()
  syncAnimatedAgents()
  animationFrameId = window.requestAnimationFrame(animateFrame)
})

onUnmounted(() => {
  window.removeEventListener('resize', updateViewportMetrics)
  stopCoreLiveStream()
  stopCoreFallbackPolling()
  stopLogsLiveStream()

  if (animationFrameId) {
    cancelAnimationFrame(animationFrameId)
  }

  if (resizeObserver) {
    resizeObserver.disconnect()
    resizeObserver = null
  }
})
</script>

<template>
  <div class="screen" :class="{ fullscreen: isFullscreen, 'screen--strict': isStrictLargeScreen }">
    <header class="screen-header">
      <div>
        <h1>多 Agent 流水线仿真大屏（Vue-Konva）</h1>
        <p>更新时间：{{ lastUpdateTime || '--:--:--' }}</p>
      </div>
      <div class="header-actions">
        <button @click="refreshCoreData">刷新快照</button>
        <button @click="reconnectLogsLiveStream">刷新日志</button>
        <button @click="toggleFullscreen">{{ isFullscreen ? '退出全屏' : '全屏' }}</button>
      </div>
    </header>

    <section class="stats-row">
      <article class="stat-card">
        <span>任务总数</span>
        <strong>{{ totalTasks }}</strong>
      </article>
      <article class="stat-card running">
        <span>运行中</span>
        <strong>{{ runningTasks }}</strong>
      </article>
      <article class="stat-card warning">
        <span>异常数</span>
        <strong>{{ exceptionTasks }}</strong>
      </article>
      <article class="stat-card success">
        <span>完成数</span>
        <strong>{{ completedTasks }}</strong>
      </article>
      <article class="stat-card">
        <span>待处理门控</span>
        <strong>{{ snapshot?.gateStats.pendingCount ?? 0 }}</strong>
      </article>
    </section>

    <section class="sandbox-panel">
      <div ref="stageHost" class="konva-host">
        <Stage :config="stageConfig">
          <Layer>
            <Rect :config="{ x: 0, y: 0, width: stageWidth, height: stageHeight, fill: '#071222' }" />

            <Line
              :config="{
                points: [0, stageHeight - 108, stageWidth, stageHeight - 108],
                stroke: 'rgba(148,163,184,0.35)',
                strokeWidth: 1,
                dash: [6, 4],
              }"
            />

            <Line
              v-for="guide in laneGuides"
              :key="`lane-${guide.y}`"
              :config="{
                points: [52, guide.y, stageWidth - 52, guide.y],
                stroke: 'rgba(96,165,250,0.18)',
                strokeWidth: 1,
                dash: [5, 8],
              }"
            />

            <Line
              v-for="step in stepLabels"
              :key="`step-column-${step.step}`"
              :config="{
                points: [step.x, 46, step.x, stageHeight - 16],
                stroke: 'rgba(148,163,184,0.15)',
                strokeWidth: 1,
              }"
            />

            <Group v-for="step in stepLabels" :key="`step-label-${step.step}`">
              <Circle
                :config="{
                  x: step.x,
                  y: step.y,
                  radius: 10,
                  fill: '#0f172a',
                  stroke: '#64748b',
                  strokeWidth: 1,
                }"
              />
              <Text
                :config="{
                  x: step.x - 3,
                  y: step.y - 5,
                  text: String(step.step),
                  fontSize: 10,
                  fill: '#cbd5e1',
                }"
              />
              <Text
                :config="{
                  x: step.x - 38,
                  y: step.y + 14,
                  width: 76,
                  text: step.label,
                  align: 'center',
                  fontSize: 11,
                  fill: '#dbeafe',
                }"
              />
            </Group>

            <Group
              v-for="task in animatedTasks"
              :key="`task-${task.id}`"
              @click="selectTask(task.id)"
            >
              <Circle
                :config="{
                  x: task.x,
                  y: task.y,
                  radius: 7,
                  fill: getTaskBaseColor(task),
                  stroke: '#e2e8f0',
                  strokeWidth: 1,
                  shadowColor: getTaskBaseColor(task),
                  shadowBlur: 8,
                  shadowOpacity: 0.5,
                }"
              />

              <Circle
                v-if="task.isTimeout"
                :config="{
                  x: task.x,
                  y: task.y,
                  radius: 11,
                  stroke: '#fb923c',
                  strokeWidth: 2,
                  dash: [4, 4],
                }"
              />

              <Line
                v-if="task.hasRejectedGate"
                :config="{
                  points: [task.x - 8, task.y - 8, task.x + 8, task.y + 8, task.x - 8, task.y + 8, task.x + 8, task.y - 8],
                  stroke: '#ef4444',
                  strokeWidth: 1.8,
                }"
              />

              <Circle
                v-if="task.hasEmergencyGate"
                :config="{
                  x: task.x + 10,
                  y: task.y - 10,
                  radius: 3.5,
                  fill: '#f0abfc',
                  stroke: '#86198f',
                  strokeWidth: 1,
                }"
              />

              <Text
                :config="{
                  x: task.x + 10,
                  y: task.y - 6,
                  text: `${task.title} (${task.progress}%)`,
                  fontSize: 10,
                  fill: '#cbd5e1',
                }"
              />
            </Group>

            <Group
              v-for="agent in animatedAgents"
              :key="`agent-${agent.id}`"
              @click="selectAgent(agent.id)"
            >
              <Rect
                :config="{
                  x: agent.x - 16,
                  y: agent.y - 10,
                  width: 32,
                  height: 20,
                  cornerRadius: 6,
                  fill: '#0f172a',
                  stroke: stateColors[agent.state] || '#60a5fa',
                  strokeWidth: 1.8,
                }"
              />

              <Circle
                :config="{
                  x: agent.x - 10,
                  y: agent.y,
                  radius: 3,
                  fill: stateColors[agent.state] || '#60a5fa',
                }"
              />

              <Text
                :config="{
                  x: agent.x - 5,
                  y: agent.y - 5,
                  text: `${agent.name}`,
                  fontSize: 9,
                  fill: '#e2e8f0',
                }"
              />

              <Circle
                v-if="agent.state === 'Dormant'"
                :config="{
                  x: agent.x,
                  y: agent.y,
                  radius: 15,
                  stroke: '#ef4444',
                  strokeWidth: 1.5,
                  dash: [4, 3],
                }"
              />
            </Group>
          </Layer>
        </Stage>
      </div>

      <div class="legend-row">
        <span class="legend-item"><i class="dot timeout"></i>超时</span>
        <span class="legend-item"><i class="dot rejected"></i>驳回</span>
        <span class="legend-item"><i class="dot emergency"></i>紧急门控</span>
      </div>
    </section>

    <section class="bottom-grid">
      <article class="panel">
        <h2>Agent 状态分布</h2>
        <div class="agent-stats">
          <div class="agent-stat idle">
            <span>空闲</span>
            <strong>{{ snapshot?.agentStats.idle ?? 0 }}</strong>
          </div>
          <div class="agent-stat handling">
            <span>处理中</span>
            <strong>{{ snapshot?.agentStats.handling ?? 0 }}</strong>
          </div>
          <div class="agent-stat learning">
            <span>学习中</span>
            <strong>{{ snapshot?.agentStats.learning ?? 0 }}</strong>
          </div>
          <div class="agent-stat dormant">
            <span>休眠</span>
            <strong>{{ snapshot?.agentStats.dormant ?? 0 }}</strong>
          </div>
        </div>
        <ul class="mini-list">
          <li v-for="agent in agents.slice(0, 8)" :key="agent.id">
            <span>{{ agent.name }}</span>
            <span>{{ roleLabels[agent.role] || agent.role }} · {{ agent.workstationName }}</span>
          </li>
        </ul>
      </article>

      <article class="panel">
        <h2>任务看板</h2>
        <ul class="mini-list">
          <li v-for="task in tasks.slice(0, 12)" :key="task.id" :class="{ anomaly: task.alertLevel !== 'normal' }">
            <span>{{ task.title }}</span>
            <span>{{ task.currentStepName }} · {{ getTaskStatusText(task.status) }} · {{ task.progress }}%</span>
          </li>
        </ul>
      </article>

      <article class="panel logs-panel">
        <div class="logs-header">
          <h2>实时日志</h2>
          <div class="tabs">
            <button :class="{ active: selectedLogType === 'task' }" @click="resetSelection('task')">任务日志</button>
            <button :class="{ active: selectedLogType === 'agent' }" @click="resetSelection('agent')">Agent日志</button>
            <button :class="{ active: selectedLogType === 'system' }" @click="resetSelection('system')">系统日志</button>
          </div>
        </div>

        <div class="logs-list">
          <div v-for="log in filteredLogs" :key="`${log.type}-${log.time}-${log.content}`" class="log-item" :class="log.level">
            <span class="log-time">{{ DateUtils.formatDateTime(log.time) }}</span>
            <span class="log-content">{{ log.content }}</span>
          </div>
          <div v-if="filteredLogs.length === 0" class="log-empty">暂无日志</div>
        </div>
      </article>
    </section>
  </div>
</template>

<style scoped>
.screen {
  --screen-gap: 14px;
  --header-padding-y: 16px;
  --header-padding-x: 18px;
  --header-title-size: 28px;
  --stats-gap: 12px;
  --stat-card-padding: 14px;
  --stat-value-size: 28px;
  --section-padding: 16px;
  --panel-padding: 14px;
  --panel-title-size: 17px;
  --list-item-font-size: 12px;
  --list-item-padding-y: 8px;
  --header-button-padding: 10px 14px;
  --header-button-font-size: 14px;
  --tab-button-padding: 6px 10px;
  --tab-button-font-size: 12px;

  height: 100vh;
  min-height: 100vh;
  min-width: 1280px;
  padding: 18px;
  display: flex;
  flex-direction: column;
  gap: var(--screen-gap);
  overflow: hidden;
  color: #e2e8f0;
  background:
    radial-gradient(circle at 16% 8%, rgba(59, 130, 246, 0.25), transparent 38%),
    radial-gradient(circle at 88% 85%, rgba(244, 63, 94, 0.17), transparent 35%),
    linear-gradient(155deg, #07111f 0%, #0c1d33 45%, #0a1424 100%);
}

.screen-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex: 0 0 auto;
  min-height: 0;
  padding: var(--header-padding-y) var(--header-padding-x);
  border-radius: 14px;
  border: 1px solid rgba(148, 163, 184, 0.24);
  background: rgba(15, 23, 42, 0.6);
}

.screen-header h1 {
  margin: 0;
  font-size: var(--header-title-size);
  letter-spacing: 1px;
}

.screen-header p {
  margin: 4px 0 0;
  color: #94a3b8;
  font-size: 13px;
}

.header-actions {
  display: flex;
  gap: 10px;
}

.header-actions button {
  padding: var(--header-button-padding);
  font-size: var(--header-button-font-size);
  border-radius: 10px;
  border: 1px solid rgba(148, 163, 184, 0.3);
  background: rgba(30, 41, 59, 0.7);
  color: #e2e8f0;
  cursor: pointer;
}

.stats-row {
  flex: 0 0 auto;
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: var(--stats-gap);
}

.stat-card {
  padding: var(--stat-card-padding);
  border-radius: 12px;
  border: 1px solid rgba(148, 163, 184, 0.2);
  background: rgba(15, 23, 42, 0.55);
}

.stat-card span {
  color: #94a3b8;
  font-size: 12px;
}

.stat-card strong {
  display: block;
  margin-top: 6px;
  font-size: var(--stat-value-size);
}

.stat-card.running strong {
  color: #60a5fa;
}

.stat-card.warning strong {
  color: #f87171;
}

.stat-card.success strong {
  color: #34d399;
}

.sandbox-panel {
  flex: 1 1 56%;
  min-height: 300px;
  min-width: 0;
  display: flex;
  flex-direction: column;
  border-radius: 14px;
  border: 1px solid rgba(148, 163, 184, 0.23);
  background: rgba(15, 23, 42, 0.58);
  padding: var(--section-padding);
  overflow: hidden;
}

.konva-host {
  flex: 1;
  width: 100%;
  height: 100%;
  min-height: 0;
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid rgba(148, 163, 184, 0.16);
}

.legend-row {
  margin-top: 10px;
  display: flex;
  gap: 16px;
  color: #cbd5e1;
  font-size: 12px;
}

.legend-item {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  display: inline-block;
}

.dot.timeout {
  border: 2px dashed #fb923c;
  background: transparent;
}

.dot.rejected {
  background: #ef4444;
}

.dot.emergency {
  background: #f0abfc;
}

.bottom-grid {
  flex: 1 1 44%;
  min-height: 0;
  display: grid;
  grid-template-columns: 320px 1fr 1.2fr;
  gap: 12px;
}

.panel {
  min-width: 0;
  min-height: 0;
  display: flex;
  flex-direction: column;
  padding: var(--panel-padding);
  border-radius: 12px;
  border: 1px solid rgba(148, 163, 184, 0.22);
  background: rgba(15, 23, 42, 0.6);
}

.panel h2 {
  margin: 0 0 12px;
  font-size: var(--panel-title-size);
}

.agent-stats {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 8px;
  margin-bottom: 12px;
}

.agent-stat {
  border-radius: 8px;
  border: 1px solid rgba(148, 163, 184, 0.2);
  padding: 8px;
}

.agent-stat span {
  font-size: 12px;
  color: #94a3b8;
}

.agent-stat strong {
  display: block;
  margin-top: 2px;
  font-size: 22px;
}

.agent-stat.idle strong {
  color: #6ee7b7;
}

.agent-stat.handling strong {
  color: #60a5fa;
}

.agent-stat.learning strong {
  color: #fbbf24;
}

.agent-stat.dormant strong {
  color: #f87171;
}

.mini-list {
  list-style: none;
  margin: 0;
  padding: 0;
  flex: 1;
  min-height: 0;
  overflow: auto;
}

.mini-list li {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  padding: var(--list-item-padding-y) 0;
  border-bottom: 1px dashed rgba(148, 163, 184, 0.2);
  font-size: var(--list-item-font-size);
}

.mini-list li.anomaly {
  color: #fecaca;
}

.logs-panel {
  display: flex;
  flex-direction: column;
}

.logs-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 8px;
}

.tabs {
  display: flex;
  gap: 6px;
}

.tabs button {
  border: 1px solid rgba(148, 163, 184, 0.28);
  background: rgba(30, 41, 59, 0.7);
  color: #cbd5e1;
  border-radius: 999px;
  padding: var(--tab-button-padding);
  font-size: var(--tab-button-font-size);
  cursor: pointer;
}

.tabs button.active {
  background: rgba(96, 165, 250, 0.25);
  color: #dbeafe;
}

.logs-list {
  flex: 1;
  min-height: 0;
  margin-top: 12px;
  overflow: auto;
  border-radius: 8px;
  border: 1px solid rgba(148, 163, 184, 0.2);
  background: rgba(2, 6, 23, 0.7);
}

.log-item {
  display: grid;
  grid-template-columns: 150px 1fr;
  gap: 10px;
  padding: 8px 10px;
  border-bottom: 1px dashed rgba(148, 163, 184, 0.16);
  font-size: var(--list-item-font-size);
}

.log-item.warning .log-content {
  color: #fbbf24;
}

.log-item.error .log-content {
  color: #f87171;
}

.log-time {
  color: #64748b;
}

.log-empty {
  padding: 16px;
  color: #64748b;
  text-align: center;
}

.screen--strict {
  --screen-gap: 10px;
  --header-padding-y: 10px;
  --header-padding-x: 12px;
  --header-title-size: 22px;
  --stats-gap: 8px;
  --stat-card-padding: 10px;
  --stat-value-size: 22px;
  --section-padding: 12px;
  --panel-padding: 10px;
  --panel-title-size: 15px;
  --list-item-font-size: 11px;
  --list-item-padding-y: 6px;
  --header-button-padding: 7px 10px;
  --header-button-font-size: 12px;
  --tab-button-padding: 5px 8px;
  --tab-button-font-size: 11px;
}

.screen--strict .sandbox-panel {
  flex-basis: 52%;
  min-height: 260px;
}

.screen--strict .bottom-grid {
  flex-basis: 48%;
  gap: 10px;
}

.screen--strict .agent-stats {
  gap: 6px;
  margin-bottom: 8px;
}

.screen--strict .logs-list {
  margin-top: 8px;
}

@media (max-width: 1450px) {
  .screen {
    overflow-x: auto;
    overflow-y: hidden;
  }

  .bottom-grid {
    grid-template-columns: 300px 1fr 1.1fr;
  }
}
</style>
