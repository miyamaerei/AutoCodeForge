<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { MoreFilled, Setting, SwitchButton, User } from '@element-plus/icons-vue'
import { useAuthStore } from '../modules/auth/store/useAuthStore'

export interface MenuItem {
  id: string
  title: string
  path: string
  icon?: string
  children?: MenuItem[]
}

interface Props {
  menuItems: MenuItem[]
  userInitials: string
  userName: string
  userRole: string
  menuMode?: 'user' | 'settings'
}

const props = withDefaults(defineProps<Props>(), {
  userInitials: 'MI',
  userName: 'User',
  userRole: 'Member',
  menuMode: 'user',
})

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

// 当前展开的菜单项
const openedMenus = computed(() => {
  const opened: string[] = []
  const checkPath = (items: MenuItem[]) => {
    items.forEach((item) => {
      if (item.children && item.children.length > 0) {
        const hasActive = item.children.some((child) => route.path.startsWith(child.path))
        if (hasActive) {
          opened.push(item.id)
        }
        checkPath(item.children)
      }
    })
  }
  checkPath(props.menuItems)
  return opened
})

const activeMenuIndex = computed(() => {
  let active = ''
  const checkPath = (items: MenuItem[]) => {
    items.forEach((item) => {
      if (route.path === item.path || route.path.startsWith(item.path + '/')) {
        active = item.id
      }
      if (item.children && item.children.length > 0) {
        checkPath(item.children)
      }
    })
  }
  checkPath(props.menuItems)
  return active
})

const handleMenuSelect = (path: string) => {
  router.push(path)
}

const handleAvatarClick = () => {
  if (props.menuMode === 'user') {
    router.push('/settings/preferences')
  }
}

const handleBackHome = () => {
  router.push('/')
}

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}

const handleProfile = () => {
  router.push('/settings/preferences')
}

const handleSettings = () => {
  router.push('/settings/management')
}

const handleActionMenuCommand = (command: string) => {
  if (command === 'profile') {
    handleProfile()
    return
  }
  if (command === 'settings') {
    handleSettings()
    return
  }
  if (command === 'logout') {
    handleLogout()
  }
}

const modeBadgeText = computed(() =>
  props.menuMode === 'settings' ? 'Settings Mode' : 'Workspace Mode',
)

const getMenuItemIndex = (id: string) => id
</script>

<template>
  <aside class="app-sidebar">
    <!-- 用户卡片 -->
    <div class="sidebar-header">
      <template v-if="menuMode === 'settings'">
        <el-avatar
          :size="48"
          class="user-avatar clickable-avatar"
          data-guide="avatar-switch-primary"
          @click="handleBackHome"
        >
          {{ userInitials }}
        </el-avatar>
        <div class="user-info">
          <div class="user-main-row">
            <div class="user-name">{{ userName }}</div>
            <el-dropdown trigger="click" popper-class="profile-actions-popper" @command="handleActionMenuCommand">
              <button class="profile-more-btn" type="button" aria-label="Profile actions">
                <el-icon><MoreFilled /></el-icon>
              </button>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item command="profile">
                    <span class="action-item-content">
                      <el-icon><User /></el-icon>
                      <span>Profile</span>
                    </span>
                  </el-dropdown-item>
                  <el-dropdown-item command="settings">
                    <span class="action-item-content">
                      <el-icon><Setting /></el-icon>
                      <span>Settings</span>
                    </span>
                  </el-dropdown-item>
                  <el-dropdown-item command="logout" divided class="danger-item">
                    <span class="action-item-content danger-text">
                      <el-icon><SwitchButton /></el-icon>
                      <span>Logout</span>
                    </span>
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>
          <div class="user-role">{{ userRole }}</div>
          <div
            class="user-meta"
            role="button"
            tabindex="0"
            data-guide="avatar-switch-ribbon"
            @click="handleBackHome"
          >
            <span class="mode-pill">{{ modeBadgeText }}</span>
            <span class="meta-arrow">→</span>
          </div>
        </div>
      </template>
      <template v-else>
        <el-tooltip content="点击头像进入 Settings" placement="right">
          <el-avatar
            :size="48"
            class="user-avatar clickable-avatar guided-avatar"
            data-guide="avatar-switch-primary"
            @click="handleAvatarClick"
          >
            {{ userInitials }}
          </el-avatar>
        </el-tooltip>
        <div class="user-info">
          <div class="user-main-row">
            <div class="user-name">{{ userName }}</div>
            <el-dropdown trigger="click" popper-class="profile-actions-popper" @command="handleActionMenuCommand">
              <button class="profile-more-btn" type="button" aria-label="Profile actions">
                <el-icon><MoreFilled /></el-icon>
              </button>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item command="profile">
                    <span class="action-item-content">
                      <el-icon><User /></el-icon>
                      <span>Profile</span>
                    </span>
                  </el-dropdown-item>
                  <el-dropdown-item command="settings">
                    <span class="action-item-content">
                      <el-icon><Setting /></el-icon>
                      <span>Settings</span>
                    </span>
                  </el-dropdown-item>
                  <el-dropdown-item command="logout" divided class="danger-item">
                    <span class="action-item-content danger-text">
                      <el-icon><SwitchButton /></el-icon>
                      <span>Logout</span>
                    </span>
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>
          <div class="user-role">{{ userRole }}</div>
          <div
            class="user-meta"
            role="button"
            tabindex="0"
            data-guide="avatar-switch-ribbon"
            @click="handleAvatarClick"
          >
            <span class="mode-pill">{{ modeBadgeText }}</span>
            <span class="meta-arrow">→</span>
          </div>
        </div>
      </template>
    </div>

    <!-- 菜单 -->
    <div class="menu-section-hint">Navigation</div>
    <el-menu class="sidebar-menu" :default-openeds="openedMenus" :default-active="activeMenuIndex" unique-opened>
      <template v-for="item in menuItems" :key="item.id">
        <!-- 有子菜单的项 -->
        <el-sub-menu
          v-if="item.children && item.children.length > 0"
          :index="item.id"
          :data-guide="item.id === 'task-center' ? 'task-center' : item.id === 'repo-management' ? 'repo-management' : item.id === 'pipeline-center' ? 'pipeline-center' : item.id === 'quick-actions' ? 'chat-console' : ''"
        >
          <template #title>
            <span>{{ item.title }}</span>
          </template>
          <el-menu-item
            v-for="child in item.children"
            :key="child.id"
            :index="child.id"
            @click="handleMenuSelect(child.path)"
          >
            {{ child.title }}
          </el-menu-item>
        </el-sub-menu>

        <!-- 无子菜单的项 -->
        <el-menu-item
          v-else
          :index="item.id"
          :data-guide="item.id === 'dashboard' ? 'dashboard' : item.id === 'settings-root' ? 'system-config' : item.id === 'agent-center' ? 'agent-center' : ''"
          @click="handleMenuSelect(item.path)"
        >
          {{ item.title }}
        </el-menu-item>
      </template>
    </el-menu>
  </aside>
</template>

<style scoped>
.app-sidebar {
  width: 280px;
  height: 100vh;
  background: linear-gradient(180deg, #ffffff 0%, #f8fafc 100%);
  border-right: 1px solid #e2e8f0;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  box-shadow: 2px 0 8px rgba(15, 23, 42, 0.06);
}

.sidebar-header {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px 14px 12px;
  border-bottom: 1px solid #e2e8f0;
  background: linear-gradient(135deg, #f0f4ff 0%, #ffffff 100%);
}

.clickable-avatar {
  cursor: pointer;
}

.guided-avatar {
  position: relative;
  animation: avatarPulse 1.8s infinite;
}

.user-avatar {
  flex-shrink: 0;
  width: 44px;
  height: 44px;
  border-radius: 10px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: 600;
  font-size: 16px;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.user-info {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.user-name {
  font-weight: 700;
  font-size: 15px;
  color: #0f172a;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  letter-spacing: 0.3px;
}

.user-main-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.profile-more-btn {
  width: 24px;
  height: 24px;
  border-radius: 999px;
  border: 1px solid #dbeafe;
  background: #f8fafc;
  color: #475569;
  line-height: 1;
  font-size: 16px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.action-item-content {
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.danger-text {
  color: #b91c1c;
  font-weight: 600;
}

.profile-more-btn:hover {
  border-color: #93c5fd;
  background: #eef2ff;
  color: #0f172a;
}

:deep(.profile-actions-popper .danger-item:not(.is-disabled)) {
  color: #b91c1c;
}

:deep(.profile-actions-popper .danger-item:not(.is-disabled):hover) {
  background-color: #fef2f2;
  color: #991b1b;
}

.user-role {
  font-size: 12px;
  color: #64748b;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin-top: 1px;
}

.user-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 4px;
  cursor: pointer;
  width: fit-content;
}

.mode-pill {
  border-radius: 999px;
  padding: 3px 10px;
  background: #eef2ff;
  border: 1px solid #c7d2fe;
  color: #3730a3;
  font-size: 11px;
  font-weight: 700;
  line-height: 1.3;
}

.meta-arrow {
  color: #4f46e5;
  font-size: 12px;
  transition: transform 0.2s ease;
}

.user-meta:hover .meta-arrow {
  transform: translateX(2px);
}

.menu-section-hint {
  padding: 10px 14px 2px;
  color: #64748b;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

@keyframes avatarPulse {
  0% {
    box-shadow: 0 0 0 0 rgba(102, 126, 234, 0.55);
  }
  70% {
    box-shadow: 0 0 0 10px rgba(102, 126, 234, 0);
  }
  100% {
    box-shadow: 0 0 0 0 rgba(102, 126, 234, 0);
  }
}

.sidebar-menu {
  flex: 1;
  border: none;
  background: transparent;
  padding: 8px 10px 12px;
}

:deep(.el-menu) {
  border: none;
  background: transparent;
}

:deep(.el-menu-item),
:deep(.el-sub-menu__title) {
  background: transparent !important;
  border: none;
  height: 48px;
  line-height: 48px;
  margin: 6px 0;
  border-radius: 12px;
  color: #475569 !important;
  font-weight: 600;
  font-size: 15px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

:deep(.el-menu-item:hover),
:deep(.el-sub-menu__title:hover) {
  background-color: #eef2ff !important;
  color: #0f172a !important;
  padding-left: 20px;
  box-shadow: 0 2px 8px rgba(99, 102, 241, 0.12);
}

:deep(.el-menu-item.is-active) {
  background: linear-gradient(135deg, #4f46e5 0%, #4338ca 100%) !important;
  color: white !important;
  border-right: 3px solid #4f46e5;
  font-weight: 600;
  padding-left: 16px;
  box-shadow: 0 6px 14px rgba(79, 70, 229, 0.35);
}

:deep(.el-sub-menu.is-opened > .el-sub-menu__title) {
  background-color: #f1f5f9 !important;
  color: #0f172a !important;
  font-weight: 600;
}

:deep(.el-sub-menu__title:hover) {
  padding-left: 20px;
}

:deep(.el-sub-menu .el-menu-item) {
  padding-left: 34px !important;
  color: #64748b !important;
  font-size: 14px;
}

:deep(.el-sub-menu .el-menu-item:hover) {
  background-color: #e0e7ff !important;
  color: #4338ca !important;
  padding-left: 38px;
}

:deep(.el-sub-menu .el-menu-item.is-active) {
  background: #e0e7ff !important;
  color: #4338ca !important;
  border-right: 3px solid #4338ca;
  font-weight: 600;
  padding-left: 34px;
}

:deep(.el-menu--collapse) {
  width: 64px;
}

/* 美化菜单图标展开/收起箭头 */
:deep(.el-sub-menu__icon-arrow) {
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

:deep(.el-sub-menu.is-opened .el-sub-menu__icon-arrow) {
  transform: rotateZ(180deg);
}
</style>
