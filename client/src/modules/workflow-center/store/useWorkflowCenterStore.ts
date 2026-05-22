import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { useRepoManagementStore } from '../../repo-management/store/useRepoManagementStore'
import { useRepoStore } from '../../../stores/useRepoStore'
import type {
  ApprovalItemDto,
  FlowStageDto,
  HumanLoopCaseDto,
  NotificationItemDto,
  RequirementItemDto,
  RoundItemDto,
  TaskItemDto,
} from '../workflow-center.types'

const requirementSeed: RequirementItemDto[] = [
  {
    id: 'REQ-401',
    repositoryId: 'repo-101',
    repositoryName: 'AutoCodeForge/client',
    repositoryUrl: 'https://github.com/miyamaerei/AutoCodeForge',
    title: '配置中心支持多环境参数版本化',
    source: 'ticket',
    priority: 'P0',
    businessValue: 92,
    riskLevel: 'medium',
    state: '执行中',
    owner: 'Luna Chen',
    stakeholders: ['DevOps', 'Backend', 'SRE'],
    tags: ['config', 'versioning', 'release'],
    expectedRelease: '2026-06-15',
    createdAt: '2026-05-02',
    updatedAt: '2026-05-21',
    acceptanceCriteria: [
      '支持按仓库 + 环境读取配置版本',
      '支持配置发布回滚与审计',
      '前端页面展示版本对比与变更人',
    ],
  },
  {
    id: 'REQ-402',
    repositoryId: 'repo-102',
    repositoryName: 'AutoCodeForge/server',
    repositoryUrl: 'https://github.com/miyamaerei/AutoCodeForge',
    title: '任务中心增加任务 SLA 与超时告警',
    source: 'issue',
    priority: 'P1',
    businessValue: 84,
    riskLevel: 'high',
    state: '待分析',
    owner: 'David Gu',
    stakeholders: ['Backend', 'Agent Team'],
    tags: ['task-center', 'sla', 'alert'],
    expectedRelease: '2026-06-28',
    createdAt: '2026-05-10',
    updatedAt: '2026-05-20',
    acceptanceCriteria: [
      'SLA 阈值支持按任务类型配置',
      '超时任务进入风险看板并触发通知',
      '支持轮次维度查询 SLA 达成率',
    ],
  },
  {
    id: 'REQ-403',
    repositoryId: 'repo-101',
    repositoryName: 'AutoCodeForge/client',
    repositoryUrl: 'https://github.com/miyamaerei/AutoCodeForge',
    title: '流程中心提供端到端需求追踪页',
    source: 'prd',
    priority: 'P1',
    businessValue: 88,
    riskLevel: 'low',
    state: '已完成',
    owner: 'Mia Zhou',
    stakeholders: ['Frontend', 'PMO'],
    tags: ['workflow', 'traceability'],
    expectedRelease: '2026-05-30',
    createdAt: '2026-04-30',
    updatedAt: '2026-05-22',
    acceptanceCriteria: [
      '展示需求-轮次-任务三级关联',
      '流程状态可视化且可筛选仓库',
      '支持查看每个需求完成率',
    ],
  },
]

const roundSeed: RoundItemDto[] = [
  {
    id: 'ROUND-31',
    requirementId: 'REQ-401',
    repositoryId: 'repo-101',
    requirementTitle: '配置中心支持多环境参数版本化',
    repositoryName: 'AutoCodeForge/client',
    sprintName: 'Sprint 2026-W21-A',
    goal: '完成版本化 UI 与筛选器',
    state: '执行中',
    startDate: '2026-05-18',
    endDate: '2026-05-29',
    velocity: 38,
    completionRate: 62,
    blockerCount: 1,
    reviewOwner: 'Luna Chen',
    retrospectiveSummary: '筛选逻辑稳定，接口字段仍需统一。',
  },
  {
    id: 'ROUND-32',
    requirementId: 'REQ-401',
    repositoryId: 'repo-101',
    requirementTitle: '配置中心支持多环境参数版本化',
    repositoryName: 'AutoCodeForge/client',
    sprintName: 'Sprint 2026-W22-B',
    goal: '补齐回滚与审计联调',
    state: '规划中',
    startDate: '2026-06-01',
    endDate: '2026-06-12',
    velocity: 42,
    completionRate: 0,
    blockerCount: 0,
    reviewOwner: 'Mia Zhou',
    retrospectiveSummary: '待开始。',
  },
  {
    id: 'ROUND-33',
    requirementId: 'REQ-403',
    repositoryId: 'repo-101',
    requirementTitle: '流程中心提供端到端需求追踪页',
    repositoryName: 'AutoCodeForge/client',
    sprintName: 'Sprint 2026-W20',
    goal: '完成需求-轮次-任务链路视图',
    state: '已完成',
    startDate: '2026-05-05',
    endDate: '2026-05-16',
    velocity: 35,
    completionRate: 100,
    blockerCount: 0,
    reviewOwner: 'David Gu',
    retrospectiveSummary: '交互可用，后续补字段统计。',
  },
]

const taskSeed: TaskItemDto[] = [
  {
    id: 'TASK-9601',
    requirementId: 'REQ-401',
    roundIds: ['ROUND-31'],
    repositoryId: 'repo-101',
    repositoryName: 'AutoCodeForge/client',
    title: '实现仓库维度需求筛选栏',
    type: '开发',
    assignee: 'Ariel Li',
    state: '已完成',
    priority: '高',
    estimateHours: 8,
    spentHours: 7,
    remainingHours: 0,
    dependencyTaskIds: [],
    updatedAt: '2026-05-21 15:20',
  },
  {
    id: 'TASK-9602',
    requirementId: 'REQ-401',
    roundIds: ['ROUND-31', 'ROUND-32'],
    repositoryId: 'repo-101',
    repositoryName: 'AutoCodeForge/client',
    title: '补充版本回滚审批流程表单',
    type: '开发',
    assignee: 'Mia Zhou',
    state: '进行中',
    priority: '高',
    estimateHours: 13,
    spentHours: 9,
    remainingHours: 4,
    dependencyTaskIds: ['TASK-9601'],
    updatedAt: '2026-05-22 10:42',
  },
  {
    id: 'TASK-9603',
    requirementId: 'REQ-401',
    roundIds: ['ROUND-31'],
    repositoryId: 'repo-101',
    repositoryName: 'AutoCodeForge/client',
    title: '编写配置审计 API 联调测试用例',
    type: '测试',
    assignee: 'Nina Gao',
    state: '待验收',
    priority: '中',
    estimateHours: 6,
    spentHours: 5,
    remainingHours: 1,
    dependencyTaskIds: ['TASK-9602'],
    updatedAt: '2026-05-22 09:18',
  },
  {
    id: 'TASK-9604',
    requirementId: 'REQ-402',
    roundIds: [],
    repositoryId: 'repo-102',
    repositoryName: 'AutoCodeForge/server',
    title: '定义任务 SLA 规则与状态枚举',
    type: '开发',
    assignee: 'Ken Wu',
    state: '待办',
    priority: '高',
    estimateHours: 10,
    spentHours: 0,
    remainingHours: 10,
    dependencyTaskIds: [],
    updatedAt: '2026-05-20 18:00',
  },
]

const flowSeed: FlowStageDto[] = [
  {
    key: 'receive',
    title: '收到需求',
    description: '需求绑定仓库，进入待分析池',
    target: 16,
    completed: 15,
  },
  {
    key: 'decompose',
    title: '拆分任务',
    description: '需求拆分为可执行任务并建立依赖',
    target: 16,
    completed: 13,
  },
  {
    key: 'execute',
    title: '多次轮次执行任务',
    description: '按轮次推进，跟踪速度与阻塞项',
    target: 45,
    completed: 29,
  },
  {
    key: 'finish-tasks',
    title: '完成所有任务',
    description: '任务全部完成并通过验收',
    target: 16,
    completed: 9,
  },
  {
    key: 'finish-requirement',
    title: '完成需求',
    description: '关联任务全绿并完成发布',
    target: 16,
    completed: 7,
  },
]

const notificationSeed: NotificationItemDto[] = [
  {
    id: 'NTF-7001',
    repositoryId: 'repo-101',
    repositoryName: 'AutoCodeForge/client',
    category: 'review-request',
    reason: 'review_requested',
    title: 'PR #182 等待你完成代码评审',
    sourceType: 'pull-request',
    sourceUrl: 'https://github.com/miyamaerei/AutoCodeForge/pull/182',
    priority: 'P1',
    state: '待处理',
    receivedAt: '2026-05-22 09:03',
  },
  {
    id: 'NTF-7002',
    repositoryId: 'repo-102',
    repositoryName: 'AutoCodeForge/server',
    category: 'workflow-failure',
    reason: 'ci_failed',
    title: 'Pipeline #844 在集成测试阶段失败',
    sourceType: 'workflow',
    sourceUrl: 'https://github.com/miyamaerei/AutoCodeForge/actions',
    priority: 'P0',
    state: '待处理',
    receivedAt: '2026-05-22 10:25',
  },
  {
    id: 'NTF-7003',
    repositoryId: 'repo-101',
    repositoryName: 'AutoCodeForge/client',
    category: 'mention',
    reason: 'mention',
    title: '你被 @ 提及：配置发布页面需要补充审批人字段',
    sourceType: 'issue',
    sourceUrl: 'https://github.com/miyamaerei/AutoCodeForge/issues',
    priority: 'P2',
    state: '已保存',
    receivedAt: '2026-05-21 16:40',
  },
]

const approvalSeed: ApprovalItemDto[] = [
  {
    id: 'APR-4201',
    repositoryId: 'repo-101',
    repositoryName: 'AutoCodeForge/client',
    objectType: 'pull-request',
    objectId: 'PR-182',
    title: '配置中心回滚审批表单联调',
    requiredAction: 'approve',
    reviewer: 'Luna Chen',
    dueAt: '2026-05-22 18:00',
    status: '等待审批',
  },
  {
    id: 'APR-4202',
    repositoryId: 'repo-102',
    repositoryName: 'AutoCodeForge/server',
    objectType: 'deployment',
    objectId: 'DEP-91',
    title: '生产部署前手动门禁确认',
    requiredAction: 'approve',
    reviewer: 'David Gu',
    dueAt: '2026-05-22 14:30',
    status: '超时风险',
  },
  {
    id: 'APR-4203',
    repositoryId: 'repo-101',
    repositoryName: 'AutoCodeForge/client',
    objectType: 'config-release',
    objectId: 'CFG-27',
    title: '多环境配置版本 v2.3.0 发布审批',
    requiredAction: 'comment',
    reviewer: 'Mia Zhou',
    dueAt: '2026-05-23 11:00',
    status: '等待审批',
  },
]

const humanLoopCaseSeed: HumanLoopCaseDto[] = [
  {
    id: 'HITL-9001',
    repositoryId: 'repo-101',
    repositoryName: 'AutoCodeForge/client',
    taskId: 'TASK-9602',
    triggerReason: '模型对审批分支判定冲突，建议人工确认是否允许回滚覆盖。',
    riskLevel: 'high',
    confidence: 0.56,
    recommendedAction: '指派主审 + 备份审查人，在 2 小时内完成 go/no-go 决策。',
    owner: 'Mia Zhou',
    escalationOwner: 'Luna Chen',
    slaDeadline: '2026-05-22 13:30',
    status: '待人工介入',
    contextSummary: '涉及生产配置回滚，影响 3 个环境，需核对审计日志完整性。',
  },
  {
    id: 'HITL-9002',
    repositoryId: 'repo-102',
    repositoryName: 'AutoCodeForge/server',
    taskId: 'TASK-9604',
    triggerReason: 'CI 失败后自动修复建议存在依赖升级风险。',
    riskLevel: 'medium',
    confidence: 0.63,
    recommendedAction: '先进行小流量验证，再决定是否合并修复补丁。',
    owner: 'Ken Wu',
    escalationOwner: 'David Gu',
    slaDeadline: '2026-05-22 17:00',
    status: '处理中',
    contextSummary: '依赖升级可能影响任务中心 API 序列化，需人工验证向后兼容。',
  },
]

export const useWorkflowCenterStore = defineStore('module.workflow-center', () => {
  const repoManagementStore = useRepoManagementStore()
  const repoGlobal = useRepoStore()

  const requirements = ref<RequirementItemDto[]>([])
  const rounds = ref<RoundItemDto[]>([])
  const tasks = ref<TaskItemDto[]>([])
  const flow = ref<FlowStageDto[]>([])
  const notifications = ref<NotificationItemDto[]>([])
  const approvals = ref<ApprovalItemDto[]>([])
  const humanLoopCases = ref<HumanLoopCaseDto[]>([])

  const selectedRepository = computed<string>({
    get: () => repoGlobal.selectedRepositoryId || 'all',
    set: (repositoryId: string) => {
      repoGlobal.selectRepository(repositoryId === 'all' ? null : repositoryId)
    },
  })

  const loading = ref(false)
  const loaded = ref(false)
  const error = ref<string | null>(null)

  const repositoryOptions = computed(() => {
    return repoManagementStore.repositories.map((repo) => ({
      id: repo.id,
      name: repo.name,
    }))
  })

  const hasSelectedRepositoryMapping = computed(() => {
    if (selectedRepository.value === 'all') {
      return true
    }

    const selected = selectedRepository.value
    return (
      requirements.value.some((item) => item.repositoryId === selected) ||
      rounds.value.some((item) => item.repositoryId === selected) ||
      tasks.value.some((item) => item.repositoryId === selected) ||
      notifications.value.some((item) => item.repositoryId === selected) ||
      approvals.value.some((item) => item.repositoryId === selected) ||
      humanLoopCases.value.some((item) => item.repositoryId === selected)
    )
  })

  const effectiveRepository = computed(() => {
    if (!hasSelectedRepositoryMapping.value) {
      return 'all'
    }
    return selectedRepository.value
  })

  const filteredRequirements = computed(() => {
    if (effectiveRepository.value === 'all') {
      return requirements.value
    }
    return requirements.value.filter((item) => item.repositoryId === effectiveRepository.value)
  })

  const filteredRounds = computed(() => {
    if (effectiveRepository.value === 'all') {
      return rounds.value
    }
    return rounds.value.filter((item) => item.repositoryId === effectiveRepository.value)
  })

  const filteredTasks = computed(() => {
    if (effectiveRepository.value === 'all') {
      return tasks.value
    }
    return tasks.value.filter((item) => item.repositoryId === effectiveRepository.value)
  })

  const filteredNotifications = computed(() => {
    if (effectiveRepository.value === 'all') {
      return notifications.value
    }
    return notifications.value.filter((item) => item.repositoryId === effectiveRepository.value)
  })

  const filteredApprovals = computed(() => {
    if (effectiveRepository.value === 'all') {
      return approvals.value
    }
    return approvals.value.filter((item) => item.repositoryId === effectiveRepository.value)
  })

  const filteredHumanLoopCases = computed(() => {
    if (effectiveRepository.value === 'all') {
      return humanLoopCases.value
    }
    return humanLoopCases.value.filter((item) => item.repositoryId === effectiveRepository.value)
  })

  const stats = computed(() => {
    const reqDone = filteredRequirements.value.filter((item) => item.state === '已完成').length
    const taskDone = filteredTasks.value.filter((item) => item.state === '已完成').length
    const taskBlocked = filteredTasks.value.filter((item) => item.state === '已阻塞').length
    return {
      requirementTotal: filteredRequirements.value.length,
      requirementDone: reqDone,
      roundTotal: filteredRounds.value.length,
      taskTotal: filteredTasks.value.length,
      taskDone,
      taskBlocked,
      notificationPending: filteredNotifications.value.filter((item) => item.state === '待处理').length,
      approvalPending: filteredApprovals.value.filter((item) => item.status !== '已完成').length,
      humanLoopOpen: filteredHumanLoopCases.value.filter((item) => item.status !== '已闭环').length,
    }
  })

  async function loadWorkflowData(): Promise<void> {
    if (loaded.value) {
      return
    }
    loading.value = true
    error.value = null
    try {
      if (!repoManagementStore.repositories.length) {
        await repoManagementStore.loadRepositories()
      }
      requirements.value = requirementSeed
      rounds.value = roundSeed
      tasks.value = taskSeed
      flow.value = flowSeed
      notifications.value = notificationSeed
      approvals.value = approvalSeed
      humanLoopCases.value = humanLoopCaseSeed
      loaded.value = true
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载流程数据失败'
    } finally {
      loading.value = false
    }
  }

  function setSelectedRepository(repositoryId: string): void {
    selectedRepository.value = repositoryId
  }

  return {
    requirements,
    rounds,
    tasks,
    flow,
    notifications,
    approvals,
    humanLoopCases,
    loading,
    loaded,
    error,
    selectedRepository,
    repositoryOptions,
    filteredRequirements,
    filteredRounds,
    filteredTasks,
    filteredNotifications,
    filteredApprovals,
    filteredHumanLoopCases,
    stats,
    loadWorkflowData,
    setSelectedRepository,
  }
})
