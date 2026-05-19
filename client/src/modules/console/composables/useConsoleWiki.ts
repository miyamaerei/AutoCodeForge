import { computed, ref } from 'vue'

interface WikiNoteRecord {
  content?: string
}

interface WikiMenuRecord {
  group?: string
  order?: number
}

interface WikiRawPage {
  title?: string
  purpose?: string
  parent?: string
  repoName?: string
  ref?: string
  author?: string
  updatedAt?: string
  slug?: string
  menu?: WikiMenuRecord
  toc?: string[]
  page_notes?: WikiNoteRecord[]
}

interface WikiRepositoryRecord {
  repoName?: string
  ref?: string
  author?: string
  updatedAt?: string
  pages?: WikiRawPage[]
}

interface WikiPayload {
  schemaVersion?: string
  generatedAt?: string
  repoName?: string
  ref?: string
  author?: string
  updatedAt?: string
  repositories?: WikiRepositoryRecord[]
  pages?: WikiRawPage[]
}

export interface WikiMetaModel {
  schemaVersion: string
  generatedAt: string
}

export interface WikiRepositoryModel {
  key: string
  repoName: string
  ref: string
  author: string
  updatedAt: string
}

export interface WikiPageModel {
  id: string
  title: string
  category: string
  purpose: string
  notes: string[]
  toc: string[]
  repoName: string
  ref: string
  author: string
  updatedAt: string
  slug: string
  menuOrder: number
}

function toPageId(title: string, index: number): string {
  const normalized = title
    .toLowerCase()
    .replace(/[^a-z0-9\u4e00-\u9fa5]+/g, '-')
    .replace(/^-+|-+$/g, '')

  return normalized.length > 0 ? normalized : `wiki-page-${index + 1}`
}

function normalizeText(value: string | undefined, fallback: string): string {
  const normalized = value?.trim() ?? ''
  return normalized.length > 0 ? normalized : fallback
}

function mapPage(
  rawPage: WikiRawPage,
  index: number,
  defaults: Pick<WikiRepositoryModel, 'repoName' | 'ref' | 'author' | 'updatedAt'>,
): WikiPageModel | null {
  const title = rawPage.title?.trim() ?? ''
  if (!title) {
    return null
  }

  const purpose = normalizeText(rawPage.purpose, '暂无目的说明')
  const category = normalizeText(rawPage.parent ?? rawPage.menu?.group, '根目录')
  const toc = (rawPage.toc ?? [])
    .map((item) => item.trim())
    .filter((item) => item.length > 0)

  const notes = (rawPage.page_notes ?? [])
    .map((note) => note.content?.trim() ?? '')
    .filter((content) => content.length > 0)

  const repoName = normalizeText(rawPage.repoName, defaults.repoName)
  const refName = normalizeText(rawPage.ref, defaults.ref)
  const author = normalizeText(rawPage.author, defaults.author)
  const updatedAt = normalizeText(rawPage.updatedAt, defaults.updatedAt)
  const slug = normalizeText(rawPage.slug, toPageId(title, index))

  return {
    id: toPageId(title, index),
    title,
    category,
    purpose,
    notes,
    toc: toc.length > 0 ? toc : ['页面目的', '页面笔记'],
    repoName,
    ref: refName,
    author,
    updatedAt,
    slug,
    menuOrder: rawPage.menu?.order ?? index + 1,
  }
}

function repoKey(repoName: string, refName: string): string {
  return `${repoName}::${refName}`
}

function buildLoadError(errors: string[]): string {
  if (errors.length === 0) {
    return '无法加载 Wiki 数据。'
  }
  return `无法加载 Wiki 数据。已尝试路径: ${errors.join(' | ')}`
}

export function useConsoleWiki() {
  const loading = ref(false)
  const error = ref<string | null>(null)
  const allPages = ref<WikiPageModel[]>([])
  const repositories = ref<WikiRepositoryModel[]>([])
  const activeRepositoryKey = ref('')
  const wikiMeta = ref<WikiMetaModel>({
    schemaVersion: 'unknown',
    generatedAt: '',
  })

  const pages = computed(() => {
    if (!activeRepositoryKey.value) {
      return allPages.value
    }

    const selectedRepo = repositories.value.find((repo) => repo.key === activeRepositoryKey.value)
    if (!selectedRepo) {
      return allPages.value
    }

    return allPages.value.filter(
      (page) => page.repoName === selectedRepo.repoName && page.ref === selectedRepo.ref,
    )
  })

  const hasData = computed(() => pages.value.length > 0)

  function setActiveRepository(key: string): void {
    activeRepositoryKey.value = key
  }

  async function loadPages(): Promise<void> {
    loading.value = true
    error.value = null

    const attempts: string[] = []
    const candidates = ['/.devin/wiki.json', '/wiki.json']

    try {
      for (const url of candidates) {
        try {
          const response = await fetch(url, {
            method: 'GET',
            cache: 'no-store',
          })

          if (!response.ok) {
            attempts.push(`${url} (${response.status})`)
            continue
          }

          const payload = (await response.json()) as WikiPayload
          wikiMeta.value = {
            schemaVersion: payload.schemaVersion?.trim() ?? 'legacy',
            generatedAt: payload.generatedAt?.trim() ?? payload.updatedAt?.trim() ?? '',
          }

          const defaultRepo: Pick<WikiRepositoryModel, 'repoName' | 'ref' | 'author' | 'updatedAt'> = {
            repoName: normalizeText(payload.repoName, 'unknown-repo'),
            ref: normalizeText(payload.ref, 'unknown-ref'),
            author: normalizeText(payload.author, 'unknown-author'),
            updatedAt: normalizeText(payload.updatedAt, ''),
          }

          const mappedRepos = (payload.repositories ?? [])
            .map((repo) => {
              const repoName = normalizeText(repo.repoName, defaultRepo.repoName)
              const refName = normalizeText(repo.ref, defaultRepo.ref)
              return {
                key: repoKey(repoName, refName),
                repoName,
                ref: refName,
                author: normalizeText(repo.author, defaultRepo.author),
                updatedAt: normalizeText(repo.updatedAt, defaultRepo.updatedAt),
              }
            })
            .filter((repo, index, arr) => arr.findIndex((item) => item.key === repo.key) === index)

          const mappedFromRepos = (payload.repositories ?? []).flatMap((repo, repoIndex) => {
            const repoDefaults: Pick<WikiRepositoryModel, 'repoName' | 'ref' | 'author' | 'updatedAt'> = {
              repoName: normalizeText(repo.repoName, defaultRepo.repoName),
              ref: normalizeText(repo.ref, defaultRepo.ref),
              author: normalizeText(repo.author, defaultRepo.author),
              updatedAt: normalizeText(repo.updatedAt, defaultRepo.updatedAt),
            }

            return (repo.pages ?? [])
              .map((item, index) => mapPage(item, repoIndex * 1000 + index, repoDefaults))
              .filter((item): item is WikiPageModel => item !== null)
          })

          const mappedFallback = (payload.pages ?? [])
            .map((item, index) => mapPage(item, index, defaultRepo))
            .filter((item): item is WikiPageModel => item !== null)

          const mapped = mappedFromRepos.length > 0 ? mappedFromRepos : mappedFallback

          const derivedRepos = mapped.reduce<WikiRepositoryModel[]>((acc, page) => {
            const key = repoKey(page.repoName, page.ref)
            if (!acc.some((repo) => repo.key === key)) {
              acc.push({
                key,
                repoName: page.repoName,
                ref: page.ref,
                author: page.author,
                updatedAt: page.updatedAt,
              })
            }
            return acc
          }, [])

          repositories.value = mappedRepos.length > 0 ? mappedRepos : derivedRepos
          allPages.value = mapped

          if (!repositories.value.some((repo) => repo.key === activeRepositoryKey.value)) {
            activeRepositoryKey.value = repositories.value[0]?.key ?? ''
          }

          if (mapped.length === 0) {
            error.value = 'Wiki JSON 已读取，但 pages 为空。'
          }
          return
        } catch {
          attempts.push(`${url} (read-failed)`)
        }
      }

      allPages.value = []
      repositories.value = []
      error.value = buildLoadError(attempts)
    } finally {
      loading.value = false
    }
  }

  return {
    loading,
    error,
    wikiMeta,
    repositories,
    activeRepositoryKey,
    pages,
    hasData,
    loadPages,
    setActiveRepository,
  }
}
