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
]
