<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useWorkflowStore } from '../store/useWorkflowStore'
import type { WorkflowStatus } from '../types/workflow'

const router = useRouter()
const store = useWorkflowStore()
const { workflows, loading, error, hasWorkflows } = storeToRefs(store)

onMounted(async () => {
  await store.loadWorkflows()
})

/** 状态颜色映射 */
const statusColorMap: Record<WorkflowStatus, string> = {
  Draft: 'info',
  Published: 'success',
  Archived: 'warning',
}

/** 状态中文显示 */
const statusLabelMap: Record<WorkflowStatus, string> = {
  Draft: '草稿',
  Published: '已发布',
  Archived: '已归档',
}

/** 跳转到工作流详情 */
const handleRowClick = (workflowId: string) => {
  router.push(`/workflows/${workflowId}`)
}

/** 跳转到创建工作流 */
const handleCreateWorkflow = () => {
  router.push('/workflows/create')
}

/** 跳转到实例列表 */
const handleViewInstances = (workflowId: string) => {
  router.push({
    path: '/workflow-instances',
    query: { workflowId }
  })
}

/** 执行工作流 */
const handleExecuteWorkflow = async (workflowId: string) => {
  try {
    const instance = await store.executeWorkflow(workflowId)
    router.push(`/workflow-instances/${instance.id}`)
  } catch (e) {
    console.error('执行失败:', e)
  }
}
</script>

<template>
  <section class="workflow-list">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>工作流列表</span>
          <div class="actions">
            <el-button type="primary" @click="handleCreateWorkflow">
              + 创建工作流
            </el-button>
          </div>
        </div>
      </template>

      <el-skeleton v-if="loading" :rows="6" animated />

      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />

      <el-empty v-else-if="!hasWorkflows" description="暂无工作流" />

      <el-table v-else :data="workflows" stripe style="cursor: pointer">
        <el-table-column prop="id" label="ID" width="180" />
        <el-table-column prop="name" label="名称" min-width="200" />
        <el-table-column prop="description" label="描述" min-width="250" show-overflow-tooltip />
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusColorMap[row.status as WorkflowStatus]">
              {{ statusLabelMap[row.status as WorkflowStatus] }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="version" label="版本" width="80" />
        <el-table-column prop="createdAtUtc" label="创建时间" width="180" />
        <el-table-column prop="updatedAtUtc" label="更新时间" width="180" />
        <el-table-column label="操作" width="260" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" size="small" @click.stop="handleRowClick(row.id)">
              详情
            </el-button>
            <el-button link type="success" size="small" @click.stop="handleExecuteWorkflow(row.id)">
              执行
            </el-button>
            <el-button link type="info" size="small" @click.stop="handleViewInstances(row.id)">
              实例
            </el-button>
            <el-button link type="danger" size="small" @click.stop="store.deleteWorkflow(row.id)">
              删除
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </section>
</template>

<style scoped>
.workflow-list {
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

.actions {
  display: flex;
  gap: 0.5rem;
}
</style>
