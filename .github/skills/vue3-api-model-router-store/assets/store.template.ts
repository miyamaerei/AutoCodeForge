import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import {
  create__ModulePascal__,
  delete__ModulePascal__,
  get__ModulePascal__ById,
  list__ModulePascal__,
  update__ModulePascal__,
} from '../api/__module__.api'
import type {
  __ModulePascal__CreateRequestDto,
  __ModulePascal__ListQueryDto,
  __ModulePascal__UpdateRequestDto,
} from '../api/__module__.types'
import type { __ModulePascal__Model } from '../models/__module__.model'
import { dtoToModel } from '../models/__module__.mapper'

export const use__ModulePascal__Store = defineStore('module.__module__', () => {
  const items = ref<__ModulePascal__Model[]>([])
  const currentItem = ref<__ModulePascal__Model | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const hasData = computed(() => items.value.length > 0)

  async function fetchList(query: __ModulePascal__ListQueryDto): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const result = await list__ModulePascal__(query)
      items.value = result.items.map(dtoToModel)
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch list'
    } finally {
      loading.value = false
    }
  }

  async function fetchDetail(id: string): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const dto = await get__ModulePascal__ById(id)
      currentItem.value = dtoToModel(dto)
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch detail'
    } finally {
      loading.value = false
    }
  }

  async function createItem(payload: __ModulePascal__CreateRequestDto): Promise<void> {
    const dto = await create__ModulePascal__(payload)
    items.value.unshift(dtoToModel(dto))
  }

  async function updateItem(id: string, payload: __ModulePascal__UpdateRequestDto): Promise<void> {
    const dto = await update__ModulePascal__(id, payload)
    const updated = dtoToModel(dto)
    items.value = items.value.map((item) => (item.id === id ? updated : item))
    if (currentItem.value?.id === id) {
      currentItem.value = updated
    }
  }

  async function deleteItem(id: string): Promise<void> {
    await delete__ModulePascal__(id)
    items.value = items.value.filter((item) => item.id !== id)
    if (currentItem.value?.id === id) {
      currentItem.value = null
    }
  }

  return {
    items,
    currentItem,
    loading,
    error,
    hasData,
    fetchList,
    fetchDetail,
    createItem,
    updateItem,
    deleteItem,
  }
})
