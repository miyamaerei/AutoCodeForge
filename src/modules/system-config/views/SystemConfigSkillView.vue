<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'

interface SkillConfigForm {
  skillName: string
  mode: string
  timeoutSeconds: number
  maxRetries: number
  enabled: boolean
  allowWriteFiles: boolean
  allowTerminal: boolean
}

const loading = ref(false)
const error = ref('')
const saving = ref(false)
const formRef = ref<FormInstance>()

const form = reactive<SkillConfigForm>({
  skillName: 'vue3-page-builder',
  mode: 'strict',
  timeoutSeconds: 45,
  maxRetries: 2,
  enabled: true,
  allowWriteFiles: true,
  allowTerminal: false,
})

const hasData = computed(() => true)

const previewRows = computed(() => [
  {
    skill: form.skillName,
    mode: form.mode,
    timeout: `${form.timeoutSeconds}s`,
    retries: form.maxRetries,
    enabled: form.enabled ? 'Yes' : 'No',
  },
])

const rules: FormRules<SkillConfigForm> = {
  skillName: [{ required: true, message: '请选择技能', trigger: 'change' }],
  mode: [{ required: true, message: '请选择模式', trigger: 'change' }],
  timeoutSeconds: [
    { required: true, type: 'number', min: 10, max: 300, message: '范围 10-300 秒', trigger: 'change' },
  ],
  maxRetries: [{ required: true, type: 'number', min: 0, max: 8, message: '范围 0-8', trigger: 'change' }],
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
      ElMessage.success('Skill 配置已保存')
    } finally {
      saving.value = false
    }
  })
}
</script>

<template>
  <section class="settings-page">
    <div class="settings-shell">
      <el-page-header content="Skill" />

      <el-skeleton v-if="loading" :rows="8" animated class="state-block" />
      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" class="state-block" />
      <el-empty v-else-if="!hasData" description="No skill configuration" class="state-block" />

      <el-card v-else shadow="hover" class="main-card">
        <template #header>
          <div class="card-header">
            <strong>Skill Runtime</strong>
            <span class="card-subtitle">技能运行模式、时限和开关配置</span>
          </div>
        </template>

        <el-form ref="formRef" :model="form" :rules="rules" label-width="150px" class="settings-form">
          <el-row :gutter="16">
            <el-col :span="8">
              <el-form-item label="Skill" prop="skillName">
                <el-select v-model="form.skillName">
                  <el-option label="vue3-page-builder" value="vue3-page-builder" />
                  <el-option label="azure-prepare" value="azure-prepare" />
                  <el-option label="azure-deploy" value="azure-deploy" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Mode" prop="mode">
                <el-select v-model="form.mode">
                  <el-option label="strict" value="strict" />
                  <el-option label="balanced" value="balanced" />
                  <el-option label="exploratory" value="exploratory" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Enabled">
                <el-switch v-model="form.enabled" active-text="启用" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-row :gutter="16">
            <el-col :span="8">
              <el-form-item label="Timeout(s)" prop="timeoutSeconds">
                <el-input-number v-model="form.timeoutSeconds" :min="10" :max="300" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Max Retries" prop="maxRetries">
                <el-input-number v-model="form.maxRetries" :min="0" :max="8" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-form-item label="Capability">
            <div class="inline-switches">
              <el-switch v-model="form.allowWriteFiles" active-text="允许文件写入" />
              <el-switch v-model="form.allowTerminal" active-text="允许终端执行" />
            </div>
          </el-form-item>

          <el-table :data="previewRows" stripe border class="preview-table">
            <el-table-column prop="skill" label="Skill" min-width="280" />
            <el-table-column prop="mode" label="Mode" width="160" />
            <el-table-column prop="timeout" label="Timeout" width="160" />
            <el-table-column prop="retries" label="Retries" width="140" />
            <el-table-column prop="enabled" label="Enabled" width="140" />
          </el-table>

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

.inline-switches {
  display: flex;
  gap: 16px;
}

.preview-table {
  margin-top: 8px;
}

.actions {
  margin-top: 12px;
}

@media (max-width: 1365px) {
  .settings-page {
    overflow-x: auto;
    padding-bottom: 8px;
  }
}
</style>
