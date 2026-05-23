export type NotificationChannel = 'InApp' | 'Email' | 'Webhook' | 'IM'

export type NotificationPriority = 'Low' | 'Medium' | 'High'

export interface NotificationDto {
  id: string
  userId: string
  channel: NotificationChannel
  priority: NotificationPriority
  title: string
  content: string
  isRead: boolean
  readAt?: string
  taskId?: string
  gateId?: string
  actionUrl?: string
  createdAtUtc: string
}

export interface SendNotificationRequest {
  userId: string
  channel: NotificationChannel
  templateId: string
  variables?: Record<string, string>
  priority?: NotificationPriority
}

export interface NotificationTemplateDto {
  templateId: string
  name: string
  subject: string
  content: string
  channel: NotificationChannel
}