/**
 * Workflow 模块
 * 极简的 Agent 工作流编排模块
 *
 * 核心概念：
 * - Workflow: 工作流可视化编排（基于 Vue Flow）
 * - Kanban: 任务看板
 * - Agent: AI 执行者
 * - Task: 任务
 * - Repo: 代码仓库
 */

// ============================================
// 导出组件
// ============================================

export { default as WorkflowView } from './views/WorkflowView.vue'
export { default as KanbanBoard } from './components/KanbanBoard.vue'
export { default as WorkflowCanvas } from './components/WorkflowCanvas.vue'

// ============================================
// 导出 Store
// ============================================

export { useWorkflowStore } from './store/useWorkflowStore'
export type {
  Task,
  TaskStatus,
  Agent,
  Repo,
  WorkflowNode,
  WorkflowEdge,
  WorkflowInstance
} from './store/useWorkflowStore'

// ============================================
// 路由配置
// ============================================

import WorkflowView from './views/WorkflowView.vue'

export const workflowRoutes = [
  {
    path: '/workflow',
    name: 'Workflow',
    component: WorkflowView,
    meta: {
      title: '工作流中心',
      icon: '🎯',
      requiresAuth: true
    }
  },
  {
    path: '/workflow/designer',
    name: 'WorkflowDesigner',
    component: WorkflowView,
    props: { defaultView: 'designer' },
    meta: {
      title: '工作流设计器',
      requiresAuth: true
    }
  },
  {
    path: '/workflow/kanban',
    name: 'WorkflowKanban',
    component: WorkflowView,
    props: { defaultView: 'kanban' },
    meta: {
      title: '任务看板',
      requiresAuth: true
    }
  },
  {
    path: '/workflow/agents',
    name: 'WorkflowAgents',
    component: WorkflowView,
    props: { defaultView: 'agents' },
    meta: {
      title: 'Agent 监控',
      requiresAuth: true
    }
  }
]

// ============================================
// 模块注册
// ============================================

/**
 * 注册模块到应用
 * 在 main.ts 或 router/index.ts 中调用
 *
 * @example
 * import { workflowRoutes } from './modules/workflow'
 * router.addRoute(...workflowRoutes)
 */
export function registerWorkflowModule(app: any) {
  // 可以在这里注册全局组件、指令等
  // app.component('WorkflowCanvas', WorkflowCanvas)
  // app.component('KanbanBoard', KanbanBoard)
}

export default {
  routes: workflowRoutes,
  register: registerWorkflowModule
}
