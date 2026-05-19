import { ref, computed } from 'vue'

export interface OnboardingStep {
  id: string
  title: string
  description: string
  targetElement: string
  placement: 'top' | 'bottom' | 'left' | 'right'
  skipable: boolean
}

export interface OnboardingState {
  isActive: boolean
  currentStepIndex: number
  steps: OnboardingStep[]
  hasCompleted: boolean
}

const onboardingSteps: OnboardingStep[] = [
  {
    id: 'welcome',
    title: '欢迎使用 AutoCodeForge',
    description: 'AI驱动的代码自动化平台，通过自然语言描述需求，自动生成代码并管理开发任务。',
    targetElement: 'body',
    placement: 'bottom',
    skipable: true,
  },
  {
    id: 'sidebar',
    title: '导航菜单',
    description: '左侧导航栏提供所有功能模块的快速访问入口。',
    targetElement: '.app-sidebar',
    placement: 'right',
    skipable: true,
  },
  {
    id: 'task-center',
    title: 'AI任务中心',
    description: '核心功能模块，通过自然语言描述需求创建开发任务，AI自动分析并生成代码。',
    targetElement: '[data-guide="task-center"]',
    placement: 'bottom',
    skipable: true,
  },
  {
    id: 'chat-console',
    title: 'AI聊天控制台',
    description: '支持单次提问和多轮会话模式，获取开发建议和技术咨询。',
    targetElement: '[data-guide="chat-console"]',
    placement: 'bottom',
    skipable: true,
  },
  {
    id: 'agent-center',
    title: 'Agent中心',
    description: '配置AI Agent，支持自动选择和手动引用，定制不同场景的AI助手。',
    targetElement: '[data-guide="agent-center"]',
    placement: 'bottom',
    skipable: true,
  },
  {
    id: 'repo-management',
    title: '仓库管理',
    description: '管理代码仓库、分支和Pull Request，查看项目结构。',
    targetElement: '[data-guide="repo-management"]',
    placement: 'bottom',
    skipable: true,
  },
  {
    id: 'system-config',
    title: '系统配置',
    description: '首次使用前请先配置代码仓库，这是使用任务中心等功能的前提。',
    targetElement: '[data-guide="system-config"]',
    placement: 'bottom',
    skipable: true,
  },
  {
    id: 'dashboard',
    title: 'Dashboard工作台',
    description: '查看系统概览、今日任务数、成功率和告警信息。',
    targetElement: '[data-guide="dashboard"]',
    placement: 'bottom',
    skipable: true,
  },
  {
    id: 'complete',
    title: '引导完成',
    description: '恭喜！你已经了解了AutoCodeForge的主要功能。现在可以开始创建你的第一个AI任务了！',
    targetElement: 'body',
    placement: 'bottom',
    skipable: false,
  },
]

const STORAGE_KEY = 'autocodeforge-onboarding-completed'

const isActive = ref(false)
const currentStepIndex = ref(0)
const hasCompleted = ref(false)

function checkCompletionStatus(): void {
  const completed = localStorage.getItem(STORAGE_KEY) === 'true'
  hasCompleted.value = completed
}

const currentStep = computed(() => {
  const step = onboardingSteps[currentStepIndex.value]
  return (step ?? onboardingSteps[0]) as OnboardingStep
})

const totalSteps = computed(() => onboardingSteps.length)
const isFirstStep = computed(() => currentStepIndex.value === 0)
const isLastStep = computed(() => currentStepIndex.value === totalSteps.value - 1)
const progress = computed(() => ((currentStepIndex.value + 1) / totalSteps.value) * 100)

function start(): void {
  isActive.value = true
  currentStepIndex.value = 0
}

function next(): void {
  if (currentStepIndex.value < totalSteps.value - 1) {
    currentStepIndex.value++
  } else {
    complete()
  }
}

function prev(): void {
  if (currentStepIndex.value > 0) {
    currentStepIndex.value--
  }
}

function goToStep(index: number): void {
  if (index >= 0 && index < totalSteps.value) {
    currentStepIndex.value = index
  }
}

function skip(): void {
  isActive.value = false
  localStorage.setItem(STORAGE_KEY, 'true')
  hasCompleted.value = true
}

function complete(): void {
  isActive.value = false
  localStorage.setItem(STORAGE_KEY, 'true')
  hasCompleted.value = true
}

function reset(): void {
  isActive.value = false
  currentStepIndex.value = 0
  localStorage.removeItem(STORAGE_KEY)
  hasCompleted.value = false
}

function shouldShowOnboarding(): boolean {
  checkCompletionStatus()
  return !hasCompleted.value
}

export function useOnboarding() {
  return {
    isActive,
    currentStep,
    currentStepIndex,
    totalSteps,
    isFirstStep,
    isLastStep,
    progress,
    hasCompleted,
    steps: onboardingSteps,
    start,
    next,
    prev,
    goToStep,
    skip,
    complete,
    reset,
    shouldShowOnboarding,
    checkCompletionStatus,
  }
}
