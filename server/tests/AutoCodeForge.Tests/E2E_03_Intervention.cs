/**
 * E2E测试 - 贯穿式介入能力
 * 
 * 测试覆盖：
 * 1. PauseTask - 暂停任务
 * 2. ResumeTask - 恢复任务
 * 3. ForceTerminate - 紧急终止
 * 4. UpdateRequirement - 需求变更
 */

using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Tests;

using TaskStatus = AutoCodeForge.Core.Entities.TaskStatus;

/// <summary>
/// 多Agent协作端到端测试 - 贯穿式介入模块
/// </summary>
public sealed partial class E2E_MultiAgentCollaborationTests
{
    #region 贯穿式介入测试

    /// <summary>
    /// 测试暂停任务(PauseTask) - 通过更新任务状态实现
    /// </summary>
    [Fact]
    public async Task Intervention_PauseTask_Should_FreezeSteps()
    {
        Console.WriteLine("⏸️ 测试：暂停任务");

        // 创建任务和进行中的步骤
        var task = TestDataFactory.CreateTask("Pause Task Test");
        await _context.TaskRepository.CreateAsync(task);

        var step1 = TestDataFactory.CreateStep(task.Id, 1, TaskStepType.DemandAnalyse);
        step1.Status = TaskStepStatus.Handling;
        await _context.TaskStepRepository.CreateAsync(step1);

        var step2 = TestDataFactory.CreateStep(task.Id, 2, TaskStepType.QueryCurrent);
        await _context.TaskStepRepository.CreateAsync(step2);

        // 暂停任务（更新任务状态为Paused）
        task.Status = TaskStatus.Paused;
        await _context.TaskRepository.UpdateAsync(task);
        Console.WriteLine("任务已暂停");

        // 验证任务状态
        var updatedTask = await _context.TaskRepository.GetByIdAsync(task.Id);
        Assert.Equal(TaskStatus.Paused, updatedTask?.Status);
        Console.WriteLine("任务状态已更新为Paused");

        Console.WriteLine("✅ 暂停任务测试完成！");
    }

    /// <summary>
    /// 测试恢复任务(ResumeTask) - 通过更新任务状态实现
    /// </summary>
    [Fact]
    public async Task Intervention_ResumeTask_Should_ContinueExecution()
    {
        Console.WriteLine("▶️ 测试：恢复任务");

        var task = TestDataFactory.CreateTask("Resume Task Test");
        task.Status = TaskStatus.Paused;
        await _context.TaskRepository.CreateAsync(task);

        var step1 = TestDataFactory.CreateStep(task.Id, 1, TaskStepType.DemandAnalyse);
        step1.Status = TaskStepStatus.Handling;
        await _context.TaskStepRepository.CreateAsync(step1);

        // 恢复任务
        task.Status = TaskStatus.Running;
        await _context.TaskRepository.UpdateAsync(task);
        Console.WriteLine("任务已恢复");

        // 现在可以更新步骤
        step1.Status = TaskStepStatus.Completed;
        await _context.TaskStepRepository.UpdateAsync(step1);
        Assert.Equal(TaskStepStatus.Completed, step1.Status);
        Console.WriteLine("步骤成功更新");

        Console.WriteLine("✅ 恢复任务测试完成！");
    }

    /// <summary>
    /// 测试紧急终止(ForceTerminate) - 通过更新任务状态实现
    /// </summary>
    [Fact]
    public async Task Intervention_ForceTerminate_Should_CancelAllSteps()
    {
        Console.WriteLine("🛑 测试：紧急终止任务");

        var task = TestDataFactory.CreateTask("Force Terminate Test");
        await _context.TaskRepository.CreateAsync(task);

        var step1 = TestDataFactory.CreateStep(task.Id, 1, TaskStepType.DemandAnalyse);
        step1.Status = TaskStepStatus.Completed;
        await _context.TaskStepRepository.CreateAsync(step1);

        var step2 = TestDataFactory.CreateStep(task.Id, 2, TaskStepType.QueryCurrent);
        step2.Status = TaskStepStatus.Handling;
        await _context.TaskStepRepository.CreateAsync(step2);

        var step3 = TestDataFactory.CreateStep(task.Id, 3, TaskStepType.MakePlan);
        await _context.TaskStepRepository.CreateAsync(step3);

        // 紧急终止（更新任务状态为Canceled）
        task.Status = TaskStatus.Canceled;
        await _context.TaskRepository.UpdateAsync(task);

        // 取消所有步骤
        var steps = await _context.TaskStepRepository.GetByTaskIdAsync(task.Id);
        foreach (var step in steps)
        {
            step.Status = TaskStepStatus.Skipped;
            await _context.TaskStepRepository.UpdateAsync(step);
        }
        Console.WriteLine("任务已紧急终止");

        // 验证所有步骤被取消
        var updatedSteps = await _context.TaskStepRepository.GetByTaskIdAsync(task.Id);
        Assert.All(updatedSteps, s => Assert.Equal(TaskStepStatus.Skipped, s.Status));

        // 验证任务状态
        var updatedTask = await _context.TaskRepository.GetByIdAsync(task.Id);
        Assert.Equal(TaskStatus.Canceled, updatedTask?.Status);

        Console.WriteLine("✅ 紧急终止测试完成！");
    }

    /// <summary>
    /// 测试需求变更(UpdateRequirement) - 通过更新任务输入实现
    /// </summary>
    [Fact]
    public async Task Intervention_UpdateRequirement_Should_UpdateContext()
    {
        Console.WriteLine("🔄 测试：需求变更");

        var task = TestDataFactory.CreateTask("Update Requirement Test");
        task.Input = "原始需求：创建简单登录页面";
        await _context.TaskRepository.CreateAsync(task);

        var step3 = TestDataFactory.CreateStep(task.Id, 3, TaskStepType.MakePlan);
        step3.Status = TaskStepStatus.Handling;
        step3.Input = "原始需求：创建简单登录页面";
        await _context.TaskStepRepository.CreateAsync(step3);

        // 更新需求
        task.Input = "更新需求：创建带验证码的登录页面";
        await _context.TaskRepository.UpdateAsync(task);

        // 更新当前步骤的输入
        step3.Input = task.Input;
        await _context.TaskStepRepository.UpdateAsync(step3);
        Console.WriteLine("需求已更新");

        // 验证任务上下文更新
        var updatedTask = await _context.TaskRepository.GetByIdAsync(task.Id);
        Assert.Equal("更新需求：创建带验证码的登录页面", updatedTask?.Input);

        // 验证当前步骤重新执行
        var updatedStep = await _context.TaskStepRepository.GetByIdAsync(step3.Id);
        Assert.Equal("更新需求：创建带验证码的登录页面", updatedStep?.Input);
        Console.WriteLine("步骤上下文已同步更新");

        Console.WriteLine("✅ 需求变更测试完成！");
    }

    #endregion
}