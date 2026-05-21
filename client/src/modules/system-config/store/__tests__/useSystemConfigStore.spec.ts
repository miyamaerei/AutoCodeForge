/**
 * useSystemConfigStore 单元测试
 */
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useSystemConfigStore } from '../useSystemConfigStore'
import * as configApi from '../../api/config.api'
import type {
  ConfigResponse,
  ConfigType,
  SandboxConfigDto,
  ConfigTemplateResponse,
  ConfigRequest,
} from '../../api/config.types'

describe('useSystemConfigStore', () => {
  // 测试数据
  const mockConfig: ConfigResponse = {
    configKey: 'test-key',
    configValue: { enabled: true },
    description: '测试配置',
    configType: 'knowledge' as ConfigType,
    createdAt: '2026-05-01 10:00:00',
    updatedAt: '2026-05-21 10:00:00',
  }

  const mockConfigs: ConfigResponse[] = [
    mockConfig,
    {
      configKey: 'another-key',
      configValue: { value: 'test' },
      description: '另一个配置',
      configType: 'knowledge' as ConfigType,
      createdAt: '2026-05-01 10:00:00',
      updatedAt: '2026-05-21 10:00:00',
    },
  ]

  const mockSandboxConfig: SandboxConfigDto = {
    enabled: true,
    sandboxType: 'docker',
    image: 'sandbox:latest',
  }

  // Spy objects
  let fetchConfigsSpy: ReturnType<typeof vi.spyOn>
  let getConfigSpy: ReturnType<typeof vi.spyOn>
  let upsertConfigSpy: ReturnType<typeof vi.spyOn>
  let deleteConfigSpy: ReturnType<typeof vi.spyOn>
  let getSandboxConfigSpy: ReturnType<typeof vi.spyOn>
  let updateSandboxConfigSpy: ReturnType<typeof vi.spyOn>
  let getConfigDefaultsSpy: ReturnType<typeof vi.spyOn>
  let initConfigSpy: ReturnType<typeof vi.spyOn>
  let resetConfigSpy: ReturnType<typeof vi.spyOn>

  // Helper function to set store state
  function setStoreState(
    store: ReturnType<typeof useSystemConfigStore>,
    state: Partial<{
      configsByType: Record<ConfigType, ConfigResponse[]>
      currentConfig: ConfigResponse | null
      sandboxConfig: SandboxConfigDto | null
    }>,
  ) {
    store.$patch(state)
  }

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()

    // Setup spies
    fetchConfigsSpy = vi.spyOn(configApi, 'fetchConfigs')
    getConfigSpy = vi.spyOn(configApi, 'getConfig')
    upsertConfigSpy = vi.spyOn(configApi, 'upsertConfig')
    deleteConfigSpy = vi.spyOn(configApi, 'deleteConfig')
    getSandboxConfigSpy = vi.spyOn(configApi, 'getSandboxConfig')
    updateSandboxConfigSpy = vi.spyOn(configApi, 'updateSandboxConfig')
    getConfigDefaultsSpy = vi.spyOn(configApi, 'getConfigDefaults')
    initConfigSpy = vi.spyOn(configApi, 'initConfig')
    resetConfigSpy = vi.spyOn(configApi, 'resetConfig')
  })

  describe('初始状态', () => {
    it('should have correct initial state', () => {
      const store = useSystemConfigStore()

      expect(store.configsByType).toEqual({})
      expect(store.currentConfig).toBe(null)
      expect(store.loading).toBe(false)
      expect(store.saving).toBe(false)
      expect(store.error).toBe(null)
      expect(store.sandboxConfig).toBe(null)
    })
  })

  describe('loadConfigs', () => {
    it('should load configs by type successfully', async () => {
      const store = useSystemConfigStore()
      fetchConfigsSpy.mockResolvedValue(mockConfigs)

      await store.loadConfigs('knowledge')

      expect(store.configsByType['knowledge']).toEqual(mockConfigs)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
    })

    it('should set error on load failure', async () => {
      const store = useSystemConfigStore()
      fetchConfigsSpy.mockRejectedValue(new Error('加载配置失败'))

      await store.loadConfigs('knowledge')

      expect(store.error).toBe('加载配置失败')
      expect(store.loading).toBe(false)
    })
  })

  describe('loadConfig', () => {
    it('should load single config successfully', async () => {
      const store = useSystemConfigStore()
      getConfigSpy.mockResolvedValue(mockConfig)

      await store.loadConfig('knowledge', 'test-key')

      expect(store.currentConfig).toEqual(mockConfig)
      expect(store.loading).toBe(false)
    })
  })

  describe('saveConfig', () => {
    it('should save config successfully (create)', async () => {
      const store = useSystemConfigStore()
      upsertConfigSpy.mockResolvedValue(mockConfig)

      const result = await store.saveConfig('knowledge', {
        configKey: 'test-key',
        configValue: { enabled: true },
      })

      expect(result).toEqual(mockConfig)
      expect(store.configsByType['knowledge']).toContainEqual(mockConfig)
      expect(store.saving).toBe(false)
    })

    it('should update existing config', async () => {
      const store = useSystemConfigStore()
      setStoreState(store, { configsByType: { knowledge: [mockConfig] } })
      const updatedConfig = { ...mockConfig, configValue: { enabled: false } }
      upsertConfigSpy.mockResolvedValue(updatedConfig)

      await store.saveConfig('knowledge', {
        configKey: 'test-key',
        configValue: { enabled: false },
      })

      expect(store.configsByType['knowledge'][0].configValue).toEqual({ enabled: false })
    })
  })

  describe('removeConfig', () => {
    it('should remove config successfully', async () => {
      const store = useSystemConfigStore()
      setStoreState(store, { configsByType: { knowledge: [...mockConfigs] } })
      deleteConfigSpy.mockResolvedValue(true)

      await store.removeConfig('knowledge', 'test-key')

      expect(store.configsByType['knowledge'].find((c) => c.configKey === 'test-key')).toBeUndefined()
    })
  })

  describe('loadSandboxConfig', () => {
    it('should load sandbox config successfully', async () => {
      const store = useSystemConfigStore()
      getSandboxConfigSpy.mockResolvedValue(mockSandboxConfig)

      await store.loadSandboxConfig()

      expect(store.sandboxConfig).toEqual(mockSandboxConfig)
    })
  })

  describe('saveSandboxConfig', () => {
    it('should save sandbox config successfully', async () => {
      const store = useSystemConfigStore()
      updateSandboxConfigSpy.mockResolvedValue(mockSandboxConfig)

      await store.saveSandboxConfig(mockSandboxConfig)

      expect(store.sandboxConfig).toEqual(mockSandboxConfig)
    })
  })

  describe('loadConfigDefaults', () => {
    it('should load config defaults successfully', async () => {
      const store = useSystemConfigStore()
      const mockDefaults: ConfigTemplateResponse = {
        type: 'knowledge',
        defaults: {},
      }
      getConfigDefaultsSpy.mockResolvedValue(mockDefaults)

      await store.loadConfigDefaults('knowledge')

      expect(store.configDefaults['knowledge']).toEqual(mockDefaults)
    })
  })
})
