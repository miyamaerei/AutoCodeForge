<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { useRepoManagementStore } from '../store/useRepoManagementStore'
import { useRepoSyncProgressStore } from '../store/useRepoSyncProgressStore'
import { useRepoStore } from '@/stores/useRepoStore'

const repoStore = useRepoManagementStore()
const repoSyncStore = useRepoSyncProgressStore()
const repoGlobal = useRepoStore()
const router = useRouter()
const { repositories } = storeToRefs(repoStore)

const createTaskDialogVisible = ref(false)
const createTaskFormRef = ref<FormInstance>()
const selectedRepositoryLabel = ref('')

const activityRows = computed(() => {
  return repositories.value.map((repo) => {
    const withUiFields = repo as typeof repo & { branch?: string }
    return {
      id: repo.id,
      repo: repo.name,
      branch: withUiFields.branch || 'main',
      status: '已接入',
    }
  })
})

const createTaskForm = reactive({
  repositoryId: '',
  branch: 'main',
  title: '',
  description: '',
})

const createTaskRules: FormRules = {
  repositoryId: [{ required: true, message: '缺少目标仓库', trigger: 'change' }],
  branch: [{ max: 200, message: '分支长度不能超过 200', trigger: 'blur' }],
  title: [{ max: 200, message: '标题长度不能超过 200', trigger: 'blur' }],
  description: [{ max: 4000, message: '描述长度不能超过 4000', trigger: 'blur' }],
}

function navigateToBranches(row: { id: string }) {
  repoGlobal.selectRepository(row.id)
  router.push({ name: 'repo-management.branches' })
}

function openCreateTaskDialogForRepo(row: { id: string; repo: string; branch: string }) {
  createTaskForm.repositoryId = row.id
  createTaskForm.branch = row.branch || 'main'
  createTaskForm.title = `下载仓库: ${row.repo}`
  createTaskForm.description = ''
  selectedRepositoryLabel.value = row.repo
  createTaskDialogVisible.value = true
}

function resetCreateTaskForm() {
  createTaskForm.repositoryId = ''
  createTaskForm.branch = 'main'
  createTaskForm.title = ''
  createTaskForm.description = ''
  selectedRepositoryLabel.value = ''
  createTaskFormRef.value?.clearValidate()
}

function handleCreateTaskDialogClosed() {
  resetCreateTaskForm()
}

async function submitCreateDownloadTask() {
  const form = createTaskFormRef.value
  if (!form) {
    return
  }
  await form.validate()

  if (!createTaskForm.repositoryId) {
    ElMessage.error('缺少目标仓库，请从仓库行发起同步任务')
    return
  }

  try {
    await repoSyncStore.createTask({
      repositoryId: createTaskForm.repositoryId,
      branch: createTaskForm.branch.trim() || 'main',
      title: createTaskForm.title.trim() || undefined,
      description: createTaskForm.description.trim() || undefined,
    })

    ElMessage.success(`下载仓库任务已创建，任务ID: ${repoSyncStore.currentTaskId}`)
    createTaskDialogVisible.value = false
  } catch (err) {
    const message = err instanceof Error ? err.message : '创建下载仓库任务失败'
    ElMessage.error(message)
  }
}

onMounted(async () => {
  if (!repositories.value.length) {
    await repoStore.loadRepositories()
  }
})
</script>

<template>
  <section class="desktop-page">
    <el-page-header content="项目 / 仓库管理" />
    <el-row :gutter="16" class="desktop-grid">
      <el-col :span="17">
        <el-card shadow="hover" class="panel-card">
          <template #header>
            <strong>仓库总览</strong>
          </template>
          <el-descriptions :column="2" border>
            <el-descriptions-item label="仓库列表">{{ repositories.length }} 个仓库</el-descriptions-item>
            <el-descriptions-item label="默认分支">main</el-descriptions-item>
            <el-descriptions-item label="活跃分支">release / feature/*</el-descriptions-item>
            <el-descriptions-item label="PR列表">最近 12 条变更请求</el-descriptions-item>
            <el-descriptions-item label="代码树">支持目录浏览与快速检索</el-descriptions-item>
            <el-descriptions-item label="最近同步">2 分钟前</el-descriptions-item>
          </el-descriptions>
        </el-card>

        <el-card shadow="hover" class="panel-card">
          <template #header>
            <strong>最近仓库活动</strong>
          </template>
          <el-table :data="activityRows" size="small">
            <el-table-column prop="repo" label="仓库" />
            <el-table-column prop="branch" label="分支" />
            <el-table-column prop="status" label="状态" />
            <el-table-column label="操作" width="200">
              <template #default="{ row }">
                <el-button size="small" type="primary" link @click="navigateToBranches(row)">查看分支</el-button>
                <el-button size="small" type="primary" link @click="openCreateTaskDialogForRepo(row)">同步任务</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>

      <el-col :span="7">
        <el-card shadow="hover" class="panel-card">
          <template #header>
            <strong>快捷操作</strong>
          </template>
          <el-space direction="vertical" fill>
            <el-button type="primary" plain>新建仓库连接</el-button>
            <el-button plain>拉取最新分支</el-button>
            <el-button plain>查看全部 PR</el-button>
          </el-space>
        </el-card>

        <el-card shadow="hover" class="panel-card">
          <template #header>
            <strong>告警提醒</strong>
          </template>
          <el-alert title="web-console 分支落后 12 次提交" type="warning" :closable="false" show-icon />
        </el-card>
      </el-col>
    </el-row>

    <el-dialog
      v-model="createTaskDialogVisible"
      title="添加下载仓库任务"
      width="640px"
      :close-on-click-modal="false"
      @closed="handleCreateTaskDialogClosed"
    >
      <el-form ref="createTaskFormRef" :model="createTaskForm" :rules="createTaskRules" label-width="110px">
        <el-form-item label="目标仓库" prop="repositoryId">
          <el-input :model-value="selectedRepositoryLabel" disabled />
        </el-form-item>

        <el-form-item label="目标分支" prop="branch">
          <el-input v-model="createTaskForm.branch" placeholder="默认 main" />
        </el-form-item>

        <el-form-item label="任务标题" prop="title">
          <el-input v-model="createTaskForm.title" placeholder="可选，最多 200 字符" />
        </el-form-item>

        <el-form-item label="任务描述" prop="description">
          <el-input
            v-model="createTaskForm.description"
            type="textarea"
            :rows="4"
            placeholder="可选，最多 4000 字符"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-space>
          <el-button @click="createTaskDialogVisible = false">取消</el-button>
          <el-button type="primary" :loading="repoSyncStore.submitting" @click="submitCreateDownloadTask">
            创建任务
          </el-button>
        </el-space>
      </template>
    </el-dialog>
  </section>
</template>

<style scoped>
.desktop-page {
  margin-top: 0;
  min-width: 1280px;
}

.desktop-grid {
  margin-top: 0.5rem;
}

.panel-card {
  margin-top: 0.75rem;
}

@media (max-width: 1365px) {
  .desktop-page {
    overflow-x: auto;
    padding-bottom: 8px;
  }
}
</style>
