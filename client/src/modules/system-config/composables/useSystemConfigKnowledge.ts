import { computed, reactive, ref } from 'vue'
import { useRepoManagementStore } from '../../repo-management/store/useRepoManagementStore'
import type { RepositoryDto } from '../../repo-management/api/repo-management.api'

export type KnowledgeSourceType = 'markdown' | 'remote-wiki' | 'repository' | 'url'

export type RefreshPolicy = 'manual' | 'hourly' | 'daily' | 'weekly'

export type AccessLevel = 'public' | 'internal' | 'restricted'

export interface KnowledgeSource {
  id: string
  name: string
  type: KnowledgeSourceType
  location: string
  contentFormat: string
  refreshPolicy: RefreshPolicy
  chunkSize: number
  chunkOverlap: number
  indexingScope: string[]
  accessLevel: AccessLevel
  enabled: boolean
  lastSyncAt: string | null
  syncStatus: 'synced' | 'syncing' | 'failed'
  freshnessSla: string
  repositoryIds: string[]
}

export interface NewKnowledgeSourceForm {
  name: string
  type: KnowledgeSourceType
  location: string
  contentFormat: string
  refreshPolicy: RefreshPolicy
  chunkSize: number
  chunkOverlap: number
  indexingScope: string
  accessLevel: AccessLevel
  repositoryIds: string[]
}

export function useSystemConfigKnowledge() {
  const storageKey = 'system-config.knowledge.v1'

  const loading = ref(false)
  const error = ref('')
  const saving = ref(false)
  const saveError = ref('')
  const saveSuccess = ref(false)
  const lastSavedAt = ref('')
  const searchKeyword = ref('')
  const selectedSourceId = ref<string>('')

  const sources = ref<KnowledgeSource[]>([])

  const hasData = computed(() => sources.value.length > 0)

  const filteredSources = computed(() => {
    const keyword = searchKeyword.value.trim().toLowerCase()
    if (!keyword) {
      return sources.value
    }
    return sources.value.filter((item) => {
      const haystack = `${item.name} ${item.location} ${item.type}`.toLowerCase()
      return haystack.includes(keyword)
    })
  })

  const selectedSource = computed(() =>
    sources.value.find((item) => item.id === selectedSourceId.value) || null,
  )

  const enabledCount = computed(() => sources.value.filter((item) => item.enabled).length)

  const syncedCount = computed(() => sources.value.filter((item) => item.syncStatus === 'synced').length)

  const statusTagType = (status: string) => {
    if (status === 'synced') {
      return 'success'
    }
    if (status === 'syncing') {
      return 'warning'
    }
    if (status === 'failed') {
      return 'danger'
    }
    return 'info'
  }

  const persistConfig = () => {
    const payload = {
      lastSavedAt: lastSavedAt.value,
      selectedSourceId: selectedSourceId.value,
    }
    localStorage.setItem(storageKey, JSON.stringify(payload))
  }

  const restoreConfig = () => {
    const raw = localStorage.getItem(storageKey)
    if (!raw) {
      return
    }

    const parsed = JSON.parse(raw) as {
      lastSavedAt?: string
      selectedSourceId?: string
    }

    lastSavedAt.value = parsed.lastSavedAt || ''
    if (parsed.selectedSourceId) {
      selectedSourceId.value = parsed.selectedSourceId
    }
  }

  const loadSources = async () => {
    loading.value = true
    error.value = ''
    try {
      await new Promise((resolve) => setTimeout(resolve, 200))

      sources.value = [
        {
          id: 'project-docs',
          name: 'Project Documentation',
          type: 'markdown',
          location: '/docs',
          contentFormat: 'markdown',
          refreshPolicy: 'daily',
          chunkSize: 800,
          chunkOverlap: 120,
          indexingScope: ['main', 'docs/**'],
          accessLevel: 'internal',
          enabled: true,
          lastSyncAt: '2024-01-15 10:30:00',
          syncStatus: 'synced',
          freshnessSla: '24h',
          repositoryIds: ['repo-1', 'repo-2'],
        },
        {
          id: 'api-reference',
          name: 'API Reference',
          type: 'url',
          location: 'https://docs.example.com/api',
          contentFormat: 'docs',
          refreshPolicy: 'hourly',
          chunkSize: 1000,
          chunkOverlap: 150,
          indexingScope: ['v1', 'v2'],
          accessLevel: 'public',
          enabled: true,
          lastSyncAt: '2024-01-15 14:00:00',
          syncStatus: 'synced',
          freshnessSla: '1h',
          repositoryIds: [],
        },
        {
          id: 'internal-wiki',
          name: 'Internal Wiki',
          type: 'remote-wiki',
          location: 'https://wiki.internal.example.com',
          contentFormat: 'wiki',
          refreshPolicy: 'daily',
          chunkSize: 600,
          chunkOverlap: 100,
          indexingScope: ['engineering', 'product'],
          accessLevel: 'restricted',
          enabled: false,
          lastSyncAt: '2024-01-14 09:00:00',
          syncStatus: 'failed',
          freshnessSla: '24h',
          repositoryIds: ['repo-1'],
        },
        {
          id: 'code-repo',
          name: 'Code Repository',
          type: 'repository',
          location: 'github:example/project',
          contentFormat: 'code',
          refreshPolicy: 'weekly',
          chunkSize: 500,
          chunkOverlap: 80,
          indexingScope: ['main', 'src/**/*.ts'],
          accessLevel: 'internal',
          enabled: true,
          lastSyncAt: '2024-01-13 00:00:00',
          syncStatus: 'syncing',
          freshnessSla: '7d',
          repositoryIds: ['repo-2'],
        },
      ]

      if (!selectedSourceId.value && sources.value.length > 0) {
        const firstSource = sources.value[0]
        if (firstSource) {
          selectedSourceId.value = firstSource.id
        }
      }

      restoreConfig()
    } catch {
      error.value = '加载知识源配置失败，请稍后重试。'
    } finally {
      loading.value = false
    }
  }

  const saveSources = async () => {
    saving.value = true
    saveError.value = ''
    saveSuccess.value = false
    try {
      await new Promise((resolve) => setTimeout(resolve, 250))
      lastSavedAt.value = new Date().toLocaleString('zh-CN')
      saveSuccess.value = true
      persistConfig()
    } catch {
      saveError.value = '保存失败，请稍后重试。'
    } finally {
      saving.value = false
    }
  }

  const selectSource = (id: string) => {
    selectedSourceId.value = id
    persistConfig()
  }

  const toggleSourceEnabled = (id: string, enabled: boolean) => {
    const source = sources.value.find((s) => s.id === id)
    if (source) {
      source.enabled = enabled
    }
    persistConfig()
  }

  const addSource = (sourceData: NewKnowledgeSourceForm) => {
    const newId = sourceData.name.toLowerCase().replace(/\s+/g, '-').replace(/[^a-z0-9-]/g, '')
    const newSource: KnowledgeSource = {
      id: newId,
      name: sourceData.name,
      type: sourceData.type,
      location: sourceData.location,
      contentFormat: sourceData.contentFormat,
      refreshPolicy: sourceData.refreshPolicy,
      chunkSize: sourceData.chunkSize,
      chunkOverlap: sourceData.chunkOverlap,
      indexingScope: sourceData.indexingScope.split('\n').map(line => line.trim()).filter(line => line),
      accessLevel: sourceData.accessLevel,
      enabled: true,
      lastSyncAt: null,
      syncStatus: 'synced',
      freshnessSla: sourceData.refreshPolicy === 'hourly' ? '1h' : sourceData.refreshPolicy === 'daily' ? '24h' : sourceData.refreshPolicy === 'weekly' ? '7d' : '-',
      repositoryIds: sourceData.repositoryIds || [],
    }
    sources.value.push(newSource)
    selectedSourceId.value = newId
    persistConfig()
    return newId
  }

  const updateSource = (id: string, sourceData: Partial<Omit<KnowledgeSource, 'id' | 'lastSyncAt' | 'syncStatus'>>) => {
    const index = sources.value.findIndex(s => s.id === id)
    if (index !== -1) {
      const existing = sources.value[index]
      if (existing) {
        const updated: KnowledgeSource = {
          ...existing,
          ...sourceData,
          id: existing.id,
          lastSyncAt: existing.lastSyncAt,
          syncStatus: existing.syncStatus,
        } as KnowledgeSource
        sources.value[index] = updated
      }
    }
    persistConfig()
  }

  const deleteSource = (id: string) => {
    const index = sources.value.findIndex(s => s.id === id)
    if (index !== -1) {
      sources.value.splice(index, 1)
      if (selectedSourceId.value === id) {
        if (sources.value.length > 0) {
          const firstSource = sources.value[0]
          if (firstSource) {
            selectedSourceId.value = firstSource.id
          }
        } else {
          selectedSourceId.value = ''
        }
      }
      persistConfig()
    }
  }

  const refreshSource = async (id: string) => {
    const source = sources.value.find(s => s.id === id)
    if (source) {
      source.syncStatus = 'syncing'
      await new Promise((resolve) => setTimeout(resolve, 1500))
      source.syncStatus = 'synced'
      source.lastSyncAt = new Date().toLocaleString('zh-CN')
      persistConfig()
    }
  }

  const repoStore = useRepoManagementStore()

  const repositories = computed(() => repoStore.repositories)
  const repositoriesLoading = computed(() => repoStore.loading)

  const loadRepositories = async () => {
    if (!repoStore.hasRepositories) {
      await repoStore.loadRepositories()
    }
  }

  const getRepositoryById = (id: string): RepositoryDto | undefined => {
    return repoStore.repositories.find(repo => repo.id === id)
  }

  const getSourceRepositories = (sourceId: string): RepositoryDto[] => {
    const source = sources.value.find(s => s.id === sourceId)
    if (!source) return []
    return source.repositoryIds
      .map(id => getRepositoryById(id))
      .filter((repo): repo is RepositoryDto => repo !== undefined)
  }

  return {
    loading,
    error,
    saving,
    saveError,
    saveSuccess,
    lastSavedAt,
    searchKeyword,
    sources,
    filteredSources,
    selectedSource,
    enabledCount,
    syncedCount,
    hasData,
    statusTagType,
    loadSources,
    saveSources,
    selectSource,
    toggleSourceEnabled,
    addSource,
    updateSource,
    deleteSource,
    refreshSource,
    persistConfig,
    repositories,
    repositoriesLoading,
    loadRepositories,
    getRepositoryById,
    getSourceRepositories,
  }
}
