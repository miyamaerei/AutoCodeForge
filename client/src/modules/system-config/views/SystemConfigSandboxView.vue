<script setup lang="ts">
import { onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useSystemConfigSandbox } from '../composables/useSystemConfigSandbox'

const {
  loading,
  error,
  saving,
  saveError,
  saveSuccess,
  lastSavedAt,
  optionSections,
  scenarios,
  selectedScenario,
  form,
  hasData,
  isDirty,
  riskLabel,
  riskTagType,
  loadConfig,
  saveConfig,
  applyScenario,
} = useSystemConfigSandbox()

const handleSave = async () => {
  await saveConfig()
  if (saveSuccess.value) {
    ElMessage.success('Sandbox 配置已保存')
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
      <el-page-header content="Sandbox" />

      <el-skeleton v-if="loading" :rows="9" animated class="state-block" />
      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" class="state-block" />
      <el-empty v-else-if="!hasData" description="暂无可配置的沙盒选项" class="state-block" />

      <div v-else class="sandbox-layout">
        <el-card shadow="hover" class="scenario-card">
          <template #header>
            <div class="card-header">
              <strong>预设策略</strong>
              <span class="card-subtitle">快速切换沙盒行为</span>
            </div>
          </template>

          <div class="scenario-list">
            <button
              v-for="item in scenarios"
              :key="item.id"
              class="scenario-item"
              :class="{ active: item.id === selectedScenario }"
              @click="applyScenario(item.id)"
            >
              <div class="scenario-title">{{ item.name }}</div>
              <p class="scenario-desc">{{ item.description }}</p>
            </button>
          </div>

          <el-divider />

          <div class="section-list">
            <h4>建议配置维度</h4>
            <ul>
              <li v-for="section in optionSections" :key="section.key">
                <strong>{{ section.title }}</strong>
                <p>{{ section.description }}</p>
              </li>
            </ul>
          </div>
        </el-card>

        <el-card shadow="hover" class="editor-card">
          <template #header>
            <div class="card-header">
              <strong>Sandbox 配置编辑</strong>
              <span class="card-subtitle">按你的工作习惯调整默认执行策略</span>
            </div>
          </template>

          <el-form label-width="170px" class="settings-form">
            <el-row :gutter="16">
              <el-col :span="24">
                <el-form-item label="Workspace Root Path">
                  <el-input v-model="form.workspaceRootPath" placeholder="例如 C:/gitrepos/AutoCodeForge" />
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="16">
              <el-col :span="24">
                <el-form-item label="Artifact Output Path">
                  <el-input v-model="form.artifactOutputPath" placeholder="例如 C:/gitrepos/AutoCodeForge/.sandbox-artifacts" />
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="16">
              <el-col :span="12">
                <el-form-item label="Profile Name">
                  <el-input v-model="form.profileName" placeholder="例如 dev-sandbox" />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="Execution Mode">
                  <el-select v-model="form.executionMode">
                    <el-option label="Dry Run" value="dry-run" />
                    <el-option label="Sandbox Live" value="sandbox-live" />
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="16">
              <el-col :span="12">
                <el-form-item label="Approval Mode">
                  <el-select v-model="form.approvalMode">
                    <el-option label="Strict" value="strict" />
                    <el-option label="Manual" value="manual" />
                    <el-option label="Off" value="off" />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="Max Parallel Tasks">
                  <el-input-number v-model="form.maxParallelTasks" :min="1" :max="12" />
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="16">
              <el-col :span="12">
                <el-form-item label="Command Timeout (s)">
                  <el-input-number v-model="form.commandTimeoutSec" :min="60" :max="1800" :step="30" />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="Default Model">
                  <el-input v-model="form.defaultModel" placeholder="例如 gpt-5.3-codex" />
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="16">
              <el-col :span="12">
                <el-form-item label="Fallback Model">
                  <el-input v-model="form.fallbackModel" placeholder="例如 gpt-4.1" />
                </el-form-item>
              </el-col>
            </el-row>

            <el-form-item label="Prompt Guardrail">
              <el-input
                v-model="form.promptGuardrail"
                type="textarea"
                :rows="2"
                placeholder="例如：先分析风险，再执行最小改动。"
              />
            </el-form-item>

            <el-form-item label="Allowed Write Paths">
              <el-input
                v-model="form.allowedWritePaths"
                type="textarea"
                :rows="3"
                placeholder="每行一个 glob，例如 src/**"
              />
            </el-form-item>

            <el-form-item label="Ignored Paths">
              <el-input
                v-model="form.ignoredPaths"
                type="textarea"
                :rows="3"
                placeholder="每行一个 glob，例如 node_modules/**"
              />
            </el-form-item>

            <el-form-item label="Capability Flags">
              <div class="inline-switches">
                <el-switch v-model="form.allowWriteOps" active-text="允许写操作" />
                <el-switch v-model="form.allowNetworkAccess" active-text="允许网络访问" />
                <el-switch v-model="form.storeTerminalLogs" active-text="保留终端日志" />
                <el-switch v-model="form.maskSecretsInLogs" active-text="日志脱敏" />
              </div>
            </el-form-item>
          </el-form>
        </el-card>

        <el-card shadow="never" class="status-card">
          <template #header>
            <div class="card-header">
              <strong>配置状态</strong>
              <span class="card-subtitle">保存前检查风险与变更</span>
            </div>
          </template>

          <el-descriptions :column="1" border size="small" class="status-meta">
            <el-descriptions-item label="Current Preset">
              {{ selectedScenario }}
            </el-descriptions-item>
            <el-descriptions-item label="Profile">{{ form.profileName }}</el-descriptions-item>
            <el-descriptions-item label="Workspace Root">{{ form.workspaceRootPath || '-' }}</el-descriptions-item>
            <el-descriptions-item label="Artifact Path">{{ form.artifactOutputPath || '-' }}</el-descriptions-item>
            <el-descriptions-item label="Risk Level">
              <el-tag :type="riskTagType" size="small">{{ riskLabel }}</el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="Mode">{{ form.executionMode }}</el-descriptions-item>
            <el-descriptions-item label="Approval">{{ form.approvalMode }}</el-descriptions-item>
            <el-descriptions-item label="Unsaved Changes">
              <el-tag :type="isDirty ? 'warning' : 'info'" size="small">{{ isDirty ? 'Yes' : 'No' }}</el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="Last Saved At">{{ lastSavedAt || '-' }}</el-descriptions-item>
          </el-descriptions>

          <el-alert
            v-if="saveSuccess"
            title="沙盒配置保存成功"
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
            <el-button type="success" :loading="saving" @click="handleSave">保存沙盒配置</el-button>
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
  max-width: 1580px;
  margin: 0 auto;
}

.state-block {
  margin-top: 12px;
}

.sandbox-layout {
  margin-top: 12px;
  display: grid;
  grid-template-columns: 320px minmax(720px, 1fr) 320px;
  gap: 16px;
  align-items: start;
}

.scenario-card,
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

.scenario-list {
  display: grid;
  gap: 10px;
}

.scenario-item {
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  padding: 12px;
  background: #fff;
  text-align: left;
  cursor: pointer;
}

.scenario-item.active {
  border-color: #16a34a;
  box-shadow: 0 0 0 2px rgba(22, 163, 74, 0.12);
}

.scenario-title {
  font-weight: 600;
}

.scenario-desc {
  margin: 8px 0 0;
  color: #64748b;
  line-height: 1.45;
  font-size: 13px;
}

.section-list h4 {
  margin: 0;
  font-size: 14px;
}

.section-list ul {
  margin: 10px 0 0;
  padding-left: 16px;
  display: grid;
  gap: 10px;
}

.section-list li p {
  margin: 4px 0 0;
  color: #64748b;
  line-height: 1.45;
}

.settings-form {
  max-width: 100%;
}

.inline-switches {
  display: grid;
  gap: 12px;
  width: 100%;
}

.status-card {
  position: sticky;
  top: 10px;
}

.status-meta {
  margin-top: 8px;
}

.save-state {
  margin-top: 10px;
}

.actions {
  margin-top: 8px;
}

.sticky-actions {
  margin-top: 14px;
}

@media (max-width: 1365px) {
  .settings-page {
    overflow-x: auto;
    padding-bottom: 10px;
  }

  .sandbox-layout {
    min-width: 1320px;
  }
}
</style>