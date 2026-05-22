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
]
