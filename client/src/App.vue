<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import AppSidebar from './components/AppSidebar.vue'
import OnboardingGuide from './components/OnboardingGuide.vue'
import type { MenuItem } from './components/AppSidebar.vue'
import { useOnboarding } from './composables/useOnboarding'
import { useAuthStore } from './modules/auth/store/useAuthStore'

const authStore = useAuthStore()

// 从 authStore 获取用户信息
const userProfile = computed(() => {
  if (authStore.user) {
    return {
      initials: authStore.user.userName
        .split(' ')
        .map((n) => n[0])
        .join('')
        .toUpperCase()
        .slice(0, 2),
      name: authStore.user.userName,
      role: 'Repo Owner',
    }
  }
  return {
    initials: 'GU',
    name: 'Guest User',
    role: 'Member',
  }
})

const route = useRoute()
const isLoginRoute = computed(() => route.path === '/login')
const isFullscreenRoute = computed(() => route.meta.fullscreen === true)

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
        path: '/mdwiki',
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
    id: 'dashboard',
    title: 'Dashboard',
    path: '/dashboard',
    highlighted: true,
  },
  {
    id: 'task-center',
    title: 'AI任务中心',
    path: '/task-center',
    highlighted: true,
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
    id: 'workflow-center',
    title: '流程中心',
    path: '/workflow-center/requirements',
    highlighted: true,
    children: [
      {
        id: 'workflow-requirements',
        title: '需求',
        path: '/workflow-center/requirements',
      },
      {
        id: 'workflow-rounds',
        title: '轮次',
        path: '/workflow-center/rounds',
      },
      {
        id: 'workflow-tasks',
        title: '任务',
        path: '/workflow-center/tasks',
      },
      {
        id: 'workflow-flow',
        title: '流程',
        path: '/workflow-center/flow',
      },
      {
        id: 'workflow-ops',
        title: '通知审批台',
        path: '/workflow-center/ops',
      },
    ],
  },
  {
    id: 'repo-management',
    title: '仓库管理',
    path: '/repo-management',
    highlighted: true,
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
      {
        id: 'repo-sync-progress',
        title: '同步进度',
        path: '/repo-management/sync-progress',
      },
    ],
  },
 
  {
    id: 'agent-center',
    title: 'Agent中心',
    path: '/agent-center/prompt-guide',
    highlighted: true,
    children: [
      {
        id: 'agent-prompt-guide',
        title: '通用提示',
        path: '/agent-center/prompt-guide',
      },
      {
        id: 'agent-manage',
        title: 'Agent管理',
        path: '/agent-center',
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
      {
        id: 'settings-integrations',
        title: 'Integrations',
        path: '/settings/integrations',
      },
      {
        id: 'settings-notifications',
        title: 'Notifications',
        path: '/settings/notifications',
      },
      {
        id: 'settings-sandbox',
        title: 'Sandbox',
        path: '/settings/sandbox',
      },
      {
        id: 'settings-workflow',
        title: 'Workflow',
        path: '/settings/workflow',
      },
      {
        id: 'settings-management',
        title: 'Management',
        path: '/settings/management',
      },
    ],
  },
])

const isSettingsMenu = computed(
  () => route.path.startsWith('/settings'),
)

const menuItems = computed<MenuItem[]>(() =>
  isSettingsMenu.value ? settingsMenuItems.value : userMenuItems.value,
)

const onboarding = useOnboarding()
const showOnboarding = ref(false)

onMounted(async () => {
  // 如果有 token，尝试获取用户信息
  if (authStore.token) {
    try {
      await authStore.fetchMe()
    } catch (error) {
      console.error('Failed to fetch user info:', error)
    }
  }

  if (onboarding.shouldShowOnboarding()) {
    setTimeout(() => {
      showOnboarding.value = true
      onboarding.start()
    }, 1000)
  }
})

function handleOnboardingComplete(): void {
  showOnboarding.value = false
}

function handleOnboardingSkip(): void {
  showOnboarding.value = false
}
</script>

<template>
  <RouterView v-if="isLoginRoute" />
  <RouterView v-else-if="isFullscreenRoute" />
  <el-container v-else class="app-shell">
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
    <OnboardingGuide
      v-if="showOnboarding"
      @complete="handleOnboardingComplete"
      @skip="handleOnboardingSkip"
    />
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
