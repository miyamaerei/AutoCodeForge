import type { RouteRecordRaw } from 'vue-router'

export const authRoutes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'auth.login',
    component: () => import('./views/AuthLoginView.vue'),
    meta: {
      requiresAuth: false,
      title: '用户登录',
    },
  },
]
