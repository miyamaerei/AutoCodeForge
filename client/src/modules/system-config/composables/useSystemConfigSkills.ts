import { computed, reactive, ref } from 'vue'
import { useRepoManagementStore } from '../../repo-management/store/useRepoManagementStore'
import type { RepositoryDto } from '../../repo-management/api/repo-management.api'

export type SkillStatus = 'active' | 'inactive' | 'beta'

export interface SkillItem {
  id: string
  name: string
  description: string
  argumentHint: string
  enabled: boolean
  status: SkillStatus
  tags: string[]
  whenToUse: string[]
  outputTargets: string[]
  repositoryIds: string[]
}

export interface NewSkillForm {
  name: string
  description: string
  argumentHint: string
  tags: string
  whenToUse: string
  outputTargets: string
  repositoryIds: string[]
}

export interface SkillConfigForm {
  [skillId: string]: {
    enabled: boolean
    priority: number
  }
}

export function useSystemConfigSkills() {
  const storageKey = 'system-config.skills.v1'

  const loading = ref(false)
  const error = ref('')
  const saving = ref(false)
  const saveError = ref('')
  const saveSuccess = ref(false)
  const lastSavedAt = ref('')
  const searchKeyword = ref('')
  const selectedSkillId = ref<string>('')

  const skills = ref<SkillItem[]>([])

  const form = reactive<SkillConfigForm>({})

  const hasData = computed(() => skills.value.length > 0)

  const filteredSkills = computed(() => {
    const keyword = searchKeyword.value.trim().toLowerCase()
    if (!keyword) {
      return skills.value
    }
    return skills.value.filter((item) => {
      const haystack = `${item.name} ${item.description} ${item.tags.join(' ')}`.toLowerCase()
      return haystack.includes(keyword)
    })
  })

  const selectedSkill = computed(() =>
    skills.value.find((item) => item.id === selectedSkillId.value) || null,
  )

  const enabledCount = computed(() => skills.value.filter((item) => item.enabled).length)

  const activeCount = computed(() => skills.value.filter((item) => item.status === 'active').length)

  const statusTagType = (status: SkillStatus) => {
    if (status === 'active') {
      return 'success'
    }
    if (status === 'beta') {
      return 'warning'
    }
    return 'info'
  }

  const persistConfig = () => {
    const payload = {
      form: { ...form },
      lastSavedAt: lastSavedAt.value,
      selectedSkillId: selectedSkillId.value,
    }
    localStorage.setItem(storageKey, JSON.stringify(payload))
  }

  const restoreConfig = () => {
    const raw = localStorage.getItem(storageKey)
    if (!raw) {
      return
    }

    const parsed = JSON.parse(raw) as {
      form?: SkillConfigForm
      lastSavedAt?: string
      selectedSkillId?: string
    }

    if (parsed.form) {
      Object.assign(form, parsed.form)
    }
    lastSavedAt.value = parsed.lastSavedAt || ''
    if (parsed.selectedSkillId) {
      selectedSkillId.value = parsed.selectedSkillId
    }
  }

  const loadSkills = async () => {
    loading.value = true
    error.value = ''
    try {
      await new Promise((resolve) => setTimeout(resolve, 200))
      
      skills.value = [
        {
          id: 'vue3-page-builder',
          name: 'Vue3 Page Builder',
          description: 'Create Vue 3 pages in module-first structure. Use for: add list/detail/form pages, add lazy routes with meta.requiresAuth, wire page to Pinia setup store, scaffold page quickly with consistent naming.',
          argumentHint: 'Describe module, page type, and route path (e.g. users list page at /users)',
          enabled: true,
          status: 'active',
          tags: ['vue', 'page', 'routing'],
          whenToUse: [
            'You need a new Vue 3 page quickly',
            'You want consistent page naming and route conventions',
            'You want page scaffolding aligned with src/modules/** rules'
          ],
          outputTargets: [
            'src/modules/<module>/views/<ModulePascal><PageTypePascal>View.vue',
            'src/modules/<module>/routes.ts'
          ],
          repositoryIds: ['repo-1', 'repo-2']
        },
        {
          id: 'vue3-api-model-router-store',
          name: 'Vue3 API + Model + Router + Store',
          description: 'Checklist skill for Vue 3 module scaffolding. Use for: create api/model/router/store with fixed stack (axios + pinia setup store + route meta.requiresAuth) and enforced module-first folder structure.',
          argumentHint: 'Describe module goal and pages (e.g. users list+detail+form)',
          enabled: true,
          status: 'active',
          tags: ['vue', 'api', 'store', 'routing'],
          whenToUse: [
            'You want a fast, repeatable checklist to scaffold a Vue 3 module',
            'You want fixed technical choices instead of per-task architecture debates',
            'You need API, model, router, and store delivered in one pass'
          ],
          outputTargets: [
            'src/modules/<module>/api/<module>.api.ts',
            'src/modules/<module>/api/<module>.types.ts',
            'src/modules/<module>/models/<module>.model.ts',
            'src/modules/<module>/models/<module>.mapper.ts',
            'src/modules/<module>/store/use<Module>Store.ts',
            'src/modules/<module>/routes.ts',
            'src/modules/<module>/views/<Module>ListView.vue',
            'src/modules/<module>/views/<Module>DetailView.vue',
            'src/modules/<module>/views/<Module>FormView.vue',
            'src/modules/<module>/index.ts'
          ],
          repositoryIds: ['repo-1']
        }
      ]

      skills.value.forEach((skill) => {
        if (!form[skill.id]) {
          form[skill.id] = {
            enabled: skill.enabled,
            priority: 0
          }
        } else {
          const skillConfig = form[skill.id]
          if (skillConfig) {
            skill.enabled = skillConfig.enabled
          }
        }
      })

      if (!selectedSkillId.value && skills.value.length > 0) {
        const firstSkill = skills.value[0]
        if (firstSkill) {
          selectedSkillId.value = firstSkill.id
        }
      }

      restoreConfig()
    } catch {
      error.value = '加载技能配置失败，请稍后重试。'
    } finally {
      loading.value = false
    }
  }

  const saveSkills = async () => {
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

  const selectSkill = (id: string) => {
    selectedSkillId.value = id
    persistConfig()
  }

  const toggleSkillEnabled = (id: string, enabled: boolean) => {
    const skill = skills.value.find((s) => s.id === id)
    if (skill) {
      skill.enabled = enabled
    }
    if (!form[id]) {
      form[id] = { enabled, priority: 0 }
    } else {
      form[id].enabled = enabled
    }
    persistConfig()
  }

  const updateSkillPriority = (id: string, priority: number) => {
    if (!form[id]) {
      form[id] = { enabled: true, priority }
    } else {
      form[id].priority = priority
    }
    persistConfig()
  }

  const addSkill = (skillData: NewSkillForm) => {
    const newId = skillData.name.toLowerCase().replace(/\s+/g, '-').replace(/[^a-z0-9-]/g, '')
    const newSkill: SkillItem = {
      id: newId,
      name: skillData.name,
      description: skillData.description,
      argumentHint: skillData.argumentHint,
      enabled: true,
      status: 'beta',
      tags: skillData.tags.split(',').map(tag => tag.trim()).filter(tag => tag),
      whenToUse: skillData.whenToUse.split('\n').map(line => line.trim()).filter(line => line),
      outputTargets: skillData.outputTargets.split('\n').map(line => line.trim()).filter(line => line),
      repositoryIds: skillData.repositoryIds || [],
    }
    skills.value.push(newSkill)
    form[newId] = { enabled: true, priority: 0 }
    selectedSkillId.value = newId
    persistConfig()
    return newId
  }

  const updateSkill = (id: string, skillData: Partial<Omit<SkillItem, 'id' | 'enabled'>>) => {
    const index = skills.value.findIndex(s => s.id === id)
    if (index !== -1) {
      const existing = skills.value[index]
      if (existing) {
        const updated: SkillItem = {
          ...existing,
          ...skillData,
          id: existing.id,
          enabled: existing.enabled
        } as SkillItem
        skills.value[index] = updated
      }
    }
    persistConfig()
  }

  const deleteSkill = (id: string) => {
    const index = skills.value.findIndex(s => s.id === id)
    if (index !== -1) {
      skills.value.splice(index, 1)
      delete form[id]
      if (selectedSkillId.value === id) {
        if (skills.value.length > 0) {
          const firstSkill = skills.value[0]
          if (firstSkill) {
            selectedSkillId.value = firstSkill.id
          }
        } else {
          selectedSkillId.value = ''
        }
      }
      persistConfig()
    }
  }

  const duplicateSkill = (id: string) => {
    const skill = skills.value.find(s => s.id === id)
    if (!skill) return null
    
    const newId = `${id}-copy`
    const duplicatedSkill: SkillItem = {
      ...skill,
      id: newId,
      name: `${skill.name} (Copy)`,
      status: 'beta',
    }
    skills.value.push(duplicatedSkill)
    form[newId] = { enabled: true, priority: 0 }
    selectedSkillId.value = newId
    persistConfig()
    return newId
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

  const getSkillRepositories = (skillId: string): RepositoryDto[] => {
    const skill = skills.value.find(s => s.id === skillId)
    if (!skill) return []
    return skill.repositoryIds
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
    skills,
    filteredSkills,
    selectedSkill,
    enabledCount,
    activeCount,
    form,
    hasData,
    statusTagType,
    loadSkills,
    saveSkills,
    selectSkill,
    toggleSkillEnabled,
    updateSkillPriority,
    addSkill,
    updateSkill,
    deleteSkill,
    duplicateSkill,
    persistConfig,
    repositories,
    repositoriesLoading,
    loadRepositories,
    getRepositoryById,
    getSkillRepositories,
  }
}
