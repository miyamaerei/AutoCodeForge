# AutoCodeForge 后端优先级分析

## 1. 概述

从后端视角分析 AutoCodeForge 系统的技术实现优先级，重点关注核心业务流程、数据模型、API 设计和基础设施需求。

## 2. 系统架构分析

### 2.1 模块依赖关系

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              前端层 (Vue 3)                                  │
│  Console  │  TaskCenter  │  AgentCenter  │  ScheduledTask  │  Dashboard   │
└───────────┼──────────────┼───────────────┼─────────────────┼──────────────┘
            ↓              ↓               ↓                 ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│                              API 网关层                                      │
│  REST API  │  WebSocket  │  GraphQL (可选)                                 │
└────────────┼─────────────┼────────────────────────────────────────────────┘
             ↓             ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│                              后端服务层                                      │
│  TaskService  │  ChatService  │  AgentService  │  ScheduleService  │  RepoService │
└───────────────┼───────────────┼────────────────┼───────────────────┼─────────────┘
                ↓               ↓                 ↓                    ↓
┌─────────────────────────────────────────────────────────────────────────────┐
│                              数据存储层                                      │
│  MySQL      │  Redis         │  MongoDB        │  File Storage       │
│  (业务数据)  │  (缓存/会话)    │  (日志/文档)     │  (代码/Diff)        │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 核心业务流程

| 流程 | 复杂度 | 优先级 |
|------|--------|--------|
| 任务创建与执行 | 高 | P0 |
| AI聊天交互 | 高 | P0 |
| Agent管理与自动选择 | 高 | P0 |
| 定时任务调度 | 高 | P1 |
| 仓库数据同步 | 中 | P1 |
| 流水线构建集成 | 中 | P1 |
| 系统配置管理 | 低 | P2 |

---

## 3. 优先级分析

### 3.1 P0 - 核心功能（立即开发）

#### 3.1.1 任务服务 (Task Service)

**优先级**: 最高

**原因**:
- 任务中心是系统核心功能
- 用户直接通过任务与AI交互
- 所有其他功能围绕任务展开

**核心需求**:

| 功能 | 说明 | 状态 |
|------|------|------|
| 任务CRUD | 创建、查询、更新、删除任务 | 必须 |
| 任务状态流转 | 运行中→完成/失败/暂停 | 必须 |
| 任务步骤管理 | 记录执行步骤 | 必须 |
| 任务日志 | 记录执行日志 | 必须 |
| 任务Diff | 记录代码变更 | 必须 |
| 任务聊天 | 任务相关对话 | 必须 |

**数据模型**:

```sql
-- 任务表
CREATE TABLE tasks (
    id VARCHAR(64) PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    repository VARCHAR(255),
    branch VARCHAR(100),
    state ENUM('running', 'completed', 'paused', 'failed') DEFAULT 'running',
    created_at DATETIME NOT NULL,
    updated_at DATETIME NOT NULL,
    completed_at DATETIME
);

-- 任务步骤表
CREATE TABLE task_steps (
    id VARCHAR(64) PRIMARY KEY,
    task_id VARCHAR(64) NOT NULL,
    title VARCHAR(255) NOT NULL,
    order_num INT NOT NULL,
    status ENUM('pending', 'running', 'completed', 'failed') DEFAULT 'pending',
    error_message TEXT,
    created_at DATETIME NOT NULL
);

-- 任务日志表
CREATE TABLE task_logs (
    id VARCHAR(64) PRIMARY KEY,
    task_id VARCHAR(64) NOT NULL,
    level ENUM('info', 'warning', 'error', 'debug') DEFAULT 'info',
    message TEXT NOT NULL,
    timestamp DATETIME NOT NULL
);
```

**API 端点**:

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/task-center/tasks | 获取任务列表 |
| GET | /api/task-center/tasks/:id | 获取任务详情 |
| POST | /api/task-center/tasks | 创建任务 |
| PUT | /api/task-center/tasks/:id | 更新任务 |
| DELETE | /api/task-center/tasks/:id | 删除任务 |
| GET | /api/task-center/tasks/:id/logs | 获取任务日志 |
| GET | /api/task-center/tasks/:id/diff | 获取任务Diff |

---

#### 3.1.2 聊天服务 (Chat Service)

**优先级**: 最高

**原因**:
- 聊天是用户与AI交互的核心方式
- 支持多轮对话和上下文管理
- 任务执行过程需要聊天支持

**核心需求**:

| 功能 | 说明 | 状态 |
|------|------|------|
| 消息发送/接收 | 核心聊天功能 | 必须 |
| 会话管理 | 创建、选择、删除会话 | 必须 |
| 上下文保持 | 多轮对话上下文 | 必须 |
| 消息持久化 | 历史消息存储 | 必须 |

**数据模型**:

```sql
-- 会话表
CREATE TABLE chat_sessions (
    id VARCHAR(64) PRIMARY KEY,
    title VARCHAR(255),
    created_at DATETIME NOT NULL,
    updated_at DATETIME NOT NULL
);

-- 消息表
CREATE TABLE chat_messages (
    id VARCHAR(64) PRIMARY KEY,
    session_id VARCHAR(64),
    task_id VARCHAR(64),  -- 关联任务（可选）
    type ENUM('user', 'ai') NOT NULL,
    content TEXT NOT NULL,
    timestamp DATETIME NOT NULL
);
```

**API 端点**:

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/chat/sessions | 获取会话列表 |
| POST | /api/chat/sessions | 创建会话 |
| DELETE | /api/chat/sessions/:id | 删除会话 |
| GET | /api/chat/sessions/:id/messages | 获取会话消息 |
| POST | /api/chat/sessions/:id/messages | 发送消息 |
| POST | /api/chat/ask | 单次提问（无会话） |

---

#### 3.1.3 Agent 服务 (Agent Service)

**优先级**: 最高

**原因**:
- Agent 是聊天自动选择的核心
- 支持手动引用特定 Agent
- 任务模板的基础

**核心需求**:

| 功能 | 说明 | 状态 |
|------|------|------|
| Agent CRUD | 创建、查询、更新、删除 Agent | 必须 |
| Agent 关键词配置 | 配置自动选择关键词和权重 | 必须 |
| 自动选择算法 | 根据聊天内容匹配最合适的 Agent | 必须 |
| Agent 引用 | 聊天时手动指定 Agent | 必须 |
| Agent 启用/禁用 | 控制 Agent 是否可用 | 必须 |

**数据模型**:

```sql
-- Agent 表
CREATE TABLE agents (
    id VARCHAR(64) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    icon VARCHAR(50) DEFAULT 'assistant',
    system_prompt TEXT NOT NULL,
    enabled BOOLEAN DEFAULT TRUE,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NOT NULL
);

-- Agent 关键词表
CREATE TABLE agent_keywords (
    id VARCHAR(64) PRIMARY KEY,
    agent_id VARCHAR(64) NOT NULL,
    keyword VARCHAR(100) NOT NULL,
    weight DECIMAL(3,2) DEFAULT 1.00,
    created_at DATETIME NOT NULL,
    INDEX idx_agent_keyword (agent_id, keyword)
);
```

**API 端点**:

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/agent | 获取 Agent 列表 |
| GET | /api/agent/:id | 获取 Agent 详情 |
| POST | /api/agent | 创建 Agent |
| PUT | /api/agent/:id | 更新 Agent |
| DELETE | /api/agent/:id | 删除 Agent |
| POST | /api/agent/:id/keywords | 添加关键词 |
| DELETE | /api/agent/:id/keywords/:kid | 删除关键词 |
| POST | /api/agent/auto-select | 根据内容自动选择 Agent |

**自动选择算法**:

```python
def auto_select_agent(message: str, agents: List[Agent]) -> Optional[Agent]:
    """
    根据聊天内容自动选择最合适的 Agent
    :param message: 用户消息
    :param agents: 可用的 Agent 列表
    :return: 匹配的 Agent 或 None
    """
    scores = {}
    for agent in agents:
        if not agent.enabled:
            continue
        score = 0.0
        for kw in agent.keywords:
            if kw.keyword.lower() in message.lower():
                score += kw.weight
        if score > 0.5:  # 阈值
            scores[agent.id] = score

    if not scores:
        return None
    return max(scores, key=scores.get)
```

---

### 3.2 P1 - 重要功能（次优先级）

#### 3.2.1 仓库服务 (Repository Service)

**优先级**: 高

**原因**:
- 任务需要关联代码仓库
- 代码变更需要仓库支持

**核心需求**:

| 功能 | 说明 | 状态 |
|------|------|------|
| 仓库列表 | 获取用户仓库 | 必须 |
| 分支管理 | 获取分支信息 | 必须 |
| PR管理 | 获取Pull Request | 建议 |
| 文件树 | 获取仓库文件结构 | 建议 |

**数据模型**:

```sql
-- 仓库表
CREATE TABLE repositories (
    id VARCHAR(64) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    url VARCHAR(512) NOT NULL,
    provider ENUM('github', 'gitlab', 'gitee') DEFAULT 'github',
    created_at DATETIME NOT NULL
);

-- 分支表
CREATE TABLE branches (
    id VARCHAR(64) PRIMARY KEY,
    repo_id VARCHAR(64) NOT NULL,
    name VARCHAR(255) NOT NULL,
    last_commit VARCHAR(64),
    commit_date DATETIME
);

-- PR表
CREATE TABLE pull_requests (
    id VARCHAR(64) PRIMARY KEY,
    repo_id VARCHAR(64) NOT NULL,
    title VARCHAR(255) NOT NULL,
    state ENUM('open', 'closed', 'merged') DEFAULT 'open',
    author VARCHAR(255) NOT NULL,
    created_at DATETIME NOT NULL
);
```

**API 端点**:

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/repositories | 获取仓库列表 |
| GET | /api/repositories/:id/branches | 获取分支列表 |
| GET | /api/repositories/:id/prs | 获取PR列表 |

---

#### 3.2.2 流水线服务 (Pipeline Service)

**优先级**: 高

**原因**:
- 任务执行需要CI/CD支持
- 构建状态是任务结果的重要反馈

**核心需求**:

| 功能 | 说明 | 状态 |
|------|------|------|
| 流水线列表 | 获取流水线配置 | 必须 |
| 构建状态 | 获取构建结果 | 必须 |
| 构建日志 | 获取构建日志 | 建议 |

**数据模型**:

```sql
-- 流水线表
CREATE TABLE pipelines (
    id VARCHAR(64) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    repo_id VARCHAR(64) NOT NULL,
    status ENUM('running', 'success', 'failed', 'pending') DEFAULT 'pending',
    last_build_time DATETIME
);

-- 构建表
CREATE TABLE builds (
    id VARCHAR(64) PRIMARY KEY,
    pipeline_id VARCHAR(64) NOT NULL,
    status ENUM('running', 'success', 'failed') DEFAULT 'running',
    start_time DATETIME NOT NULL,
    end_time DATETIME,
    duration INT  -- 毫秒
);
```

**API 端点**:

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/pipelines | 获取流水线列表 |
| GET | /api/pipelines/:id/builds | 获取构建历史 |
| GET | /api/builds/:id/logs | 获取构建日志 |

---

#### 3.2.3 定时任务服务 (Scheduled Task Service)

**优先级**: 高

**原因**:
- 自动化任务执行的核心
- 支持仓库关联和任务模板
- 与 Agent 服务紧密配合

**核心需求**:

| 功能 | 说明 | 状态 |
|------|------|------|
| 定时任务 CRUD | 创建、查询、更新、删除 | 必须 |
| 三种触发方式 | Cron 表达式、固定间隔、一次性 | 必须 |
| 仓库关联 | 关联仓库、分支、路径 | 必须 |
| Agent 关联 | 关联执行任务的 Agent | 必须 |
| 任务模板 | 支持预设模板快速创建 | 必须 |
| 执行记录 | 记录每次执行的时间和结果 | 必须 |
| 手动触发 | 支持手动立即执行 | 必须 |
| 启用/禁用 | 控制任务是否激活 | 必须 |

**数据模型**:

```sql
-- 定时任务表
CREATE TABLE scheduled_tasks (
    id VARCHAR(64) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    template_id VARCHAR(64),           -- 关联模板
    trigger_type ENUM('cron', 'interval', 'once') NOT NULL,
    cron_expression VARCHAR(100),       -- Cron 表达式
    interval_ms BIGINT,                -- 间隔毫秒
    once_time DATETIME,                -- 一次性执行时间
    agent_id VARCHAR(64) NOT NULL,     -- 关联 Agent
    repo_id VARCHAR(64),               -- 关联仓库
    branch VARCHAR(100) DEFAULT 'main', -- 目标分支
    path VARCHAR(512),                 -- 目标路径
    params JSON,                       -- 任务参数
    status ENUM('idle', 'running', 'failed', 'disabled') DEFAULT 'idle',
    enabled BOOLEAN DEFAULT TRUE,
    next_run_time DATETIME,
    last_run_time DATETIME,
    total_runs INT DEFAULT 0,
    success_runs INT DEFAULT 0,
    failed_runs INT DEFAULT 0,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NOT NULL
);

-- 定时任务执行记录表
CREATE TABLE scheduled_task_executions (
    id VARCHAR(64) PRIMARY KEY,
    task_id VARCHAR(64) NOT NULL,
    status ENUM('running', 'success', 'failed') DEFAULT 'running',
    started_at DATETIME NOT NULL,
    completed_at DATETIME,
    duration_ms BIGINT,               -- 执行耗时
    result_message TEXT,              -- 执行结果信息
    error_message TEXT,                -- 错误信息
    created_at DATETIME NOT NULL
);

-- 任务模板表
CREATE TABLE task_templates (
    id VARCHAR(64) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    agent_id VARCHAR(64) NOT NULL,
    system_prompt_override TEXT,
    trigger_type ENUM('cron', 'interval', 'once') NOT NULL,
    cron_expression VARCHAR(100),
    interval_ms BIGINT,
    default_params JSON,
    is_built_in BOOLEAN DEFAULT FALSE,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NOT NULL
);
```

**API 端点**:

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/scheduled-task | 获取定时任务列表 |
| GET | /api/scheduled-task/:id | 获取任务详情 |
| POST | /api/scheduled-task | 创建定时任务 |
| PUT | /api/scheduled-task/:id | 更新定时任务 |
| DELETE | /api/scheduled-task/:id | 删除定时任务 |
| POST | /api/scheduled-task/:id/toggle | 启用/禁用任务 |
| POST | /api/scheduled-task/:id/run | 手动立即执行 |
| GET | /api/scheduled-task/:id/executions | 获取执行记录 |
| GET | /api/scheduled-task/templates | 获取任务模板列表 |
| POST | /api/scheduled-task/templates | 创建任务模板 |

**触发方式说明**:

| 触发类型 | 说明 | 配置示例 |
|---------|------|---------|
| cron | Cron 表达式定时执行 | `0 9 * * *` 每天 9:00 |
| interval | 固定间隔执行 | `interval_ms: 21600000` 每 6 小时 |
| once | 指定时间执行一次 | `once_time: '2026-06-01 10:00'` |

**内置任务模板**:

| 模板名称 | Agent | 触发方式 | 说明 |
|---------|-------|---------|------|
| 代码审查任务 | 代码审查助手 | Cron: 每天 9:00 | 自动化代码质量检查 |
| 数据库备份任务 | 数据库专家 | 固定间隔: 6小时 | 数据库备份 |
| 文档生成任务 | 文档撰写助手 | Cron: 每周五 18:00 | 周报、API 文档生成 |
| 系统监控任务 | 架构设计专家 | 固定间隔: 1小时 | 系统性能监控 |
| 代码重构任务 | 前端开发助手 | 一次性 | 代码重构优化 |

---

### 3.3 P2 - 次要功能（后续开发）

#### 3.3.1 知识库服务 (Knowledge Service)

**优先级**: 中

**原因**:
- 为AI提供项目上下文
- 提高AI理解准确性
- 支持文档自动同步

**核心需求**:

| 功能 | 说明 | 状态 |
|------|------|------|
| 知识源管理 | 添加、删除、配置知识源 | 必须 |
| 文档抓取 | 自动抓取文档内容 | 必须 |
| 分块处理 | 智能文档分块 | 必须 |
| 刷新策略 | 手动/定时刷新 | 必须 |
| 访问控制 | 文档访问权限 | 建议 |

**数据模型**:

```sql
-- 知识源表
CREATE TABLE knowledge_sources (
    id VARCHAR(64) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    source_type ENUM('markdown', 'remote-wiki', 'repository') NOT NULL,
    source_path VARCHAR(512) NOT NULL,
    refresh_policy ENUM('manual', 'hourly', 'daily') DEFAULT 'manual',
    chunk_size INT DEFAULT 800,
    chunk_overlap INT DEFAULT 120,
    access_scope ENUM('public', 'internal', 'restricted') DEFAULT 'internal',
    repository_id VARCHAR(64),  -- 关联仓库（如果是repository类型）
    last_refresh_at DATETIME,
    next_refresh_at DATETIME,
    status ENUM('active', 'inactive', 'error') DEFAULT 'active',
    created_at DATETIME NOT NULL,
    updated_at DATETIME NOT NULL
);

-- 知识块表
CREATE TABLE knowledge_chunks (
    id VARCHAR(64) PRIMARY KEY,
    source_id VARCHAR(64) NOT NULL,
    content TEXT NOT NULL,
    chunk_index INT NOT NULL,
    metadata JSON,  -- 包含标题、路径等信息
    vector_id VARCHAR(128),  -- 向量数据库ID
    created_at DATETIME NOT NULL
);
```

**API 端点**:

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/system-config/knowledge | 获取知识源列表 |
| POST | /api/system-config/knowledge | 创建知识源 |
| PUT | /api/system-config/knowledge/:id | 更新知识源 |
| DELETE | /api/system-config/knowledge/:id | 删除知识源 |
| POST | /api/system-config/knowledge/:id/refresh | 手动刷新知识源 |
| GET | /api/system-config/knowledge/:id/chunks | 获取知识块列表 |

**支持的知识源类型**:

| 类型 | 说明 | 配置项 |
|------|------|--------|
| Markdown | 本地Markdown文件 | 路径、编码 |
| Remote Wiki | 远程Wiki系统 | URL、认证信息 |
| Repository | Git仓库文档 | 仓库ID、分支、路径 |

---

#### 3.3.3 DeepWiki 向量搜索服务

**优先级**: 中

**原因**:
- 提供语义搜索能力
- 支持智能问答
- 提高知识检索准确性

**核心需求**:

| 功能 | 说明 | 状态 |
|------|------|------|
| 向量索引管理 | 创建、更新、删除索引 | 必须 |
| 文档向量化 | 自动生成文档向量 | 必须 |
| 语义搜索 | 基于向量的相似度搜索 | 必须 |
| 索引重建 | 支持手动/自动重建索引 | 建议 |
| 数据保留策略 | 自动清理过期数据 | 建议 |

**数据模型**:

```sql
-- DeepWiki配置表
CREATE TABLE deepwiki_profiles (
    id VARCHAR(64) PRIMARY KEY,
    workspace VARCHAR(255) NOT NULL,
    index_name VARCHAR(255) NOT NULL,
    embedding_model VARCHAR(100) DEFAULT 'text-embedding-3-large',
    top_k INT DEFAULT 8,
    metric ENUM('cosine', 'dot', 'euclidean') DEFAULT 'cosine',
    retention_days INT DEFAULT 90,
    auto_reindex BOOLEAN DEFAULT TRUE,
    status ENUM('active', 'inactive', 'building') DEFAULT 'active',
    total_documents INT DEFAULT 0,
    last_indexed_at DATETIME,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NOT NULL
);

-- 向量索引记录表
CREATE TABLE vector_index_records (
    id VARCHAR(64) PRIMARY KEY,
    profile_id VARCHAR(64) NOT NULL,
    document_id VARCHAR(128) NOT NULL,
    chunk_id VARCHAR(64),
    vector_id VARCHAR(128),
    content_hash VARCHAR(64),
    indexed_at DATETIME NOT NULL,
    metadata JSON
);
```

**API 端点**:

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/system-config/deepwiki | 获取DeepWiki配置列表 |
| POST | /api/system-config/deepwiki | 创建DeepWiki配置 |
| PUT | /api/system-config/deepwiki/:id | 更新配置 |
| DELETE | /api/system-config/deepwiki/:id | 删除配置 |
| POST | /api/system-config/deepwiki/:id/reindex | 触发索引重建 |
| GET | /api/system-config/deepwiki/:id/stats | 获取索引统计 |

**向量模型选择**:

| 模型 | 向量维度 | 适用场景 | 成本 |
|------|---------|---------|------|
| text-embedding-3-large | 3072 | 高精度需求 | 高 |
| text-embedding-3-small | 1536 | 通用场景 | 中 |
| text-embedding-2 | 1536 | 成本敏感 | 低 |

**距离度量方式**:

| 方式 | 说明 | 适用场景 |
|------|------|---------|
| Cosine | 余弦相似度 | 文本相似度 |
| Dot Product | 点积 | 高维向量 |
| Euclidean | 欧氏距离 | 地理距离 |

---

#### 3.3.4 Wiki服务

**优先级**: 中

**原因**:
- 提供项目文档浏览能力
- 支持Markdown渲染
- 增强用户体验

**核心需求**:

| 功能 | 说明 | 状态 |
|------|------|------|
| MD Wiki | Markdown文件浏览 | 必须 |
| Console Wiki | 结构化Wiki页面 | 建议 |
| Wiki元数据 | 页面目录和分类 | 建议 |
| Markdown渲染 | 支持代码高亮等 | 必须 |

**数据模型**:

```sql
-- Wiki仓库表
CREATE TABLE wiki_repos (
    id VARCHAR(64) PRIMARY KEY,
    repository_id VARCHAR(64) NOT NULL,
    ref VARCHAR(100) NOT NULL,
    manifest_path VARCHAR(255) DEFAULT 'wiki.json',  -- Wiki清单文件
    generated_at DATETIME,
    status ENUM('active', 'inactive', 'error') DEFAULT 'active',
    created_at DATETIME NOT NULL,
    updated_at DATETIME NOT NULL
);

-- Wiki页面表
CREATE TABLE wiki_pages (
    id VARCHAR(64) PRIMARY KEY,
    wiki_repo_id VARCHAR(64) NOT NULL,
    slug VARCHAR(255) NOT NULL,
    title VARCHAR(255) NOT NULL,
    file_path VARCHAR(512) NOT NULL,
    category VARCHAR(100),
    author VARCHAR(255),
    ref VARCHAR(100),
    purpose TEXT,
    notes JSON,  -- 页面笔记数组
    toc JSON,    -- 目录结构
    menu_order INT DEFAULT 0,
    updated_at DATETIME,
    created_at DATETIME NOT NULL
);

-- Wiki页面内容缓存表
CREATE TABLE wiki_page_contents (
    id VARCHAR(64) PRIMARY KEY,
    page_id VARCHAR(64) NOT NULL,
    html_content LONGTEXT,
    raw_content LONGTEXT,
    cached_at DATETIME NOT NULL
);
```

**API 端点**:

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/wiki/repos | 获取Wiki仓库列表 |
| GET | /api/wiki/repos/:id/manifest | 获取Wiki清单 |
| GET | /api/wiki/pages | 获取Wiki页面列表 |
| GET | /api/wiki/pages/:id | 获取Wiki页面详情 |
| GET | /api/wiki/pages/:id/content | 获取页面HTML内容 |
| GET | /api/wiki/search | 搜索Wiki内容 |

**Wiki类型对比**:

| 类型 | 数据源 | 渲染方式 | 适用场景 |
|------|--------|---------|---------|
| MD Wiki | Git仓库Markdown | 服务端渲染 | 开发者文档 |
| Console Wiki | wiki.json清单 | 前端渲染 | 项目Wiki |

---

#### 3.3.5 集成服务 (Integration Service)

**优先级**: 中

**原因**:
- 支持第三方服务集成
- 扩展系统能力
- 提高工作流效率

**核心需求**:

| 集成类型 | 说明 | 状态 |
|---------|------|------|
| Azure DevOps | 项目管理和CI/CD | 建议 |
| GitHub Copilot | AI编程助手集成 | 建议 |
| 通用Webhook | 自定义Webhook | 建议 |

**数据模型**:

```sql
-- 集成配置表
CREATE TABLE integrations (
    id VARCHAR(64) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    type ENUM('azure-pwt', 'github-copilot', 'generic', 'webhook') NOT NULL,
    status ENUM('configured', 'beta', 'coming-soon') DEFAULT 'configured',
    description TEXT,
    tags JSON,
    config_data JSON,  -- 加密存储的敏感配置
    created_at DATETIME NOT NULL,
    updated_at DATETIME NOT NULL
);

-- Azure DevOps配置表
CREATE TABLE azure_devops_configs (
    id VARCHAR(64) PRIMARY KEY,
    integration_id VARCHAR(64) NOT NULL,
    auth_mode ENUM('entra', 'azure-devops-pat') DEFAULT 'entra',
    tenant_id VARCHAR(64),
    subscription_id VARCHAR(64),
    project_name VARCHAR(255),
    devops_org_url VARCHAR(512),
    pat_secret_ref VARCHAR(128),
    pat_rotation_days INT DEFAULT 30,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NOT NULL
);

-- GitHub Copilot配置表
CREATE TABLE github_copilot_configs (
    id VARCHAR(64) PRIMARY KEY,
    integration_id VARCHAR(64) NOT NULL,
    organization VARCHAR(255),
    executable VARCHAR(128) DEFAULT 'copilot',
    install_channel ENUM('winget', 'npm') DEFAULT 'winget',
    auth_mode ENUM('interactive', 'pat') DEFAULT 'interactive',
    pat_env_var VARCHAR(64) DEFAULT 'GH_TOKEN',
    workspace_policy ENUM('required', 'optional', 'disabled') DEFAULT 'optional',
    cli_version VARCHAR(50),
    auth_state VARCHAR(50),
    created_at DATETIME NOT NULL,
    updated_at DATETIME NOT NULL
);

-- Webhook配置表
CREATE TABLE webhook_configs (
    id VARCHAR(64) PRIMARY KEY,
    integration_id VARCHAR(64) NOT NULL,
    endpoint VARCHAR(512) NOT NULL,
    auth_type ENUM('oauth', 'token', 'app') DEFAULT 'token',
    credential_hint VARCHAR(255),
    events JSON,  -- 订阅的事件类型
    active BOOLEAN DEFAULT TRUE,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NOT NULL
);
```

**API 端点**:

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/system-config/integrations | 获取集成列表 |
| GET | /api/system-config/integrations/:id | 获取集成详情 |
| POST | /api/system-config/integrations | 创建集成 |
| PUT | /api/system-config/integrations/:id | 更新集成配置 |
| DELETE | /api/system-config/integrations/:id | 删除集成 |
| POST | /api/system-config/integrations/:id/test | 测试集成连接 |

**支持的第三方集成**:

| 集成 | 认证方式 | 主要功能 |
|------|---------|---------|
| Azure DevOps | Entra ID / PAT | 项目管理、CI/CD |
| GitHub Copilot | Interactive / PAT | AI编程助手 |
| Generic | OAuth / Token / App | 自定义集成 |
| Webhook | 自定义 | 事件通知 |

---

#### 3.3.6 配置服务 (Config Service)

**优先级**: 中

**原因**:
- 系统配置相对稳定
- 不影响核心业务流程

**核心需求**:

| 功能 | 说明 | 状态 |
|------|------|------|
| 用户偏好 | 存储用户设置 | 建议 |
| 系统配置 | 全局配置管理 | 建议 |
| 模型配置 | AI模型选择 | 建议 |

**数据模型**:

```sql
-- 用户配置表
CREATE TABLE user_configs (
    id VARCHAR(64) PRIMARY KEY,
    user_id VARCHAR(64) NOT NULL,
    key VARCHAR(100) NOT NULL,
    value TEXT,
    created_at DATETIME NOT NULL,
    updated_at DATETIME NOT NULL
);

-- 系统配置表
CREATE TABLE system_configs (
    id VARCHAR(64) PRIMARY KEY,
    key VARCHAR(100) NOT NULL UNIQUE,
    value TEXT,
    description VARCHAR(512),
    updated_at DATETIME NOT NULL
);
```

---

#### 3.3.2 Dashboard 数据服务

**优先级**: 中

**原因**:
- 统计数据基于核心业务
- 可在核心功能稳定后开发

**核心需求**:

| 功能 | 说明 | 状态 |
|------|------|------|
| 任务统计 | 今日任务数、成功率 | 建议 |
| 趋势数据 | 历史趋势统计 | 建议 |
| 告警统计 | 异常告警数量 | 建议 |

---

## 4. 技术基础设施优先级

### 4.1 数据库设计

**优先级**: P0

**关键考虑**:
- 任务数据需要事务支持（MySQL）
- 日志数据需要高效写入（MySQL/MongoDB）
- 会话数据需要缓存（Redis）

### 4.2 API 网关

**优先级**: P0

**关键考虑**:
- 统一认证和授权
- 请求限流和熔断
- API 版本管理

### 4.3 消息队列

**优先级**: P0

**关键考虑**:
- 任务执行异步处理
- AI响应异步推送
- 解耦任务执行和HTTP响应

### 4.4 缓存层

**优先级**: P1

**关键考虑**:
- 会话上下文缓存
- 仓库数据缓存
- 统计数据缓存

### 4.5 日志系统

**优先级**: P1

**关键考虑**:
- 应用日志收集
- 任务日志存储
- 日志查询和分析

---

## 5. 开发路线图

### 阶段1：核心功能（第1-2周）

| 任务 | 估计时间 | 负责人 |
|------|----------|--------|
| 任务服务API开发 | 3天 | 后端1 |
| 聊天服务API开发 | 3天 | 后端2 |
| Agent服务API开发 | 2天 | 后端1 |
| 数据库表设计与创建 | 2天 | DBA |
| Mock数据支持 | 1天 | 全栈 |

### 阶段2：重要功能（第3-4周）

| 任务 | 估计时间 | 负责人 |
|------|----------|--------|
| 定时任务服务API开发 | 3天 | 后端1 |
| 仓库服务API开发 | 2天 | 后端2 |
| 流水线服务API开发 | 2天 | 后端1 |
| 消息队列集成 | 2天 | 架构师 |
| Redis缓存集成 | 1天 | 后端2 |

### 阶段3：次要功能（第5-6周）

| 任务 | 估计时间 | 负责人 |
|------|----------|--------|
| 配置服务API开发 | 2天 | 后端2 |
| Dashboard数据API | 2天 | 后端1 |
| 日志系统集成 | 2天 | DevOps |

### 阶段4：高级功能（第7-8周）

| 任务 | 估计时间 | 负责人 |
|------|----------|--------|
| 知识库服务API | 3天 | 后端2 |
| DeepWiki向量搜索服务 | 3天 | 后端1 |
| Wiki服务API | 2天 | 后端2 |
| 集成服务API | 3天 | 后端1 |

### 阶段5：第三方集成（第9-10周）

| 任务 | 估计时间 | 负责人 |
|------|----------|--------|
| Azure DevOps集成 | 3天 | 后端2 |
| GitHub Copilot集成 | 3天 | 后端1 |
| Webhook支持 | 2天 | 后端2 |
| 集成测试和调试 | 2天 | QA |

---

## 6. 风险评估

### 6.1 高风险项

| 风险 | 描述 | 影响 | 缓解措施 |
|------|------|------|----------|
| AI响应延迟 | AI服务响应时间不确定 | 用户体验 | 异步处理+消息队列 |
| 任务执行失败 | 代码生成可能失败 | 业务流程中断 | 重试机制+错误日志 |
| 仓库访问权限 | 第三方仓库API限制 | 数据同步失败 | 缓存+指数退避 |

### 6.2 性能考虑

| 场景 | 预期QPS | 优化策略 |
|------|---------|----------|
| 任务列表查询 | 100 | 分页+索引 |
| 消息发送 | 50 | 消息队列异步 |
| 日志查询 | 30 | 分区表+全文索引 |

---

## 7. 总结

### 优先级排序

| 优先级 | 模块 | 原因 |
|--------|------|------|
| **P0** | 任务服务 | 核心业务流程 |
| **P0** | 聊天服务 | 核心交互方式 |
| **P0** | Agent服务 | 自动选择核心 |
| **P0** | 数据库设计 | 所有功能基础 |
| **P0** | API网关 | 系统入口 |
| **P0** | 消息队列 | 异步处理必备 |
| **P1** | 定时任务服务 | 自动化任务调度 |
| **P1** | 仓库服务 | 任务关联依赖 |
| **P1** | 流水线服务 | CI/CD集成 |
| **P1** | 缓存层 | 性能优化 |
| **P2** | 配置服务 | 辅助功能 |
| **P2** | Dashboard | 统计展示 |

### 推荐开发顺序

```
1. 数据库设计与初始化
2. 任务服务 API
3. 聊天服务 API
4. Agent服务 API
5. API网关配置
6. 消息队列集成
7. 定时任务服务 API
8. 仓库服务 API
9. 流水线服务 API
10. Redis缓存集成
11. 配置服务 API
12. Dashboard数据 API
13. 知识库服务
14. DeepWiki服务
15. Wiki服务
16. 集成服务
```

---

**文档版本**: v1.2
**生成日期**: 2026-05-19
**最后更新**: 2026-05-19（新增Agent服务、定时任务升级为P1、仓库关联和任务模板）
**适用对象**: 后端开发团队、架构师