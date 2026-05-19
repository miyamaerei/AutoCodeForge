export interface ChatMessageDto {
  id: string
  taskId: string
  role: 'user' | 'ai'
  content: string
  timestamp: string
}

export interface SendTaskChatRequestDto {
  taskId: string
  content: string
}

const chatByTaskId = new Map<string, ChatMessageDto[]>([
  ['T-1009', [
    {
      id: 'chat-1',
      taskId: 'T-1009',
      role: 'user',
      content: '请帮我修复订单导出空指针。',
      timestamp: '14:30',
    },
    {
      id: 'chat-2',
      taskId: 'T-1009',
      role: 'ai',
      content: '已分析问题，准备修改空值判断与导出逻辑。',
      timestamp: '14:31',
    },
  ]],
  ['T-1008', [
    {
      id: 'chat-3',
      taskId: 'T-1008',
      role: 'user',
      content: '会员模块接口需要重构。',
      timestamp: '10:00',
    },
    {
      id: 'chat-4',
      taskId: 'T-1008',
      role: 'ai',
      content: '我将按读写分离和错误码统一规范进行重构。',
      timestamp: '10:01',
    },
  ]],
])

export async function getTaskChat(taskId: string): Promise<ChatMessageDto[]> {
  await sleep(160)
  return chatByTaskId.get(taskId) ?? []
}

export async function sendTaskChatMessage(payload: SendTaskChatRequestDto): Promise<ChatMessageDto[]> {
  await sleep(220)

  const taskMessages = chatByTaskId.get(payload.taskId) ?? []
  const mark = Date.now()
  const time = nowTime()

  const userMessage: ChatMessageDto = {
    id: `chat-u-${mark}`,
    taskId: payload.taskId,
    role: 'user',
    content: payload.content,
    timestamp: time,
  }

  const aiMessage: ChatMessageDto = {
    id: `chat-a-${mark}`,
    taskId: payload.taskId,
    role: 'ai',
    content: `已接收任务消息：${payload.content}。下一步我会结合当前任务上下文继续执行。`,
    timestamp: nowTime(),
  }

  const nextMessages: ChatMessageDto[] = [...taskMessages, userMessage, aiMessage]
  chatByTaskId.set(payload.taskId, nextMessages)
  return nextMessages
}

function nowTime(): string {
  return new Date().toLocaleTimeString('zh-CN', { hour: '2-digit', minute: '2-digit' })
}

async function sleep(ms: number): Promise<void> {
  await new Promise((resolve) => {
    setTimeout(resolve, ms)
  })
}
