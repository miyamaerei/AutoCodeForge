<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { storeToRefs } from 'pinia'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { fetchGitHubRepositoriesByToken, type RemoteGitRepositoryDto } from '../api/repo-management.api'
import { useRepoManagementStore } from '../store/useRepoManagementStore'
import { useRepoStore } from '@/stores/useRepoStore'
import { useSystemConfigStore } from '../../system-config/store/useSystemConfigStore'

const store = useRepoManagementStore()
const repoGlobal = useRepoStore()
const systemConfigStore = useSystemConfigStore()
const { repositories, loading, error, hasRepositories } = storeToRefs(store)
const addDialogVisible = ref(false)
const submitting = ref(false)
const remoteRepoLoading = ref(false)
const remoteRepoError = ref('')
const tokenHint = ref('')
const selectedRemoteRepo = ref('')
const addFormRef = ref<FormInstance>()
const remoteRepos = ref<RemoteGitRepositoryDto[]>([])

const GITHUB_TOKEN_PATTERN = /^(ghp_|github_pat_|gho_|ghu_|ghs_|ghr_)[A-Za-z0-9_]{10,}$/

const addForm = reactive({
  name: '',
  url: '',
  token: '',
})

const addFormRules: FormRules = {
  name: [
    { required: true, message: '请输入仓库名称', trigger: 'blur' },
    { min: 2, max: 100, message: '仓库名称长度应在 2 到 100 个字符之间', trigger: 'blur' },
  ],
  url: [
    { required: true, message: '请输入仓库地址', trigger: 'blur' },
    { type: 'url', message: '仓库地址格式不正确', trigger: 'blur' },
  ],
  token: [
    { required: true, message: '请输入访问令牌', trigger: 'blur' },
    { min: 8, message: '访问令牌长度至少为 8 位', trigger: 'blur' },
  ],
}

/**
 * 获取表格行样式类名
 */
function rowClassName({ row }: { row: any }) {
  return row.id === repoGlobal.selectedRepositoryId ? 'is-selected-repo' : ''
}

function openAddDialog() {
  addDialogVisible.value = true
}

function resetAddForm() {
  addForm.name = ''
  addForm.url = ''
  addForm.token = ''
  addFormRef.value?.clearValidate()
}

function handleDialogClosed() {
  resetAddForm()
}

function mayBeGitHubToken(value: unknown): string {
  if (typeof value !== 'string') {
    return ''
  }
  const normalized = value.trim()
  if (!normalized || normalized.includes(' ')) {
    return ''
  }
  if (GITHUB_TOKEN_PATTERN.test(normalized)) {
    return normalized
  }
  return normalized.length >= 24 && /[A-Za-z]/.test(normalized) && /\d/.test(normalized) ? normalized : ''
}

function collectStringValues(input: unknown, collector: string[]) {
  if (typeof input === 'string') {
    collector.push(input)
    return
  }

  if (Array.isArray(input)) {
    input.forEach((item) => collectStringValues(item, collector))
    return
  }

  if (input && typeof input === 'object') {
    Object.values(input as Record<string, unknown>).forEach((item) => collectStringValues(item, collector))
  }
}

function findTokenFromConfigValue(rawValue: string): string {
  const direct = mayBeGitHubToken(rawValue)
  if (direct) {
    return direct
  }

  try {
    const parsed = JSON.parse(rawValue)
    const values: string[] = []
    collectStringValues(parsed, values)
    return values.map(mayBeGitHubToken).find(Boolean) || ''
  } catch {
    return ''
  }
}

async function loadTokenFromConfigCenter() {
  try {
    await Promise.all([
      systemConfigStore.loadConfigs('Repository'),
      systemConfigStore.loadConfigs('ApiKey'),
      systemConfigStore.loadConfigs('Integration'),
    ])

    const mergedConfigs = [
      ...systemConfigStore.getConfigs('Repository'),
      ...systemConfigStore.getConfigs('ApiKey'),
      ...systemConfigStore.getConfigs('Integration'),
    ]

    for (const config of mergedConfigs) {
      const token = findTokenFromConfigValue(config.configValue)
      if (token) {
        addForm.token = token
        tokenHint.value = '已自动读取配置中的 Git Token'
        return
      }
    }
  } catch {
    tokenHint.value = ''
  }
}

function applyRemoteRepo(fullName: string) {
  const selected = remoteRepos.value.find((item) => item.fullName === fullName)
  if (!selected) {
    return
  }
  addForm.name = selected.name
  addForm.url = selected.htmlUrl
}

async function fetchRemoteRepos() {
  const token = addForm.token.trim()
  if (!token) {
    remoteRepoError.value = '请先填写或加载 Git Token'
    return
  }

  remoteRepoLoading.value = true
  remoteRepoError.value = ''
  try {
    const repos = await fetchGitHubRepositoriesByToken(token)
    remoteRepos.value = repos
    if (!repos.length) {
      ElMessage.info('未获取到可访问仓库')
      return
    }
    selectedRemoteRepo.value = repos[0].fullName
    applyRemoteRepo(selectedRemoteRepo.value)
    ElMessage.success(`已拉取 ${repos.length} 个仓库`) 
  } catch (err) {
    remoteRepoError.value = err instanceof Error ? err.message : '拉取仓库失败'
    ElMessage.error(remoteRepoError.value)
  } finally {
    remoteRepoLoading.value = false
  }
}

async function submitAddRepository() {
  const form = addFormRef.value
  if (!form) return
  await form.validate()

  submitting.value = true
  try {
    await store.submitCreate({
      name: addForm.name.trim(),
      url: addForm.url.trim(),
      token: addForm.token.trim(),
    })
    ElMessage.success('仓库添加成功')
    addDialogVisible.value = false
  } catch (err) {
    const message = err instanceof Error ? err.message : '仓库添加失败'
    ElMessage.error(message)
  } finally {
    submitting.value = false
  }
}

onMounted(async () => {
  if (!hasRepositories.value) {
    await store.loadRepositories()
  }
  await loadTokenFromConfigCenter()
})
</script>

<template>
  <section class="repo-list">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>仓库列表</span>
          <el-button type="primary" @click="openAddDialog">+ 添加仓库</el-button>
        </div>
      </template>

      <el-skeleton v-if="loading" :rows="6" animated />

      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />

      <el-empty v-else-if="!hasRepositories" description="暂无仓库数据" />

    <el-table v-else :data="repositories" stripe :row-class-name="rowClassName">
        <el-table-column prop="name" label="仓库名称" width="200" />
        <el-table-column prop="url" label="仓库地址" min-width="300" />
        <el-table-column prop="branch" label="默认分支" width="120" />
        <el-table-column prop="lastUpdate" label="最后更新" width="180" />
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <el-button link type="primary" size="small">查看</el-button>
            <el-button link type="primary" size="small" @click="repoGlobal.selectRepository(row.id)">设为当前仓库</el-button>
            <el-button link type="danger" size="small">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog
      v-model="addDialogVisible"
      title="添加仓库"
      width="640px"
      :close-on-click-modal="false"
      @closed="handleDialogClosed"
    >
      <el-form ref="addFormRef" :model="addForm" :rules="addFormRules" label-width="100px" status-icon>
        <el-form-item label="仓库名称" prop="name">
          <el-input v-model="addForm.name" placeholder="例如：AutoCodeForge" />
        </el-form-item>

        <el-form-item label="仓库地址" prop="url">
          <el-input v-model="addForm.url" placeholder="例如：https://github.com/owner/repo" />
        </el-form-item>

        <el-form-item label="访问令牌" prop="token">
          <el-input v-model="addForm.token" type="password" show-password placeholder="输入用于拉取仓库的 Token" />
        </el-form-item>

        <el-form-item label="远程仓库">
          <el-space wrap>
            <el-button :loading="remoteRepoLoading" @click="fetchRemoteRepos">拉取我的 GitHub 仓库</el-button>
            <el-select
              v-model="selectedRemoteRepo"
              filterable
              clearable
              placeholder="选择仓库后自动填充"
              style="min-width: 300px"
              @change="(value) => applyRemoteRepo(String(value || ''))"
            >
              <el-option
                v-for="item in remoteRepos"
                :key="item.id"
                :label="item.fullName"
                :value="item.fullName"
              />
            </el-select>
          </el-space>
        </el-form-item>
      </el-form>

      <el-alert
        v-if="tokenHint"
        :title="tokenHint"
        type="success"
        show-icon
        :closable="false"
      />

      <el-alert
        v-if="remoteRepoError"
        :title="remoteRepoError"
        type="error"
        show-icon
        :closable="false"
      />

      <template #footer>
        <el-space>
          <el-button @click="addDialogVisible = false">取消</el-button>
          <el-button type="primary" :loading="submitting" @click="submitAddRepository">确认添加</el-button>
        </el-space>
      </template>
    </el-dialog>
  </section>
</template>

<style scoped>
.repo-list {
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

.is-selected-repo {
  background: rgba(59, 130, 246, 0.06) !important;
}
</style>
