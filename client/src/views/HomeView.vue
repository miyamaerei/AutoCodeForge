<script setup lang="ts">
const moduleEntries = [
  {
    title: 'AI任务中心',
    desc: '自然语言提需求、多轮对话、执行步骤、日志和Diff展示。',
    type: 'danger',
    status: '核心模块',
  },
  {
    title: '项目/仓库管理',
    desc: '仓库列表、分支、文件树与PR视图。',
    type: 'primary',
    status: 'MVP',
  },
  {
    title: 'Dashboard工作台',
    desc: '今日任务数、成功率、最近任务与告警。',
    type: 'success',
    status: 'MVP',
  },
  {
    title: '流水线中心',
    desc: '构建状态、步骤状态、日志查看（基础版）。',
    type: 'warning',
    status: '基础版',
  },
  {
    title: '系统配置',
    desc: 'API Key、模型选择、用户管理（简化）。',
    type: 'info',
    status: '基础版',
  },
] as const

const recentTasks = [
  { id: 'T-1009', name: '订单导出异常修复', state: '执行中', percent: 58 },
  { id: 'T-1008', name: '会员模块接口重构', state: '成功', percent: 100 },
  { id: 'T-1007', name: '支付回调重试策略', state: '暂停', percent: 71 },
]
</script>

<template>
  <section class="pc-entry">
    <el-alert
      title="AI 自主编程助手平台 · PC端入口"
      type="success"
      description="当前为无后端依赖演示模式，可直接用于MVP讲解与交互演示。"
      :closable="false"
      show-icon
    />

    <el-row :gutter="16" class="stats-row">
      <el-col :span="6">
        <el-statistic title="今日任务" :value="23" />
      </el-col>
      <el-col :span="6">
        <el-statistic title="成功率" :value="92.4" suffix="%" />
      </el-col>
      <el-col :span="6">
        <el-statistic title="执行中" :value="5" />
      </el-col>
      <el-col :span="6">
        <el-statistic title="告警数" :value="2" />
      </el-col>
    </el-row>

    <el-row :gutter="16">
      <el-col :span="16">
        <el-card shadow="hover">
          <template #header>
            <div class="section-title">核心模块入口</div>
          </template>
          <el-row :gutter="12">
            <el-col v-for="item in moduleEntries" :key="item.title" :span="12" class="entry-col">
              <el-card shadow="never" class="entry-card">
                <div class="entry-head">
                  <h3>{{ item.title }}</h3>
                  <el-tag :type="item.type">{{ item.status }}</el-tag>
                </div>
                <p>{{ item.desc }}</p>
                <el-button type="primary" plain size="small">进入模块</el-button>
              </el-card>
            </el-col>
          </el-row>
        </el-card>
      </el-col>

      <el-col :span="8">
        <el-card shadow="hover" class="panel-card">
          <template #header>
            <div class="section-title">最近任务</div>
          </template>
          <div v-for="task in recentTasks" :key="task.id" class="task-row">
            <div class="task-meta">
              <strong>{{ task.name }}</strong>
              <span>{{ task.id }} · {{ task.state }}</span>
            </div>
            <el-progress :percentage="task.percent" :stroke-width="8" />
          </div>
        </el-card>

        <el-card shadow="hover" class="panel-card">
          <template #header>
            <div class="section-title">演示建议流程</div>
          </template>
          <el-timeline>
            <el-timeline-item type="primary" timestamp="Step 1">输入需求并创建任务</el-timeline-item>
            <el-timeline-item type="warning" timestamp="Step 2">观察AI执行步骤和日志</el-timeline-item>
            <el-timeline-item type="success" timestamp="Step 3">查看Diff并确认结果</el-timeline-item>
          </el-timeline>
        </el-card>
      </el-col>
    </el-row>
  </section>
</template>

<style scoped>
.pc-entry {
  margin-top: 1rem;
}

.stats-row {
  margin: 1rem 0;
  padding: 0.75rem;
  border-radius: 12px;
  background: #f8fbff;
  border: 1px solid #dbe7ff;
}

.section-title {
  font-weight: 700;
}

.entry-col {
  margin-bottom: 12px;
}

.entry-card {
  min-height: 170px;
}

.entry-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.entry-head h3 {
  margin: 0;
  font-size: 1rem;
}

.entry-card p {
  min-height: 44px;
  margin: 0 0 12px;
  color: #57627a;
}

.panel-card {
  margin-bottom: 12px;
}

.task-row {
  margin-bottom: 12px;
}

.task-meta {
  display: flex;
  flex-direction: column;
  margin-bottom: 6px;
}

.task-meta span {
  color: #6a7489;
  font-size: 12px;
}

@media (max-width: 1200px) {
  .stats-row :deep(.el-col) {
    margin-bottom: 8px;
  }
}
</style>
