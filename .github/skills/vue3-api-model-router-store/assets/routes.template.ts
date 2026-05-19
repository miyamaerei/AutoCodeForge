import type { RouteRecordRaw } from 'vue-router'

export const __module__Routes: RouteRecordRaw[] = [
  {
    path: '/__module__',
    name: '__module__.list',
    component: () => import('./views/__ModulePascal__ListView.vue'),
    meta: {
      requiresAuth: true,
      title: '__ModulePascal__ List',
    },
  },
  {
    path: '/__module__/:id',
    name: '__module__.detail',
    component: () => import('./views/__ModulePascal__DetailView.vue'),
    meta: {
      requiresAuth: true,
      title: '__ModulePascal__ Detail',
    },
  },
  {
    path: '/__module__/create',
    name: '__module__.create',
    component: () => import('./views/__ModulePascal__FormView.vue'),
    meta: {
      requiresAuth: true,
      title: 'Create __ModulePascal__',
    },
  },
  {
    path: '/__module__/:id/edit',
    name: '__module__.edit',
    component: () => import('./views/__ModulePascal__FormView.vue'),
    meta: {
      requiresAuth: true,
      title: 'Edit __ModulePascal__',
    },
  },
]
