<script setup lang="ts">
import { onMounted, ref, computed, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useWorkflowStore } from '../store/useWorkflowStore'
import type { WorkflowInstanceStatus, WorkflowEvent } from '../types/workflow'
import { ElMessage, ElMessageBox } from 'element-plus'

const router = useRouter()
const route = useRoute()
const store = useWorkflowStore()
const { currentInstance, loading, error } = storeToRefs(store)

/** 事件列表 */
const events = ref<WorkflowEvent[]>([])

/** 实例ID */
const instanceId = computed(() => route.params.id as string)

onMounted(async () => {
  if (instanceId.value) {
    await store.loadInstance(instanceId.value)
    await loadEvents()
    store.subscribeToInstanceEvents(instanceId.value)
  }
})

onUnmounted(() => {
  if (instanceId.value) {
    store.unsubscribeFromEvents(instanceId.value)
  }
})

/** 加载事件列表 */
const loadEvents = async () => {
  events.value = await store.loadInstanceEvents(instanceId.value)
}

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

/** 事件级别颜色映射 */
const eventLevelColorMap: Record<string, string> = {
  Info: 'info',
  Warning: 'warning',
  Error: 'danger',
}

/** 事件级别中文显示 */
const eventLevelLabelMap: Record<string, string> = {
  Info: '信息',
  Warning: '警告',
  Error: '错误',
}

/** 暂停实例 */
const handlePause = async () => {
  try {
    await store.pauseWorkflowInstance(instanceId.value)
    ElMessage.success('暂停成功')
  } catch (e) {
    ElMessage.error('暂停失败: ' + String(e))
  }
}

/** 恢复实例 */
const handleResume = async () => {
  try {
    await store.resumeWorkflowInstance(instanceId.value)
    ElMessage.success('恢复成功')
  } catch (e) {
    ElMessage.error('恢复失败: ' + String(e))
  }
}

/** 终止实例 */
const handleTerminate = async () => {
  try {
    await ElMessageBox.confirm('确定要终止此实例吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    await store.terminateWorkflowInstance(instanceId.value)
    ElMessage.success('终止成功')
  } catch (e) {
    ElMessage.error('终止失败: ' + String(e))
  }
}

/** 删除实例 */
const handleDelete = async () => {
  try {
    await ElMessageBox.confirm('确定要删除此实例吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    await store.deleteWorkflowInstance(instanceId.value)
    ElMessage.success('删除成功')
    router.push('/workflow-instances')
  } catch (e) {
    ElMessage.error('删除失败: ' + String(e))
  }
}

/** 返回实例列表 */
const handleBack = () => {
  if (currentInstance.value) {
    router.push({
      path: '/workflow-instances',
      query: { workflowId: currentInstance.value.workflowId }
    })
  } else {
    router.push('/workflow-instances')
  }
}

/** 跳转到工作流详情 */
const handleViewWorkflow = () => {
  if (currentInstance.value) {
    router.push(`/workflows/${currentInstance.value.workflowId}`)
  }
}
</script>

<template>
  <section class="workflow-instance-detail">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>实例详情</span>
          <div class="actions">
            <el-button @click="handleBack">返回列表</el-button>
          </div>
        </div>
      </template>

      <el-skeleton v-if="loading" :rows="10" animated />

      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />

      <div v-else-if="!currentInstance" class="empty-container">
        <el-empty description="实例不存在" />
      </div>

      <div v-else class="detail-content">
        <!-- 基本信息 -->
        <div class="section">
          <div class="section-header">
            <span class="section-title">基本信息</span>
            <div class="section-actions">
              <el-button type="primary" size="small" @click="handleViewWorkflow">
                查看工作流
              </el-button>
              <template v-if="currentInstance.status === 'Running'">
                <el-button type="warning" size="small" @click="handlePause">
                  暂停
                </el-button>
                <el-button type="danger" size="small" @click="handleTerminate">
                  终止
                </el-button>
              </template>
              <template v-else-if="currentInstance.status === 'Paused'">
                <el-button type="success" size="small" @click="handleResume">
                  恢复
                </el-button>
                <el-button type="danger" size="small" @click="handleTerminate">
                  终止
                </el-button>
              </template>
              <el-button type="danger" size="small" @click="handleDelete">
                删除
              </el-button>
            </div>
          </div>

          <el-descriptions :column="2" border>
            <el-descriptions-item label="实例ID">{{ currentInstance.id }}</el-descriptions-item>
            <el-descriptions-item label="状态">
              <el-tag :type="statusColorMap[currentInstance.status]">
                {{ statusLabelMap[currentInstance.status] }}
              </el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="工作流ID">
              <el-link type="primary" @click="handleViewWorkflow" style="text-decoration: none">
                {{ currentInstance.workflowId }}
              </el-link>
            </el-descriptions-item>
            <el-descriptions-item label="工作流名称">{{ currentInstance.workflowName }}</el-descriptions-item>
            <el-descriptions-item label="当前节点">{{ currentInstance.currentNodeId || '-' }}</el-descriptions-item>
            <el-descriptions-item label="Agent">{{ currentInstance.agentId || '-' }}</el-descriptions-item>
            <el-descriptions-item label="开始时间">{{ currentInstance.startedAtUtc || '-' }}</el-descriptions-item>
            <el-descriptions-item label="完成时间">{{ currentInstance.completedAtUtc || '-' }}</el-descriptions-item>
            <el-descriptions-item label="创建时间">{{ currentInstance.createdAtUtc }}</el-descriptions-item>
            <el-descriptions-item label="更新时间">{{ currentInstance.updatedAtUtc }}</el-descriptions-item>
          </el-descriptions>
        </div>

        <!-- 进度 -->
        <div class="section">
          <div class="section-header">
            <span class="section-title">进度</span>
          </div>
          <el-progress :percentage="currentInstance.progress" :stroke-width="20" :status="
            currentInstance.status === 'Failed' || currentInstance.status === 'Terminated' 
              ? 'exception' 
              : currentInstance.status === 'Completed' 
                ? 'success' 
                : undefined
          " />
        </div>

        <!-- 错误信息 -->
        <div v-if="currentInstance.errorMessage" class="section">
          <div class="section-header">
            <span class="section-title">错误信息</span>
          </div>
          <el-alert :title="currentInstance.errorMessage" type="error" show-icon />
        </div>

        <!-- 输入/输出 -->
        <div class="section">
          <div class="section-header">
            <span class="section-title">输入 / 输出</span>
          </div>
          <el-row :gutter="20">
            <el-col :span="12">
              <div class="data-panel">
                <div class="panel-title">输入</div>
                <div class="panel-content">
                  <pre><code>{{ currentInstance.inputJson || '{}' }}</code></pre>
                </div>
              </div>
            </el-col>
            <el-col :span="12">
              <div class="data-panel">
                <div class="panel-title">输出</div>
                <div class="panel-content">
                  <pre><code>{{ currentInstance.outputJson || '{}' }}</code></pre>
                </div>
              </div>
            </el-col>
          </el-row>
        </div>

        <!-- 事件列表 -->
        <div class="section">
          <div class="section-header">
            <span class="section-title">事件历史</span>
            <el-button size="small" @click="loadEvents">刷新</el-button>
          </div>
          
          <el-timeline>
            <el-timeline-item
              v-for="event in events"
              :key="event.id"
              :timestamp="event.timestamp"
              placement="top"
              :type="eventLevelColorMap[event.level] || 'info'"
            >
              <div class="event-item">
                <div class="event-header">
                  <el-tag :type="eventLevelColorMap[event.level]" size="small">
                    {{ eventLevelLabelMap[event.level] }}
                  </el-tag>
                  <span class="event-type">{{ event.eventType }}</span>
                </div>
                <div v-if="event.message" class="event-message">{{ event.message }}</div>
                <div v-if="event.nodeId" class="event-node">节点: {{ event.nodeId }}</div>
                <div v-if="event.dataJson" class="event-data">
                  <details>
                    <summary>详情数据</summary>
                    <pre><code>{{ event.dataJson }}</code></pre>
                  </details>
                </div>
              </div>
            </el-timeline-item>
          </el-timeline>
          
          <el-empty v-if="events.length === 0" description="暂无事件" />
        </div>
      </div>
    </el-card>
  </section>
</template>

<style scoped>
.workflow-instance-detail {
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

.section {
  margin-bottom: 1.5rem;
}

.section:last-child {
  margin-bottom: 0;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid var(--el-border-color-lighter);
}

.section-title {
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.section-actions {
  display: flex;
  gap: 0.5rem;
}

.data-panel {
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 8px;
  overflow: hidden;
}

.panel-title {
  padding: 0.75rem 1rem;
  background: var(--el-fill-color-light);
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.panel-content {
  padding: 1rem;
  max-height: 300px;
  overflow: auto;
}

.panel-content pre {
  margin: 0;
}

.panel-content code {
  font-family: 'Monaco', 'Consolas', monospace;
  font-size: 0.9rem;
  color: var(--el-text-color-regular);
  white-space: pre-wrap;
  word-break: break-all;
}

.event-item {
  padding: 0.5rem 0;
}

.event-header {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  margin-bottom: 0.5rem;
}

.event-type {
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.event-message {
  color: var(--el-text-color-regular);
  margin-bottom: 0.25rem;
}

.event-node {
  color: var(--el-text-color-secondary);
  font-size: 0.9rem;
  margin-bottom: 0.25rem;
}

.event-data details {
  margin-top: 0.5rem;
}

.event-data summary {
  cursor: pointer;
  color: var(--el-color-primary);
  font-size: 0.9rem;
}

.event-data pre {
  margin: 0.5rem 0;
  padding: 0.5rem;
  background: var(--el-fill-color-lighter);
  border-radius: 4px;
  overflow-x: auto;
}

.event-data code {
  font-family: 'Monaco', 'Consolas', monospace;
  font-size: 0.85rem;
}

.empty-container {
  padding: 2rem;
}
</style>
