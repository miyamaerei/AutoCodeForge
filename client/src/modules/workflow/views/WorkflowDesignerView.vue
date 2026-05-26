<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { useWorkflowStore } from '../store/useWorkflowStore'
import WorkflowCanvas from '../components/WorkflowCanvas.vue'

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
  <section class="workflow-designer-view">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>工作流设计器</span>
        </div>
      </template>
      <WorkflowCanvas />
    </el-card>
  </section>
</template>

<style scoped>
.workflow-designer-view {
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
