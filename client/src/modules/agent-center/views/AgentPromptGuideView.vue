<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useAgent } from '../composables/useAgent'

const { agents, loading, error, fetchAgents } = useAgent()

const genericPrompt =
  '你是企业研发场景下的通用 Agent。流程：1)先确认目标、范围与验收标准；2)阅读已有代码、接口与可复用组件；3)给出分步计划并标注风险与边界；4)按最小改动执行并记录关键决策；5)完成后输出结果、验证与后续建议。边界：不越权、不泄露密钥和隐私、不执行破坏性命令、未经确认不修改无关文件。质量：优先复用 store/composable，覆盖加载/错误/空状态，并进行基本回归检查。'

const keyMenus = [
  { title: 'AI任务中心', path: '/task-center', reason: '任务入口与执行主线' },
  { title: '流程中心', path: '/workflow-center/requirements', reason: '需求、轮次、任务、流程闭环' },
  { title: '仓库管理', path: '/repo-management', reason: '仓库、分支、PR、同步进度联动' },
  { title: 'Agent中心', path: '/agent-center', reason: 'Agent能力配置与策略管理' },
]

const enabledAgentCount = computed(() => agents.value.filter((item) => item.enabled).length)

onMounted(async () => {
  await fetchAgents()
})

async function copyPromptTemplate() {
  await navigator.clipboard.writeText(genericPrompt)
  ElMessage.success('通用 Agent 提示已复制')
}
</script>

<template>
  <section class="agent-prompt-guide-page">
    <header class="hero">
      <div>
        <p class="hero-subtitle">Agent 执行流程与边界模板</p>
        <h1>通用 Agent 提示中心</h1>
        <p class="hero-desc">
          这是独立页面，用于统一管理通用提示、关键菜单路径和执行边界。新建 Agent 时可直接复用此模板。
        </p>
      </div>
      <el-button type="primary" size="large" @click="copyPromptTemplate">复制通用提示</el-button>
    </header>

    <div class="layout">
      <el-card class="panel" shadow="never">
        <template #header>
          <div class="panel-title">通用提示模板</div>
        </template>

        <el-skeleton v-if="loading" :rows="4" animated />

        <template v-else>
          <el-alert
            v-if="error"
            :title="error"
            type="error"
            show-icon
            :closable="false"
            class="state-block"
          />

          <template v-else-if="agents.length === 0">
            <el-empty description="当前还没有 Agent，建议先创建一个并使用该模板。" class="state-block" />
          </template>

          <template v-else>
            <el-alert
              type="success"
              show-icon
              :closable="false"
              class="state-block"
              :title="`当前共 ${agents.length} 个 Agent，已启用 ${enabledAgentCount} 个`"
            />
          </template>

          <pre class="prompt-block">{{ genericPrompt }}</pre>
        </template>
      </el-card>

      <el-card class="panel" shadow="never">
        <template #header>
          <div class="panel-title">关键菜单高亮说明</div>
        </template>

        <div class="menu-grid">
          <div v-for="menu in keyMenus" :key="menu.path" class="menu-item">
            <div class="menu-name">{{ menu.title }}</div>
            <div class="menu-path">{{ menu.path }}</div>
            <div class="menu-reason">{{ menu.reason }}</div>
          </div>
        </div>
      </el-card>
    </div>
  </section>
</template>

<style scoped>
.agent-prompt-guide-page {
  min-width: 1280px;
  padding: 1rem;
}

.hero {
  border-radius: 16px;
  padding: 1.25rem 1.5rem;
  margin-bottom: 1rem;
  background: linear-gradient(120deg, #fff4e6 0%, #ffe8cc 42%, #ffd8a8 100%);
  border: 1px solid #ffd8a8;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.hero-subtitle {
  font-size: 0.85rem;
  color: #9a3412;
  font-weight: 700;
  letter-spacing: 0.04em;
  margin: 0 0 0.35rem;
}

.hero h1 {
  margin: 0;
  font-size: 1.9rem;
  color: #7c2d12;
}

.hero-desc {
  margin: 0.6rem 0 0;
  color: #9a3412;
}

.layout {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 1rem;
}

.panel {
  border-radius: 12px;
}

.panel-title {
  font-weight: 700;
  color: #1e293b;
}

.state-block {
  margin-bottom: 0.75rem;
}

.prompt-block {
  margin: 0;
  padding: 1rem;
  border-radius: 10px;
  border: 1px solid #fde68a;
  background: #fffbeb;
  color: #78350f;
  font-size: 0.9rem;
  line-height: 1.5;
  white-space: pre-wrap;
}

.menu-grid {
  display: grid;
  gap: 0.75rem;
}

.menu-item {
  border: 1px solid #fcd34d;
  background: linear-gradient(180deg, #fffef5 0%, #fffbeb 100%);
  border-radius: 10px;
  padding: 0.75rem;
}

.menu-name {
  font-weight: 700;
  color: #92400e;
}

.menu-path {
  margin-top: 0.3rem;
  font-family: Consolas, 'Courier New', monospace;
  font-size: 0.8rem;
  color: #b45309;
}

.menu-reason {
  margin-top: 0.4rem;
  color: #78350f;
  font-size: 0.85rem;
}
</style>
