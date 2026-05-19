<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'

interface ReviewForm {
  minApprovals: number
  blockOnFailingChecks: boolean
  enforceCodeOwners: boolean
  requiredChecks: string[]
  firstResponseSlaHours: number
  exceptionApproverRole: string
}

const loading = ref(false)
const error = ref('')
const saving = ref(false)
const formRef = ref<FormInstance>()

const form = reactive<ReviewForm>({
  minApprovals: 1,
  blockOnFailingChecks: true,
  enforceCodeOwners: true,
  requiredChecks: ['ci', 'lint'],
  firstResponseSlaHours: 8,
  exceptionApproverRole: 'Tech Lead',
})

const hasData = computed(() => true)

const rules: FormRules<ReviewForm> = {
  minApprovals: [{ required: true, type: 'number', min: 0, max: 5, message: '范围 0-5', trigger: 'change' }],
  requiredChecks: [
    {
      type: 'array',
      required: true,
      min: 1,
      message: '至少选择一个检查项',
      trigger: 'change',
    },
  ],
  firstResponseSlaHours: [
    { required: true, type: 'number', min: 1, max: 72, message: '范围 1-72 小时', trigger: 'change' },
  ],
  exceptionApproverRole: [{ required: true, message: '异常审批角色不能为空', trigger: 'blur' }],
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
      ElMessage.success('Review 配置已保存')
    } finally {
      saving.value = false
    }
  })
}
</script>

<template>
  <section class="settings-page">
    <div class="settings-shell">
      <el-page-header content="Review" />

      <el-skeleton v-if="loading" :rows="8" animated class="state-block" />
      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" class="state-block" />
      <el-empty v-else-if="!hasData" description="No review policy" class="state-block" />

      <el-card v-else shadow="hover" class="main-card">
        <template #header>
          <div class="card-header">
            <strong>Review Policies</strong>
            <span class="card-subtitle">代码评审门禁和质量阈值配置</span>
          </div>
        </template>

        <el-form ref="formRef" :model="form" :rules="rules" label-width="180px" class="settings-form">
          <el-row :gutter="16">
            <el-col :span="8">
              <el-form-item label="最小审批数" prop="minApprovals">
                <el-input-number v-model="form.minApprovals" :min="0" :max="5" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="首响 SLA (小时)" prop="firstResponseSlaHours">
                <el-input-number v-model="form.firstResponseSlaHours" :min="1" :max="72" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-form-item label="质量门禁">
            <div class="inline-switches">
              <el-switch v-model="form.blockOnFailingChecks" active-text="失败检查阻止合并" />
              <el-switch v-model="form.enforceCodeOwners" active-text="强制 Code Owner" />
            </div>
          </el-form-item>

          <el-form-item label="必需检查项" prop="requiredChecks">
            <el-checkbox-group v-model="form.requiredChecks">
              <el-checkbox value="ci">CI</el-checkbox>
              <el-checkbox value="lint">Lint</el-checkbox>
              <el-checkbox value="unit-test">Unit Test</el-checkbox>
              <el-checkbox value="security">Security</el-checkbox>
            </el-checkbox-group>
          </el-form-item>

          <el-form-item label="异常审批角色" prop="exceptionApproverRole">
            <el-input v-model="form.exceptionApproverRole" />
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
