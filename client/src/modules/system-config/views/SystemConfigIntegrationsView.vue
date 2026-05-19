<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useSystemConfigIntegrations } from '../composables/useSystemConfigIntegrations'

const azurePatDocsUrl =
  'https://learn.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate'
const azureDevOpsPortalUrl = 'https://dev.azure.com/'
const githubPatCreateUrl = 'https://github.com/settings/personal-access-tokens/new'

const {
  loading,
  error,
  saving,
  saveError,
  saveSuccess,
  lastSavedAt,
  lastAuthCommand,
  authHint,
  searchKeyword,
  integrations,
  filteredIntegrations,
  selectedIntegration,
  enabledCount,
  configuredCount,
  cliStatus,
  form,
  hasData,
  installCommand,
  verifyCommand,
  launchCommand,
  loginGuide,
  azurePatGuide,
  hostBridgeAvailable,
  loadConfigs,
  saveConfigs,
  checkCopilotCliStatus,
  triggerGithubCopilotAuth,
  selectIntegration,
  getGenericForm,
  isGenericIntegration,
  updateGenericConfig,
  persistConfig,
} = useSystemConfigIntegrations()

const selectedGenericForm = computed(() => {
  if (!selectedIntegration.value) {
    return null
  }
  return getGenericForm(selectedIntegration.value.id)
})

const statusTagType = (status: string) => {
  if (status === 'configured') {
    return 'success'
  }
  if (status === 'beta') {
    return 'warning'
  }
  return 'info'
}

const handleSave = async () => {
  await saveConfigs()
  if (saveSuccess.value) {
    ElMessage.success('集成配置已保存')
  }
  if (saveError.value) {
    ElMessage.error(saveError.value)
  }
}

const handleGithubCopilotAuth = async () => {
  await triggerGithubCopilotAuth()
  ElMessage.info('已触发 Copilot CLI 认证流程，请按页面指引完成登录')
}

const handleCheckCli = async () => {
  await checkCopilotCliStatus()
  if (cliStatus.installed) {
    ElMessage.success('Copilot CLI 已检测到')
    return
  }
  ElMessage.warning('未检测到 Copilot CLI，可先执行安装命令')
}

const copyCommand = async (command: string, label: string) => {
  try {
    await navigator.clipboard.writeText(command)
    ElMessage.success(`${label}已复制`)
  } catch {
    ElMessage.error('复制失败，请手动复制命令')
  }
}

const handleFieldChanged = () => {
  persistConfig()
}

const handleGenericFieldChanged = (field: 'endpoint' | 'workspace' | 'authType' | 'credentialHint', value: string) => {
  if (!selectedIntegration.value || !isGenericIntegration(selectedIntegration.value.id)) {
    return
  }
  updateGenericConfig(selectedIntegration.value.id, { [field]: value })
}

onMounted(() => {
  loadConfigs()
})
</script>

<template>
  <section class="settings-page">
    <div class="settings-shell">
      <el-page-header content="Integrations" />

      <el-skeleton v-if="loading" :rows="10" animated class="state-block" />
      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" class="state-block" />
      <el-empty v-else-if="!hasData" description="暂无可配置集成" class="state-block" />

      <div v-else class="integration-layout">
        <el-card shadow="hover" class="catalog-card">
          <template #header>
            <div class="card-header">
              <strong>集成目录</strong>
              <span class="card-subtitle">当前 {{ integrations.length }} 项，可继续扩展</span>
            </div>
          </template>

          <el-input v-model="searchKeyword" placeholder="搜索集成名称、标签" clearable />

          <div class="catalog-list">
            <button
              v-for="item in filteredIntegrations"
              :key="item.id"
              class="catalog-item"
              :class="{ active: selectedIntegration?.id === item.id }"
              @click="selectIntegration(item.id)"
            >
              <div class="catalog-item-title">
                <span>{{ item.name }}</span>
                <el-tag :type="statusTagType(item.status)" size="small">{{ item.status }}</el-tag>
              </div>
              <p class="catalog-item-desc">{{ item.description }}</p>
              <div class="catalog-item-meta">
                <el-tag v-for="tag in item.tags" :key="tag" size="small" effect="plain">{{ tag }}</el-tag>
              </div>
            </button>
          </div>
        </el-card>

        <el-card shadow="hover" class="detail-card">
          <template #header>
            <div class="card-header">
              <strong>{{ selectedIntegration?.name || 'Integration' }}</strong>
              <span class="card-subtitle">{{ selectedIntegration?.description || '-' }}</span>
            </div>
          </template>

          <template v-if="selectedIntegration?.id === 'azure-pwt'">
            <el-form label-width="160px" class="settings-form">
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="Auth Mode">
                    <el-select v-model="form.azurePwt.authMode" @change="handleFieldChanged">
                      <el-option label="Microsoft Entra (Recommended)" value="entra" />
                      <el-option label="Azure DevOps PAT (Limited)" value="azure-devops-pat" />
                    </el-select>
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="PWT Command Hint">
                    <el-input v-model="form.azurePwt.commandHint" placeholder="例如 az login" @change="handleFieldChanged" />
                  </el-form-item>
                </el-col>
              </el-row>

              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="Tenant ID">
                    <el-input v-model="form.azurePwt.tenantId" placeholder="输入 Azure Tenant ID" @change="handleFieldChanged" />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="Subscription ID">
                    <el-input
                      v-model="form.azurePwt.subscriptionId"
                      placeholder="输入 Azure Subscription ID"
                      @change="handleFieldChanged"
                    />
                  </el-form-item>
                </el-col>
              </el-row>

              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="Project Name">
                    <el-input v-model="form.azurePwt.projectName" placeholder="输入项目标识" @change="handleFieldChanged" />
                  </el-form-item>
                </el-col>
              </el-row>

              <template v-if="form.azurePwt.authMode === 'azure-devops-pat'">
                <el-row :gutter="16">
                  <el-col :span="12">
                    <el-form-item label="Azure DevOps Org URL">
                      <el-input
                        v-model="form.azurePwt.devOpsOrganizationUrl"
                        placeholder="https://dev.azure.com/<org>"
                        @change="handleFieldChanged"
                      />
                    </el-form-item>
                  </el-col>
                  <el-col :span="12">
                    <el-form-item label="PAT Secret Ref">
                      <el-input
                        v-model="form.azurePwt.patSecretRef"
                        placeholder="例如 AZDO_PAT"
                        @change="handleFieldChanged"
                      />
                    </el-form-item>
                  </el-col>
                </el-row>

                <el-row :gutter="16">
                  <el-col :span="12">
                    <el-form-item label="PAT Scope Hint">
                      <el-input
                        v-model="form.azurePwt.patScopeHint"
                        placeholder="例如仅授予 Agent Pools Read&Manage"
                        @change="handleFieldChanged"
                      />
                    </el-form-item>
                  </el-col>
                  <el-col :span="12">
                    <el-form-item label="Rotation Days">
                      <el-input-number
                        v-model="form.azurePwt.patRotationDays"
                        :min="7"
                        :max="90"
                        @change="handleFieldChanged"
                      />
                    </el-form-item>
                  </el-col>
                </el-row>

                <el-alert title="Azure DevOps PAT 配置位置" type="warning" show-icon :closable="false">
                  <template #default>
                    <p class="helper-text">
                      在 Azure DevOps 组织中进入 User settings > Personal access tokens > New Token 创建 PAT。
                    </p>
                    <div class="helper-links">
                      <el-link :href="azureDevOpsPortalUrl" target="_blank" type="primary">
                        打开 Azure DevOps 门户
                      </el-link>
                      <el-link :href="azurePatDocsUrl" target="_blank" type="primary">
                        打开 PAT 官方文档
                      </el-link>
                    </div>
                  </template>
                </el-alert>
              </template>

              <el-alert
                :title="form.azurePwt.authMode === 'entra' ? 'Azure 认证建议' : 'Azure DevOps PAT 使用提醒'"
                :type="form.azurePwt.authMode === 'entra' ? 'success' : 'warning'"
                :description="azurePatGuide"
                show-icon
                :closable="false"
              />
            </el-form>
          </template>

          <template v-else-if="selectedIntegration?.id === 'github-copilot'">
            <el-form label-width="160px" class="settings-form">
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="Organization">
                    <el-input
                      v-model="form.githubCopilot.organization"
                      placeholder="输入组织名（可选）"
                      @change="handleFieldChanged"
                    />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="CLI Executable">
                    <el-input v-model="form.githubCopilot.executable" placeholder="默认 copilot" @change="handleFieldChanged" />
                  </el-form-item>
                </el-col>
              </el-row>

              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="Install Channel">
                    <el-select v-model="form.githubCopilot.installChannel" @change="handleFieldChanged">
                      <el-option label="WinGet" value="winget" />
                      <el-option label="npm" value="npm" />
                    </el-select>
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="Auth Mode">
                    <el-select v-model="form.githubCopilot.authMode" @change="handleFieldChanged">
                      <el-option label="Interactive (/login)" value="interactive" />
                      <el-option label="PAT (env token)" value="pat" />
                    </el-select>
                  </el-form-item>
                </el-col>
              </el-row>

              <el-form-item v-if="form.githubCopilot.authMode === 'pat'" label="PAT Env Var">
                <el-select v-model="form.githubCopilot.patEnvVar" @change="handleFieldChanged">
                  <el-option label="GH_TOKEN" value="GH_TOKEN" />
                  <el-option label="GITHUB_TOKEN" value="GITHUB_TOKEN" />
                </el-select>
              </el-form-item>

              <el-alert
                v-if="form.githubCopilot.authMode === 'pat'"
                title="GitHub PAT 配置位置"
                type="warning"
                show-icon
                :closable="false"
                class="bridge-alert"
              >
                <template #default>
                  <p class="helper-text">
                    请在 GitHub PAT 页面创建 fine-grained token，并授予 Copilot Requests 权限，然后写入
                    {{ form.githubCopilot.patEnvVar }}。
                  </p>
                  <el-link :href="githubPatCreateUrl" target="_blank" type="primary">
                    打开 GitHub PAT 创建页
                  </el-link>
                </template>
              </el-alert>

              <el-form-item label="Install Command">
                <div class="command-row">
                  <el-input :model-value="installCommand" readonly />
                  <el-button @click="copyCommand(installCommand, '安装命令')">复制</el-button>
                </div>
              </el-form-item>

              <el-form-item label="Verify Command">
                <div class="command-row">
                  <el-input :model-value="verifyCommand" readonly />
                  <el-button @click="copyCommand(verifyCommand, '检测命令')">复制</el-button>
                </div>
              </el-form-item>

              <el-form-item label="Launch Command">
                <div class="command-row">
                  <el-input :model-value="launchCommand" readonly />
                  <el-button @click="copyCommand(launchCommand, '启动命令')">复制</el-button>
                </div>
              </el-form-item>

              <el-form-item label="Workspace Policy">
                <el-select v-model="form.githubCopilot.workspacePolicy" @change="handleFieldChanged">
                  <el-option label="Required" value="required" />
                  <el-option label="Optional" value="optional" />
                  <el-option label="Disabled" value="disabled" />
                </el-select>
              </el-form-item>

              <el-alert title="认证说明" :description="loginGuide" type="info" show-icon :closable="false" />

              <div class="actions command-actions">
                <el-button :loading="cliStatus.checking" @click="handleCheckCli">检测 CLI</el-button>
                <el-button type="primary" @click="handleGithubCopilotAuth">触发认证流程</el-button>
              </div>

              <el-alert
                v-if="!hostBridgeAvailable"
                title="当前页面未接入终端桥接"
                description="请复制命令到本机终端执行。若你希望在页面内直接执行命令，需要宿主应用注入 runTerminalCommand 桥接。"
                type="warning"
                show-icon
                :closable="false"
                class="bridge-alert"
              />
            </el-form>
          </template>

          <template v-else-if="selectedIntegration && selectedGenericForm">
            <el-form label-width="160px" class="settings-form">
              <el-form-item label="Endpoint">
                <el-input
                  :model-value="selectedGenericForm.endpoint"
                  placeholder="输入服务 Endpoint"
                  @change="(value: string) => handleGenericFieldChanged('endpoint', value)"
                />
              </el-form-item>
              <el-form-item label="Workspace / Project">
                <el-input
                  :model-value="selectedGenericForm.workspace"
                  placeholder="输入工作区或项目标识"
                  @change="(value: string) => handleGenericFieldChanged('workspace', value)"
                />
              </el-form-item>
              <el-form-item label="Auth Type">
                <el-select
                  :model-value="selectedGenericForm.authType"
                  @change="(value: string) => handleGenericFieldChanged('authType', value)"
                >
                  <el-option label="OAuth" value="oauth" />
                  <el-option label="Token" value="token" />
                  <el-option label="App Credentials" value="app" />
                </el-select>
              </el-form-item>
              <el-form-item label="Credential Hint">
                <el-input
                  :model-value="selectedGenericForm.credentialHint"
                  placeholder="输入密钥说明或变量名"
                  @change="(value: string) => handleGenericFieldChanged('credentialHint', value)"
                />
              </el-form-item>

              <el-alert
                title="扩展说明"
                description="该集成当前为通用占位模板。后续新增字段时只需要在 composable 中扩展对应表单模型，无需改动页面骨架。"
                type="info"
                show-icon
                :closable="false"
              />
            </el-form>
          </template>
        </el-card>

        <el-card shadow="never" class="status-card">
          <template #header>
            <div class="card-header">
              <strong>状态面板</strong>
              <span class="card-subtitle">统一统计与操作</span>
            </div>
          </template>

          <el-descriptions :column="1" border size="small" class="status-meta">
            <el-descriptions-item label="Total Integrations">{{ integrations.length }}</el-descriptions-item>
            <el-descriptions-item label="Enabled">{{ enabledCount }}</el-descriptions-item>
            <el-descriptions-item label="Configured">{{ configuredCount }}</el-descriptions-item>
            <el-descriptions-item label="Selected">{{ selectedIntegration?.name || '-' }}</el-descriptions-item>
            <el-descriptions-item label="Copilot Auth State">{{ form.githubCopilot.authState }}</el-descriptions-item>
            <el-descriptions-item label="CLI Installed">{{ cliStatus.installed ? 'Yes' : 'No / Unknown' }}</el-descriptions-item>
            <el-descriptions-item label="CLI Version Output">{{ cliStatus.version || '-' }}</el-descriptions-item>
            <el-descriptions-item label="Last CLI Check">{{ cliStatus.lastCheckedAt || '-' }}</el-descriptions-item>
            <el-descriptions-item label="Last Auth Command">{{ lastAuthCommand || '-' }}</el-descriptions-item>
            <el-descriptions-item label="Auth Hint">{{ authHint || '-' }}</el-descriptions-item>
            <el-descriptions-item label="Last Saved At">{{ lastSavedAt || '-' }}</el-descriptions-item>
          </el-descriptions>

          <el-alert
            v-if="cliStatus.checkOutput"
            title="CLI 检测输出"
            :description="cliStatus.checkOutput"
            type="info"
            show-icon
            :closable="false"
            class="save-state"
          />

          <el-alert
            v-if="saveSuccess"
            title="配置保存成功"
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
            <el-button type="success" :loading="saving" @click="handleSave">保存全部配置</el-button>
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

.integration-layout {
  margin-top: 12px;
  display: grid;
  grid-template-columns: 320px minmax(720px, 1fr) 340px;
  gap: 16px;
  align-items: start;
}

.catalog-card,
.detail-card,
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

.catalog-list {
  margin-top: 12px;
  display: grid;
  gap: 10px;
  max-height: 700px;
  overflow: auto;
}

.catalog-item {
  text-align: left;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  background: #fff;
  padding: 10px;
  cursor: pointer;
}

.catalog-item.active {
  border-color: #6366f1;
  box-shadow: 0 0 0 2px rgba(99, 102, 241, 0.12);
}

.catalog-item-title {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 8px;
  font-weight: 600;
}

.catalog-item-desc {
  margin: 8px 0;
  color: #64748b;
  line-height: 1.4;
  font-size: 13px;
}

.catalog-item-meta {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.settings-form {
  max-width: 100%;
}

.actions {
  margin-top: 8px;
}

.command-actions {
  display: flex;
  gap: 8px;
}

.command-row {
  width: 100%;
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 8px;
}

.bridge-alert {
  margin-top: 10px;
}

.helper-text {
  margin: 0 0 8px;
  line-height: 1.45;
}

.helper-links {
  display: flex;
  gap: 14px;
  flex-wrap: wrap;
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

.sticky-actions {
  margin-top: 14px;
}

@media (max-width: 1365px) {
  .settings-page {
    overflow-x: auto;
    padding-bottom: 10px;
  }

  .integration-layout {
    min-width: 1320px;
  }
}
</style>
