import { computed, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { fetchConfigHistory, rollbackConfig, exportConfig, importConfig } from '../api/config.api'
import { useSystemConfigStore } from '../store/useSystemConfigStore'
import type { ConfigHistoryResponse, ConfigType } from '../api/config.types'

/**
 * Tab 类型定义
 */
export type ManagementTabKey = 'history' | 'import-export'

/**
 * Tab 配置项
 */
export interface ManagementTab {
  key: ManagementTabKey
  label: string
}

/**
 * 历史记录过滤条件
 */
export interface ConfigHistoryFilter {
  configType?: ConfigType
  changedBy?: string
  page: number
  pageSize: number
}

/**
 * 导入导出状态
 */
export interface ImportExportState {
  selectedConfigType: ConfigType
  importData: string
  overwriteExisting: boolean
  importResult: string
  exportResult: string
}

/**
 * 配置管理 Composable
 * 管理配置历史查询、导入导出功能
 * 遵循 vue3-composable-builder 模板规范
 */
export function useSystemConfigManagement() {
  const store = useSystemConfigStore()

  // ========== 状态定义 ==========

  /** 当前激活的标签页 */
  const activeTab = ref<ManagementTabKey>('history')

  /** 全局加载状态 */
  const loading = ref(false)

  /** 全局错误信息 */
  const error = ref<string | null>(null)

  /** Tab 配置 */
  const tabs: ManagementTab[] = [
    { key: 'history', label: '配置历史' },
    { key: 'import-export', label: '导入导出' },
  ]

  // --- History 相关状态 ---
  /** 历史记录列表 */
  const historyList = ref<ConfigHistoryResponse[]>([])

  /** 历史记录加载状态 */
  const historyLoading = ref(false)

  /** 历史记录过滤条件 */
  const historyFilter = ref<ConfigHistoryFilter>({
    page: 1,
    pageSize: 20,
  })

  // --- Import/Export 相关状态 ---
  /** 导入导出加载状态 */
  const importExportLoading = ref(false)

  /** 导入文件 */
  const importFile = ref<File | null>(null)

  /** 导入导出状态 */
  const importExportState = ref<ImportExportState>({
    selectedConfigType: 'Preferences',
    importData: '',
    overwriteExisting: true,
    importResult: '',
    exportResult: '',
  })

  // --- 配置类型选项 ---
  const configTypes: { value: ConfigType; label: string }[] = [
    { value: 'Global', label: 'Global' },
    { value: 'User', label: 'User' },
    { value: 'Preferences', label: 'Preferences' },
    { value: 'Repository', label: 'Repository' },
    { value: 'Knowledge', label: 'Knowledge' },
    { value: 'Skill', label: 'Skill' },
    { value: 'Schedule', label: 'Schedule' },
    { value: 'DeepWiki', label: 'DeepWiki' },
    { value: 'Review', label: 'Review' },
    { value: 'Integration', label: 'Integration' },
    { value: 'Notification', label: 'Notification' },
    { value: 'Sandbox', label: 'Sandbox' },
    { value: 'Workflow', label: 'Workflow' },
    { value: 'Llm', label: 'LLM' },
    { value: 'Git', label: 'Git' },
    { value: 'System', label: 'System' },
  ]

  // ========== Computed ==========

  /** 是否有历史数据 */
  const hasHistoryData = computed(() => historyList.value.length > 0)

  /** 选中的配置类型 */
  const selectedConfigType = computed({
    get: () => importExportState.value.selectedConfigType,
    set: (val: ConfigType) => {
      importExportState.value.selectedConfigType = val
    },
  })

  // ========== 方法定义 ==========

  /**
   * 加载配置历史记录
   */
  async function loadHistory(): Promise<void> {
    historyLoading.value = true
    error.value = null
    try {
      const changedBy = historyFilter.value.changedBy?.trim() || undefined
      historyList.value = await fetchConfigHistory(
        historyFilter.value.configType,
        changedBy,
        historyFilter.value.page,
        historyFilter.value.pageSize,
      )
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载配置历史失败'
      ElMessage.error(error.value)
    } finally {
      historyLoading.value = false
    }
  }

  /**
   * 回滚配置到历史版本
   */
  async function handleRollback(historyItem: ConfigHistoryResponse): Promise<void> {
    try {
      await ElMessageBox.confirm(
        `确定要将配置 "${historyItem.configKey}" 回滚到 ${historyItem.changedAt} 的状态吗？`,
        '确认回滚',
        {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning',
        },
      )
      await rollbackConfig(historyItem.id)
      ElMessage.success('配置已回滚')
      await loadHistory()
    } catch (err) {
      if (err !== 'cancel') {
        const errMsg = err instanceof Error ? err.message : '回滚失败'
        ElMessage.error(errMsg)
      }
    }
  }

  /**
   * 导出配置
   */
  async function handleExport(): Promise<void> {
    importExportLoading.value = true
    error.value = null
    importExportState.value.exportResult = ''
    try {
      const jsonData = await exportConfig(importExportState.value.selectedConfigType)
      importExportState.value.exportResult = `导出成功：${importExportState.value.selectedConfigType} 配置`
      ElMessage.success('配置导出成功')

      // 下载文件
      const blob = new Blob([jsonData], { type: 'application/json' })
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `${importExportState.value.selectedConfigType.toLowerCase()}-config-${new Date().toISOString().slice(0, 10)}.json`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      URL.revokeObjectURL(url)
    } catch (err) {
      error.value = err instanceof Error ? err.message : '导出配置失败'
      ElMessage.error(error.value)
    } finally {
      importExportLoading.value = false
    }
  }

  /**
   * 处理文件选择
   */
  function handleFileChange(file: File): boolean {
    importFile.value = file
    const reader = new FileReader()
    reader.onload = (e) => {
      importExportState.value.importData = (e.target?.result as string) || ''
    }
    reader.readAsText(file)
    return false
  }

  /**
   * 导入配置
   */
  async function handleImport(): Promise<void> {
    if (!importExportState.value.importData.trim()) {
      ElMessage.warning('请先选择或粘贴要导入的 JSON 数据')
      return
    }

    importExportLoading.value = true
    error.value = null
    importExportState.value.importResult = ''
    try {
      const importedCount = await importConfig(
        importExportState.value.selectedConfigType,
        importExportState.value.importData,
        importExportState.value.overwriteExisting,
      )
      importExportState.value.importResult = `导入成功：已导入 ${importedCount} 条配置`
      ElMessage.success(`成功导入 ${importedCount} 条配置`)

      // 导入成功后刷新 store 中的配置
      await store.loadConfigs(importExportState.value.selectedConfigType)
    } catch (err) {
      error.value = err instanceof Error ? err.message : '导入配置失败'
      ElMessage.error(error.value)
    } finally {
      importExportLoading.value = false
    }
  }

  /**
   * 清空导入数据
   */
  function clearImportData(): void {
    importExportState.value.importData = ''
    importFile.value = null
    importExportState.value.importResult = ''
  }

  /**
   * 切换标签页
   */
  function setActiveTab(tab: ManagementTabKey): void {
    activeTab.value = tab
  }

  /**
   * 重置历史过滤条件
   */
  function resetHistoryFilter(): void {
    historyFilter.value = {
      page: 1,
      pageSize: 20,
    }
  }

  /**
   * 更新历史过滤条件
   */
  function updateHistoryFilter(patch: Partial<ConfigHistoryFilter>): void {
    historyFilter.value = { ...historyFilter.value, ...patch }
  }

  /**
   * 重置所有状态
   */
  function reset(): void {
    activeTab.value = 'history'
    error.value = null
    historyList.value = []
    historyFilter.value = {
      page: 1,
      pageSize: 20,
    }
    importExportState.value = {
      selectedConfigType: 'Preferences',
      importData: '',
      overwriteExisting: true,
      importResult: '',
      exportResult: '',
    }
    importFile.value = null
  }

  return {
    // 状态
    activeTab,
    loading,
    error,
    tabs,
    historyList,
    historyLoading,
    historyFilter,
    importExportLoading,
    importFile,
    importExportState,
    configTypes,

    // Computed
    hasHistoryData,
    selectedConfigType,

    // 方法
    loadHistory,
    handleRollback,
    handleExport,
    handleFileChange,
    handleImport,
    clearImportData,
    setActiveTab,
    resetHistoryFilter,
    updateHistoryFilter,
    reset,
  }
}
