<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import { ElMessage } from 'element-plus'
import { useTaskCenterStore } from '../store/useTaskCenterStore'
import { useRepoStore } from '@/stores/useRepoStore'

const router = useRouter()
const store = useTaskCenterStore()
const { creating, error } = storeToRefs(store)

interface TaskForm {
  title: string
  description: string
  repository: string
  branch: string
}

const form = ref<TaskForm>({
  title: '',
  description: '',
  repository: 'AutoCodeForge',
  branch: 'main',
})

const repoGlobal = useRepoStore()

onMounted(() => {
  // initialize form repository from global selection if available
  if (repoGlobal.selectedRepositoryId) {
    form.value.repository = repoGlobal.selectedRepositoryId
  }
})

function handleRepoChange(val: string) {
  repoGlobal.selectRepository(val)
}

const repositories = [
  { label: 'AutoCodeForge', value: 'AutoCodeForge' },
  { label: 'backend-service', value: 'backend-service' },
  { label: 'mobile-app', value: 'mobile-app' },
]

const branches = [
  { label: 'main', value: 'main' },
  { label: 'develop', value: 'develop' },
  { label: 'staging', value: 'staging' },
]

const handleSubmit = async () => {
  if (!form.value.title || !form.value.description) {
    ElMessage.error('请填写必要字段')
    return
  }

  const task = await store.submitTask(form.value)
  if (!task) {
    ElMessage.error(error.value || '创建任务失败')
    return
  }
  ElMessage.success('任务已创建，跳转到任务详情页...')
  await router.push(`/task-center/${task.id}`)
}

const handleCancel = () => {
  router.back()
}
</script>

<template>
  <section class="task-create">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>创建新任务</span>
        </div>
      </template>

      <el-form :model="form" label-width="120px" @submit.prevent="handleSubmit">
        <el-alert v-if="error" :title="error" type="error" show-icon :closable="false" class="form-alert" />

        <el-form-item label="任务标题" required>
          <el-input v-model="form.title" placeholder="请输入任务标题，如：修复订单导出功能" />
        </el-form-item>

        <el-form-item label="任务描述" required>
          <el-input
            v-model="form.description"
            type="textarea"
            :rows="8"
            placeholder="详细描述你的需求..."
          />
        </el-form-item>

        <el-form-item label="选择仓库">
          <el-select v-model="form.repository" placeholder="选择目标仓库" @change="handleRepoChange">
            <el-option v-for="repo in repositories" :key="repo.value" :label="repo.label" :value="repo.value" />
          </el-select>
        </el-form-item>

        <el-form-item label="选择分支">
          <el-select v-model="form.branch" placeholder="选择目标分支">
            <el-option v-for="branch in branches" :key="branch.value" :label="branch.label" :value="branch.value" />
          </el-select>
        </el-form-item>

        <el-form-item>
          <el-space>
            <el-button type="primary" :loading="creating" @click="handleSubmit">创建任务</el-button>
            <el-button @click="handleCancel">取消</el-button>
          </el-space>
        </el-form-item>
      </el-form>
    </el-card>
  </section>
</template>

<style scoped>
.task-create {
  min-width: 1280px;
  max-width: 800px;
}

.content-card {
  margin-bottom: 1rem;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.form-alert {
  margin-bottom: 12px;
}
</style>
