<script setup lang="ts">
import { ref } from 'vue'

interface Model {
  id: string
  name: string
  provider: string
  version: string
  status: string
  maxTokens: number
}

const models = ref<Model[]>([
  {
    id: 'gpt-4',
    name: 'GPT-4',
    provider: 'OpenAI',
    version: '4o',
    status: 'active',
    maxTokens: 128000,
  },
  {
    id: 'gpt-35',
    name: 'GPT-3.5 Turbo',
    provider: 'OpenAI',
    version: '0125',
    status: 'active',
    maxTokens: 4096,
  },
  {
    id: 'claude3',
    name: 'Claude 3 Opus',
    provider: 'Anthropic',
    version: '1.0',
    status: 'inactive',
    maxTokens: 200000,
  },
  {
    id: 'llama2',
    name: 'Llama 2',
    provider: 'Meta',
    version: '70b',
    status: 'inactive',
    maxTokens: 4096,
  },
])

const defaultModel = ref('gpt-4')
</script>

<template>
  <section class="models-config">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>模型选择</span>
        </div>
      </template>

      <el-alert type="info" :closable="false" class="alert-box">
        <span>选择默认模型用于AI任务执行。当前默认模型：<strong>{{ defaultModel }}</strong></span>
      </el-alert>

      <el-table :data="models" stripe style="margin-top: 1rem">
        <el-table-column prop="name" label="模型名称" width="200" />
        <el-table-column prop="provider" label="提供商" width="150" />
        <el-table-column prop="version" label="版本" width="100" />
        <el-table-column prop="maxTokens" label="最大Token" width="120" />
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.status === 'active' ? 'success' : 'info'">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <el-radio v-model="defaultModel" :label="row.id" @change="defaultModel = row.id">
              设为默认
            </el-radio>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </section>
</template>

<style scoped>
.models-config {
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

.alert-box {
  margin-bottom: 1rem;
}
</style>
