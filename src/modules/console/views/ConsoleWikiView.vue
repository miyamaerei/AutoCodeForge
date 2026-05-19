<script setup lang="ts">
import { ref } from 'vue'

interface WikiPage {
  id: string
  title: string
  category: string
  updatedAt: string
  author: string
}

const selectedRepository = ref('AutoCodeForge')
const repositories = ['AutoCodeForge', 'backend-service', 'mobile-app']

const wikiPages = ref<WikiPage[]>([
  {
    id: '1',
    title: '项目架构设计',
    category: '架构',
    updatedAt: '2024-05-19',
    author: 'Alice Chen',
  },
  {
    id: '2',
    title: '开发环境配置',
    category: '入门',
    updatedAt: '2024-05-18',
    author: 'Bob Smith',
  },
  {
    id: '3',
    title: 'API 文档',
    category: '文档',
    updatedAt: '2024-05-17',
    author: 'Carol Wang',
  },
  {
    id: '4',
    title: '数据库设计',
    category: '数据库',
    updatedAt: '2024-05-16',
    author: 'David Lee',
  },
])

const selectedPage = ref<WikiPage | null>(null)

const selectPage = (page: WikiPage) => {
  selectedPage.value = page
}
</script>

<template>
  <section class="wiki-view">
    <el-container>
      <el-aside width="280px" class="wiki-sidebar">
        <div class="sidebar-header">
          <h3>Wiki</h3>
        </div>

        <div class="repo-selector">
          <label>选择仓库</label>
          <el-select v-model="selectedRepository" size="small">
            <el-option v-for="repo in repositories" :key="repo" :label="repo" :value="repo" />
          </el-select>
        </div>

        <div class="wiki-list">
          <div
            v-for="page in wikiPages"
            :key="page.id"
            class="wiki-item"
            :class="{ active: selectedPage?.id === page.id }"
            @click="selectPage(page)"
          >
            <div class="wiki-icon">📄</div>
            <div class="wiki-info">
              <div class="wiki-title">{{ page.title }}</div>
              <div class="wiki-category">{{ page.category }}</div>
            </div>
          </div>
        </div>

        <div class="sidebar-footer">
          <el-button type="primary" size="small" block>+ 新建页面</el-button>
        </div>
      </el-aside>

      <el-main class="wiki-content">
        <div v-if="selectedPage" class="content-area">
          <div class="content-header">
            <h2>{{ selectedPage.title }}</h2>
            <div class="content-meta">
              <span>📅 {{ selectedPage.updatedAt }}</span>
              <span>✍️ {{ selectedPage.author }}</span>
            </div>
          </div>

          <div class="content-body">
            <el-card shadow="never">
              <div class="wiki-markdown">
                <h3>概述</h3>
                <p>这是 {{ selectedPage.title }} 的详细文档。包含完整的说明和示例代码。</p>

                <h3>目录</h3>
                <ul>
                  <li>简介</li>
                  <li>功能特性</li>
                  <li>使用方法</li>
                  <li>常见问题</li>
                </ul>

                <h3>详细说明</h3>
                <p>这里包含详细的技术说明和使用指南。您可以在这里找到所有相关的信息和最佳实践。</p>

                <pre><code>// 示例代码
function example() {
  console.log('Hello, Wiki!');
}</code></pre>

                <h3>相关资源</h3>
                <ul>
                  <li><a href="#">项目仓库</a></li>
                  <li><a href="#">API 文档</a></li>
                  <li><a href="#">问题跟踪</a></li>
                </ul>
              </div>
            </el-card>
          </div>

          <div class="content-footer">
            <el-button type="primary" plain>编辑</el-button>
            <el-button plain>版本历史</el-button>
            <el-button type="danger" plain>删除</el-button>
          </div>
        </div>

        <div v-else class="empty-state">
          <el-empty description="选择一个 Wiki 页面查看内容" />
        </div>
      </el-main>
    </el-container>
  </section>
</template>

<style scoped>
.wiki-view {
  width: 100%;
  height: 100%;
  background: white;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
}

.wiki-sidebar {
  background: #f8fafc;
  border-right: 1px solid #e2e8f0;
  padding: 16px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
}

.sidebar-header {
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid #e2e8f0;
}

.sidebar-header h3 {
  font-size: 16px;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}

.repo-selector {
  margin-bottom: 16px;
}

.repo-selector label {
  display: block;
  font-size: 12px;
  font-weight: 600;
  color: #64748b;
  margin-bottom: 6px;
}

.wiki-list {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-bottom: 16px;
}

.wiki-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 12px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
}

.wiki-item:hover {
  border-color: #cbd5e1;
  background: #f1f5f9;
}

.wiki-item.active {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border-color: #667eea;
}

.wiki-icon {
  font-size: 18px;
}

.wiki-info {
  flex: 1;
  min-width: 0;
}

.wiki-title {
  font-weight: 600;
  font-size: 13px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.wiki-category {
  font-size: 11px;
  color: #94a3b8;
  margin-top: 2px;
}

.wiki-item.active .wiki-category {
  color: rgba(255, 255, 255, 0.7);
}

.sidebar-footer {
  padding-top: 12px;
  border-top: 1px solid #e2e8f0;
}

.wiki-content {
  display: flex;
  flex-direction: column;
  overflow-y: auto;
}

.content-area {
  display: flex;
  flex-direction: column;
  height: 100%;
  padding: 24px;
}

.content-header {
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid #e2e8f0;
}

.content-header h2 {
  font-size: 24px;
  font-weight: 700;
  color: #0f172a;
  margin: 0 0 8px;
}

.content-meta {
  display: flex;
  gap: 16px;
  font-size: 13px;
  color: #64748b;
}

.content-body {
  flex: 1;
  overflow-y: auto;
  margin-bottom: 16px;
}

.wiki-markdown {
  line-height: 1.8;
  color: #1f2937;
}

.wiki-markdown h3 {
  font-size: 16px;
  font-weight: 700;
  color: #0f172a;
  margin: 16px 0 8px;
}

.wiki-markdown p {
  margin: 8px 0;
  color: #4b5563;
}

.wiki-markdown ul {
  margin: 8px 0;
  padding-left: 24px;
  color: #4b5563;
}

.wiki-markdown pre {
  background: #f1f5f9;
  padding: 12px 16px;
  border-radius: 8px;
  overflow-x: auto;
  margin: 12px 0;
}

.wiki-markdown code {
  font-family: 'Monaco', 'Courier New', monospace;
  font-size: 13px;
  color: #0f172a;
}

.content-footer {
  padding-top: 12px;
  border-top: 1px solid #e2e8f0;
  display: flex;
  gap: 8px;
}

.empty-state {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
}
</style>
