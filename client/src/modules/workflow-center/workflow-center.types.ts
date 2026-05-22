export interface RequirementItemDto {
  id: string
  repositoryId: string
  repositoryName: string
  repositoryUrl: string
  title: string
  source: 'manual' | 'issue' | 'prd' | 'ticket'
  priority: 'P0' | 'P1' | 'P2' | 'P3'
  businessValue: number
  riskLevel: 'low' | 'medium' | 'high'
  state: '待分析' | '执行中' | '已完成' | '已阻塞'
  owner: string
  stakeholders: string[]
  tags: string[]
  expectedRelease: string
  createdAt: string
  updatedAt: string
  acceptanceCriteria: string[]
}

export interface RoundItemDto {
  id: string
  requirementId: string
  repositoryId: string
  requirementTitle: string
  repositoryName: string
  sprintName: string
  goal: string
  state: '规划中' | '执行中' | '已完成' | '复盘中'
  startDate: string
  endDate: string
  velocity: number
  completionRate: number
  blockerCount: number
  reviewOwner: string
  retrospectiveSummary: string
}

export interface TaskItemDto {
  id: string
  requirementId: string
  roundIds: string[]
  repositoryId: string
  repositoryName: string
  title: string
  type: '开发' | '测试' | '文档' | '运维'
  assignee: string
  state: '待办' | '进行中' | '联调中' | '待验收' | '已完成' | '已阻塞'
  priority: '高' | '中' | '低'
  estimateHours: number
  spentHours: number
  remainingHours: number
  dependencyTaskIds: string[]
  updatedAt: string
}

export interface FlowStageDto {
  key: 'receive' | 'decompose' | 'execute' | 'finish-tasks' | 'finish-requirement'
  title: string
  description: string
  target: number
  completed: number
}

export interface NotificationItemDto {
  id: string
  repositoryId: string
  repositoryName: string
  category: 'review-request' | 'mention' | 'deployment-approval' | 'workflow-failure'
  reason: string
  title: string
  sourceType: 'issue' | 'pull-request' | 'workflow' | 'deployment'
  sourceUrl: string
  priority: 'P0' | 'P1' | 'P2'
  state: '待处理' | '已保存' | '已完成'
  receivedAt: string
}

export interface ApprovalItemDto {
  id: string
  repositoryId: string
  repositoryName: string
  objectType: 'pull-request' | 'deployment' | 'config-release'
  objectId: string
  title: string
  requiredAction: 'approve' | 'comment' | 'request-changes'
  reviewer: string
  dueAt: string
  status: '等待审批' | '超时风险' | '已完成'
}

export interface HumanLoopCaseDto {
  id: string
  repositoryId: string
  repositoryName: string
  taskId: string
  triggerReason: string
  riskLevel: 'high' | 'medium' | 'low'
  confidence: number
  recommendedAction: string
  owner: string
  escalationOwner: string
  slaDeadline: string
  status: '待人工介入' | '处理中' | '已闭环'
  contextSummary: string
}
