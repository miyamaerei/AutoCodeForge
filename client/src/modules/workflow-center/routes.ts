import type { RouteRecordRaw } from 'vue-router'

export const workflowCenterRoutes: RouteRecordRaw[] = [
  {
    path: '/workflow-center/requirements',
    name: 'workflow-center.requirements',
    component: () => import('./views/WorkflowCenterRequirementView.vue'),
    meta: {
      requiresAuth: true,
      title: '需求中心',
    },
  },
  {
    path: '/workflow-center/rounds',
    name: 'workflow-center.rounds',
    component: () => import('./views/WorkflowCenterRoundView.vue'),
    meta: {
      requiresAuth: true,
      title: '轮次中心',
    },
  },
  {
    path: '/workflow-center/tasks',
    name: 'workflow-center.tasks',
    component: () => import('./views/WorkflowCenterTaskView.vue'),
    meta: {
      requiresAuth: true,
      title: '流程任务',
    },
  },
  {
    path: '/workflow-center/flow',
    name: 'workflow-center.flow',
    component: () => import('./views/WorkflowCenterFlowView.vue'),
    meta: {
      requiresAuth: true,
      title: '流程总览',
    },
  },
  {
    path: '/workflow-center/map',
    name: 'workflow-center.map',
    component: () => import('./views/WorkflowCenterMapView.vue'),
    meta: {
      requiresAuth: true,
      title: '拆分关系图',
    },
  },
  {
    path: '/workflow-center/ops',
    name: 'workflow-center.custom',
    component: () => import('./views/WorkflowCenterCustomView.vue'),
    meta: {
      requiresAuth: true,
      title: '通知审批台',
    },
  },
]
