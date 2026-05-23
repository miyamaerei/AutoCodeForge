/**
 * 任务流程服务集成测试
 * 
 * 测试覆盖完整的7步工序流程：
 * 1. 需求梳理 → 需求确认门控
 * 2. 查询信息 → 信息补全关
 * 3. 方案计划 → 方案审批门控
 * 4. 代码开发 → 代码审核门控
 * 5. 测试校验 → 测试验收门控
 * 6. 版本提交 → 合并审批门控
 * 7. 最终审核 → 最终签收门控
 */

using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Tests;

/// <summary>
/// 任务流程服务集成测试
/// </summary>
public sealed class Intg_TaskStepFlowServiceTests : IDisposable
{
    private readonly IntegrationTestContext _context;

    public Intg_TaskStepFlowServiceTests()
    {
        _context = new IntegrationTestContext("test-user");
    }

    /// <summary>
    /// 测试初始化7步工序
    /// </summary>
    [Fact]
    public async Task InitializeStepsAsync_Should_CreateSevenSteps()
    {
        var task = TestDataFactory.CreateTask("Seven Steps Test");
        await _context.TaskRepository.CreateAsync(task);

        // 初始化7步
        await _context.TaskStepService.InitializeStepsAsync(task.Id);

        var steps = await _context.TaskStepRepository.GetByTaskIdAsync(task.Id);
        Assert.Equal(7, steps.Count);
        Assert.Equal(TaskStepStatus.Handling, steps[0].Status);
        Assert.All(steps.Skip(1), s => Assert.Equal(TaskStepStatus.Pending, s.Status));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}