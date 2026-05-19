<script setup lang="ts">
import { onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useSystemConfigNotifications } from '../composables/useSystemConfigNotifications'

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
  enabledChannelsCount,
  enabledEventsCount,
  noiseLevelLabel,
  noiseTagType,
  loadConfig,
  saveConfig,
  applyScenario,
} = useSystemConfigNotifications()

const handleSave = async () => {
  await saveConfig()
  if (saveSuccess.value) {
    ElMessage.success('通知配置已保存')
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
      <el-page-header content="Notifications" />

      <el-skeleton v-if="loading" :rows="9" animated class="state-block" />
      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" class="state-block" />
      <el-empty v-else-if="!hasData" description="暂无可配置的通知选项" class="state-block" />

      <div v-else class="notification-layout">
        <el-card shadow="hover" class="scenario-card">
          <template #header>
            <div class="card-header">
              <strong>推荐策略</strong>
              <span class="card-subtitle">按团队阶段切换通知密度</span>
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
            <h4>建议配置项</h4>
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
              <strong>通知策略编辑</strong>
              <span class="card-subtitle">让关键信息可见，同时避免噪音</span>
            </div>
          </template>

          <el-form label-width="180px" class="settings-form">
            <el-row :gutter="16">
              <el-col :span="12">
                <el-form-item label="项目标识">
                  <el-input v-model="form.projectName" placeholder="例如 AutoCodeForge" />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="专注策略">
                  <el-select v-model="form.focusMode">
                    <el-option label="Balanced" value="balanced" />
                    <el-option label="Quiet" value="quiet" />
                    <el-option label="Critical Only" value="critical-only" />
                  </el-select>
                </el-form-item>
              </el-col>
            </el-row>

            <el-form-item label="通知渠道">
              <div class="inline-switches">
                <el-switch v-model="form.enableInApp" active-text="站内通知" />
                <el-switch v-model="form.enableEmail" active-text="邮件通知" />
              </div>
            </el-form-item>

            <el-divider content-position="left">邮件发送配置</el-divider>

            <el-row v-if="form.enableEmail" :gutter="16">
              <el-col :span="12">
                <el-form-item label="邮件服务商">
                  <el-select v-model="form.emailProvider">
                    <el-option label="SMTP" value="smtp" />
                    <el-option label="Amazon SES" value="ses" />
                    <el-option label="SendGrid" value="sendgrid" />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="发件人名称">
                  <el-input v-model="form.emailFromName" placeholder="例如 AutoCodeForge Bot" />
                </el-form-item>
              </el-col>
            </el-row>

            <el-row v-if="form.enableEmail" :gutter="16">
              <el-col :span="12">
                <el-form-item label="发件邮箱">
                  <el-input v-model="form.emailFromAddress" placeholder="例如 noreply@company.com" />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="SMTP Host">
                  <el-input v-model="form.smtpHost" placeholder="例如 smtp.office365.com" />
                </el-form-item>
              </el-col>
            </el-row>

            <el-row v-if="form.enableEmail" :gutter="16">
              <el-col :span="12">
                <el-form-item label="SMTP Port">
                  <el-input-number v-model="form.smtpPort" :min="1" :max="65535" />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="连接安全">
                  <el-switch v-model="form.smtpSecure" active-text="SSL/TLS" inactive-text="STARTTLS" />
                </el-form-item>
              </el-col>
            </el-row>

            <el-row v-if="form.enableEmail" :gutter="16">
              <el-col :span="12">
                <el-form-item label="SMTP 用户名">
                  <el-input v-model="form.smtpUsername" placeholder="通常为邮箱地址" />
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="SMTP 密码">
                  <el-input
                    v-model="form.smtpPasswordPlaceholder"
                    type="password"
                    show-password
                    placeholder="输入应用专用密码（仅用于保存配置）"
                  />
                </el-form-item>
              </el-col>
            </el-row>

            <el-row :gutter="16">
              <el-col :span="12">
                <el-form-item label="汇总频率">
                  <el-select v-model="form.digestMode">
                    <el-option label="实时" value="immediate" />
                    <el-option label="每小时" value="hourly" />
                    <el-option label="每日汇总" value="daily" />
                  </el-select>
                </el-form-item>
              </el-col>
              <el-col :span="12">
                <el-form-item label="仅 @我 时通知">
                  <el-switch v-model="form.onlyMentioned" />
                </el-form-item>
              </el-col>
            </el-row>

            <el-form-item label="事件范围">
              <div class="event-grid">
                <el-checkbox v-model="form.notifyOnBuildFailed">构建失败</el-checkbox>
                <el-checkbox v-model="form.notifyOnReviewRequested">收到代码评审请求</el-checkbox>
                <el-checkbox v-model="form.notifyOnDeploymentFinished">部署完成</el-checkbox>
                <el-checkbox v-model="form.notifyOnSecurityAlert">安全告警</el-checkbox>
              </div>
            </el-form-item>

            <el-form-item label="投递时间窗口">
              <div class="window-grid">
                <el-switch v-model="form.deliveryWindow.enabled" active-text="启用时间窗口" />
                <el-time-select
                  v-model="form.deliveryWindow.start"
                  :max-time="form.deliveryWindow.end"
                  start="00:00"
                  step="00:30"
                  end="23:30"
                  placeholder="开始时间"
                />
                <el-time-select
                  v-model="form.deliveryWindow.end"
                  :min-time="form.deliveryWindow.start"
                  start="00:30"
                  step="00:30"
                  end="24:00"
                  placeholder="结束时间"
                />
                <el-input v-model="form.deliveryWindow.timezone" placeholder="例如 Asia/Shanghai" />
              </div>
            </el-form-item>
          </el-form>
        </el-card>

        <el-card shadow="never" class="status-card">
          <template #header>
            <div class="card-header">
              <strong>配置状态</strong>
              <span class="card-subtitle">保存前快速检查</span>
            </div>
          </template>

          <el-descriptions :column="1" border size="small" class="status-meta">
            <el-descriptions-item label="Project">{{ form.projectName || '-' }}</el-descriptions-item>
            <el-descriptions-item label="Focus Mode">{{ form.focusMode }}</el-descriptions-item>
            <el-descriptions-item label="Enabled Channels">{{ enabledChannelsCount }}</el-descriptions-item>
            <el-descriptions-item label="Enabled Events">{{ enabledEventsCount }}</el-descriptions-item>
            <el-descriptions-item label="Noise Level">
              <el-tag :type="noiseTagType" size="small">{{ noiseLevelLabel }}</el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="Digest">{{ form.digestMode }}</el-descriptions-item>
            <el-descriptions-item label="Window">
              {{ form.deliveryWindow.enabled ? form.deliveryWindow.start + ' - ' + form.deliveryWindow.end : '关闭' }}
            </el-descriptions-item>
            <el-descriptions-item label="Unsaved Changes">
              <el-tag :type="isDirty ? 'warning' : 'info'" size="small">{{ isDirty ? 'Yes' : 'No' }}</el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="Last Saved At">{{ lastSavedAt || '-' }}</el-descriptions-item>
          </el-descriptions>

          <el-alert
            v-if="saveSuccess"
            title="通知配置保存成功"
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
            <el-button type="success" :loading="saving" @click="handleSave">保存通知配置</el-button>
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

.notification-layout {
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
  border-color: #0ea5e9;
  box-shadow: 0 0 0 2px rgba(14, 165, 233, 0.16);
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

.event-grid {
  width: 100%;
  display: grid;
  grid-template-columns: repeat(2, minmax(220px, 1fr));
  gap: 10px 16px;
}

.window-grid {
  width: 100%;
  display: grid;
  grid-template-columns: repeat(4, minmax(120px, 1fr));
  gap: 10px;
  align-items: center;
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
  margin-top: 12px;
}

.sticky-actions {
  position: sticky;
  bottom: 0;
  background: #fff;
  padding-top: 10px;
}

@media (max-width: 1365px) {
  .settings-page {
    overflow-x: auto;
    padding-bottom: 8px;
  }
}
</style>
