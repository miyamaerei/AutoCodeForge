export interface DashboardStatsDto {
  tasksToday: number
  successRate: number
  runningCount: number
  alertCount: number
}

export interface RecentTaskDto {
  id: string
  name: string
  state: '执行中' | '成功' | '暂停'
  percent: number
}

export interface ModuleEntryDto {
  title: string
  desc: string
  tagType: 'danger' | 'primary' | 'success' | 'warning' | 'info'
  status: string
  routePath: string
}

export interface TaskSummaryDto {
  id: string
  title: string
  state: '运行中' | '已完成' | '已暂停' | '失败'
  createdAt: string
}

export interface TaskCreateRequestDto {
  title: string
  taskType: string
  description: string
  repository: string
  branch: string
}

export interface TaskStepDto {
  id: string
  title: string
}

export interface TaskDetailDto {
  id: string
  title: string
  state: '运行中' | '已完成' | '已暂停' | '失败'
  steps: TaskStepDto[]
}

const taskSummaries: TaskSummaryDto[] = [
  { id: 'T-1009', title: '订单导出异常修复', state: '运行中', createdAt: '2024-05-19 14:30' },
  { id: 'T-1008', title: '会员模块接口重构', state: '已完成', createdAt: '2024-05-18 10:15' },
  { id: 'T-1007', title: '支付回调重试策略', state: '已暂停', createdAt: '2024-05-17 09:45' },
]

const taskDetails: Record<string, TaskDetailDto> = {
  'T-1009': {
    id: 'T-1009',
    title: '订单导出异常修复',
    state: '运行中',
    steps: [
      { id: 'step-1', title: '拉取代码' },
      { id: 'step-2', title: '修改代码' },
      { id: 'step-3', title: '编译失败' },
      { id: 'step-4', title: '自动修复' },
      { id: 'step-5', title: '编译成功' },
      { id: 'step-6', title: '提交PR' },
    ],
  },
  'T-1008': {
    id: 'T-1008',
    title: '会员模块接口重构',
    state: '已完成',
    steps: [
      { id: 'step-1', title: '拉取代码' },
      { id: 'step-2', title: '修改代码' },
      { id: 'step-3', title: '编译成功' },
      { id: 'step-4', title: '提交PR' },
    ],
  },
}

export async function getDashboardStats(): Promise<DashboardStatsDto> {
  await sleep(220)
  return {
    tasksToday: 23,
    successRate: 92.4,
    runningCount: 5,
    alertCount: 2,
  }
}

export async function getRecentTasks(): Promise<RecentTaskDto[]> {
  await sleep(260)
  return [
    { id: 'T-1009', name: '订单导出异常修复', state: '执行中', percent: 58 },
    { id: 'T-1008', name: '会员模块接口重构', state: '成功', percent: 100 },
    { id: 'T-1007', name: '支付回调重试策略', state: '暂停', percent: 71 },
  ]
}

export async function getModuleEntries(): Promise<ModuleEntryDto[]> {
  await sleep(180)
  return [
    {
      title: 'AI任务中心',
      desc: '自然语言提需求、多轮对话、执行步骤、日志和Diff展示。',
      tagType: 'danger',
      status: '核心模块',
      routePath: '/task-center',
    },
    {
      title: '项目/仓库管理',
      desc: '仓库列表、分支、文件树与PR视图。',
      tagType: 'primary',
      status: 'MVP',
      routePath: '/repo-management',
    },
    {
      title: 'Dashboard工作台',
      desc: '今日任务数、成功率、最近任务与告警。',
      tagType: 'success',
      status: 'MVP',
      routePath: '/dashboard',
    },
    {
      title: '流水线中心',
      desc: '构建状态、步骤状态、日志查看（基础版）。',
      tagType: 'warning',
      status: '基础版',
      routePath: '/pipeline-center',
    },
    {
      title: '系统配置',
      desc: 'API Key、模型选择、用户管理（简化）。',
      tagType: 'info',
      status: '基础版',
      routePath: '/settings',
    },
  ]
}

export async function getTaskSummaries(): Promise<TaskSummaryDto[]> {
  await sleep(220)
  return taskSummaries
}

export async function getTaskDetail(taskId: string): Promise<TaskDetailDto> {
  await sleep(260)
  const detail = taskDetails[taskId]
  if (!detail) {
    throw new Error('任务不存在')
  }
  return detail
}

export async function createTask(payload: TaskCreateRequestDto): Promise<TaskSummaryDto> {
  await sleep(240)
  const now = Date.now().toString().slice(-4)
  const task: TaskSummaryDto = {
    id: `T-${now}`,
    title: payload.title,
    state: '运行中',
    createdAt: new Date().toLocaleString('zh-CN', { hour12: false }),
  }
  taskSummaries.unshift(task)
  taskDetails[task.id] = {
    id: task.id,
    title: task.title,
    state: task.state,
    steps: [
      { id: 'step-1', title: '拉取代码' },
      { id: 'step-2', title: '分析需求' },
      { id: 'step-3', title: '执行任务' },
    ],
  }
  return task
}

async function sleep(ms: number): Promise<void> {
  await new Promise((resolve) => {
    setTimeout(resolve, ms)
  })
}
