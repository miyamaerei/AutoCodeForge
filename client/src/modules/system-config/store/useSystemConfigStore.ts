/**
 * System Config Store
 * Manages unified configuration state for all config types
 */
import { ref } from 'vue'
import { defineStore } from 'pinia'
import {
  fetchConfigs,
  getConfig,
  upsertConfig,
  deleteConfig,
  getSandboxConfig,
  updateSandboxConfig,
  getConfigDefaults,
  initConfig,
  resetConfig,
  getGitConfig,
  updateGitConfig,
} from '../api/config.api'
import type {
  ConfigResponse,
  ConfigType,
  SandboxConfigDto,
  ConfigTemplateResponse,
  ConfigInitResult,
  ConfigRequest,
  GitOptions,
} from '../api/config.types'

export const useSystemConfigStore = defineStore('module.system-config', () => {
  // Generic config state by type
  const configsByType = ref<Record<ConfigType, ConfigResponse[]>>({} as Record<ConfigType, ConfigResponse[]>)
  const currentConfig = ref<ConfigResponse | null>(null)
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)

  // Sandbox config state
  const sandboxConfig = ref<SandboxConfigDto | null>(null)

  // Git config state
  const gitConfig = ref<GitOptions | null>(null)

  // Config defaults by type
  const configDefaults = ref<Record<ConfigType, ConfigTemplateResponse>>({} as Record<ConfigType, ConfigTemplateResponse>)

  /**
   * Load configs by type
   */
  async function loadConfigs(configType: ConfigType): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const configs = await fetchConfigs(configType)
      configsByType.value[configType] = configs
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载配置失败'
    } finally {
      loading.value = false
    }
  }

  /**
   * Load single config by type and key
   */
  async function loadConfig(configType: ConfigType, configKey: string): Promise<void> {
    loading.value = true
    error.value = null
    try {
      currentConfig.value = await getConfig(configType, configKey)
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载配置详情失败'
    } finally {
      loading.value = false
    }
  }

  /**
   * Save config (create or update)
   */
  async function saveConfig(configType: ConfigType, payload: ConfigRequest): Promise<ConfigResponse> {
    saving.value = true
    error.value = null
    try {
      const saved = await upsertConfig(configType, payload)
      // Update local cache
      if (!configsByType.value[configType]) {
        configsByType.value[configType] = []
      }
      const idx = configsByType.value[configType].findIndex((c) => c.configKey === payload.configKey)
      if (idx !== -1) {
        configsByType.value[configType][idx] = saved
      } else {
        configsByType.value[configType].push(saved)
      }
      return saved
    } catch (err) {
      error.value = err instanceof Error ? err.message : '保存配置失败'
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * Delete config
   */
  async function removeConfig(configType: ConfigType, configKey: string): Promise<boolean> {
    error.value = null
    try {
      const success = await deleteConfig(configType, configKey)
      if (success && configsByType.value[configType]) {
        configsByType.value[configType] = configsByType.value[configType].filter(
          (c) => c.configKey !== configKey,
        )
      }
      return success
    } catch (err) {
      error.value = err instanceof Error ? err.message : '删除配置失败'
      return false
    }
  }

  /**
   * Load sandbox config
   */
  async function loadSandboxConfig(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      sandboxConfig.value = await getSandboxConfig()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载沙盒配置失败'
    } finally {
      loading.value = false
    }
  }

  /**
   * Save sandbox config
   */
  async function saveSandboxConfig(payload: SandboxConfigDto): Promise<SandboxConfigDto> {
    saving.value = true
    error.value = null
    try {
      sandboxConfig.value = await updateSandboxConfig(payload)
      return sandboxConfig.value
    } catch (err) {
      error.value = err instanceof Error ? err.message : '保存沙盒配置失败'
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * Load Git config
   */
  async function loadGitConfig(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      gitConfig.value = await getGitConfig()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载 Git 配置失败'
    } finally {
      loading.value = false
    }
  }

  /**
   * Save Git config
   */
  async function saveGitConfig(payload: GitOptions): Promise<GitOptions> {
    saving.value = true
    error.value = null
    try {
      gitConfig.value = await updateGitConfig(payload)
      return gitConfig.value
    } catch (err) {
      error.value = err instanceof Error ? err.message : '保存 Git 配置失败'
      throw err
    } finally {
      saving.value = false
    }
  }

  /**
   * Load config defaults
   */
  async function loadConfigDefaults(configType: ConfigType): Promise<void> {
    try {
      configDefaults.value[configType] = await getConfigDefaults(configType)
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载默认配置失败'
    }
  }

  /**
   * Initialize config to defaults
   */
  async function initializeConfig(configType: ConfigType): Promise<ConfigInitResult> {
    loading.value = true
    error.value = null
    try {
      const result = await initConfig(configType)
      // Reload configs after init
      await loadConfigs(configType)
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : '初始化配置失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  /**
   * Reset config to defaults
   */
  async function resetToDefaults(configType: ConfigType): Promise<boolean> {
    loading.value = true
    error.value = null
    try {
      const success = await resetConfig(configType)
      if (success) {
        // Reload configs after reset
        await loadConfigs(configType)
      }
      return success
    } catch (err) {
      error.value = err instanceof Error ? err.message : '重置配置失败'
      return false
    } finally {
      loading.value = false
    }
  }

  /**
   * Get cached configs for a type
   */
  function getConfigs(configType: ConfigType): ConfigResponse[] {
    return configsByType.value[configType] || []
  }

  /**
   * Get cached config defaults for a type
   */
  function getDefaults(configType: ConfigType): ConfigTemplateResponse | undefined {
    return configDefaults.value[configType]
  }

  return {
    configsByType,
    currentConfig,
    sandboxConfig,
    gitConfig,
    configDefaults,
    loading,
    saving,
    error,
    loadConfigs,
    loadConfig,
    saveConfig,
    removeConfig,
    loadSandboxConfig,
    saveSandboxConfig,
    loadGitConfig,
    saveGitConfig,
    loadConfigDefaults,
    initializeConfig,
    resetToDefaults,
    getConfigs,
    getDefaults,
  }
})
