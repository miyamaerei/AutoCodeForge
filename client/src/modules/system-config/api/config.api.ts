/**
 * System Config API functions
 * Wires frontend to backend /v1/configs endpoints
 */
import type {
  ConfigResponse,
  ConfigRequest,
  ConfigType,
  SandboxConfigDto,
  ConfigTemplateResponse,
  ConfigInitResult,
  ConfigHistoryResponse,
  BatchConfigRequest,
  ApiResponse,
  GitOptions,
} from './config.types'
import { request } from '@/lib/request'
import { USE_MOCK } from '@/config/runtime'
import {
  mockGetConfigs,
  mockGetConfig,
  mockUpsertConfig,
  mockDeleteConfig,
  mockGetSandboxConfig,
  mockUpdateSandboxConfig,
  mockGetConfigDefaults,
  mockInitConfig,
  mockResetConfig,
  mockGetConfigHistory,
  mockBatchUpdateConfigs,
  mockExportConfig,
  mockImportConfig,
  mockGetGitConfig,
  mockUpdateGitConfig,
} from './config.mock'

/**
 * Fetch configs by type
 */
export async function fetchConfigs(configType: ConfigType): Promise<ConfigResponse[]> {
  if (USE_MOCK) return mockGetConfigs(configType)
  const { data } = await request.get<ApiResponse<ConfigResponse[]>>(`/v1/configs/${configType}`)
  return data.data
}

/**
 * Get single config by type and key
 */
export async function getConfig(configType: ConfigType, configKey: string): Promise<ConfigResponse | null> {
  if (USE_MOCK) return mockGetConfig(configType, configKey)
  try {
    const { data } = await request.get<ApiResponse<ConfigResponse>>(`/v1/configs/${configType}/${configKey}`)
    return data.data
  } catch {
    return null
  }
}

/**
 * Create new config
 */
export async function createConfig(configType: ConfigType, payload: ConfigRequest): Promise<ConfigResponse> {
  if (USE_MOCK) return mockUpsertConfig(configType, payload)
  const { data } = await request.post<ApiResponse<ConfigResponse>>(`/v1/configs/${configType}`, payload)
  return data.data
}

/**
 * Update existing config
 */
export async function updateConfig(
  configType: ConfigType,
  configKey: string,
  payload: ConfigRequest,
): Promise<ConfigResponse> {
  if (USE_MOCK) return mockUpsertConfig(configType, payload)
  const { data } = await request.put<ApiResponse<ConfigResponse>>(`/v1/configs/${configType}/${configKey}`, payload)
  return data.data
}

/**
 * Upsert config (create or update)
 * Direct POST to backend which handles both creation and update via UpsertAsync
 */
export async function upsertConfig(configType: ConfigType, payload: ConfigRequest): Promise<ConfigResponse> {
  if (USE_MOCK) return mockUpsertConfig(configType, payload)
  const { data } = await request.post<ApiResponse<ConfigResponse>>(`/v1/configs/${configType}`, payload)
  return data.data
}

/**
 * Delete config
 */
export async function deleteConfig(configType: ConfigType, configKey: string): Promise<boolean> {
  if (USE_MOCK) return mockDeleteConfig(configType, configKey)
  try {
    await request.delete(`/v1/configs/${configType}/${configKey}`)
    return true
  } catch {
    return false
  }
}

/**
 * Get sandbox config
 */
export async function getSandboxConfig(): Promise<SandboxConfigDto> {
  if (USE_MOCK) return mockGetSandboxConfig()
  const { data } = await request.get<ApiResponse<SandboxConfigDto>>('/v1/configs/user/sandbox')
  return data.data
}

/**
 * Update sandbox config
 */
export async function updateSandboxConfig(payload: SandboxConfigDto): Promise<SandboxConfigDto> {
  if (USE_MOCK) return mockUpdateSandboxConfig(payload)
  const { data } = await request.put<ApiResponse<SandboxConfigDto>>('/v1/configs/user/sandbox', payload)
  return data.data
}

/**
 * Get config defaults for a type
 */
export async function getConfigDefaults(configType: ConfigType): Promise<ConfigTemplateResponse> {
  if (USE_MOCK) return mockGetConfigDefaults(configType)
  const { data } = await request.get<ApiResponse<ConfigTemplateResponse>>(`/v1/configs/${configType}/defaults`)
  return data.data
}

/**
 * Initialize config to defaults
 */
export async function initConfig(configType: ConfigType, ntId?: string): Promise<ConfigInitResult> {
  if (USE_MOCK) return mockInitConfig(configType)
  const payload = ntId ? { ntId } : undefined
  const { data } = await request.post<ApiResponse<ConfigInitResult>>(`/v1/configs/${configType}/init`, payload)
  return data.data
}

/**
 * Reset config to defaults
 */
export async function resetConfig(configType: ConfigType, ntId?: string): Promise<boolean> {
  if (USE_MOCK) return mockResetConfig(configType)
  const payload = ntId ? { ntId } : undefined
  try {
    await request.post(`/v1/configs/${configType}/reset`, payload)
    return true
  } catch {
    return false
  }
}

/**
 * Get config history (admin only)
 */
export async function fetchConfigHistory(
  configType?: ConfigType,
  changedBy?: string,
  page = 1,
  pageSize = 20,
): Promise<ConfigHistoryResponse[]> {
  if (USE_MOCK) return mockGetConfigHistory(configType, page, pageSize)
  const params: Record<string, string | number> = { page, pageSize }
  if (configType) params.configType = configType
  if (changedBy) params.changedBy = changedBy
  const { data } = await request.get<ApiResponse<ConfigHistoryResponse[]>>('/v1/configs/history', { params })
  return data.data
}

/**
 * Rollback config to history point (admin only)
 */
export async function rollbackConfig(historyId: string): Promise<ConfigResponse> {
  const { data } = await request.post<ApiResponse<ConfigResponse>>(`/v1/configs/history/${historyId}/rollback`)
  return data.data
}

/**
 * Batch update configs (admin only)
 */
export async function batchUpdateConfigs(request: BatchConfigRequest): Promise<number> {
  if (USE_MOCK) return mockBatchUpdateConfigs(request)
  await request.post('/v1/configs/batch', request)
  return request.configs.length
}

/**
 * Export config by type (admin only)
 */
export async function exportConfig(configType: ConfigType): Promise<string> {
  if (USE_MOCK) return mockExportConfig(configType)
  const { data } = await request.get<ApiResponse<string>>(`/v1/configs/${configType}/export`)
  return data.data
}

/**
 * Import config (admin only)
 */
export async function importConfig(configType: ConfigType, jsonData: string, overwriteExisting = true): Promise<number> {
  if (USE_MOCK) return mockImportConfig(configType, jsonData, overwriteExisting)
  const payload = { jsonData, overwriteExisting }
  const { data } = await request.post<ApiResponse<{ importedCount: number }>>(`/v1/configs/${configType}/import`, payload)
  return data.data.importedCount
}

/**
 * Get Git config
 */
export async function getGitConfig(): Promise<GitOptions> {
  if (USE_MOCK) return mockGetGitConfig()
  const { data } = await request.get<ApiResponse<GitOptions>>('/v1/configs/git')
  return data.data
}

/**
 * Update Git config
 */
export async function updateGitConfig(payload: GitOptions): Promise<GitOptions> {
  if (USE_MOCK) return mockUpdateGitConfig(payload)
  const { data } = await request.put<ApiResponse<GitOptions>>('/v1/configs/git', payload)
  return data.data
}
