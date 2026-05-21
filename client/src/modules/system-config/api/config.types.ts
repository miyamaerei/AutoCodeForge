/**
 * ConfigType enum matching backend AutoCodeForge.Core.Enums.ConfigType
 */
export type ConfigType =
  | 'Global'
  | 'User'
  | 'Preferences'
  | 'Repository'
  | 'Knowledge'
  | 'Skill'
  | 'Schedule'
  | 'DeepWiki'
  | 'Review'
  | 'Integration'
  | 'Notification'
  | 'Sandbox'
  | 'Workflow'
  | 'ApiKey'
  | 'Model'
  | 'System'

/**
 * Config response from backend
 */
export interface ConfigResponse {
  id: string
  configType: ConfigType
  configKey: string
  configValue: string
  ntId?: string
  isEncrypted: boolean
  isEnabled: boolean
  description?: string
  group?: string
  createdBy?: string
  updatedBy?: string
  createdAt: string
  updatedAt: string
}

/**
 * Config request for create/update
 */
export interface ConfigRequest {
  configKey: string
  configValue: string
  isEncrypted: boolean
  description?: string
  group?: string
}

/**
 * Batch config request item
 */
export interface ConfigRequestItem {
  configType: ConfigType
  configKey: string
  configValue: string
  isEncrypted: boolean
  isEnabled: boolean
  description?: string
  group?: string
}

/**
 * Batch config request
 */
export interface BatchConfigRequest {
  configs: ConfigRequestItem[]
  overwriteExisting: boolean
}

/**
 * Config initialization request
 */
export interface InitConfigRequest {
  ntId?: string
}

/**
 * Config reset request
 */
export interface ResetConfigRequest {
  ntId?: string
}

/**
 * Config history response
 */
export interface ConfigHistoryResponse {
  id: string
  configId: string
  configType: ConfigType
  configKey: string
  previousValue: string
  newValue: string
  operation: string
  changedBy: string
  changedAt: string
}

/**
 * Config init result
 */
export interface ConfigInitResult {
  success: boolean
  initializedCount: number
  message: string
}

/**
 * Config import result
 */
export interface ConfigImportResult {
  success: boolean
  importedCount: number
  message: string
}

/**
 * Config template response
 */
export interface ConfigTemplateResponse {
  configType: ConfigType
  defaultKey: string
  description: string
  defaultValue: string
}

/**
 * Sandbox config DTO
 */
export interface SandboxConfigDto {
  workspaceRootPath: string
  allowedWritePaths: string[]
  timeoutSeconds: number
  userIsolationEnabled: boolean
}

/**
 * Paged result wrapper
 */
export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

/**
 * Standard API response wrapper
 */
export interface ApiResponse<T> {
  success: boolean
  message?: string
  data: T
}

/**
 * Git configuration options (matches backend GitOptions)
 */
export interface GitOptions {
  azureDevOps: AzureDevOpsOptions
  path: PathOptions
  stringHandling: StringHandlingOptions
  providers: ProviderOptions
}

/**
 * Azure DevOps specific configuration options
 */
export interface AzureDevOpsOptions {
  domainPatterns: string[]
  username: string
  enableUrlEncoding: boolean
  defaultOrganization: string
  defaultProject: string
}

/**
 * Path handling configuration options
 */
export interface PathOptions {
  maxPathLength: number
  tempPathTemplate: string
  autoShortenPaths: boolean
  shortPathIdLength: number
}

/**
 * Special string handling configuration options
 */
export interface StringHandlingOptions {
  gitHubUsername: string
  gitLabUsername: string
  specialCharacters: string[]
  normalizeWhitespace: boolean
  whitespaceReplacement: string
}

/**
 * Provider-specific configuration options
 */
export interface ProviderOptions {
  defaultUsername: string
  useSshByDefault: boolean
  sshPort: number
  connectionTimeoutSeconds: number
}

/**
 * Git provider type
 */
export type GitProviderType = 'AzureDevOps' | 'GitHub' | 'GitLab' | 'Generic'
