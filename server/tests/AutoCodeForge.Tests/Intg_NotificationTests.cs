/**
 * NotificationTests 通知服务功能测试
 *
 * 测试覆盖：
 * 1. 发送站内信通知
 * 2. 获取用户通知列表
 * 3. 标记通知已读
 * 4. 通知频率控制（防抖/合并）
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Infrastructure.Repositories;
using Microsoft.Extensions.Options;

namespace AutoCodeForge.Tests;

/// <summary>
/// 通知服务功能测试
/// </summary>
public sealed class Intg_NotificationTests : IDisposable
{
    private readonly IntegrationTestContext _context;
    private readonly INotificationService _notificationService;

    public Intg_NotificationTests()
    {
        _context = new IntegrationTestContext("test-notification-user");
        
        var templateSettings = Options.Create(new Application.Models.NotificationTemplateSettings
        {
            Templates = new List<Application.Models.NotificationTemplate>
            {
                new Application.Models.NotificationTemplate
                {
                    TemplateId = "PlanApproval",
                    Name = "计划审批通知",
                    Subject = "任务 {{TaskId}} 需要计划审批",
                    Content = "任务 {{TaskId}} 的计划已完成，需要您进行审批。",
                    Channel = NotificationChannel.InApp
                },
                new Application.Models.NotificationTemplate
                {
                    TemplateId = "CodeReview",
                    Name = "代码审核通知",
                    Subject = "任务 {{TaskId}} 需要代码审核",
                    Content = "任务 {{TaskId}} 的代码已完成，需要您进行审核。",
                    Channel = NotificationChannel.InApp
                },
                new Application.Models.NotificationTemplate
                {
                    TemplateId = "Emergency",
                    Name = "紧急通知",
                    Subject = "紧急：任务 {{TaskId}} 需要立即处理",
                    Content = "任务 {{TaskId}} 出现紧急情况，请立即处理。",
                    Channel = NotificationChannel.InApp
                }
            }
        });
        
        var inAppChannel = new Infrastructure.Notification.Channels.InAppNotificationChannel(_context.NotificationRepository);
        
        _notificationService = new NotificationService(
            _context.NotificationRepository,
            _context.InMemoryTaskEventPublisher,
            templateSettings,
            new List<INotificationChannel> { inAppChannel });
    }

    #region SendNotification Tests

    /// <summary>
    /// 测试发送站内信通知
    /// </summary>
    [Fact]
    public async Task SendNotificationAsync_Should_SaveInAppNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var variables = new Dictionary<string, string>
        {
            { "TaskId", Guid.NewGuid().ToString() },
            { "GateType", "PlanApproval" }
        };

        // Act
        await _notificationService.SendNotificationAsync(userId, NotificationChannel.InApp, "PlanApproval", variables);

        // Assert
        var notifications = await _context.NotificationRepository.GetByUserIdAsync(userId);
        Assert.Single(notifications);
        Assert.Equal(NotificationChannel.InApp, notifications[0].Channel);
        Assert.Equal(NotificationPriority.Medium, notifications[0].Priority);
        Assert.False(notifications[0].IsRead);
        Console.WriteLine("[测试1] 发送站内信通知成功");
    }

    /// <summary>
    /// 测试发送通知 - 变量替换
    /// </summary>
    [Fact]
    public async Task SendNotificationAsync_Should_ReplaceVariables()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid().ToString();
        var variables = new Dictionary<string, string>
        {
            { "TaskId", taskId },
            { "GateType", "PlanApproval" },
            { "UserName", "TestUser" }
        };

        // Act
        await _notificationService.SendNotificationAsync(userId, NotificationChannel.InApp, "PlanApproval", variables);

        // Assert
        var notifications = await _context.NotificationRepository.GetByUserIdAsync(userId);
        Assert.Single(notifications);
        Assert.Contains(taskId, notifications[0].Content);
        Console.WriteLine("[测试2] 变量替换成功");
    }

    #endregion

    #region GetUserNotifications Tests

    /// <summary>
    /// 测试获取用户通知列表
    /// </summary>
    [Fact]
    public async Task GetUserNotificationsAsync_Should_ReturnAllNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // 使用不同的模板ID避免防抖合并
        await _notificationService.SendNotificationAsync(userId, NotificationChannel.InApp, "PlanApproval", new Dictionary<string, string>());
        await _notificationService.SendNotificationAsync(userId, NotificationChannel.InApp, "CodeReview", new Dictionary<string, string>());
        await _notificationService.SendNotificationAsync(userId, NotificationChannel.InApp, "Emergency", new Dictionary<string, string>());

        // Act
        var notifications = await _notificationService.GetUserNotificationsAsync(userId);

        // Assert
        Assert.Equal(3, notifications.Count);
        Console.WriteLine("[测试3] 获取用户通知列表成功");
    }

    /// <summary>
    /// 测试获取未读通知
    /// </summary>
    [Fact]
    public async Task GetUserNotificationsAsync_Should_FilterByReadStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        await _notificationService.SendNotificationAsync(userId, NotificationChannel.InApp, "PlanApproval", new Dictionary<string, string>());

        var notifications = await _context.NotificationRepository.GetByUserIdAsync(userId);
        Assert.Single(notifications);
        notifications[0].IsRead = true;
        await _context.NotificationRepository.UpdateAsync(notifications[0]);

        // Act
        var unreadNotifications = await _notificationService.GetUserNotificationsAsync(userId, false);

        // Assert
        Assert.Empty(unreadNotifications);
        Console.WriteLine("[测试4] 按已读状态过滤成功");
    }

    #endregion

    #region MarkAsRead Tests

    /// <summary>
    /// 测试标记通知已读
    /// </summary>
    [Fact]
    public async Task MarkAsReadAsync_Should_UpdateReadStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        await _notificationService.SendNotificationAsync(userId, NotificationChannel.InApp, "PlanApproval", new Dictionary<string, string>());

        var notifications = await _context.NotificationRepository.GetByUserIdAsync(userId);
        Assert.Single(notifications);
        var notificationId = notifications[0].Id;

        // Act
        await _notificationService.MarkAsReadAsync(notificationId);

        // Assert
        var updatedNotification = await _context.NotificationRepository.GetByIdAsync(notificationId);
        Assert.NotNull(updatedNotification);
        Assert.True(updatedNotification.IsRead);
        Assert.NotNull(updatedNotification.ReadAt);
        Console.WriteLine("[测试5] 标记通知已读成功");
    }

    /// <summary>
    /// 测试标记所有通知已读
    /// </summary>
    [Fact]
    public async Task MarkAllAsReadAsync_Should_UpdateAllNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        for (int i = 0; i < 3; i++)
        {
            await _notificationService.SendNotificationAsync(userId, NotificationChannel.InApp, "PlanApproval", new Dictionary<string, string>());
        }

        // Act
        await _notificationService.MarkAllAsReadAsync(userId);

        // Assert
        var notifications = await _context.NotificationRepository.GetByUserIdAsync(userId);
        Assert.True(notifications.All(n => n.IsRead));
        Console.WriteLine("[测试6] 标记所有通知已读成功");
    }

    #endregion

    #region RateLimiting Tests

    /// <summary>
    /// 测试通知防抖 - 相同任务的通知在短时间内合并
    /// </summary>
    [Fact]
    public async Task RateLimiting_Should_MergeDuplicateNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid().ToString();

        // Act - 短时间内发送相同类型的通知
        await _notificationService.SendNotificationAsync(userId, NotificationChannel.InApp, "PlanApproval", new Dictionary<string, string> { { "TaskId", taskId } });

        await Task.Delay(10); // 小于防抖时间

        await _notificationService.SendNotificationAsync(userId, NotificationChannel.InApp, "PlanApproval", new Dictionary<string, string> { { "TaskId", taskId } });

        // Assert - 应该只收到一条通知（防抖合并）
        var notifications = await _context.NotificationRepository.GetByUserIdAsync(userId);
        Assert.Single(notifications);
        Console.WriteLine("[测试7] 通知防抖合并成功");
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}