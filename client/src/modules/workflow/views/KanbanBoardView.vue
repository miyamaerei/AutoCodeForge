<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { useWorkflowStore } from '../store/useWorkflowStore'
import KanbanBoard from '../components/KanbanBoard.vue'

const workflowStore = useWorkflowStore()

onMounted(() => {
  workflowStore.loadTasks()
  workflowStore.loadAgents()
})

onUnmounted(() => {
  workflowStore.cleanup()
})
</script>

<template>
  <section class="kanban-board-view">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>任务看板</span>
        </div>
      </template>
      <KanbanBoard />
    </el-card>
  </section>
</template>

<style scoped>
.kanban-board-view {
  min-width: 1280px;
}

.content-card {
  margin-bottom: 1rem;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>
