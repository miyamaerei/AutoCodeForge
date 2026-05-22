import type { RouteRecordRaw } from 'vue-router'

export const repoManagementRoutes: RouteRecordRaw[] = [
  {
    path: '/repo-management',
    name: 'repo-management.list',
    component: () => import('./views/RepoManagementListView.vue'),
    meta: {
      requiresAuth: false,
      title: '仓库管理',
    },
  },
  {
    path: '/repo-management/branches',
    name: 'repo-management.branches',
    component: () => import('./views/RepoManagementBranchesView.vue'),
    meta: {
      requiresAuth: false,
      title: '分支管理',
    },
  },
  {
    path: '/repo-management/prs',
    name: 'repo-management.prs',
    component: () => import('./views/RepoManagementPRsView.vue'),
    meta: {
      requiresAuth: false,
      title: 'PR管理',
    },
  },
  {
    path: '/repo-management/sync-progress',
    name: 'repo-management.sync-progress',
    component: () => import('./views/RepoManagementSyncProgressView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Repo同步进度',
    },
  },
]
