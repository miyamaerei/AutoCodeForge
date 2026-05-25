import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import * as api from '../api/notification.api'
import type { NotificationDto, SendNotificationRequest, NotificationTemplateDto } from '../api/notification.types'

export const useNotificationStore = defineStore('notification', () => {
  const notifications = ref<NotificationDto[]>([])
  const templates = ref<NotificationTemplateDto[]>([])
  const loading = ref(false)
  const error = ref('')

  const unreadCount = computed(() => notifications.value.filter(n => !n.isRead).length)

  async function loadUserNotifications(userId: string) {
    loading.value = true
    error.value = ''
    try {
      notifications.value = await api.getUserNotifications(userId)
    } catch (e) {
      error.value = '加载通知失败'
      console.error('Failed to load notifications:', e)
    } finally {
      loading.value = false
    }
  }

  async function loadTemplates() {
    loading.value = true
    error.value = ''
    try {
      templates.value = await api.getNotificationTemplates()
    } catch (e) {
      error.value = '加载模板失败'
      console.error('Failed to load templates:', e)
    } finally {
      loading.value = false
    }
  }

  async function send(request: SendNotificationRequest) {
    try {
      await api.sendNotification(request)
    } catch (e) {
      error.value = '发送通知失败'
      throw e
    }
  }

  async function markNotificationAsRead(notificationId: string) {
    try {
      await api.markAsRead(notificationId)
      const notification = notifications.value.find(n => n.id === notificationId)
      if (notification) {
        notification.isRead = true
        notification.readAt = new Date().toISOString()
      }
    } catch (e) {
      error.value = '标记已读失败'
      throw e
    }
  }

  async function markAllUserNotificationsAsRead(userId: string) {
    try {
      await api.markAllAsRead(userId)
      notifications.value.forEach(n => {
        n.isRead = true
        n.readAt = new Date().toISOString()
      })
    } catch (e) {
      error.value = '全部标记已读失败'
      throw e
    }
  }

  function dismissError() {
    error.value = ''
  }

  return {
    notifications,
    templates,
    loading,
    error,
    unreadCount,
    loadUserNotifications,
    loadTemplates,
    send,
    markNotificationAsRead,
    markAllUserNotificationsAsRead,
    dismissError,
  }
})