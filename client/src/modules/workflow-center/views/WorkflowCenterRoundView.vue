<script setup lang="ts">
import { computed, onMounted } from 'vue'
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
  filteredRounds,
  filteredTasks,
  stats,
} = storeToRefs(store)

onMounted(async () => {
  await store.loadWorkflowData()
  const queryRepositoryId = typeof route.query.repositoryId === 'string' ? route.query.repositoryId : null
  if (queryRepositoryId && queryRepositoryId !== selectedRepository.value) {
    store.setSelectedRepository(queryRepositoryId)
  }
})

const activeRequirementId = computed(() =>
  typeof route.query.requirementId === 'string' ? route.query.requirementId : null,
)
const activeRoundId = computed(() =>
  typeof route.query.roundId === 'string' ? route.query.roundId : null,
)

const tableData = computed(() => {
  return filteredRounds.value.filter((item) => {
    if (activeRequirementId.value && item.requirementId !== activeRequirementId.value) {
      return false
    }
    if (activeRoundId.value && item.id !== activeRoundId.value) {
      return false
    }
    return true
  })
})

const roundTaskCountMap = computed(() => {
  const map = new Map<string, number>()
  tableData.value.forEach((round) => {
    const count = filteredTasks.value.filter((task) => task.roundIds.includes(round.id)).length
    map.set(round.id, count)
  })
  return map
})

function getRoundTaskCount(roundId: string): number {
  const count = roundTaskCountMap.value.get(roundId)
  return typeof count === 'number' ? count : 0
}

function goToTasks(roundId: string, requirementId: string): void {
  router.push({
    path: '/workflow-center/tasks',
    query: {
      roundId,
      requirementId,
      repositoryId: selectedRepository.value,
    },
  })
}

function goToRequirement(requirementId: string): void {
  router.push({
    path: '/workflow-center/requirements',
    query: {
      requirementId,
      repositoryId: selectedRepository.value,
    },
  })
}

const hasData = computed(() => tableData.value.length > 0)

const stateType: Record<string, 'primary' | 'warning' | 'success' | 'info'> = {
  规划中: 'info',
  执行中: 'warning',
  已完成: 'success',
  复盘中: 'primary',
}

</script>

<template>
  <section class="workflow-page">
    <header class="workflow-header">
      <div>
        <h2>轮次页面</h2>
        <p>轮次与需求关联，展示目标、速度、复盘和阻塞信息。</p>
      </div>
      <el-select
        v-model="selectedRepository"
        class="repo-selector"
        placeholder="选择仓库"
      >
        <el-option label="全部仓库" value="all" />
        <el-option
          v-for="repo in repositoryOptions"
          :key="repo.id"
          :label="repo.name"
          :value="repo.id"
        />
      </el-select>
    </header>

    <el-skeleton v-if="loading" :rows="8" animated />
    <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />
    <el-empty v-else-if="!hasData" description="当前仓库暂无轮次" />

    <template v-else>
      <section class="summary-grid">
        <el-card>
          <p class="summary-label">轮次总数</p>
          <h3>{{ stats.roundTotal }}</h3>
        </el-card>
        <el-card>
          <p class="summary-label">任务总数</p>
          <h3>{{ stats.taskTotal }}</h3>
        </el-card>
        <el-card>
          <p class="summary-label">阻塞任务</p>
          <h3>{{ stats.taskBlocked }}</h3>
        </el-card>
      </section>

      <section class="desktop-grid">
        <el-card class="main-panel">
          <template #header>
            <div class="panel-header">轮次列表</div>
          </template>
          <el-table :data="tableData" stripe>
            <el-table-column prop="id" label="轮次ID" width="110" />
            <el-table-column prop="requirementTitle" label="关联需求" min-width="220" />
            <el-table-column prop="sprintName" label="轮次名称" width="180" />
            <el-table-column prop="goal" label="轮次目标" min-width="220" />
            <el-table-column label="关联任务" width="100">
              <template #default="{ row }">{{ getRoundTaskCount(row.id) }}</template>
            </el-table-column>
            <el-table-column prop="velocity" label="速度" width="90" />
            <el-table-column prop="completionRate" label="完成率" width="90">
              <template #default="{ row }">{{ row.completionRate }}%</template>
            </el-table-column>
            <el-table-column label="状态" width="110">
              <template #default="{ row }">
                <el-tag :type="stateType[row.state]">{{ row.state }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="跳转" width="200">
              <template #default="{ row }">
                <el-button link type="primary" size="small" @click="goToRequirement(row.requirementId)">
                  需求
                </el-button>
                <el-button link type="primary" size="small" @click="goToTasks(row.id, row.requirementId)">
                  任务
                </el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>

        <el-card class="side-panel">
          <template #header>
            <div class="panel-header">时间窗口</div>
          </template>
          <el-timeline>
            <el-timeline-item
              v-for="item in tableData"
              :key="item.id"
              :timestamp="`${item.startDate} ~ ${item.endDate}`"
            >
              <div class="timeline-title">{{ item.sprintName }}</div>
              <div class="timeline-desc">目标：{{ item.goal }}</div>
            </el-timeline-item>
          </el-timeline>
        </el-card>

        <el-card class="side-panel">
          <template #header>
            <div class="panel-header">复盘摘要</div>
          </template>
          <el-collapse>
            <el-collapse-item
              v-for="item in tableData"
              :key="item.id"
              :title="item.sprintName"
              :name="item.id"
            >
              <p>Review Owner: {{ item.reviewOwner }}</p>
              <p>阻塞项: {{ item.blockerCount }}</p>
              <p>{{ item.retrospectiveSummary }}</p>
            </el-collapse-item>
          </el-collapse>
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
  margin: 0.4rem 0 0;
  color: #5f6b7a;
}

.repo-selector {
  width: 280px;
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
  font-size: 1.6rem;
}

.desktop-grid {
  display: grid;
  grid-template-columns: 2.2fr 1fr 1fr;
  gap: 1rem;
  overflow-x: auto;
}

.main-panel,
.side-panel {
  min-width: 320px;
}

.panel-header {
  font-weight: 600;
}

.timeline-title {
  font-weight: 600;
}

.timeline-desc {
  color: #5f6b7a;
}
</style>
