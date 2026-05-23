import { request } from '@/lib/request'
import type { NotificationDto, SendNotificationRequest, NotificationTemplateDto } from './notification.types'

export async function sendNotification(payload: SendNotificationRequest): Promise<void> {
  await request.post('/api/v1/notifications/send', payload)
}

export async function getUserNotifications(userId: string, isRead?: boolean): Promise<NotificationDto[]> {
  const params = isRead !== undefined ? { isRead } : {}
  const { data } = await request.get<NotificationDto[]>(`/api/v1/notifications/user/${userId}`, { params })
  return data
}

export async function markAsRead(notificationId: string): Promise<void> {
  await request.put(`/api/v1/notifications/${notificationId}/read`)
}

export async function markAllAsRead(userId: string): Promise<void> {
  await request.put(`/api/v1/notifications/user/${userId}/read-all`)
}

export async function getNotificationTemplates(): Promise<NotificationTemplateDto[]> {
  const { data } = await request.get<NotificationTemplateDto[]>('/api/v1/notifications/templates')
  return data
}