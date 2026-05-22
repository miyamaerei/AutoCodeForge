/**
 * Mock 函数单元测试 - chat.ts
 */
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { getTaskChat, sendTaskChatMessage } from '../chat'

describe('chat mock', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('getTaskChat', () => {
    it('should return messages for known task', async () => {
      const messages = await getTaskChat('T-1009')

      expect(Array.isArray(messages)).toBe(true)
      expect(messages.length).toBeGreaterThan(0)
      expect(messages[0]).toHaveProperty('id')
      expect(messages[0]).toHaveProperty('taskId')
      expect(messages[0]).toHaveProperty('role')
      expect(messages[0]).toHaveProperty('content')
      expect(messages[0]).toHaveProperty('timestamp')
    })

    it('should return empty array for unknown task', async () => {
      const messages = await getTaskChat('unknown-task')

      expect(messages).toEqual([])
    })
  })

  describe('sendTaskChatMessage', () => {
    it('should send message and return updated chat', async () => {
      const result = await sendTaskChatMessage({
        taskId: 'T-1009',
        content: '测试消息',
      })

      expect(Array.isArray(result)).toBe(true)
      expect(result.length).toBeGreaterThan(2) // Original + user + ai
      // Last message should be AI response
      const lastMessage = result[result.length - 1]
      expect(lastMessage).toBeDefined()
      expect(lastMessage!.role).toBe('ai')
      expect(lastMessage!.content).toContain('测试消息')
    })

    it('should add both user and ai messages', async () => {
      const initialMessages = await getTaskChat('T-1009')
      const initialLength = initialMessages.length

      const result = await sendTaskChatMessage({
        taskId: 'T-1009',
        content: '新消息',
      })

      expect(result.length).toBe(initialLength + 2)
      const userMessage = result[result.length - 2]
      const aiMessage = result[result.length - 1]
      expect(userMessage).toBeDefined()
      expect(aiMessage).toBeDefined()
      expect(userMessage!.role).toBe('user')
      expect(aiMessage!.role).toBe('ai')
    })
  })
})
