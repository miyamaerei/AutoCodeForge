---
name: "build-and-push"
description: "构建前端和后端项目，编译通过后总结代码变更并推送到远程仓库。Invoke when user asks to build and push code, or wants to verify compilation before publishing."
---

# Build and Push

## 功能说明

该技能用于构建前端和后端项目，验证编译是否通过，然后总结代码变更并推送到远程仓库。

## 使用场景

- 用户要求构建并推送代码
- 需要在发布前验证编译是否通过
- 想要自动总结代码变更并推送到远程

## 项目结构

- **前端路径**: `client/` (Vue 3 + Vite)
- **后端路径**: `server/` (.NET)
- **后端解决方案**: `server/AutoCodeForge.slnx`

## 执行步骤

### 1. 检查 Git 状态

- 运行 `git status` 查看当前工作区状态
- 确认哪些文件已修改

### 2. 构建前端项目

- 进入 `client` 目录
- 运行 `npm ci` 安装依赖
- 运行 `npm run type-check` 进行类型检查
- 运行 `npm run build` 构建项目

### 3. 构建后端项目

- 进入 `server` 目录
- 运行 `dotnet restore AutoCodeForge.slnx` 恢复依赖
- 运行 `dotnet build AutoCodeForge.slnx -c Release` 构建项目

### 4. 代码变更总结

- 使用 `git diff` 或 `git log` 查看代码变更
- 生成变更总结，包括：
  - 修改的文件列表
  - 主要变更内容
  - 变更类型（新增/修改/删除）

### 5. 提交并推送代码

- 运行 `git add -A` 添加所有变更
- 运行 `git commit -m "chore: build and push"` 提交变更
- 运行 `git push` 推送到远程仓库

## 成功条件

- 前端类型检查通过（退出码 0）
- 前端构建通过（退出码 0）
- 后端依赖恢复成功（退出码 0）
- 后端构建通过（退出码 0）

## 失败处理

- 如果任何构建步骤失败，立即停止执行
- 输出失败信息和错误日志
- 不进行代码提交和推送

## 示例使用

```
/build-and-push
```
