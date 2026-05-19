<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'

interface DeepWikiForm {
  workspace: string
  indexName: string
  embeddingModel: string
  topK: number
  metric: string
  retentionDays: number
  autoReindex: boolean
}

const loading = ref(false)
const error = ref('')
const saving = ref(false)
const formRef = ref<FormInstance>()

const form = reactive<DeepWikiForm>({
  workspace: 'AutoCodeForge',
  indexName: 'autocodeforge-main-index',
  embeddingModel: 'text-embedding-3-large',
  topK: 8,
  metric: 'cosine',
  retentionDays: 90,
  autoReindex: true,
})

const hasData = computed(() => true)

const rules: FormRules<DeepWikiForm> = {
  workspace: [{ required: true, message: 'Workspace 不能为空', trigger: 'blur' }],
  indexName: [{ required: true, message: 'Index 名称不能为空', trigger: 'blur' }],
  embeddingModel: [{ required: true, message: '请选择向量模型', trigger: 'change' }],
  topK: [{ required: true, type: 'number', min: 1, max: 50, message: '范围 1-50', trigger: 'change' }],
  metric: [{ required: true, message: '请选择距离度量', trigger: 'change' }],
  retentionDays: [{ required: true, type: 'number', min: 7, max: 365, message: '范围 7-365', trigger: 'change' }],
}

const handleSave = async () => {
  if (!formRef.value) {
    return
  }
  await formRef.value.validate(async (valid) => {
    if (!valid) {
      return
    }
    saving.value = true
    try {
      await new Promise((resolve) => setTimeout(resolve, 350))
      ElMessage.success('DeepWiki 配置已保存')
    } finally {
      saving.value = false
    }
  })
}
</script>

<template>
  <section class="settings-page">
    <div class="settings-shell">
      <el-page-header content="DeepWiki" />

      <el-skeleton v-if="loading" :rows="8" animated class="state-block" />
      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" class="state-block" />
      <el-empty v-else-if="!hasData" description="No DeepWiki profile" class="state-block" />

      <el-card v-else shadow="hover" class="main-card">
        <template #header>
          <div class="card-header">
            <strong>DeepWiki Profiles</strong>
            <span class="card-subtitle">索引策略、向量模型和保留规则</span>
          </div>
        </template>

        <el-form ref="formRef" :model="form" :rules="rules" label-width="150px" class="settings-form">
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="Workspace" prop="workspace">
                <el-input v-model="form.workspace" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="Index Name" prop="indexName">
                <el-input v-model="form.indexName" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-row :gutter="16">
            <el-col :span="8">
              <el-form-item label="Embedding" prop="embeddingModel">
                <el-select v-model="form.embeddingModel">
                  <el-option label="text-embedding-3-large" value="text-embedding-3-large" />
                  <el-option label="text-embedding-3-small" value="text-embedding-3-small" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Top K" prop="topK">
                <el-input-number v-model="form.topK" :min="1" :max="50" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Metric" prop="metric">
                <el-select v-model="form.metric">
                  <el-option label="Cosine" value="cosine" />
                  <el-option label="Dot Product" value="dot" />
                  <el-option label="Euclidean" value="euclidean" />
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>

          <el-row :gutter="16">
            <el-col :span="8">
              <el-form-item label="Retention Days" prop="retentionDays">
                <el-input-number v-model="form.retentionDays" :min="7" :max="365" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Auto Reindex">
                <el-switch v-model="form.autoReindex" active-text="开启" />
              </el-form-item>
            </el-col>
          </el-row>

          <div class="actions">
            <el-button type="primary" :loading="saving" @click="handleSave">保存配置</el-button>
          </div>
        </el-form>
      </el-card>
    </div>
  </section>
</template>

<style scoped>
.settings-page {
  min-width: 1280px;
  padding: 20px 16px 40px;
}

.settings-shell {
  width: 100%;
  max-width: 1180px;
  margin: 0 auto;
}

.state-block {
  margin-top: 0.75rem;
}

.main-card {
  margin-top: 0.75rem;
}

.card-header {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 12px;
}

.card-subtitle {
  color: #64748b;
  font-size: 13px;
}

.settings-form {
  max-width: 1020px;
}

.actions {
  margin-top: 8px;
}

@media (max-width: 1365px) {
  .settings-page {
    overflow-x: auto;
    padding-bottom: 8px;
  }
}
</style>
