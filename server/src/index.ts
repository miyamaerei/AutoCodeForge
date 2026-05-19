// 后端服务入口文件
import express from 'express'
import cors from 'cors'

const app = express()
const PORT = process.env.PORT || 3000

// 中间件
app.use(cors())
app.use(express.json())

// 健康检查
app.get('/health', (_req, res) => {
  res.json({ status: 'ok', timestamp: new Date().toISOString() })
})

// API 路由示例
app.get('/api/hello', (_req, res) => {
  res.json({ message: 'Hello from AutoCodeForge Server' })
})

app.listen(PORT, () => {
  console.log(`🚀 Server running on http://localhost:${PORT}`)
})

export default app
