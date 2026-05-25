import type { RouteRecordRaw } from 'vue-router'

export const dashboardRoutes: RouteRecordRaw[] = [
  {
    path: '/dashboard',
    name: 'dashboard.list',
    component: () => import('./views/DashboardView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Dashboard',
    },
  },
  {
    path: '/dashboard/large-screen',
    name: 'dashboard.large-screen',
    component: () => import('./views/DashboardLargeScreenView.vue'),
    meta: {
      requiresAuth: false,
      title: '大屏监控',
      fullscreen: true,
    },
  },
]
