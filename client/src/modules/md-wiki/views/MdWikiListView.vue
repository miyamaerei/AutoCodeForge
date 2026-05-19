<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  useMdWiki,
} from '../composables/useMdWiki'

const route = useRoute()
const router = useRouter()

const {
  loading,
  loadingContent,
  error,
  repos,
  activeRepoKey,
  activeRepo,
  activePage,
  menuGroups,
  generatedAt,
  htmlContent,
  hasData,
  loadManifest,
  findPageIdBySlug,
  selectRepo,
  selectPage,
} = useMdWiki()

const pageToc = computed(() => activePage.value?.toc ?? [])
const expandedGroups = ref<Record<string, boolean>>({})

const suppressRouteSync = ref(false)

function ensureExpandedState(): void {
  const nextState: Record<string, boolean> = {}
  for (const group of menuGroups.value) {
    const isActiveTop = activePage.value?.id === group.top.id
    const isActiveChild = group.children.some((page) => page.id === activePage.value?.id)
    nextState[group.top.id] = expandedGroups.value[group.top.id] ?? (isActiveTop || isActiveChild)
  }
  expandedGroups.value = nextState
}

function toggleGroup(topId: string): void {
  expandedGroups.value[topId] = !expandedGroups.value[topId]
}

async function restoreFromQuery(): Promise<void> {
  const repoKey = typeof route.query.repo === 'string' ? route.query.repo : ''
  const pageSlug = typeof route.query.page === 'string' ? route.query.page : ''

  if (repoKey && repos.value.some((repo) => repo.key === repoKey)) {
    await selectRepo(repoKey, pageSlug)
    return
  }

  if (activeRepoKey.value) {
    if (pageSlug) {
      const pageId = findPageIdBySlug(pageSlug)
      if (pageId) {
        await selectPage(pageId)
      }
    }
    return
  }

  if (repos.value.length > 0) {
    await selectRepo(repos.value[0]!.key, pageSlug)
  }
}

async function syncQuery(): Promise<void> {
  if (suppressRouteSync.value) {
    return
  }

  const repoKey = activeRepoKey.value
  const pageSlug = activePage.value?.slug ?? ''
  const currentRepo = typeof route.query.repo === 'string' ? route.query.repo : ''
  const currentPage = typeof route.query.page === 'string' ? route.query.page : ''

  if (repoKey === currentRepo && pageSlug === currentPage) {
    return
  }

  suppressRouteSync.value = true
  try {
    await router.replace({
      query: {
        ...route.query,
        repo: repoKey || undefined,
        page: pageSlug || undefined,
      },
    })
  } finally {
    suppressRouteSync.value = false
  }
}

function onSelectRepo(value: string): void {
  void selectRepo(value)
}

function onSelectPage(pageId: string): void {
  void selectPage(pageId)
}

onMounted(async () => {
  await loadManifest()
  await restoreFromQuery()
  ensureExpandedState()
})

watch(menuGroups, () => {
  ensureExpandedState()
})

watch(
  () => [activeRepoKey.value, activePage.value?.id],
  () => {
    ensureExpandedState()
    void syncQuery()
  },
)

watch(
  () => route.query,
  async () => {
    if (suppressRouteSync.value) {
      return
    }
    await restoreFromQuery()
    ensureExpandedState()
  },
)
</script>

<template>
  <section class="md-wiki-view">
    <el-skeleton v-if="loading" :rows="8" animated class="state-box" />
    <el-alert
      v-else-if="error"
      class="state-box"
      type="error"
      show-icon
      :closable="false"
      :title="error"
    />
    <div v-else-if="!hasData" class="state-empty">
      <el-empty description="没有找到可渲染的 Markdown Wiki 页面" />
    </div>

    <div v-else class="wiki-shell">
      <aside class="wiki-left">
        <div class="panel-header">
          <h3>MD Wiki</h3>
          <p>Git 结构 Markdown 渲染</p>
        </div>

        <div class="repo-select">
          <label>仓库</label>
          <el-select :model-value="activeRepoKey" size="small" @update:model-value="onSelectRepo">
            <el-option
              v-for="repo in repos"
              :key="repo.key"
              :label="`${repo.repoName}@${repo.ref}`"
              :value="repo.key"
            />
          </el-select>
        </div>

        <div class="repo-meta" v-if="activeRepo">
          <div><strong>Repo:</strong> {{ activeRepo.repoName }}</div>
          <div><strong>Ref:</strong> {{ activeRepo.ref }}</div>
          <div><strong>Generated:</strong> {{ generatedAt || 'unknown' }}</div>
        </div>

        <div class="page-list">
          <section v-for="group in menuGroups" :key="group.top.id" class="page-group">
            <div class="group-head">
              <div
                class="group-toggle"
                role="button"
                tabindex="0"
                @click="toggleGroup(group.top.id)"
              >
                <span class="arrow" :class="{ expanded: expandedGroups[group.top.id] }" />
              </div>

              <div
                class="page-item level-1"
                :class="{ active: activePage?.id === group.top.id }"
                @click="onSelectPage(group.top.id)"
              >
                <div class="page-title">{{ group.top.title }}</div>
                <div class="page-sub">{{ group.children.length }} pages</div>
              </div>
            </div>

            <div v-show="expandedGroups[group.top.id]" class="group-children">
              <div
                v-for="page in group.children"
                :key="page.id"
                class="page-item level-2"
                :class="{ active: activePage?.id === page.id }"
                @click="onSelectPage(page.id)"
              >
                <span class="page-dot" />
                <div class="page-title">{{ page.title }}</div>
                <div class="page-sub">{{ page.slug }}</div>
              </div>
            </div>
          </section>
        </div>
      </aside>

      <main class="wiki-main">
        <div v-if="activePage" class="content-wrap">
          <header class="content-header">
            <h2>{{ activePage.title }}</h2>
            <div class="content-meta">
              <span>路径: {{ activePage.path || '/' }}</span>
              <span>文件: {{ activePage.filePath }}</span>
            </div>
          </header>

          <el-skeleton v-if="loadingContent" :rows="10" animated />
          <article v-else class="markdown-body" v-html="htmlContent" />
        </div>
      </main>

      <aside class="wiki-right">
        <h4>目录</h4>
        <div class="toc-list" v-if="pageToc.length > 0">
          <div v-for="(item, index) in pageToc" :key="item" class="toc-item">
            <span class="toc-index">{{ index + 1 }}</span>
            <span>{{ item }}</span>
          </div>
        </div>
        <el-empty v-else description="无目录" :image-size="72" />
      </aside>
    </div>
  </section>
</template>

<style scoped>
.md-wiki-view {
  width: 100%;
  height: 100%;
  overflow-x: auto;
}

.state-box {
  margin: 20px;
}

.state-empty {
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.wiki-shell {
  min-width: 1280px;
  height: 100%;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  overflow: hidden;
  background: #ffffff;
  display: grid;
  grid-template-columns: 340px minmax(760px, 1fr) 260px;
}

.wiki-left {
  background: #f8fafc;
  border-right: 1px solid #e2e8f0;
  padding: 16px;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
}

.panel-header {
  border-bottom: 1px solid #dbeafe;
  padding-bottom: 12px;
  margin-bottom: 16px;
}

.panel-header h3 {
  margin: 0;
  color: #0f172a;
  font-size: 16px;
}

.panel-header p {
  margin: 6px 0 0;
  font-size: 12px;
  color: #64748b;
}

.repo-select {
  margin-bottom: 14px;
}

.repo-select label {
  display: block;
  margin-bottom: 6px;
  color: #64748b;
  font-size: 12px;
  font-weight: 700;
}

.repo-meta {
  border: 1px solid #dbeafe;
  background: #eff6ff;
  color: #1e3a8a;
  border-radius: 10px;
  font-size: 12px;
  line-height: 1.6;
  padding: 10px 12px;
  margin-bottom: 14px;
}

.page-list {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.page-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.group-head {
  display: grid;
  grid-template-columns: 28px minmax(0, 1fr);
  gap: 8px;
}

.group-toggle {
  border: 1px solid #dbeafe;
  border-radius: 999px;
  background: #ffffff;
  color: #334155;
  cursor: pointer;
  width: 28px;
  height: 28px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.group-toggle:hover {
  border-color: #2563eb;
  background: #eff6ff;
}

.group-toggle:focus-visible {
  outline: 2px solid #93c5fd;
  outline-offset: 2px;
}

.arrow {
  width: 8px;
  height: 8px;
  border-right: 2px solid currentColor;
  border-bottom: 2px solid currentColor;
  transform: rotate(-45deg);
  transition: transform 0.2s ease;
}

.arrow.expanded {
  transform: rotate(45deg);
}

.group-children {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.page-item {
  text-align: left;
  width: 100%;
  border: 1px solid #dbeafe;
  border-radius: 12px;
  background: linear-gradient(180deg, #ffffff 0%, #f8fafc 100%);
  padding: 11px 12px;
  cursor: pointer;
  transition: border-color 0.2s ease, box-shadow 0.2s ease, transform 0.2s ease;
}

.page-item:hover {
  border-color: #1d4ed8;
  box-shadow: 0 6px 14px rgba(29, 78, 216, 0.12);
  transform: translateY(-1px);
}

.page-item.active {
  border-color: #1e3a8a;
  background: linear-gradient(135deg, #1e3a8a 0%, #0f766e 100%);
  color: #ffffff;
}

.page-item.level-1 {
  border-color: #bfdbfe;
}

.page-item.level-2 {
  margin-left: 16px;
  border-color: #e2e8f0;
  display: grid;
  grid-template-columns: 12px minmax(0, 1fr);
  column-gap: 8px;
  align-items: center;
}

.page-dot {
  width: 6px;
  height: 6px;
  border-radius: 999px;
  background: #60a5fa;
}

.page-title {
  font-weight: 700;
  font-size: 13px;
  line-height: 1.4;
}

.page-sub {
  margin-top: 2px;
  color: #475569;
  font-size: 11px;
}

.page-item.active .page-sub {
  color: rgba(255, 255, 255, 0.8);
}

.wiki-main {
  overflow-y: auto;
  padding: 24px;
  border-right: 1px solid #e2e8f0;
}

.content-header {
  margin-bottom: 18px;
  border-bottom: 1px solid #e2e8f0;
  padding-bottom: 12px;
}

.content-header h2 {
  margin: 0 0 8px;
  color: #0f172a;
  font-size: 24px;
}

.content-meta {
  color: #64748b;
  font-size: 12px;
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.markdown-body :deep(h1),
.markdown-body :deep(h2),
.markdown-body :deep(h3) {
  color: #0f172a;
}

.markdown-body :deep(p),
.markdown-body :deep(li) {
  color: #334155;
  line-height: 1.8;
}

.markdown-body :deep(pre) {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 12px;
  overflow-x: auto;
}

.wiki-right {
  background: linear-gradient(180deg, #f8fafc 0%, #f1f5f9 100%);
  padding: 16px;
  overflow-y: auto;
}

.wiki-right h4 {
  margin: 0 0 12px;
  color: #0f172a;
  font-size: 14px;
}

.toc-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.toc-item {
  border: 1px solid #bfdbfe;
  border-radius: 8px;
  background: #ffffff;
  padding: 8px 10px;
  font-size: 12px;
  color: #1e293b;
  display: grid;
  grid-template-columns: 24px minmax(0, 1fr);
  align-items: center;
  gap: 8px;
}

.toc-index {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 20px;
  height: 20px;
  border-radius: 999px;
  background: #dbeafe;
  color: #1d4ed8;
  font-weight: 700;
}
</style>
