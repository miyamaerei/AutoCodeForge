import type { RouteRecordRaw } from 'vue-router'

export const pipelineCenterRoutes: RouteRecordRaw[] = [
  {
    path: '/pipeline-center',
    name: 'pipeline-center.list',
    component: () => import('./views/PipelineCenterListView.vue'),
    meta: {
      requiresAuth: false,
      title: '流水线中心',
    },
  },
  {
    path: '/pipeline-center/builds',
    name: 'pipeline-center.builds',
    component: () => import('./views/PipelineCenterBuildsView.vue'),
    meta: {
      requiresAuth: false,
      title: '构建状态',
    },
  },
]
