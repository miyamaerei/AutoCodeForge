<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import AppSidebar from './components/AppSidebar.vue'
import type { MenuItem } from './components/AppSidebar.vue'

const userProfile = {
  initials: 'MI',
  name: 'miyamaerei',
  role: 'Repo Owner',
}

const route = useRoute()

const userMenuItems = computed<MenuItem[]>(() => [
  {
    id: 'console',
    title: '入口',
    path: '/',
  },
  {
    id: 'quick-actions',
    title: '快速操作',
    path: '/session',
    children: [
      {
        id: 'session',
        title: 'Session',
        path: '/session',
      },
      {
        id: 'ask',
        title: 'Ask',
        path: '/ask',
      },
      {
        id: 'wiki',
        title: 'Wiki',
        path: '/wiki',
      },
      {
        id: 'review',
        title: 'Review',
        path: '/review',
      },
      {
        id: 'automations',
        title: 'Automations',
        path: '/automations',
      },
    ],
  },
  {
    id: 'task-center',
    title: 'AI任务中心',
    path: '/task-center',
    children: [
      {
        id: 'task-create',
        title: '任务创建',
        path: '/task-center/create',
      },
      {
        id: 'task-list',
        title: '任务列表',
        path: '/task-center',
      },
    ],
  },
  {
    id: 'repo-management',
    title: '仓库管理',
    path: '/repo-management',
    children: [
      {
        id: 'repo-list',
        title: '仓库列表',
        path: '/repo-management',
      },
      {
        id: 'repo-branches',
        title: '分支管理',
        path: '/repo-management/branches',
      },
      {
        id: 'repo-prs',
        title: 'PR管理',
        path: '/repo-management/prs',
      },
    ],
  },
  {
    id: 'dashboard',
    title: 'Dashboard',
    path: '/dashboard',
  },
  {
    id: 'pipeline-center',
    title: '流水线中心',
    path: '/pipeline-center',
    children: [
      {
        id: 'pipeline-list',
        title: '流水线列表',
        path: '/pipeline-center',
      },
      {
        id: 'pipeline-builds',
        title: '构建状态',
        path: '/pipeline-center/builds',
      },
    ],
  },
])

const settingsMenuItems = computed<MenuItem[]>(() => [
  {
    id: 'settings-root',
    title: 'Settings',
    path: '/settings/preferences',
    children: [
      {
        id: 'settings-preferences',
        title: 'Preferences',
        path: '/settings/preferences',
      },
      {
        id: 'settings-repositories',
        title: 'Repositories',
        path: '/settings/repositories',
      },
      {
        id: 'settings-knowledge',
        title: 'Knowledge',
        path: '/settings/knowledge',
      },
      {
        id: 'settings-skill',
        title: 'Skill',
        path: '/settings/skill',
      },
      {
        id: 'settings-schedules',
        title: 'Schedules',
        path: '/settings/schedules',
      },
      {
        id: 'settings-deepwiki',
        title: 'DeepWiki',
        path: '/settings/deepwiki',
      },
      {
        id: 'settings-review',
        title: 'Review',
        path: '/settings/review',
      },
    ],
  },
])

const isSettingsMenu = computed(() => route.path.startsWith('/settings'))

const menuItems = computed<MenuItem[]>(() =>
  isSettingsMenu.value ? settingsMenuItems.value : userMenuItems.value,
)
</script>

<template>
  <el-container class="app-shell">
    <AppSidebar
      :menu-items="menuItems"
      :user-initials="userProfile.initials"
      :user-name="userProfile.name"
      :user-role="userProfile.role"
      :menu-mode="isSettingsMenu ? 'settings' : 'user'"
    />
    <el-container class="app-container">
      <el-main class="app-main">
        <RouterView />
      </el-main>
    </el-container>
  </el-container>
</template>

<style scoped>
.app-shell {
  height: 100vh;
  min-width: 1280px;
  display: flex;
  flex-direction: row;
}

.app-container {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.app-main {
  flex: 1;
  padding: 0.5rem 0.5rem 0.75rem;
  overflow: auto;
}
</style>
