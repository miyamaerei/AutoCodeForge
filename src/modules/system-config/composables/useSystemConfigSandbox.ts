import { computed, reactive, ref } from 'vue'

type SandboxScenarioId = 'balanced' | 'strict' | 'debug'
type SandboxExecutionMode = 'dry-run' | 'sandbox-live'
type SandboxApprovalMode = 'strict' | 'manual' | 'off'
type SandboxRiskLevel = 'low' | 'medium' | 'high'

interface SandboxOptionSectionModel {
  key: string
  title: string
  description: string
}

interface SandboxScenarioModel {
  id: SandboxScenarioId
  name: string
  description: string
}

interface SandboxFormModel {
  profileName: string
  workspaceRootPath: string
  artifactOutputPath: string
  allowedWritePaths: string
  ignoredPaths: string
  executionMode: SandboxExecutionMode
  approvalMode: SandboxApprovalMode
  maxParallelTasks: number
  commandTimeoutSec: number
  allowWriteOps: boolean
  allowNetworkAccess: boolean
  storeTerminalLogs: boolean
  maskSecretsInLogs: boolean
  defaultModel: string
  fallbackModel: string
  promptGuardrail: string
}

interface SandboxStorageModel {
  form: SandboxFormModel
  selectedScenario: SandboxScenarioId
  lastSavedAt: string
}

const defaultForm = (): SandboxFormModel => ({
  profileName: 'default-sandbox',
  workspaceRootPath: 'C:/gitrepos/AutoCodeForge',
  artifactOutputPath: 'C:/gitrepos/AutoCodeForge/.sandbox-artifacts',
  allowedWritePaths: 'src/**\ndocs/**',
  ignoredPaths: 'node_modules/**\ndist/**\n.git/**',
  executionMode: 'dry-run',
  approvalMode: 'strict',
  maxParallelTasks: 3,
  commandTimeoutSec: 300,
  allowWriteOps: false,
  allowNetworkAccess: false,
  storeTerminalLogs: true,
  maskSecretsInLogs: true,
  defaultModel: 'gpt-5.3-codex',
  fallbackModel: 'gpt-4.1',
  promptGuardrail: '先分析风险，再执行最小改动。',
})

const cloneForm = (value: SandboxFormModel): SandboxFormModel => ({
  ...value,
})

export function useSystemConfigSandbox() {
  const storageKey = 'system-config.sandbox.v1'

  const loading = ref(false)
  const error = ref('')
  const saving = ref(false)
  const saveError = ref('')
  const saveSuccess = ref(false)
  const lastSavedAt = ref('')
  const selectedScenario = ref<SandboxScenarioId>('balanced')
  const optionSections = ref<SandboxOptionSectionModel[]>([])

  const form = reactive<SandboxFormModel>(defaultForm())
  const initialSnapshot = ref(JSON.stringify(form))

  const scenarios: SandboxScenarioModel[] = [
    {
      id: 'balanced',
      name: 'Balanced',
      description: '默认推荐，兼顾执行效率与安全约束。',
    },
    {
      id: 'strict',
      name: 'Strict Guardrail',
      description: '更高安全要求，禁止写操作和网络访问。',
    },
    {
      id: 'debug',
      name: 'Debug Assist',
      description: '适合定位问题，可放宽审批策略以提升排障速度。',
    },
  ]

  const hasData = computed(() => optionSections.value.length > 0)
  const isDirty = computed(() => JSON.stringify(form) !== initialSnapshot.value)

  const riskLevel = computed<SandboxRiskLevel>(() => {
    if (form.allowWriteOps && form.allowNetworkAccess) {
      return 'high'
    }
    if (form.allowWriteOps || form.allowNetworkAccess || form.approvalMode === 'off') {
      return 'medium'
    }
    return 'low'
  })

  const riskLabel = computed(() => {
    if (riskLevel.value === 'high') {
      return '高风险'
    }
    if (riskLevel.value === 'medium') {
      return '中风险'
    }
    return '低风险'
  })

  const riskTagType = computed(() => {
    if (riskLevel.value === 'high') {
      return 'danger'
    }
    if (riskLevel.value === 'medium') {
      return 'warning'
    }
    return 'success'
  })

  const applyScenario = (scenarioId: SandboxScenarioId) => {
    selectedScenario.value = scenarioId

    if (scenarioId === 'strict') {
      Object.assign(form, {
        executionMode: 'dry-run',
        approvalMode: 'strict',
        maxParallelTasks: 2,
        commandTimeoutSec: 180,
        allowWriteOps: false,
        allowNetworkAccess: false,
        allowedWritePaths: 'src/**\ndocs/**',
        ignoredPaths: 'node_modules/**\ndist/**\n.git/**\n.env*',
        storeTerminalLogs: true,
        maskSecretsInLogs: true,
      })
      return
    }

    if (scenarioId === 'debug') {
      Object.assign(form, {
        executionMode: 'sandbox-live',
        approvalMode: 'manual',
        maxParallelTasks: 5,
        commandTimeoutSec: 480,
        allowWriteOps: true,
        allowNetworkAccess: true,
        allowedWritePaths: 'src/**\ntests/**\ndocs/**',
        ignoredPaths: 'node_modules/**\ndist/**\n.git/**',
        storeTerminalLogs: true,
        maskSecretsInLogs: true,
      })
      return
    }

    Object.assign(form, {
      executionMode: 'dry-run',
      approvalMode: 'strict',
      maxParallelTasks: 3,
      commandTimeoutSec: 300,
      allowWriteOps: false,
      allowNetworkAccess: false,
      allowedWritePaths: 'src/**\ndocs/**',
      ignoredPaths: 'node_modules/**\ndist/**\n.git/**',
      storeTerminalLogs: true,
      maskSecretsInLogs: true,
    })
  }

  const persistConfig = () => {
    const payload: SandboxStorageModel = {
      form: cloneForm(form),
      selectedScenario: selectedScenario.value,
      lastSavedAt: lastSavedAt.value,
    }
    localStorage.setItem(storageKey, JSON.stringify(payload))
    initialSnapshot.value = JSON.stringify(form)
  }

  const restoreConfig = () => {
    const raw = localStorage.getItem(storageKey)
    if (!raw) {
      return
    }

    const parsed = JSON.parse(raw) as Partial<SandboxStorageModel>
    if (parsed.form) {
      Object.assign(form, parsed.form)
    }
    if (parsed.selectedScenario) {
      selectedScenario.value = parsed.selectedScenario
    }
    lastSavedAt.value = parsed.lastSavedAt || ''
    initialSnapshot.value = JSON.stringify(form)
  }

  const loadConfig = async () => {
    loading.value = true
    error.value = ''
    saveSuccess.value = false
    try {
      await new Promise((resolve) => setTimeout(resolve, 180))
      optionSections.value = [
        {
          key: 'execution',
          title: '执行模式',
          description: '用于控制沙盒任务是否直接执行，或仅进行 dry-run。',
        },
        {
          key: 'guardrail',
          title: '安全护栏',
          description: '控制写操作、网络访问、审批策略和日志脱敏。',
        },
        {
          key: 'resource',
          title: '资源配额',
          description: '限制并发任务数、超时阈值，避免沙盒任务失控。',
        },
        {
          key: 'file-scope',
          title: '文件地址范围',
          description: '设置工作目录、产物目录、允许写入路径和忽略路径。',
        },
        {
          key: 'model-policy',
          title: '模型策略',
          description: '配置默认模型和回退模型，确保请求失败时可降级。',
        },
      ]
      restoreConfig()
    } catch {
      error.value = '沙盒配置加载失败，请稍后重试。'
    } finally {
      loading.value = false
    }
  }

  const saveConfig = async () => {
    saving.value = true
    saveError.value = ''
    saveSuccess.value = false
    try {
      await new Promise((resolve) => setTimeout(resolve, 280))
      lastSavedAt.value = new Date().toLocaleString('zh-CN')
      saveSuccess.value = true
      persistConfig()
    } catch {
      saveError.value = '保存沙盒配置失败，请稍后重试。'
    } finally {
      saving.value = false
    }
  }

  return {
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
  }
}