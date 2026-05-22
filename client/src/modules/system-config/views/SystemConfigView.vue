<script setup lang="ts">
import { reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { useSystemConfigStore } from '../store/useSystemConfigStore'
import type { ConfigType } from '../api/config.types'

const store = useSystemConfigStore()

const saving = ref(false)
const form = reactive({
  apiKey: '',
  model: 'gpt-4o',
  maxConcurrentTasks: 5,
})

const handleSave = async () => {
  saving.value = true
  try {
    // Save to LLM config (using Llm type instead of ApiKey/Model)
    await store.saveConfig('Llm' as ConfigType, {
      configKey: 'credential.api-key-main',
      configValue: JSON.stringify({
        type: 'credential',
        providerType: 'ApiKey',
        apiKey: form.apiKey,
        description: 'API Key for LLM access',
      }),
      isEncrypted: true,
      description: 'LLM API Key credential',
      group: 'credentials',
    })
    await store.saveConfig('Llm' as ConfigType, {
      configKey: 'model.azureopenai-gpt4o',
      configValue: JSON.stringify({
        type: 'model',
        provider: 'AzureOpenAI',
        modelName: form.model,
        isDefault: true,
        isActive: true,
      }),
      isEncrypted: false,
      description: 'Default LLM model',
      group: 'models',
    })
    await store.saveConfig('Global' as ConfigType, {
      configKey: 'maxConcurrentTasks',
      configValue: String(form.maxConcurrentTasks),
      isEncrypted: false,
      description: 'Maximum concurrent tasks',
    })
    ElMessage.success('配置已保存')
  } catch (err) {
    ElMessage.error(err instanceof Error ? err.message : '保存失败')
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <section class="desktop-page">
    <el-page-header content="系统配置" />
    <el-row :gutter="16" class="desktop-grid">
      <el-col :span="16">
        <el-card class="panel-card" shadow="hover">
          <template #header>
            <strong>模型与密钥配置</strong>
          </template>
          <el-form label-position="top">
            <el-form-item label="API Key">
              <el-input v-model="form.apiKey" placeholder="请输入 API Key" show-password />
            </el-form-item>
            <el-form-item label="模型选择">
              <el-select v-model="form.model" placeholder="请选择模型">
                <el-option label="gpt-4o" value="gpt-4o" />
                <el-option label="gpt-4.1" value="gpt-4.1" />
                <el-option label="o4-mini" value="o4-mini" />
              </el-select>
            </el-form-item>
            <el-form-item label="并发任务上限">
              <el-input-number v-model="form.maxConcurrentTasks" :min="1" :max="20" :step="1" />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" :loading="saving" @click="handleSave">保存配置</el-button>
            </el-form-item>
          </el-form>
        </el-card>
      </el-col>

      <el-col :span="8">
        <el-card class="panel-card" shadow="hover">
          <template #header>
            <strong>配置提示</strong>
          </template>
          <el-alert title="建议为生产与演示环境使用不同 API Key" type="warning" :closable="false" show-icon />
          <el-divider />
          <el-alert title="当前策略: 每 30 分钟轮转一次会话密钥" type="info" :closable="false" show-icon />
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
