/**
 * useAuthStore 单元测试
 */
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '../useAuthStore'
import * as authApi from '../../auth.api'
import type { AuthUserDto, LoginRequestDto, RegisterRequestDto } from '../auth.types'

describe('useAuthStore', () => {
  // 测试数据
  const mockUser: AuthUserDto = {
    id: '00000000-0000-0000-0000-000000000000',
    ntId: 'mock.user',
    userName: 'Mock User',
    email: 'mock@example.com',
  }

  // Spy objects
  let loginSpy: ReturnType<typeof vi.spyOn>
  let registerSpy: ReturnType<typeof vi.spyOn>
  let getMeSpy: ReturnType<typeof vi.spyOn>

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()

    // Setup spies
    loginSpy = vi.spyOn(authApi, 'login')
    registerSpy = vi.spyOn(authApi, 'register')
    getMeSpy = vi.spyOn(authApi, 'getMe')

    // Clear localStorage mock
    vi.stubGlobal('localStorage', {
      getItem: vi.fn().mockReturnValue(null),
      setItem: vi.fn(),
      removeItem: vi.fn(),
      clear: vi.fn(),
    })
  })

  describe('初始状态', () => {
    it('should have correct initial state', () => {
      const store = useAuthStore()

      expect(store.token).toBe(null)
      expect(store.user).toBe(null)
      expect(store.loading).toBe(false)
      expect(store.error).toBe(null)
    })

    it('should not be authenticated initially', () => {
      const store = useAuthStore()
      expect(store.isAuthenticated).toBe(false)
    })
  })

  describe('loginWithWindows', () => {
    it('should login successfully with Windows auth', async () => {
      const store = useAuthStore()
      const loginResponse = {
        accessToken: 'test-token',
        expiresAtUtc: new Date(Date.now() + 3600000).toISOString(),
        ntId: 'test.user',
        userName: 'Test User',
      }
      loginSpy.mockResolvedValue(loginResponse)
      getMeSpy.mockResolvedValue(mockUser)

      await store.loginWithWindows({ ntId: 'test.user' })

      expect(store.token).toBe('test-token')
      expect(store.user).toEqual(mockUser)
      expect(store.loading).toBe(false)
      expect(store.isAuthenticated).toBe(true)
    })

    it('should set error on login failure', async () => {
      const store = useAuthStore()
      loginSpy.mockRejectedValue(new Error('登录失败'))

      try {
        await store.loginWithWindows({})
      } catch (err) {
        // Expected
      }

      expect(store.error).toBe('登录失败')
      expect(store.token).toBe(null)
      expect(store.user).toBe(null)
    })
  })

  describe('registerUser', () => {
    it('should register successfully', async () => {
      const store = useAuthStore()
      const registerResponse = {
        accessToken: 'new-token',
        expiresAtUtc: new Date(Date.now() + 3600000).toISOString(),
        ntId: 'new.user',
        userName: 'New User',
      }
      registerSpy.mockResolvedValue(registerResponse)
      getMeSpy.mockResolvedValue(mockUser)

      await store.registerUser({ ntId: 'new.user', userName: 'New User', email: 'new@example.com' })

      expect(store.token).toBe('new-token')
      expect(store.loading).toBe(false)
    })

    it('should set error on register failure', async () => {
      const store = useAuthStore()
      registerSpy.mockRejectedValue(new Error('注册失败'))

      try {
        await store.registerUser({ ntId: 'new.user', userName: 'New User' })
      } catch (err) {
        // Expected
      }

      expect(store.error).toBe('注册失败')
    })
  })

  describe('fetchMe', () => {
    it('should fetch current user successfully', async () => {
      const store = useAuthStore()
      store.$patch({ token: 'valid-token' })
      getMeSpy.mockResolvedValue(mockUser)

      await store.fetchMe()

      expect(store.user).toEqual(mockUser)
      expect(store.loading).toBe(false)
    })

    it('should return null when no token', async () => {
      const store = useAuthStore()
      store.$patch({ token: null })

      await store.fetchMe()

      expect(store.user).toBe(null)
    })
  })

  describe('logout', () => {
    it('should clear user and token on logout', () => {
      const store = useAuthStore()
      store.$patch({ token: 'test-token', user: mockUser })

      store.logout()

      expect(store.token).toBe(null)
      expect(store.user).toBe(null)
      expect(store.error).toBe(null)
      expect(store.isAuthenticated).toBe(false)
    })
  })
})
