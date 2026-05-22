<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { ElMessage } from 'element-plus'
import { useRepoManagementStore } from '../store/useRepoManagementStore'
import { useRepoSyncProgressStore } from '../store/useRepoSyncProgressStore'

const repoStore = useRepoManagementStore()
const progressStore = useRepoSyncProgressStore()

const { repositories, branches } = storeToRefs(repoStore)
const {
  currentTaskId,
  taskDetail,
  loading,
  submitting,
  canceling,
  error,
  hasTaskDetail,
  progressPercent,
  normalizedTaskStatus,
  normalizedWorkspaceStatus,
  canCancel,
} = storeToRefs(progressStore)

const createForm = reactive({
  repositoryId: '',
  branch: '',
  title: '',
  description: '',
})

const statusTagType = computed(() => {
  if (normalizedTaskStatus.value === 'COMPLETED') {
    return 'success'
  }
  if (normalizedTaskStatus.value === 'FAILED') {
    return 'danger'
  }
  if (normalizedTaskStatus.value === 'CANCELED') {
    return 'warning'
  }
  return 'info'
})

const workspaceTagType = computed(() => {
  if (normalizedWorkspaceStatus.value === 'PULLED') {
    return 'success'
  }
  if (normalizedWorkspaceStatus.value === 'FAILED') {
    return 'danger'
  }
  if (normalizedWorkspaceStatus.value === 'CLEANED') {
    return 'warning'
  }
  return 'info'
})

async function loadBranchesForRepo() {
  if (!createForm.repositoryId) return
  await repoStore.loadBranchesForRepo(createForm.repositoryId)
  createForm.branch = branches.value[0]?.name ?? ''
}

async function ensureRepositoriesLoaded() {
  if (repositories.value.length > 0) {
    return
  }
  await repoStore.loadRepositories()
  if (!createForm.repositoryId && repositories.value.length > 0 && repositories.value[0]?.id) {
    createForm.repositoryId = repositories.value[0].id
  }
}

watch(() => createForm.repositoryId, async (newId) => {
  if (newId) {
    await loadBranchesForRepo()
  }
})

async function handleCreateTask() {
  if (!createForm.repositoryId) {
    ElMessage.warning('请先选择仓库')
    return
  }

  try {
    await progressStore.createTask({
      repositoryId: createForm.repositoryId,
      branch: createForm.branch.trim(),
      // branch fallback handled server-side; do not silently override to 'main'
      title: createForm.title.trim() || undefined,
      description: createForm.description.trim() || undefined,
    })
    ElMessage.success('同步任务已创建')
  } catch (err) {
    const message = err instanceof Error ? err.message : '创建任务失败'
    ElMessage.error(message)
  }
}

async function handleQueryTask() {
  if (!currentTaskId.value.trim()) {
    ElMessage.warning('请输入任务 ID')
    return
  }

  try {
    await progressStore.refreshTaskDetail(currentTaskId.value)
    ElMessage.success('进度已刷新')
  } catch (err) {
    const message = err instanceof Error ? err.message : '查询失败'
    ElMessage.error(message)
  }
}

async function handleCancelTask() {
  try {
    await progressStore.cancelTask()
    ElMessage.success('任务已取消')
  } catch (err) {
    const message = err instanceof Error ? err.message : '取消任务失败'
    ElMessage.error(message)
  }
}

onMounted(async () => {
  await ensureRepositoriesLoaded()
  if (!createForm.repositoryId && repositories.value.length > 0 && repositories.value[0]?.id) {
    createForm.repositoryId = repositories.value[0].id
    await loadBranchesForRepo()
  }
})

onBeforeUnmount(() => {
  progressStore.stopAutoRefresh()
})
</script>

<template>
  <section class="repo-sync-progress-view">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>Repo 同步任务</span>
          <el-tag type="info">独立进度页面</el-tag>
        </div>
      </template>

      <el-form label-width="120px" class="create-form">
        <el-form-item label="仓库">
          <el-select v-model="createForm.repositoryId" placeholder="选择仓库" style="width: 360px">
            <el-option
              v-for="repo in repositories"
              :key="repo.id"
              :label="repo.name"
              :value="repo.id"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="分支">
          <el-select v-model="createForm.branch" placeholder="请选择分支" style="width: 360px" :loading="false">
            <el-option
              v-for="b in branches"
              :key="b.name"
              :label="b.name"
              :value="b.name"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="任务标题">
          <el-input v-model="createForm.title" placeholder="可选" style="width: 520px" />
        </el-form-item>

        <el-form-item label="任务描述">
          <el-input v-model="createForm.description" type="textarea" :rows="2" placeholder="可选" style="width: 520px" />
        </el-form-item>

        <el-form-item>
          <el-button type="primary" :loading="submitting" @click="handleCreateTask">创建同步任务</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>任务进度跟踪</span>
          <el-space>
            <el-button :loading="loading" @click="handleQueryTask">刷新</el-button>
            <el-button type="danger" plain :loading="canceling" :disabled="!canCancel" @click="handleCancelTask">取消任务</el-button>
          </el-space>
        </div>
      </template>

      <el-form inline>
        <el-form-item label="任务 ID">
          <el-input v-model="currentTaskId" placeholder="输入 taskId 后查询" style="width: 460px" />
        </el-form-item>
      </el-form>

      <el-alert v-if="error" :title="error" type="error" show-icon :closable="false" class="status-alert" />
      <el-empty v-else-if="!hasTaskDetail" description="尚未查询任务详情" />

      <div v-else class="status-panel">
        <el-progress :percentage="progressPercent" :stroke-width="14" status="success" />

        <div class="status-grid">
          <div class="status-item">
            <span class="label">任务状态</span>
            <el-tag :type="statusTagType">{{ taskDetail?.status }}</el-tag>
          </div>
          <div class="status-item">
            <span class="label">工作区状态</span>
            <el-tag :type="workspaceTagType">{{ taskDetail?.workspaceStatus }}</el-tag>
          </div>
          <div class="status-item">
            <span class="label">分支</span>
            <span>{{ taskDetail?.branch || '-' }}</span>
          </div>
          <div class="status-item">
            <span class="label">Commit SHA</span>
            <span>{{ taskDetail?.commitSha || '-' }}</span>
          </div>
          <div class="status-item full-row">
            <span class="label">Sandbox 路径</span>
            <span class="path">{{ taskDetail?.effectiveSandboxPath || '-' }}</span>
          </div>
          <div class="status-item full-row" v-if="taskDetail?.errorMessage">
            <span class="label">错误信息</span>
            <span class="error-text">{{ taskDetail.errorMessage }}</span>
          </div>
        </div>
      </div>
    </el-card>
  </section>
</template>

<style scoped>
.repo-sync-progress-view {
  min-width: 1280px;
}

.content-card {
  margin-bottom: 1rem;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.create-form {
  margin-top: 0.5rem;
}

.status-alert {
  margin-top: 0.75rem;
}

.status-panel {
  margin-top: 0.75rem;
}

.status-grid {
  margin-top: 1rem;
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.75rem 1rem;
}

.status-item {
  border: 1px solid var(--el-border-color);
  border-radius: 8px;
  padding: 0.75rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.full-row {
  grid-column: 1 / -1;
}

.label {
  color: var(--el-text-color-secondary);
  font-size: 13px;
}

.path {
  font-family: 'Consolas', 'Courier New', monospace;
  word-break: break-all;
}

.error-text {
  color: var(--el-color-danger);
}
</style>
