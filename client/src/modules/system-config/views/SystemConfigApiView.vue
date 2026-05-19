<script setup lang="ts">
import { ref } from 'vue'

interface ApiConfig {
  name: string
  key: string
  secret: string
  status: string
}

const apiConfigs = ref<ApiConfig[]>([
  {
    name: 'OpenAI API',
    key: 'sk-proj-xxxxx',
    secret: '•••••••••••••••',
    status: 'active',
  },
  {
    name: 'GitHub API',
    key: 'ghp_xxxxx',
    secret: '•••••••••••••••',
    status: 'active',
  },
  {
    name: 'Azure API',
    key: 'azure-key-xxxxx',
    secret: '•••••••••••••••',
    status: 'inactive',
  },
])

const showAddDialog = ref(false)
const newApiConfig = ref({ name: '', key: '', secret: '' })

const handleAddApi = () => {
  if (newApiConfig.value.name && newApiConfig.value.key) {
    apiConfigs.value.push({
      ...newApiConfig.value,
      status: 'active',
    })
    newApiConfig.value = { name: '', key: '', secret: '' }
    showAddDialog.value = false
  }
}
</script>

<template>
  <section class="api-config">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>API Key配置</span>
          <el-button type="primary" @click="showAddDialog = true">+ 添加API</el-button>
        </div>
      </template>

      <el-table :data="apiConfigs" stripe>
        <el-table-column prop="name" label="API名称" width="200" />
        <el-table-column prop="key" label="API Key" min-width="300" />
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.status === 'active' ? 'success' : 'info'">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <el-button link type="primary" size="small">编辑</el-button>
            <el-button link type="primary" size="small">测试</el-button>
            <el-button link type="danger" size="small">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="showAddDialog" title="添加API配置">
      <el-form :model="newApiConfig">
        <el-form-item label="API名称">
          <el-input v-model="newApiConfig.name" />
        </el-form-item>
        <el-form-item label="API Key">
          <el-input v-model="newApiConfig.key" />
        </el-form-item>
        <el-form-item label="Secret(可选)">
          <el-input v-model="newApiConfig.secret" type="password" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddDialog = false">取消</el-button>
        <el-button type="primary" @click="handleAddApi">确定</el-button>
      </template>
    </el-dialog>
  </section>
</template>

<style scoped>
.api-config {
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
