<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useWorkflowStore } from '../store/useWorkflowStore'
import type { WorkflowStatus, UpdateWorkflowRequest } from '../types/workflow'
import { ElMessage, ElMessageBox } from 'element-plus'

const router = useRouter()
const route = useRoute()
const store = useWorkflowStore()
const { currentWorkflow, loading, error } = storeToRefs(store)

/** 编辑模式 */
const isEditing = ref(false)

/** 编辑表单 */
const editForm = ref<UpdateWorkflowRequest>({})

/** 工作流ID */
const workflowId = computed(() => route.params.id as string)

onMounted(async () => {
  if (workflowId.value) {
    await store.loadWorkflow(workflowId.value)
  }
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

/** 进入编辑模式 */
const handleEdit = () => {
  if (!currentWorkflow.value) return
  editForm.value = {
    name: currentWorkflow.value.name,
    description: currentWorkflow.value.description,
    contextProviders: currentWorkflow.value.contextProviders
  }
  isEditing.value = true
}

/** 取消编辑 */
const handleCancelEdit = () => {
  isEditing.value = false
}

/** 保存编辑 */
const handleSaveEdit = async () => {
  try {
    await store.updateWorkflow(workflowId.value, editForm.value)
    isEditing.value = false
    ElMessage.success('保存成功')
  } catch (e) {
    ElMessage.error('保存失败: ' + String(e))
  }
}

/** 发布工作流 */
const handlePublish = async () => {
  try {
    await store.updateWorkflow(workflowId.value, { status: 'Published' })
    ElMessage.success('发布成功')
  } catch (e) {
    ElMessage.error('发布失败: ' + String(e))
  }
}

/** 归档工作流 */
const handleArchive = async () => {
  try {
    await ElMessageBox.confirm('确定要归档此工作流吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    await store.updateWorkflow(workflowId.value, { status: 'Archived' })
    ElMessage.success('归档成功')
  } catch (e) {
    ElMessage.error('归档失败: ' + String(e))
  }
}

/** 删除工作流 */
const handleDelete = async () => {
  try {
    await ElMessageBox.confirm('确定要删除此工作流吗？', '提示', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'warning'
    })
    await store.deleteWorkflow(workflowId.value)
    ElMessage.success('删除成功')
    router.push('/workflows')
  } catch (e) {
    ElMessage.error('删除失败: ' + String(e))
  }
}

/** 执行工作流 */
const handleExecute = async () => {
  try {
    const instance = await store.executeWorkflow(workflowId.value)
    ElMessage.success('执行成功')
    router.push(`/workflow-instances/${instance.id}`)
  } catch (e) {
    ElMessage.error('执行失败: ' + String(e))
  }
}

/** 返回列表 */
const handleBack = () => {
  router.push('/workflows')
}

/** 查看实例列表 */
const handleViewInstances = () => {
  router.push({
    path: '/workflow-instances',
    query: { workflowId: workflowId.value }
  })
}
</script>

<template>
  <section class="workflow-detail">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>工作流详情</span>
          <div class="actions">
            <el-button @click="handleBack">返回列表</el-button>
          </div>
        </div>
      </template>

      <el-skeleton v-if="loading" :rows="10" animated />

      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />

      <div v-else-if="!currentWorkflow" class="empty-container">
        <el-empty description="工作流不存在" />
      </div>

      <div v-else class="detail-content">
        <!-- 工作流信息 -->
        <div class="section">
          <div class="section-header">
            <span class="section-title">基本信息</span>
            <div class="section-actions">
              <template v-if="!isEditing">
                <el-button type="primary" size="small" @click="handleEdit">编辑</el-button>
                <el-button type="success" size="small" @click="handleExecute" :disabled="currentWorkflow.status === 'Archived'">
                  执行
                </el-button>
                <el-button type="info" size="small" @click="handleViewInstances">实例</el-button>
                <el-button v-if="currentWorkflow.status === 'Draft'" type="success" size="small" @click="handlePublish">
                  发布
                </el-button>
                <el-button v-if="currentWorkflow.status !== 'Archived'" type="warning" size="small" @click="handleArchive">
                  归档
                </el-button>
                <el-button type="danger" size="small" @click="handleDelete">删除</el-button>
              </template>
              <template v-else>
                <el-button type="primary" size="small" @click="handleSaveEdit">保存</el-button>
                <el-button size="small" @click="handleCancelEdit">取消</el-button>
              </template>
            </div>
          </div>

          <el-descriptions :column="2" border>
            <el-descriptions-item label="ID">{{ currentWorkflow.id }}</el-descriptions-item>
            <el-descriptions-item label="状态">
              <el-tag :type="statusColorMap[currentWorkflow.status]">
                {{ statusLabelMap[currentWorkflow.status] }}
              </el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="版本">{{ currentWorkflow.version }}</el-descriptions-item>
            <el-descriptions-item label="创建时间">{{ currentWorkflow.createdAtUtc }}</el-descriptions-item>
            <el-descriptions-item label="更新时间" :span="2">{{ currentWorkflow.updatedAtUtc }}</el-descriptions-item>
          </el-descriptions>

          <!-- 名称和描述 -->
          <div class="form-row">
            <div class="form-item">
              <label>工作流名称</label>
              <template v-if="!isEditing">
                <div class="field-value">{{ currentWorkflow.name }}</div>
              </template>
              <template v-else>
                <el-input v-model="editForm.name" placeholder="请输入名称" />
              </template>
            </div>
          </div>

          <div class="form-row">
            <div class="form-item">
              <label>描述</label>
              <template v-if="!isEditing">
                <div class="field-value">{{ currentWorkflow.description || '-' }}</div>
              </template>
              <template v-else>
                <el-input v-model="editForm.description" type="textarea" :rows="3" placeholder="请输入描述" />
              </template>
            </div>
          </div>

          <div class="form-row">
            <div class="form-item">
              <label>上下文提供者</label>
              <template v-if="!isEditing">
                <div class="tag-list">
                  <el-tag v-for="provider in currentWorkflow.contextProviders" :key="provider" size="small" style="margin-right: 0.5rem">
                    {{ provider }}
                  </el-tag>
                  <span v-if="!currentWorkflow.contextProviders?.length">-</span>
                </div>
              </template>
              <template v-else>
                <el-select v-model="editForm.contextProviders" multiple placeholder="选择上下文提供者" style="width: 100%">
                  <el-option label="默认提供者" value="default" />
                  <el-option label="Git信息提供者" value="git-info" />
                  <el-option label="任务信息提供者" value="task-info" />
                </el-select>
              </template>
            </div>
          </div>
        </div>

        <!-- 工作流设计器 -->
        <div class="section">
          <div class="section-header">
            <span class="section-title">工作流设计</span>
          </div>
          
          <div class="designer-placeholder">
            <el-empty description="工作流设计器功能即将上线">
              <template #image>
                <div class="workflow-icon">
                  <el-icon :size="60"><component :is="'Workflow'" /></el-icon>
                </div>
              </template>
            </el-empty>
          </div>
        </div>

        <!-- JSON 数据 -->
        <div class="section">
          <div class="section-header">
            <span class="section-title">JSON 数据</span>
          </div>
          
          <div class="json-container">
            <el-descriptions :column="1" border>
              <el-descriptions-item label="节点 JSON">
                <code>{{ currentWorkflow.nodesJson || '{}' }}</code>
              </el-descriptions-item>
              <el-descriptions-item label="边 JSON">
                <code>{{ currentWorkflow.edgesJson || '{}' }}</code>
              </el-descriptions-item>
              <el-descriptions-item label="执行器 JSON">
                <code>{{ currentWorkflow.executorsJson || '{}' }}</code>
              </el-descriptions-item>
            </el-descriptions>
          </div>
        </div>
      </div>
    </el-card>
  </section>
</template>

<style scoped>
.workflow-detail {
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

.form-row {
  margin-bottom: 1rem;
}

.form-item label {
  display: block;
  margin-bottom: 0.5rem;
  color: var(--el-text-color-primary);
  font-weight: 500;
}

.field-value {
  padding: 0.5rem;
  background: var(--el-fill-color-light);
  border-radius: 4px;
  color: var(--el-text-color-regular);
}

.tag-list {
  display: flex;
  flex-wrap: wrap;
}

.designer-placeholder {
  padding: 2rem;
  background: var(--el-fill-color-lighter);
  border-radius: 8px;
}

.workflow-icon {
  color: var(--el-color-primary);
}

.json-container code {
  display: block;
  font-family: 'Monaco', 'Consolas', monospace;
  font-size: 0.9rem;
  padding: 0.5rem;
  background: var(--el-fill-color-lighter);
  border-radius: 4px;
  overflow-x: auto;
  white-space: pre-wrap;
}

.empty-container {
  padding: 2rem;
}
</style>
