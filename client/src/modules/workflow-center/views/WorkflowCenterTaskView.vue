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
  return filteredTasks.value.filter((item) => {
    if (activeRequirementId.value && item.requirementId !== activeRequirementId.value) {
      return false
    }
    if (activeRoundId.value && !item.roundIds.includes(activeRoundId.value)) {
      return false
    }
    return true
  })
})

const hasData = computed(() => tableData.value.length > 0)

const taskStateType: Record<string, 'info' | 'warning' | 'primary' | 'success' | 'danger'> = {
  待办: 'info',
  进行中: 'warning',
  联调中: 'primary',
  待验收: 'warning',
  已完成: 'success',
  已阻塞: 'danger',
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

function goToRound(roundId: string, requirementId: string): void {
  router.push({
    path: '/workflow-center/rounds',
    query: {
      roundId,
      requirementId,
      repositoryId: selectedRepository.value,
    },
  })
}
</script>

<template>
  <section class="workflow-page">
    <header class="workflow-header">
      <div>
        <h2>任务页面</h2>
        <p>任务是执行最小单元，支持工时、依赖、优先级与状态追踪。</p>
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
    <el-empty v-else-if="!hasData" description="当前仓库暂无任务" />

    <template v-else>
      <section class="summary-grid">
        <el-card>
          <p class="summary-label">任务总量</p>
          <h3>{{ stats.taskTotal }}</h3>
        </el-card>
        <el-card>
          <p class="summary-label">完成任务</p>
          <h3>{{ stats.taskDone }}</h3>
        </el-card>
        <el-card>
          <p class="summary-label">阻塞任务</p>
          <h3>{{ stats.taskBlocked }}</h3>
        </el-card>
      </section>

      <section class="desktop-grid">
        <el-card class="main-panel">
          <template #header>
            <div class="panel-header">任务清单</div>
          </template>
          <el-table :data="tableData" stripe>
            <el-table-column prop="id" label="任务ID" width="120" />
            <el-table-column prop="title" label="任务标题" min-width="260" />
            <el-table-column prop="type" label="类型" width="90" />
            <el-table-column prop="assignee" label="负责人" width="110" />
            <el-table-column label="关联轮次" width="180">
              <template #default="{ row }">
                <span v-if="row.roundIds.length > 0">{{ row.roundIds.join(', ') }}</span>
                <span v-else>未分配轮次</span>
              </template>
            </el-table-column>
            <el-table-column prop="priority" label="优先级" width="90" />
            <el-table-column label="工时" width="120">
              <template #default="{ row }">
                {{ row.spentHours }} / {{ row.estimateHours }}h
              </template>
            </el-table-column>
            <el-table-column label="状态" width="110">
              <template #default="{ row }">
                <el-tag :type="taskStateType[row.state]">{{ row.state }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="跳转" width="190">
              <template #default="{ row }">
                <el-button link type="primary" size="small" @click="goToRequirement(row.requirementId)">
                  需求
                </el-button>
                <el-button
                  v-if="row.roundIds.length > 0"
                  link
                  type="primary"
                  size="small"
                  @click="goToRound(row.roundIds[0], row.requirementId)"
                >
                  轮次
                </el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>

        <el-card class="side-panel">
          <template #header>
            <div class="panel-header">依赖关系</div>
          </template>
          <el-table :data="tableData" size="small">
            <el-table-column prop="id" label="任务" width="120" />
            <el-table-column label="依赖任务">
              <template #default="{ row }">
                <span v-if="row.dependencyTaskIds.length > 0">{{ row.dependencyTaskIds.join(', ') }}</span>
                <span v-else>无依赖</span>
              </template>
            </el-table-column>
          </el-table>
        </el-card>

        <el-card class="side-panel">
          <template #header>
            <div class="panel-header">工时燃尽</div>
          </template>
          <el-progress
            v-for="task in tableData"
            :key="task.id"
            :percentage="Math.min(100, Math.round((task.spentHours / task.estimateHours) * 100))"
            :text-inside="true"
            :stroke-width="18"
            class="burn-item"
          >
            <span>{{ task.id }}</span>
          </el-progress>
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

.burn-item {
  margin-bottom: 0.7rem;
}
</style>
