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
  delete request.defaults.headers.common['X-NtId']
}

/**
 * Decode JWT token without verification (client-side only)
 * Returns the payload object or null if token is invalid
 */
function decodeJwt(token: string): Record<string, unknown> | null {
  try {
    const parts = token.split('.')
    if (parts.length !== 3) {
      return null
    }
    const payload = parts[1]
    const decoded = JSON.parse(atob(payload))
    return decoded as Record<string, unknown>
  } catch {
    return null
  }
}

request.interceptors.request.use((config) => {
  const nextConfig = { ...config }
  nextConfig.headers = nextConfig.headers ?? {}
  nextConfig.headers['X-Requested-With'] = 'XMLHttpRequest'
  
  const token = localStorage.getItem(AUTH_TOKEN_KEY)
  if (token) {
    nextConfig.headers.Authorization = `Bearer ${token}`
    
    // Extract NtId from JWT token and add to X-NtId header for backend identification
    const payload = decodeJwt(token)
    if (payload) {
      const ntId = payload.NtId || payload.sub || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
      if (ntId) {
        nextConfig.headers['X-NtId'] = String(ntId)
        console.debug('[Request] Added X-NtId header:', ntId)
      }
    }
  } else {
    delete (nextConfig.headers as Record<string, unknown>).Authorization
    delete (nextConfig.headers as Record<string, unknown>)['X-NtId']
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
