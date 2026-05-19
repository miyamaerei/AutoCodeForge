<template>
  <section class="desktop-page">
    <el-page-header content="流水线中心" />
    <el-row :gutter="16" class="desktop-grid">
      <el-col :span="16">
        <el-card class="panel-card" shadow="hover">
          <template #header>
            <strong>构建执行状态</strong>
          </template>
          <el-steps :active="3" finish-status="success" align-center>
            <el-step title="拉取代码" />
            <el-step title="安装依赖" />
            <el-step title="单元测试" />
            <el-step title="构建产物" />
          </el-steps>
        </el-card>

        <el-card class="panel-card" shadow="hover">
          <template #header>
            <strong>执行日志</strong>
          </template>
          <el-scrollbar max-height="260px">
            <p class="log-line">[14:50:21] checkout branch: release/v1.2</p>
            <p class="log-line">[14:50:33] npm ci completed</p>
            <p class="log-line">[14:50:54] test suite passed</p>
            <p class="log-line">[14:51:07] build artifact generated</p>
          </el-scrollbar>
        </el-card>
      </el-col>

      <el-col :span="8">
        <el-card class="panel-card" shadow="hover">
          <template #header>
            <strong>流水线概况</strong>
          </template>
          <el-descriptions :column="1" border>
            <el-descriptions-item label="成功率">95.2%</el-descriptions-item>
            <el-descriptions-item label="平均耗时">8m 21s</el-descriptions-item>
            <el-descriptions-item label="待处理">2 条</el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-col>
    </el-row>
  </section>
</template>

<style scoped>
.desktop-page {
  margin-top: 0;
  min-width: 1280px;
}

.desktop-grid {
  margin-top: 0.5rem;
}

.panel-card {
  margin-top: 0.75rem;
}

.log-line {
  margin: 0 0 4px;
  font-family: Consolas, 'Courier New', monospace;
  font-size: 12px;
}

@media (max-width: 1365px) {
  .desktop-page {
    overflow-x: auto;
    padding-bottom: 8px;
  }
}
</style>
