<script setup lang="ts">
const tasks = [
  { id: 'T-1009', title: '订单导出异常修复', state: '运行中' },
  { id: 'T-1008', title: '会员模块接口重构', state: '已完成' },
  { id: 'T-1007', title: '支付回调重试策略', state: '已暂停' },
]

const steps = ['拉取代码', '修改代码', '编译失败', '自动修复', '编译成功', '提交PR']
const logs = [
  '[14:31:22] clone repository success',
  '[14:31:24] apply patch to OrderService',
  '[14:31:26] build failed: null check missing',
  '[14:31:29] auto-fix applied',
  '[14:31:31] build success',
]
</script>

<template>
  <section class="task-center">
    <el-row :gutter="12" class="three-panel">
      <el-col :span="5" class="pane-col left-pane">
        <el-card class="pane" shadow="never">
          <template #header>任务列表</template>
          <el-scrollbar max-height="560px">
            <el-menu default-active="T-1009">
              <el-menu-item v-for="task in tasks" :key="task.id" :index="task.id">
                {{ task.title }}
                <el-tag size="small" class="task-state">{{ task.state }}</el-tag>
              </el-menu-item>
            </el-menu>
          </el-scrollbar>
        </el-card>
      </el-col>

      <el-col :span="11" class="pane-col center-pane">
        <el-card class="pane" shadow="never">
          <template #header>AI 对话 + 输入</template>
          <el-scrollbar max-height="500px">
            <el-timeline>
              <el-timeline-item type="primary" timestamp="用户">请帮我修复订单导出空指针。</el-timeline-item>
              <el-timeline-item type="success" timestamp="AI">已分析问题，准备修改空值判断与导出逻辑。</el-timeline-item>
              <el-timeline-item type="warning" timestamp="AI">第一次编译失败，正在执行自动修复策略。</el-timeline-item>
            </el-timeline>
          </el-scrollbar>
          <el-input type="textarea" :rows="4" placeholder="输入需求并回车创建任务..." />
          <el-space class="actions">
            <el-button type="primary">创建任务</el-button>
            <el-button>重试</el-button>
            <el-button type="danger" plain>终止</el-button>
          </el-space>
        </el-card>
      </el-col>

      <el-col :span="8" class="pane-col right-pane">
        <el-card class="pane" shadow="never">
          <template #header>执行过程 / 日志 / Diff</template>
          <el-scrollbar max-height="290px">
            <el-steps direction="vertical" :active="4">
              <el-step v-for="step in steps" :key="step" :title="step" />
            </el-steps>
          </el-scrollbar>
          <el-divider />
          <el-scrollbar max-height="120px">
            <p v-for="line in logs" :key="line" class="log-line">{{ line }}</p>
          </el-scrollbar>
          <el-divider />
          <pre class="diff-preview">- return exportOrder(data)
+ return exportOrder(data ?? [])</pre>
        </el-card>
      </el-col>
    </el-row>
  </section>
</template>

<style scoped>
.task-center {
  margin-top: 0;
  min-width: 1320px;
}

.pane {
  min-height: 650px;
}

.pane-col {
  display: flex;
}

.pane-col .pane {
  width: 100%;
}

.left-pane .pane {
  min-width: 250px;
}

.center-pane .pane {
  min-width: 520px;
}

.right-pane .pane {
  min-width: 360px;
}

.task-state {
  margin-left: 8px;
}

.actions {
  margin-top: 12px;
}

.log-line {
  margin: 0 0 4px;
  font-family: Consolas, 'Courier New', monospace;
  font-size: 12px;
}

.diff-preview {
  margin: 0;
  padding: 10px;
  border-radius: 8px;
  background: #0f172a;
  color: #d5e7ff;
  font-family: Consolas, 'Courier New', monospace;
}

@media (max-width: 1365px) {
  .task-center {
    overflow-x: auto;
    padding-bottom: 8px;
  }
}
</style>
