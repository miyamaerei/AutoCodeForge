<script setup lang="ts">
import { onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { usePipelineCenterStore } from '../store/usePipelineCenterStore'

const store = usePipelineCenterStore()
const { builds, loading, error, hasBuilds } = storeToRefs(store)

onMounted(async () => {
  if (!hasBuilds.value) {
    await store.loadBuilds()
  }
})

const statusColorMap = {
  success: 'success',
  running: 'warning',
  failed: 'danger',
}
</script>

<template>
  <section class="builds-view">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>构建状态</span>
        </div>
      </template>

      <el-skeleton v-if="loading" :rows="6" animated />

      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />

      <el-empty v-else-if="!hasBuilds" description="暂无构建数据" />

      <el-table v-else :data="builds" stripe>
        <el-table-column prop="pipelineName" label="流水线" min-width="250" />
        <el-table-column prop="buildNumber" label="构建号" width="100" />
        <el-table-column prop="branch" label="分支" width="150" />
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusColorMap[row.status as keyof typeof statusColorMap]">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="startTime" label="开始时间" width="180" />
        <el-table-column prop="duration" label="耗时" width="100" />
        <el-table-column label="操作" width="150">
          <template #default="{ row }">
            <el-button link type="primary" size="small">查看日志</el-button>
            <el-button link type="primary" size="small">重新运行</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </section>
</template>

<style scoped>
.builds-view {
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
