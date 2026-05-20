---
name: vue3-project-onboarding
description: 'Guide new developers through Vue 3 project onboarding. Use for: understand project structure, learn tech stack, follow coding conventions, get started quickly.'
argument-hint: 'Describe what you want to learn (e.g. project structure, tech stack, or how to start development)'
---

# Vue3 Project Onboarding

## When to Use
- You are new to this Vue 3 project and need guidance.
- You want to understand the project structure and architecture.
- You need to learn the tech stack and coding conventions.
- You want to know how to start development quickly.

## Project Overview

### Project Name
**AutoCodeForge** - AI-powered code automation platform

### Tech Stack
| Category | Technology | Version |
|----------|-----------|---------|
| Framework | Vue 3 | ^3.5.32 |
| Language | TypeScript | ~6.0.0 |
| State Management | Pinia | ^3.0.4 |
| Routing | Vue Router | ^5.0.7 |
| UI Library | Element Plus | ^2.14.0 |
| HTTP Client | Axios | ^1.16.1 |
| Build Tool | Vite | ^8.0.8 |
| Testing | Vitest | ^4.1.4 |
| Validation | Vee-Validate + Zod | ^4.15.1 |
| Charts | ECharts + vue-echarts | ^6.0.0 |
| Markdown | markdown-it | ^14.1.1 |

### Node.js Requirements
- Node.js: `^20.19.0` or `>=22.12.0`

## Project Structure

```
AutoCodeForge/
├── .github/                    # GitHub 配置
│   ├── instructions/           # 项目指令和规范
│   └── skills/                 # GitHub Skills
├── .trae/                      # Trae IDE 配置
│   └── skills/                 # 本地 Skills
├── docs/                       # 项目文档
├── client/                     # 前端应用
│   ├── public/                 # 静态资源
│   │   └── AutoCodeForge.wiki/ # Wiki 文档
│   └── src/                    # 源代码
│       ├── assets/             # 静态资源（CSS、图片）
│       ├── components/         # 共享组件
│       ├── config/             # 配置文件
│       ├── host/               # 宿主集成
│       ├── lib/                # 工具库
│       ├── mock/               # Mock 数据
│       ├── modules/            # 功能模块（核心）
│       ├── router/             # 路由配置
│       ├── stores/             # 全局 Store
│       ├── views/              # 全局视图
│       ├── App.vue             # 根组件
│       └── main.ts             # 入口文件
├── server/                     # 后端服务
├── tools/                      # 开发工具
├── package.json                # 根域配置
├── package-lock.json
├── client/
│   ├── package.json            # 前端依赖
│   ├── vite.config.ts          # Vite 配置
│   ├── vitest.config.ts        # Vitest 配置
│   └── tsconfig*.json          # TypeScript 配置
└── server/
    ├── package.json            # 后端依赖
    └── tsconfig.json           # 后端配置
```

## Module Architecture

### Module Structure
每个功能模块遵循统一的结构：

```
client/src/modules/<module-name>/
├── composables/                # 组合式函数
│   └── use<Feature>.ts
├── store/                      # Pinia Store
│   └── use<Module>Store.ts
├── views/                      # 视图组件
│   ├── <Module>ListView.vue
│   ├── <Module>DetailView.vue
│   └── <Module>FormView.vue
├── <module>.api.ts             # API 层
├── routes.ts                   # 路由配置
└── index.ts                    # 模块导出
```

### Current Modules
| Module | Path | Description |
|--------|------|-------------|
| Console | `/console` | AI 开发控制台 |
| Task Center | `/task-center` | AI 任务中心 |
| Repo Management | `/repo-management` | 仓库管理 |
| Pipeline Center | `/pipeline-center` | 流水线中心 |
| System Config | `/system-config` | 系统配置 |
| Dashboard | `/dashboard` | 工作台 |
| MD Wiki | `/md-wiki` | Markdown Wiki |
| Agent Center | `/agent` | Agent 管理 |
| Scheduled Task | `/scheduled-task` | 定时任务 |

## Key Concepts

### 1. Mock/Real API Switching
项目支持 Mock 和真实 API 切换：

```typescript
// client/src/config/runtime.ts
export const USE_MOCK = true  // 切换为 false 使用真实 API

// client/src/modules/<module>/<module>.api.ts
export async function fetchData(): Promise<DataDto> {
  if (USE_MOCK) {
    return fetchDataMock()  // 使用 Mock
  }
  const { data } = await request.get<DataDto>('/api/endpoint')
  return data
}
```

### 2. Pinia Setup Store
使用 Pinia Setup Store 语法：

```typescript
export const useModuleStore = defineStore('module.name', () => {
  const items = ref<Item[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchItems(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      items.value = await api.fetchItems()
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed'
    } finally {
      loading.value = false
    }
  }

  return { items, loading, error, fetchItems }
})
```

### 3. Route Conventions
路由配置约定：

```typescript
export const moduleRoutes: RouteRecordRaw[] = [
  {
    path: '/module',
    name: 'module.list',
    component: () => import('./views/ModuleListView.vue'),
    meta: {
      requiresAuth: false,  // 必须明确设置
      title: '模块列表',
    },
  },
]
```

### 4. PC-First Design
项目采用 PC-First 设计理念：
- 最小宽度：1280px
- 多列布局
- 桌面导航可见
- 不优先优化移动端

## Development Workflow

### 1. Start Development
```bash
# 安装依赖
npm install

# 启动开发服务器
npm run dev

# 类型检查
npm run type-check

# 运行测试
npm run test:unit

# 构建生产版本
npm run build
```

### 2. Create New Module
使用 Skill 创建新模块：

```
/vue3-api-model-router-store create <module> with list detail form
```

### 3. Create New Page
使用 Skill 创建新页面：

```
/vue3-page-builder create <module> list page at /module
```

### 4. Create Composable
使用 Skill 创建 Composable：

```
/vue3-composable-builder create use<Feature> with <features>
```

## Coding Conventions

### 1. Naming Conventions
| Type | Convention | Example |
|------|-----------|---------|
| Components | PascalCase | `UserList.vue` |
| Views | PascalCase + View | `UserListView.vue` |
| Composables | camelCase + use | `useUserList.ts` |
| Stores | camelCase + use + Store | `useUserStore.ts` |
| APIs | camelCase | `user.api.ts` |
| Routes | kebab-case | `user-management` |

### 2. File Organization
- 每个模块独立目录
- 相关文件放在一起
- 导出统一在 `index.ts`
- 类型定义在同一文件或 `.types.ts`

### 3. TypeScript Guidelines
- 所有函数和变量都要有类型
- 使用 `interface` 定义对象类型
- 使用 `type` 定义联合类型
- 避免使用 `any`

### 4. Vue 3 Best Practices
- 使用 `<script setup>` 语法
- 使用 Composition API
- Props 使用 `defineProps` with TypeScript
- Emits 使用 `defineEmits` with TypeScript

## Available Skills

### Code Generation Skills
| Skill | Purpose | Command |
|-------|---------|---------|
| vue3-api-model-router-store | 创建完整模块 | `/vue3-api-model-router-store` |
| vue3-page-builder | 创建单个页面 | `/vue3-page-builder` |
| vue3-composable-builder | 创建 Composable | `/vue3-composable-builder` |
| vue3-mock-builder | 创建 Mock 数据 | `/vue3-mock-builder` |
| vue3-api-mock-integration | 创建 API 层 | `/vue3-api-mock-integration` |
| vue3-component-builder | 创建共享组件 | `/vue3-component-builder` |
| vue3-store-builder | 创建 Pinia Store | `/vue3-store-builder` |
| vue3-test-builder | 创建测试用例 | `/vue3-test-builder` |

### Planning Skills
| Skill | Purpose | Command |
|-------|---------|---------|
| vue3-feature-planner | 规划新功能 | `/vue3-feature-planner` |

## Quick Start Guide

### Step 1: Understand the Project
1. 阅读本文档了解项目结构
2. 查看 `client/src/modules/` 了解现有模块
3. 查看 `client/src/mock/` 了解 Mock 数据结构

### Step 2: Set Up Development Environment
1. 安装 Node.js 20.19+ 或 22.12+
2. 运行 `npm install`
3. 运行 `npm run dev` 启动开发服务器
4. 访问 `http://localhost:5173`

### Step 3: Create Your First Feature
1. 使用 `/vue3-feature-planner` 规划功能
2. 使用 `/vue3-api-model-router-store` 创建模块
3. 使用 `/vue3-mock-builder` 创建 Mock 数据
4. 使用 `/vue3-api-mock-integration` 创建 API 层
5. 使用 `/vue3-page-builder` 创建页面
6. 使用 `/vue3-test-builder` 创建测试

### Step 4: Test and Build
1. 运行 `npm run type-check` 检查类型
2. 运行 `npm run test:unit` 运行测试
3. 运行 `npm run build` 构建生产版本

## Common Tasks

### Add a New Module
```
/vue3-api-model-router-store create <module> with list detail form
```

### Add a New Page
```
/vue3-page-builder create <module> <page-type> page at /path
```

### Add a New Component
```
/vue3-component-builder create <ComponentName> with <features>
```

### Add a New API
```
/vue3-api-mock-integration create <entity> API with CRUD
```

### Add a New Store
```
/vue3-store-builder create use<Module>Store with <state>
```

## Troubleshooting

### Common Issues

1. **Type Errors**
   - 运行 `npm run type-check` 查看详细错误
   - 检查 TypeScript 版本是否正确
   - 确保所有类型定义正确

2. **Build Errors**
   - 清除 `node_modules` 和 `dist` 目录
   - 重新运行 `npm install`
   - 检查 Node.js 版本

3. **Test Failures**
   - 检查测试配置
   - 确保所有依赖正确安装
   - 查看测试输出了解错误详情

## Resources

### Official Documentation
- [Vue 3](https://vuejs.org/)
- [TypeScript](https://www.typescriptlang.org/)
- [Pinia](https://pinia.vuejs.org/)
- [Vue Router](https://router.vuejs.org/)
- [Element Plus](https://element-plus.org/)
- [Vite](https://vitejs.dev/)
- [Vitest](https://vitest.dev/)

### Project Documentation
- Wiki: `public/AutoCodeForge.wiki/`
- Requirements: `docs/requirement.txt`
- Settings: `docs/settings-config-requirements.md`

## Example Prompts
- /vue3-project-onboarding explain project structure
- /vue3-project-onboarding show tech stack and versions
- /vue3-project-onboarding how to create a new module
- /vue3-project-onboarding guide me through first feature development
