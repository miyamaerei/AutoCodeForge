<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'

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

const getMenuItemIndex = (id: string) => id
</script>

<template>
  <aside class="app-sidebar">
    <!-- 用户卡片 -->
    <div class="sidebar-header">
      <template v-if="menuMode === 'settings'">
        <el-avatar :size="48" class="user-avatar clickable-avatar" @click="handleBackHome">H</el-avatar>
        <div class="user-info">
          <div class="user-name">返回 Home</div>
          <div class="user-role">当前为配置菜单</div>
        </div>
      </template>
      <template v-else>
        <el-tooltip content="点击头像进入 Settings" placement="right">
          <el-avatar :size="48" class="user-avatar clickable-avatar guided-avatar" @click="handleAvatarClick">
            {{ userInitials }}
          </el-avatar>
        </el-tooltip>
        <div class="user-info">
          <div class="user-name">{{ userName }}</div>
          <div class="user-role">{{ userRole }}</div>
          <div class="avatar-guide" role="button" tabindex="0" @click="handleAvatarClick">
            <span class="avatar-guide-text">Settings</span>
            <span class="avatar-guide-arrow">→</span>
          </div>
        </div>
      </template>
    </div>

    <!-- 菜单 -->
    <el-menu class="sidebar-menu" :default-openeds="openedMenus" :default-active="activeMenuIndex" unique-opened>
      <template v-for="item in menuItems" :key="item.id">
        <!-- 有子菜单的项 -->
        <el-sub-menu v-if="item.children && item.children.length > 0" :index="item.id">
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
        <el-menu-item v-else :index="item.id" @click="handleMenuSelect(item.path)">
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
  gap: 14px;
  padding: 24px 16px;
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
  width: 48px;
  height: 48px;
  border-radius: 12px;
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
}

.user-name {
  font-weight: 700;
  font-size: 14px;
  color: #0f172a;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  letter-spacing: 0.3px;
}

.user-role {
  font-size: 12px;
  color: #64748b;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin-top: 2px;
}

.avatar-guide {
  margin-top: 6px;
  border-radius: 10px;
  padding: 5px 10px;
  background: linear-gradient(135deg, #ecfeff 0%, #f0f9ff 100%);
  border: 1px solid #bae6fd;
  color: #0c4a6e;
  font-size: 11px;
  font-weight: 700;
  line-height: 1.35;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  transition: all 0.2s ease;
}

.avatar-guide:hover {
  border-color: #38bdf8;
  background: linear-gradient(135deg, #dbeafe 0%, #e0f2fe 100%);
  color: #0f172a;
  transform: translateX(1px);
}

.avatar-guide-text {
  letter-spacing: 0.1px;
}

.avatar-guide-arrow {
  font-size: 12px;
  transition: transform 0.2s ease;
}

.avatar-guide:hover .avatar-guide-arrow {
  transform: translateX(2px);
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
  padding: 12px 8px;
}

:deep(.el-menu) {
  border: none;
  background: transparent;
}

:deep(.el-menu-item),
:deep(.el-sub-menu__title) {
  background: transparent !important;
  border: none;
  height: 42px;
  line-height: 42px;
  margin: 4px 0;
  border-radius: 8px;
  color: #475569 !important;
  font-weight: 500;
  font-size: 14px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

:deep(.el-menu-item:hover),
:deep(.el-sub-menu__title:hover) {
  background-color: #f1f5f9 !important;
  color: #0f172a !important;
  padding-left: 20px;
}

:deep(.el-menu-item.is-active) {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
  color: white !important;
  border-right: 3px solid #667eea;
  font-weight: 600;
  padding-left: 16px;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
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
  padding-left: 28px !important;
  color: #64748b !important;
}

:deep(.el-sub-menu .el-menu-item:hover) {
  background-color: #f0f4ff !important;
  color: #667eea !important;
  padding-left: 32px;
}

:deep(.el-sub-menu .el-menu-item.is-active) {
  background: #f0f4ff !important;
  color: #667eea !important;
  border-right: 3px solid #667eea;
  font-weight: 600;
  padding-left: 28px;
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
