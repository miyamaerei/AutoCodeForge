<script setup lang="ts">
import { onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useRepoManagementStore } from '../store/useRepoManagementStore'

const store = useRepoManagementStore()
const { pullRequests, loading, error, hasPullRequests } = storeToRefs(store)

onMounted(async () => {
  if (!hasPullRequests.value) {
    await store.loadPullRequests()
  }
})

const statusColorMap = {
  open: 'success',
  merged: 'info',
  closed: 'danger',
}
</script>

<template>
  <section class="prs-view">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>PR管理</span>
          <el-button type="primary">+ 创建PR</el-button>
        </div>
      </template>

      <el-skeleton v-if="loading" :rows="6" animated />

      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />

      <el-empty v-else-if="!hasPullRequests" description="暂无 PR 数据" />

      <el-table v-else :data="pullRequests" stripe>
        <el-table-column prop="id" label="PR ID" width="100" />
        <el-table-column prop="title" label="标题" min-width="300" />
        <el-table-column prop="author" label="作者" width="150" />
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusColorMap[row.status as keyof typeof statusColorMap]">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="180" />
        <el-table-column label="操作" width="150">
          <template #default="{ row }">
            <el-button link type="primary" size="small">查看</el-button>
            <el-button link type="primary" size="small">评论</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </section>
</template>

<style scoped>
.prs-view {
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
