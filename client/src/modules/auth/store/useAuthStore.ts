import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { getMe, login, register } from '../auth.api'
import type { AuthUserDto, LoginRequestDto, RegisterRequestDto } from '../auth.types'
import { clearAuthTokenState } from '../../../lib/request'

const AUTH_TOKEN_KEY = 'auth_token'

export const useAuthStore = defineStore('module.auth', () => {
  const token = ref<string | null>(localStorage.getItem(AUTH_TOKEN_KEY))
  const user = ref<AuthUserDto | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const isAuthenticated = computed(() => Boolean(token.value))

  function persistToken(value: string | null): void {
    token.value = value
    if (value) {
      localStorage.setItem(AUTH_TOKEN_KEY, value)
      return
    }
    clearAuthTokenState()
  }

  async function loginWithWindows(payload: LoginRequestDto = {}): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const response = await login(payload)
      persistToken(response.accessToken)
      user.value = {
        id: user.value?.id ?? '',
        ntId: response.ntId,
        userName: response.userName,
        email: user.value?.email ?? null,
      }
      await fetchMe()
    } catch (err) {
      persistToken(null)
      user.value = null
      error.value = err instanceof Error ? err.message : '登录失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function registerUser(payload: RegisterRequestDto): Promise<void> {
    loading.value = true
    error.value = null
    try {
      const response = await register(payload)
      persistToken(response.accessToken)
      user.value = {
        id: user.value?.id ?? '',
        ntId: response.ntId,
        userName: response.userName,
        email: payload.email ?? null,
      }
      await fetchMe()
    } catch (err) {
      error.value = err instanceof Error ? err.message : '注册失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  async function fetchMe(): Promise<void> {
    if (!token.value) {
      user.value = null
      return
    }

    loading.value = true
    error.value = null
    try {
      user.value = await getMe()
    } catch (err) {
      persistToken(null)
      user.value = null
      error.value = err instanceof Error ? err.message : '获取当前用户失败'
      throw err
    } finally {
      loading.value = false
    }
  }

  function logout(): void {
    persistToken(null)
    user.value = null
    error.value = null
  }

  return {
    token,
    user,
    loading,
    error,
    isAuthenticated,
    loginWithWindows,
    registerUser,
    fetchMe,
    logout,
  }
})
