# 构建阶段
FROM node:20-alpine AS build
WORKDIR /app

# 复制依赖文件
COPY package*.json ./

# 安装依赖
RUN npm ci --only=production

# 复制源代码
COPY . .

# 构建生产版本
RUN npm run build

# 运行时阶段
FROM nginx:alpine AS runtime
WORKDIR /usr/share/nginx/html

# 删除默认文件
RUN rm -rf ./*

# 复制构建产物
COPY --from=build /app/dist .

# 复制 Nginx 配置
COPY nginx.conf /etc/nginx/conf.d/default.conf

# 暴露端口
EXPOSE 80

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD wget -q --spider http://localhost || exit 1

# 启动 Nginx
CMD ["nginx", "-g", "daemon off;"]
