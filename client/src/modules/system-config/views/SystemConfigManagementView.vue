<script setup lang="ts">
import { onMounted } from 'vue'
import { useSystemConfigManagement } from '../composables/useSystemConfigManagement'
import type { ConfigHistoryResponse } from '../api/config.types'

const {
  activeTab,
  loading,
  error,
  tabs,
  historyList,
  historyFilter,
  historyLoading,
  hasHistoryData,
  importExportLoading,
  importFile,
  importExportState,
  configTypes,
  loadHistory,
  handleRollback,
  handleExport,
  handleFileChange,
  handleImport,
  clearImportData,
  setActiveTab,
  resetHistoryFilter,
  updateHistoryFilter,
} = useSystemConfigManagement()

/**
 * 格式化 JSON 显示
 */
const formatJson = (jsonStr: string) => {
  try {
    return JSON.stringify(JSON.parse(jsonStr), null, 2)
  } catch {
    return jsonStr
  }
}

/**
 * 截取长文本
 */
const truncateText = (text: string, maxLength = 50) => {
  if (text.length <= maxLength) return text
  return text.slice(0, maxLength) + '...'
}

/**
 * 获取操作类型显示文本
 */
const getOperationLabel = (operation: string) => {
  const map: Record<string, string> = {
    Create: '创建',
    Update: '更新',
    Delete: '删除',
  }
  return map[operation] || operation
}

/**
 * 处理过滤条件变化
 */
const handleFilterChange = () => {
  loadHistory()
}

onMounted(() => {
  loadHistory()
})
</script>

<template>
  <section class="settings-page">
    <div class="settings-shell">
      <el-page-header content="Configuration Management" />

      <el-skeleton v-if="loading" :rows="8" animated class="state-block" />
      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" class="state-block" />

      <template v-else>
        <!-- Tab 导航 -->
        <el-card shadow="hover" class="main-card">
          <el-tabs v-model="activeTab" type="card" class="management-tabs">
            <!-- 配置历史 Tab -->
            <el-tab-pane :label="tabs[0].label" :name="tabs[0].key">
              <div class="history-section">
                <!-- 过滤条件 -->
                <div class="filter-bar">
                  <el-select
                    v-model="historyFilter.configType"
                    placeholder="选择配置类型"
                    clearable
                    style="width: 200px"
                    @change="handleFilterChange"
                  >
                    <el-option
                      v-for="type in configTypes"
                      :key="type.value"
                      :label="type.label"
                      :value="type.value"
                    />
                  </el-select>
                  <el-input
                    v-model="historyFilter.changedBy"
                    placeholder="操作人"
                    clearable
                    style="width: 200px"
                    @keyup.enter="handleFilterChange"
                  />
                  <el-button type="primary" :loading="historyLoading" @click="handleFilterChange">
                    查询
                  </el-button>
                  <el-button @click="resetHistoryFilter(); loadHistory()">
                    重置
                  </el-button>
                </div>

                <!-- 历史列表 -->
                <el-table
                  v-loading="historyLoading"
                  :data="historyList"
                  style="width: 100%; margin-top: 16px"
                  stripe
                >
                  <el-table-column prop="configType" label="配置类型" width="120" />
                  <el-table-column prop="configKey" label="配置键" width="180" />
                  <el-table-column prop="operation" label="操作" width="80">
                    <template #default="{ row }">
                      <el-tag :type="row.operation === 'Delete' ? 'danger' : 'primary'" size="small">
                        {{ getOperationLabel(row.operation) }}
                      </el-tag>
                    </template>
                  </el-table-column>
                  <el-table-column prop="previousValue" label="旧值" min-width="180">
                    <template #default="{ row }">
                      <el-tooltip :content="formatJson(row.previousValue)" placement="top">
                        <span class="truncate-text">{{ truncateText(row.previousValue) }}</span>
                      </el-tooltip>
                    </template>
                  </el-table-column>
                  <el-table-column prop="newValue" label="新值" min-width="180">
                    <template #default="{ row }">
                      <el-tooltip :content="formatJson(row.newValue)" placement="top">
                        <span class="truncate-text">{{ truncateText(row.newValue) }}</span>
                      </el-tooltip>
                    </template>
                  </el-table-column>
                  <el-table-column prop="changedBy" label="操作人" width="120" />
                  <el-table-column prop="changedAt" label="操作时间" width="180" />
                  <el-table-column label="操作" width="100" fixed="right">
                    <template #default="{ row }">
                      <el-button
                        type="primary"
                        size="small"
                        link
                        @click="handleRollback(row as ConfigHistoryResponse)"
                      >
                        回滚
                      </el-button>
                    </template>
                  </el-table-column>
                </el-table>

                <el-empty v-if="!hasHistoryData && !historyLoading" description="暂无配置历史" />
              </div>
            </el-tab-pane>

            <!-- 导入导出 Tab -->
            <el-tab-pane :label="tabs[1].label" :name="tabs[1].key">
              <div class="import-export-section">
                <el-row :gutter="24">
                  <!-- 导出配置 -->
                  <el-col :span="12">
                    <el-card class="export-card">
                      <template #header>
                        <div class="card-header">
                          <strong>导出配置</strong>
                        </div>
                      </template>
                      <div class="export-form">
                        <el-form label-width="100px">
                          <el-form-item label="配置类型">
                            <el-select v-model="importExportState.selectedConfigType" style="width: 100%">
                              <el-option
                                v-for="type in configTypes"
                                :key="type.value"
                                :label="type.label"
                                :value="type.value"
                              />
                            </el-select>
                          </el-form-item>
                          <el-form-item>
                            <el-button
                              type="primary"
                              :loading="importExportLoading"
                              @click="handleExport"
                            >
                              导出配置
                            </el-button>
                          </el-form-item>
                        </el-form>
                        <el-alert
                          v-if="importExportState.exportResult"
                          :title="importExportState.exportResult"
                          type="success"
                          show-icon
                          :closable="false"
                        />
                      </div>
                    </el-card>
                  </el-col>

                  <!-- 导入配置 -->
                  <el-col :span="12">
                    <el-card class="import-card">
                      <template #header>
                        <div class="card-header">
                          <strong>导入配置</strong>
                        </div>
                      </template>
                      <div class="import-form">
                        <el-form label-width="100px">
                          <el-form-item label="配置类型">
                            <el-select v-model="importExportState.selectedConfigType" style="width: 100%">
                              <el-option
                                v-for="type in configTypes"
                                :key="type.value"
                                :label="type.label"
                                :value="type.value"
                              />
                            </el-select>
                          </el-form-item>
                          <el-form-item label="选择文件">
                            <el-upload
                              :auto-upload="false"
                              :show-file-list="false"
                              :on-change="(file: any) => handleFileChange(file.raw as File)"
                              accept=".json"
                            >
                              <el-button type="primary">选择文件</el-button>
                            </el-upload>
                            <span v-if="importFile" class="file-name">{{ importFile.name }}</span>
                          </el-form-item>
                          <el-form-item label="或粘贴 JSON">
                            <el-input
                              v-model="importExportState.importData"
                              type="textarea"
                              :rows="8"
                              placeholder="粘贴配置 JSON 数据..."
                            />
                          </el-form-item>
                          <el-form-item>
                            <el-checkbox v-model="importExportState.overwriteExisting">覆盖已有配置</el-checkbox>
                          </el-form-item>
                          <el-form-item>
                            <el-button
                              type="primary"
                              :loading="importExportLoading"
                              :disabled="!importExportState.importData"
                              @click="handleImport"
                            >
                              导入配置
                            </el-button>
                            <el-button @click="clearImportData">清空</el-button>
                          </el-form-item>
                        </el-form>
                        <el-alert
                          v-if="importExportState.importResult"
                          :title="importExportState.importResult"
                          type="success"
                          show-icon
                          :closable="false"
                        />
                      </div>
                    </el-card>
                  </el-col>
                </el-row>
              </div>
            </el-tab-pane>
          </el-tabs>
        </el-card>
      </template>
    </div>
  </section>
</template>

<style scoped>
.settings-page {
  min-width: 1280px;
  padding: 20px 16px 40px;
}

.settings-shell {
  width: 100%;
  max-width: 1180px;
  margin: 0 auto;
}

.state-block {
  margin-top: 0.75rem;
}

.main-card {
  margin-top: 0.75rem;
}

.management-tabs {
  margin-top: 8px;
}

.filter-bar {
  display: flex;
  gap: 12px;
  align-items: center;
}

.truncate-text {
  display: inline-block;
  max-width: 180px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  vertical-align: bottom;
}

.card-header {
  display: flex;
  align-items: center;
}

.export-form,
.import-form {
  padding: 8px 0;
}

.file-name {
  margin-left: 12px;
  color: #64748b;
  font-size: 14px;
}

@media (max-width: 1365px) {
  .settings-page {
    overflow-x: auto;
    padding-bottom: 8px;
  }
}
</style>
