<script setup lang="ts">
import { onMounted, reactive, ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { fetchGitHubRepositoriesByToken, type RemoteGitRepositoryDto } from '../api/repo-management.api'
import { useRepoManagementStore } from '../store/useRepoManagementStore'
import { useRepoStore } from '@/stores/useRepoStore'
import { useSystemConfigStore } from '../../system-config/store/useSystemConfigStore'
import { fetchConfigs, upsertConfig } from '../../system-config/api/config.api'
import type { ConfigResponse, ConfigType } from '../../system-config/api/config.types'
import { GitProvider, AuthenticationType, MergeStrategy } from '../api/repo-management.types'

/**
 * Token 配置项接口
 */
interface TokenOption {
  configKey: string
  configType: ConfigType
  token: string
  label: string
  description: string
}

const store = useRepoManagementStore()
const repoGlobal = useRepoStore()
const router = useRouter()

function navigateToBranches(id: string) {
  repoGlobal.selectRepository(id)
  router.push({ name: 'repo-management.branches' })
}
const systemConfigStore = useSystemConfigStore()
const { repositories, loading, error, hasRepositories } = storeToRefs(store)
const addDialogVisible = ref(false)
const submitting = ref(false)
const remoteRepoLoading = ref(false)
const remoteRepoError = ref('')
const tokenHint = ref('')
const selectedRemoteRepo = ref('')
const addFormRef = ref<FormInstance>()
const remoteRepos = ref<RemoteGitRepositoryDto[]>([])
const tokenLoading = ref(false)

// Token 管理相关
const tokenOptions = ref<TokenOption[]>([])
const selectedTokenKey = ref<string>('')
const showCustomToken = ref(false)
const customTokenInput = ref('')
const newTokenName = ref('')

const GITHUB_TOKEN_PATTERN = /^(ghp_|github_pat_|gho_|ghu_|ghs_|ghr_)[A-Za-z0-9_]{10,}$/

const providerOptions = [
  { label: 'GitHub', value: GitProvider.GitHub },
  { label: 'GitLab', value: GitProvider.GitLab },
  { label: 'Azure DevOps', value: GitProvider.AzureDevOps },
  { label: 'Bitbucket', value: GitProvider.Bitbucket },
]

const addForm = reactive({
  name: '',
  url: '',
  token: '',
  provider: GitProvider.GitHub,
  branch: 'main',
})

const addFormRules: FormRules = {
  name: [
    { required: true, message: '请输入仓库名称', trigger: 'blur' },
    { min: 2, max: 100, message: '仓库名称长度应在 2 到 100 个字符之间', trigger: 'blur' },
  ],
  url: [
    { required: true, message: '请输入仓库地址', trigger: 'blur' },
    { type: 'url', message: '仓库地址格式不正确', trigger: 'blur' },
  ],
  token: [
    { required: true, message: '请输入访问令牌', trigger: 'blur' },
    { min: 8, message: '访问令牌长度至少为 8 位', trigger: 'blur' },
  ],
}

/**
 * 计算最终的 Token 值
 * 优先级：自定义输入 > 选中的配置 Token > 自动读取的 Token
 */
const finalToken = computed(() => {
  if (showCustomToken.value) {
    return customTokenInput.value.trim()
  }
  if (selectedTokenKey.value) {
    const selected = tokenOptions.value.find((opt) => opt.configKey === selectedTokenKey.value)
    return selected?.token || ''
  }
  return addForm.token
})

const providerLabelMap: Record<number, string> = {
  [GitProvider.GitHub]: 'GitHub',
  [GitProvider.GitLab]: 'GitLab',
  [GitProvider.AzureDevOps]: 'Azure DevOps',
  [GitProvider.Bitbucket]: 'Bitbucket',
}

const authTypeLabelMap: Record<number, string> = {
  [AuthenticationType.Token]: 'Token',
  [AuthenticationType.SshKey]: 'SSH Key',
  [AuthenticationType.UsernamePassword]: 'Username/Password',
}

const mergeStrategyLabelMap: Record<number, string> = {
  [MergeStrategy.MergeCommit]: 'Merge Commit',
  [MergeStrategy.Squash]: 'Squash',
  [MergeStrategy.Rebase]: 'Rebase',
}

function formatProvider(provider: number) {
  return providerLabelMap[provider] || `Unknown(${provider})`
}

function formatAuthType(authType: number) {
  return authTypeLabelMap[authType] || `Unknown(${authType})`
}

function formatMergeStrategy(strategy: number) {
  return mergeStrategyLabelMap[strategy] || `Unknown(${strategy})`
}

/**
 * Token 选择变化时的处理
 */
function handleTokenSelect(value: string) {
  if (value === '__custom__') {
    showCustomToken.value = true
    selectedTokenKey.value = ''
    customTokenInput.value = ''
  } else {
    showCustomToken.value = false
    selectedTokenKey.value = value
    const selected = tokenOptions.value.find((opt) => opt.configKey === value)
    if (selected) {
      addForm.token = selected.token
      tokenHint.value = `已选择: ${selected.label}`
    }
  }
}

/**
 * 保存 Token 到配置
 */
async function saveTokenToConfig() {
  const token = customTokenInput.value.trim()
  const tokenName = newTokenName.value.trim() || `GitHub-Token-${Date.now()}`

  if (!token) {
    ElMessage.warning('请输入有效的 Token')
    return
  }

  if (!mayBeGitHubToken(token)) {
    ElMessage.warning('输入的 Token 格式不正确，GitHub Token 通常以 ghp_、github_pat_ 等开头')
    return
  }

  tokenLoading.value = true
  try {
    const configKey = `credential.github-${tokenName.toLowerCase().replace(/\s+/g, '-')}`
    console.log(`[Token] Saving token with key: ${configKey}`)
    
    await upsertConfig('Git' as ConfigType, {
      configKey,
      configValue: JSON.stringify({
        type: 'credential',
        provider: 'GitHub',
        authMode: 'token',
        token: token,
        description: `GitHub Token: ${tokenName}`,
      }),
      isEncrypted: true,
      description: `GitHub Token: ${tokenName}`,
      group: 'credentials',
    })
    console.log(`[Token] Token saved successfully, key: ${configKey}`)

    // 刷新 Token 列表并等待完成
    console.log('[Token] Reloading token options after save...')
    await loadTokenOptions()

    // 检查新 Token 是否在列表中
    const newTokenOption = tokenOptions.value.find((opt) => opt.configKey === configKey)
    if (newTokenOption) {
      console.log('[Token] New token found in options, selecting it:', newTokenOption)
      selectedTokenKey.value = configKey
      addForm.token = newTokenOption.token
      ElMessage.success(`Token "${tokenName}" 已保存并选中`)
    } else {
      console.warn('[Token] New token NOT found in loaded options!', {
        savedKey: configKey,
        loadedKeys: tokenOptions.value.map((opt) => opt.configKey),
      })
      // 降级处理：至少用保存时的值
      showCustomToken.value = false
      addForm.token = token
      ElMessage.warning(`Token 已保存，但刷新配置列表时未找到，请检查浏览器控制台`)
      return
    }

    showCustomToken.value = false
    newTokenName.value = ''
    customTokenInput.value = ''
  } catch (err) {
    const message = err instanceof Error ? err.message : '未知错误'
    console.error('[Token] Failed to save token:', err)
    ElMessage.error('保存 Token 失败: ' + message)
  } finally {
    tokenLoading.value = false
  }
}

/**
 * 加载 Token 配置选项
 */
async function loadTokenOptions() {
  tokenLoading.value = true
  try {
    const configTypes: ConfigType[] = ['Git', 'Repository', 'Integration']
    const allConfigs: ConfigResponse[] = []
    const failedTypes: string[] = []

    for (const configType of configTypes) {
      try {
        const configs = await fetchConfigs(configType)
        console.log(`[Token] Fetched ${configs.length} configs of type ${configType}`, configs)
        allConfigs.push(...configs)
      } catch (err) {
        const errorMsg = err instanceof Error ? err.message : String(err)
        console.warn(`[Token] Failed to fetch ${configType} configs:`, errorMsg)
        failedTypes.push(configType)
      }
    }

    if (failedTypes.length > 0) {
      console.warn(`[Token] Failed to fetch from config types: ${failedTypes.join(', ')}`)
    }

    // 从配置中提取 Token
    const options: TokenOption[] = []
    const seenTokens = new Set<string>()
    const failedExtractions: { configKey: string; configType: ConfigType; reason: string }[] = []

    for (const config of allConfigs) {
      const token = findTokenFromConfigValue(config.configValue)
      console.log(`[Token] Extraction for ${config.configKey} (${config.configType}): token=${token ? '***' : 'empty'}, value=${config.configValue.substring(0, 50)}...`)
      
      if (token && !seenTokens.has(token)) {
        seenTokens.add(token)
        options.push({
          configKey: config.configKey,
          configType: config.configType,
          token,
          label: `${config.configKey} (${config.configType})`,
          description: config.description || `存储于 ${config.configType}`,
        })
      } else if (!token) {
        failedExtractions.push({
          configKey: config.configKey,
          configType: config.configType,
          reason: 'Token extraction failed',
        })
      }
    }

    if (failedExtractions.length > 0) {
      console.warn(`[Token] Failed to extract tokens:`, failedExtractions)
    }

    tokenOptions.value = options
    console.log(`[Token] Total token options loaded: ${options.length}`)

    // 如果有 Token 且未选中任何项，自动选中第一个
    if (options.length > 0 && !selectedTokenKey.value && !showCustomToken.value) {
      const firstOption = options[0]
      if (firstOption) {
        selectedTokenKey.value = firstOption.configKey
        addForm.token = firstOption.token
        tokenHint.value = `已自动选择: ${firstOption.label}`
      }
    }
  } catch (err) {
    const errorMsg = err instanceof Error ? err.message : String(err)
    console.error('[Token] Critical error loading token options:', errorMsg)
    tokenHint.value = ''
  } finally {
    tokenLoading.value = false
  }
}

/**
 * 获取表格行样式类名
 */
function rowClassName({ row }: { row: any }) {
  return row.id === repoGlobal.selectedRepositoryId ? 'is-selected-repo' : ''
}

function openAddDialog() {
  addDialogVisible.value = true
  // 打开对话框时加载 Token 选项
  loadTokenOptions()
}

function resetAddForm() {
  addForm.name = ''
  addForm.url = ''
  addForm.token = ''
  addForm.provider = GitProvider.GitHub
  addForm.branch = 'main'
  selectedTokenKey.value = ''
  showCustomToken.value = false
  customTokenInput.value = ''
  newTokenName.value = ''
  addFormRef.value?.clearValidate()
}

function handleDialogClosed() {
  resetAddForm()
}

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
    Object.values(input as Record<string, unknown>).forEach((item) => collectStringValues(item, collector))
  }
}

function findTokenFromConfigValue(rawValue: string): string {
  const direct = mayBeGitHubToken(rawValue)
  if (direct) {
    return direct
  }

  try {
    const parsed = JSON.parse(rawValue)
    const values: string[] = []
    collectStringValues(parsed, values)
    return values.map(mayBeGitHubToken).find(Boolean) || ''
  } catch {
    return ''
  }
}

async function loadTokenFromConfigCenter() {
  try {
    await Promise.all([
      systemConfigStore.loadConfigs('Repository'),
      systemConfigStore.loadConfigs('Git'),
      systemConfigStore.loadConfigs('Integration'),
    ])

    const mergedConfigs = [
      ...systemConfigStore.getConfigs('Repository'),
      ...systemConfigStore.getConfigs('Git'),
      ...systemConfigStore.getConfigs('Integration'),
    ]

    for (const config of mergedConfigs) {
      const token = findTokenFromConfigValue(config.configValue)
      if (token) {
        addForm.token = token
        tokenHint.value = '已自动读取配置中的 Git Token'
        return
      }
    }
  } catch {
    tokenHint.value = ''
  }
}

function applyRemoteRepo(fullName: string) {
  const selected = remoteRepos.value.find((item) => item.fullName === fullName)
  if (!selected) {
    return
  }
  addForm.name = selected.name
  addForm.url = selected.htmlUrl
  addForm.branch = selected.defaultBranch || 'main'
}

function handleRemoteRepoChange(value: string | undefined) {
  applyRemoteRepo(value || '')
}

async function fetchRemoteRepos() {
  const token = finalToken.value
  if (!token) {
    remoteRepoError.value = '请先选择或输入 Git Token'
    return
  }

  remoteRepoLoading.value = true
  remoteRepoError.value = ''
  try {
    const repos = await fetchGitHubRepositoriesByToken(token)
    remoteRepos.value = repos
    if (!repos.length) {
      ElMessage.info('未获取到可访问仓库')
      return
    }
    const firstRepo = repos[0]
    if (!firstRepo) {
      ElMessage.info('未获取到可访问仓库')
      return
    }
    selectedRemoteRepo.value = firstRepo.fullName
    applyRemoteRepo(selectedRemoteRepo.value)
    ElMessage.success(`已拉取 ${repos.length} 个仓库`)
  } catch (err) {
    remoteRepoError.value = err instanceof Error ? err.message : '拉取仓库失败'
    ElMessage.error(remoteRepoError.value)
  } finally {
    remoteRepoLoading.value = false
  }
}

async function submitAddRepository() {
  const form = addFormRef.value
  if (!form) return
  await form.validate()

  const token = finalToken.value
  if (!token) {
    ElMessage.error('请选择或输入 Git Token')
    return
  }

  submitting.value = true
  try {
    await store.submitCreate({
      name: addForm.name.trim(),
      url: addForm.url.trim(),
      token: token,
      provider: addForm.provider,
      authType: AuthenticationType.Token,
      mergeStrategy: MergeStrategy.Squash,
      branch: addForm.branch.trim() || 'main',
    })
    ElMessage.success('仓库添加成功')
    addDialogVisible.value = false
  } catch (err) {
    const message = err instanceof Error ? err.message : '仓库添加失败'
    ElMessage.error(message)
  } finally {
    submitting.value = false
  }
}

onMounted(async () => {
  if (!hasRepositories.value) {
    await store.loadRepositories()
  }
  await loadTokenFromConfigCenter()
})
</script>

<template>
  <section class="repo-list">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>仓库列表</span>
          <el-button type="primary" @click="openAddDialog">+ 添加仓库</el-button>
        </div>
      </template>

      <el-skeleton v-if="loading" :rows="6" animated />

      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />

      <el-empty v-else-if="!hasRepositories" description="暂无仓库数据" />

    <el-table v-else :data="repositories" stripe :row-class-name="rowClassName">
        <el-table-column prop="name" label="仓库名称" min-width="220" />
        <el-table-column prop="url" label="仓库地址" min-width="360" show-overflow-tooltip />
        <el-table-column prop="provider" label="代码源" width="130">
          <template #default="{ row }">{{ formatProvider(Number(row.provider)) }}</template>
        </el-table-column>
        <el-table-column prop="authType" label="鉴权" width="150">
          <template #default="{ row }">{{ formatAuthType(Number(row.authType)) }}</template>
        </el-table-column>
        <el-table-column prop="mergeStrategy" label="合并策略" width="140">
          <template #default="{ row }">{{ formatMergeStrategy(Number(row.mergeStrategy)) }}</template>
        </el-table-column>
        <el-table-column prop="branch" label="默认分支" width="120" />
        <el-table-column prop="lastUpdate" label="最后更新" width="180" />
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <el-button link type="primary" size="small" @click="navigateToBranches(row.id)">查看分支</el-button>
            <el-button link type="primary" size="small" @click="repoGlobal.selectRepository(row.id)">设为当前仓库</el-button>
            <el-button link type="danger" size="small">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog
      v-model="addDialogVisible"
      title="添加仓库"
      width="640px"
      :close-on-click-modal="false"
      @closed="handleDialogClosed"
    >
      <el-form ref="addFormRef" :model="addForm" :rules="addFormRules" label-width="100px" status-icon>
        <el-form-item label="仓库名称" prop="name">
          <el-input v-model="addForm.name" placeholder="例如：AutoCodeForge" />
        </el-form-item>

        <el-form-item label="仓库地址" prop="url">
          <el-input v-model="addForm.url" placeholder="例如：https://github.com/owner/repo" />
        </el-form-item>

        <el-form-item label="代码源类型">
          <el-select v-model="addForm.provider" style="width: 100%" placeholder="请选择代码源">
            <el-option
              v-for="item in providerOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="默认分支">
          <el-input v-model="addForm.branch" placeholder="例如：main / master / develop" />
        </el-form-item>

        <!-- Token 选择器 -->
        <el-form-item label="访问令牌">
          <div class="token-selector">
            <el-select
              v-model="selectedTokenKey"
              placeholder="从配置中选择 Token"
              filterable
              clearable
              style="width: 100%"
              :loading="tokenLoading"
              @change="handleTokenSelect"
            >
              <el-option
                v-for="item in tokenOptions"
                :key="item.configKey"
                :label="item.label"
                :value="item.configKey"
              >
                <div class="token-option">
                  <span class="token-label">{{ item.label }}</span>
                  <span class="token-desc">{{ item.description }}</span>
                </div>
              </el-option>
              <el-option value="__custom__" label="+ 使用自定义 Token">
                <span style="color: #409eff">+ 使用自定义 Token</span>
              </el-option>
            </el-select>

            <!-- 自定义 Token 输入区域 -->
            <div v-if="showCustomToken" class="custom-token-area">
              <el-divider content-position="left">自定义 Token</el-divider>
              <el-input
                v-model="customTokenInput"
                type="password"
                show-password
                placeholder="输入新的 GitHub Token"
                style="margin-bottom: 12px"
              />
              <el-input
                v-model="newTokenName"
                placeholder="为此 Token 起个名称（可选）"
                style="margin-bottom: 12px"
              />
              <div class="custom-token-actions">
                <el-button
                  type="primary"
                  size="small"
                  :loading="tokenLoading"
                  @click="saveTokenToConfig"
                >
                  保存到配置
                </el-button>
                <el-button size="small" @click="showCustomToken = false">取消</el-button>
              </div>
            </div>
          </div>
        </el-form-item>

        <el-form-item label="远程仓库">
          <el-space wrap>
            <el-button :loading="remoteRepoLoading" @click="fetchRemoteRepos">拉取我的 GitHub 仓库</el-button>
            <el-select
              v-model="selectedRemoteRepo"
              filterable
              clearable
              placeholder="选择仓库后自动填充"
              style="min-width: 300px"
              @change="handleRemoteRepoChange"
            >
              <el-option
                v-for="item in remoteRepos"
                :key="item.id"
                :label="item.fullName"
                :value="item.fullName"
              />
            </el-select>
          </el-space>
        </el-form-item>
      </el-form>

      <el-alert
        v-if="tokenHint"
        :title="tokenHint"
        type="success"
        show-icon
        :closable="false"
      />

      <el-alert
        v-if="remoteRepoError"
        :title="remoteRepoError"
        type="error"
        show-icon
        :closable="false"
      />

      <template #footer>
        <el-space>
          <el-button @click="addDialogVisible = false">取消</el-button>
          <el-button type="primary" :loading="submitting" @click="submitAddRepository">确认添加</el-button>
        </el-space>
      </template>
    </el-dialog>
  </section>
</template>

<style scoped>
.repo-list {
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

.is-selected-repo {
  background: rgba(59, 130, 246, 0.06) !important;
}

/* Token 选择器样式 */
.token-selector {
  width: 100%;
}

.token-option {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: 4px 0;
}

.token-label {
  font-weight: 500;
}

.token-desc {
  font-size: 12px;
  color: #909399;
}

.custom-token-area {
  margin-top: 12px;
  padding: 12px;
  background: #f5f7fa;
  border-radius: 4px;
}

.custom-token-actions {
  display: flex;
  gap: 8px;
}
</style>
