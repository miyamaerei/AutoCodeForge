<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useConsoleWiki, type WikiPageModel } from '../composables/useConsoleWiki'

const {
  loading,
  error,
  wikiMeta,
  repositories,
  activeRepositoryKey,
  pages,
  hasData,
  loadPages,
  setActiveRepository,
} = useConsoleWiki()

const selectedPageId = ref<string | null>(null)

const selectedRepository = computed({
  get: () => activeRepositoryKey.value,
  set: (value: string) => setActiveRepository(value),
})

const selectedRepositoryInfo = computed(() =>
  repositories.value.find((repo) => repo.key === selectedRepository.value) ?? null,
)

const groupedPages = computed(() => {
  const groupMap = new Map<string, WikiPageModel[]>()

  for (const page of pages.value) {
    const groupKey = page.category || '根目录'
    const current = groupMap.get(groupKey) ?? []
    current.push(page)
    groupMap.set(groupKey, current)
  }

  return Array.from(groupMap.entries())
    .map(([group, items]) => ({
      group,
      items: [...items].sort((a, b) => a.menuOrder - b.menuOrder),
    }))
    .sort((a, b) => a.group.localeCompare(b.group, 'zh-CN'))
})

const selectedPage = computed(() => {
  if (!selectedPageId.value) {
    return null
  }
  return pages.value.find((page) => page.id === selectedPageId.value) ?? null
})

watch(
  pages,
  (newPages) => {
    if (newPages.length === 0) {
      selectedPageId.value = null
      return
    }

    const stillExists = newPages.some((page) => page.id === selectedPageId.value)
    if (!stillExists) {
      selectedPageId.value = newPages[0]!.id
    }
  },
  { immediate: true },
)

function selectPage(page: WikiPageModel): void {
  selectedPageId.value = page.id
}

function sectionId(label: string): string {
  return `toc-${label.replace(/\s+/g, '-').toLowerCase()}`
}

function scrollToSection(label: string): void {
  const section = document.getElementById(sectionId(label))
  section?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

onMounted(async () => {
  await loadPages()
})
</script>

<template>
  <section class="wiki-view">
    <el-skeleton v-if="loading" :rows="6" animated class="wiki-skeleton" />
    <el-alert
      v-else-if="error"
      class="wiki-error"
      type="error"
      show-icon
      :closable="false"
      :title="error"
    />
    <div v-else-if="!hasData" class="empty-state">
      <el-empty description="Wiki JSON 已加载，但没有可展示的页面" />
    </div>

    <div v-else class="wiki-shell">
      <aside class="wiki-sidebar">
        <div class="sidebar-header">
          <h3>Wiki</h3>
          <p>仓库菜单与页面目录</p>
        </div>

        <div class="repo-selector">
          <label>选择仓库</label>
          <el-select v-model="selectedRepository" size="small">
            <el-option
              v-for="repo in repositories"
              :key="repo.key"
              :label="`${repo.repoName}@${repo.ref}`"
              :value="repo.key"
            />
          </el-select>
        </div>

        <div class="wiki-meta" v-if="selectedRepositoryInfo">
          <div>Author: {{ selectedRepositoryInfo.author }}</div>
          <div>Updated: {{ selectedRepositoryInfo.updatedAt || 'unknown' }}</div>
        </div>

        <div class="wiki-list">
          <section v-for="group in groupedPages" :key="group.group" class="wiki-group">
            <h4>{{ group.group }}</h4>
            <button
              v-for="page in group.items"
              :key="page.id"
              type="button"
              class="wiki-item"
              :class="{ active: selectedPage?.id === page.id }"
              @click="selectPage(page)"
            >
              <div class="wiki-icon">文</div>
              <div class="wiki-info">
                <div class="wiki-title">{{ page.title }}</div>
                <div class="wiki-category">ref: {{ page.ref }}</div>
              </div>
            </button>
          </section>
        </div>
      </aside>

      <main class="wiki-content">
        <div v-if="selectedPage" class="content-area">
          <div class="content-header">
            <h2>{{ selectedPage.title }}</h2>
            <div class="content-meta">
              <span>仓库: {{ selectedPage.repoName }}</span>
              <span>分支: {{ selectedPage.ref }}</span>
              <span>作者: {{ selectedPage.author }}</span>
              <span>更新时间: {{ selectedPage.updatedAt || 'unknown' }}</span>
              <span>分类: {{ selectedPage.category }}</span>
              <span>笔记数: {{ selectedPage.notes.length }}</span>
            </div>
          </div>

          <div class="content-body">
            <el-card shadow="never" class="content-card">
              <section :id="sectionId('页面目的')" class="wiki-section">
                <h3>页面目的</h3>
                <p>{{ selectedPage.purpose }}</p>
              </section>

              <section :id="sectionId('页面笔记')" class="wiki-section">
                <h3>页面笔记</h3>
                <ul v-if="selectedPage.notes.length > 0" class="note-list">
                  <li v-for="(note, index) in selectedPage.notes" :key="`${selectedPage.id}-${index}`">{{ note }}</li>
                </ul>
                <el-empty v-else description="该页面暂无 page_notes" :image-size="72" />
              </section>

              <section :id="sectionId('页面元信息')" class="wiki-section">
                <h3>页面元信息</h3>
                <ul class="note-list">
                  <li>slug: {{ selectedPage.slug }}</li>
                  <li>schemaVersion: {{ wikiMeta.schemaVersion }}</li>
                  <li>generatedAt: {{ wikiMeta.generatedAt || 'unknown' }}</li>
                </ul>
              </section>
            </el-card>
          </div>
        </div>

        <div v-else class="empty-state">
          <el-empty description="请选择左侧 Wiki 页面" />
        </div>
      </main>

      <aside class="wiki-toc" v-if="selectedPage">
        <h4>页面目录</h4>
        <button
          v-for="item in selectedPage.toc"
          :key="item"
          type="button"
          class="toc-item"
          @click="scrollToSection(item)"
        >
          {{ item }}
        </button>
        <button type="button" class="toc-item" @click="scrollToSection('页面元信息')">页面元信息</button>
      </aside>
    </div>
  </section>
</template>

<style scoped>
.wiki-view {
  width: 100%;
  height: 100%;
  overflow-x: auto;
}

.wiki-skeleton,
.wiki-error {
  margin: 20px;
}

.wiki-shell {
  display: grid;
  grid-template-columns: 360px minmax(760px, 1fr) 260px;
  gap: 0;
  min-width: 1280px;
  width: 100%;
  height: 100%;
  background: white;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
}

.wiki-sidebar {
  background: #f8fafc;
  border-right: 1px solid #e2e8f0;
  padding: 16px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}

.sidebar-header {
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid #e2e8f0;
}

.sidebar-header p {
  margin: 6px 0 0;
  color: #64748b;
  font-size: 12px;
}

.sidebar-header h3 {
  font-size: 16px;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}

.repo-selector {
  margin-bottom: 16px;
}

.wiki-meta {
  border: 1px solid #dbeafe;
  background: #eff6ff;
  color: #1e3a8a;
  border-radius: 10px;
  padding: 10px 12px;
  margin-bottom: 16px;
  font-size: 12px;
  line-height: 1.6;
}

.repo-selector label {
  display: block;
  font-size: 12px;
  font-weight: 600;
  color: #64748b;
  margin-bottom: 6px;
}

.wiki-list {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 16px;
}

.wiki-group h4 {
  margin: 0 0 8px;
  color: #334155;
  font-size: 12px;
  font-weight: 700;
}

.wiki-item {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 10px 12px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
  text-align: left;
}

.wiki-item:hover {
  border-color: #cbd5e1;
  background: #f1f5f9;
}

.wiki-item.active {
  background: linear-gradient(135deg, #1e3a8a 0%, #0f766e 100%);
  color: white;
  border-color: #1e3a8a;
}

.wiki-icon {
  width: 24px;
  height: 24px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 700;
  background: #e2e8f0;
  color: #1e293b;
}

.wiki-item.active .wiki-icon {
  background: rgba(255, 255, 255, 0.22);
  color: #f8fafc;
}

.wiki-info {
  flex: 1;
  min-width: 0;
}

.wiki-title {
  font-weight: 600;
  font-size: 13px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.wiki-category {
  font-size: 11px;
  color: #94a3b8;
  margin-top: 2px;
}

.wiki-item.active .wiki-category {
  color: rgba(255, 255, 255, 0.7);
}

.sidebar-footer {
  padding-top: 12px;
  border-top: 1px solid #e2e8f0;
}

.wiki-content {
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  border-right: 1px solid #e2e8f0;
}

.content-area {
  display: flex;
  flex-direction: column;
  height: 100%;
  padding: 24px;
}

.content-header {
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid #e2e8f0;
}

.content-header h2 {
  font-size: 24px;
  font-weight: 700;
  color: #0f172a;
  margin: 0 0 8px;
}

.content-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
  font-size: 13px;
  color: #64748b;
}

.content-body {
  flex: 1;
  overflow-y: auto;
  margin-bottom: 16px;
}

.content-card {
  border-color: #e2e8f0;
}

.wiki-section + .wiki-section {
  margin-top: 24px;
}

.wiki-section h3 {
  font-size: 16px;
  font-weight: 700;
  color: #0f172a;
  margin: 0 0 10px;
}

.wiki-section p {
  margin: 0;
  line-height: 1.8;
  color: #4b5563;
}

.note-list {
  margin: 0;
  padding-left: 24px;
  line-height: 1.8;
  color: #4b5563;
}

.wiki-toc {
  background: #f8fafc;
  padding: 18px 14px;
  overflow-y: auto;
}

.wiki-toc h4 {
  margin: 0 0 12px;
  color: #0f172a;
  font-size: 14px;
  font-weight: 700;
}

.toc-item {
  width: 100%;
  display: block;
  text-align: left;
  border: 1px solid #cbd5e1;
  background: #ffffff;
  color: #1e293b;
  border-radius: 8px;
  padding: 8px 10px;
  margin-bottom: 8px;
  cursor: pointer;
  transition: border-color 0.2s ease, background-color 0.2s ease;
}

.toc-item:hover {
  border-color: #0f766e;
  background: #f0fdfa;
}

.empty-state {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
}
</style>
