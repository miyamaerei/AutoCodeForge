/**
 * AgentCommunication Agent间通信功能测试
 *
 * 测试覆盖：
 * 1. InMemoryTaskEventPublisher - 进程内事件发布与订阅
 * 2. DatabaseArtifactStore - 产出物存储操作
 * 3. ContextChainService - 上下文链式传递
 * 4. ArtifactContract - 标准化产出物格式
 */

using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Tests;

/// <summary>
/// Agent间通信功能测试
/// </summary>
public sealed class Intg_AgentCommunicationTests : IDisposable
{
    private readonly IntegrationTestContext _context;

    public Intg_AgentCommunicationTests()
    {
        _context = new IntegrationTestContext("test-communication-user");
    }

    #region InMemoryTaskEventPublisher Tests

    /// <summary>
    /// 测试事件发布 - TaskCreated事件
    /// </summary>
    [Fact]
    public async Task PublishTaskCreatedAsync_Should_NotifySubscribers()
    {
        // Arrange
        var receivedEvents = new List<TaskEvent>();
        _context.InMemoryTaskEventPublisher.Subscribe((e, ct) =>
        {
            receivedEvents.Add(e);
            return Task.CompletedTask;
        });

        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Pending,
            TaskType = Core.Enums.TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        // Act
        await _context.InMemoryTaskEventPublisher.PublishTaskCreatedAsync(task);

        // Assert
        Assert.Single(receivedEvents);
        Assert.Equal("TaskCreated", receivedEvents[0].EventType);
        Assert.Equal(task.Id, receivedEvents[0].TaskId);
        Console.WriteLine("[测试1] TaskCreated事件发布成功");
    }

    /// <summary>
    /// 测试事件发布 - TaskCompleted事件
    /// </summary>
    [Fact]
    public async Task PublishTaskCompletedAsync_Should_NotifySubscribers()
    {
        // Arrange
        var receivedEvents = new List<TaskEvent>();
        _context.InMemoryTaskEventPublisher.Subscribe((e, ct) =>
        {
            receivedEvents.Add(e);
            return Task.CompletedTask;
        });

        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Completed,
            TaskType = Core.Enums.TaskType.General,
            Result = "{\"success\":true}",
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        // Act
        await _context.InMemoryTaskEventPublisher.PublishTaskCompletedAsync(task);

        // Assert
        Assert.Single(receivedEvents);
        Assert.Equal("TaskCompleted", receivedEvents[0].EventType);
        Assert.Equal(task.Id, receivedEvents[0].TaskId);
        Console.WriteLine("[测试2] TaskCompleted事件发布成功");
    }

    /// <summary>
    /// 测试事件发布 - StepTransition事件
    /// </summary>
    [Fact]
    public async Task PublishStepTransitionAsync_Should_NotifySubscribers()
    {
        // Arrange
        var receivedEvents = new List<TaskEvent>();
        _context.InMemoryTaskEventPublisher.Subscribe((e, ct) =>
        {
            receivedEvents.Add(e);
            return Task.CompletedTask;
        });

        var step = new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = Guid.NewGuid(),
            Step = 1,
            StepType = TaskStepType.Development,
            Status = TaskStepStatus.Handling,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        // Act
        await _context.InMemoryTaskEventPublisher.PublishStepTransitionAsync(step, "Pending");

        // Assert
        Assert.Single(receivedEvents);
        Assert.Equal("StepTransition", receivedEvents[0].EventType);
        Assert.Equal(step.TaskId, receivedEvents[0].TaskId);
        Assert.Equal(step.Id, receivedEvents[0].StepId);
        Console.WriteLine("[测试3] StepTransition事件发布成功");
    }

    /// <summary>
    /// 测试事件发布 - ArtifactCreated事件
    /// </summary>
    [Fact]
    public async Task PublishArtifactCreatedAsync_Should_NotifySubscribers()
    {
        // Arrange
        var receivedEvents = new List<TaskEvent>();
        _context.InMemoryTaskEventPublisher.Subscribe((e, ct) =>
        {
            receivedEvents.Add(e);
            return Task.CompletedTask;
        });

        var taskId = Guid.NewGuid();
        var stepId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        // Act
        await _context.InMemoryTaskEventPublisher.PublishArtifactCreatedAsync(taskId, stepId, agentId);

        // Assert
        Assert.Single(receivedEvents);
        Assert.Equal("ArtifactCreated", receivedEvents[0].EventType);
        Assert.Equal(taskId, receivedEvents[0].TaskId);
        Assert.Equal(stepId, receivedEvents[0].StepId);
        Assert.Equal(agentId, receivedEvents[0].AgentId);
        Console.WriteLine("[测试4] ArtifactCreated事件发布成功");
    }

    /// <summary>
    /// 测试事件发布 - Failure事件
    /// </summary>
    [Fact]
    public async Task PublishFailureAsync_Should_NotifySubscribers()
    {
        // Arrange
        var receivedEvents = new List<TaskEvent>();
        _context.InMemoryTaskEventPublisher.Subscribe((e, ct) =>
        {
            receivedEvents.Add(e);
            return Task.CompletedTask;
        });

        var taskId = Guid.NewGuid();
        var stepId = Guid.NewGuid();

        // Act
        await _context.InMemoryTaskEventPublisher.PublishFailureAsync(taskId, stepId, "TestFailure", "Test message");

        // Assert
        Assert.Single(receivedEvents);
        Assert.Equal("Failure", receivedEvents[0].EventType);
        Assert.Equal(taskId, receivedEvents[0].TaskId);
        Assert.Equal(stepId, receivedEvents[0].StepId);
        Console.WriteLine("[测试5] Failure事件发布成功");
    }

    /// <summary>
    /// 测试取消订阅后不再接收事件
    /// </summary>
    [Fact]
    public async Task Unsubscribe_Should_StopReceivingEvents()
    {
        // Arrange
        var receivedEvents = new List<TaskEvent>();
        Func<TaskEvent, CancellationToken, Task> handler = (e, ct) =>
        {
            receivedEvents.Add(e);
            return Task.CompletedTask;
        };

        _context.InMemoryTaskEventPublisher.Subscribe(handler);

        var task1 = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Task 1",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Pending,
            TaskType = Core.Enums.TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        // Act - 第一次发布
        await _context.InMemoryTaskEventPublisher.PublishTaskCreatedAsync(task1);
        
        // 取消订阅
        _context.InMemoryTaskEventPublisher.Unsubscribe(handler);

        var task2 = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Task 2",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Pending,
            TaskType = Core.Enums.TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        // 第二次发布（不应收到）
        await _context.InMemoryTaskEventPublisher.PublishTaskCreatedAsync(task2);

        // Assert
        Assert.Single(receivedEvents);
        Console.WriteLine("[测试6] 取消订阅后不再接收事件");
    }

    #endregion

    #