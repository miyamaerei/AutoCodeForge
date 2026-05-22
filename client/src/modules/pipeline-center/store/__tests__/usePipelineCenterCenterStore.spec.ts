/**
 * usePipelineCenterStore 单元测试
 */
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { usePipelineCenterStore } from '../usePipelineCenterStore'
import * as pipelineApi from '../../pipeline-center.api'
import type { PipelineDto, BuildDto } from '../../pipeline-center.api'

describe('usePipelineCenterStore', () => {
  // 测试数据
  const mockPipelines: PipelineDto[] = [
    {
      id: 'pipeline-1',
      name: '主流水线',
      status: 'success',
      lastRun: '2026-05-21 10:00:00',
      successRate: '95%',
      branch: 'main',
    },
    {
      id: 'pipeline-2',
      name: '开发流水线',
      status: 'running',
      lastRun: '2026-05-21 11:00:00',
      successRate: '88%',
      branch: 'develop',
    },
  ]

  const mockBuilds: BuildDto[] = [
    {
      id: 'build-1',
      pipelineName: '主流水线',
      buildNumber: '#1',
      status: 'success',
      startTime: '2026-05-21 10:00:00',
      duration: '120s',
      branch: 'main',
    },
    {
      id: 'build-2',
      pipelineName: '主流水线',
      buildNumber: '#2',
      status: 'failed',
      startTime: '2026-05-21 11:00:00',
      duration: '60s',
      branch: 'main',
    },
  ]

  // Spy objects
  let fetchPipelinesSpy: ReturnType<typeof vi.spyOn>
  let fetchBuildsSpy: ReturnType<typeof vi.spyOn>

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()

    fetchPipelinesSpy = vi.spyOn(pipelineApi, 'fetchPipelines')
    fetchBuildsSpy = vi.spyOn(pipelineApi, 'fetchBuilds')
  })

  describe('初始状态', () => {
    it('should have correct initial state', () => {
      const store = usePipelineCenterStore()

      expect(store.pipelines).toEqual([])
      expect(store.builds).toEqual([])
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
    })

    it('should have correct computed properties', () => {
      const store = usePipelineCenterStore()
      expect(store.hasPipelines).toBe(false)
      expect(store.hasBuilds).toBe(false)
    })
  })

  describe('loadPipelines', () => {
    it('should load pipelines successfully', async () => {
      const store = usePipelineCenterStore()
      fetchPipelinesSpy.mockResolvedValue(mockPipelines)

      await store.loadPipelines()

      expect(store.pipelines).toEqual(mockPipelines)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
      expect(store.hasPipelines).toBe(true)
    })

    it('should set error on load failure', async () => {
      const store = usePipelineCenterStore()
      fetchPipelinesSpy.mockRejectedValue(new Error('加载流水线失败'))

      await store.loadPipelines()

      expect(store.error).toBe('加载流水线失败')
      expect(store.loading).toBe(false)
    })
  })

  describe('loadBuilds', () => {
    it('should load builds successfully', async () => {
      const store = usePipelineCenterStore()
      fetchBuildsSpy.mockResolvedValue(mockBuilds)

      await store.loadBuilds()

      expect(store.builds).toEqual(mockBuilds)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
      expect(store.hasBuilds).toBe(true)
    })

    it('should set error on load failure', async () => {
      const store = usePipelineCenterStore()
      fetchBuildsSpy.mockRejectedValue(new Error('加载构建失败'))

      await store.loadBuilds()

      expect(store.error).toBe('加载构建失败')
      expect(store.loading).toBe(false)
    })
  })
})
