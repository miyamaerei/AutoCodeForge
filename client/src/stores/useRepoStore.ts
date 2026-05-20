import { ref } from 'vue'
import { defineStore } from 'pinia'

export const useRepoStore = defineStore('repo', () => {
  const selectedRepositoryId = ref<string | null>(null)

  function selectRepository(id: string | null) {
    selectedRepositoryId.value = id
  }

  return {
    selectedRepositoryId,
    selectRepository,
  }
})
