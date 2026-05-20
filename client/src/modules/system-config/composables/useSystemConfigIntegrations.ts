import { computed, reactive, ref } from 'vue'
import { useSystemConfigStore } from '../store/useSystemConfigStore'
import type { ConfigType } from '../api/config.types'

export type IntegrationAuthState = 'unauthenticated' | 'pending' | 'connected'
type InstallChannel = 'winget' | 'npm'
type CopilotAuthMode = 'interactive' | 'pat'
type AzureAuthMode = 'entra' | 'azure-devops-pat'
type IntegrationStatus = 'not-configured' | 'configured' | 'beta'
type IntegrationKind = 'azure' | 'copilot' | 'generic'

export type IntegrationId = 'azure-pwt' | 'github-copilot' | 'gitlab'

type GenericIntegrationId = Exclude<IntegrationId, 'azure-pwt' | 'github-copilot'>

export interface IntegrationItemModel {
  id: IntegrationId
  name: string
  description: string
  enabled: boolean
  status: IntegrationStatus
  kind: IntegrationKind
  tags: string[]
}

interface AzurePwtFormModel {
  authMode: AzureAuthMode
  tenantId: string
  subscriptionId: string
  projectName: string
  commandHint: string
  devOpsOrganizationUrl: string
  patScopeHint: string
  patRotationDays: number
  patSecretRef: string
}

interface GithubCopilotFormModel {
  organization: string
  executable: string
  installChannel: InstallChannel
  authMode: CopilotAuthMode
  patEnvVar: 'GH_TOKEN' | 'GITHUB_TOKEN'
  workspacePolicy: string
  authState: IntegrationAuthState
}

interface GenericIntegrationFormModel {
  endpoint: string
  workspace: string
  authType: 'oauth' | 'token' | 'app'
  credentialHint: string
}

type GenericIntegrationFormsModel = Record<GenericIntegrationId, GenericIntegrationFormModel>

interface CopilotCliStatusModel {
  checking: boolean
  installed: boolean
  version: string
  checkOutput: string
  lastCheckedAt: string
}

export interface IntegrationConfigFormModel {
  azurePwt: AzurePwtFormModel
  githubCopilot: GithubCopilotFormModel
  generic: GenericIntegrationFormsModel
}

export function useSystemConfigIntegrations() {
  const store = useSystemConfigStore()
  const storageKey = 'system-config.integrations.v1'

  const loading = ref(false)
  const error = ref('')
  const saving = ref(false)
  const saveError = ref('')
  const saveSuccess = ref(false)
  const lastSavedAt = ref('')
  const lastAuthCommand = ref('')
  const authHint = ref('')
  const searchKeyword = ref('')
  const selectedIntegrationId = ref<IntegrationId>('azure-pwt')

  const integrations = ref<IntegrationItemModel[]>([])
  const cliStatus = reactive<CopilotCliStatusModel>({
    checking: false,
    installed: false,
    version: '',
    checkOutput: '',
    lastCheckedAt: '',
  })

  const form = reactive<IntegrationConfigFormModel>({
    azurePwt: {
      authMode: 'entra',
      tenantId: '',
      subscriptionId: '',
      projectName: 'autocodeforge-core',
      commandHint: 'az login',
      devOpsOrganizationUrl: 'https://dev.azure.com/your-organization',
      patScopeHint: '最小权限原则：按场景拆分 PAT，默认仅授予 Read。',
      patRotationDays: 30,
      patSecretRef: 'AZDO_PAT',
    },
    githubCopilot: {
      organization: '',
      executable: 'copilot',
      installChannel: 'winget',
      authMode: 'interactive',
      patEnvVar: 'GH_TOKEN',
      workspacePolicy: 'required',
      authState: 'unauthenticated',
    },
    generic: {
      gitlab: {
        endpoint: 'https://gitlab.com/api/v4',
        workspace: '',
        authType: 'token',
        credentialHint: 'glpat-***',
      },
    },
  })

  const hasData = computed(() => integrations.value.length > 0)

  const filteredIntegrations = computed(() => {
    const keyword = searchKeyword.value.trim().toLowerCase()
    if (!keyword) {
      return integrations.value
    }
    return integrations.value.filter((item) => {
      const haystack = `${item.name} ${item.description} ${item.tags.join(' ')}`.toLowerCase()
      return haystack.includes(keyword)
    })
  })

  const selectedIntegration = computed(() =>
    integrations.value.find((item) => item.id === selectedIntegrationId.value) || null,
  )

  const enabledCount = computed(() => integrations.value.filter((item) => item.enabled).length)
  const configuredCount = computed(
    () => integrations.value.filter((item) => item.status === 'configured').length,
  )

  const isGenericIntegration = (id: IntegrationId): id is GenericIntegrationId => {
    return id !== 'azure-pwt' && id !== 'github-copilot'
  }

  const getGenericForm = (id: IntegrationId): GenericIntegrationFormModel | null => {
    if (!isGenericIntegration(id)) {
      return null
    }
    return form.generic[id]
  }

  const installCommand = computed(() =>
    form.githubCopilot.installChannel === 'npm'
      ? 'npm install -g @github/copilot'
      : 'winget install GitHub.Copilot',
  )

  const verifyCommand = computed(() => `${form.githubCopilot.executable.trim() || 'copilot'} --version`)

  const launchCommand = computed(() => form.githubCopilot.executable.trim() || 'copilot')

  const loginGuide = computed(() => {
    if (form.githubCopilot.authMode === 'pat') {
      return `设置 ${form.githubCopilot.patEnvVar} 后执行 ${launchCommand.value}`
    }
    return `执行 ${launchCommand.value} 后输入 /login 完成认证`
  })

  const azurePatGuide = computed(() => {
    if (form.azurePwt.authMode !== 'azure-devops-pat') {
      return '推荐使用 Microsoft Entra（Service Principal / Managed Identity）访问 Azure 资源。'
    }

    return [
      'Azure DevOps PAT 仅建议用于 DevOps REST API、Git 或临时自动化，不建议长期用于生产流程。',
      'PAT 应最小权限、短有效期并定期轮换；不要写入仓库，建议存放在 Key Vault 或 CI Secret。',
      '如需非用户绑定或长期稳定认证，优先改用 Entra Token、Service Principal 或 Managed Identity。',
    ].join(' ')
  })

  const hostBridgeAvailable = computed(() => {
    return typeof window.__AUTOCODEFORGE_HOST__?.runTerminalCommand === 'function'
  })

  const runHostCommand = async (command: string): Promise<HostCommandResult> => {
    if (!window.__AUTOCODEFORGE_HOST__?.runTerminalCommand) {
      return {
        ok: false,
        error: '当前运行环境未注入终端桥接，请复制命令到终端执行。',
      }
    }
    return window.__AUTOCODEFORGE_HOST__.runTerminalCommand(command)
  }

  const persistConfig = () => {
    const payload = {
      form: {
        azurePwt: { ...form.azurePwt },
        githubCopilot: { ...form.githubCopilot },
        generic: { ...form.generic },
      },
      cliStatus: { ...cliStatus },
      lastSavedAt: lastSavedAt.value,
      lastAuthCommand: lastAuthCommand.value,
      authHint: authHint.value,
      selectedIntegrationId: selectedIntegrationId.value,
    }
    localStorage.setItem(storageKey, JSON.stringify(payload))
  }

  const restoreConfig = () => {
    const raw = localStorage.getItem(storageKey)
    if (!raw) {
      return
    }

    const parsed = JSON.parse(raw) as {
      form?: IntegrationConfigFormModel
      cliStatus?: CopilotCliStatusModel
      lastSavedAt?: string
      lastAuthCommand?: string
      authHint?: string
      selectedIntegrationId?: IntegrationId
    }

    if (parsed.form?.azurePwt) {
      Object.assign(form.azurePwt, parsed.form.azurePwt)
    }
    if (parsed.form?.githubCopilot) {
      Object.assign(form.githubCopilot, parsed.form.githubCopilot)
    }
    if (parsed.form?.generic) {
      Object.assign(form.generic, parsed.form.generic)
    }
    if (parsed.cliStatus) {
      Object.assign(cliStatus, parsed.cliStatus)
    }
    lastSavedAt.value = parsed.lastSavedAt || ''
    lastAuthCommand.value = parsed.lastAuthCommand || ''
    authHint.value = parsed.authHint || ''
    if (parsed.selectedIntegrationId) {
      selectedIntegrationId.value = parsed.selectedIntegrationId
    }
  }

  const loadConfigs = async () => {
    loading.value = true
    error.value = ''
    try {
      await new Promise((resolve) => setTimeout(resolve, 220))
      integrations.value = [
        {
          id: 'azure-pwt',
          name: 'Azure PWT',
          description: '配置 Azure 项目和命令提示，供任务执行链路使用。',
          enabled: true,
          status: 'configured',
          kind: 'azure',
          tags: ['azure', 'cloud', 'pipeline'],
        },
        {
          id: 'github-copilot',
          name: 'GitHub Copilot CLI',
          description: '配置 copilot CLI 安装、认证和策略。',
          enabled: true,
          status: 'configured',
          kind: 'copilot',
          tags: ['copilot', 'agent', 'cli'],
        },
        {
          id: 'gitlab',
          name: 'GitLab',
          description: '对接 GitLab 仓库、流水线和 MR 数据。',
          enabled: false,
          status: 'beta',
          kind: 'generic',
          tags: ['scm', 'ci'],
        },
      ]
      restoreConfig()
    } catch {
      error.value = '加载集成配置失败，请稍后重试。'
    } finally {
      loading.value = false
    }
  }

  const saveConfigs = async () => {
    saving.value = true
    saveError.value = ''
    saveSuccess.value = false
    try {
      await new Promise((resolve) => setTimeout(resolve, 280))
      lastSavedAt.value = new Date().toLocaleString('zh-CN')
      saveSuccess.value = true
      persistConfig()

      // 保存到后端 store
      try {
        await store.saveConfig('Integration' as ConfigType, {
          configKey: 'integration-settings',
          configValue: JSON.stringify(form),
          isEncrypted: false,
          description: '集成配置',
        })
      } catch (backendErr) {
        console.warn('Failed to save to backend, localStorage preserved:', backendErr)
      }
    } catch {
      saveError.value = '保存失败，请稍后重试。'
    } finally {
      saving.value = false
    }
  }

  const checkCopilotCliStatus = async () => {
    cliStatus.checking = true
    cliStatus.checkOutput = ''

    try {
      const result = await runHostCommand(verifyCommand.value)
      cliStatus.lastCheckedAt = new Date().toLocaleString('zh-CN')

      if (!result.ok) {
        cliStatus.installed = false
        cliStatus.version = ''
        cliStatus.checkOutput = result.error || '检测失败'
        return
      }

      const output = (result.output || '').trim()
      cliStatus.checkOutput = output
      cliStatus.installed = /copilot cli/i.test(output)
      cliStatus.version = output
    } finally {
      cliStatus.checking = false
    }
  }

  const triggerGithubCopilotAuth = async () => {
    const resolvedCommand = launchCommand.value
    lastAuthCommand.value = resolvedCommand
    form.githubCopilot.authState = 'pending'
    authHint.value = loginGuide.value

    if (form.githubCopilot.authMode === 'interactive') {
      await runHostCommand(resolvedCommand)
    }

    await new Promise((resolve) => setTimeout(resolve, 220))
    form.githubCopilot.authState = 'pending'
    persistConfig()
  }

  const selectIntegration = (id: IntegrationId) => {
    selectedIntegrationId.value = id
    persistConfig()
  }

  const updateGenericConfig = (id: IntegrationId, patch: Partial<GenericIntegrationFormModel>) => {
    const target = getGenericForm(id)
    if (!target) {
      return
    }
    Object.assign(target, patch)
    persistConfig()
  }

  return {
    loading,
    error,
    saving,
    saveError,
    saveSuccess,
    lastSavedAt,
    lastAuthCommand,
    authHint,
    searchKeyword,
    selectedIntegrationId,
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
    updateGenericConfig,
    isGenericIntegration,
    persistConfig,
  }
}
