import type { RouteRecordRaw } from 'vue-router'

export const agentCenterRoutes: RouteRecordRaw[] = [
  {
    path: '/agent-center',
    name: 'agent.list',
    component: () => import('./views/AgentCenterView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Agent 管理',
    },
  },
  {
    path: '/agent-center/prompt-guide',
    name: 'agent.prompt',
    component: () => import('./views/AgentPromptGuideView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Agent 通用提示',
    },
  },
  {
    path: '/agent-center/:id',
    name: 'agent.detail',
    component: () => import('./views/AgentCenterView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Agent 详情',
    },
  },
  {
    path: '/agent-center/state/:state',
    name: 'agent.state',
    component: () => import('./views/AgentCenterView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Agent 状态',
    },
  },
]
