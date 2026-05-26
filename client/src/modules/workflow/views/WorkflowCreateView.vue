<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useWorkflowStore } from '../store/useWorkflowStore'
import { ElMessage } from 'element-plus'

const router = useRouter()
const store = useWorkflowStore()

/** 表单数据 */
const form = ref({
  name: '',
  description: '',
  contextProviders: [] as string[]
})

/** 表单验证规则 */
const rules = {
  name: [
    { required: true, message: '请输入工作流名称', trigger: 'blur' },
    { min: 2, max: 200, message: '长度在 2 到 200 个字符', trigger: 'blur' }
  ]
}

/** 表单引用 */
const formRef = ref()

/** 提交表单 */
const handleSubmit = async () => {
  if (!formRef.value) return
  
  await formRef.value.validate(async (valid: boolean) => {
    if (!valid) return
    
    try {
      const workflow = await store.createWorkflow(form.value)
      ElMessage.success('创建成功')
      router.push(`/workflows/${workflow.id}`)
    } catch (e) {
      ElMessage.error('创建失败: ' + String(e))
    }
  })
}

/** 返回列表 */
const handleBack = () => {
  router.push('/workflows')
}
</script>

<template>
  <section class="workflow-create">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>创建工作流</span>
          <el-button @click="handleBack">返回列表</el-button>
        </div>
      </template>

      <el-form ref="formRef" :model="form" :rules="rules" label-width="120px" class="create-form">
        <el-form-item label="工作流名称" prop="name">
          <el-input v-model="form.name" placeholder="请输入工作流名称" />
        </el-form-item>
        
        <el-form-item label="描述" prop="description">
          <el-input v-model="form.description" type="textarea" :rows="3" placeholder="请输入工作流描述" />
        </el-form-item>
        
        <el-form-item label="上下文提供者">
          <el-select v-model="form.contextProviders" multiple placeholder="选择上下文提供者" style="width: 100%">
            <el-option label="默认提供者" value="default" />
            <el-option label="Git信息提供者" value="git-info" />
            <el-option label="任务信息提供者" value="task-info" />
          </el-select>
        </el-form-item>
        
        <el-form-item>
          <el-button type="primary" @click="handleSubmit" :loading="store.loading">
            创建并编辑
          </el-button>
          <el-button @click="handleBack">取消</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </section>
</template>

<style scoped>
.workflow-create {
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

.create-form {
  max-width: 600px;
  margin: 0 auto;
}
</style>
