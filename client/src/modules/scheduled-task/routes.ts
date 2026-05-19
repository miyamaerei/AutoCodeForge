import type { RouteRecordRaw } from 'vue-router'

export const scheduledTaskRoutes: RouteRecordRaw[] = [
  {
    path: '/scheduled-task',
    name: 'scheduled-task.list',
    component: () => import('./views/ScheduledTaskView.vue'),
    meta: {
      requiresAuth: false,
      title: '定时任务',
    },
  },
]
