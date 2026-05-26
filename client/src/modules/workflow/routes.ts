import type { RouteRecordRaw } from 'vue-router'

export const workflowRoutes: RouteRecordRaw[] = [
  // 工作流列表
  {
    path: '/workflows',
    name: 'workflow.list',
    component: () => import('./views/WorkflowListView.vue'),
    meta: {
      requiresAuth: false,
      title: '工作流列表',
    },
  },
  {
    path: '/workflows/create',
    name: 'workflow.create',
    component: () => import('./views/WorkflowCreateView.vue'),
    meta: {
      requiresAuth: false,
      title: '创建工作流',
    },
  },
  {
    path: '/workflows/:id',
    name: 'workflow.detail',
    component: () => import('./views/WorkflowDetailView.vue'),
    meta: {
      requiresAuth: false,
      title: '工作流详情',
    },
  },
  // 工作流设计器（独立页面）
  {
    path: '/workflow-designer',
    name: 'workflow.designer',
    component: () => import('./views/WorkflowDesignerView.vue'),
    meta: {
      requiresAuth: false,
      title: '工作流设计器',
    },
  },
  // 看板（独立页面）
  {
    path: '/kanban',
    name: 'workflow.kanban',
    component: () => import('./views/KanbanBoardView.vue'),
    meta: {
      requiresAuth: false,
      title: '任务看板',
    },
  },
  // 工作流实例
  {
    path: '/workflow-instances',
    name: 'workflow.instance.list',
    component: () => import('./views/WorkflowInstanceListView.vue'),
    meta: {
      requiresAuth: false,
      title: '工作流实例',
    },
  },
  {
    path: '/workflow-instances/:id',
    name: 'workflow.instance.detail',
    component: () => import('./views/WorkflowInstanceDetailView.vue'),
    meta: {
      requiresAuth: false,
      title: '实例详情',
    },
  },
]
