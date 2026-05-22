<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import { ElMessage } from 'element-plus'
import { useTaskCenterStore } from '../store/useTaskCenterStore'
import RepositoryBranchSelector from '@/components/repository/RepositoryBranchSelector.vue'

const router = useRouter()
const store = useTaskCenterStore()
const { creating, error } = storeToRefs(store)

interface TaskForm {
  title: string
  taskType: string
  description: string
  repository: string
  branch: string
}

const taskTypeOptions = [
  { value: 'requirement', label: '需求开发' },
  { value: 'bug-fix', label: '修复 Bug' },
  { value: 'data-sync', label: '同步数据' },
  { value: 'auto-unit-test', label: '自动添加单元测试' },
  { value: 'api-integration', label: 'API 对接' },
  { value: 'code-refactor', label: '代码重构' },
  { value: 'performance-opt', label: '性能优化' },
  { value: 'doc-generation', label: '文档生成' },
  { value: 'code-review', label: '代码审查' },
  { value: 'other-ai-dev', label: '其他 AI 开发任务' },
] as const

const form = ref<TaskForm>({
  title: '',
  taskType: 'requirement',
  description: '',
  repository: '',
  branch: '',
})

const handleSubmit = async () => {
  if (!form.value.title || !form.value.taskType || !form.value.description || !form.value.repository || !form.value.branch) {
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

        <el-form-item label="任务类型" required>
          <el-select v-model="form.taskType" placeholder="请选择任务类型" style="width: 100%">
            <el-option
              v-for="option in taskTypeOptions"
              :key="option.value"
              :label="option.label"
              :value="option.value"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="任务描述" required>
          <el-input
            v-model="form.description"
            type="textarea"
            :rows="8"
            placeholder="详细描述你的需求..."
          />
        </el-form-item>

        <RepositoryBranchSelector
          v-model:repository-id="form.repository"
          v-model:branch="form.branch"
          repository-label="选择仓库"
          branch-label="选择分支"
          repository-placeholder="选择目标仓库"
          branch-placeholder="选择目标分支"
          :disabled="creating"
        />

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
