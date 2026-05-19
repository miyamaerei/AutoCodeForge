<script setup lang="ts">
import { onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useScheduledTask } from '../composables/useScheduledTask'
import type { ScheduledTaskDto, TaskTemplateDto } from '../scheduled-task.api'

const {
  tasks,
  selectedTask,
  executions,
  templates,
  loading,
  saving,
  triggering,
  error,
  dialogVisible,
  dialogTitle,
  formData,
  canSave,
  availableAgents,
  availableRepos,
  fetchTasks,
  fetchTemplates,
  openCreateDialog,
  openEditDialog,
  closeDialog,
  saveTask,
  removeTask,
  toggleEnabled,
  runTask,
  selectTask,
  clearSelection,
  applyTemplate,
} = useScheduledTask()

/** 初始化加载 */
onMounted(() => {
  fetchTasks()
  fetchTemplates()
})

/** 处理选择模板 */
function handleSelectTemplate(template: TaskTemplateDto) {
  applyTemplate(template)
  ElMessage.success(`已应用模板：${template.name}`)
}

/** 处理保存 */
async function handleSave() {
  const success = await saveTask()
  if (success) {
    ElMessage.success('保存成功')
  }
}

/** 处理删除 */
async function handleDelete(task: ScheduledTaskDto) {
  try {
    await ElMessageBox.confirm(`确定要删除任务「${task.name}」吗？`, '删除确认', {
      confirmButtonText: '删除',
      cancelButtonText: '取消',
      type: 'warning',
    })
    const success = await removeTask(task.id)
    if (success) {
      ElMessage.success('删除成功')
      if (selectedTask.value?.id === task.id) {
        clearSelection()
      }
    }
  } catch {
    // 用户取消
  }
}

/** 处理启用/禁用 */
async function handleToggle(task: ScheduledTaskDto) {
  const success = await toggleEnabled(task)
  if (success) {
    ElMessage.success(task.enabled ? '已禁用' : '已启用')
  }
}

/** 处理手动触发 */
async function handleTrigger(task: ScheduledTaskDto) {
  try {
    await ElMessageBox.confirm(`确定要立即执行任务「${task.name}」吗？`, '执行确认', {
      confirmButtonText: '执行',
      cancelButtonText: '取消',
      type: 'info',
    })
    const success = await runTask(task)
    if (success) {
      ElMessage.success('任务已触发执行')
    }
  } catch {
    // 用户取消
  }
}

/** 获取状态标签类型 */
function getStatusType(status: string): '' | 'success' | 'warning' | 'danger' | 'info' {
  const map: Record<string, '' | 'success' | 'warning' | 'danger' | 'info'> = {
    pending: 'info',
    running: 'warning',
    success: 'success',
    failed: 'danger',
    disabled: 'info',
  }
  return map[status] ?? 'info'
}

/** 获取状态文本 */
function getStatusText(status: string): string {
  const map: Record<string, string> = {
    pending: '待执行',
    running: '执行中',
    success: '成功',
    failed: '失败',
    disabled: '已禁用',
  }
  return map[status] ?? status
}

/** 获取触发类型文本 */
function getTriggerTypeText(type: string): string {
  const map: Record<string, string> = {
    cron: 'Cron 表达式',
    interval: '固定间隔',
    once: '一次性',
  }
  return map[type] ?? type
}

/** 格式化触发配置 */
function formatTrigger(task: ScheduledTaskDto): string {
  if (task.triggerType === 'cron') {
    return task.cronExpression
  }
  if (task.triggerType === 'interval') {
    const hours = Math.floor(task.intervalMs / (1000 * 60 * 60))
    const minutes = Math.floor((task.intervalMs % (1000 * 60 * 60)) / (1000 * 60))
    const parts: string[] = []
    if (hours > 0) parts.push(`${hours} 小时`)
    if (minutes > 0) parts.push(`${minutes} 分钟`)
    return parts.join(' ') || '未知'
  }
  if (task.triggerType === 'once') {
    return task.onceTime || '未设置'
  }
  return '未知'
}

/** 计算执行成功率 */
function getSuccessRate(task: ScheduledTaskDto): string {
  if (task.totalRuns === 0) return '-'
  const rate = ((task.successRuns / task.totalRuns) * 100).toFixed(1)
  return `${rate}%`
}

/** 选中任务 */
function handleSelectTask(task: ScheduledTaskDto) {
  selectTask(task)
}
</script>

<template>
  <section class="scheduled-task-page">
    <el-page-header content="定时任务" />

    <!-- 顶部操作栏 -->
    <div class="toolbar">
      <el-button type="primary" @click="openCreateDialog">新建定时任务</el-button>
      <el-button @click="fetchTasks">刷新列表</el-button>
    </div>

    <!-- 错误提示 -->
    <el-alert v-if="error" :title="error" type="error" show-icon :closable="false" class="error-alert" />

    <!-- 主体内容 -->
    <el-row :gutter="16" class="content-row">
      <!-- 左侧：任务列表 -->
      <el-col :span="selectedTask ? 10 : 24">
        <div v-loading="loading" class="task-list">
          <el-table :data="tasks" stripe style="width: 100%" @row-click="handleSelectTask">
            <el-table-column prop="name" label="任务名称" min-width="150">
              <template #default="{ row }">
                <div class="task-name-cell">
                  <span>{{ row.name }}</span>
                  <el-tag v-if="!row.enabled" size="small" type="info">已禁用</el-tag>
                </div>
              </template>
            </el-table-column>
            <el-table-column prop="agentName" label="关联 Agent" width="120" />
            <el-table-column prop="repo" label="关联仓库" width="140">
              <template #default="{ row }">
                {{ row.repo?.repoName || '-' }}
              </template>
            </el-table-column>
            <el-table-column prop="triggerType" label="触发方式" width="100">
              <template #default="{ row }">
                {{ getTriggerTypeText(row.triggerType) }}
              </template>
            </el-table-column>
            <el-table-column prop="status" label="状态" width="80">
              <template #default="{ row }">
                <el-tag :type="getStatusType(row.status)" size="small">
                  {{ getStatusText(row.status) }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="180" fixed="right">
              <template #default="{ row }">
                <el-button size="small" link type="primary" @click.stop="openEditDialog(row)">编辑</el-button>
                <el-button size="small" link type="primary" @click.stop="handleToggle(row)">
                  {{ row.enabled ? '禁用' : '启用' }}
                </el-button>
                <el-button size="small" link type="success" @click.stop="handleTrigger(row)">执行</el-button>
                <el-button size="small" link type="danger" @click.stop="handleDelete(row)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>

          <!-- 空状态 -->
          <el-empty v-if="!loading && tasks.length === 0" description="暂无定时任务，请点击新建">
            <el-button type="primary" @click="openCreateDialog">新建定时任务</el-button>
          </el-empty>
        </div>
      </el-col>

      <!-- 右侧：任务详情和执行记录 -->
      <el-col v-if="selectedTask" :span="14">
        <el-card class="detail-card" shadow="never">
          <template #header>
            <div class="detail-header">
              <span>任务详情</span>
              <el-button size="small" link type="primary" @click="clearSelection">关闭</el-button>
            </div>
          </template>

          <el-descriptions :column="2" border>
            <el-descriptions-item label="任务名称">{{ selectedTask.name }}</el-descriptions-item>
            <el-descriptions-item label="关联 Agent">{{ selectedTask.agentName }}</el-descriptions-item>
            <el-descriptions-item label="关联仓库">
              <template v-if="selectedTask.repo">
                {{ selectedTask.repo.repoName }} / {{ selectedTask.repo.branch }}
                <span v-if="selectedTask.repo.path"> / {{ selectedTask.repo.path }}</span>
              </template>
              <span v-else>-</span>
            </el-descriptions-item>
            <el-descriptions-item label="触发方式">{{ getTriggerTypeText(selectedTask.triggerType) }}</el-descriptions-item>
            <el-descriptions-item label="触发配置">{{ formatTrigger(selectedTask) }}</el-descriptions-item>
            <el-descriptions-item label="当前状态">
              <el-tag :type="getStatusType(selectedTask.status)" size="small">
                {{ getStatusText(selectedTask.status) }}
              </el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="成功率">{{ getSuccessRate(selectedTask) }}</el-descriptions-item>
            <el-descriptions-item label="下次执行">{{ selectedTask.nextRunTime || '-' }}</el-descriptions-item>
            <el-descriptions-item label="上次执行">{{ selectedTask.lastRunTime || '-' }}</el-descriptions-item>
            <el-descriptions-item label="总执行次数" :span="2">{{ selectedTask.totalRuns }} 次（成功 {{ selectedTask.successRuns }} / 失败 {{ selectedTask.failedRuns }}）</el-descriptions-item>
            <el-descriptions-item label="任务描述" :span="2">{{ selectedTask.description || '无' }}</el-descriptions-item>
            <el-descriptions-item label="任务参数" :span="2">
              <code class="params-code">{{ selectedTask.params }}</code>
            </el-descriptions-item>
          </el-descriptions>
        </el-card>

        <!-- 执行记录 -->
        <el-card class="exec-card" shadow="never">
          <template #header>
            <span>执行记录</span>
          </template>

          <el-table :data="executions" stripe size="small" max-height="300">
            <el-table-column prop="startTime" label="开始时间" width="160" />
            <el-table-column prop="duration" label="耗时" width="80">
              <template #default="{ row }">
                {{ row.duration > 0 ? `${(row.duration / 1000).toFixed(1)}s` : '-' }}
              </template>
            </el-table-column>
            <el-table-column prop="status" label="状态" width="80">
              <template #default="{ row }">
                <el-tag :type="getStatusType(row.status)" size="small">
                  {{ getStatusText(row.status) }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="result" label="执行结果" />
          </el-table>

          <el-empty v-if="executions.length === 0" description="暂无执行记录" />
        </el-card>
      </el-col>
    </el-row>

    <!-- 新建/编辑弹窗 -->
    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="700px" :close-on-click-modal="false">
      <el-form label-position="top">
        <!-- 从模板创建 -->
        <el-form-item v-if="!selectedTask" label="快速开始（可选）">
          <div class="template-grid">
            <el-card
              v-for="tpl in templates"
              :key="tpl.id"
              class="template-card"
              shadow="hover"
              @click="handleSelectTemplate(tpl)"
            >
              <div class="template-name">{{ tpl.name }}</div>
              <div class="template-desc">{{ tpl.description }}</div>
              <el-tag size="small" type="info">{{ getTriggerTypeText(tpl.triggerType) }}</el-tag>
            </el-card>
          </div>
        </el-form-item>

        <el-divider />

        <el-form-item label="任务名称" required>
          <el-input v-model="formData.name" placeholder="输入任务名称" maxlength="50" show-word-limit />
        </el-form-item>

        <el-form-item label="任务描述">
          <el-input
            v-model="formData.description"
            type="textarea"
            placeholder="简要描述任务的职责"
            :rows="2"
            maxlength="200"
            show-word-limit
          />
        </el-form-item>

        <el-form-item label="关联仓库" required>
          <el-select v-model="formData.repoId" placeholder="选择关联的仓库" style="width: 100%">
            <el-option v-for="repo in availableRepos" :key="repo.id" :label="repo.name" :value="repo.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="目标分支">
          <el-input v-model="formData.branch" placeholder="如 main、develop" style="width: 100%" />
        </el-form-item>

        <el-form-item label="目标路径（可选）">
          <el-input v-model="formData.path" placeholder="如 src/utils（留空表示整个仓库）" style="width: 100%" />
        </el-form-item>

        <el-form-item label="关联 Agent" required>
          <el-select v-model="formData.agentId" placeholder="选择关联的 Agent" style="width: 100%">
            <el-option v-for="agent in availableAgents" :key="agent.id" :label="agent.name" :value="agent.id" />
          </el-select>
        </el-form-item>

        <el-form-item label="触发方式">
          <el-radio-group v-model="formData.triggerType">
            <el-radio value="cron">Cron 表达式</el-radio>
            <el-radio value="interval">固定间隔</el-radio>
            <el-radio value="once">一次性</el-radio>
          </el-radio-group>
        </el-form-item>

        <!-- Cron 表达式 -->
        <el-form-item v-if="formData.triggerType === 'cron'" label="Cron 表达式">
          <el-input v-model="formData.cronExpression" placeholder="0 9 * * *" />
          <div class="form-tip">格式：分 时 日 月 周，如 "0 9 * * *" 表示每天 9:00 执行</div>
        </el-form-item>

        <!-- 固定间隔 -->
        <el-form-item v-if="formData.triggerType === 'interval'" label="执行间隔">
          <el-space>
            <el-input-number v-model="formData.intervalHours" :min="0" :max="72" />
            <span>小时</span>
            <el-input-number v-model="formData.intervalMinutes" :min="0" :max="59" />
            <span>分钟</span>
          </el-space>
        </el-form-item>

        <!-- 一次性执行 -->
        <el-form-item v-if="formData.triggerType === 'once'" label="执行时间">
          <el-date-picker
            v-model="formData.onceTime"
            type="datetime"
            placeholder="选择执行时间"
            style="width: 100%"
          />
        </el-form-item>

        <el-form-item label="任务参数">
          <el-input
            v-model="formData.params"
            type="textarea"
            placeholder='JSON 格式参数，如 {"key": "value"}'
            :rows="2"
          />
        </el-form-item>

        <el-form-item label="启用状态">
          <el-switch v-model="formData.enabled" />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="closeDialog">取消</el-button>
        <el-button type="primary" :loading="saving" :disabled="!canSave" @click="handleSave">保存</el-button>
      </template>
    </el-dialog>
  </section>
</template>

<style scoped>
.scheduled-task-page {
  margin-top: 0;
  min-width: 1280px;
}

.toolbar {
  margin: 1rem 0;
  display: flex;
  gap: 0.5rem;
}

.error-alert {
  margin-bottom: 1rem;
}

.content-row {
  min-height: 600px;
}

.task-list {
  min-height: 400px;
}

.task-name-cell {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.detail-card {
  margin-bottom: 1rem;
}

.detail-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.params-code {
  background: #f5f7fa;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.85rem;
}

.form-tip {
  color: #909399;
  font-size: 0.8rem;
  margin-top: 0.25rem;
}

.template-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 0.75rem;
}

.template-card {
  cursor: pointer;
  transition: all 0.2s;
}

.template-card:hover {
  border-color: #409eff;
}

.template-name {
  font-weight: bold;
  font-size: 0.9rem;
  margin-bottom: 0.25rem;
}

.template-desc {
  color: #666;
  font-size: 0.8rem;
  margin-bottom: 0.5rem;
  line-height: 1.3;
  min-height: 2.6em;
}

.exec-card {
  margin-top: 0;
}
</style>
