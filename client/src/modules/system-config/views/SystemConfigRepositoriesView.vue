<script setup lang="ts">
import { computed, reactive, ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { useSystemConfigStore } from '../store/useSystemConfigStore'
import { fetchGitHubRepositoriesByToken, type RemoteGitRepositoryDto } from '../../repo-management/api/repo-management.api'
import type { ConfigType, ConfigResponse } from '../api/config.types'

interface RepositoryConfigForm {
  provider: string
  owner: string
  repositoryName: string
  defaultBranch: string
  gitToken: string
  authMode: string
  mergeStrategies: string[]
  requireChecks: boolean
}

const store = useSystemConfigStore()
const loading = ref(false)
const error = ref('')
const saving = ref(false)
const repoLoading = ref(false)
const repoFetchError = ref('')
const tokenHint = ref('')
const formRef = ref<FormInstance>()
const selectedRemoteRepo = ref('')
const remoteRepositories = ref<RemoteGitRepositoryDto[]>([])

const GITHUB_TOKEN_PATTERN = /^(ghp_|github_pat_|gho_|ghu_|ghs_|ghr_)[A-Za-z0-9_]{10,}$/

const form = reactive<RepositoryConfigForm>({
  provider: 'github',
  owner: 'miyamaerei',
  repositoryName: 'AutoCodeForge',
  defaultBranch: 'main',
  gitToken: '',
  authMode: 'token',
  mergeStrategies: ['squash', 'merge'],
  requireChecks: true,
})

const repositoryPreview = computed(() => [
  {
    fullName: `${form.owner}/${form.repositoryName}`,
    url: form.owner && form.repositoryName ? `https://github.com/${form.owner}/${form.repositoryName}` : '-',
    provider: form.provider,
    defaultBranch: form.defaultBranch,
    authMode: form.authMode,
  },
])

const hasData = computed(() => true)

const rules: FormRules<RepositoryConfigForm> = {
  provider: [{ required: true, message: '请选择代码托管平台', trigger: 'change' }],
  owner: [{ required: true, message: 'Owner 不能为空', trigger: 'blur' }],
  repositoryName: [{ required: true, message: '仓库名不能为空', trigger: 'blur' }],
  defaultBranch: [{ required: true, message: '默认分支不能为空', trigger: 'blur' }],
  gitToken: [{ min: 8, message: 'Token 长度至少 8 位', trigger: 'blur' }],
  authMode: [{ required: true, message: '请选择鉴权方式', trigger: 'change' }],
  mergeStrategies: [
    {
      type: 'array',
      required: true,
      min: 1,
      message: '至少选择一个合并策略',
      trigger: 'change',
    },
  ],
}

const configKey = 'repository-config'

function mayBeGitHubToken(value: unknown): string {
  if (typeof value !== 'string') {
    return ''
  }

  const normalized = value.trim()
  if (!normalized || normalized.includes(' ')) {
    return ''
  }

  if (GITHUB_TOKEN_PATTERN.test(normalized)) {
    return normalized
  }

  return normalized.length >= 24 && /[A-Za-z]/.test(normalized) && /\d/.test(normalized) ? normalized : ''
}

function collectStringValues(input: unknown, collector: string[]) {
  if (typeof input === 'string') {
    collector.push(input)
    return
  }

  if (Array.isArray(input)) {
    input.forEach((item) => collectStringValues(item, collector))
    return
  }

  if (input && typeof input === 'object') {
    Object.values(input as Record<string, unknown>).forEach((value) => collectStringValues(value, collector))
  }
}

function findTokenInConfig(configs: ConfigResponse[]): string {
  for (const config of configs) {
    const key = (config.configKey || '').toLowerCase()
    const direct = mayBeGitHubToken(config.configValue)
    if (direct && (key.includes('git') || key.includes('github') || key.includes('token') || key.includes('pat'))) {
      return direct
    }

    try {
      const parsed = JSON.parse(config.configValue)
      const values: string[] = []
      collectStringValues(parsed, values)
      const candidate = values.map(mayBeGitHubToken).find(Boolean)
      if (candidate) {
        return candidate
      }
    } catch {
      // keep scanning other configs
    }
  }

  return ''
}

function applyRemoteRepository(fullName: string) {
  const target = remoteRepositories.value.find((item) => item.fullName === fullName)
  if (!target) {
    return
  }
  form.owner = target.ownerLogin
  form.repositoryName = target.name
  form.defaultBranch = target.defaultBranch || 'main'
  form.provider = 'github'
}

async function loadTokenFromConfigs() {
  try {
    await Promise.all([
      store.loadConfigs('ApiKey' as ConfigType),
      store.loadConfigs('Integration' as ConfigType),
      store.loadConfigs('Repository' as ConfigType),
    ])

    const candidates = [
      ...store.getConfigs('Repository' as ConfigType),
      ...store.getConfigs('ApiKey' as ConfigType),
      ...store.getConfigs('Integration' as ConfigType),
    ]

    const token = findTokenInConfig(candidates)
    if (token && !form.gitToken) {
      form.gitToken = token
      tokenHint.value = '已从配置中检测到 Git Token，可直接拉取仓库列表。'
    }
  } catch {
    tokenHint.value = ''
  }
}

async function fetchRemoteRepositories() {
  if (form.provider !== 'github') {
    ElMessage.warning('当前仅支持 GitHub 仓库自动拉取')
    return
  }

  const token = form.gitToken.trim()
  if (!token) {
    repoFetchError.value = '请先配置 Git Token 再拉取仓库列表。'
    return
  }

  repoLoading.value = true
  repoFetchError.value = ''
  try {
    const repos = await fetchGitHubRepositoriesByToken(token)
    remoteRepositories.value = repos
    if (!repos.length) {
      ElMessage.info('未拉取到可访问的仓库')
      return
    }
    selectedRemoteRepo.value = repos[0].fullName
    applyRemoteRepository(selectedRemoteRepo.value)
    ElMessage.success(`已拉取 ${repos.length} 个仓库`)
  } catch (err) {
    repoFetchError.value = err instanceof Error ? err.message : '拉取仓库失败'
    ElMessage.error(repoFetchError.value)
  } finally {
    repoLoading.value = false
  }
}

// Load configs on mount
onMounted(async () => {
  loading.value = true
  error.value = ''
  try {
    await store.loadConfigs('Repository' as ConfigType)
    const configs = store.getConfigs('Repository' as ConfigType)
    const savedForm = configs.find((c) => c.configKey === configKey)
    if (savedForm) {
      const parsed = JSON.parse(savedForm.configValue)
      Object.assign(form, parsed)
    }
    await loadTokenFromConfigs()
  } catch (err) {
    error.value = err instanceof Error ? err.message : '加载配置失败'
  } finally {
    loading.value = false
  }
})

const handleSave = async () => {
  if (!formRef.value) {
    return
  }
  await formRef.value.validate(async (valid) => {
    if (!valid) {
      return
    }
    saving.value = true
    error.value = ''
    try {
      await store.saveConfig('Repository' as ConfigType, {
        configKey,
        configValue: JSON.stringify(form),
        isEncrypted: false,
        description: 'Repository configuration',
      })
      ElMessage.success('Repositories 配置已保存')
    } catch (err) {
      error.value = err instanceof Error ? err.message : '保存配置失败'
      ElMessage.error(error.value)
    } finally {
      saving.value = false
    }
  })
}
</script>

<template>
  <section class="settings-page">
    <div class="settings-shell">
      <el-page-header content="Repositories" />

      <el-skeleton v-if="loading" :rows="8" animated class="state-block" />
      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" class="state-block" />
      <el-empty v-else-if="!hasData" description="No repository configuration" class="state-block" />

      <el-card v-else shadow="hover" class="main-card">
        <template #header>
          <div class="card-header">
            <strong>Repository Integrations</strong>
            <span class="card-subtitle">仓库接入、分支策略与权限配置</span>
          </div>
        </template>

        <el-form ref="formRef" :model="form" :rules="rules" label-width="150px" class="settings-form">
          <el-row :gutter="16">
            <el-col :span="8">
              <el-form-item label="Provider" prop="provider">
                <el-select v-model="form.provider">
                  <el-option label="GitHub" value="github" />
                  <el-option label="Azure DevOps" value="azure-devops" />
                  <el-option label="GitLab" value="gitlab" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Owner" prop="owner">
                <el-input v-model="form.owner" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Repository" prop="repositoryName">
                <el-input v-model="form.repositoryName" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-row :gutter="16">
            <el-col :span="8">
              <el-form-item label="默认分支" prop="defaultBranch">
                <el-input v-model="form.defaultBranch" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Git Token" prop="gitToken">
                <el-input v-model="form.gitToken" type="password" show-password placeholder="输入或自动读取 Git Token" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="鉴权方式" prop="authMode">
                <el-select v-model="form.authMode">
                  <el-option label="Token" value="token" />
                  <el-option label="App" value="app" />
                  <el-option label="OAuth" value="oauth" />
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>

          <el-row :gutter="16">
            <el-col :span="8">
              <el-form-item label="检查门禁">
                <el-switch v-model="form.requireChecks" active-text="启用" />
              </el-form-item>
            </el-col>
            <el-col :span="16">
              <el-form-item label="远程仓库">
                <el-space wrap>
                  <el-button :loading="repoLoading" @click="fetchRemoteRepositories">拉取我的仓库</el-button>
                  <el-select
                    v-model="selectedRemoteRepo"
                    filterable
                    clearable
                    placeholder="从拉取结果中选择仓库"
                    style="min-width: 360px"
                    @change="(value) => applyRemoteRepository(String(value || ''))"
                  >
                    <el-option
                      v-for="item in remoteRepositories"
                      :key="item.id"
                      :label="item.fullName"
                      :value="item.fullName"
                    />
                  </el-select>
                </el-space>
              </el-form-item>
            </el-col>
          </el-row>

          <el-alert
            v-if="tokenHint"
            :title="tokenHint"
            type="success"
            show-icon
            :closable="false"
            class="state-block"
          />

          <el-alert
            v-if="repoFetchError"
            :title="repoFetchError"
            type="error"
            show-icon
            :closable="false"
            class="state-block"
          />

          <el-form-item label="合并策略" prop="mergeStrategies">
            <el-checkbox-group v-model="form.mergeStrategies">
              <el-checkbox value="merge">Merge</el-checkbox>
              <el-checkbox value="squash">Squash</el-checkbox>
              <el-checkbox value="rebase">Rebase</el-checkbox>
            </el-checkbox-group>
          </el-form-item>

          <el-table :data="repositoryPreview" stripe border class="preview-table">
            <el-table-column prop="fullName" label="Preview Repository" min-width="320" />
            <el-table-column prop="url" label="URL" min-width="340" />
            <el-table-column prop="provider" label="Provider" width="180" />
            <el-table-column prop="defaultBranch" label="Branch" width="180" />
            <el-table-column prop="authMode" label="Auth" width="150" />
          </el-table>

          <div class="actions">
            <el-button type="primary" :loading="saving" @click="handleSave">保存配置</el-button>
          </div>
        </el-form>
      </el-card>
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
  max-width: 1180px;
  margin: 0 auto;
}

.state-block {
  margin-top: 0.75rem;
}

.main-card {
  margin-top: 0.75rem;
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

.settings-form {
  max-width: 1020px;
}

.preview-table {
  margin-top: 8px;
}

.actions {
  margin-top: 12px;
}

@media (max-width: 1365px) {
  .settings-page {
    overflow-x: auto;
    padding-bottom: 8px;
  }
}
</style>
