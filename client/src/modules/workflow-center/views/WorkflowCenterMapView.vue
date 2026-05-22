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
  filteredRequirements,
  filteredRounds,
  filteredTasks,
  repositoryOptions,
  selectedRepository,
} = storeToRefs(store)

onMounted(async () => {
  await store.loadWorkflowData()
  const queryRepositoryId = typeof route.query.repositoryId === 'string' ? route.query.repositoryId : null
  if (queryRepositoryId) {
    store.setSelectedRepository(queryRepositoryId)
  }
})

function handleRepositoryChange(value: string): void {
  store.setSelectedRepository(value)
}

const activeRepositoryId = computed(() => {
  return selectedRepository.value
})

const activeRepositoryInfo = computed(
  () => repositoryOptions.value.find((repo) => repo.id === selectedRepository.value) ?? null,
)

const repoRequirements = computed(() => filteredRequirements.value)

const repoRounds = computed(() => filteredRounds.value)

const repoTasks = computed(() => filteredTasks.value)

const hasData = computed(() => repoRequirements.value.length > 0)

const requirementMap = computed(() => {
  return repoRequirements.value.map((requirement) => {
    const relatedRounds = repoRounds.value.filter((round) => round.requirementId === requirement.id)
    const relatedTasks = repoTasks.value.filter((task) => task.requirementId === requirement.id)
    const unassignedTasks = relatedTasks.filter((task) => task.roundIds.length === 0)
    const roundTaskCount = new Map<string, number>()

    relatedRounds.forEach((round) => {
      const count = relatedTasks.filter((task) => task.roundIds.includes(round.id)).length
      roundTaskCount.set(round.id, count)
    })

    return {
      requirement,
      rounds: relatedRounds,
      tasks: relatedTasks,
      unassignedTasks,
      roundTaskCount,
    }
  })
})

function goToFlow(): void {
  router.push('/workflow-center/flow')
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

function goToRound(requirementId: string): void {
  router.push({
    path: '/workflow-center/rounds',
    query: {
      requirementId,
      repositoryId: selectedRepository.value,
    },
  })
}

function goToTask(requirementId: string): void {
  router.push({
    path: '/workflow-center/tasks',
    query: {
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
        <h2>单仓库拆分关系图</h2>
        <p>聚焦一个仓库，展示需求 -> 轮次 -> 任务的拆分与执行映射。</p>
      </div>
      <div class="header-actions">
        <el-select
          :model-value="activeRepositoryId"
          class="repo-selector"
          placeholder="选择仓库"
          @update:model-value="handleRepositoryChange"
        >
          <el-option label="全部仓库" value="all" />
          <el-option
            v-for="repo in repositoryOptions"
            :key="repo.id"
            :label="repo.name"
            :value="repo.id"
          />
        </el-select>
        <el-button plain @click="goToFlow">返回流程页</el-button>
      </div>
    </header>

    <el-skeleton v-if="loading" :rows="8" animated />
    <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />
    <el-empty v-else-if="repositoryOptions.length === 0" description="暂无可用仓库" />
    <el-empty v-else-if="!hasData" description="当前仓库暂无拆分数据" />

    <template v-else>
      <el-card class="repo-overview">
        <div class="repo-overview-grid">
          <div>
            <p class="overview-label">当前仓库</p>
            <h3>{{ activeRepositoryInfo?.name || '全部仓库' }}</h3>
            <p class="overview-meta">{{ activeRepositoryId }}</p>
          </div>
          <div>
            <p class="overview-label">需求</p>
            <h4>{{ repoRequirements.length }}</h4>
          </div>
          <div>
            <p class="overview-label">轮次</p>
            <h4>{{ repoRounds.length }}</h4>
          </div>
          <div>
            <p class="overview-label">任务</p>
            <h4>{{ repoTasks.length }}</h4>
          </div>
          <div>
            <p class="overview-label">未分配轮次任务</p>
            <h4>{{ repoTasks.filter((task) => task.roundIds.length === 0).length }}</h4>
          </div>
        </div>
      </el-card>

      <section class="lane-grid">
        <el-card class="lane-card lane-requirement">
          <template #header>
            <div class="lane-title">需求</div>
          </template>
          <div v-for="item in requirementMap" :key="item.requirement.id" class="lane-item">
            <div class="item-head">
              <strong>{{ item.requirement.id }}</strong>
              <el-tag size="small">{{ item.requirement.state }}</el-tag>
            </div>
            <p class="item-title">{{ item.requirement.title }}</p>
            <div class="node-actions">
              <el-button size="small" @click="goToRequirement(item.requirement.id)">查看需求页</el-button>
            </div>
          </div>
        </el-card>

        <el-card class="lane-card lane-round">
          <template #header>
            <div class="lane-title">轮次</div>
          </template>
          <div v-for="item in requirementMap" :key="`${item.requirement.id}-round`" class="lane-item">
            <div class="item-head">
              <strong>{{ item.requirement.id }}</strong>
              <span class="item-meta">{{ item.rounds.length }} 个轮次</span>
            </div>
            <ul>
              <li v-for="round in item.rounds" :key="round.id" class="round-line">
                <span>{{ round.id }} · {{ round.sprintName }}</span>
                <el-tag size="small" type="info">任务 {{ item.roundTaskCount.get(round.id) ?? 0 }}</el-tag>
              </li>
              <li v-if="item.rounds.length === 0" class="empty-line">无轮次</li>
            </ul>
            <div class="node-actions">
              <el-button size="small" @click="goToRound(item.requirement.id)">查看轮次页</el-button>
            </div>
          </div>
        </el-card>

        <el-card class="lane-card lane-task">
          <template #header>
            <div class="lane-title">任务</div>
          </template>
          <div v-for="item in requirementMap" :key="`${item.requirement.id}-task`" class="lane-item">
            <div class="item-head">
              <strong>{{ item.requirement.id }}</strong>
              <span class="item-meta">{{ item.tasks.length }} 个任务</span>
            </div>
            <ul>
              <li v-for="task in item.tasks.slice(0, 4)" :key="task.id" class="task-line">
                <span>{{ task.id }} · {{ task.title }}</span>
                <el-tag v-if="task.roundIds.length === 0" size="small" type="warning">未分配轮次</el-tag>
              </li>
              <li v-if="item.tasks.length === 0" class="empty-line">无任务</li>
            </ul>
            <div class="node-actions">
              <el-button size="small" type="primary" @click="goToTask(item.requirement.id)">
                查看任务页
              </el-button>
            </div>
          </div>
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

.header-actions {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.repo-selector {
  width: 280px;
}

.repo-overview {
  border: 1px solid #d6deea;
  background: linear-gradient(135deg, #f8fbff, #eef5ff);
}

.repo-overview-grid {
  display: grid;
  grid-template-columns: 2.3fr repeat(4, 1fr);
  gap: 0.75rem;
  align-items: end;
}

.overview-label {
  margin: 0;
  color: #6e7890;
  font-size: 0.82rem;
}

.repo-overview h3,
.repo-overview h4 {
  margin: 0.2rem 0 0;
}

.overview-meta {
  margin: 0.25rem 0 0;
  color: #8a95ab;
}

.lane-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 1rem;
  min-width: 1280px;
  overflow-x: auto;
}

.lane-card {
  border: 1px solid #dce3ef;
}

.lane-requirement {
  background: linear-gradient(180deg, #f8fffb, #effff4);
}

.lane-round {
  background: linear-gradient(180deg, #fffdf8, #fff5ea);
}

.lane-task {
  background: linear-gradient(180deg, #fff9f9, #fff0f0);
}

.lane-title {
  font-weight: 700;
}

.lane-item {
  border: 1px solid #d8e0ec;
  border-radius: 10px;
  background: #fff;
  padding: 0.7rem;
  margin-bottom: 0.8rem;
}

.item-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.5rem;
}

.item-title {
  margin: 0.45rem 0;
  color: #2f3848;
}

.item-meta {
  color: #79859a;
  font-size: 0.82rem;
}

.lane-item ul {
  margin: 0.5rem 0;
  padding-left: 1rem;
}

.round-line,
.task-line {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.5rem;
  color: #4c586d;
  margin-bottom: 0.3rem;
}

.empty-line {
  color: #9aa5b8;
}

.node-actions {
  display: flex;
  justify-content: flex-end;
}
</style>
