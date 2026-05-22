<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useRouter } from 'vue-router'
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
  flow,
  filteredRequirements,
  filteredRounds,
  filteredTasks,
} = storeToRefs(store)

onMounted(async () => {
  await store.loadWorkflowData()
  const queryRepositoryId = typeof route.query.repositoryId === 'string' ? route.query.repositoryId : null
  if (queryRepositoryId && queryRepositoryId !== selectedRepository.value) {
    store.setSelectedRepository(queryRepositoryId)
  }
})

const hasData = computed(() => flow.value.length > 0)

function openDecompositionMap(): void {
  router.push({
    path: '/workflow-center/map',
    query: {
      repositoryId: selectedRepository.value,
    },
  })
}

const flowChain = computed(() => [
  '收到需求',
  '拆分任务',
  '多次轮次执行任务',
  '完成所有任务',
  '完成需求',
])
</script>

<template>
  <section class="workflow-page">
    <header class="workflow-header">
      <div>
        <h2>流程页面</h2>
        <p>完整流程：收到需求 → 拆分任务 → 多次轮次执行任务 → 完成所有任务 → 完成需求</p>
      </div>
      <div class="header-actions">
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
        <el-button type="primary" @click="openDecompositionMap">查看仓库拆分图</el-button>
      </div>
    </header>

    <el-skeleton v-if="loading" :rows="8" animated />
    <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />
    <el-empty v-else-if="!hasData" description="暂无流程定义" />

    <template v-else>
      <section class="summary-grid">
        <el-card>
          <p class="summary-label">需求池</p>
          <h3>{{ filteredRequirements.length }}</h3>
        </el-card>
        <el-card>
          <p class="summary-label">活跃轮次</p>
          <h3>{{ filteredRounds.length }}</h3>
        </el-card>
        <el-card>
          <p class="summary-label">执行任务</p>
          <h3>{{ filteredTasks.length }}</h3>
        </el-card>
      </section>

      <section class="desktop-grid">
        <el-card class="main-panel">
          <template #header>
            <div class="panel-header">阶段推进</div>
          </template>
          <div class="flow-chain">
            <div v-for="step in flowChain" :key="step" class="flow-node">
              {{ step }}
            </div>
          </div>
          <el-table :data="flow" stripe>
            <el-table-column prop="title" label="阶段" width="180" />
            <el-table-column prop="description" label="说明" min-width="260" />
            <el-table-column label="完成进度" min-width="220">
              <template #default="{ row }">
                <el-progress
                  :percentage="Math.round((row.completed / row.target) * 100)"
                  :text-inside="true"
                  :stroke-width="16"
                />
              </template>
            </el-table-column>
            <el-table-column label="数量" width="120">
              <template #default="{ row }">{{ row.completed }} / {{ row.target }}</template>
            </el-table-column>
          </el-table>
        </el-card>

        <el-card class="side-panel">
          <template #header>
            <div class="panel-header">关键检查点</div>
          </template>
          <el-steps direction="vertical" :active="4" finish-status="success">
            <el-step title="需求绑定仓库" description="每条需求必须关联 repositoryId" />
            <el-step title="拆分可执行任务" description="任务必须关联 requirementId 和 roundId" />
            <el-step title="多轮次交付" description="每轮记录目标、速度、复盘和阻塞" />
            <el-step title="任务全量验收" description="所有任务状态进入已完成" />
            <el-step title="需求关单" description="输出结果与发布记录" />
          </el-steps>
        </el-card>

        <el-card class="side-panel">
          <template #header>
            <div class="panel-header">风险提醒</div>
          </template>
          <el-alert
            title="高优先级需求仍存在未开始轮次，建议先完成拆分评审。"
            type="warning"
            :closable="false"
            show-icon
          />
          <el-divider />
          <el-alert
            title="发现依赖链任务未闭环，建议在当前轮次内消化。"
            type="error"
            :closable="false"
            show-icon
          />
          <el-divider />
          <div class="quick-links">
            <el-button link type="primary" @click="router.push('/workflow-center/requirements')">
              跳转需求页
            </el-button>
            <el-button link type="primary" @click="router.push('/workflow-center/rounds')">
              跳转轮次页
            </el-button>
            <el-button link type="primary" @click="router.push('/workflow-center/tasks')">
              跳转任务页
            </el-button>
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

.repo-selector {
  width: 280px;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 0.75rem;
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

.flow-chain {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.flow-node {
  border: 1px solid #d5dbe6;
  border-radius: 10px;
  padding: 0.6rem;
  text-align: center;
  background: linear-gradient(180deg, #f8faff, #eef3ff);
  font-size: 0.88rem;
}

.quick-links {
  display: flex;
  flex-direction: column;
  gap: 0.2rem;
}
</style>
