import type { RouteRecordRaw } from 'vue-router'

export const systemConfigRoutes: RouteRecordRaw[] = [
  {
    path: '/settings',
    redirect: '/settings/preferences',
  },
  {
    path: '/settings/preferences',
    name: 'system-config.preferences',
    component: () => import('./views/SystemConfigPreferencesView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Preferences',
    },
  },
  {
    path: '/settings/repositories',
    name: 'system-config.repositories',
    component: () => import('./views/SystemConfigRepositoriesView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Repositories',
    },
  },
  {
    path: '/settings/knowledge',
    name: 'system-config.knowledge',
    component: () => import('./views/SystemConfigKnowledgeView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Knowledge',
    },
  },
  {
    path: '/settings/skill',
    name: 'system-config.skill',
    component: () => import('./views/SystemConfigSkillView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Skill',
    },
  },
  {
    path: '/settings/schedules',
    name: 'system-config.schedules',
    component: () => import('./views/SystemConfigSchedulesView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Schedules',
    },
  },
  {
    path: '/settings/deepwiki',
    name: 'system-config.deepwiki',
    component: () => import('./views/SystemConfigDeepWikiView.vue'),
    meta: {
      requiresAuth: false,
      title: 'DeepWiki',
    },
  },
  {
    path: '/settings/review',
    name: 'system-config.review',
    component: () => import('./views/SystemConfigReviewView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Review',
    },
  },
  {
    path: '/settings/integrations',
    name: 'system-config.integrations',
    component: () => import('./views/SystemConfigIntegrationsView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Integrations',
    },
  },
  {
    path: '/settings/notifications',
    name: 'system-config.notifications',
    component: () => import('./views/SystemConfigNotificationsView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Notifications',
    },
  },
  {
    path: '/settings/sandbox',
    name: 'system-config.sandbox',
    component: () => import('./views/SystemConfigSandboxView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Sandbox',
    },
  },
  {
    path: '/settings/workflow',
    name: 'system-config.workflow',
    component: () => import('./views/SystemConfigWorkflowView.vue'),
    meta: {
      requiresAuth: false,
      title: 'Workflow',
    },
  },
  {
    path: '/system-config/api',
    name: 'system-config.api',
    component: () => import('./views/SystemConfigApiView.vue'),
    meta: {
      requiresAuth: false,
      title: 'API配置',
    },
  },
  {
    path: '/system-config/models',
    name: 'system-config.models',
    component: () => import('./views/SystemConfigModelsView.vue'),
    meta: {
      requiresAuth: false,
      title: '模型选择',
    },
  },
  {
    path: '/system-config/users',
    name: 'system-config.users',
    component: () => import('./views/SystemConfigUsersView.vue'),
    meta: {
      requiresAuth: false,
      title: '用户管理',
    },
  },
]
