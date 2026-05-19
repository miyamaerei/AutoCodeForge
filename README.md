# AutoCodeForge

AutoCodeForge 是一个基于 Vue 3 的前端控制台应用，采用前后端分离架构。

## 项目结构

```
AutoCodeForge/
├── client/                     # 前端应用 (Vue 3 + Vite)
│   ├── src/                    # 前端源代码
│   │   ├── assets/             # 静态资源
│   │   ├── components/         # 共享组件
│   │   ├── composables/         # 组合式函数
│   │   ├── config/             # 配置文件
│   │   ├── modules/            # 功能模块
│   │   ├── router/             # 路由配置
│   │   ├── stores/             # 全局 Store
│   │   └── mock/               # Mock 数据
│   ├── public/                  # 静态资源
│   ├── package.json
│   └── vite.config.ts
│
├── server/                     # 后端服务 (Express + TypeScript)
│   ├── src/                    # 后端源代码
│   │   ├── controllers/        # 控制器
│   │   ├── services/           # 业务逻辑
│   │   ├── routes/             # 路由定义
│   │   ├── models/             # 数据模型
│   │   ├── middleware/         # 中间件
│   │   └── index.ts            # 入口文件
│   └── package.json
│
├── docs/                       # 项目文档
└── tools/                      # 开发工具
```

## 技术栈

### 前端 (client/)
- **框架**: Vue 3.5 + Composition API
- **构建工具**: Vite 8
- **路由**: Vue Router 5
- **状态管理**: Pinia 3
- **UI 框架**: Element Plus 2.14
- **表单验证**: VeeValidate + Zod
- **HTTP 客户端**: Axios
- **图表**: ECharts + Vue-ECharts
- **类型检查**: TypeScript 6 + vue-tsc
- **测试**: Vitest

### 后端 (server/)
- **运行时**: Node.js + Express
- **语言**: TypeScript
- **数据验证**: Zod

## 开发环境

- **Node.js**: ^20.19.0 || >=22.12.0
- **包管理**: npm

## 快速开始

### 前端开发

```sh
# 进入 client 目录
cd client

# 安装依赖
npm install

# 启动开发服务器
npm run dev
```

### 后端开发

```sh
# 进入 server 目录
cd server

# 安装依赖
npm install

# 启动开发服务器
npm run dev
```

## 常用命令

### 前端 (client/)

```sh
npm run dev          # 启动开发服务器
npm run build        # 构建生产版本
npm run preview      # 预览生产构建
npm run test:unit     # 运行单元测试
npm run type-check    # TypeScript 类型检查
```

### 后端 (server/)

```sh
npm run dev          # 启动开发服务器 (tsx watch)
npm run build        # 构建
npm run start        # 生产环境启动
npm run test         # 运行测试
```

## 推荐 IDE 配置

- **IDE**: VS Code / Trae IDE
- **扩展**: Vue.volar, Vitest Explorer
- **浏览器**: Chrome/Edge + Vue.js devtools

## 模块架构

项目采用模块化架构，每个功能模块位于 `client/src/modules/<module-name>/` 目录下，包含：

- `views/` - 视图组件
- `store/` - Pinia Store
- `composables/` - 组合式函数
- `<module>.api.ts` - API 层
- `routes.ts` - 路由配置
- `index.ts` - 模块导出

详见 [模块架构文档](docs/module-architecture.md)。
