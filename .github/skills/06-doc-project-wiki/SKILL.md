# 06-doc-project-wiki

## 目标和范围
自动生成 AutoCodeForge 项目的 Wiki 文档，包括项目概览、架构设计、技术栈、安装指南、开发指南、API 文档、部署指南、故障排除、贡献指南和更新日志。基于技能 01（系统知识）提取项目实际信息。

## Wiki 结构模板

标准 Wiki 包含以下 10 个章节：

```
Wiki/
├── 01-项目概览.md          # 项目简介、核心功能、目标用户
├── 02-架构设计.md          # 系统架构、技术选型、模块划分
├── 03-技术栈.md            # 前后端技术栈、版本要求
├── 04-安装指南.md          # 环境准备、依赖安装、配置说明
├── 05-开发指南.md          # 开发流程、代码规范、分支策略
├── 06-API文档.md           # 后端 API 端点、请求响应示例
├── 07-部署指南.md          # 生产部署、环境配置、监控告警
├── 08-故障排除.md          # 常见问题、错误码、调试方法
├── 09-贡献指南.md          # 如何贡献代码、PR 流程、代码审查
└── 10-更新日志.md          # 版本历史、功能变更、已知问题
```

## 项目信息发现流程

### 第一步：调用技能 01 获取系统知识

**提取信息**：
- 项目名称和定位
- 技术栈和版本
- 模块映射表
- 入口点和路由
- 目录结构

### 第二步：扫描关键配置文件

**配置文件列表**：
1. **前端配置**：
   - `client/package.json` - 依赖和脚本
   - `client/vite.config.ts` - 构建配置
   - `client/tsconfig.json` - TypeScript 配置
   - `client/.env.*` - 环境变量（如存在）

2. **后端配置**：
   - `server/AutoCodeForge.sln` - 解决方案结构
   - `server/src/AutoCodeForge.Api/appsettings.json` - 应用配置
   - `server/src/AutoCodeForge.Api/AutoCodeForge.Api.csproj` - 项目依赖

3. **文档文件**：
   - `README.md` - 项目基础信息
   - `docs/` - 现有文档

### 第三步：生成 Wiki 章节内容

对每个章节，基于实际代码生成内容：

---

## Wiki 章节定义

### 章节 1: 项目概览

**必含内容**：
- 项目名称和 Logo（如有）
- 一句话描述（基于技能 01 的系统定位）
- 核心功能列表（基于技能 01 的模块映射）
- 目标用户和使用场景
- 项目状态（开发中/稳定版/Beta）

**模板**：
```markdown
# 项目概览

## 简介
**AutoCodeForge** 是一个 [从技能 01 提取定位]。

## 核心功能
- **Agent 管理**: [描述]
- **任务中心**: [描述]
- **仓库集成**: [描述]
- ...

## 技术亮点
- [亮点 1]
- [亮点 2]

## 项目状态
- **当前版本**: v[从 package.json 提取]
- **状态**: [开发中/Beta/稳定]
```

---

### 章节 2: 架构设计

**必含内容**：
- 系统架构图（基于技能 01 的架构视图）
- 前后端分离架构说明
- 四层架构详解（Api/Application/Core/Infrastructure）
- 模块间通信方式（HTTP API + JWT）
- 数据流向图

**模板**：
```markdown
# 架构设计

## 整体架构
[从技能 01 提取架构图]

## 后端四层架构
- **Api 层**: HTTP 端点、中间件
- **Application 层**: 业务服务、状态机
- **Core 层**: 实体、DTO、接口
- **Infrastructure 层**: 仓储、外部集成

## 前端模块架构
- **模块优先结构**: 每个功能一个独立模块
- **状态管理**: Pinia Store
- **路由管理**: Vue Router with lazy loading

## 通信方式
- **前后端**: HTTP REST API + JWT 认证
- **后端内部**: 服务注入 + 仓储模式
- **前端内部**: Store + Composables
```

---

### 章节 3: 技术栈

**必含内容**：
- 前端技术栈表格（从技能 01 提取）
- 后端技术栈表格（从技能 01 提取）
- 开发工具推荐
- Node.js 和 .NET 版本要求

**模板**：
```markdown
# 技术栈

## 前端技术栈
[从技能 01 复制前端技术栈表格]

## 后端技术栈
[从技能 01 复制后端技术栈表格]

## 开发工具
- **IDE**: Visual Studio Code / Visual Studio 2022
- **数据库工具**: SQLite Browser / Azure Data Studio
- **API 测试**: Swagger UI / Postman
- **Git 客户端**: Git CLI / GitHub Desktop

## 版本要求
- **Node.js**: ^20.19.0 || >=22.12.0
- **.NET**: 10.0
- **npm**: ≥ 8.0
```

---

### 章节 4: 安装指南

**必含内容**：
- 环境准备（Node.js、.NET SDK）
- 依赖安装步骤（前端 + 后端）
- 数据库初始化
- 环境变量配置
- 首次运行验证

**模板**：
```markdown
# 安装指南

## 环境准备

### 必备软件
1. **Node.js**: [从 package.json 提取版本要求]
2. **.NET SDK**: [从 .csproj 提取版本]
3. **Git**: 用于克隆代码

### 可选软件
- **SQLite Browser**: 查看开发数据库
- **Visual Studio Code**: 推荐 IDE

## 安装步骤

### 1. 克隆代码
\`\`\`bash
git clone https://github.com/[org]/AutoCodeForge.git
cd AutoCodeForge
\`\`\`

### 2. 安装前端依赖
\`\`\`bash
cd client
npm install
\`\`\`

### 3. 安装后端依赖
\`\`\`bash
cd ../server
dotnet restore
\`\`\`

### 4. 配置环境变量
复制配置模板：
\`\`\`bash
# 后端配置
cp server/src/AutoCodeForge.Api/appsettings.json server/src/AutoCodeForge.Api/appsettings.Development.json

# 编辑配置文件，设置：
# - Jwt:Key: 至少 32 字符的密钥
# - Encryption:Key: 32 字节的 Base64 密钥
# - Git: Git 平台配置
\`\`\`

### 5. 初始化数据库
\`\`\`bash
cd server/src/AutoCodeForge.Api
dotnet run # 首次运行自动初始化 SQLite 数据库
\`\`\`

### 6. 验证安装
- **后端**: 访问 http://localhost:[端口]/swagger 查看 Swagger UI
- **前端**: 访问 http://localhost:[端口] 查看登录页面

## 常见安装问题
- **问题**: npm install 失败
  - **解决**: 切换 npm 镜像 `npm config set registry https://registry.npmmirror.com`
  
- **问题**: dotnet restore 超时
  - **解决**: 配置 NuGet 镜像 `nuget sources add -name aliyun -source https://mirrors.aliyun.com/nuget/v3/index.json`
```

---

### 章节 5: 开发指南

**必含内容**：
- 开发流程（需求 → 开发 → 测试 → 提交 → 审查）
- 分支策略（main/feature/hotfix）
- 代码规范（引用 PROJECT_SPEC.md）
- 提交规范（Conventional Commits）
- 本地调试方法

**模板**：
```markdown
# 开发指南

## 开发流程
1. **创建功能分支**: `git checkout -b feature/module-name/feature-name`
2. **开发代码**: 遵循代码规范
3. **编写测试**: 单元测试覆盖率 ≥ 80%
4. **本地验证**: 运行测试 + 构建验证
5. **提交代码**: 遵循 Conventional Commits
6. **创建 PR**: 填写 PR 模板，关联 Issue
7. **代码审查**: 通过审查后合并

## 分支策略
- **main**: 稳定分支，受保护
- **feature/**: 功能开发分支
- **hotfix/**: 紧急修复分支

## 代码规范
详见 `docs/PROJECT_SPEC.md`

核心规范：
- **后端**: 所有仓储继承 `BaseRepository<T>`
- **前端**: 所有 Store 使用 `defineStore` setup 格式
- **注释**: 所有公共方法有 XML 注释（C#）或 JSDoc 注释（TypeScript）
- **命名**: 驼峰命名法（camelCase / PascalCase）

## 提交规范
使用 Conventional Commits：
\`\`\`
feat(task-center): 添加任务优先级字段
fix(auth): 修复登录令牌刷新逻辑
docs(wiki): 更新安装指南
test(task): 新增任务优先级单元测试
\`\`\`

## 本地调试

### 前端调试
\`\`\`bash
cd client
npm run dev # 启动开发服务器
\`\`\`
访问 http://localhost:5173（默认端口）

### 后端调试
\`\`\`bash
cd server/src/AutoCodeForge.Api
dotnet run # 或使用 VS Code / Visual Studio 调试
\`\`\`
访问 http://localhost:5000/swagger

### 数据库调试
使用 SQLite Browser 打开 `server/src/AutoCodeForge.Api/autocodeforge.dev.db`
```

---

### 章节 6: API 文档

**必含内容**：
- Swagger UI 地址
- 核心 API 端点列表（从技能 01 提取）
- 认证方式（JWT Bearer Token）
- 请求/响应示例
- 错误码说明

**模板**：
```markdown
# API 文档

## Swagger UI
开发环境: http://localhost:5000/swagger  
生产环境: [待配置]

## 认证
所有 API（除 `/api/auth/login`）需要 JWT Bearer Token：
\`\`\`
Authorization: Bearer <token>
\`\`\`

## 核心 API 端点

### 认证相关
- `POST /api/auth/login` - 用户登录
- `POST /api/auth/refresh` - 刷新令牌
- `POST /api/auth/logout` - 用户登出

### 任务管理
- `GET /api/tasks` - 获取任务列表
- `POST /api/tasks` - 创建任务
- `GET /api/tasks/{id}` - 获取任务详情
- `PUT /api/tasks/{id}` - 更新任务
- `DELETE /api/tasks/{id}` - 删除任务

[从技能 01 的端点列表提取更多端点]

## 请求示例

### 创建任务
\`\`\`http
POST /api/tasks HTTP/1.1
Host: localhost:5000
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "测试任务",
  "description": "测试描述",
  "priority": 2
}
\`\`\`

### 响应示例
\`\`\`json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": 1,
    "title": "测试任务",
    "description": "测试描述",
    "status": "Pending",
    "priority": 2,
    "createdAt": "2026-05-25T10:00:00Z"
  }
}
\`\`\`

## 错误码
| 状态码 | 说明 | 示例 |
|-------|------|------|
| 200 | 成功 | - |
| 400 | 请求参数错误 | 缺少必填字段 |
| 401 | 未认证 | JWT 令牌无效 |
| 403 | 无权限 | 访问其他用户资源 |
| 404 | 资源不存在 | 任务 ID 不存在 |
| 500 | 服务器错误 | 数据库连接失败 |
```

---

### 章节 7: 部署指南

**必含内容**：
- 生产环境要求
- 构建步骤
- 部署方式（Docker / IIS / 静态托管）
- 环境变量配置
- 数据库迁移
- 监控和日志

**模板**：
```markdown
# 部署指南

## 生产环境要求
- **服务器**: Linux / Windows Server
- **Node.js**: [版本要求]
- **.NET Runtime**: 10.0
- **数据库**: SQL Server / PostgreSQL（生产推荐）
- **反向代理**: Nginx / IIS

## 构建步骤

### 前端构建
\`\`\`bash
cd client
npm run build
# 输出到 client/dist/
\`\`\`

### 后端构建
\`\`\`bash
cd server
dotnet publish -c Release -o ./publish
# 输出到 server/publish/
\`\`\`

## 部署方式

### 方式 1: Docker 部署（推荐）
\`\`\`dockerfile
# Dockerfile（待创建）
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY ./server/publish .
ENTRYPOINT ["dotnet", "AutoCodeForge.Api.dll"]
\`\`\`

### 方式 2: IIS 部署
1. 安装 .NET 10.0 Hosting Bundle
2. 将 `server/publish/` 部署到 IIS 站点
3. 将 `client/dist/` 部署到静态文件服务器

### 方式 3: Nginx + Kestrel
\`\`\`nginx
server {
    listen 80;
    server_name your-domain.com;
    
    # 前端静态文件
    location / {
        root /var/www/autocodeforge/client/dist;
        try_files $uri $uri/ /index.html;
    }
    
    # 后端 API 代理
    location /api/ {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
\`\`\`

## 环境变量配置
\`\`\`bash
# 生产环境必须配置：
export JWT_KEY="至少_32_字符的高强度密钥"
export ENCRYPTION_KEY="32字节Base64密钥"
export ConnectionStrings__DefaultConnection="Server=...;Database=...;"
\`\`\`

## 数据库迁移
生产环境首次部署时，数据库自动初始化（`DatabaseInitializer`）。  
后续版本升级需运行迁移脚本（待创建）。

## 监控和日志
- **日志位置**: `logs/` 目录（待配置）
- **监控工具**: Application Insights / Prometheus（待集成）
```

---

### 章节 8: 故障排除

**必含内容**：
- 常见错误及解决方案
- 日志查看方法
- 性能诊断
- 数据库连接问题
- 认证问题

**模板**：
```markdown
# 故障排除

## 常见错误

### 错误: "JWT 令牌无效"
**原因**: JWT 密钥不一致或令牌过期  
**解决**:
1. 检查 `appsettings.json` 中的 `Jwt:Key`
2. 检查环境变量 `JWT_KEY`
3. 重新登录获取新令牌

### 错误: "数据库连接失败"
**原因**: 数据库服务未启动或连接字符串错误  
**解决**:
1. 检查 SQLite 文件是否存在（开发环境）
2. 检查 SQL Server 连接字符串（生产环境）
3. 查看日志: `server/logs/app.log`

### 错误: "前端无法调用后端 API"
**原因**: CORS 配置或后端未启动  
**解决**:
1. 检查后端是否运行: http://localhost:5000/swagger
2. 检查 `appsettings.json` 中的 `Cors:AllowedOrigins`
3. 检查前端 Axios baseURL 配置

## 日志查看
\`\`\`bash
# 后端日志
tail -f server/logs/app.log

# 前端浏览器控制台
F12 → Console
\`\`\`

## 性能诊断
- **慢查询**: 检查 SqlSugar 日志，添加索引
- **内存泄漏**: 使用 dotnet-counters 监控
- **前端性能**: Chrome DevTools → Performance
```

---

### 章节 9: 贡献指南

**必含内容**：
- 如何贡献代码
- PR 流程
- 代码审查标准
- Issue 模板
- 社区行为准则

**模板**：
```markdown
# 贡献指南

## 如何贡献
1. Fork 本仓库
2. 创建功能分支: `git checkout -b feature/my-feature`
3. 提交代码: `git commit -m "feat: add my feature"`
4. 推送分支: `git push origin feature/my-feature`
5. 创建 Pull Request

## PR 检查清单
- [ ] 代码符合 PROJECT_SPEC.md 规范
- [ ] 所有测试通过
- [ ] 添加了单元测试（如适用）
- [ ] 更新了文档（如需要）
- [ ] 提交信息遵循 Conventional Commits

## 代码审查标准
- 架构层次正确
- 正确复用基类和组件
- 前后端契约一致
- 测试覆盖率 ≥ 80%
- 文档同步更新

## Issue 模板
提交 Issue 时，请包含：
- **问题描述**: 清晰描述问题
- **复现步骤**: 如何触发问题
- **预期行为**: 期望的正确行为
- **实际行为**: 当前的错误行为
- **环境信息**: Node.js 版本、.NET 版本、操作系统
```

---

### 章节 10: 更新日志

**必含内容**：
- 版本号和发布日期
- 功能变更（新增/修改/删除）
- Bug 修复
- 已知问题
- 升级指南

**模板**：
```markdown
# 更新日志

## [Unreleased]

### 新增
- [从 MasterPlan.md 提取近期完成任务]

### 修改
- [列出修改项]

### 修复
- [列出修复的 Bug]

## [0.1.0] - 2026-05-25

### 新增
- 阶段一: 基础设施初始化（四层架构）
- 阶段二: 数据层与认证系统（16 实体 + JWT）
- 阶段三: AI 核心模块（LLM 网关 + Agent）
- 阶段四: 任务中心模块
- 阶段五: 定时任务调度
- 阶段六: Git 仓库集成
- 阶段七: 流水线管理

### 已知问题
- [从 MasterPlan.md 提取待收敛项]

## 升级指南
### 从 0.0.x 升级到 0.1.0
1. 备份数据库
2. 运行数据库迁移
3. 更新环境变量配置
4. 重启服务
```

---

## 内容生成规则

### 规则 1: 基于实际代码
所有内容必须基于代码仓库的实际文件生成，禁止虚构：
- ✅ 从 `package.json` 提取版本号
- ✅ 从 `appsettings.json` 提取配置项
- ✅ 从技能 01 提取架构信息
- ❌ 不要编造不存在的功能

### 规则 2: 保持更新
Wiki 应定期更新，特别是：
- 技术栈版本变更时
- 新增模块时
- 部署方式变更时
- API 契约变更时

### 规则 3: 用户友好
- 提供清晰的步骤说明
- 包含代码示例
- 列出常见问题和解决方案
- 使用截图（如适用）

## 资产链接策略

**图片和附件**：
- 放置在 `docs/assets/` 目录
- 使用相对路径引用: `![架构图](./assets/architecture.png)`
- 图片命名清晰: `frontend-module-structure.png`

**代码示例**：
- 使用代码块语法高亮
- 标注语言类型: ```typescript, ```csharp
- 提供完整可运行的示例

## 输出格式

生成的 Wiki 文件保存在 `docs/wiki/` 目录，文件命名格式：
```
01-项目概览.md
02-架构设计.md
03-技术栈.md
04-安装指南.md
05-开发指南.md
06-API文档.md
07-部署指南.md
08-故障排除.md
09-贡献指南.md
10-更新日志.md
```

**生成命令**（如有）:
```bash
# 使用 Node.js 脚本生成 Wiki
node tools/generate-wiki.mjs
```

---

## 质量检查清单

使用本技能时，必须确保：

- [ ] 已调用技能 01 获取系统知识
- [ ] 所有章节内容完整
- [ ] 版本号从实际配置文件提取
- [ ] 代码示例可运行
- [ ] 文件路径正确
- [ ] 无虚构内容
- [ ] 图片链接有效
- [ ] 语法正确（Markdown）

---

## 更新历史

| 日期 | 版本 | 变更说明 |
|------|------|----------|
| 2026-05-25 | 1.0.0 | 初始版本，定义 10 章节 Wiki 结构 |

---

**维护说明**: 本技能依赖技能 01（系统知识）。当项目架构、技术栈或部署方式变更时，需重新生成 Wiki 文档。
