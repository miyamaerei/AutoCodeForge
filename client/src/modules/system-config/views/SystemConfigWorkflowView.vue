<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { useSystemConfigWorkflow } from '../composables/useSystemConfigWorkflow'

const activeTab = ref('global')

const {
  loading,
  error,
  saving,
  saveError,
  saveSuccess,
  lastSavedAt,
  form,
  hasData,
  isDirty,
  automationScore,
  automationLabel,
  automationTagType,
  loadConfig,
  saveConfig,
} = useSystemConfigWorkflow()

const handleSave = async () => {
  await saveConfig()
  if (saveSuccess.value) {
    ElMessage.success('流程配置已保存')
  }
  if (saveError.value) {
    ElMessage.error(saveError.value)
  }
}

onMounted(() => {
  loadConfig()
})
</script>

<template>
  <section class="settings-page">
    <div class="settings-shell">
      <el-page-header content="Workflow" />

      <el-skeleton v-if="loading" :rows="9" animated class="state-block" />
      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" class="state-block" />
      <el-empty v-else-if="!hasData" description="暂无可配置的流程策略" class="state-block" />

      <div v-else class="workflow-layout">
        <el-card shadow="hover" class="editor-card">
          <template #header>
            <div class="card-header">
              <strong>Agent 流程编排</strong>
              <span class="card-subtitle">配置初始化、提问、修复和需求处理策略</span>
            </div>
          </template>

          <el-tabs v-model="activeTab" class="workflow-tabs" type="border-card" stretch>
            <el-tab-pane label="全局策略" name="global">
              <el-form label-width="190px" class="settings-form">
                <el-row :gutter="16">
                  <el-col :span="12">
                    <el-form-item label="配置名称">
                      <el-input v-model="form.profileName" placeholder="例如 team-default" />
                    </el-form-item>
                  </el-col>
                  <el-col :span="12">
                    <el-form-item label="响应风格">
                      <el-select v-model="form.outputStyle">
                        <el-option label="Concise" value="concise" />
                        <el-option label="Standard" value="standard" />
                        <el-option label="Detailed" value="detailed" />
                      </el-select>
                    </el-form-item>
                  </el-col>
                </el-row>

                <el-row :gutter="16">
                  <el-col :span="12">
                    <el-form-item label="命令超时 (s)">
                      <el-input-number v-model="form.commandTimeoutSec" :min="60" :max="1200" :step="30" />
                    </el-form-item>
                  </el-col>
                  <el-col :span="12">
                    <el-form-item label="最大并发任务">
                      <el-input-number v-model="form.maxParallelTasks" :min="1" :max="8" />
                    </el-form-item>
                  </el-col>
                </el-row>

                <el-form-item label="执行控制">
                  <div class="inline-switches">
                    <el-switch v-model="form.requireApprovalForWrite" active-text="写操作需审批" />
                    <el-switch v-model="form.autoCreateTodo" active-text="自动生成 Todo 计划" />
                  </div>
                </el-form-item>
              </el-form>
            </el-tab-pane>

            <el-tab-pane label="初始化" name="initialization">
              <el-form label-width="190px" class="settings-form">
                <el-row :gutter="16">
                  <el-col :span="8">
                    <el-form-item label="Agent 模式">
                      <el-select v-model="form.initialization.mode">
                        <el-option label="Default" value="default" />
                        <el-option label="Explore" value="explore" />
                        <el-option label="Mixed" value="mixed" />
                      </el-select>
                    </el-form-item>
                  </el-col>
                  <el-col :span="16">
                    <el-form-item label="流程开关">
                      <div class="inline-switches inline-flow">
                        <el-switch v-model="form.initialization.preloadInstructionFiles" active-text="预加载指令文件" />
                        <el-switch v-model="form.initialization.preloadRepoMemory" active-text="读取仓库记忆" />
                        <el-switch v-model="form.initialization.generateChecklist" active-text="生成执行清单" />
                      </div>
                    </el-form-item>
                  </el-col>
                </el-row>

                <el-form-item label="初始化模板">
                  <el-input v-model="form.initialization.notesTemplate" type="textarea" :rows="3" />
                </el-form-item>
              </el-form>
            </el-tab-pane>

            <el-tab-pane label="提问" name="question">
              <el-form label-width="190px" class="settings-form">
                <el-row :gutter="16">
                  <el-col :span="8">
                    <el-form-item label="Agent 模式">
                      <el-select v-model="form.question.mode">
                        <el-option label="Default" value="default" />
                        <el-option label="Explore" value="explore" />
                        <el-option label="Mixed" value="mixed" />
                      </el-select>
                    </el-form-item>
                  </el-col>
                  <el-col :span="16">
                    <el-form-item label="问答策略">
                      <div class="inline-switches inline-flow">
                        <el-switch v-model="form.question.mustAskClarifying" active-text="必须先澄清" />
                        <el-switch v-model="form.question.useRepoSearch" active-text="默认仓库检索" />
                        <el-switch v-model="form.question.alwaysCiteFiles" active-text="必须引用文件" />
                        <el-switch v-model="form.question.includeAlternatives" active-text="给出备选方案" />
                      </div>
                    </el-form-item>
                  </el-col>
                </el-row>

                <el-form-item label="提问模板">
                  <el-input v-model="form.question.notesTemplate" type="textarea" :rows="3" />
                </el-form-item>
              </el-form>
            </el-tab-pane>

            <el-tab-pane label="修 Bug" name="bugfix">
              <el-form label-width="190px" class="settings-form">
                <el-row :gutter="16">
                  <el-col :span="8">
                    <el-form-item label="Agent 模式">
                      <el-select v-model="form.bugfix.mode">
                        <el-option label="Default" value="default" />
                        <el-option label="Explore" value="explore" />
                        <el-option label="Mixed" value="mixed" />
                      </el-select>
                    </el-form-item>
                  </el-col>
                  <el-col :span="8">
                    <el-form-item label="最大修复轮次">
                      <el-input-number v-model="form.bugfix.maxFixAttempts" :min="1" :max="5" />
                    </el-form-item>
                  </el-col>
                  <el-col :span="8">
                    <el-form-item label="验证开关">
                      <div class="inline-switches inline-flow">
                        <el-switch v-model="form.bugfix.requireReproSteps" active-text="要求复现步骤" />
                        <el-switch v-model="form.bugfix.autoRunUnitTests" active-text="自动跑单测" />
                        <el-switch v-model="form.bugfix.runValidation" active-text="修复后验证" />
                      </div>
                    </el-form-item>
                  </el-col>
                </el-row>

                <el-form-item label="修复模板">
                  <el-input v-model="form.bugfix.notesTemplate" type="textarea" :rows="3" />
                </el-form-item>
              </el-form>
            </el-tab-pane>

            <el-tab-pane label="新需求" name="feature">
              <el-form label-width="190px" class="settings-form">
                <el-row :gutter="16">
                  <el-col :span="8">
                    <el-form-item label="Agent 模式">
                      <el-select v-model="form.feature.mode">
                        <el-option label="Default" value="default" />
                        <el-option label="Explore" value="explore" />
                        <el-option label="Mixed" value="mixed" />
                      </el-select>
                    </el-form-item>
                  </el-col>
                  <el-col :span="16">
                    <el-form-item label="需求策略">
                      <div class="inline-switches inline-flow">
                        <el-switch v-model="form.feature.requireAcceptanceCriteria" active-text="要求验收标准" />
                        <el-switch v-model="form.feature.createImplementationPlan" active-text="生成实施计划" />
                        <el-switch v-model="form.feature.includeRollbackPlan" active-text="包含回滚方案" />
                        <el-switch v-model="form.feature.runValidation" active-text="实施后验证" />
                      </div>
                    </el-form-item>
                  </el-col>
                </el-row>

                <el-form-item label="需求模板">
                  <el-input v-model="form.feature.notesTemplate" type="textarea" :rows="3" />
                </el-form-item>
              </el-form>
            </el-tab-pane>
          </el-tabs>
        </el-card>

        <el-card shadow="never" class="status-card">
          <template #header>
            <div class="card-header">
              <strong>策略状态</strong>
              <span class="card-subtitle">保存前快速检查流程映射</span>
            </div>
          </template>

          <el-descriptions :column="1" border size="small" class="status-meta">
            <el-descriptions-item label="Profile">{{ form.profileName || '-' }}</el-descriptions-item>
            <el-descriptions-item label="Init Agent">{{ form.initialization.mode }}</el-descriptions-item>
            <el-descriptions-item label="Question Agent">{{ form.question.mode }}</el-descriptions-item>
            <el-descriptions-item label="Bugfix Agent">{{ form.bugfix.mode }}</el-descriptions-item>
            <el-descriptions-item label="Feature Agent">{{ form.feature.mode }}</el-descriptions-item>
            <el-descriptions-item label="Automation Score">
              <el-tag :type="automationTagType" size="small">{{ automationScore }} / 100 · {{ automationLabel }}</el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="Unsaved Changes">
              <el-tag :type="isDirty ? 'warning' : 'info'" size="small">{{ isDirty ? 'Yes' : 'No' }}</el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="Last Saved At">{{ lastSavedAt || '-' }}</el-descriptions-item>
          </el-descriptions>

          <el-alert
            v-if="saveSuccess"
            title="流程配置保存成功"
            type="success"
            show-icon
            :closable="false"
            class="save-state"
          />
          <el-alert
            v-if="saveError"
            :title="saveError"
            type="error"
            show-icon
            :closable="false"
            class="save-state"
          />

          <div class="actions sticky-actions">
            <el-button type="success" :loading="saving" @click="handleSave">保存流程配置</el-button>
          </div>
        </el-card>
      </div>
    </div>
  </section>
</template>

<style scoped>
.settings-page {
  min-width: 1280px;
  padding: 20px 16px 40px;
}

.settings-shell {
  width: 100%;
  max-width: 1680px;
  margin: 0 auto;
}

.state-block {
  margin-top: 12px;
}

.workflow-layout {
  margin-top: 12px;
  display: grid;
  grid-template-columns: minmax(980px, 1fr) 320px;
  gap: 16px;
  align-items: start;
}

.editor-card,
.status-card {
  min-height: 620px;
}

.card-header {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 12px;
}

.card-subtitle {
  color: #64748b;
  font-size: 13px;
}

.inline-switches {
  display: grid;
  gap: 12px;
  width: 100%;
}

.inline-flow {
  grid-template-columns: repeat(2, minmax(180px, 1fr));
}

.workflow-tabs {
  margin-top: 4px;
}

:deep(.workflow-tabs .el-tabs__content) {
  padding: 16px 8px 8px;
}

.status-card {
  position: sticky;
  top: 10px;
}

.status-meta {
  margin-top: 8px;
}

.save-state {
  margin-top: 12px;
}

.actions {
  margin-top: 16px;
}

.sticky-actions {
  position: sticky;
  bottom: 0;
  background: linear-gradient(180deg, rgba(255, 255, 255, 0.7), #fff 48%);
  padding-top: 10px;
}

@media (max-width: 1536px) {
  .workflow-layout {
    width: 1320px;
  }

  .settings-shell {
    overflow-x: auto;
    padding-bottom: 10px;
  }
}
</style>
