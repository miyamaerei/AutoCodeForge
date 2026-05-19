import MarkdownIt from 'markdown-it'
import { computed, ref } from 'vue'

interface ManifestPageRecord {
  title?: string
  slug?: string
  repoName?: string
  ref?: string
  parent?: string
  topTitle?: string
  level?: number
  path?: string
  filePath?: string
  order?: number
  toc?: string[]
}

interface ManifestRepoRecord {
  repoName?: string
  ref?: string
  pages?: ManifestPageRecord[]
}

interface ManifestPayload {
  generatedAt?: string
  repos?: ManifestRepoRecord[]
}

export interface MdWikiPageModel {
  id: string
  title: string
  slug: string
  repoName: string
  ref: string
  parent: string
  topTitle: string
  level: number
  path: string
  filePath: string
  order: number
  toc: string[]
}

export interface MdWikiMenuGroupModel {
  top: MdWikiPageModel
  children: MdWikiPageModel[]
}

export interface MdWikiRepoModel {
  key: string
  repoName: string
  ref: string
}

const markdown = new MarkdownIt({
  html: false,
  linkify: true,
  typographer: true,
})

function normalize(value: string | undefined, fallback: string): string {
  const text = value?.trim() ?? ''
  return text.length > 0 ? text : fallback
}

function buildRepoKey(repoName: string, ref: string): string {
  return `${repoName}::${ref}`
}

function toPageModel(page: ManifestPageRecord, index: number, repo: MdWikiRepoModel): MdWikiPageModel | null {
  const title = normalize(page.title, '')
  const slug = normalize(page.slug, '')
  const filePath = normalize(page.filePath, '')

  if (!title || !slug || !filePath) {
    return null
  }

  return {
    id: `${repo.key}:${slug}`,
    title,
    slug,
    repoName: normalize(page.repoName, repo.repoName),
    ref: normalize(page.ref, repo.ref),
    parent: normalize(page.parent, ''),
    topTitle: normalize(page.topTitle, title),
    level: page.level === 2 ? 2 : 1,
    path: normalize(page.path, '/'),
    filePath,
    order: page.order ?? index + 1,
    toc: (page.toc ?? []).filter((item) => item.trim().length > 0),
  }
}

export function useMdWiki() {
  const loading = ref(false)
  const loadingContent = ref(false)
  const error = ref<string | null>(null)
  const repos = ref<MdWikiRepoModel[]>([])
  const pages = ref<MdWikiPageModel[]>([])
  const activeRepoKey = ref('')
  const activePageId = ref('')
  const htmlContent = ref('')
  const generatedAt = ref('')

  const activeRepo = computed(() => repos.value.find((repo) => repo.key === activeRepoKey.value) ?? null)

  const filteredPages = computed(() => {
    if (!activeRepo.value) {
      return []
    }

    return pages.value
      .filter((page) => page.repoName === activeRepo.value?.repoName && page.ref === activeRepo.value.ref)
      .sort((a, b) => a.order - b.order)
  })

  const menuGroups = computed<MdWikiMenuGroupModel[]>(() => {
    const topPages = filteredPages.value
      .filter((page) => page.level === 1)
      .sort((a, b) => a.order - b.order)

    return topPages.map((top) => ({
      top,
      children: filteredPages.value
        .filter((page) => page.level === 2 && page.topTitle === top.title)
        .sort((a, b) => a.order - b.order),
    }))
  })

  const activePage = computed(
    () => filteredPages.value.find((page) => page.id === activePageId.value) ?? filteredPages.value[0] ?? null,
  )

  const hasData = computed(() => repos.value.length > 0 && pages.value.length > 0)

  async function loadManifest(): Promise<void> {
    loading.value = true
    error.value = null

    try {
      const response = await fetch('/AutoCodeForge.wiki/manifest.json', {
        method: 'GET',
        cache: 'no-store',
      })

      if (!response.ok) {
        throw new Error(`读取 manifest 失败: ${response.status}`)
      }

      const payload = (await response.json()) as ManifestPayload
      generatedAt.value = normalize(payload.generatedAt, '')

      const mappedRepos = (payload.repos ?? [])
        .map((repo) => {
          const repoName = normalize(repo.repoName, '')
          const ref = normalize(repo.ref, '')
          if (!repoName || !ref) {
            return null
          }
          return {
            key: buildRepoKey(repoName, ref),
            repoName,
            ref,
          }
        })
        .filter((repo): repo is MdWikiRepoModel => repo !== null)

      const mappedPages = (payload.repos ?? []).flatMap((repo) => {
        const repoName = normalize(repo.repoName, '')
        const ref = normalize(repo.ref, '')
        const repoModel: MdWikiRepoModel = {
          key: buildRepoKey(repoName, ref),
          repoName,
          ref,
        }

        return (repo.pages ?? [])
          .map((page, index) => toPageModel(page, index, repoModel))
          .filter((page): page is MdWikiPageModel => page !== null)
      })

      repos.value = mappedRepos
      pages.value = mappedPages

      if (repos.value.length === 0 || pages.value.length === 0) {
        error.value = 'manifest 已加载，但未发现仓库或页面数据。'
        return
      }

      if (!repos.value.some((repo) => repo.key === activeRepoKey.value)) {
        activeRepoKey.value = repos.value[0]!.key
      }

      if (!filteredPages.value.some((page) => page.id === activePageId.value)) {
        activePageId.value = filteredPages.value[0]?.id ?? ''
      }

      if (activePage.value) {
        await loadActivePage()
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : '加载 Wiki 清单失败'
    } finally {
      loading.value = false
    }
  }

  async function loadActivePage(): Promise<void> {
    if (!activePage.value) {
      htmlContent.value = ''
      return
    }

    loadingContent.value = true
    error.value = null

    try {
      const response = await fetch(`/AutoCodeForge.wiki/${activePage.value.filePath}`, {
        method: 'GET',
        cache: 'no-store',
      })

      if (!response.ok) {
        throw new Error(`读取 Markdown 失败: ${response.status}`)
      }

      const markdownText = await response.text()
      htmlContent.value = markdown.render(markdownText)
    } catch (err) {
      htmlContent.value = ''
      error.value = err instanceof Error ? err.message : '读取 Markdown 页面失败'
    } finally {
      loadingContent.value = false
    }
  }

  function findPageIdBySlug(slug: string): string | null {
    const normalized = slug.trim()
    if (!normalized) {
      return null
    }
    return filteredPages.value.find((page) => page.slug === normalized)?.id ?? null
  }

  async function selectRepo(key: string, preferredPageSlug?: string): Promise<void> {
    activeRepoKey.value = key
    const preferredPageId = preferredPageSlug ? findPageIdBySlug(preferredPageSlug) : null
    activePageId.value = preferredPageId ?? filteredPages.value[0]?.id ?? ''

    if (activePageId.value) {
      await loadActivePage()
    } else {
      htmlContent.value = ''
    }
  }

  async function selectPage(pageId: string): Promise<void> {
    activePageId.value = pageId
    await loadActivePage()
  }

  return {
    loading,
    loadingContent,
    error,
    repos,
    pages,
    activeRepoKey,
    activeRepo,
    activePageId,
    activePage,
    filteredPages,
    menuGroups,
    generatedAt,
    htmlContent,
    hasData,
    loadManifest,
    loadActivePage,
    findPageIdBySlug,
    selectRepo,
    selectPage,
  }
}
