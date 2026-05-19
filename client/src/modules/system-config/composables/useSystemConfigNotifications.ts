import { computed, reactive, ref } from 'vue'

type DigestMode = 'immediate' | 'hourly' | 'daily'
type FocusMode = 'balanced' | 'quiet' | 'critical-only'

type DeliveryWindow = {
  enabled: boolean
  start: string
  end: string
  timezone: string
}

interface NotificationFormModel {
  projectName: string
  enableInApp: boolean
  enableEmail: boolean
  digestMode: DigestMode
  focusMode: FocusMode
  onlyMentioned: boolean
  notifyOnBuildFailed: boolean
  notifyOnReviewRequested: boolean
  notifyOnDeploymentFinished: boolean
  notifyOnSecurityAlert: boolean
  emailProvider: 'smtp' | 'ses' | 'sendgrid'
  emailFromName: string
  emailFromAddress: string
  smtpHost: string
  smtpPort: number
  smtpSecure: boolean
  smtpUsername: string
  smtpPasswordPlaceholder: string
  deliveryWindow: DeliveryWindow
}

interface NotificationOptionSectionModel {
  key: string
  title: string
  description: string
}

interface NotificationScenarioModel {
  id: FocusMode
  name: string
  description: string
}

interface NotificationStorageModel {
  form: NotificationFormModel
  lastSavedAt: string
  selectedScenario: FocusMode
}

const defaultForm = (): NotificationFormModel => ({
  projectName: 'AutoCodeForge',
  enableInApp: true,
  enableEmail: true,
  digestMode: 'hourly',
  focusMode: 'balanced',
  onlyMentioned: false,
  notifyOnBuildFailed: true,
  notifyOnReviewRequested: true,
  notifyOnDeploymentFinished: false,
  notifyOnSecurityAlert: true,
  emailProvider: 'smtp',
  emailFromName: 'AutoCodeForge Bot',
  emailFromAddress: 'noreply@autocodeforge.local',
  smtpHost: 'smtp.office365.com',
  smtpPort: 587,
  smtpSecure: false,
  smtpUsername: 'noreply@autocodeforge.local',
  smtpPasswordPlaceholder: '******',
  deliveryWindow: {
    enabled: true,
    start: '09:00',
    end: '20:00',
    timezone: 'Asia/Shanghai',
  },
})

const cloneForm = (value: NotificationFormModel): NotificationFormModel => ({
  ...value,
  deliveryWindow: {
    ...value.deliveryWindow,
  },
})

export function useSystemConfigNotifications() {
  const storageKey = 'system-config.notifications.v1'

  const loading = ref(false)
  const error = ref('')
  const saving = ref(false)
  const saveError = ref('')
  const saveSuccess = ref(false)
  const lastSavedAt = ref('')
  const selectedScenario = ref<FocusMode>('balanced')
  const optionSections = ref<NotificationOptionSectionModel[]>([])

  const form = reactive<NotificationFormModel>(defaultForm())
  const initialSnapshot = ref(JSON.stringify(form))

  const scenarios: NotificationScenarioModel[] = [
    {
      id: 'balanced',
      name: 'Balanced',
      description: '推荐模式。关键通知实时，其他通知按小时汇总。',
    },
    {
      id: 'quiet',
      name: 'Quiet Hours First',
      description: '降低噪音。仅保留高价值通知，减少打扰。',
    },
    {
      id: 'critical-only',
      name: 'Critical Only',
      description: '只保留阻塞交付的关键信号。',
    },
  ]

  const hasData = computed(() => optionSections.value.length > 0)
  const isDirty = computed(() => JSON.stringify(form) !== initialSnapshot.value)

  const enabledChannelsCount = computed(() => {
    let total = 0
    if (form.enableInApp) {
      total += 1
    }
    if (form.enableEmail) {
      total += 1
    }
    return total
  })

  const enabledEventsCount = computed(() => {
    const flags = [
      form.notifyOnBuildFailed,
      form.notifyOnReviewRequested,
      form.notifyOnDeploymentFinished,
      form.notifyOnSecurityAlert,
    ]
    return flags.filter(Boolean).length
  })

  const noiseLevelLabel = computed(() => {
    const highNoise = enabledChannelsCount.value >= 3 && enabledEventsCount.value >= 3 && !form.onlyMentioned
    if (highNoise) {
      return '偏高'
    }
    if (enabledChannelsCount.value === 0 || enabledEventsCount.value <= 1) {
      return '偏低'
    }
    return '平衡'
  })

  const noiseTagType = computed(() => {
    if (noiseLevelLabel.value === '偏高') {
      return 'warning'
    }
    if (noiseLevelLabel.value === '偏低') {
      return 'info'
    }
    return 'success'
  })

  const applyScenario = (scenarioId: FocusMode) => {
    selectedScenario.value = scenarioId
    form.focusMode = scenarioId

    if (scenarioId === 'quiet') {
      Object.assign(form, {
        enableInApp: true,
        enableEmail: true,
        digestMode: 'daily',
        onlyMentioned: true,
        notifyOnBuildFailed: true,
        notifyOnReviewRequested: false,
        notifyOnDeploymentFinished: false,
        notifyOnSecurityAlert: true,
      })
      return
    }

    if (scenarioId === 'critical-only') {
      Object.assign(form, {
        enableInApp: true,
        enableEmail: true,
        digestMode: 'immediate',
        onlyMentioned: false,
        notifyOnBuildFailed: true,
        notifyOnReviewRequested: false,
        notifyOnDeploymentFinished: false,
        notifyOnSecurityAlert: true,
      })
      return
    }

    Object.assign(form, {
      enableInApp: true,
      enableEmail: true,
      digestMode: 'hourly',
      onlyMentioned: false,
      notifyOnBuildFailed: true,
      notifyOnReviewRequested: true,
      notifyOnDeploymentFinished: false,
      notifyOnSecurityAlert: true,
    })
  }

  const persistConfig = () => {
    const payload: NotificationStorageModel = {
      form: cloneForm(form),
      lastSavedAt: lastSavedAt.value,
      selectedScenario: selectedScenario.value,
    }
    localStorage.setItem(storageKey, JSON.stringify(payload))
    initialSnapshot.value = JSON.stringify(form)
  }

  const restoreConfig = () => {
    const raw = localStorage.getItem(storageKey)
    if (!raw) {
      return
    }

    const parsed = JSON.parse(raw) as Partial<NotificationStorageModel>
    if (parsed.form) {
      Object.assign(form, parsed.form)
      form.deliveryWindow = {
        ...form.deliveryWindow,
        ...parsed.form.deliveryWindow,
      }
    }

    if (parsed.selectedScenario) {
      selectedScenario.value = parsed.selectedScenario
      form.focusMode = parsed.selectedScenario
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
          key: 'channels',
          title: '通知渠道',
          description: '决定消息通过站内和邮件渠道发送。',
        },
        {
          key: 'events',
          title: '触发事件',
          description: '选择哪些事件会触发通知，避免过载。',
        },
        {
          key: 'delivery',
          title: '送达策略',
          description: '配置汇总频率、打扰窗口和安静时段。',
        },
        {
          key: 'focus',
          title: '专注策略',
          description: '按团队阶段在平衡、低噪音和关键告警之间切换。',
        },
      ]
      restoreConfig()
    } catch {
      error.value = '通知配置加载失败，请稍后重试。'
    } finally {
      loading.value = false
    }
  }

  const saveConfig = async () => {
    saving.value = true
    saveError.value = ''
    saveSuccess.value = false

    try {
      await new Promise((resolve) => setTimeout(resolve, 260))
      if (form.enableEmail && (!form.smtpHost.trim() || !form.emailFromAddress.trim() || !form.smtpUsername.trim())) {
        throw new Error('启用邮件通知时，需要填写 SMTP Host、发件邮箱和 SMTP 用户名。')
      }
      lastSavedAt.value = new Date().toLocaleString('zh-CN')
      saveSuccess.value = true
      persistConfig()
    } catch (e) {
      const message = e instanceof Error ? e.message : '保存通知配置失败，请稍后重试。'
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
    enabledChannelsCount,
    enabledEventsCount,
    noiseLevelLabel,
    noiseTagType,
    loadConfig,
    saveConfig,
    applyScenario,
  }
}
