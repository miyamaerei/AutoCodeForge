export interface ChatQuickOptionConfig {
  key: string
  label: string
  prompt: string
}

export type ChatOptionModuleKey = 'task-center' | 'console.ask' | 'console.session'

const chatOptionMap: Record<ChatOptionModuleKey, ChatQuickOptionConfig[]> = {
  'task-center': [
    { key: 'repo', label: '仓库', prompt: '基于当前仓库，请先给出最小改动方案。' },
    { key: 'branch', label: '分支', prompt: '请按当前分支状态给出下一步提交建议。' },
    { key: 'history', label: '历史任务', prompt: '请总结这个任务的历史聊天并给出下一步。' },
  ],
  'console.ask': [
    { key: 'repo', label: '仓库', prompt: '基于当前仓库结构，帮我先做实现方案评估。' },
    { key: 'history', label: '历史会话', prompt: '请总结最近会话的关键上下文。' },
    { key: 'preset', label: '常用指令', prompt: '请给我输出：问题分析 -> 变更清单 -> 风险点。' },
    { key: 'template', label: '提问模板', prompt: '请按标准结构回答：目标、实现步骤、验证方式。' },
  ],
  'console.session': [
    { key: 'repo', label: '仓库', prompt: '请从仓库层面给我一个执行计划。' },
    { key: 'history', label: '历史会话', prompt: '请回顾当前会话上下文并继续。' },
    { key: 'preset', label: '常用指令', prompt: '请给我输出：问题分析 -> 变更清单 -> 风险点。' },
  ],
}

export function getChatOptionsByModule(moduleKey: ChatOptionModuleKey): ChatQuickOptionConfig[] {
  return chatOptionMap[moduleKey] ?? []
}
