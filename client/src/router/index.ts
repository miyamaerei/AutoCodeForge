import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { authRoutes } from '../modules/auth'
import { consoleRoutes } from '../modules/console'
import { dashboardRoutes } from '../modules/dashboard'
import { mdWikiRoutes } from '../modules/md-wiki'
import { pipelineCenterRoutes } from '../modules/pipeline-center'
import { repoManagementRoutes } from '../modules/repo-management'
import { systemConfigRoutes } from '../modules/system-config'
import { taskCenterRoutes } from '../modules/task-center'
import { agentCenterRoutes } from '../modules/agent-center'
import { scheduledTaskRoutes } from '../modules/scheduled-task'

const routes: RouteRecordRaw[] = [
  ...authRoutes,
  ...consoleRoutes,
  ...taskCenterRoutes,
  ...repoManagementRoutes,
  ...dashboardRoutes,
  ...pipelineCenterRoutes,
  ...mdWikiRoutes,
  ...systemConfigRoutes,
  ...agentCenterRoutes,
  ...scheduledTaskRoutes,
]

export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

router.beforeEach((to) => {
  const token = localStorage.getItem('auth_token')

  // Legacy routes currently mark requiresAuth as false. Enforce auth globally except login.
  const requiresAuth = to.path !== '/login'

  if (requiresAuth && !token) {
    return '/login'
  }

  if (to.path === '/login' && token) {
    return '/'
  }

  if (typeof to.meta.title === 'string') {
    document.title = `${to.meta.title} | AutoCodeForge`
  }
})
