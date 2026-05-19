<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useTaskCenterStore } from '../store/useTaskCenterStore'

const router = useRouter()
const store = useTaskCenterStore()
const { tasks, loading, error, hasTasks } = storeToRefs(store)

onMounted(async () => {
  if (!hasTasks.value) {
    await store.loadTasks()
  }
})

const stateColorMap = {
  '运行中': 'processing',
  '已完成': 'success',
  '已暂停': 'warning',
  '失败': 'danger',
}

const handleRowClick = (taskId: string) => {
  router.push(`/task-center/${taskId}`)
}

const handleCreateTask = () => {
  router.push('/task-center/create')
}

const handleTaskRowClick = (row: { id: string }) => {
  handleRowClick(row.id)
}
</script>

<template>
  <section class="task-list">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>任务列表</span>
          <el-button type="primary" @click="handleCreateTask">+ 创建任务</el-button>
        </div>
      </template>

      <el-skeleton v-if="loading" :rows="6" animated />

      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />

      <el-empty v-else-if="!hasTasks" description="暂无任务" />

      <el-table v-else :data="tasks" stripe @row-click="handleTaskRowClick" style="cursor: pointer">
        <el-table-column prop="id" label="任务ID" width="120" />
        <el-table-column prop="title" label="任务标题" min-width="300" />
        <el-table-column prop="state" label="状态" width="120">
          <template #default="{ row }">
            <el-tag :type="stateColorMap[row.state as keyof typeof stateColorMap]">
              {{ row.state }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="180" />
        <el-table-column label="操作" width="120">
          <template #default="{ row }">
            <el-button link type="primary" size="small" @click.stop="handleRowClick(row.id)">
              查看
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </section>
</template>

<style scoped>
.task-list {
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
</style>
