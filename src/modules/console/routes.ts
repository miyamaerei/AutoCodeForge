import type { RouteRecordRaw } from 'vue-router'

export const consoleRoutes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'console.list',
    component: () => import('./views/ConsoleEntryView.vue'),
    meta: {
      requiresAuth: false,
      title: '控制台入口',
    },
  },
  {
    path: '/session',
    name: 'console.session',
    component: () => import('./views/ConsoleSessionView.vue'),
    meta: {
      requiresAuth: false,
      title: '聊天（会话）',
    },
  },
  {
    path: '/ask',
    name: 'console.ask',
    component: () => import('./views/ConsoleAskView.vue'),
    meta: {
      requiresAuth: false,
      title: '聊天（提问）',
    },
  },
  {
    path: '/wiki',
    name: 'console.wiki',
    component: () => import('./views/ConsoleWikiView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Wiki',
    },
  },
  {
    path: '/review',
    name: 'console.review',
    component: () => import('./views/ConsoleReviewView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Review',
    },
  },
  {
    path: '/automations',
    name: 'console.automations',
    component: () => import('./views/ConsoleAutomationsView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Automations',
    },
  },
]
