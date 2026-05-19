<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'

interface KnowledgeForm {
  sourceName: string
  sourceType: string
  sourcePath: string
  refreshPolicy: string
  chunkSize: number
  chunkOverlap: number
  accessScope: string
}

const loading = ref(false)
const error = ref('')
const saving = ref(false)
const formRef = ref<FormInstance>()

const form = reactive<KnowledgeForm>({
  sourceName: 'project-docs',
  sourceType: 'markdown',
  sourcePath: '/docs',
  refreshPolicy: 'daily',
  chunkSize: 800,
  chunkOverlap: 120,
  accessScope: 'internal',
})

const hasData = computed(() => true)

const rules: FormRules<KnowledgeForm> = {
  sourceName: [{ required: true, message: '来源名称不能为空', trigger: 'blur' }],
  sourceType: [{ required: true, message: '请选择来源类型', trigger: 'change' }],
  sourcePath: [{ required: true, message: '来源路径不能为空', trigger: 'blur' }],
  refreshPolicy: [{ required: true, message: '请选择刷新策略', trigger: 'change' }],
  chunkSize: [{ required: true, type: 'number', min: 100, max: 4000, message: '范围 100-4000', trigger: 'change' }],
  chunkOverlap: [
    { required: true, type: 'number', min: 0, max: 1000, message: '范围 0-1000', trigger: 'change' },
  ],
  accessScope: [{ required: true, message: '请选择访问范围', trigger: 'change' }],
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
      ElMessage.success('Knowledge 配置已保存')
    } finally {
      saving.value = false
    }
  })
}
</script>

<template>
  <section class="settings-page">
    <div class="settings-shell">
      <el-page-header content="Knowledge" />

      <el-skeleton v-if="loading" :rows="8" animated class="state-block" />
      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" class="state-block" />
      <el-empty v-else-if="!hasData" description="No knowledge source" class="state-block" />

      <el-card v-else shadow="hover" class="main-card">
        <template #header>
          <div class="card-header">
            <strong>Knowledge Sources</strong>
            <span class="card-subtitle">知识源接入与刷新策略配置</span>
          </div>
        </template>

        <el-form ref="formRef" :model="form" :rules="rules" label-width="150px" class="settings-form">
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="来源名称" prop="sourceName">
                <el-input v-model="form.sourceName" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="来源类型" prop="sourceType">
                <el-select v-model="form.sourceType">
                  <el-option label="Markdown" value="markdown" />
                  <el-option label="Remote Wiki" value="remote-wiki" />
                  <el-option label="Repository" value="repository" />
                </el-select>
              </el-form-item>
            </el-col>
          </el-row>

          <el-form-item label="来源路径" prop="sourcePath">
            <el-input v-model="form.sourcePath" placeholder="例如 /docs 或 https://example/wiki" />
          </el-form-item>

          <el-row :gutter="16">
            <el-col :span="8">
              <el-form-item label="刷新策略" prop="refreshPolicy">
                <el-select v-model="form.refreshPolicy">
                  <el-option label="Manual" value="manual" />
                  <el-option label="Hourly" value="hourly" />
                  <el-option label="Daily" value="daily" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Chunk Size" prop="chunkSize">
                <el-input-number v-model="form.chunkSize" :min="100" :max="4000" :step="50" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Overlap" prop="chunkOverlap">
                <el-input-number v-model="form.chunkOverlap" :min="0" :max="1000" :step="20" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-form-item label="访问范围" prop="accessScope">
            <el-radio-group v-model="form.accessScope">
              <el-radio value="public">Public</el-radio>
              <el-radio value="internal">Internal</el-radio>
              <el-radio value="restricted">Restricted</el-radio>
            </el-radio-group>
          </el-form-item>

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
