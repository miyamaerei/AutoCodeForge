/**
 * 瀹氭椂浠诲姟 API 妯″潡 - 鎻愪緵瀹氭椂浠诲姟鐨?CRUD 鎿嶄綔 Mock 瀹炵幇
 */

export type TaskStatus = 'pending' | 'running' | 'success' | 'failed' | 'disabled'
export type TriggerType = 'cron' | 'interval' | 'once'

/** 鍏宠仈鐨勪粨搴撲俊鎭?*/
export interface TaskRepoRef {
  /** 浠撳簱 ID */
  repoId: string
  /** 浠撳簱鍚嶇О */
  repoName: string
  /** 浠撳簱 URL */
  repoUrl: string
  /** 鐩爣鍒嗘敮 */
  branch: string
  /** 鐩爣璺緞锛堝彲閫夛紝濡?src/utils锛?*/
  path?: string
}

export interface ScheduledTaskDto {
  /** 浠诲姟 ID */
  id: string
  /** 浠诲姟鍚嶇О */
  name: string
  /** 浠诲姟鎻忚堪 */
  description: string
  /** 鍏宠仈鐨勬ā鏉?ID锛堝彲閫夛級 */
  templateId?: string
  /** 瑙﹀彂绫诲瀷 */
  triggerType: TriggerType
  /** Cron 琛ㄨ揪寮忥紙triggerType 涓?cron 鏃讹級 */
  cronExpression: string
  /** 闂撮殧姣锛坱riggerType 涓?interval 鏃讹級 */
  intervalMs: number
  /** 涓€娆℃€ф墽琛屾椂闂达紙triggerType 涓?once 鏃讹級 */
  onceTime: string
  /** 鍏宠仈鐨?Agent ID */
  agentId: string
  /** 鍏宠仈鐨?Agent 鍚嶇О */
  agentName: string
  /** 鍏宠仈鐨勪粨搴?*/
  repo?: TaskRepoRef
  /** 浠诲姟鍙傛暟锛圝SON 瀛楃涓诧級 */
  params: string
  /** 浠诲姟鐘舵€?*/
  status: TaskStatus
  /** 涓嬫鎵ц鏃堕棿 */
  nextRunTime: string
  /** 涓婃鎵ц鏃堕棿 */
  lastRunTime: string
  /** 鎬绘墽琛屾鏁?*/
  totalRuns: number
  /** 鎴愬姛娆℃暟 */
  successRuns: number
  /** 澶辫触娆℃暟 */
  failedRuns: number
  /** 鏄惁鍚敤 */
  enabled: boolean
  /** 鍒涘缓鏃堕棿 */
  createdAt: string
  /** 鏇存柊鏃堕棿 */
  updatedAt: string
}

export interface CreateScheduledTaskDto {
  name: string
  description: string
  templateId?: string
  triggerType: TriggerType
  cronExpression: string
  intervalMs: number
  onceTime: string
  agentId: string
  repo?: TaskRepoRef
  params: string
  enabled: boolean
}

export interface UpdateScheduledTaskDto extends Partial<CreateScheduledTaskDto> {
  id: string
}

/** 浠诲姟妯℃澘 */
export interface TaskTemplateDto {
  /** 妯℃澘 ID */
  id: string
  /** 妯℃澘鍚嶇О */
  name: string
  /** 妯℃澘鎻忚堪 */
  description: string
  /** 棰勮鐨?Agent ID */
  agentId: string
  /** 棰勮鐨勭郴缁熸彁绀鸿瘝锛堥儴鍒嗚鐩栵級 */
  systemPromptOverride?: string
  /** 棰勮鐨勮Е鍙戠被鍨?*/
  triggerType: TriggerType
  /** 棰勮鐨?Cron 琛ㄨ揪寮?*/
  cronExpression?: string
  /** 棰勮鐨勯棿闅旓紙姣锛?*/
  intervalMs?: number
  /** 棰勮鍙傛暟 */
  defaultParams: string
  /** 鏄惁涓虹郴缁熷唴缃ā鏉?*/
  isBuiltIn: boolean
}

export interface ExecutionRecordDto {
  /** 鎵ц璁板綍 ID */
  id: string
  /** 浠诲姟 ID */
  taskId: string
  /** 鎵ц寮€濮嬫椂闂?*/
  startTime: string
  /** 鎵ц缁撴潫鏃堕棿 */
  endTime: string
  /** 鎵ц鐘舵€?*/
  status: TaskStatus
  /** 鎵ц鑰楁椂锛堟绉掞級 */
  duration: number
  /** 鎵ц缁撴灉/閿欒淇℃伅 */
  result: string
}

const defaultTasks: ScheduledTaskDto[] = [
  {
    id: 'st-1',
    name: '姣忔棩浠ｇ爜璐ㄩ噺妫€鏌?,
    description: '姣忓ぉ鏃╀笂9鐐硅嚜鍔ㄦ墽琛屼唬鐮佽川閲忔鏌ワ紝鐢熸垚鎶ュ憡',
    templateId: 'tpl-code-review',
    triggerType: 'cron',
    cronExpression: '0 9 * * *',
    intervalMs: 0,
    onceTime: '',
    agentId: 'agent-1',
    agentName: '浠ｇ爜瀹℃煡鍔╂墜',
    repo: {
      repoId: 'repo-1',
      repoName: 'AutoCodeForge',
      repoUrl: 'https://github.com/org/AutoCodeForge',
      branch: 'main',
    },
    params: '{"reportLevel":"detailed"}',
    status: 'success',
    nextRunTime: '2026-05-20 09:00:00',
    lastRunTime: '2026-05-19 09:00:00',
    totalRuns: 19,
    successRuns: 18,
    failedRuns: 1,
    enabled: true,
    createdAt: '2026-05-01 10:00:00',
    updatedAt: '2026-05-19 09:00:00',
  },
  {
    id: 'st-2',
    name: '鏁版嵁搴撳浠?,
    description: '姣?灏忔椂鎵ц涓€娆℃暟鎹簱鍏ㄩ噺澶囦唤',
    templateId: 'tpl-db-backup',
    triggerType: 'interval',
    cronExpression: '',
    intervalMs: 6 * 60 * 60 * 1000,
    onceTime: '',
    agentId: 'agent-3',
    agentName: '鏁版嵁搴撲笓瀹?,
    repo: {
      repoId: 'repo-2',
      repoName: 'backend-service',
      repoUrl: 'https://github.com/org/backend-service',
      branch: 'develop',
    },
    params: '{"backupType":"full"}',
    status: 'success',
    nextRunTime: '2026-05-19 22:00:00',
    lastRunTime: '2026-05-19 16:00:00',
    totalRuns: 76,
    successRuns: 75,
    failedRuns: 1,
    enabled: true,
    createdAt: '2026-05-02 08:00:00',
    updatedAt: '2026-05-19 16:00:00',
  },
  {
    id: 'st-3',
    name: '鍛ㄦ姤鑷姩鐢熸垚',
    description: '姣忓懆浜斾笅鍗?鐐硅嚜鍔ㄧ敓鎴愰」鐩懆鎶?,
    templateId: 'tpl-doc-gen',
    triggerType: 'cron',
    cronExpression: '0 18 * * 5',
    intervalMs: 0,
    onceTime: '',
    agentId: 'agent-5',
    agentName: '鏂囨。鎾板啓鍔╂墜',
    repo: {
      repoId: 'repo-1',
      repoName: 'AutoCodeForge',
      repoUrl: 'https://github.com/org/AutoCodeForge',
      branch: 'main',
      path: 'docs',
    },
    params: '{"template":"weekly","recipients":["team@example.com"]}',
    status: 'pending',
    nextRunTime: '2026-05-23 18:00:00',
    lastRunTime: '2026-05-16 18:00:00',
    totalRuns: 4,
    successRuns: 4,
    failedRuns: 0,
    enabled: true,
    createdAt: '2026-05-03 14:00:00',
    updatedAt: '2026-05-16 18:00:00',
  },
  {
    id: 'st-4',
    name: '鎬ц兘鐩戞帶鎶ュ憡',
    description: '姣忓皬鏃舵鏌ョ郴缁熸€ц兘鎸囨爣锛屽紓甯告椂鍛婅',
    templateId: 'tpl-monitor',
    triggerType: 'interval',
    cronExpression: '',
    intervalMs: 60 * 60 * 1000,
    onceTime: '',
    agentId: 'agent-2',
    agentName: '鏋舵瀯璁捐涓撳',
    repo: {
      repoId: 'repo-2',
      repoName: 'backend-service',
      repoUrl: 'https://github.com/org/backend-service',
      branch: 'main',
    },
    params: '{"alertThreshold":{"cpu":80,"memory":85}}',
    status: 'failed',
    nextRunTime: '2026-05-19 21:00:00',
    lastRunTime: '2026-05-19 20:00:00',
    totalRuns: 200,
    successRuns: 195,
    failedRuns: 5,
    enabled: false,
    createdAt: '2026-05-04 10:00:00',
    updatedAt: '2026-05-19 20:00:00',
  },
  {
    id: 'st-5',
    name: '涓存椂鏁版嵁娓呯悊',
    description: '涓€娆℃€т换鍔★細娓呯悊30澶╁墠鐨勪复鏃舵枃浠?,
    triggerType: 'once',
    cronExpression: '',
    intervalMs: 0,
    onceTime: '2026-05-25 02:00:00',
    agentId: 'agent-3',
    agentName: '鏁版嵁搴撲笓瀹?,
    repo: {
      repoId: 'repo-2',
      repoName: 'backend-service',
      repoUrl: 'https://github.com/org/backend-service',
      branch: 'develop',
      path: 'temp',
    },
    params: '{"retentionDays":30,"dryRun":false}',
    status: 'pending',
    nextRunTime: '2026-05-25 02:00:00',
    lastRunTime: '',
    totalRuns: 0,
    successRuns: 0,
    failedRuns: 0,
    enabled: true,
    createdAt: '2026-05-05 11:00:00',
    updatedAt: '2026-05-19 15:00:00',
  },
]

/** 榛樿浠诲姟妯℃澘 */
const defaultTemplates: TaskTemplateDto[] = [
  {
    id: 'tpl-code-review',
    name: '浠ｇ爜瀹℃煡浠诲姟',
    description: '鑷姩鍖栦唬鐮佽川閲忔鏌ュ拰瀹夊叏鎵弿',
    agentId: 'agent-1',
    systemPromptOverride: '浣犳槸涓€涓笓涓氱殑浠ｇ爜瀹℃煡鍔╂墜锛岃礋璐ｅ垎鏋愪唬鐮佽川閲忋€佸彂鐜版綔鍦ㄩ棶棰樺苟鎻愪緵鏀硅繘寤鸿銆?,
    triggerType: 'cron',
    cronExpression: '0 9 * * *',
    defaultParams: '{"reportLevel":"detailed","checkSecurity":true}',
    isBuiltIn: true,
  },
  {
    id: 'tpl-db-backup',
    name: '鏁版嵁搴撳浠戒换鍔?,
    description: '瀹氭湡鎵ц鏁版嵁搴撳叏閲忔垨澧為噺澶囦唤',
    agentId: 'agent-3',
    triggerType: 'interval',
    intervalMs: 6 * 60 * 60 * 1000,
    defaultParams: '{"backupType":"full","compression":true}',
    isBuiltIn: true,
  },
  {
    id: 'tpl-doc-gen',
    name: '鏂囨。鐢熸垚浠诲姟',
    description: '鑷姩鐢熸垚鎶€鏈枃妗ｃ€佸懆鎶ャ€丄PI 鏂囨。绛?,
    agentId: 'agent-5',
    systemPromptOverride: '浣犳槸涓€涓妧鏈枃妗ｄ笓瀹讹紝鎿呴暱鎾板啓娓呮櫚銆佸噯纭殑鎶€鏈枃妗ｃ€?,
    triggerType: 'cron',
    cronExpression: '0 18 * * 5',
    defaultParams: '{"template":"weekly","format":"markdown"}',
    isBuiltIn: true,
  },
  {
    id: 'tpl-monitor',
    name: '绯荤粺鐩戞帶浠诲姟',
    description: '鐩戞帶绯荤粺鎬ц兘鎸囨爣锛屽紓甯告椂鍙戦€佸憡璀?,
    agentId: 'agent-2',
    triggerType: 'interval',
    intervalMs: 60 * 60 * 1000,
    defaultParams: '{"alertThreshold":{"cpu":80,"memory":85}}',
    isBuiltIn: true,
  },
  {
    id: 'tpl-refactor',
    name: '浠ｇ爜閲嶆瀯浠诲姟',
    description: '瀵规寚瀹氫唬鐮佽繘琛岄噸鏋勪紭鍖?,
    agentId: 'agent-4',
    systemPromptOverride: '浣犳槸涓€涓噸鏋勪笓瀹讹紝璐熻矗浼樺寲浠ｇ爜缁撴瀯锛屾彁楂樺彲缁存姢鎬у拰鎬ц兘銆?,
    triggerType: 'once',
    defaultParams: '{"scope":"incremental","safetyLevel":"high"}',
    isBuiltIn: true,
  },
]

const defaultExecutions: ExecutionRecordDto[] = [
  {
    id: 'ex-1',
    taskId: 'st-1',
    startTime: '2026-05-19 09:00:00',
    endTime: '2026-05-19 09:02:35',
    status: 'success',
    duration: 155000,
    result: '浠ｇ爜璐ㄩ噺妫€鏌ュ畬鎴愶紝鍙戠幇 3 涓缓璁敼杩涚偣锛? 涓弗閲嶉棶棰?,
  },
  {
    id: 'ex-2',
    taskId: 'st-1',
    startTime: '2026-05-18 09:00:00',
    endTime: '2026-05-18 09:01:48',
    status: 'success',
    duration: 108000,
    result: '浠ｇ爜璐ㄩ噺妫€鏌ュ畬鎴愶紝鏃犻棶棰?,
  },
  {
    id: 'ex-3',
    taskId: 'st-4',
    startTime: '2026-05-19 20:00:00',
    endTime: '2026-05-19 20:00:12',
    status: 'failed',
    duration: 12000,
    result: '鐩戞帶鏁版嵁閲囬泦澶辫触锛氳繛鎺ヨ秴鏃?,
  },
]

let tasksData: ScheduledTaskDto[] = [...defaultTasks]
let templatesData: TaskTemplateDto[] = [...defaultTemplates]
let executionsData: ExecutionRecordDto[] = [...defaultExecutions]

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => window.setTimeout(resolve, ms))
}

/** 鑾峰彇鎵€鏈夊畾鏃朵换鍔?*/
export async function getScheduledTasks(): Promise<ScheduledTaskDto[]> {
  await wait(200)
  return [...tasksData]
}

/** 鏍规嵁 ID 鑾峰彇鍗曚釜瀹氭椂浠诲姟 */
export async function getScheduledTask(id: string): Promise<ScheduledTaskDto | null> {
  await wait(100)
  return tasksData.find((t) => t.id === id) ?? null
}

/** 鍒涘缓鏂板畾鏃朵换鍔?*/
export async function createScheduledTask(dto: CreateScheduledTaskDto): Promise<ScheduledTaskDto> {
  await wait(300)
  const now = new Date().toLocaleString('zh-CN', { hour12: false })
  const task: ScheduledTaskDto = {
    id: `st-${Date.now()}`,
    ...dto,
    agentName: getAgentName(dto.agentId),
    status: 'pending',
    nextRunTime: calculateNextRunTime(dto),
    lastRunTime: '',
    totalRuns: 0,
    successRuns: 0,
    failedRuns: 0,
    enabled: dto.enabled,
    createdAt: now,
    updatedAt: now,
  }
  tasksData.push(task)
  return task
}

/** 鏇存柊瀹氭椂浠诲姟 */
export async function updateScheduledTask(dto: UpdateScheduledTaskDto): Promise<ScheduledTaskDto | null> {
  await wait(300)
  const index = tasksData.findIndex((t) => t.id === dto.id)
  if (index === -1) return null
  const existing = tasksData[index]
  if (!existing) return null
  const updated: ScheduledTaskDto = {
    ...existing,
    ...dto,
    agentName: dto.agentId ? getAgentName(dto.agentId) : existing.agentName,
    updatedAt: new Date().toLocaleString('zh-CN', { hour12: false }),
  }
  if (dto.triggerType || dto.cronExpression || dto.intervalMs || dto.onceTime) {
    updated.nextRunTime = calculateNextRunTime(updated)
  }
  tasksData[index] = updated
  return updated
}

/** 鍒犻櫎瀹氭椂浠诲姟 */
export async function deleteScheduledTask(id: string): Promise<boolean> {
  await wait(200)
  const index = tasksData.findIndex((t) => t.id === id)
  if (index === -1) return false
  tasksData.splice(index, 1)
  executionsData = executionsData.filter((e) => e.taskId !== id)
  return true
}

/** 鑾峰彇浠诲姟鎵ц璁板綍 */
export async function getExecutionRecords(taskId: string): Promise<ExecutionRecordDto[]> {
  await wait(150)
  return executionsData.filter((e) => e.taskId === taskId)
}

/** 鎵嬪姩瑙﹀彂浠诲姟鎵ц */
export async function triggerTask(taskId: string): Promise<ExecutionRecordDto> {
  await wait(500)
  const task = tasksData.find((t) => t.id === taskId)
  if (!task) throw new Error('浠诲姟涓嶅瓨鍦?)

  const record: ExecutionRecordDto = {
    id: `ex-${Date.now()}`,
    taskId,
    startTime: new Date().toLocaleString('zh-CN', { hour12: false }),
    endTime: '',
    status: 'running',
    duration: 0,
    result: '鎵ц涓?..',
  }
  executionsData.unshift(record)
  return record
}

/** 鍒囨崲浠诲姟鍚敤鐘舵€?*/
export async function toggleTaskEnabled(id: string): Promise<ScheduledTaskDto | null> {
  await wait(100)
  const task = tasksData.find((t) => t.id === id)
  if (!task) return null
  task.enabled = !task.enabled
  task.status = task.enabled ? 'pending' : 'disabled'
  task.updatedAt = new Date().toLocaleString('zh-CN', { hour12: false })
  return { ...task }
}

/** 鏍规嵁 Agent ID 鑾峰彇 Agent 鍚嶇О */
function getAgentName(agentId: string): string {
  const agentNames: Record<string, string> = {
    'agent-1': '浠ｇ爜瀹℃煡鍔╂墜',
    'agent-2': '鏋舵瀯璁捐涓撳',
    'agent-3': '鏁版嵁搴撲笓瀹?,
    'agent-4': '鍓嶇寮€鍙戝姪鎵?,
    'agent-5': '鏂囨。鎾板啓鍔╂墜',
  }
  return agentNames[agentId] ?? '鏈煡 Agent'
}

/** 鏍规嵁瑙﹀彂閰嶇疆璁＄畻涓嬫鎵ц鏃堕棿 */
function calculateNextRunTime(task: CreateScheduledTaskDto | ScheduledTaskDto): string {
  const now = new Date()
  if (task.triggerType === 'once') {
    return task.onceTime || ''
  }
  if (task.triggerType === 'interval' && task.intervalMs > 0) {
    const next = new Date(now.getTime() + task.intervalMs)
    return next.toLocaleString('zh-CN', { hour12: false })
  }
  // 瀵逛簬 cron 琛ㄨ揪寮忥紝绠€鍗曡繑鍥?鍗冲皢鎵ц"
  return '寰呰绠?
}

/** 鑾峰彇鎵€鏈変换鍔℃ā鏉?*/
export async function getTaskTemplates(): Promise<TaskTemplateDto[]> {
  await wait(100)
  return [...templatesData]
}

/** 鏍规嵁 ID 鑾峰彇鍗曚釜浠诲姟妯℃澘 */
export async function getTaskTemplate(id: string): Promise<TaskTemplateDto | null> {
  await wait(50)
  return templatesData.find((t) => t.id === id) ?? null
}
