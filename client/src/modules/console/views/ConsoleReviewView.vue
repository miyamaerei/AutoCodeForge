<script setup lang="ts">
import { ref } from 'vue'

interface ReviewItem {
  id: string
  prNumber: string
  title: string
  author: string
  status: 'open' | 'merged' | 'closed'
  createdAt: string
  reviewers: string[]
}

const selectedRepository = ref('AutoCodeForge')
const repositories = ['AutoCodeForge', 'backend-service', 'mobile-app']

const reviews = ref<ReviewItem[]>([
  {
    id: '1',
    prNumber: 'PR-145',
    title: '修复订单导出功能的空指针异常',
    author: 'Alice Chen',
    status: 'open',
    createdAt: '2024-05-19 08:00',
    reviewers: ['Bob Smith', 'Carol Wang'],
  },
  {
    id: '2',
    prNumber: 'PR-144',
    title: '优化数据库连接池配置',
    author: 'Bob Smith',
    status: 'merged',
    createdAt: '2024-05-18 10:00',
    reviewers: ['Alice Chen'],
  },
  {
    id: '3',
    prNumber: 'PR-143',
    title: '添加支付回调重试机制',
    author: 'Carol Wang',
    status: 'open',
    createdAt: '2024-05-17 09:30',
    reviewers: ['David Lee'],
  },
])

const selectedReview = ref<ReviewItem | null>(null)
const showReviewDrawer = ref(false)

const selectReview = (review: ReviewItem) => {
  selectedReview.value = review
  showReviewDrawer.value = true
}

const statusColorMap = {
  open: 'success',
  merged: 'info',
  closed: 'danger',
}
</script>

<template>
  <section class="review-view">
    <div class="review-header">
      <div class="header-title">
        <h2>代码审查</h2>
        <p>PR 管理和代码审查</p>
      </div>

      <div class="repo-selector">
        <label>选择仓库</label>
        <el-select v-model="selectedRepository" size="small">
          <el-option v-for="repo in repositories" :key="repo" :label="repo" :value="repo" />
        </el-select>
      </div>
    </div>

    <el-table :data="reviews" stripe class="review-table">
      <el-table-column prop="prNumber" label="PR ID" width="100" />
      <el-table-column prop="title" label="标题" min-width="300" />
      <el-table-column prop="author" label="作者" width="150" />
      <el-table-column prop="status" label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="statusColorMap[row.status as keyof typeof statusColorMap]">
            {{ row.status }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column prop="createdAt" label="创建时间" width="180" />
      <el-table-column label="审查者" min-width="200">
        <template #default="{ row }">
          <div class="reviewers">
            <el-avatar
              v-for="reviewer in row.reviewers"
              :key="reviewer"
              :size="28"
              :src="`https://api.dicebear.com/7.x/avataaars/svg?seed=${reviewer}`"
            />
          </div>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="150">
        <template #default="{ row }">
          <el-button link type="primary" size="small" @click="selectReview(row)">查看</el-button>
          <el-button link type="primary" size="small">评论</el-button>
          <el-button link type="primary" size="small">批准</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 审查详情 -->
    <el-drawer v-model="showReviewDrawer" title="PR 详情" size="50%">
      <div v-if="selectedReview" class="review-detail">
        <div class="detail-header">
          <h3>{{ selectedReview.title }}</h3>
          <el-tag :type="statusColorMap[selectedReview.status as keyof typeof statusColorMap]">
            {{ selectedReview.status }}
          </el-tag>
        </div>

        <div class="detail-info">
          <div class="info-item">
            <span class="label">PR ID:</span>
            <span class="value">{{ selectedReview.prNumber }}</span>
          </div>
          <div class="info-item">
            <span class="label">作者:</span>
            <span class="value">{{ selectedReview.author }}</span>
          </div>
          <div class="info-item">
            <span class="label">创建时间:</span>
            <span class="value">{{ selectedReview.createdAt }}</span>
          </div>
          <div class="info-item">
            <span class="label">审查者:</span>
            <div class="reviewers">
              <el-tag v-for="reviewer in selectedReview.reviewers" :key="reviewer" size="small">
                {{ reviewer }}
              </el-tag>
            </div>
          </div>
        </div>

        <el-divider />

        <h4>代码变更</h4>
        <div class="code-diff">
          <pre><code>- return exportOrder(data)
+ return exportOrder(data ?? [])</code></pre>
        </div>

        <el-divider />

        <h4>评论</h4>
        <div class="comments">
          <div class="comment">
            <div class="comment-header">
              <strong>Alice Chen</strong>
              <span class="comment-time">2024-05-19 09:30</span>
            </div>
            <div class="comment-body">看起来很好！这个修复是必需的。</div>
          </div>
        </div>
      </div>

      <template #footer>
        <div style="display: flex; gap: 8px">
          <el-button @click="showReviewDrawer = false">关闭</el-button>
          <el-button type="primary">批准</el-button>
          <el-button type="warning">请求更改</el-button>
        </div>
      </template>
    </el-drawer>
  </section>
</template>

<style scoped>
.review-view {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  background: white;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
}

.review-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px;
  border-bottom: 1px solid #e2e8f0;
}

.header-title h2 {
  font-size: 24px;
  font-weight: 700;
  color: #0f172a;
  margin: 0 0 4px;
}

.header-title p {
  font-size: 13px;
  color: #64748b;
  margin: 0;
}

.repo-selector {
  width: 200px;
}

.repo-selector label {
  display: block;
  font-size: 12px;
  font-weight: 600;
  color: #64748b;
  margin-bottom: 6px;
}

.review-table {
  flex: 1;
  overflow-y: auto;
}

.reviewers {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.review-detail {
  padding: 16px;
}

.detail-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 16px;
}

.detail-header h3 {
  font-size: 18px;
  font-weight: 700;
  color: #0f172a;
  margin: 0;
}

.detail-info {
  display: grid;
  gap: 12px;
  margin-bottom: 16px;
}

.info-item {
  display: grid;
  grid-template-columns: 100px 1fr;
  gap: 12px;
  align-items: center;
}

.label {
  font-weight: 600;
  color: #64748b;
  font-size: 13px;
}

.value {
  color: #0f172a;
  font-size: 13px;
}

.code-diff {
  background: #f1f5f9;
  border-radius: 8px;
  padding: 12px;
  overflow-x: auto;
  margin-bottom: 16px;
}

.code-diff pre {
  margin: 0;
  font-size: 13px;
  font-family: 'Monaco', 'Courier New', monospace;
}

.code-diff code {
  color: #0f172a;
}

.comments {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.comment {
  padding: 12px;
  background: #f8fafc;
  border-radius: 8px;
  border-left: 3px solid #667eea;
}

.comment-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 6px;
}

.comment-header strong {
  font-size: 13px;
  color: #0f172a;
}

.comment-time {
  font-size: 12px;
  color: #94a3b8;
}

.comment-body {
  font-size: 13px;
  color: #475569;
  line-height: 1.5;
}
</style>
