<script setup lang="ts">
import { onMounted, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { useRepoManagementStore } from '../store/useRepoManagementStore'
import { useRepoStore } from '@/stores/useRepoStore'

const store = useRepoManagementStore()
const repoGlobal = useRepoStore()
const { branches, loading, error, hasBranches, repositories } = storeToRefs(store)

// bind to global selected repo id from useRepoStore
const { selectedRepositoryId } = repoGlobal

onMounted(async () => {
  if (!repositories.value || repositories.value.length === 0) {
    await store.loadRepositories()
  }
  if (!hasBranches.value) {
    await store.loadBranches()
  }
})

// reload branches when selected repository changes
watch(() => repoGlobal.selectedRepositoryId, async (newId, oldId) => {
  if (newId && newId !== oldId) {
    await store.loadBranches()
  }
})
</script>

<template>
  <section class="branches-view">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <div style="display:flex; align-items:center; gap:12px">
            <span>分支管理</span>
            <el-select v-model="selectedRepositoryId" placeholder="选择仓库" style="min-width:240px">
              <el-option v-for="repo in repositories" :key="repo.id" :label="repo.name" :value="repo.id" />
            </el-select>
          </div>
          <el-button type="primary">+ 新建分支</el-button>
        </div>
      </template>

      <el-skeleton v-if="loading" :rows="6" animated />

      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />

      <el-empty v-else-if="!hasBranches" description="暂无分支数据" />

      <el-table v-else :data="branches" stripe>
        <el-table-column prop="name" label="分支名称" width="250" />
        <el-table-column prop="commit" label="最新提交" width="150" />
        <el-table-column prop="author" label="提交者" width="150" />
        <el-table-column prop="lastUpdate" label="最后更新" width="180" />
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <el-button link type="primary" size="small">查看代码</el-button>
            <el-button link type="primary" size="small">创建PR</el-button>
            <el-button link type="danger" size="small">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </section>
</template>

<style scoped>
.branches-view {
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
