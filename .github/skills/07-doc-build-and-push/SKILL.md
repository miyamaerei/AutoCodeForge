# 07-doc-build-and-push

## 目标和范围
自动识别 AutoCodeForge 项目类型，生成构建命令、推送脚本和 CI/CD 配置。支持前端（Vue + Vite）和后端（.NET）的构建、测试、容器化和部署。

## 项目类型识别规则

### 识别流程

1. **扫描根目录和子目录**：
   - 检查 `client/` 和 `server/` 目录是否存在
   - 检查关键配置文件

2. **识别项目类型**：

| 配置文件 | 项目类型 | 构建工具 | 证据文件 |
|---------|---------|---------|---------|
| `client/package.json` + `client/vite.config.ts` | Vue 3 + Vite | npm + vite | `client/package.json#scripts.build` |
| `server/AutoCodeForge.sln` + `*.csproj` | .NET Web API | MSBuild / dotnet CLI | `server/AutoCodeForge.sln` |
| `Dockerfile` | 容器化应用 | Docker | `Dockerfile` |

## 入口点发现流程

### 前端入口点

**识别方法**：
1. 检查 `client/package.json` 中的 `scripts.dev` 和 `scripts.build`
2. 检查 `client/vite.config.ts` 或 `client/webpack.config.js`
3. 检查 `client/src/main.ts` 或 `client/src/main.js`

**AutoCodeForge 前端入口**：
- **开发脚本**: `npm run dev`（Vite 开发服务器）
- **构建脚本**: `npm run build`（生产构建）
- **预览脚本**: `npm run preview`（预览生产构建）
- **入口文件**: `client/src/main.ts`

**证据**：`client/package.json#L6-L11`

---

### 后端入口点

**识别方法**：
1. 检查 `*.sln` 解决方案文件
2. 检查 `*Api.csproj` 或 `*Web.csproj` 项目文件
3. 检查 `Program.cs` 入口文件

**AutoCodeForge 后端入口**：
- **解决方案**: `server/AutoCodeForge.sln`
- **API 项目**: `server/src/AutoCodeForge.Api/AutoCodeForge.Api.csproj`
- **入口文件**: `server/src/AutoCodeForge.Api/Program.cs`

**证据**：`server/AutoCodeForge.sln#L6-L18`

---

## 构建工具检测

### 前端构建工具

| 工具 | 检测文件 | 构建命令 | 输出目录 |
|------|---------|---------|---------|
| **Vite** | `vite.config.ts` | `npm run build` | `dist/` |
| **Webpack** | `webpack.config.js` | `npm run build` | `dist/` |
| **Parcel** | `package.json` (parcel 依赖) | `npm run build` | `dist/` |

**AutoCodeForge 前端构建工具**: Vite 8.0.8  
**证据**: `client/vite.config.ts` + `client/package.json#L41`

---

### 后端构建工具

| 工具 | 检测文件 | 构建命令 | 输出目录 |
|------|---------|---------|---------|
| **dotnet CLI** | `*.csproj` | `dotnet build` / `dotnet publish` | `bin/` 或自定义 |
| **MSBuild** | `*.sln` | `msbuild` | `bin/` |

**AutoCodeForge 后端构建工具**: dotnet CLI  
**证据**: `server/AutoCodeForge.sln` + `server/src/AutoCodeForge.Api/AutoCodeForge.Api.csproj`

---

## 构建命令模板

### 前端构建命令

#### 开发环境
```bash
# 进入前端目录
cd client

# 安装依赖
npm install

# 启动开发服务器
npm run dev
# 或
npm run dev -- --host 0.0.0.0 --port 3000
```

#### 生产构建
```bash
# 类型检查
npm run type-check

# 构建生产版本
npm run build

# 输出目录: client/dist/
# 包含: index.html, assets/, ...
```

#### 构建验证
```bash
# 预览生产构建
npm run preview

# 或使用静态服务器
npx serve dist
```

---

### 后端构建命令

#### 开发环境
```bash
# 进入后端目录
cd server

# 恢复 NuGet 包
dotnet restore

# 运行开发服务器
cd src/AutoCodeForge.Api
dotnet run

# 或使用 watch 模式
dotnet watch run
```

#### 生产构建
```bash
# 构建 Release 配置
dotnet build -c Release

# 发布（包含运行时）
dotnet publish -c Release -o ./publish

# 输出目录: server/publish/
# 包含: AutoCodeForge.Api.dll, appsettings.json, ...
```

#### 测试
```bash
# 运行所有测试
dotnet test

# 运行测试并生成覆盖率报告
dotnet test --collect:"XPlat Code Coverage"
```

---

## 容器化配置（Dockerfile 生成规则）

### 后端 Dockerfile

**模板**（多阶段构建）:
```dockerfile
# 阶段 1: 构建
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制解决方案和项目文件
COPY server/AutoCodeForge.sln ./
COPY server/src/AutoCodeForge.Api/AutoCodeForge.Api.csproj ./src/AutoCodeForge.Api/
COPY server/src/AutoCodeForge.Application/AutoCodeForge.Application.csproj ./src/AutoCodeForge.Application/
COPY server/src/AutoCodeForge.Core/AutoCodeForge.Core.csproj ./src/AutoCodeForge.Core/
COPY server/src/AutoCodeForge.Infrastructure/AutoCodeForge.Infrastructure.csproj ./src/AutoCodeForge.Infrastructure/

# 恢复依赖
RUN dotnet restore

# 复制所有源代码
COPY server/ ./

# 构建和发布
WORKDIR /src/src/AutoCodeForge.Api
RUN dotnet publish -c Release -o /app/publish

# 阶段 2: 运行时
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# 复制构建产物
COPY --from=build /app/publish .

# 暴露端口
EXPOSE 80
EXPOSE 443

# 设置入口点
ENTRYPOINT ["dotnet", "AutoCodeForge.Api.dll"]
```

**构建和运行**:
```bash
# 构建镜像
docker build -t autocodeforge-api:latest -f server/Dockerfile .

# 运行容器
docker run -d -p 5000:80 \
  -e JWT_KEY="your-secret-key" \
  -e ENCRYPTION_KEY="your-encryption-key" \
  --name autocodeforge-api \
  autocodeforge-api:latest
```

---

### 前端 Dockerfile

**模板**（多阶段构建）:
```dockerfile
# 阶段 1: 构建
FROM node:20-alpine AS build
WORKDIR /app

# 复制 package.json 和 lock 文件
COPY client/package*.json ./

# 安装依赖
RUN npm ci

# 复制源代码
COPY client/ ./

# 构建生产版本
RUN npm run build

# 阶段 2: Nginx 静态服务
FROM nginx:alpine AS runtime
WORKDIR /usr/share/nginx/html

# 删除默认文件
RUN rm -rf ./*

# 复制构建产物
COPY --from=build /app/dist ./

# 复制 Nginx 配置
COPY client/nginx.conf /etc/nginx/conf.d/default.conf

# 暴露端口
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
```

**Nginx 配置** (`client/nginx.conf`):
```nginx
server {
    listen 80;
    server_name _;
    root /usr/share/nginx/html;
    index index.html;

    # SPA 路由处理
    location / {
        try_files $uri $uri/ /index.html;
    }

    # API 代理
    location /api/ {
        proxy_pass http://backend:80;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }

    # 静态资源缓存
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
```

---

## CI/CD 集成模板

### GitHub Actions

**文件**: `.github/workflows/ci.yml`

```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  # 前端构建和测试
  frontend:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: client/package-lock.json

      - name: Install dependencies
        working-directory: client
        run: npm ci

      - name: Type check
        working-directory: client
        run: npm run type-check

      - name: Run tests
        working-directory: client
        run: npm run test:unit

      - name: Build
        working-directory: client
        run: npm run build

      - name: Upload build artifacts
        uses: actions/upload-artifact@v4
        with:
          name: frontend-dist
          path: client/dist/

  # 后端构建和测试
  backend:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Restore dependencies
        working-directory: server
        run: dotnet restore

      - name: Build
        working-directory: server
        run: dotnet build -c Release --no-restore

      - name: Run tests
        working-directory: server
        run: dotnet test --no-build --verbosity normal

      - name: Publish
        working-directory: server
        run: dotnet publish -c Release -o ./publish

      - name: Upload build artifacts
        uses: actions/upload-artifact@v4
        with:
          name: backend-publish
          path: server/publish/

  # Docker 镜像构建
  docker:
    needs: [frontend, backend]
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Login to Docker Hub
        uses: docker/login-action@v3
        with:
          username: ${{ secrets.DOCKER_USERNAME }}
          password: ${{ secrets.DOCKER_PASSWORD }}

      - name: Build and push backend
        uses: docker/build-push-action@v5
        with:
          context: .
          file: server/Dockerfile
          push: true
          tags: ${{ secrets.DOCKER_USERNAME }}/autocodeforge-api:latest

      - name: Build and push frontend
        uses: docker/build-push-action@v5
        with:
          context: .
          file: client/Dockerfile
          push: true
          tags: ${{ secrets.DOCKER_USERNAME }}/autocodeforge-client:latest
```

---

### Azure DevOps Pipeline

**文件**: `azure-pipelines.yml`

```yaml
trigger:
  branches:
    include:
      - main
      - develop

pool:
  vmImage: 'ubuntu-latest'

stages:
  - stage: Build
    jobs:
      # 前端构建
      - job: Frontend
        steps:
          - task: NodeTool@0
            inputs:
              versionSpec: '20.x'

          - script: |
              cd client
              npm ci
              npm run type-check
              npm run build
            displayName: 'Build Frontend'

          - task: PublishBuildArtifacts@1
            inputs:
              PathtoPublish: 'client/dist'
              ArtifactName: 'frontend-dist'

      # 后端构建
      - job: Backend
        steps:
          - task: UseDotNet@2
            inputs:
              version: '10.0.x'

          - script: |
              cd server
              dotnet restore
              dotnet build -c Release
              dotnet test
              dotnet publish -c Release -o ./publish
            displayName: 'Build Backend'

          - task: PublishBuildArtifacts@1
            inputs:
              PathtoPublish: 'server/publish'
              ArtifactName: 'backend-publish'

  - stage: Deploy
    dependsOn: Build
    condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
    jobs:
      - job: Deploy
        steps:
          - download: current
            artifact: frontend-dist

          - download: current
            artifact: backend-publish

          - task: Docker@2
            displayName: 'Build and Push Docker Images'
            inputs:
              command: buildAndPush
              repository: 'autocodeforge'
              tags: |
                $(Build.BuildId)
                latest
```

---

## 推送目标配置

### Docker 注册表

**Docker Hub**:
```bash
# 登录
docker login

# 推送
docker push username/autocodeforge-api:latest
docker push username/autocodeforge-client:latest
```

**Azure Container Registry**:
```bash
# 登录
az acr login --name myregistry

# 推送
docker tag autocodeforge-api myregistry.azurecr.io/autocodeforge-api:latest
docker push myregistry.azurecr.io/autocodeforge-api:latest
```

---

### 包管理器

**npm（前端库发布）**:
```bash
# 登录 npm
npm login

# 发布包
cd client
npm publish
```

**NuGet（后端库发布）**:
```bash
# 打包
dotnet pack -c Release -o ./nupkg

# 推送
dotnet nuget push ./nupkg/AutoCodeForge.Core.1.0.0.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

---

### 部署目标

**Azure App Service**:
```bash
# 部署前端（静态 Web 应用）
az webapp deployment source config-zip \
  --resource-group myResourceGroup \
  --name myFrontendApp \
  --src client/dist.zip

# 部署后端（App Service）
az webapp deployment source config-zip \
  --resource-group myResourceGroup \
  --name myBackendApp \
  --src server/publish.zip
```

**Kubernetes**:
```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: autocodeforge-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: autocodeforge-api
  template:
    metadata:
      labels:
        app: autocodeforge-api
    spec:
      containers:
      - name: api
        image: myregistry.azurecr.io/autocodeforge-api:latest
        ports:
        - containerPort: 80
        env:
        - name: JWT_KEY
          valueFrom:
            secretKeyRef:
              name: autocodeforge-secrets
              key: jwt-key
---
apiVersion: v1
kind: Service
metadata:
  name: autocodeforge-api
spec:
  selector:
    app: autocodeforge-api
  ports:
  - port: 80
    targetPort: 80
  type: LoadBalancer
```

---

## 输出工件清单

### 前端构建产物

**目录**: `client/dist/`

**包含文件**:
- `index.html` - 入口 HTML
- `assets/` - 打包后的 JS/CSS/图片
  - `index-[hash].js`
  - `index-[hash].css`
  - `vendor-[hash].js`
- `web.config` - IIS 部署配置（如有）

**验证**:
```bash
# 检查文件大小
ls -lh client/dist/

# 预览
npx serve client/dist
```

---

### 后端构建产物

**目录**: `server/publish/`

**包含文件**:
- `AutoCodeForge.Api.dll` - 主程序集
- `AutoCodeForge.*.dll` - 依赖程序集
- `appsettings.json` - 配置文件
- `appsettings.Production.json` - 生产配置（如有）
- `wwwroot/` - 静态文件（如有）
- `web.config` - IIS 部署配置（如有）

**验证**:
```bash
# 检查程序集
ls -lh server/publish/*.dll

# 本地运行
cd server/publish
dotnet AutoCodeForge.Api.dll
```

---

## 质量检查清单

使用本技能时，必须确保：

- [ ] 已识别项目类型（前端 + 后端）
- [ ] 已识别入口点（main.ts + Program.cs）
- [ ] 已识别构建工具（Vite + dotnet CLI）
- [ ] 构建命令可执行（npm run build + dotnet build）
- [ ] 生成的 Dockerfile 可构建成功
- [ ] CI/CD 配置文件语法正确
- [ ] 环境变量配置完整
- [ ] 输出工件清单准确

---

## 更新历史

| 日期 | 版本 | 变更说明 |
|------|------|----------|
| 2026-05-25 | 1.0.0 | 初始版本，支持 Vue 3 + Vite 和 .NET 10.0 |

---

**维护说明**: 本技能基于 AutoCodeForge 实际项目结构生成。当构建工具、部署方式或 CI/CD 平台变更时，需更新对应模板。
