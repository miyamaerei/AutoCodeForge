/**
 * Agent 模块类型定义
 */

/** 分页结果 */
export interface PagedResult<T> {
  /** 数据项 */
  items: T[]
  /** 总数 */
  totalCount: number
  /** 当前页 */
  page: number
  /** 每页大小 */
  pageSize: number
}

/** API 响应包装器 */
export interface ApiEnvelope<T> {
  success: boolean
  message: string
  data: T
  traceId?: string
}

/** API 错误 */
export interface ApiError {
  message: string
  errors?: Record<string, string[]>
}
