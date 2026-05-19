/// <reference types="vite/client" />

interface HostCommandResult {
	ok: boolean
	output?: string
	error?: string
}

interface AutoCodeForgeHostBridge {
	runTerminalCommand: (command: string) => Promise<HostCommandResult>
}

interface AutoCodeForgeBridgeInjector {
	setHostBridge: (bridge: AutoCodeForgeHostBridge) => void
}

interface Window {
	__AUTOCODEFORGE_HOST__?: AutoCodeForgeHostBridge
	__AUTOCODEFORGE_BRIDGE_INJECTOR__?: AutoCodeForgeBridgeInjector
	acquireVsCodeApi?: () => {
		postMessage: (message: unknown) => void
	}
}
