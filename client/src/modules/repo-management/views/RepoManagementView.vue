<template>
  <section class="desktop-page">
    <el-page-header content="项目 / 仓库管理" />
    <el-row :gutter="16" class="desktop-grid">
      <el-col :span="17">
        <el-card shadow="hover" class="panel-card">
          <template #header>
            <strong>仓库总览</strong>
          </template>
          <el-descriptions :column="2" border>
            <el-descriptions-item label="仓库列表">3 个仓库</el-descriptions-item>
            <el-descriptions-item label="默认分支">main</el-descriptions-item>
            <el-descriptions-item label="活跃分支">release / feature/*</el-descriptions-item>
            <el-descriptions-item label="PR列表">最近 12 条变更请求</el-descriptions-item>
            <el-descriptions-item label="代码树">支持目录浏览与快速检索</el-descriptions-item>
            <el-descriptions-item label="最近同步">2 分钟前</el-descriptions-item>
          </el-descriptions>
        </el-card>

        <el-card shadow="hover" class="panel-card">
          <template #header>
            <strong>最近仓库活动</strong>
          </template>
          <el-table :data="[
            { repo: 'core-api', branch: 'main', status: '正常' },
            { repo: 'web-console', branch: 'release/v1.2', status: '有待审PR' },
            { repo: 'ai-worker', branch: 'feature/mock-log', status: '构建中' },
          ]" size="small">
            <el-table-column prop="repo" label="仓库" />
            <el-table-column prop="branch" label="分支" />
            <el-table-column prop="status" label="状态" />
          </el-table>
        </el-card>
      </el-col>

      <el-col :span="7">
        <el-card shadow="hover" class="panel-card">
          <template #header>
            <strong>快捷操作</strong>
          </template>
          <el-space direction="vertical" fill>
            <el-button type="primary" plain>新建仓库连接</el-button>
            <el-button plain>拉取最新分支</el-button>
            <el-button plain>查看全部 PR</el-button>
          </el-space>
        </el-card>

        <el-card shadow="hover" class="panel-card">
          <template #header>
            <strong>告警提醒</strong>
          </template>
          <el-alert title="web-console 分支落后 12 次提交" type="warning" :closable="false" show-icon />
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

@media (max-width: 1365px) {
  .desktop-page {
    overflow-x: auto;
    padding-bottom: 8px;
  }
}
</style>
