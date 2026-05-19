import type { RouteRecordRaw } from 'vue-router'

export const agentCenterRoutes: RouteRecordRaw[] = [
  {
    path: '/agent-center',
    name: 'agent.center',
    component: () => import('./views/AgentCenterView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Agent 管理',
    },
  },
]
