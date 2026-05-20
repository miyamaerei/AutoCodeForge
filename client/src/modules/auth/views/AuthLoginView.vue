<script setup lang="ts">
import { reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../store/useAuthStore'

const router = useRouter()
const authStore = useAuthStore()

const loginForm = reactive({
  ntId: '',
  userName: '',
  email: '',
})

async function handleWindowsLogin(): Promise<void> {
  await authStore.loginWithWindows()
  await router.replace('/')
}

async function handleFallbackLogin(): Promise<void> {
  await authStore.loginWithWindows({
    ntId: loginForm.ntId.trim() || undefined,
    userName: loginForm.userName.trim() || undefined,
    email: loginForm.email.trim() || undefined,
  })
  await router.replace('/')
}
</script>

<template>
  <section class="auth-login-view">
    <el-card class="auth-login-card" shadow="hover">
      <template #header>
        <div class="auth-login-header">
          <h1>AutoCodeForge 登录</h1>
          <p>优先使用 Windows 身份自动登录，必要时可手动输入 NTID 回退登录。</p>
        </div>
      </template>

      <div class="auth-login-actions">
        <el-button type="primary" :loading="authStore.loading" @click="handleWindowsLogin">
          Windows 自动登录
        </el-button>
      </div>

      <el-divider>回退登录</el-divider>

      <el-form label-position="top" class="auth-login-form">
        <el-form-item label="NTID">
          <el-input v-model="loginForm.ntId" placeholder="例如：miyamaerei" clearable />
        </el-form-item>
        <el-form-item label="显示名（可选）">
          <el-input v-model="loginForm.userName" placeholder="例如：Miyamaerei" clearable />
        </el-form-item>
        <el-form-item label="邮箱（可选）">
          <el-input v-model="loginForm.email" placeholder="you@example.com" clearable />
        </el-form-item>
      </el-form>

      <div class="auth-login-actions">
        <el-button :loading="authStore.loading" @click="handleFallbackLogin">
          使用 NTID 登录
        </el-button>
      </div>

      <el-alert
        v-if="authStore.error"
        :title="authStore.error"
        type="error"
        :closable="false"
        show-icon
      />
    </el-card>
  </section>
</template>

<style scoped>
.auth-login-view {
  min-height: calc(100vh - 1rem);
  display: grid;
  place-items: center;
  padding: 1rem;
}

.auth-login-card {
  width: min(560px, 100%);
}

.auth-login-header {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.auth-login-header h1 {
  margin: 0;
  font-size: 1.5rem;
}

.auth-login-header p {
  margin: 0;
  color: var(--el-text-color-secondary);
}

.auth-login-form {
  margin-bottom: 1rem;
}

.auth-login-actions {
  display: flex;
  justify-content: flex-end;
}
</style>
