import type { RouteRecordRaw } from 'vue-router'

export const taskCenterRoutes: RouteRecordRaw[] = [
  {
    path: '/task-center',
    name: 'task-center.list',
    component: () => import('./views/TaskCenterListView.vue'),
    meta: {
      requiresAuth: false,
      title: 'AI任务中心',
    },
  },
  {
    path: '/task-center/create',
    name: 'task-center.create',
    component: () => import('./views/TaskCenterCreateView.vue'),
    meta: {
      requiresAuth: false,
      title: '创建任务',
    },
  },
  {
    path: '/task-center/:id',
    name: 'task-center.detail',
    component: () => import('./views/TaskCenterDetailView.vue'),
    meta: {
      requiresAuth: false,
      title: '任务详情',
    },
  },
]
