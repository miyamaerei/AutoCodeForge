import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import {
  registerAgent,
  renewHeartbeat,
  deregisterAgent,
  getAvailableAgents,
  getAgentRegistration,
} from '../api/agent-registration.api'
import type { AgentRegistrationDto, RegisterAgentRequest } from '../api/agent-registration.types'

export const useAgentRegistrationStore = defineStore('agent.registration', () => {
  const registrations = ref<AgentRegistrationDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const onlineAgents = computed(() => 
    registrations.value.filter((r: AgentRegistrationDto) => r.status === 'Online')
  )

  const offlineAgents = computed(() => 
    registrations.value.filter((r: AgentRegistrationDto) => r.status === 'Offline')
  )

  async function loadRegistrations(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      registrations.value = await getAvailableAgents()
    } catch (e: any) {
      error.value = e.message || 'Failed to load agent registrations'
    } finally {
      loading.value = false
    }
  }

  async function register(payload: RegisterAgentRequest): Promise<AgentRegistrationDto> {
    const registered = await registerAgent(payload)
    const index = registrations.value.findIndex((r: AgentRegistrationDto) => r.agentId === payload.agentId)
    if (index >= 0) {
      registrations.value[index] = registered
    } else {
      registrations.value.push(registered)
    }
    return registered
  }

  async function heartbeat(agentId: string): Promise<void> {
    await renewHeartbeat({ agentId })
    const registration = registrations.value.find((r: AgentRegistrationDto) => r.agentId === agentId)
    if (registration) {
      registration.lastHeartbeat = new Date().toISOString()
      registration.status = 'Online'
    }
  }

  async function deregister(agentId: string): Promise<void> {
    await deregisterAgent(agentId)
    registrations.value = registrations.value.filter((r: AgentRegistrationDto) => r.agentId !== agentId)
  }

  async function getRegistration(agentId: string): Promise<AgentRegistrationDto> {
    const existing = registrations.value.find((r: AgentRegistrationDto) => r.agentId === agentId)
    if (existing) {
      return existing
    }
    return await getAgentRegistration(agentId)
  }

  return {
    registrations,
    loading,
    error,
    onlineAgents,
    offlineAgents,
    loadRegistrations,
    register,
    heartbeat,
    deregister,
    getRegistration,
  }
})