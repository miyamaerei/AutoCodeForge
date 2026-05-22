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
  filteredRequirements,
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

const tableData = computed(() => {
  if (!activeRequirementId.value) {
    return filteredRequirements.value
  }
  return filteredRequirements.value.filter((item) => item.id === activeRequirementId.value)
})

const hasData = computed(() => tableData.value.length > 0)

const selectedRequirement = computed(() => tableData.value[0] ?? null)

const stateTagType: Record<string, 'success' | 'warning' | 'danger' | 'info'> = {
  待分析: 'info',
  执行中: 'warning',
  已完成: 'success',
  已阻塞: 'danger',
}

function goToRounds(requirementId: string): void {
  router.push({
    path: '/workflow-center/rounds',
    query: {
      requirementId,
      repositoryId: selectedRepository.value,
    },
  })
}

function goToTasks(requirementId: string): void {
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
        <h2>需求页面</h2>
        <p>需求必须绑定仓库，支持按仓库筛选和追踪验收标准。</p>
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

    <el-empty v-else-if="!hasData" description="当前仓库暂无需求数据" />

    <template v-else>
      <section class="summary-grid">
        <el-card>
          <p class="summary-label">需求总数</p>
          <h3>{{ stats.requirementTotal }}</h3>
        </el-card>
        <el-card>
          <p class="summary-label">已完成需求</p>
          <h3>{{ stats.requirementDone }}</h3>
        </el-card>
        <el-card>
          <p class="summary-label">关联任务</p>
          <h3>{{ stats.taskTotal }}</h3>
        </el-card>
      </section>

      <section class="desktop-grid">
        <el-card class="main-panel">
          <template #header>
            <div class="panel-header">需求列表</div>
          </template>
          <el-table :data="tableData" stripe>
            <el-table-column prop="id" label="需求ID" width="110" />
            <el-table-column prop="repositoryName" label="仓库" width="220" />
            <el-table-column prop="title" label="需求标题" min-width="260" />
            <el-table-column prop="priority" label="优先级" width="90" />
            <el-table-column prop="businessValue" label="商业价值" width="110" />
            <el-table-column prop="owner" label="负责人" width="120" />
            <el-table-column label="状态" width="110">
              <template #default="{ row }">
                <el-tag :type="stateTagType[row.state]">{{ row.state }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="跳转" width="180">
              <template #default="{ row }">
                <el-button link type="primary" size="small" @click="goToRounds(row.id)">轮次</el-button>
                <el-button link type="primary" size="small" @click="goToTasks(row.id)">任务</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>

        <el-card class="side-panel">
          <template #header>
            <div class="panel-header">验收标准</div>
          </template>
          <el-timeline>
            <el-timeline-item
              v-for="item in selectedRequirement?.acceptanceCriteria ?? []"
              :key="item"
              type="primary"
            >
              {{ item }}
            </el-timeline-item>
          </el-timeline>
        </el-card>

        <el-card class="side-panel">
          <template #header>
            <div class="panel-header">仓库关联信息</div>
          </template>
          <el-descriptions :column="1" border>
            <el-descriptions-item label="仓库名">
              {{ selectedRequirement?.repositoryName }}
            </el-descriptions-item>
            <el-descriptions-item label="仓库地址">
              <a :href="selectedRequirement?.repositoryUrl" target="_blank">查看仓库</a>
            </el-descriptions-item>
            <el-descriptions-item label="干系人">
              {{ selectedRequirement?.stakeholders?.join(' / ') }}
            </el-descriptions-item>
            <el-descriptions-item label="发布时间">
              {{ selectedRequirement?.expectedRelease }}
            </el-descriptions-item>
          </el-descriptions>
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
</style>
