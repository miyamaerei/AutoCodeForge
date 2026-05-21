import axios, { AxiosError } from 'axios'

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL?.trim() || '/api'
const AUTH_TOKEN_KEY = 'auth_token'

export const request = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})

export function clearAuthTokenState(): void {
  localStorage.removeItem(AUTH_TOKEN_KEY)
  delete request.defaults.headers.common.Authorization
}

request.interceptors.request.use((config) => {
  const nextConfig = { ...config }
  nextConfig.headers = nextConfig.headers ?? {}
  nextConfig.headers['X-Requested-With'] = 'XMLHttpRequest'
  const token = localStorage.getItem(AUTH_TOKEN_KEY)
  if (token) {
    nextConfig.headers.Authorization = `Bearer ${token}`
  } else {
    delete (nextConfig.headers as Record<string, unknown>).Authorization
  }
  return nextConfig
})

request.interceptors.response.use(
  (response) => response,
  (error: AxiosError<{ message?: string }>) => {
    if (error.response?.status === 401) {
      clearAuthTokenState()
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
    }
    const message = error.response?.data?.message || error.message || 'Request failed'
    return Promise.reject(new Error(message))
  },
)
