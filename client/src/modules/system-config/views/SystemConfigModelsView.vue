<script setup lang="ts">
import { ref, reactive, computed } from 'vue'
import { ElMessage } from 'element-plus'

interface Model {
  id: string
  name: string
  provider: string
  version: string
  status: string
  maxTokens: number
  type: 'api' | 'cli'
}

const models = ref<Model[]>([
  {
    id: 'gpt-4',
    name: 'GPT-4',
    provider: 'OpenAI',
    version: '4o',
    status: 'active',
    maxTokens: 128000,
    type: 'api',
  },
  {
    id: 'gpt-35',
    name: 'GPT-3.5 Turbo',
    provider: 'OpenAI',
    version: '0125',
    status: 'active',
    maxTokens: 4096,
    type: 'api',
  },
  {
    id: 'claude3',
    name: 'Claude 3 Opus',
    provider: 'Anthropic',
    version: '1.0',
    status: 'inactive',
    maxTokens: 200000,
    type: 'api',
  },
  {
    id: 'llama2',
    name: 'Llama 2',
    provider: 'Meta',
    version: '70b',
    status: 'inactive',
    maxTokens: 4096,
    type: 'api',
  },
  {
    id: 'copilot-cli',
    name: 'GitHub Copilot CLI',
    provider: 'GitHub',
    version: 'latest',
    status: 'active',
    maxTokens: 8192,
    type: 'cli',
  },
])

const defaultModel = ref('gpt-4')

const showCopilotConfig = ref(false)

const copilotConfig = reactive({
  executable: 'copilot',
  organization: '',
  authMode: 'interactive' as 'interactive' | 'pat',
  patEnvVar: 'GH_TOKEN',
})

const installCommand = computed(() => {
  if (process.platform === 'win32') {
    return 'winget install GitHub.Copilot'
  }
  return 'npm install -g @github/copilot-cli'
})

const verifyCommand = computed(() => `${copilotConfig.executable} --version`)

const handleCopilotConfig = () => {
  showCopilotConfig.value = !showCopilotConfig.value
}

const saveCopilotConfig = () => {
  localStorage.setItem('copilot-config', JSON.stringify(copilotConfig))
  ElMessage.success('Copilot CLI 配置已保存')
}

const loadCopilotConfig = () => {
  const saved = localStorage.getItem('copilot-config')
  if (saved) {
    Object.assign(copilotConfig, JSON.parse(saved))
  }
}

loadCopilotConfig()

const copyCommand = async (command: string, label: string) => {
  try {
    await navigator.clipboard.writeText(command)
    ElMessage.success(`${label}已复制`)
  } catch {
    ElMessage.error('复制失败，请手动复制')
  }
}
</script>

<template>
  <section class="models-config">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>模型选择</span>
        </div>
      </template>

      <el-alert type="info" :closable="false" class="alert-box">
        <span>选择默认模型用于AI任务执行。当前默认模型：<strong>{{ defaultModel }}</strong></span>
      </el-alert>

      <el-table :data="models" stripe style="margin-top: 1rem">
        <el-table-column prop="name" label="模型名称" width="200" />
        <el-table-column prop="provider" label="提供商" width="150" />
        <el-table-column prop="version" label="版本" width="100" />
        <el-table-column prop="maxTokens" label="最大Token" width="120" />
        <el-table-column prop="type" label="类型" width="100">
          <template #default="{ row }">
            <el-tag :type="row.type === 'api' ? 'primary' : 'warning'">
              {{ row.type === 'api' ? 'API' : 'CLI' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.status === 'active' ? 'success' : 'info'">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="250">
          <template #default="{ row }">
            <el-radio v-model="defaultModel" :label="row.id" @change="defaultModel = row.id">
              设为默认
            </el-radio>
            <el-button
              v-if="row.type === 'cli'"
              type="text"
              @click="handleCopilotConfig"
              class="config-btn"
            >
              配置CLI
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-card v-if="showCopilotConfig" class="content-card copilot-config-card">
      <template #header>
        <div class="card-header">
          <span>GitHub Copilot CLI 配置</span>
          <el-button type="text" @click="showCopilotConfig = false">关闭</el-button>
        </div>
      </template>

      <el-form label-width="160px" class="copilot-form">
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="CLI 可执行文件">
              <el-input
                v-model="copilotConfig.executable"
                placeholder="默认: copilot"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="组织名称（可选）">
              <el-input
                v-model="copilotConfig.organization"
                placeholder="输入组织名"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="认证模式">
              <el-select v-model="copilotConfig.authMode">
                <el-option label="交互式 (/login)" value="interactive" />
                <el-option label="PAT (环境变量)" value="pat" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12" v-if="copilotConfig.authMode === 'pat'">
            <el-form-item label="PAT 环境变量">
              <el-select v-model="copilotConfig.patEnvVar">
                <el-option label="GH_TOKEN" value="GH_TOKEN" />
                <el-option label="GITHUB_TOKEN" value="GITHUB_TOKEN" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="安装命令">
          <div class="command-row">
            <el-input :model-value="installCommand" readonly />
            <el-button @click="copyCommand(installCommand, '安装命令')">复制</el-button>
          </div>
        </el-form-item>

        <el-form-item label="验证命令">
          <div class="command-row">
            <el-input :model-value="verifyCommand" readonly />
            <el-button @click="copyCommand(verifyCommand, '验证命令')">复制</el-button>
          </div>
        </el-form-item>

        <el-alert
          v-if="copilotConfig.authMode === 'pat'"
          title="GitHub PAT 配置说明"
          type="warning"
          show-icon
          :closable="false"
        >
          <p>请在 GitHub 创建 fine-grained token，并授予 Copilot Requests 权限。</p>
          <p>将 token 值设置到环境变量 <strong>{{ copilotConfig.patEnvVar }}</strong> 中。</p>
        </el-alert>

        <el-alert
          v-else
          title="交互式认证说明"
          type="info"
          show-icon
          :closable="false"
        >
          <p>执行 <code>{{ copilotConfig.executable }} login</code> 完成交互式认证。</p>
        </el-alert>

        <div class="actions">
          <el-button type="success" @click="saveCopilotConfig">保存配置</el-button>
        </div>
      </el-form>
    </el-card>
  </section>
</template>

<style scoped>
.models-config {
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

.alert-box {
  margin-bottom: 1rem;
}

.config-btn {
  margin-left: 8px;
}

.copilot-config-card {
  margin-top: 1rem;
}

.copilot-form {
  max-width: 800px;
}

.command-row {
  width: 100%;
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 8px;
}

.actions {
  margin-top: 16px;
}
</style>
