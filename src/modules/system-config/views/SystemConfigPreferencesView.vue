<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { useOnboarding } from '../../../composables/useOnboarding'

interface PreferencesForm {
  locale: string
  timezone: string
  theme: 'light' | 'dark' | 'auto'
  landingPage: string
  enableInAppNotice: boolean
  enableEmailNotice: boolean
}

const loading = ref(false)
const error = ref('')
const saving = ref(false)
const formRef = ref<FormInstance>()

const form = reactive<PreferencesForm>({
  locale: 'zh-CN',
  timezone: 'Asia/Shanghai',
  theme: 'light',
  landingPage: '/',
  enableInAppNotice: true,
  enableEmailNotice: false,
})

const hasData = computed(() => true)

const onboarding = useOnboarding()

const handleResetOnboarding = async () => {
  try {
    await ElMessageBox.confirm(
      '确定要重置引导吗？重置后下次登录时将重新显示引导。',
      '重置引导',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
      },
    )
    onboarding.reset()
    ElMessage.success('引导已重置')
  } catch {
    ElMessage.info('已取消重置')
  }
}

const rules: FormRules<PreferencesForm> = {
  locale: [{ required: true, message: '请选择语言', trigger: 'change' }],
  timezone: [
    { required: true, message: '请输入时区', trigger: 'blur' },
    { min: 3, max: 64, message: '时区长度应在 3-64 之间', trigger: 'blur' },
  ],
  theme: [{ required: true, message: '请选择主题', trigger: 'change' }],
  landingPage: [{ required: true, message: '请选择默认入口页', trigger: 'change' }],
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
      ElMessage.success('Preferences 已保存')
    } finally {
      saving.value = false
    }
  })
}
</script>

<template>
  <section class="settings-page">
    <div class="settings-shell">
      <el-page-header content="Preferences" />

      <el-skeleton v-if="loading" :rows="8" animated class="state-block" />
      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" class="state-block" />
      <el-empty v-else-if="!hasData" description="No preference data" class="state-block" />

      <el-card v-else shadow="hover" class="main-card">
        <template #header>
          <div class="card-header">
            <strong>Preference Items</strong>
            <span class="card-subtitle">基础偏好配置，影响全局工作区体验</span>
          </div>
        </template>

        <el-form ref="formRef" :model="form" :rules="rules" label-width="150px" class="settings-form">
          <el-row :gutter="16">
            <el-col :span="12">
              <el-form-item label="语言" prop="locale">
                <el-select v-model="form.locale" placeholder="请选择语言">
                  <el-option label="简体中文" value="zh-CN" />
                  <el-option label="English" value="en-US" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="12">
              <el-form-item label="时区" prop="timezone">
                <el-input v-model="form.timezone" placeholder="例如 Asia/Shanghai" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-form-item label="主题" prop="theme">
            <el-radio-group v-model="form.theme">
              <el-radio value="light">Light</el-radio>
              <el-radio value="dark">Dark</el-radio>
              <el-radio value="auto">Auto</el-radio>
            </el-radio-group>
          </el-form-item>

          <el-form-item label="默认入口" prop="landingPage">
            <el-select v-model="form.landingPage" placeholder="请选择默认入口页">
              <el-option label="控制台首页" value="/" />
              <el-option label="任务中心" value="/task-center" />
              <el-option label="仓库管理" value="/repo-management" />
            </el-select>
          </el-form-item>

          <el-form-item label="通知设置">
            <div class="inline-switches">
              <el-switch v-model="form.enableInAppNotice" active-text="站内通知" />
              <el-switch v-model="form.enableEmailNotice" active-text="邮件通知" />
            </div>
          </el-form-item>

          <div class="actions">
            <el-button type="primary" :loading="saving" @click="handleSave">保存配置</el-button>
            <el-button @click="handleResetOnboarding">重置引导</el-button>
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
  max-width: 960px;
}

.inline-switches {
  display: flex;
  gap: 16px;
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
