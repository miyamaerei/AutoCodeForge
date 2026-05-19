<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useTaskCenterStore } from '../store/useTaskCenterStore'
import UnifiedChatPanel, { type UnifiedChatMessage, type UnifiedChatOption } from '../../../components/UnifiedChatPanel.vue'
import { getChatOptionsByModule } from '../../../config/chat-options'

const route = useRoute()
const router = useRouter()
const store = useTaskCenterStore()

const { tasks, selectedTask, logs, chat, diff, error, loading, detailLoading, chatSending, hasTasks, hasDetail } =
  storeToRefs(store)
const taskId = computed(() => String(route.params.id ?? ''))
const taskInputMessage = ref('')
const taskQuickOptions = getChatOptionsByModule('task-center')
const taskChatOptions: UnifiedChatOption[] = taskQuickOptions.map((option) => ({
  key: option.key,
  label: option.label,
}))

onMounted(async () => {
  if (!hasTasks.value) {
    await store.loadTasks()
  }
  if (taskId.value) {
    await store.loadTaskDetail(taskId.value)
  }
})

watch(taskId, async (nextId) => {
  taskInputMessage.value = ''
  if (nextId) {
    await store.loadTaskDetail(nextId)
  }
})

const steps = computed(() => selectedTask.value?.steps ?? [])
const taskChatMessages = computed<UnifiedChatMessage[]>(() => {
  return chat.value.map<UnifiedChatMessage>((message) => ({
    id: message.id,
    type: message.role,
    content: message.content,
    timestamp: message.timestamp,
  }))
})

const switchTask = async (id: string) => {
  await router.push(`/task-center/${id}`)
}

const sendTaskMessage = async () => {
  if (!taskId.value) {
    return
  }
  const text = taskInputMessage.value.trim()
  if (!text) {
    return
  }
  taskInputMessage.value = ''
  await store.sendTaskChat(taskId.value, text)
}

const applyTaskOption = (optionKey: string) => {
  const targetOption = taskQuickOptions.find((option) => option.key === optionKey)
  if (targetOption) {
    taskInputMessage.value = targetOption.prompt
  }
}
</script>

<template>
  <section class="task-center">
    <el-row :gutter="12" class="three-panel">
      <el-col :span="5" class="pane-col left-pane">
        <el-card class="pane" shadow="never">
          <template #header>任务列表</template>
          <el-scrollbar max-height="560px">
            <el-menu :default-active="taskId || ''">
              <el-menu-item v-for="task in tasks" :key="task.id" :index="task.id">
                <span @click="switchTask(task.id)">{{ task.title }}</span>
                <el-tag size="small" class="task-state">{{ task.state }}</el-tag>
              </el-menu-item>
            </el-menu>
          </el-scrollbar>
        </el-card>
      </el-col>

      <el-col :span="11" class="pane-col center-pane">
        <el-card class="pane" shadow="never">
          <template #header>统一聊天框</template>
          <el-skeleton v-if="loading || detailLoading" :rows="6" animated />

          <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" />

          <el-empty v-else-if="!hasDetail" description="请选择一个任务查看详情" />

          <template v-else>
            <UnifiedChatPanel
              v-model="taskInputMessage"
              title="任务会话"
              description="聊天框保持中置，消息增加时在中轴区域扩展。"
              :messages="taskChatMessages"
              :loading="detailLoading"
              :options="taskChatOptions"
              :min-height="540"
              :max-width="1120"
              placeholder="输入需求或执行指令，Ctrl+Enter 发送"
              @send="sendTaskMessage"
              @option-click="applyTaskOption"
            >
              <template #actions>
                <el-space>
                  <el-button type="primary" :loading="chatSending" @click="sendTaskMessage">发送</el-button>
                  <el-button>重试</el-button>
                  <el-button type="danger" plain>终止</el-button>
                </el-space>
              </template>
            </UnifiedChatPanel>
          </template>
        </el-card>
      </el-col>

      <el-col :span="8" class="pane-col right-pane">
        <el-card class="pane" shadow="never">
          <template #header>执行过程 / 日志 / Diff</template>
          <el-skeleton v-if="loading || detailLoading" :rows="5" animated />

          <template v-else-if="hasDetail">
            <el-scrollbar max-height="290px">
            <el-steps direction="vertical" :active="4">
              <el-step v-for="step in steps" :key="step.id" :title="step.title" />
            </el-steps>
          </el-scrollbar>
          <el-divider />
          <el-scrollbar max-height="120px">
            <p v-for="line in logs" :key="line.id" class="log-line">{{ line.message }}</p>
          </el-scrollbar>
          <el-divider />
            <pre v-if="diff" class="diff-preview">- {{ diff.oldCode }}
+ {{ diff.newCode }}</pre>
            <el-empty v-else description="暂无 Diff 数据" />
          </template>

          <el-empty v-else description="暂无详情数据" />
        </el-card>
      </el-col>
    </el-row>
  </section>
</template>

<style scoped>
.task-center {
  margin-top: 0;
  min-width: 1320px;
}

.pane {
  min-height: 650px;
}

.pane-col {
  display: flex;
}

.pane-col .pane {
  width: 100%;
}

.left-pane .pane {
  min-width: 250px;
}

.center-pane .pane {
  min-width: 520px;
}

.right-pane .pane {
  min-width: 360px;
}

.task-state {
  margin-left: 8px;
}

.actions {
  margin-top: 12px;
}

.log-line {
  margin: 0 0 4px;
  font-family: Consolas, 'Courier New', monospace;
  font-size: 12px;
}

.diff-preview {
  margin: 0;
  padding: 8px;
  background: #f5f5f5;
  border-radius: 4px;
  font-size: 12px;
  font-family: Consolas, 'Courier New', monospace;
}
</style>
