import { computed, ref } from 'vue'
import { defineStore } from 'pinia'

export interface ContactFormSubmission {
  name: string
  email: string
  message: string
}

export const useAppStore = defineStore('app', () => {
  const lastSubmission = ref<ContactFormSubmission | null>(null)

  function setLastSubmission(submission: ContactFormSubmission): void {
    lastSubmission.value = submission
  }

  const hasSubmission = computed(() => lastSubmission.value !== null)

  return {
    lastSubmission,
    hasSubmission,
    setLastSubmission,
  }
})
