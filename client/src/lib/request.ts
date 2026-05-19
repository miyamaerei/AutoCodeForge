import axios, { AxiosError } from 'axios'

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL?.trim() || '/api'

export const request = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})

request.interceptors.request.use((config) => {
  const nextConfig = { ...config }
  nextConfig.headers = nextConfig.headers ?? {}
  nextConfig.headers['X-Requested-With'] = 'XMLHttpRequest'
  return nextConfig
})

request.interceptors.response.use(
  (response) => response,
  (error: AxiosError<{ message?: string }>) => {
    const message = error.response?.data?.message || error.message || 'Request failed'
    return Promise.reject(new Error(message))
  },
)
