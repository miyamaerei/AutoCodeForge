<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useWorkflowCenterStore } from '../store/useWorkflowCenterStore'

const route = useRoute()
const router = useRouter()
const store = useWorkflowCenterStore()

const {
  loading,
  error,
  repositoryOptions,
  selectedRepository,
  filteredTasks,
  filteredNotifications,
  filteredApprovals,
  filteredHumanLoopCases,
  stats,
} = storeToRefs(store)

const activeQueue = ref<'all' | 'notifications' | 'approvals' | 'hitl'>('all')

onMounted(async () => {
  await store.loadWorkflowData()

  const queryRepositoryId = typeof route.query.repositoryId === 'string' ? route.query.repositoryId : null
  if (queryRepositoryId && queryRepositoryId !== selectedRepository.value) {
    store.setSelectedRepository(queryRepositoryId)
  }

  const queue = typeof route.query.queue === 'string' ? route.query.queue : null
  if (queue === 'notifications' || queue === 'approvals' || queue === 'hitl' || queue === 'all') {
    activeQueue.value = queue
  }
})

const hasData = computed(() => {
  return (
    filteredNotifications.value.length > 0 ||
    filteredApprovals.value.length > 0 ||
    filteredHumanLoopCases.value.length > 0 ||
    filteredTasks.value.length > 0
  )
})

const visibleNotifications = computed(() => {
  if (activeQueue.value === 'approvals' || activeQueue.value === 'hitl') {
    return []
  }
  return filteredNotifications.value
})

const visibleApprovals = computed(() => {
  if (activeQueue.value === 'notifications' || activeQueue.value === 'hitl') {
    return []
  }
  return filteredApprovals.value
})

const visibleHumanLoopCases = computed(() => {
  if (activeQueue.value === 'notifications' || activeQueue.value === 'approvals') {
    return []
  }
  return filteredHumanLoopCases.value
})

const repoTaskSlice = computed(() => {
  return filteredTasks.value.slice(0, 8)
})

const notificationTagType: Record<string, 'primary' | 'warning' | 'danger' | 'info'> = {
  'review-request': 'primary',
  mention: 'info',
  'deployment-approval': 'warning',
  'workflow-failure': 'danger',
}

const approvalTagType: Record<string, 'success' | 'warning' | 'danger'> = {
  等待审批: 'warning',
  超时风险: 'danger',
  已完成: 'success',
}

const riskTagType: Record<string, 'success' | 'warning' | 'danger'> = {
  low: 'success',
  medium: 'warning',
  high: 'danger',
}

function handleRepositoryChange(value: string): void {
  store.setSelectedRepository(value)
}

function updateQueue(value: 'all' | 'notifications' | 'approvals' | 'hitl'): void {
  activeQueue.value = value
  router.replace({
    path: '/workflow-center/ops',
    query: {
      repositoryId: selectedRepository.value,
      queue: value,
    },
  })
}
</script>

<template>
  <section class="workflow-page">
    <header class="workflow-header">
      <div>
        <h2>通知与审批运营台</h2>
        <p>汇总用户通知、审批请求、仓库任务与 Human-in-the-loop 处理队列。</p>
      </div>
      <div class="header-controls">
        <el-segmented
          :model-value="activeQueue"
          :options="[
            { label: '全部', value: 'all' },
            { label: '通知', value: 'notifications' },
            { label: '审批', value: 'approvals' },
            { label: 'HITL', value: 'hitl' },
          ]"
          @update:model-value="(value: string | number | boolean) => updateQueue(value as 'all' | 'notifications' | 'approvals' | 'hitl')"
        />
        <el-select
          :model-value="selectedRepository"
          class="repo-selector"
          placeholder="选择仓库"
          @update:model-value="handleRepositoryChange"
        >
          <el-option label="全部仓库" value="all" />
          <el-option v-for="repo in repositoryOptions" :key="repo.id" :label="repo.name" :value="repo.id" />
        </el-select>
      </div>
    </header>

    <el-skeleton v-if="loading" :rows="9" animated />
    <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />
    <el-empty v-else-if="!hasData" description="当前筛选下暂无通知、审批或任务数据" />

    <template v-else>
      <section class="summary-grid">
        <el-card>
          <p class="summary-label">待处理通知</p>
          <h3>{{ stats.notificationPending }}</h3>
        </el-card>
        <el-card>
          <p class="summary-label">待审批项</p>
          <h3>{{ stats.approvalPending }}</h3>
        </el-card>
        <el-card>
          <p class="summary-label">需人工介入</p>
          <h3>{{ stats.humanLoopOpen }}</h3>
        </el-card>
      </section>

      <section class="desktop-grid">
        <el-card class="main-panel">
          <template #header>
            <div class="panel-header">通知与审批队列</div>
          </template>

          <el-table v-if="visibleNotifications.length > 0" :data="visibleNotifications" stripe>
            <el-table-column prop="id" label="通知ID" width="110" />
            <el-table-column prop="repositoryName" label="仓库" width="210" />
            <el-table-column prop="title" label="通知内容" min-width="260" />
            <el-table-column label="分类" width="120">
              <template #default="{ row }">
                <el-tag :type="notificationTagType[row.category]">{{ row.category }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="reason" label="触发原因" width="140" />
            <el-table-column prop="receivedAt" label="收到时间" width="160" />
          </el-table>

          <el-divider v-if="visibleNotifications.length > 0 && visibleApprovals.length > 0" />

          <el-table v-if="visibleApprovals.length > 0" :data="visibleApprovals" stripe>
            <el-table-column prop="id" label="审批ID" width="110" />
            <el-table-column prop="repositoryName" label="仓库" width="210" />
            <el-table-column prop="title" label="审批主题" min-width="260" />
            <el-table-column prop="requiredAction" label="动作" width="130" />
            <el-table-column prop="reviewer" label="审批人" width="120" />
            <el-table-column prop="dueAt" label="截止时间" width="160" />
            <el-table-column label="状态" width="110">
              <template #default="{ row }">
                <el-tag :type="approvalTagType[row.status]">{{ row.status }}</el-tag>
              </template>
            </el-table-column>
          </el-table>

          <el-empty
            v-if="visibleNotifications.length === 0 && visibleApprovals.length === 0"
            description="当前队列无通知或审批项"
          />
        </el-card>

        <el-card class="side-panel">
          <template #header>
            <div class="panel-header">按仓库任务概览</div>
          </template>
          <el-table :data="repoTaskSlice" size="small">
            <el-table-column prop="id" label="任务" width="110" />
            <el-table-column prop="repositoryName" label="仓库" min-width="180" />
            <el-table-column prop="title" label="标题" min-width="200" />
            <el-table-column prop="state" label="状态" width="100" />
          </el-table>
          <el-alert
            class="hint-alert"
            title="建议把通知、审批和任务统一按 repositoryId 聚合，减少跨仓库切换。"
            type="info"
            :closable="false"
            show-icon
          />
        </el-card>

        <el-card class="side-panel">
          <template #header>
            <div class="panel-header">Human-in-the-loop 处理</div>
          </template>

          <el-empty v-if="visibleHumanLoopCases.length === 0" description="当前队列无人工介入事项" />

          <el-timeline v-else>
            <el-timeline-item
              v-for="item in visibleHumanLoopCases"
              :key="item.id"
              placement="top"
              :timestamp="`SLA: ${item.slaDeadline}`"
            >
              <el-card class="hitl-card">
                <div class="hitl-head">
                  <strong>{{ item.id }} · {{ item.taskId }}</strong>
                  <el-tag :type="riskTagType[item.riskLevel]">{{ item.riskLevel }}</el-tag>
                </div>
                <p>{{ item.triggerReason }}</p>
                <p>置信度: {{ Math.round(item.confidence * 100) }}%</p>
                <p>建议动作: {{ item.recommendedAction }}</p>
                <p>负责人: {{ item.owner }} / 升级: {{ item.escalationOwner }}</p>
                <p class="context-line">上下文: {{ item.contextSummary }}</p>
              </el-card>
            </el-timeline-item>
          </el-timeline>

          <el-divider />
          <h4>建议采集字段</h4>
          <ul class="guidance-list">
            <li>通知来源与 reason（review request、mention、workflow failure）</li>
            <li>审批动作类型（approve/comment/request changes）和截止时间</li>
            <li>仓库、任务、关联对象链接（PR/Issue/Workflow）</li>
            <li>风险等级、模型置信度、人工决策结论与审计记录</li>
          </ul>
        </el-card>
      </section>
    </template>
  </section>
</template>

<style scoped>
.workflow-page {
  min-width: 1280px;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.workflow-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.workflow-header h2 {
  margin: 0;
}

.workflow-header p {
  margin: 0.35rem 0 0;
  color: #5f6b7a;
}

.header-controls {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.repo-selector {
  width: 260px;
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 1rem;
}

.summary-label {
  margin: 0;
  color: #7a8594;
}

.summary-grid h3 {
  margin: 0.2rem 0 0;
  font-size: 1.65rem;
}

.desktop-grid {
  display: grid;
  grid-template-columns: 2.3fr 1.3fr 1.4fr;
  gap: 1rem;
  overflow-x: auto;
}

.main-panel,
.side-panel {
  min-width: 340px;
}

.panel-header {
  font-weight: 600;
}

.hint-alert {
  margin-top: 0.8rem;
}

.hitl-card p {
  margin: 0.35rem 0;
}

.hitl-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.45rem;
}

.context-line {
  color: #687385;
}

.guidance-list {
  margin: 0.45rem 0 0;
  padding-left: 1.1rem;
  color: #4f5968;
}
</style>
