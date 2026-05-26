<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useWorkflowStore } from '../store/useWorkflowStore'
import type { WorkflowInstanceStatus } from '../types/workflow'
import { ElMessage, ElMessageBox } from 'element-plus'

const router = useRouter()
const route = useRoute()
const store = useWorkflowStore()
const { instances, loading, error, hasInstances } = storeToRefs(store)

/** 当前工作流ID（从查询参数获取） */
const currentWorkflowId = computed(() => route.query.workflowId as string)

onMounted(async () => {
  if (currentWorkflowId.value) {
    await store.loadWorkflowInstances(currentWorkflowId.value)
  } else {
    await store.loadRecentInstances()
  }
})

/** 状态颜色映射 */
const statusColorMap: Record<WorkflowInstanceStatus, string> = {
  Pending: 'info',
  Running: 'primary',
  Paused: 'warning',
  Completed: 'success',
  Failed: 'danger',
  Terminated: 'danger',
}

/** 状态中文显示 */
const statusLabelMap: Record<WorkflowInstanceStatus, string> = {
  Pending: '待处理',
  Running: '运行中',
  Paused: '已暂停',
  Completed: '已完成',
  Failed: '失败',
  Terminated: '已终止',
}

/** 跳转到详情 */
const handleRowClick = (instanceId: string) => {
  router.push(`/workflow-instances/${instanceId}`)
}

/** 暂停实例 */
const handlePause = async (instanceId: string) => {
  try {
    await store.pauseWorkflowInstance(instanceId)
    ElMessage.success('暂停成功')
  } catch (e) {
    ElMessage.error('暂停失败: ' + String(e))
  }
}

/** 恢复实例 */
const handleResume = async (instanceId: string) => {
  try {
    await store.resumeWorkflowInstance(instanceId)
    ElMessage.success('恢复成功')
  } catch (e) {
    ElMessage.error('恢复失败: ' + String(e))
  }
}

/** 终止实例 */
const handleTerminate = async (instanceId: string) => {
  try {
    await ElMessageBox.confirm('确定要终止此实例吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    await store.terminateWorkflowInstance(instanceId)
    ElMessage.success('终止成功')
  } catch (e) {
    ElMessage.error('终止失败: ' + String(e))
  }
}

/** 删除实例 */
const handleDelete = async (instanceId: string) => {
  try {
    await ElMessageBox.confirm('确定要删除此实例吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    await store.deleteWorkflowInstance(instanceId)
    ElMessage.success('删除成功')
  } catch (e) {
    ElMessage.error('删除失败: ' + String(e))
  }
}

/** 返回工作流列表 */
const handleBack = () => {
  router.push('/workflows')
}
</script>

<template>
  <section class="workflow-instance-list">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>{{ currentWorkflowId ? '工作流实例列表' : '最近实例列表' }}</span>
          <div class="actions">
            <el-button @click="handleBack">返回工作流</el-button>
          </div>
        </div>
      </template>

      <el-skeleton v-if="loading" :rows="6" animated />

      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />

      <el-empty v-else-if="!hasInstances" description="暂无实例" />

      <el-table v-else :data="instances" stripe style="cursor: pointer">
        <el-table-column prop="id" label="实例ID" width="180" />
        <el-table-column prop="workflowId" label="工作流ID" width="180" show-overflow-tooltip />
        <el-table-column prop="workflowName" label="工作流名称" min-width="150" show-overflow-tooltip />
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusColorMap[row.status as WorkflowInstanceStatus]">
              {{ statusLabelMap[row.status as WorkflowInstanceStatus] }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="progress" label="进度" width="100">
          <template #default="{ row }">
            <el-progress :percentage="row.progress" :stroke-width="8" />
          </template>
        </el-table-column>
        <el-table-column prop="agentId" label="Agent" width="150" show-overflow-tooltip />
        <el-table-column prop="createdAtUtc" label="创建时间" width="180" />
        <el-table-column label="操作" width="280" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" size="small" @click.stop="handleRowClick(row.id)">
              详情
            </el-button>
            <template v-if="row.status === 'Running'">
              <el-button link type="warning" size="small" @click.stop="handlePause(row.id)">
                暂停
              </el-button>
              <el-button link type="danger" size="small" @click.stop="handleTerminate(row.id)">
                终止
              </el-button>
            </template>
            <template v-else-if="row.status === 'Paused'">
              <el-button link type="success" size="small" @click.stop="handleResume(row.id)">
                恢复
              </el-button>
              <el-button link type="danger" size="small" @click.stop="handleTerminate(row.id)">
                终止
              </el-button>
            </template>
            <el-button link type="danger" size="small" @click.stop="handleDelete(row.id)">
              删除
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </section>
</template>

<style scoped>
.workflow-instance-list {
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
