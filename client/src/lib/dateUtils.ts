export class DateUtils {
  static formatUpTime(upTime: string | null): string {
    if (!upTime) return '--'
    const timeStr = String(upTime)
    
    const dayMatch = timeStr.match(/^(\d+)\.(\d{1,2}):(\d{2}):(\d{2})/)
    if (dayMatch) {
      const days = parseInt(dayMatch[1] || '0')
      const hours = parseInt(dayMatch[2] || '0')
      const minutes = parseInt(dayMatch[3] || '0')
      const seconds = parseInt(dayMatch[4] || '0')
      
      if (days > 0) {
        return `${days}天 ${hours}时 ${minutes}分 ${seconds}秒`
      } else {
        return `${hours}时 ${minutes}分 ${seconds}秒`
      }
    }
    
    const timeMatch = timeStr.match(/^(\d{1,2}):(\d{2}):(\d{2})/)
    if (timeMatch) {
      return `${timeMatch[1]}时 ${timeMatch[2]}分 ${timeMatch[3]}秒`
    }
    
    return '--'
  }

  static formatDateTime(dateTimeString: string | null): string {
    if (!dateTimeString) return '--'
    
    try {
      const date = new Date(dateTimeString)
      if (isNaN(date.getTime())) return '--'
      
      const year = date.getFullYear()
      const month = String(date.getMonth() + 1).padStart(2, '0')
      const day = String(date.getDate()).padStart(2, '0')
      const hours = String(date.getHours()).padStart(2, '0')
      const minutes = String(date.getMinutes()).padStart(2, '0')
      const seconds = String(date.getSeconds()).padStart(2, '0')
      
      return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`
    } catch {
      return '--'
    }
  }

  static formatDate(dateString: string | null): string {
    if (!dateString) return '--'
    
    try {
      const date = new Date(dateString)
      if (isNaN(date.getTime())) return '--'
      
      const year = date.getFullYear()
      const month = String(date.getMonth() + 1).padStart(2, '0')
      const day = String(date.getDate()).padStart(2, '0')
      
      return `${year}-${month}-${day}`
    } catch {
      return '--'
    }
  }

  static formatTime(dateTimeString: string | null): string {
    if (!dateTimeString) return '--'
    
    try {
      const date = new Date(dateTimeString)
      if (isNaN(date.getTime())) return '--'
      
      const hours = String(date.getHours()).padStart(2, '0')
      const minutes = String(date.getMinutes()).padStart(2, '0')
      const seconds = String(date.getSeconds()).padStart(2, '0')
      
      return `${hours}:${minutes}:${seconds}`
    } catch {
      return '--'
    }
  }

  static formatTimestamp(timestamp: number | string | null): string {
    if (!timestamp) return '--'
    
    try {
      let numTimestamp = typeof timestamp === 'string' ? parseInt(timestamp, 10) : timestamp
      
      if (numTimestamp < 10000000000 && numTimestamp >= 1000000000) {
        numTimestamp *= 1000
      }
      
      const date = new Date(numTimestamp)
      if (isNaN(date.getTime())) return '--'
      
      const year = date.getFullYear()
      const month = String(date.getMonth() + 1).padStart(2, '0')
      const day = String(date.getDate()).padStart(2, '0')
      const hours = String(date.getHours()).padStart(2, '0')
      const minutes = String(date.getMinutes()).padStart(2, '0')
      const seconds = String(date.getSeconds()).padStart(2, '0')
      
      return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`
    } catch {
      return '--'
    }
  }

  static getRelativeTime(dateTimeString: string | null): string {
    if (!dateTimeString) return '--'
    
    try {
      const date = new Date(dateTimeString)
      if (isNaN(date.getTime())) return '--'
      
      const now = new Date()
      const diffMs = now.getTime() - date.getTime()
      const diffSec = Math.floor(diffMs / 1000)
      const diffMin = Math.floor(diffSec / 60)
      const diffHour = Math.floor(diffMin / 60)
      const diffDay = Math.floor(diffHour / 24)
      
      if (diffDay > 0) return `${diffDay}天前`
      if (diffHour > 0) return `${diffHour}小时前`
      if (diffMin > 0) return `${diffMin}分钟前`
      return '刚刚'
    } catch {
      return '--'
    }
  }

  static formatDuration(seconds: number): string {
    const hours = Math.floor(seconds / 3600)
    const minutes = Math.floor((seconds % 3600) / 60)
    const secs = seconds % 60
    
    if (hours > 0) {
      return `${hours}小时${minutes}分${secs}秒`
    } else if (minutes > 0) {
      return `${minutes}分${secs}秒`
    } else {
      return `${secs}秒`
    }
  }

  static isValidDate(dateString: string): boolean {
    try {
      const date = new Date(dateString)
      return !isNaN(date.getTime())
    } catch {
      return false
    }
  }
}
