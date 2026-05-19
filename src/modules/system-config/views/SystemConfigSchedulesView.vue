<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'

interface ScheduleForm {
  scheduleName: string
  cron: string
  timezone: string
  retryLimit: number
  enabled: boolean
  alertChannel: string
}

const loading = ref(false)
const error = ref('')
const saving = ref(false)
const formRef = ref<FormInstance>()

const form = reactive<ScheduleForm>({
  scheduleName: 'daily-knowledge-sync',
  cron: '0 2 * * *',
  timezone: 'Asia/Shanghai',
  retryLimit: 2,
  enabled: true,
  alertChannel: 'in-app',
})

const hasData = computed(() => true)

const previewRows = computed(() => [
  {
    name: form.scheduleName,
    cron: form.cron,
    timezone: form.timezone,
    status: form.enabled ? 'active' : 'paused',
  },
])

const rules: FormRules<ScheduleForm> = {
  scheduleName: [{ required: true, message: '任务名不能为空', trigger: 'blur' }],
  cron: [
    { required: true, message: 'Cron 不能为空', trigger: 'blur' },
    { pattern: /^\S+\s+\S+\s+\S+\s+\S+\s+\S+$/, message: 'Cron 格式示例: 0 2 * * *', trigger: 'blur' },
  ],
  timezone: [{ required: true, message: '时区不能为空', trigger: 'blur' }],
  retryLimit: [{ required: true, type: 'number', min: 0, max: 10, message: '范围 0-10', trigger: 'change' }],
  alertChannel: [{ required: true, message: '请选择告警通道', trigger: 'change' }],
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
      ElMessage.success('Schedules 配置已保存')
    } finally {
      saving.value = false
    }
  })
}
</script>

<template>
  <section class="settings-page">
    <div class="settings-shell">
      <el-page-header content="Schedules" />

      <el-skeleton v-if="loading" :rows="8" animated class="state-block" />
      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" class="state-block" />
      <el-empty v-else-if="!hasData" description="No schedule found" class="state-block" />

      <el-card v-else shadow="hover" class="main-card">
        <template #header>
          <div class="card-header">
            <strong>Schedule Definitions</strong>
            <span class="card-subtitle">定时任务计划与执行策略</span>
          </div>
        </template>

        <el-form ref="formRef" :model="form" :rules="rules" label-width="150px" class="settings-form">
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="任务名称" prop="scheduleName">
                <el-input v-model="form.scheduleName" />
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="时区" prop="timezone">
                <el-input v-model="form.timezone" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="Cron" prop="cron">
                <el-input v-model="form.cron" placeholder="例如 0 2 * * *" />
              </el-form-item>
            </el-col>
            <el-col :span="6">
              <el-form-item label="失败重试" prop="retryLimit">
                <el-input-number v-model="form.retryLimit" :min="0" :max="10" />
              </el-form-item>
            </el-col>
            <el-col :span="6">
              <el-form-item label="启用状态">
                <el-switch v-model="form.enabled" active-text="启用" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-form-item label="告警通道" prop="alertChannel">
            <el-radio-group v-model="form.alertChannel">
              <el-radio value="in-app">In-App</el-radio>
              <el-radio value="email">Email</el-radio>
              <el-radio value="webhook">Webhook</el-radio>
            </el-radio-group>
          </el-form-item>

          <el-table :data="previewRows" stripe border class="preview-table">
            <el-table-column prop="name" label="Name" min-width="300" />
            <el-table-column prop="cron" label="Cron" width="220" />
            <el-table-column prop="timezone" label="Timezone" width="220" />
            <el-table-column prop="status" label="Status" width="140" />
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
