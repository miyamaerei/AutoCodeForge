# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 复制项目文件
COPY ["src/AutoCodeForge.Api/AutoCodeForge.Api.csproj", "src/AutoCodeForge.Api/"]
COPY ["src/AutoCodeForge.Application/AutoCodeForge.Application.csproj", "src/AutoCodeForge.Application/"]
COPY ["src/AutoCodeForge.Core/AutoCodeForge.Core.csproj", "src/AutoCodeForge.Core/"]
COPY ["src/AutoCodeForge.Infrastructure/AutoCodeForge.Infrastructure.csproj", "src/AutoCodeForge.Infrastructure/"]

# 恢复依赖
RUN dotnet restore "src/AutoCodeForge.Api/AutoCodeForge.Api.csproj"

# 复制所有源代码
COPY . .

# 构建和发布
WORKDIR "/src/src/AutoCodeForge.Api"
RUN dotnet publish -c Release -o /app/publish --no-restore

# 运行时阶段
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# 安装 curl 用于健康检查
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# 复制构建产物
COPY --from=build /app/publish .

# 创建数据目录
RUN mkdir -p /app/data

# 暴露端口
EXPOSE 80
EXPOSE 443

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
  CMD curl -f http://localhost/api/health || exit 1

# 设置环境变量
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# 启动应用
ENTRYPOINT ["dotnet", "AutoCodeForge.Api.dll"]
