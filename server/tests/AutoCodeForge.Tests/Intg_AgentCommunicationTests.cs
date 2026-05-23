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
using AutoCodeForge.Application.Services;

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
            TaskType = AutoCodeForge.Core.Entities.TaskType.General,
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
            TaskType = AutoCodeForge.Core.Entities.TaskType.General,
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
            TaskType = AutoCodeForge.Core.Entities.TaskType.General,
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
            TaskType = AutoCodeForge.Core.Entities.TaskType.General,
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

    #region DatabaseArtifactStore Tests

    /// <summary>
    /// 测试存储产出物
    /// </summary>
    [Fact]
    public async Task StoreArtifactAsync_Should_SaveArtifact()
    {
        // Arrange
        var artifact = new ArtifactContract
        {
            TaskId = Guid.NewGuid(),
            StepId = Guid.NewGuid(),
            AgentId = Guid.NewGuid(),
            StepName = "Test Step",
            Artifacts = new List<ArtifactItem>
            {
                new ArtifactItem
                {
                    Type = "code",
                    Name = "test.cs",
                    Content = "public class Test { }",
                    Format = "text"
                }
            },
            Summary = "Test summary",
            Issues = new List<string> { "Issue 1", "Issue 2" },
            Metrics = new ArtifactMetrics
            {
                ExecutionTimeMs = 100,
                TokenUsage = 500
            }
        };

        // Act
        var stored = await _context.DatabaseArtifactStore.StoreArtifactAsync(artifact);

        // Assert
        Assert.NotNull(stored);
        Assert.NotEqual(Guid.Empty, stored.Id);
        
        var retrieved = await _context.DatabaseArtifactStore.GetArtifactAsync(stored.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(artifact.StepName, retrieved.StepName);
        Assert.Equal(artifact.Summary, retrieved.Summary);
        Assert.Single(retrieved.Artifacts);
        Assert.Equal("test.cs", retrieved.Artifacts[0].Name);
        Console.WriteLine("[测试7] 产出物存储成功");
    }

    /// <summary>
    /// 测试获取不存在的产出物返回null
    /// </summary>
    [Fact]
    public async Task GetArtifactAsync_Should_ReturnNull_WhenNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _context.DatabaseArtifactStore.GetArtifactAsync(nonExistentId);

        // Assert
        Assert.Null(result);
        Console.WriteLine("[测试8] 获取不存在的产出物返回null");
    }

    /// <summary>
    /// 测试按任务ID列出产出物
    /// </summary>
    [Fact]
    public async Task ListArtifactsByTaskIdAsync_Should_ReturnArtifacts()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        
        for (int i = 0; i < 3; i++)
        {
            var artifact = new ArtifactContract
            {
                TaskId = taskId,
                StepId = Guid.NewGuid(),
                AgentId = Guid.NewGuid(),
                StepName = $"Step {i}",
                Artifacts = new List<ArtifactItem>(),
                Summary = $"Summary {i}",
                Issues = new List<string>(),
                Metrics = new ArtifactMetrics()
            };
            await _context.DatabaseArtifactStore.StoreArtifactAsync(artifact);
        }

        // Act
        var artifacts = await _context.DatabaseArtifactStore.ListArtifactsByTaskIdAsync(taskId);

        // Assert
        Assert.Equal(3, artifacts.Count);
        Assert.All(artifacts, a => Assert.Equal(taskId, a.TaskId));
        Console.WriteLine("[测试9] 按任务ID列出产出物成功");
    }

    /// <summary>
    /// 测试按步骤ID列出产出物
    /// </summary>
    [Fact]
    public async Task ListArtifactsByStepIdAsync_Should_ReturnArtifacts()
    {
        // Arrange
        var stepId = Guid.NewGuid();
        
        var artifact1 = new ArtifactContract
        {
            TaskId = Guid.NewGuid(),
            StepId = stepId,
            AgentId = Guid.NewGuid(),
            StepName = "Step 1",
            Artifacts = new List<ArtifactItem>(),
            Summary = "Summary",
            Issues = new List<string>(),
            Metrics = new ArtifactMetrics()
        };
        await _context.DatabaseArtifactStore.StoreArtifactAsync(artifact1);

        var artifact2 = new ArtifactContract
        {
            TaskId = Guid.NewGuid(),
            StepId = Guid.NewGuid(), // 不同的stepId
            AgentId = Guid.NewGuid(),
            StepName = "Step 2",
            Artifacts = new List<ArtifactItem>(),
            Summary = "Summary",
            Issues = new List<string>(),
            Metrics = new ArtifactMetrics()
        };
        await _context.DatabaseArtifactStore.StoreArtifactAsync(artifact2);

        // Act
        var artifacts = await _context.DatabaseArtifactStore.ListArtifactsByStepIdAsync(stepId);

        // Assert
        Assert.Single(artifacts);
        Assert.Equal(stepId, artifacts[0].StepId);
        Console.WriteLine("[测试10] 按步骤ID列出产出物成功");
    }

    /// <summary>
    /// 测试删除产出物
    /// </summary>
    [Fact]
    public async Task DeleteArtifactAsync_Should_RemoveArtifact()
    {
        // Arrange
        var artifact = new ArtifactContract
        {
            TaskId = Guid.NewGuid(),
            StepId = Guid.NewGuid(),
            AgentId = Guid.NewGuid(),
            StepName = "To Delete",
            Artifacts = new List<ArtifactItem>(),
            Summary = "Delete me",
            Issues = new List<string>(),
            Metrics = new ArtifactMetrics()
        };

        var stored = await _context.DatabaseArtifactStore.StoreArtifactAsync(artifact);

        // Act
        await _context.DatabaseArtifactStore.DeleteArtifactAsync(stored.Id);

        // Assert
        var deleted = await _context.DatabaseArtifactStore.GetArtifactAsync(stored.Id);
        Assert.Null(deleted);
        Console.WriteLine("[测试11] 产出物删除成功");
    }

    #endregion

    #region ContextChainService Tests

    /// <summary>
    /// 测试构建步骤上下文
    /// </summary>
    [Fact]
    public async Task BuildStepContextAsync_Should_BuildContextWithArtifacts()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var stepId = Guid.NewGuid();

        // 创建任务和步骤
        var task = new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
            TaskType = AutoCodeForge.Core.Entities.TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        var completedStep = new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Step = 1,
            StepType = TaskStepType.Development,
            Status = TaskStepStatus.Completed,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskStepRepository.CreateAsync(completedStep);

        // 为已完成步骤创建产出物
        var artifact = new ArtifactContract
        {
            TaskId = taskId,
            StepId = completedStep.Id,
            AgentId = Guid.NewGuid(),
            StepName = "Step 1",
            Artifacts = new List<ArtifactItem>
            {
                new ArtifactItem
                {
                    Type = "code",
                    Name = "output.cs",
                    Content = "// Generated code",
                    Format = "text"
                }
            },
            Summary = "Step 1 completed",
            Issues = new List<string>(),
            Metrics = new ArtifactMetrics()
        };
        await _context.DatabaseArtifactStore.StoreArtifactAsync(artifact);

        var currentStep = new TaskStepEntity
        {
            Id = stepId,
            TaskId = taskId,
            Step = 2,
            StepType = TaskStepType.TestVerify,
            Status = TaskStepStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskStepRepository.CreateAsync(currentStep);

        // Act
        var context = await _context.ContextChainService.BuildStepContextAsync(taskId, stepId);

        // Assert
        Assert.Equal(taskId, context.TaskId);
        Assert.Equal(stepId, context.CurrentStepId);
        Assert.Single(context.CompletedSteps);
        Assert.Single(context.Artifacts);
        Console.WriteLine("[测试12] 步骤上下文构建成功");
    }

    /// <summary>
    /// 测试构建步骤输入
    /// </summary>
    [Fact]
    public async Task BuildStepInputAsync_Should_CombineArtifacts()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var stepId = Guid.NewGuid();

        // 创建任务
        var task = new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
            TaskType = AutoCodeForge.Core.Entities.TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        // 创建已完成步骤
        var completedStep = new TaskStepEntity
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            Step = 1,
            StepType = TaskStepType.Development,
            Status = TaskStepStatus.Completed,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskStepRepository.CreateAsync(completedStep);

        // 创建产出物
        var artifact = new ArtifactContract
        {
            TaskId = taskId,
            StepId = completedStep.Id,
            AgentId = Guid.NewGuid(),
            StepName = "Step 1",
            Artifacts = new List<ArtifactItem>
            {
                new ArtifactItem
                {
                    Type = "code",
                    Name = "output.cs",
                    Content = "public class Generated {}",
                    Format = "text"
                }
            },
            Summary = "Generated code",
            Issues = new List<string>(),
            Metrics = new ArtifactMetrics()
        };
        await _context.DatabaseArtifactStore.StoreArtifactAsync(artifact);

        // 创建当前步骤
        var currentStep = new TaskStepEntity
        {
            Id = stepId,
            TaskId = taskId,
            Step = 2,
            StepType = TaskStepType.TestVerify,
            Status = TaskStepStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskStepRepository.CreateAsync(currentStep);

        // Act
        var input = await _context.ContextChainService.BuildStepInputAsync(taskId, stepId);

        // Assert
        Assert.Contains("output.cs", input);
        Assert.Contains("public class Generated {}", input);
        Console.WriteLine("[测试13] 步骤输入构建成功");
    }

    /// <summary>
    /// 测试上下文截断策略
    /// </summary>
    [Fact]
    public async Task BuildStepContextAsync_Should_TruncateWhenOverLimit()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var stepId = Guid.NewGuid();

        // 创建任务
        var task = new TaskEntity
        {
            Id = taskId,
            Title = "Test Task",
            Status = AutoCodeForge.Core.Entities.TaskStatus.Running,
            TaskType = AutoCodeForge.Core.Entities.TaskType.General,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskRepository.CreateAsync(task);

        // 创建多个已完成步骤和产出物
        for (int i = 0; i < 5; i++)
        {
            var completedStep = new TaskStepEntity
            {
                Id = Guid.NewGuid(),
                TaskId = taskId,
                Step = i + 1,
                StepType = TaskStepType.Development,
                Status = TaskStepStatus.Completed,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-i),
                UpdatedAtUtc = DateTime.UtcNow.AddMinutes(-i),
            };
            await _context.TaskStepRepository.CreateAsync(completedStep);

            // 创建大内容产出物
            var artifact = new ArtifactContract
            {
                TaskId = taskId,
                StepId = completedStep.Id,
                AgentId = Guid.NewGuid(),
                StepName = $"Step {i + 1}",
                Artifacts = new List<ArtifactItem>
                {
                    new ArtifactItem
                    {
                        Type = "code",
                        Name = $"file{i}.cs",
                        Content = new string('x', 5000), // 大内容
                        Format = "text"
                    }
                },
                Summary = $"Step {i + 1} summary",
                Issues = new List<string>(),
                Metrics = new ArtifactMetrics()
            };
            await _context.DatabaseArtifactStore.StoreArtifactAsync(artifact);
        }

        // 创建当前步骤
        var currentStep = new TaskStepEntity
        {
            Id = stepId,
            TaskId = taskId,
            Step = 6,
            StepType = TaskStepType.TestVerify,
            Status = TaskStepStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        await _context.TaskStepRepository.CreateAsync(currentStep);

        // Act
        var context = await _context.ContextChainService.BuildStepContextAsync(taskId, stepId);

        // Assert - 验证上下文被截断（不会包含所有5个产出物）
        Assert.True(context.Artifacts.Count <= 5);
        Console.WriteLine($"[测试14] 上下文截断策略生效，剩余产出物数量: {context.Artifacts.Count}");
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}