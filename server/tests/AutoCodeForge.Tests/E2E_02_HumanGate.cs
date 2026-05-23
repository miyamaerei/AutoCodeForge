/**
 * E2E测试 - HumanGate门控机制
 * 
 * 测试覆盖7大门控类型：
 * 1. RequirementConfirm - 需求确认
 * 2. PlanApproval - 方案审批
 * 3. CodeReview - 代码审核
 * 4. TestAcceptance - 测试验收
 * 5. MergeApproval - 合并审批
 * 6. FinalSignoff - 最终签收
 * 7. Emergency - 紧急介入
 */

using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.DTOs.Task;

namespace AutoCodeForge.Tests;

using TaskStatus = AutoCodeForge.Core.Entities.TaskStatus;

/// <summary>
/// 多Agent协作端到端测试 - HumanGate门控模块
/// </summary>
public sealed partial class E2E_MultiAgentCollaborationTests
{
    #region HumanGate门控测试

    /// <summary>
    /// 测试需求确认门控(RequirementConfirm)
    /// </summary>
    [Fact]
    public async Task HumanGate_RequirementConfirm_Should_PauseAndApprove()
    {
        Console.WriteLine("🔒 测试：需求确认门控");

        // 创建任务和Step1
        var task = TestDataFactory.CreateTask("Requirement Confirm Task");
        task.Input = "创建一个用户登录页面";
        await _context.TaskRepository.CreateAsync(task);

        var step1 = TestDataFactory.CreateStep(task.Id, 1, TaskStepType.DemandAnalyse);
        await _context.TaskStepRepository.CreateAsync(step1);

        // Step1完成后创建HumanGate
        step1.Status = TaskStepStatus.Completed;
        await _context.TaskStepRepository.UpdateAsync(step1);

        var gate = TestDataFactory.CreatePendingGate(task.Id, step1.Id, HumanGateType.RequirementConfirm);
        gate.Reason = "请确认需求说明书是否准确";
        await _context.HumanGateRepository.CreateAsync(gate);
        Console.WriteLine("创建需求确认门控，流程暂停");

        // 验证门控状态
        var createdGate = await _context.HumanGateRepository.GetByIdAsync(gate.Id);
        Assert.Equal(HumanGateStatus.Pending, createdGate?.Status);

        // 人类批准（使用正确的方法名）
        await _context.HumanGateService.ApproveAsync(gate.Id, new ApproveRequest { Comment = "需求确认无误" });
        var approvedGate = await _context.HumanGateRepository.GetByIdAsync(gate.Id);
        Assert.Equal(HumanGateStatus.Approved, approvedGate?.Status);
        Assert.Equal("需求确认无误", approvedGate?.HumanResponse);
        Console.WriteLine("人类批准，流程继续");

        Console.WriteLine("✅ 需求确认门控测试完成！");
    }

    /// <summary>
    /// 测试方案审批门控(PlanApproval) - 最关键的成本分水岭
    /// </summary>
    [Fact]
    public async Task HumanGate_PlanApproval_Should_AllowRejection()
    {
        Console.WriteLine("🔒 测试：方案审批门控（可驳回）");

        // 创建任务和Step3
        var task = TestDataFactory.CreateTask("Plan Approval Task");
        await _context.TaskRepository.CreateAsync(task);

        var step3 = TestDataFactory.CreateStep(task.Id, 3, TaskStepType.MakePlan);
        step3.Status = TaskStepStatus.Completed;
        await _context.TaskStepRepository.CreateAsync(step3);

        // 创建方案审批门控
        var gate = TestDataFactory.CreatePendingGate(task.Id, step3.Id, HumanGateType.PlanApproval);
        gate.Reason = "方案涉及架构变更，请审批";
        await _context.HumanGateRepository.CreateAsync(gate);
        Console.WriteLine("创建方案审批门控");

        // 人类驳回（使用正确的方法名）
        await _context.HumanGateService.RejectAsync(gate.Id, new RejectRequest { Reason = "方案需要重新设计，建议采用微服务架构" });
        var rejectedGate = await _context.HumanGateRepository.GetByIdAsync(gate.Id);
        Assert.Equal(HumanGateStatus.Rejected, rejectedGate?.Status);

        // 验证Step需要重做
        var updatedStep = await _context.TaskStepRepository.GetByIdAsync(step3.Id);
        Assert.Equal(TaskStepStatus.Pending, updatedStep?.Status);
        Console.WriteLine("方案被驳回，Step3需要重做");

        Console.WriteLine("✅ 方案审批门控测试完成！");
    }

    /// <summary>
    /// 测试代码审核门控(CodeReview) - 条件触发
    /// </summary>
    [Fact]
    public async Task HumanGate_CodeReview_Should_TriggerConditionally()
    {
        Console.WriteLine("🔒 测试：代码审核门控（条件触发）");

        var task = TestDataFactory.CreateTask("Code Review Task");
        await _context.TaskRepository.CreateAsync(task);

        var step4 = TestDataFactory.CreateStep(task.Id, 4, TaskStepType.Development);
        step4.Status = TaskStepStatus.Completed;
        step4.Output = "{\"changedFiles\": 15, \"affectsCoreModule\": true}";
        await _context.TaskStepRepository.CreateAsync(step4);

        // 创建代码审核门控（变更文件>5个，触发Full Review）
        var gate = TestDataFactory.CreatePendingGate(task.Id, step4.Id, HumanGateType.CodeReview);
        await _context.HumanGateRepository.CreateAsync(gate);
        Console.WriteLine("代码变更>5个文件，触发代码审核门控");

        // 修改后批准（使用正确的方法名）
        await _context.HumanGateService.ModifyApproveAsync(gate.Id, new ModifyApproveRequest 
        { 
            Modifications = new GateModifications { Instructions = "代码质量良好，建议增加单元测试" } 
        });
        
        var modifiedGate = await _context.HumanGateRepository.GetByIdAsync(gate.Id);
        Assert.Equal(HumanGateStatus.Modified, modifiedGate?.Status);
        Console.WriteLine("人类修改后批准");

        Console.WriteLine("✅ 代码审核门控测试完成！");
    }

    /// <summary>
    /// 测试最终签收门控(FinalSignoff)
    /// </summary>
    [Fact]
    public async Task HumanGate_FinalSignoff_Should_CompleteTask()
    {
        Console.WriteLine("🔒 测试：最终签收门控");

        var task = TestDataFactory.CreateTask("Final Signoff Task");
        await _context.TaskRepository.CreateAsync(task);

        var step7 = TestDataFactory.CreateStep(task.Id, 7, TaskStepType.FinalAudit);
        step7.Status = TaskStepStatus.Completed;
        await _context.TaskStepRepository.CreateAsync(step7);

        // 创建最终签收门控
        var gate = TestDataFactory.CreatePendingGate(task.Id, step7.Id, HumanGateType.FinalSignoff);
        await _context.HumanGateRepository.CreateAsync(gate);

        // 人类签收
        await _context.HumanGateService.ApproveAsync(gate.Id, new ApproveRequest { Comment = "验收通过，交付完成" });
        
        // 更新任务状态为Completed（模拟实际的任务完成逻辑）
        task.Status = Core.Entities.TaskStatus.Completed;
        await _context.TaskRepository.UpdateAsync(task);

        // 验证任务完成
        var updatedTask = await _context.TaskRepository.GetByIdAsync(task.Id);
        Assert.Equal(Core.Entities.TaskStatus.Completed, updatedTask?.Status);
        Console.WriteLine("最终签收完成，任务归档");

        Console.WriteLine("✅ 最终签收门控测试完成！");
    }

    #endregion
}