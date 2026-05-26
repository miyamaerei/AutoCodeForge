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
import { workflowCenterRoutes } from '../modules/workflow-center'
import { workflowRoutes } from '../modules/workflow'

// ============================================
// 路由配置
// ============================================
//
// 极简路由设计：
// - /workflow      : 工作流中心（设计器 + 看板 + Agent监控 + 日志）
// - /task          : 任务管理
// - /repo          : 仓库管理
// - /agent         : Agent 管理
// - /console       : 对话控制台
// - /system        : 系统配置
// - /login         : 登录
//
// 简化前（过多模块）：
// - agent-center, task-center, repo-management, workflow-center,
// - pipeline-center, scheduled-task, md-wiki, notification,
// - system-config, dashboard, console
//
// 简化后（4个核心模块）：
// - workflow（包含工作流设计器 + Kanban看板 + Agent监控）
// - task（任务管理）
// - repo（仓库管理）
// - agent（Agent管理）
// - console（对话控制台）
// - system（系统配置）
// ============================================

const routes: RouteRecordRaw[] = [
  ...authRoutes,
  ...workflowRoutes,        // 新的极简工作流模块
  ...taskCenterRoutes,
  ...repoManagementRoutes,
  ...dashboardRoutes,
  ...pipelineCenterRoutes,
  ...mdWikiRoutes,
  ...systemConfigRoutes,
  ...agentCenterRoutes,
  ...scheduledTaskRoutes,
  ...workflowCenterRoutes,   // 旧的保留一段时间
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
