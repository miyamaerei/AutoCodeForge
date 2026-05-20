import { request } from '../../lib/request'
import { USE_MOCK } from '../../config/runtime'
import type {
  ApiEnvelope,
  AuthResponseDto,
  AuthUserDto,
  LoginRequestDto,
  RegisterRequestDto,
} from './auth.types'

function createMockAuthResponse(payload?: LoginRequestDto): AuthResponseDto {
  const identity = payload?.ntId?.trim() || 'mock.user'
  return {
    accessToken: `mock-token-${identity}`,
    expiresAtUtc: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
    ntId: identity,
    userName: payload?.userName?.trim() || identity,
  }
}

function createMockUser(payload?: LoginRequestDto): AuthUserDto {
  const identity = payload?.ntId?.trim() || 'mock.user'
  return {
    id: '00000000-0000-0000-0000-000000000000',
    ntId: identity,
    userName: payload?.userName?.trim() || identity,
    email: payload?.email ?? null,
  }
}

export async function login(payload: LoginRequestDto = {}): Promise<AuthResponseDto> {
  if (USE_MOCK) {
    return createMockAuthResponse(payload)
  }

  try {
    const { data } = await request.post<ApiEnvelope<AuthResponseDto>>('/v1/auth/windows-login', {})
    return data.data
  } catch (error) {
    if (!payload.ntId) {
      throw error
    }

    const { data } = await request.post<ApiEnvelope<AuthResponseDto>>('/v1/auth/login', payload)
    return data.data
  }
}

export async function register(payload: RegisterRequestDto): Promise<AuthResponseDto> {
  if (USE_MOCK) {
    return createMockAuthResponse(payload)
  }

  const { data } = await request.post<ApiEnvelope<AuthResponseDto>>('/v1/auth/register', payload)
  return data.data
}

export async function getMe(): Promise<AuthUserDto> {
  if (USE_MOCK) {
    return createMockUser()
  }

  const { data } = await request.get<ApiEnvelope<AuthUserDto>>('/v1/auth/me')
  return data.data
}
