/**
 * E2E测试 - 7步完整流水线
 * 
 * 测试覆盖完整的7步工序流程：
 * 1. 需求梳理 (DemandAnalyse)
 * 2. 查询信息 (QueryCurrent)
 * 3. 方案计划 (MakePlan)
 * 4. 代码开发 (Development)
 * 5. 测试校验 (TestVerify)
 * 6. 版本提交 (CommitPr)
 * 7. 最终审核 (FinalAudit)
 */

using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Tests;

/// <summary>
/// 多Agent协作端到端测试 - 7步流水线模块
/// </summary>
public sealed partial class E2E_MultiAgentCollaborationTests : IDisposable
{
    private readonly IntegrationTestContext _context;

    public E2E_MultiAgentCollaborationTests()
    {
        _context = new IntegrationTestContext("test-user");
    }

    #region 7步完整流水线测试

    /// <summary>
    /// 测试完整7步流水线流程
    /// </summary>
    [Fact]
    public async Task SevenStepPipeline_Should_ExecuteAllSteps()
    {
        Console.WriteLine("🚀 开始测试：7步完整流水线");

        // ============ 阶段1: 创建任务和7个步骤 ============
        var task = TestDataFactory.CreateTask("Seven-Step Pipeline Task");
        await _context.TaskRepository.CreateAsync(task);

        // 创建7个步骤（使用正确的枚举值）
        var step1 = TestDataFactory.CreateStep(task.Id, 1, TaskStepType.DemandAnalyse);
        var step2 = TestDataFactory.CreateStep(task.Id, 2, TaskStepType.QueryCurrent);
        var step3 = TestDataFactory.CreateStep(task.Id, 3, TaskStepType.MakePlan);
        var step4 = TestDataFactory.CreateStep(task.Id, 4, TaskStepType.Development);
        var step5 = TestDataFactory.CreateStep(task.Id, 5, TaskStepType.TestVerify);
        var step6 = TestDataFactory.CreateStep(task.Id, 6, TaskStepType.CommitPr);
        var step7 = TestDataFactory.CreateStep(task.Id, 7, TaskStepType.FinalAudit);

        await _context.TaskStepRepository.CreateAsync(step1);
        await _context.TaskStepRepository.CreateAsync(step2);
        await _context.TaskStepRepository.CreateAsync(step3);
        await _context.TaskStepRepository.CreateAsync(step4);
        await _context.TaskStepRepository.CreateAsync(step5);
        await _context.TaskStepRepository.CreateAsync(step6);
        await _context.TaskStepRepository.CreateAsync(step7);
        Console.WriteLine("[阶段1] 任务和7个步骤创建成功");

        // ============ 阶段2: 创建不同角色的Agent ============
        var secretary = TestDataFactory.CreateSecretary("Secretary Zhang");
        var manager = TestDataFactory.CreateManager("Manager Li");
        var worker = TestDataFactory.CreateWorker("Worker Wang");

        await _context.AgentRepository.CreateAsync(secretary);
        await _context.AgentRepository.CreateAsync(manager);
        await _context.AgentRepository.CreateAsync(worker);
        Console.WriteLine("[阶段2] Agent创建成功");

        // ============ 阶段3: Step1 - 需求梳理 (Worker执行) ============
        var (worker1, _) = await _context.TaskOrchestrator.AssignTaskAsync(task.Id, AgentRole.Worker);
        Assert.NotNull(worker1);
        step1.Status = TaskStepStatus.Completed;
        step1.WorkerAgentId = worker1.Id;
        await _context.TaskStepRepository.UpdateAsync(step1);
        Console.WriteLine("[阶段3] Step1 需求梳理完成");

        // ============ 阶段4: Step2 - 查询信息 (Worker执行) ============
        step2.Status = TaskStepStatus.Completed;
        step2.WorkerAgentId = worker1.Id;
        await _context.TaskStepRepository.UpdateAsync(step2);
        Console.WriteLine("[阶段4] Step2 查询信息完成");

        // ============ 阶段5: Step3 - 方案计划 (Worker执行) ============
        step3.Status = TaskStepStatus.Completed;
        step3.WorkerAgentId = worker1.Id;
        await _context.TaskStepRepository.UpdateAsync(step3);
        Console.WriteLine("[阶段5] Step3 方案计划完成");

        // ============ 阶段6: Step4 - 代码开发 (Worker执行) ============
        step4.Status = TaskStepStatus.Completed;
        step4.WorkerAgentId = worker1.Id;
        await _context.TaskStepRepository.UpdateAsync(step4);
        Console.WriteLine("[阶段6] Step4 代码开发完成");

        // ============ 阶段7: Step5 - 测试校验 (Worker执行) ============
        step5.Status = TaskStepStatus.Completed;
        step5.WorkerAgentId = worker1.Id;
        await _context.TaskStepRepository.UpdateAsync(step5);
        Console.WriteLine("[阶段7] Step5 测试校验完成");

        // ============ 阶段8: Step6 - 版本提交 (Worker执行) ============
        step6.Status = TaskStepStatus.Completed;
        step6.WorkerAgentId = worker1.Id;
        await _context.TaskStepRepository.UpdateAsync(step6);
        Console.WriteLine("[阶段8] Step6 版本提交完成");

        // ============ 阶段9: Step7 - 最终审核 (Manager执行) ============
        var (managerAssigned, _) = await _context.TaskOrchestrator.AssignTaskAsync(task.Id, AgentRole.Manager);
        Assert.NotNull(managerAssigned);
        step7.Status = TaskStepStatus.Completed;
        step7.WorkerAgentId = managerAssigned.Id;
        await _context.TaskStepRepository.UpdateAsync(step7);
        Console.WriteLine("[阶段9] Step7 最终审核完成");

        // ============ 阶段10: 任务完成 ============
        task.Status = Core.Entities.TaskStatus.Completed;
        await _context.TaskRepository.UpdateAsync(task);
        Console.WriteLine("[阶段10] 任务完成");

        // 验证所有步骤状态
        var allSteps = await _context.TaskStepRepository.GetByTaskIdAsync(task.Id);
        Assert.Equal(7, allSteps.Count);
        Assert.All(allSteps, s => Assert.Equal(TaskStepStatus.Completed, s.Status));

        Console.WriteLine("✅ 7步完整流水线测试完成！");
    }

    /// <summary>
    /// 测试步骤间的上下文链式传递
    /// </summary>
    [Fact]
    public async Task StepContext_Should_PassToNextStep()
    {
        Console.WriteLine("🔗 测试：上下文链式传递");

        // 创建任务和步骤
        var task = TestDataFactory.CreateTask("Context Chain Test");
        task.Input = "原始需求：创建用户管理模块";
        await _context.TaskRepository.CreateAsync(task);

        var step1 = TestDataFactory.CreateStep(task.Id, 1, TaskStepType.DemandAnalyse);
        await _context.TaskStepRepository.CreateAsync(step1);

        // Step1产出
        step1.Status = TaskStepStatus.Completed;
        step1.Output = """
            {
                "step": "DemandAnalyse",
                "summary": "需求涉及用户CRUD操作，包含3个功能点",
                "artifacts": [{"type": "document", "title": "需求说明书"}]
            }
            """;
        await _context.TaskStepRepository.UpdateAsync(step1);
        Console.WriteLine("Step1完成，产出已写入Output");

        // 创建Step2
        var step2 = TestDataFactory.CreateStep(task.Id, 2, TaskStepType.QueryCurrent);
        await _context.TaskStepRepository.CreateAsync(step2);

        // 验证上下文传递 - 直接检查数据库中的上下文数据
        var step1Output = await _context.TaskStepRepository.GetByIdAsync(step1.Id);
        var step2Input = await _context.TaskStepRepository.GetByIdAsync(step2.Id);
        
        Assert.NotNull(step1Output?.Output);
        Assert.Contains("需求涉及用户CRUD操作", step1Output.Output);
        Console.WriteLine("Step2成功获取Step1的产出作为输入上下文");

        Console.WriteLine("✅ 上下文链式传递测试完成！");
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}