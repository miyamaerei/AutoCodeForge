<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { ElMessage } from 'element-plus'
import { useRepoManagementStore } from '@/modules/repo-management'
import { useRepoStore } from '@/stores/useRepoStore'

interface Props {
  repositoryId: string
  branch: string
  repositoryLabel?: string
  branchLabel?: string
  repositoryPlaceholder?: string
  branchPlaceholder?: string
  disabled?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  repositoryLabel: '选择仓库',
  branchLabel: '选择分支',
  repositoryPlaceholder: '请选择仓库',
  branchPlaceholder: '请选择分支',
  disabled: false,
})

const emit = defineEmits<{
  (e: 'update:repositoryId', value: string): void
  (e: 'update:branch', value: string): void
}>()

const repoStore = useRepoStore()
const repoManagementStore = useRepoManagementStore()
const { repositories, branches } = storeToRefs(repoManagementStore)

const loadingRepositories = ref(false)
const loadingBranches = ref(false)

const repositoryValue = computed({
  get: () => props.repositoryId,
  set: (value: string) => {
    emit('update:repositoryId', value)
  },
})

const branchValue = computed({
  get: () => props.branch,
  set: (value: string) => {
    emit('update:branch', value)
  },
})

const branchOptions = computed(() => branches.value.map((item) => item.name).filter(Boolean))

const selectedRepositoryDefaultBranch = computed(() => {
  const selectedRepo = repositories.value.find((repo) => repo.id === repositoryValue.value)
  return selectedRepo?.branch?.trim() || 'main'
})

async function loadRepositoriesIfNeeded(): Promise<void> {
  if (repositories.value.length > 0) {
    return
  }

  loadingRepositories.value = true
  try {
    await repoManagementStore.loadRepositories()
  } catch (err) {
    const message = err instanceof Error ? err.message : '加载仓库失败'
    ElMessage.error(message)
  } finally {
    loadingRepositories.value = false
  }
}

function resolveInitialRepositoryId(): string {
  if (repositoryValue.value) {
    return repositoryValue.value
  }

  if (repoStore.selectedRepositoryId) {
    return repoStore.selectedRepositoryId
  }

  return repositories.value[0]?.id || ''
}

async function loadBranchesForRepository(repoId: string): Promise<void> {
  if (!repoId) {
    emit('update:branch', '')
    return
  }

  loadingBranches.value = true
  try {
    await repoManagementStore.loadBranchesForRepo(repoId)

    const currentBranch = branchValue.value?.trim() || ''
    const hasCurrentBranch = currentBranch && branchOptions.value.includes(currentBranch)
    if (!hasCurrentBranch) {
      const fallbackBranch = selectedRepositoryDefaultBranch.value
      const nextBranch = branchOptions.value.includes(fallbackBranch)
        ? fallbackBranch
        : (branchOptions.value[0] || fallbackBranch)
      emit('update:branch', nextBranch)
    }
  } catch (err) {
    const message = err instanceof Error ? err.message : '加载分支失败'
    ElMessage.error(message)
  } finally {
    loadingBranches.value = false
  }
}

onMounted(async () => {
  await loadRepositoriesIfNeeded()

  const initialRepositoryId = resolveInitialRepositoryId()
  if (!initialRepositoryId) {
    return
  }

  if (repositoryValue.value !== initialRepositoryId) {
    emit('update:repositoryId', initialRepositoryId)
  }
  repoStore.selectRepository(initialRepositoryId)
  await loadBranchesForRepository(initialRepositoryId)
})

watch(
  () => repositoryValue.value,
  async (newValue, oldValue) => {
    if (!newValue || newValue === oldValue) {
      return
    }

    repoStore.selectRepository(newValue)
    await loadBranchesForRepository(newValue)
  },
)
</script>

<template>
  <el-form-item :label="repositoryLabel">
    <el-select
      v-model="repositoryValue"
      :placeholder="repositoryPlaceholder"
      :loading="loadingRepositories"
      :disabled="disabled"
      style="width: 100%"
    >
      <el-option v-for="repo in repositories" :key="repo.id" :label="repo.name" :value="repo.id" />
    </el-select>
  </el-form-item>

  <el-form-item :label="branchLabel">
    <el-select
      v-model="branchValue"
      :placeholder="branchPlaceholder"
      :loading="loadingBranches"
      :disabled="disabled || !repositoryValue"
      style="width: 100%"
    >
      <el-option v-for="branchName in branchOptions" :key="branchName" :label="branchName" :value="branchName" />
    </el-select>
  </el-form-item>
</template>
