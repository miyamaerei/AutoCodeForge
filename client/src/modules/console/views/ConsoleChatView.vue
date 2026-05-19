<script setup lang="ts">
import { computed, onMounted, toRef } from 'vue'
import { useRouter } from 'vue-router'
import { useConsoleChat, type ChatMode } from '../composables/useConsoleChat'
import UnifiedChatPanel, { type UnifiedChatOption } from '../../../components/UnifiedChatPanel.vue'
import { getChatOptionsByModule } from '../../../config/chat-options'

const props = withDefaults(
  defineProps<{
    mode?: ChatMode
  }>(),
  {
    mode: 'ask',
  },
)

const router = useRouter()
const chatMode = toRef(props, 'mode')
const {
  initializing,
  sending,
  error,
  inputMessage,
  selectedSession,
  sessionList,
  suggestedQuestions,
  hasSessions,
  isEmptyState,
  activeMessages,
  initialize,
  createSession,
  selectSession,
  deleteSession,
  sendMessage,
  applySuggestedQuestion,
} = useConsoleChat(chatMode)

const pageTitle = computed(() => (chatMode.value === 'session' ? '统一聊天 · 会话模式' : '统一聊天 · 提问模式'))
const pageDescription = computed(() =>
  chatMode.value === 'session'
    ? '左侧管理会话，右侧连续对话。'
    : '面向快速提问，保留统一消息控件与输入区。',
)

const showSuggestionPanel = computed(() => chatMode.value === 'ask' && activeMessages.value.length <= 1)

const chatOptions = computed<UnifiedChatOption[]>(() => {
  const moduleKey = chatMode.value === 'ask' ? 'console.ask' : 'console.session'
  return getChatOptionsByModule(moduleKey).map((option) => ({
    key: option.key,
    label: option.label,
  }))
})

function handleOptionClick(optionKey: string): void {
  if (optionKey === 'history') {
    if (chatMode.value === 'ask') {
      void router.push('/session')
      return
    }
    if (sessionList.value.length > 0) {
      selectSession(sessionList.value[0]!.id)
    }
    return
  }

  const moduleKey = chatMode.value === 'ask' ? 'console.ask' : 'console.session'
  const targetOption = getChatOptionsByModule(moduleKey).find((option) => option.key === optionKey)
  if (targetOption) {
    inputMessage.value = targetOption.prompt
  }
}

onMounted(async () => {
  await initialize()
})
</script>

<template>
  <section class="chat-view">
    <el-skeleton v-if="initializing" :rows="4" animated class="chat-skeleton" />
    <el-alert
      v-else-if="error"
      class="chat-error"
      :title="error"
      type="error"
      show-icon
      :closable="false"
    />
    <div v-else-if="isEmptyState" class="chat-empty">
      <el-empty description="暂无可展示的聊天内容" />
    </div>

    <div v-else class="chat-shell">
      <aside class="chat-side-panel">
        <header class="panel-header">
          <h3>{{ chatMode === 'session' ? '会话历史' : '快速提问' }}</h3>
          <el-button v-if="chatMode === 'session'" type="primary" size="small" @click="createSession">
            新建
          </el-button>
        </header>

        <div v-if="chatMode === 'session'" class="session-list">
          <div
            v-for="session in sessionList"
            :key="session.id"
            class="session-item"
            :class="{ active: selectedSession?.id === session.id }"
            @click="selectSession(session.id)"
          >
            <div class="session-content">
              <div class="session-title">{{ session.title }}</div>
              <div class="session-preview">{{ session.preview }}</div>
              <div class="session-meta">
                <span>{{ session.messagesCount }} 条消息</span>
                <span>{{ session.timestamp }}</span>
              </div>
            </div>
            <el-popconfirm
              title="确定删除此会话？"
              confirm-button-text="确定"
              cancel-button-text="取消"
              @confirm="deleteSession(session.id)"
            >
              <template #reference>
                <el-button type="danger" link size="small" @click.stop>删除</el-button>
              </template>
            </el-popconfirm>
          </div>
          <el-empty v-if="!hasSessions" description="暂无会话" :image-size="76" />
        </div>

        <div v-else class="ask-quick-panel">
          <p class="quick-tip">点击一个问题可直接填入输入框。</p>
          <div class="question-list">
            <button
              v-for="question in suggestedQuestions"
              :key="question"
              class="question-button"
              @click="applySuggestedQuestion(question)"
            >
              {{ question }}
            </button>
          </div>
        </div>
      </aside>

      <main class="chat-main-panel">
        <div class="chat-main-center">
          <UnifiedChatPanel
            v-model="inputMessage"
            :title="pageTitle"
            :description="pageDescription"
            :messages="activeMessages"
            :loading="sending"
            :options="chatOptions"
            :min-height="520"
            :max-width="1120"
            @send="sendMessage"
            @option-click="handleOptionClick"
          >
            <template #after-options>
              <div v-if="showSuggestionPanel" class="inline-tip">也可以从左侧快速提问区选择预设问题。</div>
            </template>
          </UnifiedChatPanel>
        </div>
      </main>
    </div>
  </section>
</template>

<style scoped>
.chat-view {
  width: 100%;
  height: 100%;
  overflow-x: auto;
}

.chat-skeleton,
.chat-error {
  margin: 20px;
}

.chat-empty {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
}

.chat-shell {
  display: grid;
  grid-template-columns: 340px minmax(860px, 1fr);
  min-width: 1280px;
  height: 100%;
  gap: 16px;
  padding: 20px;
}

.chat-side-panel {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  border-bottom: 1px solid #e2e8f0;
}

.panel-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 700;
  color: #0f172a;
}

.session-list {
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  overflow-y: auto;
}

.session-item {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 10px;
  display: flex;
  gap: 8px;
  justify-content: space-between;
  cursor: pointer;
  transition: border-color 0.2s ease;
}

.session-item:hover {
  border-color: #94a3b8;
}

.session-item.active {
  border-color: #2563eb;
  background: #eff6ff;
}

.session-content {
  min-width: 0;
}

.session-title {
  font-size: 14px;
  font-weight: 600;
  color: #0f172a;
  margin-bottom: 4px;
}

.session-preview {
  font-size: 12px;
  color: #475569;
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
  margin-bottom: 4px;
}

.session-meta {
  display: flex;
  gap: 8px;
  font-size: 11px;
  color: #64748b;
}

.ask-quick-panel {
  padding: 16px;
  overflow-y: auto;
}

.quick-tip {
  margin: 0 0 12px;
  font-size: 12px;
  color: #64748b;
}

.question-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.question-button {
  border: 1px solid #dbeafe;
  background: #f8fafc;
  color: #1e293b;
  padding: 10px;
  border-radius: 8px;
  text-align: left;
  cursor: pointer;
}

.question-button:hover {
  border-color: #60a5fa;
  background: #eff6ff;
}

.chat-main-panel {
  display: grid;
  place-items: center;
  min-width: 0;
  min-height: 700px;
}


.chat-main-center {
  width: 100%;
  display: flex;
  justify-content: center;
  padding: 8px 0;
}

.inline-tip {
  font-size: 12px;
  color: #64748b;
}
</style>
