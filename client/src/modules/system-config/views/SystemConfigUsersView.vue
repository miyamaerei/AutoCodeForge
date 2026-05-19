<script setup lang="ts">
import { ref } from 'vue'

interface User {
  id: string
  username: string
  email: string
  role: string
  status: string
  joinDate: string
}

const users = ref<User[]>([
  {
    id: 'user-1',
    username: 'Alice Chen',
    email: 'alice@example.com',
    role: 'Admin',
    status: 'active',
    joinDate: '2024-01-15',
  },
  {
    id: 'user-2',
    username: 'Bob Smith',
    email: 'bob@example.com',
    role: 'Developer',
    status: 'active',
    joinDate: '2024-02-20',
  },
  {
    id: 'user-3',
    username: 'Carol Wang',
    email: 'carol@example.com',
    role: 'Developer',
    status: 'active',
    joinDate: '2024-03-10',
  },
  {
    id: 'user-4',
    username: 'David Lee',
    email: 'david@example.com',
    role: 'Viewer',
    status: 'inactive',
    joinDate: '2024-03-25',
  },
])

const showAddDialog = ref(false)
const newUser = ref({ username: '', email: '', role: 'Developer' })

const roles = ['Admin', 'Developer', 'Viewer']

const handleAddUser = () => {
  if (newUser.value.username && newUser.value.email) {
    const todayDate = new Date().toISOString().split('T')[0] ?? ''
    users.value.push({
      id: `user-${Date.now()}`,
      ...newUser.value,
      status: 'active',
      joinDate: todayDate,
    })
    newUser.value = { username: '', email: '', role: 'Developer' }
    showAddDialog.value = false
  }
}
</script>

<template>
  <section class="users-config">
    <el-card class="content-card">
      <template #header>
        <div class="card-header">
          <span>用户管理</span>
          <el-button type="primary" @click="showAddDialog = true">+ 添加用户</el-button>
        </div>
      </template>

      <el-table :data="users" stripe>
        <el-table-column prop="username" label="用户名" width="200" />
        <el-table-column prop="email" label="邮箱" min-width="250" />
        <el-table-column prop="role" label="角色" width="120" />
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.status === 'active' ? 'success' : 'info'">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="joinDate" label="加入日期" width="150" />
        <el-table-column label="操作" width="200">
          <template #default="{ row }">
            <el-button link type="primary" size="small">编辑</el-button>
            <el-button link type="primary" size="small">重置密码</el-button>
            <el-button link type="danger" size="small">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="showAddDialog" title="添加用户">
      <el-form :model="newUser">
        <el-form-item label="用户名">
          <el-input v-model="newUser.username" />
        </el-form-item>
        <el-form-item label="邮箱">
          <el-input v-model="newUser.email" type="email" />
        </el-form-item>
        <el-form-item label="角色">
          <el-select v-model="newUser.role">
            <el-option v-for="role in roles" :key="role" :label="role" :value="role" />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddDialog = false">取消</el-button>
        <el-button type="primary" @click="handleAddUser">确定</el-button>
      </template>
    </el-dialog>
  </section>
</template>

<style scoped>
.users-config {
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
