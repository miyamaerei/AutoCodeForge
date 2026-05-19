export interface TaskLogDto {
  id: string
  taskId: string
  message: string
}

const logsByTaskId: Record<string, TaskLogDto[]> = {
  'T-1009': [
    { id: 'log-1', taskId: 'T-1009', message: '[14:31:22] clone repository success' },
    { id: 'log-2', taskId: 'T-1009', message: '[14:31:24] apply patch to OrderService' },
    { id: 'log-3', taskId: 'T-1009', message: '[14:31:26] build failed: null check missing' },
    { id: 'log-4', taskId: 'T-1009', message: '[14:31:29] auto-fix applied' },
    { id: 'log-5', taskId: 'T-1009', message: '[14:31:31] build success' },
  ],
}

export async function getTaskLogs(taskId: string): Promise<TaskLogDto[]> {
  await sleep(120)
  return logsByTaskId[taskId] ?? []
}

async function sleep(ms: number): Promise<void> {
  await new Promise((resolve) => {
    setTimeout(resolve, ms)
  })
}
