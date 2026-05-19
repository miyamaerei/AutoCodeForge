import type { RouteRecordRaw } from 'vue-router'

export const mdWikiRoutes: RouteRecordRaw[] = [
  {
    path: '/mdwiki',
    name: 'md-wiki.list',
    component: () => import('./views/MdWikiListView.vue'),
    meta: {
      requiresAuth: false,
      title: 'MD Wiki',
    },
  },
]
