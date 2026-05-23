import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { shallowMount } from '@vue/test-utils'
import DashboardLargeScreenView from '../views/DashboardLargeScreenView.vue'
import { dashboardApi } from '../api/dashboard.api'

vi.mock('../api/dashboard.api', () => ({
  dashboardApi: {
    getOverview: vi.fn(),
    getPipelineStats: vi.fn(),
    getSystemMetrics: vi.fn(),
    getRecentTasks: vi.fn(),
    getAgentList: vi.fn(),
  },
}))

vi.mock('vue-echarts', () => ({
  default: {
    name: 'VChart',
    template: '<div class="v-chart"></div>',
  },
}))

describe('DashboardLargeScreenView', () => {
  const mockOverview = {
    agentStats: { total: 5, idle: 2, handling: 2, learning: 1, dormant: 0 },
    taskStats: { total: 10, pending: 3, running: 4, completed: 2, failed: 1, paused: 0, canceled: 0 },
    gateStats: { pendingCount: 2, byType: { RequirementConfirm: 1, PlanApproval: 1 } },
    lastUpdated: '2024-01-01T12:00:00Z',
  }

  const mockPipelineStats = [
    { stepType: 'DemandAnalyse', total: 5, pending: 2, running: 2, completed: 1, failed: 0 },
    { stepType: 'Development', total: 3, pending: 1, running: 1, completed: 1, failed: 0 },
  ]

  const mockSystemMetrics = {
    agentCount: 5,
    activeAgents: 4,
    totalLearningHours: 12.5,
    averageLoad: 0.8,
    maxLoad: 2,
    lastHeartbeat: null,
    upTime: '05:30:15',
  }

  const mockAgents = [
    { id: '1', name: 'Agent-1', state: 'Idle', role: 'Worker', currentTaskId: null, createdAtUtc: '2024-01-01T00:00:00Z' },
    { id: '2', name: 'Agent-2', state: 'Handling', role: 'Manager', currentTaskId: 'task-1', createdAtUtc: '2024-01-01T00:00:00Z' },
  ]

  const mockTasks = [
    { id: '1', title: 'Task 1', status: 'Running', progress: 50, createdAtUtc: '2024-01-01T00:00:00Z' },
    { id: '2', title: 'Task 2', status: 'Completed', progress: 100, createdAtUtc: '2024-01-01T00:00:00Z' },
  ]

  beforeEach(() => {
    vi.clearAllMocks()
    vi.mocked(dashboardApi.getOverview).mockResolvedValue({
      data: mockOverview,
      status: 200,
      statusText: 'OK',
      headers: {},
      config: { headers: {} } as any,
    })
    vi.mocked(dashboardApi.getPipelineStats).mockResolvedValue({
      data: mockPipelineStats,
      status: 200,
      statusText: 'OK',
      headers: {},
      config: { headers: {} } as any,
    })
    vi.mocked(dashboardApi.getSystemMetrics).mockResolvedValue({
      data: mockSystemMetrics,
      status: 200,
      statusText: 'OK',
      headers: {},
      config: { headers: {} } as any,
    })
    vi.mocked(dashboardApi.getRecentTasks).mockResolvedValue({
      data: mockTasks as any,
      status: 200,
      statusText: 'OK',
      headers: {},
      config: { headers: {} } as any,
    })
    vi.mocked(dashboardApi.getAgentList).mockResolvedValue({
      data: mockAgents as any,
      status: 200,
      statusText: 'OK',
      headers: {},
      config: { headers: {} } as any,
    })
  })

  afterEach(() => {
    vi.clearAllTimers()
  })

  it('renders the dashboard layout', async () => {
    const wrapper = shallowMount(DashboardLargeScreenView)
    await wrapper.vm.$nextTick()
    
    expect(wrapper.find('.dashboard-screen').exists()).toBe(true)
    expect(wrapper.find('.screen-header').exists()).toBe(true)
    expect(wrapper.find('.screen-main').exists()).toBe(true)
  })

  it('loads data on mount', async () => {
    shallowMount(DashboardLargeScreenView)
    await new Promise(resolve => setTimeout(resolve, 100))
    
    expect(dashboardApi.getOverview).toHaveBeenCalled()
    expect(dashboardApi.getPipelineStats).toHaveBeenCalled()
    expect(dashboardApi.getSystemMetrics).toHaveBeenCalled()
    expect(dashboardApi.getRecentTasks).toHaveBeenCalled()
    expect(dashboardApi.getAgentList).toHaveBeenCalled()
  })

  it('displays agent statistics', async () => {
    const wrapper = shallowMount(DashboardLargeScreenView)
    await new Promise(resolve => setTimeout(resolve, 150))
    
    expect(wrapper.text()).toContain('Agent状态')
    expect(wrapper.text()).toContain('共 5 个')
    expect(wrapper.text()).toContain('空闲')
    expect(wrapper.text()).toContain('处理中')
    expect(wrapper.text()).toContain('学习中')
    expect(wrapper.text()).toContain('休眠')
  })

  it('displays task statistics', async () => {
    const wrapper = shallowMount(DashboardLargeScreenView)
    await new Promise(resolve => setTimeout(resolve, 150))
    
    expect(wrapper.text()).toContain('任务统计')
    expect(wrapper.text()).toContain('共 10 个任务')
    expect(wrapper.text()).toContain('待处理')
    expect(wrapper.text()).toContain('进行中')
    expect(wrapper.text()).toContain('已完成')
  })

  it('displays gate alerts', async () => {
    const wrapper = shallowMount(DashboardLargeScreenView)
    await new Promise(resolve => setTimeout(resolve, 150))
    
    expect(wrapper.text()).toContain('门控告警')
    expect(wrapper.text()).toContain('2 个待处理')
  })

  it('displays system metrics', async () => {
    const wrapper = shallowMount(DashboardLargeScreenView)
    await new Promise(resolve => setTimeout(resolve, 150))
    
    expect(wrapper.text()).toContain('系统指标')
    expect(wrapper.text()).toContain('成功率')
    expect(wrapper.text()).toContain('运行时间')
    expect(wrapper.text()).toContain('学习时长')
    expect(wrapper.text()).toContain('平均负载')
  })

  it('displays agent list', async () => {
    const wrapper = shallowMount(DashboardLargeScreenView)
    await new Promise(resolve => setTimeout(resolve, 150))
    
    expect(wrapper.text()).toContain('Agent列表')
    expect(wrapper.text()).toContain('Agent-1')
    expect(wrapper.text()).toContain('Agent-2')
  })

  it('displays recent tasks', async () => {
    const wrapper = shallowMount(DashboardLargeScreenView)
    await new Promise(resolve => setTimeout(resolve, 150))
    
    expect(wrapper.text()).toContain('最近任务')
    expect(wrapper.text()).toContain('Task 1')
    expect(wrapper.text()).toContain('Task 2')
  })

  it('refreshes data when refresh button is clicked', async () => {
    const wrapper = shallowMount(DashboardLargeScreenView)
    await new Promise(resolve => setTimeout(resolve, 100))
    
    const refreshBtn = wrapper.find('.btn-refresh')
    await refreshBtn.trigger('click')
    
    expect(dashboardApi.getOverview).toHaveBeenCalledTimes(2)
  })

  it('formats uptime correctly', async () => {
    const wrapper = shallowMount(DashboardLargeScreenView)
    await new Promise(resolve => setTimeout(resolve, 150))
    
    expect(wrapper.text()).toContain('05时')
    expect(wrapper.text()).toContain('30分')
    expect(wrapper.text()).toContain('15秒')
  })

  it('calculates success rate correctly', async () => {
    const wrapper = shallowMount(DashboardLargeScreenView)
    await new Promise(resolve => setTimeout(resolve, 150))
    
    expect(wrapper.text()).toContain('67%')
  })

  it('handles empty data gracefully', async () => {
    vi.mocked(dashboardApi.getOverview).mockResolvedValue({
      data: {
        agentStats: { total: 0, idle: 0, handling: 0, learning: 0, dormant: 0 },
        taskStats: { total: 0, pending: 0, running: 0, completed: 0, failed: 0, paused: 0, canceled: 0 },
        gateStats: { pendingCount: 0, byType: {} },
        lastUpdated: '2024-01-01T12:00:00Z',
      },
      status: 200,
      statusText: 'OK',
      headers: {},
      config: { headers: {} } as any,
    })
    
    const wrapper = shallowMount(DashboardLargeScreenView)
    await new Promise(resolve => setTimeout(resolve, 150))
    
    expect(wrapper.find('.dashboard-screen').exists()).toBe(true)
  })

  it('handles API errors gracefully', async () => {
    vi.mocked(dashboardApi.getOverview).mockRejectedValue(new Error('Network error'))
    
    const wrapper = shallowMount(DashboardLargeScreenView)
    await new Promise(resolve => setTimeout(resolve, 150))
    
    expect(wrapper.find('.dashboard-screen').exists()).toBe(true)
  })
})
