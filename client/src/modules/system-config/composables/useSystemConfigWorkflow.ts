import { computed, reactive, ref } from 'vue'
import { useSystemConfigStore } from '../store/useSystemConfigStore'
import type { ConfigType } from '../api/config.types'

type WorkflowScenarioId = 'safe' | 'balanced' | 'delivery'
type AgentMode = 'default' | 'explore' | 'mixed'
type OutputStyle = 'concise' | 'standard' | 'detailed'

interface WorkflowOptionSectionModel {
  key: string
  title: string
  description: string
}

interface WorkflowScenarioModel {
  id: WorkflowScenarioId
  name: string
  description: string
}

interface WorkflowStageModel {
  mode: AgentMode
  mustAskClarifying: boolean
  useRepoSearch: boolean
  runValidation: boolean
  notesTemplate: string
}

interface WorkflowFormModel {
  profileName: string
  commandTimeoutSec: number
  maxParallelTasks: number
  requireApprovalForWrite: boolean
  autoCreateTodo: boolean
  outputStyle: OutputStyle
  initialization: WorkflowStageModel & {
    preloadInstructionFiles: boolean
    preloadRepoMemory: boolean
    generateChecklist: boolean
  }
  question: WorkflowStageModel & {
    alwaysCiteFiles: boolean
    includeAlternatives: boolean
  }
  bugfix: WorkflowStageModel & {
    requireReproSteps: boolean
    maxFixAttempts: number
    autoRunUnitTests: boolean
  }
  feature: WorkflowStageModel & {
    requireAcceptanceCriteria: boolean
    createImplementationPlan: boolean
    includeRollbackPlan: boolean
  }
}

interface WorkflowStorageModel {
  form: WorkflowFormModel
  selectedScenario: WorkflowScenarioId
  lastSavedAt: string
}

const defaultStage = (): WorkflowStageModel => ({
  mode: 'mixed',
  mustAskClarifying: false,
  useRepoSearch: true,
  runValidation: true,
  notesTemplate: '',
})

const defaultForm = (): WorkflowFormModel => ({
  profileName: 'team-default',
  commandTimeoutSec: 300,
  maxParallelTasks: 3,
  requireApprovalForWrite: true,
  autoCreateTodo: true,
  outputStyle: 'standard',
  initialization: {
    ...defaultStage(),
    mode: 'mixed',
    preloadInstructionFiles: true,
    preloadRepoMemory: true,
    generateChecklist: true,
    notesTemplate: '初始化时先加载模块指令、技能，再产出执行清单。',
  },
  question: {
    ...defaultStage(),
    mode: 'default',
    alwaysCiteFiles: true,
    includeAlternatives: true,
    notesTemplate: '先澄清目标和约束，再给出可执行答案。',
  },
  bugfix: {
    ...defaultStage(),
    mode: 'mixed',
    requireReproSteps: true,
    maxFixAttempts: 3,
    autoRunUnitTests: true,
    notesTemplate: '先复现，再最小改动修复并验证。',
  },
  feature: {
    ...defaultStage(),
    mode: 'explore',
    requireAcceptanceCriteria: true,
    createImplementationPlan: true,
    includeRollbackPlan: true,
    notesTemplate: '拆解需求，分步实施，补充风险与回滚方案。',
  },
})

const cloneForm = (value: WorkflowFormModel): WorkflowFormModel => ({
  ...value,
  initialization: { ...value.initialization },
  question: { ...value.question },
  bugfix: { ...value.bugfix },
  feature: { ...value.feature },
})

export function useSystemConfigWorkflow() {
  const store = useSystemConfigStore()
  const storageKey = 'system-config.workflow.v1'

  const loading = ref(false)
  const error = ref('')
  const saving = ref(false)
  const saveError = ref('')
  const saveSuccess = ref(false)
  const lastSavedAt = ref('')
  const selectedScenario = ref<WorkflowScenarioId>('balanced')
  const optionSections = ref<WorkflowOptionSectionModel[]>([])

  const form = reactive<WorkflowFormModel>(defaultForm())
  const initialSnapshot = ref(JSON.stringify(form))

  const scenarios: WorkflowScenarioModel[] = [
    {
      id: 'safe',
      name: 'Safe Guardrail',
      description: '适合高风险仓库，强调澄清、审批与验证。',
    },
    {
      id: 'balanced',
      name: 'Balanced Team',
      description: '平衡速度与稳定性，默认推荐给多数团队。',
    },
    {
      id: 'delivery',
      name: 'Delivery First',
      description: '强调交付效率，减少不必要阻塞。',
    },
  ]

  const hasData = computed(() => optionSections.value.length > 0)
  const isDirty = computed(() => JSON.stringify(form) !== initialSnapshot.value)

  const automationScore = computed(() => {
    let score = 0
    if (!form.requireApprovalForWrite) {
      score += 25
    }
    if (form.bugfix.autoRunUnitTests) {
      score += 15
    }
    if (form.feature.createImplementationPlan) {
      score += 15
    }
    if (!form.initialization.mustAskClarifying) {
      score += 10
    }
    if (!form.question.mustAskClarifying) {
      score += 10
    }
    if (form.maxParallelTasks >= 4) {
      score += 25
    }
    return Math.min(score, 100)
  })

  const automationLabel = computed(() => {
    if (automationScore.value >= 75) {
      return '高自动化'
    }
    if (automationScore.value >= 45) {
      return '平衡自动化'
    }
    return '审慎自动化'
  })

  const automationTagType = computed(() => {
    if (automationScore.value >= 75) {
      return 'warning'
    }
    if (automationScore.value >= 45) {
      return 'success'
    }
    return 'info'
  })

  const applyScenario = (scenarioId: WorkflowScenarioId) => {
    selectedScenario.value = scenarioId

    if (scenarioId === 'safe') {
      Object.assign(form, {
        requireApprovalForWrite: true,
        maxParallelTasks: 2,
        commandTimeoutSec: 240,
        outputStyle: 'detailed',
      })
      Object.assign(form.initialization, {
        mode: 'default',
        mustAskClarifying: true,
        useRepoSearch: true,
        runValidation: true,
      })
      Object.assign(form.question, {
        mode: 'default',
        mustAskClarifying: true,
        useRepoSearch: true,
        runValidation: false,
      })
      Object.assign(form.bugfix, {
        mode: 'mixed',
        mustAskClarifying: true,
        useRepoSearch: true,
        runValidation: true,
        maxFixAttempts: 2,
        autoRunUnitTests: true,
      })
      Object.assign(form.feature, {
        mode: 'mixed',
        mustAskClarifying: true,
        useRepoSearch: true,
        runValidation: true,
        createImplementationPlan: true,
        includeRollbackPlan: true,
      })
      return
    }

    if (scenarioId === 'delivery') {
      Object.assign(form, {
        requireApprovalForWrite: false,
        maxParallelTasks: 5,
        commandTimeoutSec: 420,
        outputStyle: 'concise',
      })
      Object.assign(form.initialization, {
        mode: 'mixed',
        mustAskClarifying: false,
        useRepoSearch: true,
        runValidation: false,
      })
      Object.assign(form.question, {
        mode: 'explore',
        mustAskClarifying: false,
        useRepoSearch: true,
        runValidation: false,
      })
      Object.assign(form.bugfix, {
        mode: 'mixed',
        mustAskClarifying: false,
        useRepoSearch: true,
        runValidation: true,
        maxFixAttempts: 3,
        autoRunUnitTests: true,
      })
      Object.assign(form.feature, {
        mode: 'explore',
        mustAskClarifying: false,
        useRepoSearch: true,
        runValidation: true,
        createImplementationPlan: true,
        includeRollbackPlan: false,
      })
      return
    }

    Object.assign(form, {
      requireApprovalForWrite: true,
      maxParallelTasks: 3,
      commandTimeoutSec: 300,
      outputStyle: 'standard',
    })
    Object.assign(form.initialization, {
      mode: 'mixed',
      mustAskClarifying: false,
      useRepoSearch: true,
      runValidation: true,
    })
    Object.assign(form.question, {
      mode: 'default',
      mustAskClarifying: false,
      useRepoSearch: true,
      runValidation: false,
    })
    Object.assign(form.bugfix, {
      mode: 'mixed',
      mustAskClarifying: true,
      useRepoSearch: true,
      runValidation: true,
      maxFixAttempts: 3,
      autoRunUnitTests: true,
    })
    Object.assign(form.feature, {
      mode: 'explore',
      mustAskClarifying: true,
      useRepoSearch: true,
      runValidation: true,
      createImplementationPlan: true,
      includeRollbackPlan: true,
    })
  }

  const persistConfig = () => {
    const payload: WorkflowStorageModel = {
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

    const parsed = JSON.parse(raw) as Partial<WorkflowStorageModel>
    if (parsed.form) {
      Object.assign(form, parsed.form)
      Object.assign(form.initialization, parsed.form.initialization)
      Object.assign(form.question, parsed.form.question)
      Object.assign(form.bugfix, parsed.form.bugfix)
      Object.assign(form.feature, parsed.form.feature)
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
          key: 'initialization',
          title: '初始化流程',
          description: '定义启动阶段如何加载指令、记忆和执行清单。',
        },
        {
          key: 'question',
          title: '提问流程',
          description: '决定问答时是否先澄清、是否使用仓库检索。',
        },
        {
          key: 'bugfix',
          title: '修复流程',
          description: '约束复现、修复尝试次数以及验证策略。',
        },
        {
          key: 'feature',
          title: '新需求流程',
          description: '规范需求拆解、计划生成和回滚方案。',
        },
      ]
      restoreConfig()
    } catch {
      error.value = '流程配置加载失败，请稍后重试。'
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
      if (form.maxParallelTasks < 1 || form.maxParallelTasks > 8) {
        throw new Error('并发任务数需要在 1 到 8 之间。')
      }
      if (form.bugfix.maxFixAttempts < 1 || form.bugfix.maxFixAttempts > 5) {
        throw new Error('修复尝试次数需要在 1 到 5 之间。')
      }
      if (!form.profileName.trim()) {
        throw new Error('请填写流程配置名称。')
      }
      lastSavedAt.value = new Date().toLocaleString('zh-CN')
      saveSuccess.value = true
      persistConfig()

      // 保存到后端 store
      try {
        await store.saveConfig('Workflow' as ConfigType, {
          configKey: 'workflow-settings',
          configValue: JSON.stringify(form),
          isEncrypted: false,
          description: '工作流配置',
        })
      } catch (backendErr) {
        console.warn('Failed to save to backend, localStorage preserved:', backendErr)
      }
    } catch (e) {
      const message = e instanceof Error ? e.message : '保存流程配置失败，请稍后重试。'
      saveError.value = message
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
    automationScore,
    automationLabel,
    automationTagType,
    loadConfig,
    saveConfig,
    applyScenario,
  }
}
