import { describe, it, expect, vi, beforeEach, afterEach, beforeAll, afterAll } from 'vitest'
import { shallowMount } from '@vue/test-utils'
import DashboardLargeScreenView from '../views/DashboardLargeScreenView.vue'
import { dashboardApi } from '../api/dashboard.api'

const originalResizeObserver = globalThis.ResizeObserver

class ResizeObserverMock implements ResizeObserver {
  observe(): void {}

  unobserve(): void {}

  disconnect(): void {}
}

vi.mock('../api/dashboard.api', () => ({
  dashboardApi: {
    getSnapshot: vi.fn(),
    getTasks: vi.fn(),
    getAgents: vi.fn(),
    getLogs: vi.fn(),
    connectLiveStream: vi.fn(() => () => {}),
    connectLogsStream: vi.fn(() => () => {}),
  },
}))

describe('DashboardLargeScreenView', () => {
  const mockSnapshot = {
    agentStats: { total: 5, idle: 2, handling: 2, learning: 1, dormant: 0 },
    taskStats: { total: 10, pending: 3, running: 4, completed: 2, failed: 1, paused: 0, canceled: 0 },
    gateStats: { pendingCount: 2, byType: { RequirementConfirm: 1, PlanApproval: 1 } },
    lastUpdated: '2024-01-01T12:00:00Z',
  }

  const mockTasks = [
    {
      id: 'task-1',
      title: 'Task 1',
      status: 'Running',
      progress: 50,
      currentStep: 4,
      currentStepName: '代码开发',
      agentId: 'agent-1',
      agentName: 'Agent-1',
      errorMessage: null,
      isTimeout: false,
      hasRejectedGate: false,
      hasEmergencyGate: false,
      alertTags: [],
      alertLevel: 'normal' as const,
      updatedAtUtc: '2024-01-01T00:00:00Z',
    },
    {
      id: 'task-2',
      title: 'Task 2',
      status: 'Completed',
      progress: 100,
      currentStep: 7,
      currentStepName: '最终审核',
      agentId: 'agent-2',
      agentName: 'Agent-2',
      errorMessage: null,
      isTimeout: false,
      hasRejectedGate: false,
      hasEmergencyGate: false,
      alertTags: [],
      alertLevel: 'normal' as const,
      updatedAtUtc: '2024-01-01T00:00:00Z',
    },
  ]

  const mockAgents = [
    {
      id: 'agent-1',
      name: 'Agent-1',
      role: 'Worker',
      state: 'Idle',
      workload: 1,
      workstationStep: 4,
      workstationName: '代码开发',
      currentTaskId: null,
      dormantReason: null,
      updatedAtUtc: '2024-01-01T00:00:00Z',
    },
  ]

  const mockLogs = [
    {
      time: '2024-01-01T00:00:00Z',
      type: 'system' as const,
      taskId: null,
      agentId: null,
      content: '系统告警测试',
      level: 'info' as const,
    },
  ]

  beforeAll(() => {
    vi.stubGlobal('ResizeObserver', ResizeObserverMock)
  })

  afterAll(() => {
    if (originalResizeObserver) {
      vi.stubGlobal('ResizeObserver', originalResizeObserver)
      return
    }
    vi.unstubAllGlobals()
  })

  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(dashboardApi.getSnapshot).mockResolvedValue(mockSnapshot)
    vi.mocked(dashboardApi.getTasks).mockResolvedValue(mockTasks)
    vi.mocked(dashboardApi.getAgents).mockResolvedValue(mockAgents)
    vi.mocked(dashboardApi.getLogs).mockResolvedValue(mockLogs)
    vi.mocked(dashboardApi.connectLiveStream).mockImplementation((options) => {
      options.onOpen?.()
      options.onLive({
        snapshot: mockSnapshot,
        tasks: mockTasks,
        agents: mockAgents,
        generatedAtUtc: '2024-01-01T00:00:00Z',
      })
      return () => {}
    })
    vi.mocked(dashboardApi.connectLogsStream).mockImplementation((options) => {
      options.onOpen?.()
      options.onLogs(mockLogs)
      return () => {}
    })
  })

  afterEach(() => {
    vi.clearAllTimers()
  })

  it('renders the dashboard layout', async () => {
    const wrapper = shallowMount(DashboardLargeScreenView)
    await new Promise((resolve) => setTimeout(resolve, 80))

    expect(wrapper.find('.screen').exists()).toBe(true)
    expect(wrapper.find('.sandbox-panel').exists()).toBe(true)
    expect(wrapper.find('.bottom-grid').exists()).toBe(true)
  })

  it('loads snapshot, tasks, agents and logs', async () => {
    shallowMount(DashboardLargeScreenView)
    await new Promise((resolve) => setTimeout(resolve, 120))

    expect(dashboardApi.connectLiveStream).toHaveBeenCalled()
    expect(dashboardApi.connectLogsStream).toHaveBeenCalled()
  })

  it('displays stats and titles', async () => {
    const wrapper = shallowMount(DashboardLargeScreenView)
    await new Promise((resolve) => setTimeout(resolve, 120))

    expect(wrapper.text()).toContain('多 Agent 流水线仿真大屏（Vue-Konva）')
    expect(wrapper.text()).toContain('任务总数')
    expect(wrapper.text()).toContain('运行中')
    expect(wrapper.text()).toContain('异常数')
    expect(wrapper.text()).toContain('完成数')
    expect(wrapper.text()).toContain('待处理门控')
  })

  it('displays tasks and agents', async () => {
    const wrapper = shallowMount(DashboardLargeScreenView)
    await new Promise((resolve) => setTimeout(resolve, 120))

    expect(wrapper.text()).toContain('Task 1')
    expect(wrapper.text()).toContain('Task 2')
    expect(wrapper.text()).toContain('Agent-1')
  })

  it('switches log tabs', async () => {
    const wrapper = shallowMount(DashboardLargeScreenView)
    await new Promise((resolve) => setTimeout(resolve, 120))

    const buttons = wrapper.findAll('.tabs button')
    expect(buttons.length).toBe(3)

    const firstButton = buttons[0]
    expect(firstButton).toBeDefined()
    if (!firstButton) {
      return
    }

    await firstButton.trigger('click')
    expect(dashboardApi.connectLogsStream).toHaveBeenCalled()
  })
})
