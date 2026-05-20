---
name: vue3-component-builder
description: 'Create Vue 3 shared components with TypeScript. Use for: add reusable UI components, create form components, build layout components, implement component with props/emits/slots.'
argument-hint: 'Describe component name and features (e.g. UserCard with avatar and actions)'
---

# Vue3 Component Builder

## When to Use
- You need to create a reusable Vue 3 component.
- You want to build shared UI components.
- You need to create form components with validation.
- You want to implement layout components.

## Fixed Conventions
1. Shared components are placed under `client/src/components/`.
2. Component names use PascalCase, example `UserCard.vue`.
3. Components use `<script setup lang="ts">` syntax.
4. Props are defined with TypeScript types.
5. Emits are defined with TypeScript types.
6. Components include proper JSDoc comments.

## Required Input
1. Component name in PascalCase, example `UserCard`.
2. Component purpose and features.
3. Props needed: data, callbacks, configuration.
4. Emits needed: events to emit.
5. Slots needed: content projection areas.
6. Styling approach: scoped CSS, CSS modules, etc.

## Output Structure
- `client/src/components/<ComponentName>.vue` - Component file

## Template Structure

### Basic Component Template
```vue
<script setup lang="ts">
/**
 * <ComponentName> - <Description>
 * 
 * @example
 * <<ComponentName>
 *   :prop1="value1"
 *   @event1="handleEvent1"
 * />
 */

// Props
interface Props {
  /** Prop description */
  title: string
  /** Optional prop with default */
  size?: 'small' | 'medium' | 'large'
  /** Optional prop without default */
  disabled?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  size: 'medium',
  disabled: false,
})

// Emits
interface Emits {
  /** Event description */
  (e: 'click', id: string): void
  /** Event with payload */
  (e: 'update', value: string): void
}

const emit = defineEmits<Emits>()

// Reactive State
const isHovered = ref(false)

// Computed
const computedClasses = computed(() => ({
  '<component>': true,
  '<component>--disabled': props.disabled,
  '<component>--hovered': isHovered.value,
  [`<component>--${props.size}`]: true,
}))

// Methods
function handleClick(): void {
  if (!props.disabled) {
    emit('click', props.id)
  }
}
</script>

<template>
  <div
    :class="computedClasses"
    @click="handleClick"
    @mouseenter="isHovered = true"
    @mouseleave="isHovered = false"
  >
    <slot name="header">
      <h3>{{ title }}</h3>
    </slot>
    
    <slot>
      <!-- Default content -->
    </slot>
    
    <slot name="footer">
      <!-- Footer content -->
    </slot>
  </div>
</template>

<style scoped>
.<component> {
  /* Base styles */
}

.<component>--disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.<component>--small {
  padding: 8px;
}

.<component>--medium {
  padding: 16px;
}

.<component>--large {
  padding: 24px;
}
</style>
```

### Form Component Template
```vue
<script setup lang="ts">
import { useForm } from 'vee-validate'
import { z } from 'zod'
import { toTypedSchema } from '@vee-validate/zod'

/**
 * <FormName> - <Description>
 */

// Validation Schema
const schema = toTypedSchema(
  z.object({
    username: z.string().min(3, '用户名至少3个字符'),
    email: z.string().email('请输入有效的邮箱地址'),
    password: z.string().min(8, '密码至少8个字符'),
  })
)

// Form Setup
const { defineField, handleSubmit, errors } = useForm({
  validationSchema: schema,
})

// Field Definitions
const [username, usernameAttrs] = defineField('username')
const [email, emailAttrs] = defineField('email')
const [password, passwordAttrs] = defineField('password')

// Props
interface Props {
  initialData?: {
    username?: string
    email?: string
  }
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
})

// Emits
interface Emits {
  (e: 'submit', values: { username: string; email: string; password: string }): void
  (e: 'cancel'): void
}

const emit = defineEmits<Emits>()

// Methods
const onSubmit = handleSubmit((values) => {
  emit('submit', values)
})

function handleCancel(): void {
  emit('cancel')
}
</script>

<template>
  <el-form
    :model="{ username, email, password }"
    :disabled="loading"
    @submit.prevent="onSubmit"
  >
    <el-form-item label="用户名" :error="errors.username">
      <el-input
        v-model="username"
        v-bind="usernameAttrs"
        placeholder="请输入用户名"
      />
    </el-form-item>

    <el-form-item label="邮箱" :error="errors.email">
      <el-input
        v-model="email"
        v-bind="emailAttrs"
        type="email"
        placeholder="请输入邮箱"
      />
    </el-form-item>

    <el-form-item label="密码" :error="errors.password">
      <el-input
        v-model="password"
        v-bind="passwordAttrs"
        type="password"
        placeholder="请输入密码"
        show-password
      />
    </el-form-item>

    <el-form-item>
      <el-button type="primary" native-type="submit" :loading="loading">
        提交
      </el-button>
      <el-button @click="handleCancel">
        取消
      </el-button>
    </el-form-item>
  </el-form>
</template>
```

### Table Component Template
```vue
<script setup lang="ts">
import type { TableProps } from 'element-plus'

/**
 * <TableName> - <Description>
 */

// Types
export interface <Entity>TableItem {
  id: string
  // ... other fields
}

// Props
interface Props {
  data: <Entity>TableItem[]
  loading?: boolean
  selectable?: boolean
  pagination?: {
    page: number
    pageSize: number
    total: number
  }
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  selectable: false,
})

// Emits
interface Emits {
  (e: 'row-click', row: <Entity>TableItem): void
  (e: 'selection-change', rows: <Entity>TableItem[]): void
  (e: 'page-change', page: number): void
  (e: 'page-size-change', size: number): void
}

const emit = defineEmits<Emits>()

// State
const selectedRows = ref<<Entity>TableItem[]>([])

// Table Configuration
const tableProps: Partial<TableProps<<Entity>TableItem>> = {
  stripe: true,
  highlightCurrentRow: true,
}

// Methods
function handleRowClick(row: <Entity>TableItem): void {
  emit('row-click', row)
}

function handleSelectionChange(rows: <Entity>TableItem[]): void {
  selectedRows.value = rows
  emit('selection-change', rows)
}

function handlePageChange(page: number): void {
  emit('page-change', page)
}

function handlePageSizeChange(size: number): void {
  emit('page-size-change', size)
}
</script>

<template>
  <div class="<entity>-table">
    <el-table
      v-loading="loading"
      :data="data"
      v-bind="tableProps"
      @row-click="handleRowClick"
      @selection-change="handleSelectionChange"
    >
      <el-table-column
        v-if="selectable"
        type="selection"
        width="55"
      />

      <el-table-column prop="id" label="ID" width="120" />
      
      <!-- Add more columns -->
      
      <el-table-column label="操作" width="120">
        <template #default="{ row }">
          <slot name="actions" :row="row">
            <el-button link type="primary" size="small">
              查看
            </el-button>
          </slot>
        </template>
      </el-table-column>
    </el-table>

    <el-pagination
      v-if="pagination"
      v-model:current-page="pagination.page"
      v-model:page-size="pagination.pageSize"
      :total="pagination.total"
      :page-sizes="[10, 20, 50, 100]"
      layout="total, sizes, prev, pager, next, jumper"
      @current-change="handlePageChange"
      @size-change="handlePageSizeChange"
    />
  </div>
</template>

<style scoped>
.<entity>-table {
  width: 100%;
}

.el-pagination {
  margin-top: 16px;
  justify-content: flex-end;
}
</style>
```

## Component Types

### 1. Display Components (展示组件)
- **Card**: 信息卡片
- **Badge**: 徽章
- **Tag**: 标签
- **Avatar**: 头像
- **Icon**: 图标

### 2. Form Components (表单组件)
- **Input**: 输入框
- **Select**: 选择器
- **DatePicker**: 日期选择器
- **Upload**: 上传组件
- **Form**: 表单容器

### 3. Layout Components (布局组件)
- **Container**: 容器
- **Grid**: 网格
- **Sidebar**: 侧边栏
- **Header**: 头部
- **Footer**: 底部

### 4. Data Components (数据组件)
- **Table**: 表格
- **List**: 列表
- **Tree**: 树形
- **Pagination**: 分页

### 5. Feedback Components (反馈组件)
- **Modal**: 模态框
- **Drawer**: 抽屉
- **Toast**: 提示
- **Loading**: 加载

## Best Practices

### 1. Props Design
- 使用 TypeScript 定义 Props 类型
- 为可选 Props 提供默认值
- 使用 JSDoc 注释说明 Props 用途
- 避免使用 `any` 类型

### 2. Emits Design
- 使用 TypeScript 定义 Emits 类型
- 事件名使用 kebab-case
- 为事件添加 JSDoc 注释
- 事件参数要明确类型

### 3. Slots Design
- 为 Slot 提供默认内容
- 使用具名 Slot 区分不同区域
- 为 Slot 添加 JSDoc 注释
- 提供 Slot 的使用示例

### 4. Styling
- 使用 Scoped CSS
- 遵循 BEM 命名规范
- 使用 CSS 变量支持主题
- 响应式设计考虑移动端

### 5. Accessibility
- 添加 ARIA 属性
- 支持键盘导航
- 提供焦点管理
- 考虑屏幕阅读器

## Execution Checklist
1. Define component purpose and features.
2. Design Props interface with TypeScript.
3. Design Emits interface with TypeScript.
4. Design Slots structure.
5. Implement component logic.
6. Add proper styling.
7. Add JSDoc comments.
8. Test component in isolation.
9. Create usage examples.

## Quality Gates
1. TypeScript types are properly defined.
2. Props have proper defaults and validations.
3. Emits are properly typed.
4. Slots have clear structure.
5. Styling follows conventions.
6. Component is accessible.
7. JSDoc comments are complete.
8. Component is tested.

## Example Prompts
- /vue3-component-builder create UserCard with avatar and actions
- /vue3-component-builder create UserForm with validation
- /vue3-component-builder create DataTable with pagination and selection
- /vue3-component-builder create ModalDialog with header and footer slots
