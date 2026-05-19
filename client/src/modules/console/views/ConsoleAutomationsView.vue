<script setup lang="ts">
import { ref } from 'vue'

interface Automation {
  id: string
  name: string
  repository: string
  trigger: string
  actions: string[]
  status: 'active' | 'inactive'
  createdAt: string
}

const selectedRepository = ref('AutoCodeForge')
const repositories = ['AutoCodeForge', 'backend-service', 'mobile-app']

const automations = ref<Automation[]>([
  {
    id: '1',
    name: '自动代码审查',
    repository: 'AutoCodeForge',
    trigger: 'Pull Request',
    actions: ['代码质量检查', '自动测试', 'AI审查建议'],
    status: 'active',
    createdAt: '2024-05-18',
  },
  {
    id: '2',
    name: '自动修复常见问题',
    repository: 'AutoCodeForge',
    trigger: 'Issue 创建',
    actions: ['分析问题', '生成修复建议', '创建分支'],
    status: 'active',
    createdAt: '2024-05-17',
  },
  {
    id: '3',
    name: '自动文档生成',
    repository: 'backend-service',
    trigger: 'Commit Push',
    actions: ['提取代码注释', '生成API文档', '更新Wiki'],
    status: 'inactive',
    createdAt: '2024-05-16',
  },
])

const showCreateDialog = ref(false)
const newAutomation = ref({
  name: '',
  repository: 'AutoCodeForge',
  trigger: '',
  actions: [],
})

const availableTriggers = ['Pull Request', 'Issue 创建', 'Commit Push', '定时任务']
const availableActions = [
  '代码质量检查',
  '自动测试',
  'AI审查建议',
  '分析问题',
  '生成修复建议',
  '创建分支',
  '提取代码注释',
  '生成API文档',
  '更新Wiki',
]

const handleCreateAutomation = () => {
  const today = new Date().toISOString().split('T')[0]
  automations.value.push({
    id: Date.now().toString(),
    ...newAutomation.value,
    status: 'active',
    createdAt: today || '2024-01-01',
  })
  showCreateDialog.value = false
  newAutomation.value = {
    name: '',
    repository: 'AutoCodeForge',
    trigger: '',
    actions: [],
  }
}

const toggleAutomation = (id: string) => {
  const automation = automations.value.find((a) => a.id === id)
  if (automation) {
    automation.status = automation.status === 'active' ? 'inactive' : 'active'
  }
}
</script>

<template>
  <section class="automations-view">
    <div class="automations-header">
      <div class="header-title">
        <h2>自动化任务</h2>
        <p>创建和管理 AI 自动化工作流</p>
      </div>

      <div class="header-actions">
        <div class="repo-selector">
          <label>选择仓库</label>
          <el-select v-model="selectedRepository" size="small">
            <el-option v-for="repo in repositories" :key="repo" :label="repo" :value="repo" />
          </el-select>
        </div>
        <el-button type="primary" @click="showCreateDialog = true">+ 创建自动化</el-button>
      </div>
    </div>

    <div class="automations-grid">
      <div v-for="automation in automations" :key="automation.id" class="automation-card">
        <div class="card-header">
          <h3>{{ automation.name }}</h3>
          <el-switch
            v-model="automation.status"
            active-value="active"
            inactive-value="inactive"
            @change="toggleAutomation(automation.id)"
          />
        </div>

        <div class="card-info">
          <div class="info-row">
            <span class="info-label">仓库:</span>
            <span class="info-value">{{ automation.repository }}</span>
          </div>
          <div class="info-row">
            <span class="info-label">触发条件:</span>
            <el-tag type="info">{{ automation.trigger }}</el-tag>
          </div>
        </div>

        <div class="actions-list">
          <div class="actions-label">执行操作:</div>
          <div class="action-tags">
            <el-tag v-for="(action, idx) in automation.actions" :key="idx" size="small">
              {{ action }}
            </el-tag>
          </div>
        </div>

        <div class="card-footer">
          <span class="create-date">创建于 {{ automation.createdAt }}</span>
          <div class="action-buttons">
            <el-button link type="primary" size="small">编辑</el-button>
            <el-button link type="danger" size="small">删除</el-button>
          </div>
        </div>
      </div>
    </div>

    <!-- 创建自动化对话框 -->
    <el-dialog v-model="showCreateDialog" title="创建自动化任务" width="600px">
      <el-form :model="newAutomation" label-width="100px">
        <el-form-item label="任务名称">
          <el-input v-model="newAutomation.name" placeholder="输入自动化任务名称" />
        </el-form-item>

        <el-form-item label="选择仓库">
          <el-select v-model="newAutomation.repository" w-full>
            <el-option v-for="repo in repositories" :key="repo" :label="repo" :value="repo" />
          </el-select>
        </el-form-item>

        <el-form-item label="触发条件">
          <el-select v-model="newAutomation.trigger" w-full placeholder="选择触发条件">
            <el-option v-for="trigger in availableTriggers" :key="trigger" :label="trigger" :value="trigger" />
          </el-select>
        </el-form-item>

        <el-form-item label="执行操作">
          <el-select v-model="newAutomation.actions" multiple placeholder="选择要执行的操作" w-full>
            <el-option v-for="action in availableActions" :key="action" :label="action" :value="action" />
          </el-select>
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="showCreateDialog = false">取消</el-button>
        <el-button type="primary" @click="handleCreateAutomation">创建</el-button>
      </template>
    </el-dialog>
  </section>
</template>

<style scoped>
.automations-view {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  background: white;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
}

.automations-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 24px;
  border-bottom: 1px solid #e2e8f0;
  gap: 20px;
}

.header-title h2 {
  font-size: 24px;
  font-weight: 700;
  color: #0f172a;
  margin: 0 0 4px;
}

.header-title p {
  font-size: 13px;
  color: #64748b;
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 16px;
  align-items: flex-end;
}

.repo-selector {
  display: flex;
  flex-direction: column;
}

.repo-selector label {
  font-size: 12px;
  font-weight: 600;
  color: #64748b;
  margin-bottom: 6px;
}

.automations-grid {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 16px;
}

.automation-card {
  padding: 20px;
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex;
  flex-direction: column;
}

.automation-card:hover {
  border-color: #cbd5e1;
  box-shadow: 0 8px 24px rgba(15, 23, 42, 0.08);
  transform: translateY(-2px);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid #e2e8f0;
}

.card-header h3 {
  font-size: 16px;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}

.card-info {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-bottom: 16px;
}

.info-row {
  display: flex;
  gap: 8px;
  align-items: center;
}

.info-label {
  font-weight: 600;
  color: #64748b;
  font-size: 12px;
  min-width: 70px;
}

.info-value {
  font-size: 13px;
  color: #0f172a;
}

.actions-list {
  margin-bottom: 16px;
}

.actions-label {
  font-weight: 600;
  color: #64748b;
  font-size: 12px;
  margin-bottom: 8px;
}

.action-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.action-tags :deep(.el-tag) {
  background: #ffffff;
  border: 1px solid #cbd5e1;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 12px;
  border-top: 1px solid #e2e8f0;
  font-size: 12px;
  color: #94a3b8;
}

.action-buttons {
  display: flex;
  gap: 4px;
}
</style>
