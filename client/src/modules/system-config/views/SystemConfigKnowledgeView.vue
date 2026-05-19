<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useSystemConfigKnowledge, type NewKnowledgeSourceForm } from '../composables/useSystemConfigKnowledge'

const {
  loading,
  error,
  saving,
  saveError,
  saveSuccess,
  lastSavedAt,
  searchKeyword,
  sources,
  filteredSources,
  selectedSource,
  enabledCount,
  syncedCount,
  hasData,
  statusTagType,
  loadSources,
  saveSources,
  selectSource,
  toggleSourceEnabled,
  addSource,
  updateSource,
  deleteSource,
  refreshSource,
  repositories,
  repositoriesLoading,
  loadRepositories,
} = useSystemConfigKnowledge()

const showAddModal = ref(false)
const showDeleteConfirm = ref(false)

const newSourceForm = reactive<NewKnowledgeSourceForm>({
  name: '',
  type: 'markdown',
  location: '',
  contentFormat: 'markdown',
  refreshPolicy: 'daily',
  chunkSize: 800,
  chunkOverlap: 120,
  indexingScope: '',
  accessLevel: 'internal',
  repositoryIds: [],
})

const handleSave = async () => {
  await saveSources()
  if (saveSuccess.value) {
    ElMessage.success('知识源配置已保存')
  }
  if (saveError.value) {
    ElMessage.error(saveError.value)
  }
}

const handleToggleSource = (id: string, enabled: boolean) => {
  toggleSourceEnabled(id, enabled)
  ElMessage.success(enabled ? '知识源已启用' : '知识源已禁用')
}

const handleOpenAddModal = () => {
  newSourceForm.name = ''
  newSourceForm.type = 'markdown'
  newSourceForm.location = ''
  newSourceForm.contentFormat = 'markdown'
  newSourceForm.refreshPolicy = 'daily'
  newSourceForm.chunkSize = 800
  newSourceForm.chunkOverlap = 120
  newSourceForm.indexingScope = ''
  newSourceForm.accessLevel = 'internal'
  newSourceForm.repositoryIds = []
  showAddModal.value = true
}

const handleAddSource = () => {
  if (!newSourceForm.name.trim()) {
    ElMessage.error('请输入知识源名称')
    return
  }
  if (!newSourceForm.location.trim()) {
    ElMessage.error('请输入来源位置')
    return
  }
  addSource(newSourceForm)
  showAddModal.value = false
  ElMessage.success('知识源已添加')
}

const handleDeleteSource = () => {
  if (!selectedSource.value) return
  deleteSource(selectedSource.value.id)
  showDeleteConfirm.value = false
  ElMessage.success('知识源已删除')
}

const handleRefreshSource = async (id: string) => {
  await refreshSource(id)
  ElMessage.success('知识源刷新成功')
}

const handleToggleRepository = (repoId: string) => {
  if (!selectedSource.value) return
  const currentIds = selectedSource.value.repositoryIds || []
  const newIds = currentIds.includes(repoId)
    ? currentIds.filter(id => id !== repoId)
    : [...currentIds, repoId]
  updateSource(selectedSource.value.id, { repositoryIds: newIds })
}

onMounted(async () => {
  loadSources()
  await loadRepositories()
})
</script>

<template>
  <section class="settings-page">
    <div class="settings-shell">
      <el-page-header content="Knowledge" />

      <el-skeleton v-if="loading" :rows="10" animated class="state-block" />
      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" class="state-block" />
      <el-empty v-else-if="!hasData" description="暂无知识源配置" class="state-block" />

      <div v-else class="knowledge-layout">
        <el-card shadow="hover" class="catalog-card">
          <template #header>
            <div class="card-header">
              <strong>知识源目录</strong>
              <div class="card-actions">
                <span class="card-subtitle">当前 {{ sources.length }} 项</span>
                <el-button size="small" type="primary" icon="Plus" @click="handleOpenAddModal">
                  添加知识源
                </el-button>
              </div>
            </div>
          </template>

          <el-input v-model="searchKeyword" placeholder="搜索知识源名称、位置" clearable />

          <div class="catalog-list">
            <button
              v-for="item in filteredSources"
              :key="item.id"
              class="catalog-item"
              :class="{ active: selectedSource?.id === item.id }"
              @click="selectSource(item.id)"
            >
              <div class="catalog-item-header">
                <span class="catalog-item-title">{{ item.name }}</span>
                <el-tag :type="statusTagType(item.syncStatus)" size="small">{{ item.syncStatus }}</el-tag>
              </div>
              <p class="catalog-item-location">{{ item.location }}</p>
              <div class="catalog-item-meta">
                <span class="type-tag">{{ item.type }}</span>
                <span class="access-tag" :class="item.accessLevel">{{ item.accessLevel }}</span>
              </div>
            </button>
          </div>
        </el-card>

        <el-card shadow="hover" class="detail-card">
          <template #header>
            <div class="card-header">
              <strong>{{ selectedSource?.name || 'Knowledge Source' }}</strong>
              <span class="card-subtitle">{{ selectedSource?.location || '-' }}</span>
            </div>
          </template>

          <template v-if="selectedSource">
            <el-form label-width="160px" class="settings-form">
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="知识源状态">
                    <el-switch
                      :model-value="selectedSource.enabled"
                      @change="(val: boolean) => handleToggleSource(selectedSource!.id, val)"
                      active-text="启用"
                      inactive-text="禁用"
                    />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="同步状态">
                    <span class="status-badge">
                      <el-tag :type="statusTagType(selectedSource.syncStatus)" size="small">
                        {{ selectedSource.syncStatus }}
                      </el-tag>
                      <el-button
                        v-if="selectedSource.syncStatus !== 'syncing'"
                        size="small"
                        icon="Refresh"
                        @click="handleRefreshSource(selectedSource.id)"
                        class="refresh-btn"
                      >刷新</el-button>
                    </span>
                  </el-form-item>
                </el-col>
              </el-row>

              <el-divider content-position="left">源配置</el-divider>
              
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="来源类型">
                    <el-tag type="info" size="small">{{ selectedSource.type }}</el-tag>
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="内容格式">
                    <el-tag type="info" size="small">{{ selectedSource.contentFormat }}</el-tag>
                  </el-form-item>
                </el-col>
              </el-row>

              <el-form-item label="来源位置">
                <el-input :model-value="selectedSource.location" readonly class="readonly-input" />
              </el-form-item>

              <el-divider content-position="left">刷新策略</el-divider>
              
              <el-row :gutter="16">
                <el-col :span="8">
                  <el-form-item label="刷新频率">
                    <el-tag type="success" size="small">{{ selectedSource.refreshPolicy }}</el-tag>
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="新鲜度SLA">
                    <el-tag type="warning" size="small">{{ selectedSource.freshnessSla }}</el-tag>
                  </el-form-item>
                </el-col>
                <el-col :span="8">
                  <el-form-item label="上次同步">
                    <span class="last-sync">{{ selectedSource.lastSyncAt || '从未同步' }}</span>
                  </el-form-item>
                </el-col>
              </el-row>

              <el-divider content-position="left">分块配置</el-divider>
              
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="Chunk Size">
                    <el-input-number
                      :model-value="selectedSource.chunkSize"
                      :min="100"
                      :max="4000"
                      :step="50"
                      @change="(val: number) => updateSource(selectedSource!.id, { chunkSize: val })"
                    />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="Overlap">
                    <el-input-number
                      :model-value="selectedSource.chunkOverlap"
                      :min="0"
                      :max="1000"
                      :step="20"
                      @change="(val: number) => updateSource(selectedSource!.id, { chunkOverlap: val })"
                    />
                  </el-form-item>
                </el-col>
              </el-row>

              <el-divider content-position="left">索引范围</el-divider>
              <el-alert type="info" :closable="false" class="info-alert">
                <ul class="scope-list">
                  <li v-for="(scope, index) in selectedSource.indexingScope" :key="index">
                    {{ scope }}
                  </li>
                </ul>
              </el-alert>

              <el-divider content-position="left">访问范围</el-divider>
              <el-radio-group :model-value="selectedSource.accessLevel" class="access-group">
                <el-radio
                  v-for="option in [{ value: 'public', label: 'Public' }, { value: 'internal', label: 'Internal' }, { value: 'restricted', label: 'Restricted' }]"
                  :key="option.value"
                  :value="option.value"
                  @change="(val: string) => updateSource(selectedSource!.id, { accessLevel: val as 'public' | 'internal' | 'restricted' })"
                >{{ option.label }}</el-radio>
              </el-radio-group>

              <el-divider content-position="left">关联仓库</el-divider>
              <el-alert type="warning" :closable="false" class="info-alert">
                <p class="helper-text">该知识源适用的仓库（可多选）：</p>
                <div class="repo-selector">
                  <el-skeleton v-if="repositoriesLoading" :rows="2" animated />
                  <el-checkbox-group
                    v-else-if="repositories.length > 0"
                    v-model="selectedSource.repositoryIds"
                    class="repo-checkbox-group"
                  >
                    <el-checkbox
                      v-for="repo in repositories"
                      :key="repo.id"
                      :label="repo.id"
                      class="repo-checkbox"
                    >
                      <div class="repo-item">
                        <strong>{{ repo.name }}</strong>
                        <span class="repo-url">{{ repo.url }}</span>
                      </div>
                    </el-checkbox>
                  </el-checkbox-group>
                  <el-empty v-else description="暂无仓库可关联" :image-size="60" />
                </div>
                <p class="helper-text" style="margin-top: 8px; font-size: 12px;">
                  知识源只会在关联的仓库中生效，留空表示适用于所有仓库
                </p>
              </el-alert>

              <div class="source-actions">
                <el-button size="small" type="danger" icon="Delete" @click="showDeleteConfirm = true">删除知识源</el-button>
              </div>
            </el-form>
          </template>
        </el-card>

        <el-card shadow="never" class="status-card">
          <template #header>
            <div class="card-header">
              <strong>状态面板</strong>
              <span class="card-subtitle">统一统计与操作</span>
            </div>
          </template>

          <el-descriptions :column="1" border size="small" class="status-meta">
            <el-descriptions-item label="Total Sources">{{ sources.length }}</el-descriptions-item>
            <el-descriptions-item label="Enabled">{{ enabledCount }}</el-descriptions-item>
            <el-descriptions-item label="Synced">{{ syncedCount }}</el-descriptions-item>
            <el-descriptions-item label="Selected">{{ selectedSource?.name || '-' }}</el-descriptions-item>
            <el-descriptions-item label="Last Saved At">{{ lastSavedAt || '-' }}</el-descriptions-item>
          </el-descriptions>

          <el-alert
            v-if="saveSuccess"
            title="配置保存成功"
            type="success"
            show-icon
            :closable="false"
            class="save-state"
          />
          <el-alert
            v-if="saveError"
            :title="saveError"
            type="error"
            show-icon
            :closable="false"
            class="save-state"
          />

          <div class="actions sticky-actions">
            <el-button type="success" :loading="saving" @click="handleSave">保存全部配置</el-button>
          </div>
        </el-card>
      </div>
    </div>

    <el-dialog title="添加新知识源" v-model="showAddModal" width="700px">
      <el-form label-width="130px" class="add-source-form">
        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="知识源名称" required>
              <el-input v-model="newSourceForm.name" placeholder="输入知识源名称" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="来源类型" required>
              <el-select v-model="newSourceForm.type">
                <el-option label="Markdown" value="markdown" />
                <el-option label="Remote Wiki" value="remote-wiki" />
                <el-option label="Repository" value="repository" />
                <el-option label="URL" value="url" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="16">
          <el-col :span="12">
            <el-form-item label="来源位置" required>
              <el-input v-model="newSourceForm.location" placeholder="例如 /docs 或 https://example.com" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="内容格式" required>
              <el-select v-model="newSourceForm.contentFormat">
                <el-option label="Markdown" value="markdown" />
                <el-option label="Docs" value="docs" />
                <el-option label="Wiki" value="wiki" />
                <el-option label="Code" value="code" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="16">
          <el-col :span="8">
            <el-form-item label="刷新策略" required>
              <el-select v-model="newSourceForm.refreshPolicy">
                <el-option label="Manual" value="manual" />
                <el-option label="Hourly" value="hourly" />
                <el-option label="Daily" value="daily" />
                <el-option label="Weekly" value="weekly" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="Chunk Size">
              <el-input-number v-model="newSourceForm.chunkSize" :min="100" :max="4000" :step="50" />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="Overlap">
              <el-input-number v-model="newSourceForm.chunkOverlap" :min="0" :max="1000" :step="20" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="索引范围">
          <el-input v-model="newSourceForm.indexingScope" type="textarea" :rows="3" placeholder="每行一个范围，例如：&#10;main&#10;docs/**" />
        </el-form-item>

        <el-form-item label="访问范围" required>
          <el-radio-group v-model="newSourceForm.accessLevel">
            <el-radio value="public">Public</el-radio>
            <el-radio value="internal">Internal</el-radio>
            <el-radio value="restricted">Restricted</el-radio>
          </el-radio-group>
        </el-form-item>

        <el-form-item label="关联仓库">
          <el-select
            v-model="newSourceForm.repositoryIds"
            multiple
            :loading="repositoriesLoading"
            placeholder="选择仓库"
            style="width: 100%"
          >
            <el-option
              v-for="repo in repositories"
              :key="repo.id"
              :label="repo.name"
              :value="repo.id"
            >
              <span>{{ repo.name }}</span>
              <span style="color: #999; font-size: 12px; margin-left: 10px">{{ repo.url }}</span>
            </el-option>
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddModal = false">取消</el-button>
        <el-button type="primary" @click="handleAddSource">添加</el-button>
      </template>
    </el-dialog>

    <el-dialog title="确认删除" v-model="showDeleteConfirm">
      <p>确定要删除知识源 <strong>{{ selectedSource?.name }}</strong> 吗？此操作无法撤销。</p>
      <template #footer>
        <el-button @click="showDeleteConfirm = false">取消</el-button>
        <el-button type="danger" @click="handleDeleteSource">删除</el-button>
      </template>
    </el-dialog>
  </section>
</template>

<style scoped>
.settings-page {
  min-width: 1280px;
  padding: 20px 16px 40px;
}

.settings-shell {
  width: 100%;
  max-width: 1580px;
  margin: 0 auto;
}

.state-block {
  margin-top: 12px;
}

.knowledge-layout {
  margin-top: 12px;
  display: grid;
  grid-template-columns: 320px minmax(720px, 1fr) 340px;
  gap: 16px;
  align-items: start;
}

.catalog-card,
.detail-card,
.status-card {
  min-height: 620px;
}

.card-header {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 12px;
}

.card-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.card-subtitle {
  color: #64748b;
  font-size: 13px;
}

.catalog-list {
  margin-top: 12px;
  display: grid;
  gap: 10px;
  max-height: 700px;
  overflow: auto;
}

.catalog-item {
  text-align: left;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  background: #fff;
  padding: 12px;
  cursor: pointer;
}

.catalog-item.active {
  border-color: #6366f1;
  box-shadow: 0 0 0 2px rgba(99, 102, 241, 0.12);
}

.catalog-item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 8px;
}

.catalog-item-title {
  font-weight: 600;
}

.catalog-item-location {
  margin: 6px 0;
  color: #64748b;
  font-size: 12px;
  line-height: 1.4;
}

.catalog-item-meta {
  display: flex;
  gap: 6px;
}

.type-tag {
  font-size: 11px;
  padding: 2px 8px;
  background: #f1f5f9;
  color: #475569;
  border-radius: 4px;
}

.access-tag {
  font-size: 11px;
  padding: 2px 8px;
  border-radius: 4px;
}

.access-tag.public {
  background: #dcfce7;
  color: #166534;
}

.access-tag.internal {
  background: #fef3c7;
  color: #92400e;
}

.access-tag.restricted {
  background: #fee2e2;
  color: #991b1b;
}

.settings-form {
  max-width: 100%;
}

.info-alert {
  margin-bottom: 16px;
}

.scope-list {
  margin: 0;
  padding-left: 20px;
}

.scope-list li {
  margin: 4px 0;
}

.readonly-input {
  background-color: #f8fafc;
}

.status-badge {
  display: flex;
  align-items: center;
  gap: 8px;
}

.refresh-btn {
  padding: 4px 10px;
}

.last-sync {
  color: #64748b;
  font-size: 13px;
}

.access-group {
  display: flex;
  gap: 16px;
}

.repo-selector {
  margin-top: 8px;
}

.repo-checkbox-group {
  display: grid;
  gap: 8px;
}

.repo-checkbox {
  display: block;
  height: auto;
  padding: 8px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  margin-right: 0;
}

.repo-checkbox:hover {
  background-color: #f8fafc;
}

.repo-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.repo-url {
  font-size: 12px;
  color: #64748b;
}

.helper-text {
  margin: 0;
}

.source-actions {
  display: flex;
  gap: 8px;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid #e2e8f0;
}

.add-source-form {
  max-width: 100%;
}

.actions {
  margin-top: 8px;
}

.status-card {
  position: sticky;
  top: 10px;
}

.status-meta {
  margin-top: 8px;
}

.save-state {
  margin-top: 10px;
}

.sticky-actions {
  margin-top: 14px;
}

@media (max-width: 1365px) {
  .settings-page {
    overflow-x: auto;
    padding-bottom: 10px;
  }

  .knowledge-layout {
    min-width: 1320px;
  }
}
</style>
