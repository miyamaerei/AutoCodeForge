export interface TaskDiffDto {
  taskId: string
  fileName: string
  oldCode: string
  newCode: string
}

const diffByTaskId: Record<string, TaskDiffDto> = {
  'T-1009': {
    taskId: 'T-1009',
    fileName: 'OrderService.cs',
    oldCode: 'return exportOrder(data)',
    newCode: 'return exportOrder(data ?? [])',
  },
}

export async function getTaskDiff(taskId: string): Promise<TaskDiffDto | null> {
  await sleep(140)
  return diffByTaskId[taskId] ?? null
}

async function sleep(ms: number): Promise<void> {
  await new Promise((resolve) => {
    setTimeout(resolve, ms)
  })
}
