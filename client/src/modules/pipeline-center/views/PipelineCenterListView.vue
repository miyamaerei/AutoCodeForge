<script setup lang="ts">
import { onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { usePipelineCenterStore } from '../store/usePipelineCenterStore'

const store = usePipelineCenterStore()
const { pipelines, loading, error, hasPipelines } = storeToRefs(store)

onMounted(async () => {
  if (!hasPipelines.value) {
    await store.loadPipelines()
  }
})

const statusColorMap = {
  success: 'success',
  running: 'warning',
  failed: 'danger',
}
</script>

<template>
  <section class="pipeline-list">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>流水线列表</span>
          <el-button type="primary">+ 新建流水线</el-button>
        </div>
      </template>

      <el-skeleton v-if="loading" :rows="6" animated />

      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />

      <el-empty v-else-if="!hasPipelines" description="暂无流水线数据" />

      <el-table v-else :data="pipelines" stripe>
        <el-table-column prop="name" label="流水线名称" min-width="250" />
        <el-table-column prop="branch" label="分支" width="150" />
        <el-table-column prop="status" label="状态" width="120">
          <template #default="{ row }">
            <el-tag :type="statusColorMap[row.status as keyof typeof statusColorMap]">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="lastRun" label="最后运行" width="180" />
        <el-table-column prop="successRate" label="成功率" width="100" />
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <el-button link type="primary" size="small">查看</el-button>
            <el-button link type="primary" size="small">重新运行</el-button>
            <el-button link type="danger" size="small">配置</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </section>
</template>

<style scoped>
.pipeline-list {
  min-width: 1280px;
}

.content-card {
  margin-bottom: 1rem;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>
