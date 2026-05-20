export interface ApiEnvelope<T> {
  success: boolean
  message: string
  data: T
  traceId?: string
}

export interface ApiError {
  message: string
  errors?: Record<string, string[]>
}

export interface LoginRequestDto {
  ntId?: string
  userName?: string
  email?: string
}

export interface RegisterRequestDto {
  ntId: string
  userName: string
  email?: string
}

export interface AuthResponseDto {
  accessToken: string
  expiresAtUtc: string
  ntId: string
  userName: string
}

export interface AuthUserDto {
  id: string
  ntId: string
  userName: string
  email?: string | null
}
