<script setup lang="ts">
import { onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useAgent } from '../composables/useAgent'
import type { AgentDto } from '../agent.api'

const {
  agents,
  loading,
  saving,
  error,
  dialogVisible,
  dialogTitle,
  formData,
  canSave,
  fetchAgents,
  openCreateDialog,
  openEditDialog,
  closeDialog,
  saveAgent,
  removeAgent,
  genericSystemPromptTemplate,
} = useAgent()

/** 初始化加载 */
onMounted(() => {
  fetchAgents()
})

/** 处理保存 */
async function handleSave() {
  const success = await saveAgent()
  if (success) {
    ElMessage.success('保存成功')
  }
}

/** 处理删除 */
async function handleDelete(agent: AgentDto) {
  try {
    await ElMessageBox.confirm(`确定要删除 Agent「${agent.name}」吗？`, '删除确认', {
      confirmButtonText: '删除',
      cancelButtonText: '取消',
      type: 'warning',
    })
    const success = await removeAgent(agent.id)
    if (success) {
      ElMessage.success('删除成功')
    }
  } catch {
    // 用户取消
  }
}

/** 切换启用状态 */
async function toggleEnabled(agent: AgentDto) {
  const newEnabled = !agent.enabled
  const { updateAgent } = await import('../agent.api')
  try {
    await updateAgent({ id: agent.id, enabled: newEnabled })
    agent.enabled = newEnabled
    ElMessage.success(newEnabled ? '已启用' : '已禁用')
  } catch {
    ElMessage.error('更新状态失败')
  }
}

/** 获取状态标签类型 */
function getStatusType(enabled: boolean) {
  return enabled ? 'success' : 'info'
}

/** 获取状态文本 */
function getStatusText(enabled: boolean) {
  return enabled ? '已启用' : '已禁用'
}

/** 复制系统提示词 */
function copyPrompt(prompt: string) {
  navigator.clipboard.writeText(prompt)
  ElMessage.success('已复制到剪贴板')
}

/** 填充通用系统提示词模板 */
function applyGenericPromptTemplate() {
  formData.systemPrompt = genericSystemPromptTemplate
  ElMessage.success('已填充通用 Agent 提示')
}
</script>

<template>
  <section class="agent-center-page">
    <el-page-header content="Agent 管理" />

    <!-- 顶部操作栏 -->
    <div class="toolbar">
      <el-button type="primary" @click="openCreateDialog">新建 Agent</el-button>
      <el-button @click="fetchAgents">刷新列表</el-button>
    </div>

    <!-- 错误提示 -->
    <el-alert v-if="error" :title="error" type="error" show-icon :closable="false" class="error-alert" />

    <!-- Agent 列表 -->
    <div v-loading="loading" class="agent-grid">
      <el-card v-for="agent in agents" :key="agent.id" class="agent-card" shadow="hover">
        <template #header>
          <div class="agent-header">
            <span class="agent-icon">{{ agent.icon }}</span>
            <span class="agent-name">{{ agent.name }}</span>
            <el-tag :type="getStatusType(agent.enabled)" size="small">
              {{ getStatusText(agent.enabled) }}
            </el-tag>
          </div>
        </template>

        <div class="agent-body">
          <p class="agent-desc">{{ agent.description || '暂无描述' }}</p>

          <div class="agent-keywords">
            <span v-for="kw in agent.keywords.slice(0, 5)" :key="kw.keyword" class="keyword-tag">
              {{ kw.keyword }}
            </span>
            <span v-if="agent.keywords.length > 5" class="keyword-more">
              +{{ agent.keywords.length - 5 }}
            </span>
          </div>

          <div class="agent-meta">
            <span>创建于 {{ agent.createdAt }}</span>
          </div>
        </div>

        <template #footer>
          <div class="agent-actions">
            <el-button size="small" @click="openEditDialog(agent)">编辑</el-button>
            <el-button size="small" @click="toggleEnabled(agent)">
              {{ agent.enabled ? '禁用' : '启用' }}
            </el-button>
            <el-button size="small" type="danger" plain @click="handleDelete(agent)">删除</el-button>
          </div>
        </template>
      </el-card>

      <!-- 空状态 -->
      <el-empty v-if="!loading && agents.length === 0" description="暂无 Agent，请点击新建">
        <el-button type="primary" @click="openCreateDialog">新建 Agent</el-button>
      </el-empty>
    </div>

    <!-- 新建/编辑弹窗 -->
    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="600px" :close-on-click-modal="false">
      <el-form label-position="top">
        <el-form-item label="名称" required>
          <el-input v-model="formData.name" placeholder="输入 Agent 名称" maxlength="50" show-word-limit />
        </el-form-item>

        <el-form-item label="图标">
          <el-input v-model="formData.icon" placeholder="选择图标（emoji）" maxlength="10" style="width: 120px" />
        </el-form-item>

        <el-form-item label="描述">
          <el-input
            v-model="formData.description"
            type="textarea"
            placeholder="简要描述 Agent 的职责和能力"
            :rows="2"
            maxlength="200"
            show-word-limit
          />
        </el-form-item>

        <el-form-item label="系统提示词" required>
          <el-input
            v-model="formData.systemPrompt"
            type="textarea"
            placeholder="定义 Agent 的角色定位和行为规范"
            :rows="4"
            maxlength="500"
            show-word-limit
          />
          <div class="prompt-toolbar">
            <el-button size="small" text type="primary" @click="applyGenericPromptTemplate">
              填充通用 Agent 提示
            </el-button>
          </div>
          <div class="form-tip">
            通用模板已内置任务流程、边界约束、已有代码复用与回归验证要点，可按业务再细化。
          </div>
        </el-form-item>

        <el-form-item label="自动选择关键词">
          <el-input
            v-model="formData.keywordsText"
            type="textarea"
            placeholder="输入关键词，多个用逗号分隔，如：代码审查,bug,安全"
            :rows="2"
          />
          <div class="form-tip">当聊天内容包含这些关键词时，会自动选择该 Agent</div>
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
.agent-center-page {
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

.agent-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 1rem;
}

.agent-card {
  margin-bottom: 0;
}

.agent-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.agent-icon {
  font-size: 1.5rem;
}

.agent-name {
  font-weight: bold;
  flex: 1;
}

.agent-body {
  min-height: 100px;
}

.agent-desc {
  color: #666;
  font-size: 0.9rem;
  margin: 0 0 0.75rem 0;
  line-height: 1.4;
}

.agent-keywords {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem;
  margin-bottom: 0.75rem;
}

.keyword-tag {
  background: #f0f2f5;
  color: #606266;
  padding: 0.125rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
}

.keyword-more {
  color: #909399;
  font-size: 0.75rem;
  padding: 0.125rem 0.25rem;
}

.agent-meta {
  color: #909399;
  font-size: 0.75rem;
}

.agent-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
}

.form-tip {
  color: #909399;
  font-size: 0.8rem;
  margin-top: 0.25rem;
}

.prompt-toolbar {
  display: flex;
  justify-content: flex-end;
  margin-top: 0.25rem;
}
</style>
