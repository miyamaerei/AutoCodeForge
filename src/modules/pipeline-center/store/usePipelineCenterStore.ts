import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { fetchBuilds, fetchPipelines, type BuildDto, type PipelineDto } from '../pipeline-center.api'

export const usePipelineCenterStore = defineStore('module.pipeline-center', () => {
  const pipelines = ref<PipelineDto[]>([])
  const builds = ref<BuildDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const hasPipelines = computed(() => pipelines.value.length > 0)
  const hasBuilds = computed(() => builds.value.length > 0)

  async function loadPipelines(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      pipelines.value = await fetchPipelines()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载流水线失败'
    } finally {
      loading.value = false
    }
  }

  async function loadBuilds(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      builds.value = await fetchBuilds()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载构建失败'
    } finally {
      loading.value = false
    }
  }

  return {
    pipelines,
    builds,
    loading,
    error,
    hasPipelines,
    hasBuilds,
    loadPipelines,
    loadBuilds,
  }
})
