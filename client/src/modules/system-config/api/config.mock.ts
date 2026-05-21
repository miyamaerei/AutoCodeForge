/**
 * Mock implementations for system config module
 * Used when USE_MOCK = true
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
  GitOptions,
} from './config.types'

/**
 * Get configs by type - mock implementation
 */
export function mockGetConfigs(configType: ConfigType): ConfigResponse[] {
  // Return mock data based on config type
  const mockConfigs: Record<ConfigType, ConfigResponse[]> = {
    Preferences: [
      {
        id: 'pref-1',
        configType: 'Preferences',
        configKey: 'locale',
        configValue: 'zh-CN',
        isEncrypted: false,
        isEnabled: true,
        description: 'Language setting',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      },
      {
        id: 'pref-2',
        configType: 'Preferences',
        configKey: 'timezone',
        configValue: 'Asia/Shanghai',
        isEncrypted: false,
        isEnabled: true,
        description: 'Timezone setting',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      },
      {
        id: 'pref-3',
        configType: 'Preferences',
        configKey: 'theme',
        configValue: 'light',
        isEncrypted: false,
        isEnabled: true,
        description: 'Theme setting',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      },
    ],
    Sandbox: [
      {
        id: 'sandbox-1',
        configType: 'Sandbox',
        configKey: 'workspace',
        configValue: JSON.stringify({
          workspaceRootPath: 'C:/gitrepos/AutoCodeForge',
          allowedWritePaths: ['src/**', 'docs/**'],
          timeoutSeconds: 600,
          userIsolationEnabled: true,
        }),
        isEncrypted: false,
        isEnabled: true,
        description: 'Sandbox workspace configuration',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      },
    ],
    Knowledge: [
      {
        id: 'knowledge-1',
        configType: 'Knowledge',
        configKey: 'indexing',
        configValue: JSON.stringify({
          chunkSize: 800,
          chunkOverlap: 120,
          refreshPolicy: 'daily',
        }),
        isEncrypted: false,
        isEnabled: true,
        description: 'Knowledge indexing settings',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      },
    ],
    Skill: [
      {
        id: 'skill-1',
        configType: 'Skill',
        configKey: 'enabled-skills',
        configValue: JSON.stringify(['code-review', 'debug-assist', 'refactor']),
        isEncrypted: false,
        isEnabled: true,
        description: 'Enabled skill list',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      },
    ],
    Notification: [
      {
        id: 'notif-1',
        configType: 'Notification',
        configKey: 'channels',
        configValue: JSON.stringify({
          email: true,
          inApp: true,
          slack: false,
        }),
        isEncrypted: false,
        isEnabled: true,
        description: 'Notification channels',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      },
    ],
    Workflow: [
      {
        id: 'workflow-1',
        configType: 'Workflow',
        configKey: 'default-pipeline',
        configValue: 'build-test-deploy',
        isEncrypted: false,
        isEnabled: true,
        description: 'Default workflow pipeline',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      },
    ],
    Integration: [
      {
        id: 'int-1',
        configType: 'Integration',
        configKey: 'github',
        configValue: JSON.stringify({
          webhookSecret: '***',
          defaultBranch: 'main',
        }),
        isEncrypted: true,
        isEnabled: true,
        description: 'GitHub integration settings',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      },
    ],
    // These types have limited or no backend support
    Global: [],
    User: [],
    Repository: [],
    Schedule: [],
    DeepWiki: [],
    Review: [],
    ApiKey: [],
    Model: [],
    System: [],
  }

  return mockConfigs[configType] || []
}

/**
 * Get config by type and key - mock implementation
 */
export function mockGetConfig(configType: ConfigType, configKey: string): ConfigResponse | null {
  const configs = mockGetConfigs(configType)
  return configs.find((c) => c.configKey === configKey) || null
}

/**
 * Create or update config - mock implementation
 */
export function mockUpsertConfig(configType: ConfigType, request: ConfigRequest): ConfigResponse {
  const now = new Date().toISOString()
  return {
    id: `${configType.toLowerCase()}-${Date.now()}`,
    configType,
    configKey: request.configKey,
    configValue: request.configValue,
    isEncrypted: request.isEncrypted,
    isEnabled: true,
    description: request.description,
    group: request.group,
    createdAt: now,
    updatedAt: now,
  }
}

/**
 * Delete config - mock implementation
 */
export function mockDeleteConfig(configType: ConfigType, configKey: string): boolean {
  return true
}

/**
 * Get sandbox config - mock implementation
 */
export function mockGetSandboxConfig(): SandboxConfigDto {
  return {
    workspaceRootPath: 'C:/gitrepos/AutoCodeForge',
    allowedWritePaths: ['src/**', 'docs/**'],
    timeoutSeconds: 600,
    userIsolationEnabled: true,
  }
}

/**
 * Update sandbox config - mock implementation
 */
export function mockUpdateSandboxConfig(dto: SandboxConfigDto): SandboxConfigDto {
  return dto
}

/**
 * Get config defaults - mock implementation
 */
export function mockGetConfigDefaults(configType: ConfigType): ConfigTemplateResponse {
  const defaults: Record<ConfigType, ConfigTemplateResponse> = {
    Preferences: {
      configType: 'Preferences',
      defaultKey: 'user-prefs',
      description: 'User preferences configuration',
      defaultValue: '{"locale":"en-US","timezone":"UTC","theme":"light"}',
    },
    Sandbox: {
      configType: 'Sandbox',
      defaultKey: 'default',
      description: 'Default sandbox configuration',
      defaultValue: '{"workspaceRootPath":"C:/workspace","allowedWritePaths":[],"timeoutSeconds":600,"userIsolationEnabled":true}',
    },
    Knowledge: {
      configType: 'Knowledge',
      defaultKey: 'indexing',
      description: 'Knowledge base indexing settings',
      defaultValue: '{"chunkSize":800,"chunkOverlap":120}',
    },
    Skill: {
      configType: 'Skill',
      defaultKey: 'enabled',
      description: 'Skill configuration',
      defaultValue: '[]',
    },
    Notification: {
      configType: 'Notification',
      defaultKey: 'channels',
      description: 'Notification channels',
      defaultValue: '{"email":true,"inApp":true}',
    },
    Workflow: {
      configType: 'Workflow',
      defaultKey: 'default',
      description: 'Default workflow',
      defaultValue: 'default-pipeline',
    },
    Integration: {
      configType: 'Integration',
      defaultKey: 'default',
      description: 'Integration settings',
      defaultValue: '{}',
    },
    Global: { configType: 'Global', defaultKey: 'global', description: 'Global config', defaultValue: '{}' },
    User: { configType: 'User', defaultKey: 'user', description: 'User config', defaultValue: '{}' },
    Repository: { configType: 'Repository', defaultKey: 'repo', description: 'Repository config', defaultValue: '{}' },
    Schedule: { configType: 'Schedule', defaultKey: 'schedule', description: 'Schedule config', defaultValue: '{}' },
    DeepWiki: { configType: 'DeepWiki', defaultKey: 'deepwiki', description: 'DeepWiki config', defaultValue: '{}' },
    Review: { configType: 'Review', defaultKey: 'review', description: 'Review config', defaultValue: '{}' },
    ApiKey: { configType: 'ApiKey', defaultKey: 'apikey', description: 'API key config', defaultValue: '{}' },
    Model: { configType: 'Model', defaultKey: 'model', description: 'Model config', defaultValue: '{}' },
    System: { configType: 'System', defaultKey: 'system', description: 'System config', defaultValue: '{}' },
  }

  return defaults[configType]
}

/**
 * Initialize config - mock implementation
 */
export function mockInitConfig(configType: ConfigType): ConfigInitResult {
  return {
    success: true,
    initializedCount: 1,
    message: 'Config initialized successfully',
  }
}

/**
 * Reset config - mock implementation
 */
export function mockResetConfig(configType: ConfigType): boolean {
  return true
}

/**
 * Get config history - mock implementation (admin)
 */
export function mockGetConfigHistory(configType?: ConfigType, page = 1, pageSize = 20): ConfigHistoryResponse[] {
  const now = new Date()
  const mockHistory: ConfigHistoryResponse[] = [
    {
      id: 'hist-1',
      configId: 'pref-1',
      configType: 'Preferences',
      configKey: 'locale',
      previousValue: 'en-US',
      newValue: 'zh-CN',
      operation: 'Update',
      changedBy: 'admin',
      changedAt: new Date(now.getTime() - 3600000).toISOString(),
    },
    {
      id: 'hist-2',
      configId: 'pref-3',
      configType: 'Preferences',
      configKey: 'theme',
      previousValue: 'dark',
      newValue: 'light',
      operation: 'Update',
      changedBy: 'admin',
      changedAt: new Date(now.getTime() - 7200000).toISOString(),
    },
    {
      id: 'hist-3',
      configId: 'sandbox-1',
      configType: 'Sandbox',
      configKey: 'workspace',
      previousValue: '{"workspaceRootPath":"C:/old"}',
      newValue: '{"workspaceRootPath":"C:/gitrepos/AutoCodeForge"}',
      operation: 'Update',
      changedBy: 'user1',
      changedAt: new Date(now.getTime() - 86400000).toISOString(),
    },
  ]

  if (configType) {
    return mockHistory.filter((h) => h.configType === configType)
  }
  return mockHistory
}

/**
 * Batch update configs - mock implementation (admin)
 */
export function mockBatchUpdateConfigs(request: BatchConfigRequest): number {
  return request.configs.length
}

/**
 * Export config - mock implementation (admin)
 */
export function mockExportConfig(configType: ConfigType): string {
  const configs = mockGetConfigs(configType)
  return JSON.stringify({
    ConfigType: configType,
    ExportedAt: new Date().toISOString(),
    Configs: configs,
  }, null, 2)
}

/**
 * Import config - mock implementation (admin)
 */
export function mockImportConfig(configType: ConfigType, jsonData: string, overwriteExisting = true): number {
  try {
    const data = JSON.parse(jsonData)
    return data.Configs?.length || 1
  } catch {
    return 1
  }
}

/**
 * Get Git config - mock implementation
 */
export function mockGetGitConfig(): GitOptions {
  return {
    azureDevOps: {
      domainPatterns: ['dev.azure.com', 'visualstudio.com'],
      username: '',
      enableUrlEncoding: true,
      defaultOrganization: '',
      defaultProject: '',
    },
    path: {
      maxPathLength: 260,
      tempPathTemplate: 'C:\\temp\\repo-{guid}',
      autoShortenPaths: true,
      shortPathIdLength: 3,
    },
    stringHandling: {
      gitHubUsername: 'x-access-token',
      gitLabUsername: 'oauth2',
      specialCharacters: [' ', '@', '#', '$', '%', '^', '&', '*', '(', ')', '+', ',', ';', '=', '?', '/'],
      normalizeWhitespace: true,
      whitespaceReplacement: '_',
    },
    providers: {
      defaultUsername: 'git',
      useSshByDefault: false,
      sshPort: 22,
      connectionTimeoutSeconds: 60,
    },
  }
}

/**
 * Update Git config - mock implementation
 */
export function mockUpdateGitConfig(dto: GitOptions): GitOptions {
  return dto
}
