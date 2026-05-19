import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { consoleRoutes } from '../modules/console'
import { dashboardRoutes } from '../modules/dashboard'
import { pipelineCenterRoutes } from '../modules/pipeline-center'
import { repoManagementRoutes } from '../modules/repo-management'
import { systemConfigRoutes } from '../modules/system-config'
import { taskCenterRoutes } from '../modules/task-center'

const routes: RouteRecordRaw[] = [
  ...consoleRoutes,
  ...taskCenterRoutes,
  ...repoManagementRoutes,
  ...dashboardRoutes,
  ...pipelineCenterRoutes,
  ...systemConfigRoutes,
]

export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

router.beforeEach((to) => {
  if (typeof to.meta.title === 'string') {
    document.title = `${to.meta.title} | AutoCodeForge`
  }
})
