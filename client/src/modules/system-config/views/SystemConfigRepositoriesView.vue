<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'

interface RepositoryConfigForm {
  provider: string
  owner: string
  repositoryName: string
  defaultBranch: string
  authMode: string
  mergeStrategies: string[]
  requireChecks: boolean
}

const loading = ref(false)
const error = ref('')
const saving = ref(false)
const formRef = ref<FormInstance>()

const form = reactive<RepositoryConfigForm>({
  provider: 'github',
  owner: 'miyamaerei',
  repositoryName: 'AutoCodeForge',
  defaultBranch: 'main',
  authMode: 'token',
  mergeStrategies: ['squash', 'merge'],
  requireChecks: true,
})

const repositoryPreview = computed(() => [
  {
    fullName: `${form.owner}/${form.repositoryName}`,
    provider: form.provider,
    defaultBranch: form.defaultBranch,
    authMode: form.authMode,
  },
])

const hasData = computed(() => true)

const rules: FormRules<RepositoryConfigForm> = {
  provider: [{ required: true, message: '请选择代码托管平台', trigger: 'change' }],
  owner: [{ required: true, message: 'Owner 不能为空', trigger: 'blur' }],
  repositoryName: [{ required: true, message: '仓库名不能为空', trigger: 'blur' }],
  defaultBranch: [{ required: true, message: '默认分支不能为空', trigger: 'blur' }],
  authMode: [{ required: true, message: '请选择鉴权方式', trigger: 'change' }],
  mergeStrategies: [
    {
      type: 'array',
      required: true,
      min: 1,
      message: '至少选择一个合并策略',
      trigger: 'change',
    },
  ],
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
      ElMessage.success('Repositories 配置已保存')
    } finally {
      saving.value = false
    }
  })
}
</script>

<template>
  <section class="settings-page">
    <div class="settings-shell">
      <el-page-header content="Repositories" />

      <el-skeleton v-if="loading" :rows="8" animated class="state-block" />
      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" class="state-block" />
      <el-empty v-else-if="!hasData" description="No repository configuration" class="state-block" />

      <el-card v-else shadow="hover" class="main-card">
        <template #header>
          <div class="card-header">
            <strong>Repository Integrations</strong>
            <span class="card-subtitle">仓库接入、分支策略与权限配置</span>
          </div>
        </template>

        <el-form ref="formRef" :model="form" :rules="rules" label-width="150px" class="settings-form">
          <el-row :gutter="16">
            <el-col :span="8">
              <el-form-item label="Provider" prop="provider">
                <el-select v-model="form.provider">
                  <el-option label="GitHub" value="github" />
                  <el-option label="Azure DevOps" value="azure-devops" />
                  <el-option label="GitLab" value="gitlab" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Owner" prop="owner">
                <el-input v-model="form.owner" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="Repository" prop="repositoryName">
                <el-input v-model="form.repositoryName" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-row :gutter="16">
            <el-col :span="8">
              <el-form-item label="默认分支" prop="defaultBranch">
                <el-input v-model="form.defaultBranch" />
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="鉴权方式" prop="authMode">
                <el-select v-model="form.authMode">
                  <el-option label="Token" value="token" />
                  <el-option label="App" value="app" />
                  <el-option label="OAuth" value="oauth" />
                </el-select>
              </el-form-item>
            </el-col>
            <el-col :span="8">
              <el-form-item label="检查门禁">
                <el-switch v-model="form.requireChecks" active-text="启用" />
              </el-form-item>
            </el-col>
          </el-row>

          <el-form-item label="合并策略" prop="mergeStrategies">
            <el-checkbox-group v-model="form.mergeStrategies">
              <el-checkbox value="merge">Merge</el-checkbox>
              <el-checkbox value="squash">Squash</el-checkbox>
              <el-checkbox value="rebase">Rebase</el-checkbox>
            </el-checkbox-group>
          </el-form-item>

          <el-table :data="repositoryPreview" stripe border class="preview-table">
            <el-table-column prop="fullName" label="Preview Repository" min-width="320" />
            <el-table-column prop="provider" label="Provider" width="180" />
            <el-table-column prop="defaultBranch" label="Branch" width="180" />
            <el-table-column prop="authMode" label="Auth" width="150" />
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
