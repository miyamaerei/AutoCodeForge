export interface HostCommandResult {
  ok: boolean
  output?: string
  error?: string
}

export interface AutoCodeForgeHostBridge {
  runTerminalCommand: (command: string) => Promise<HostCommandResult>
}

interface PendingCommand {
  resolve: (value: HostCommandResult) => void
  timer: number
}

interface VsCodeApi {
  postMessage: (message: unknown) => void
}

const pendingCommands = new Map<string, PendingCommand>()

function createFallbackBridge(): AutoCodeForgeHostBridge {
  return {
    async runTerminalCommand() {
      return {
        ok: false,
        error: '未检测到宿主桥接，无法直接执行终端命令。',
      }
    },
  }
}

function createVsCodeWebviewBridge(): AutoCodeForgeHostBridge | null {
  const acquire = window.acquireVsCodeApi
  if (typeof acquire !== 'function') {
    return null
  }

  const vscodeApi = acquire() as VsCodeApi

  const handleMessage = (event: MessageEvent<unknown>) => {
    const payload = event.data as {
      type?: string
      requestId?: string
      ok?: boolean
      output?: string
      error?: string
    }

    if (payload.type !== 'autocodeforge.runTerminalCommand.result' || !payload.requestId) {
      return
    }

    const pending = pendingCommands.get(payload.requestId)
    if (!pending) {
      return
    }

    window.clearTimeout(pending.timer)
    pendingCommands.delete(payload.requestId)
    pending.resolve({
      ok: Boolean(payload.ok),
      output: payload.output,
      error: payload.error,
    })
  }

  window.addEventListener('message', handleMessage)

  return {
    runTerminalCommand(command: string) {
      const requestId = `cmd-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`

      return new Promise<HostCommandResult>((resolve) => {
        const timer = window.setTimeout(() => {
          pendingCommands.delete(requestId)
          resolve({
            ok: false,
            error: '宿主未在 20 秒内返回执行结果。',
          })
        }, 20000)

        pendingCommands.set(requestId, { resolve, timer })

        try {
          vscodeApi.postMessage({
            type: 'autocodeforge.runTerminalCommand',
            requestId,
            command,
          })
        } catch {
          window.clearTimeout(timer)
          pendingCommands.delete(requestId)
          resolve({
            ok: false,
            error: '发送终端命令到宿主失败。',
          })
        }
      })
    },
  }
}

export function installHostBridge() {
  const globalScope = window as Window

  // External host can inject before app start.
  if (globalScope.__AUTOCODEFORGE_HOST__?.runTerminalCommand) {
    return
  }

  const vscodeBridge = createVsCodeWebviewBridge()
  globalScope.__AUTOCODEFORGE_HOST__ = vscodeBridge || createFallbackBridge()

  globalScope.__AUTOCODEFORGE_BRIDGE_INJECTOR__ = {
    setHostBridge(bridge: AutoCodeForgeHostBridge) {
      globalScope.__AUTOCODEFORGE_HOST__ = bridge
    },
  }
}
