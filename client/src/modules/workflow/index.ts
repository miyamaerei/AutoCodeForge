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
  WorkflowInstance,
  WorkflowDefinition,
  WorkflowStatus,
  WorkflowEvent
} from './types/workflow'

// ============================================
// 路由配置
// ============================================
import { workflowRoutes } from './routes'
export { workflowRoutes }

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
