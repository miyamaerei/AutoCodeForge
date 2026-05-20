<script setup lang="ts">
import { onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useRepoManagementStore } from '../store/useRepoManagementStore'
import { useRepoStore } from '@/stores/useRepoStore'

const store = useRepoManagementStore()
const repoGlobal = useRepoStore()
const { repositories, loading, error, hasRepositories } = storeToRefs(store)

/**
 * 获取表格行样式类名
 */
function rowClassName({ row }: { row: any }) {
  return row.id === repoGlobal.selectedRepositoryId ? 'is-selected-repo' : ''
}

onMounted(async () => {
  if (!hasRepositories.value) {
    await store.loadRepositories()
  }
})
</script>

<template>
  <section class="repo-list">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>仓库列表</span>
          <el-button type="primary">+ 添加仓库</el-button>
        </div>
      </template>

      <el-skeleton v-if="loading" :rows="6" animated />

      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />

      <el-empty v-else-if="!hasRepositories" description="暂无仓库数据" />

  <el-table v-else :data="repositories" stripe :row-class-name="rowClassName">
        <el-table-column prop="name" label="仓库名称" width="200" />
        <el-table-column prop="url" label="仓库地址" min-width="300" />
        <el-table-column prop="branch" label="默认分支" width="120" />
        <el-table-column prop="lastUpdate" label="最后更新" width="180" />
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <el-button link type="primary" size="small">查看</el-button>
            <el-button link type="primary" size="small" @click="repoGlobal.selectRepository(row.id)">设为当前仓库</el-button>
            <el-button link type="danger" size="small">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </section>
</template>

<style scoped>
.repo-list {
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

.is-selected-repo {
  background: rgba(59, 130, 246, 0.06) !important;
}
</style>
