<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'

export interface UnifiedChatMessage {
  id: string
  type: 'user' | 'ai'
  content: string
  timestamp: string
}

export interface UnifiedChatOption {
  key: string
  label: string
}

const props = withDefaults(
  defineProps<{
    title?: string
    description?: string
    messages: UnifiedChatMessage[]
    modelValue: string
    options?: UnifiedChatOption[]
    placeholder?: string
    emptyDescription?: string
    loading?: boolean
    minHeight?: number
    maxWidth?: number
  }>(),
  {
    title: '',
    description: '',
    options: () => [],
    placeholder: '输入你的问题或指令，Ctrl+Enter 发送',
    emptyDescription: '暂无聊天内容',
    loading: false,
    minHeight: 460,
    maxWidth: 1040,
  },
)

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void
  (e: 'send'): void
  (e: 'option-click', optionKey: string): void
}>()

const messageBoardRef = ref<HTMLElement | null>(null)

const inputValue = computed({
  get: () => props.modelValue,
  set: (value: string) => emit('update:modelValue', value),
})

const panelStyle = computed(() => ({
  minHeight: `${props.minHeight}px`,
  maxWidth: `${props.maxWidth}px`,
}))

function emitSend(): void {
  emit('send')
}

function handleOptionClick(optionKey: string): void {
  emit('option-click', optionKey)
}

watch(
  () => props.messages.length,
  async () => {
    await nextTick()
    const board = messageBoardRef.value
    if (!board) {
      return
    }
    board.scrollTop = board.scrollHeight
  },
  { immediate: true },
)
</script>

<template>
  <section class="unified-chat-panel" :style="panelStyle">
    <header v-if="title || description" class="panel-header">
      <h3 v-if="title">{{ title }}</h3>
      <p v-if="description">{{ description }}</p>
    </header>

    <section ref="messageBoardRef" class="message-board">
      <el-empty v-if="!messages.length" :description="emptyDescription" />

      <div v-for="message in messages" :key="message.id" class="message-row" :class="`${message.type}-row`">
        <div class="message-avatar">{{ message.type === 'user' ? 'U' : 'AI' }}</div>
        <div class="message-bubble-wrap">
          <div class="message-bubble">{{ message.content }}</div>
          <div class="message-time">{{ message.timestamp }}</div>
        </div>
      </div>
    </section>

    <footer class="composer">
      <el-input
        v-model="inputValue"
        type="textarea"
        :autosize="{ minRows: 3, maxRows: 8 }"
        :placeholder="placeholder"
        @keyup.ctrl.enter="emitSend"
      />

      <div v-if="options.length" class="option-row">
        <el-button
          v-for="option in options"
          :key="option.key"
          class="option-chip"
          size="small"
          round
          plain
          @click="handleOptionClick(option.key)"
        >
          {{ option.label }}
        </el-button>
      </div>

      <slot name="after-options" />

      <div class="composer-actions">
        <slot name="actions">
          <el-button type="primary" :loading="loading" @click="emitSend">发送</el-button>
        </slot>
      </div>
    </footer>
  </section>
</template>

<style scoped>
.unified-chat-panel {
  width: min(100%, 1040px);
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  border: 1px solid #dbe4f0;
  border-radius: 16px;
  background: linear-gradient(180deg, #ffffff 0%, #fbfdff 100%);
  box-shadow: 0 14px 36px rgba(15, 23, 42, 0.08);
}

.panel-header {
  padding: 18px 20px;
  border-bottom: 1px solid #e2e8f0;
}

.panel-header h3 {
  margin: 0;
  font-size: 18px;
  color: #0f172a;
}

.panel-header p {
  margin: 6px 0 0;
  font-size: 13px;
  color: #64748b;
}

.message-board {
  padding: 20px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 14px;
  min-height: 340px;
}

.message-row {
  display: flex;
  gap: 10px;
}

.user-row {
  justify-content: flex-end;
}

.message-avatar {
  min-width: 30px;
  height: 30px;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 700;
  background: #e2e8f0;
  color: #0f172a;
}

.user-row .message-avatar {
  background: #2563eb;
  color: #ffffff;
}

.message-bubble-wrap {
  max-width: 72%;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.user-row .message-bubble-wrap {
  align-items: flex-end;
}

.message-bubble {
  padding: 11px 15px;
  border-radius: 14px;
  line-height: 1.5;
  font-size: 14px;
}

.user-row .message-bubble {
  background: linear-gradient(140deg, #2563eb 0%, #1d4ed8 100%);
  color: #ffffff;
}

.ai-row .message-bubble {
  background: #f1f5f9;
  color: #0f172a;
  border: 1px solid #e2e8f0;
}

.message-time {
  font-size: 11px;
  color: #94a3b8;
}

.composer {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: 16px 20px 18px;
  border-top: 1px solid #e2e8f0;
  background: linear-gradient(180deg, #f8fafc 0%, #f3f7ff 100%);
}

.option-row {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.option-chip {
  border-color: #dbeafe !important;
  background: linear-gradient(180deg, #ffffff 0%, #eff6ff 100%) !important;
  color: #1e3a8a !important;
  font-weight: 600;
}

.option-chip:hover {
  border-color: #60a5fa !important;
  color: #1d4ed8 !important;
  transform: translateY(-1px);
}

.composer-actions {
  display: flex;
  justify-content: flex-end;
}
</style>
