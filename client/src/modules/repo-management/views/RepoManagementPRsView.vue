<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { useRepoManagementStore } from '../store/useRepoManagementStore'
import { useRepoStore } from '@/stores/useRepoStore'

const store = useRepoManagementStore()
const repoGlobal = useRepoStore()
const { pullRequests, branches, loading, error, hasPullRequests, repositories } = storeToRefs(store)
const { selectedRepositoryId } = storeToRefs(repoGlobal)

const createPrDialogVisible = ref(false)
const createPrSubmitting = ref(false)
const createPrFormRef = ref<FormInstance>()

interface CreatePrFormModel {
  title: string
  description: string
  sourceBranch: string
  targetBranch: string
}

const createPrForm = reactive<CreatePrFormModel>({
  title: '',
  description: '',
  sourceBranch: '',
  targetBranch: '',
})

const createPrRules: FormRules<CreatePrFormModel> = {
  title: [
    { required: true, message: '请输入 PR 标题', trigger: 'blur' },
    { min: 1, max: 200, message: '标题长度需在 1-200 字符内', trigger: 'blur' },
  ],
  description: [{ max: 2000, message: '描述长度不能超过 2000 字符', trigger: 'blur' }],
  sourceBranch: [
    { required: true, message: '请选择源分支', trigger: 'change' },
    { min: 1, max: 100, message: '源分支长度需在 1-100 字符内', trigger: 'change' },
  ],
  targetBranch: [
    { required: true, message: '请选择目标分支', trigger: 'change' },
    { min: 1, max: 100, message: '目标分支长度需在 1-100 字符内', trigger: 'change' },
  ],
}

const branchOptions = computed(() => branches.value.map((branch) => branch.name).filter(Boolean))

const selectedRepositoryDefaultBranch = computed(() => {
  const selectedRepo = repositories.value.find((repo) => repo.id === selectedRepositoryId.value)
  return selectedRepo?.branch?.trim() || 'main'
})

function resetCreatePrForm(): void {
  const fallbackTarget = selectedRepositoryDefaultBranch.value
  const firstBranch = branchOptions.value[0] || ''
  const preferredSource = branchOptions.value.find((branch) => branch !== fallbackTarget) || firstBranch
  createPrForm.title = ''
  createPrForm.description = ''
  createPrForm.sourceBranch = preferredSource || fallbackTarget
  createPrForm.targetBranch = fallbackTarget || firstBranch
}

async function openCreatePrDialog(): Promise<void> {
  if (!selectedRepositoryId.value) {
    ElMessage.warning('请先选择仓库')
    return
  }

  await store.loadBranchesForRepo(selectedRepositoryId.value)
  if (!branchOptions.value.length) {
    ElMessage.warning('当前仓库暂无可用分支，无法创建 PR')
    return
  }

  resetCreatePrForm()
  createPrDialogVisible.value = true
}

function closeCreatePrDialog(): void {
  createPrDialogVisible.value = false
  createPrFormRef.value?.clearValidate()
}

async function submitCreatePr(): Promise<void> {
  if (!createPrFormRef.value) {
    return
  }

  const valid = await createPrFormRef.value.validate().catch(() => false)
  if (!valid || !selectedRepositoryId.value) {
    return
  }

  createPrSubmitting.value = true
  try {
    await store.submitCreatePullRequest(selectedRepositoryId.value, {
      title: createPrForm.title.trim(),
      description: createPrForm.description.trim() || undefined,
      sourceBranch: createPrForm.sourceBranch.trim(),
      targetBranch: createPrForm.targetBranch.trim(),
    })
    ElMessage.success('PR 创建成功')
    closeCreatePrDialog()
  } catch (err) {
    const message = err instanceof Error ? err.message : '创建 PR 失败'
    ElMessage.error(message)
  } finally {
    createPrSubmitting.value = false
  }
}

onMounted(async () => {
  if (!repositories.value || repositories.value.length === 0) {
    await store.loadRepositories()
  }
  if (!hasPullRequests.value) {
    await store.loadPullRequests()
  }
})

watch(selectedRepositoryId, async (newId, oldId) => {
  if (newId && newId !== oldId) {
    await store.loadPullRequests()

    if (createPrDialogVisible.value) {
      await store.loadBranchesForRepo(newId)
      resetCreatePrForm()
    }
  }
})

const statusColorMap = {
  open: 'success',
  merged: 'info',
  closed: 'danger',
}
</script>

<template>
  <section class="prs-view">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <div style="display:flex; align-items:center; gap:12px">
            <span>PR管理</span>
            <el-select v-model="selectedRepositoryId" placeholder="选择仓库" style="min-width:240px">
              <el-option v-for="repo in repositories" :key="repo.id" :label="repo.name" :value="repo.id" />
            </el-select>
          </div>
          <el-button type="primary" @click="openCreatePrDialog">+ 创建PR</el-button>
        </div>
      </template>

      <el-skeleton v-if="loading" :rows="6" animated />

      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />

      <el-empty v-else-if="!hasPullRequests" description="暂无 PR 数据" />

      <el-table v-else :data="pullRequests" stripe>
        <el-table-column prop="id" label="PR ID" width="100" />
        <el-table-column prop="title" label="标题" min-width="300" />
        <el-table-column prop="author" label="作者" width="150" />
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="statusColorMap[row.status as keyof typeof statusColorMap]">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="createdAt" label="创建时间" width="180" />
        <el-table-column label="操作" width="150">
          <template #default="{ row }">
            <el-button link type="primary" size="small">查看</el-button>
            <el-button link type="primary" size="small">评论</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog
      v-model="createPrDialogVisible"
      title="创建 Pull Request"
      width="680px"
      destroy-on-close
      @closed="closeCreatePrDialog"
    >
      <el-form ref="createPrFormRef" :model="createPrForm" :rules="createPrRules" label-width="110px" status-icon>
        <el-form-item label="标题" prop="title">
          <el-input v-model="createPrForm.title" maxlength="200" show-word-limit placeholder="请输入 PR 标题" />
        </el-form-item>

        <el-form-item label="描述" prop="description">
          <el-input
            v-model="createPrForm.description"
            type="textarea"
            :rows="4"
            maxlength="2000"
            show-word-limit
            placeholder="请输入 PR 描述（可选）"
          />
        </el-form-item>

        <el-form-item label="源分支" prop="sourceBranch">
          <el-select v-model="createPrForm.sourceBranch" placeholder="请选择源分支" style="width: 100%">
            <el-option v-for="branch in branchOptions" :key="`source-${branch}`" :label="branch" :value="branch" />
          </el-select>
        </el-form-item>

        <el-form-item label="目标分支" prop="targetBranch">
          <el-select v-model="createPrForm.targetBranch" placeholder="请选择目标分支" style="width: 100%">
            <el-option v-for="branch in branchOptions" :key="`target-${branch}`" :label="branch" :value="branch" />
          </el-select>
        </el-form-item>
      </el-form>

      <template #footer>
        <div class="dialog-footer">
          <el-button @click="closeCreatePrDialog">取消</el-button>
          <el-button type="primary" :loading="createPrSubmitting" @click="submitCreatePr">提交 PR</el-button>
        </div>
      </template>
    </el-dialog>
  </section>
</template>

<style scoped>
.prs-view {
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

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}
</style>
