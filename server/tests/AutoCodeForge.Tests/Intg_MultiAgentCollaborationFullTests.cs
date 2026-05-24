/**
 * 多Agent分层协作系统完整集成测试
 * 
 * 测试覆盖核心业务流程：
 * 1. 任务创建与7步工序初始化
 * 2. Agent角色分配与负载均衡
 * 3. 工序流转与上下文传递
 * 4. HumanGate门控审批流程
 * 5. Manager审核与驳回机制
 * 6. 状态机转换（Idle/Handling/Learning/Dormant）
 * 7. 失败恢复与重试机制
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Application.StateMachines;
using AutoCodeForge.Core.DTOs.Agent;
using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Models;

namespace AutoCodeForge.Tests;

/// <summary>
/// 多Agent分层协作系统完整集成测试
/// </summary>
public sealed class Intg_MultiAgentCollaborationFullTests : IDisposable
{
    private readonly IntegrationTestContext _context;

    public Intg_MultiAgentCollaborationFullTests()
    {
        _context = new IntegrationTestContext("multi-agent-test-user");
    }

    #region 完整业务流程测试

    /// <summary>
    /// 测试完整的任务生命周期：创建→7步工序→审核→完成
    /// </summary>
    [Fact]
    public async Task CompleteTaskLifecycle_Should_WorkCorrectly()
    {
        Console.WriteLine("🚀 开始测试：完整任务生命周期");

        // ============ 阶段1: 创建任务和Agent ============
        Console.WriteLine("[阶段1] 创建任务和Agent角色");
        var task = TestDataFactory.CreateTask("Complete Lifecycle Test");
        await _context.TaskRepository.CreateAsync(task);

        var secretary = TestDataFactory.CreateSecretary("Secretary_Amy");
        var manager = TestDataFactory.CreateManager("Manager_Bob");
        var worker = TestDataFactory.CreateWorker("Worker_Charlie");
        await _context.AgentRepository.CreateAsync(secretary);
        await _context.AgentRepository.CreateAsync(manager);
        await _context.AgentRepository.CreateAsync(worker);

        // ============ 阶段2: 初始化7步工序 ============
        Console.WriteLine("[阶段2] 初始化7步工序");
        await _context.TaskStepService.InitializeStepsAsync(task.Id);
        var steps = await _context.TaskStepRepository.GetByTaskIdAsync(task.Id);
        Assert.Equal(7, steps.Count);
        Assert.Equal(TaskStepStatus.Handling, steps[0].Status);

        // ============ 阶段3: Secretary启动任务 ============
        Console.WriteLine("[阶段3] Secretary启动任务");
        var (assignedSecretary, _) = await _context.TaskOrchestrator.AssignTaskAsync(task.Id, AgentRole.Secretary);
        Assert.NotNull(assignedSecretary);
        Assert.Equal(AgentRole.Secretary, assignedSecretary.Role);

        // 更新第一步WorkerAgentId
        var step1 = steps[0];
        step1.WorkerAgentId = assignedSecretary.Id;
        await _context.TaskStepRepository.UpdateAsync(step1);

        // ============ 阶段4: Worker执行所有步骤 ============
        Console.WriteLine("[阶段4] Worker执行7步工序");
        for (int i = 0; i < 6; i++)
        {
            var currentStep = await _context.TaskStepRepository.GetByIdAsync(steps[i].Id);
            currentStep.Status = TaskStepStatus.Completed;
            currentStep.Output = $"Step {i + 1} completed: {currentStep.StepType}";
            await _context.TaskStepRepository.UpdateAsync(currentStep);
            Console.WriteLine($"  Step{i + 1} ({currentStep.StepType}) 完成");
        }

        // ============ 阶段5: HumanGate审批（方案审批门控） ============
        Console.WriteLine("[阶段5] HumanGate审批流程");
        var planStep = steps[2];
        var gateRequest = new CreateHumanGateRequest
        {
            TaskId = task.Id,
            TaskStepId = planStep.Id,
            GateType = "PlanApproval",
            Reason = "方案需要审批"
        };
        var gateResponse = await _context.HumanGateService.CreateGateAsync(gateRequest);
        Assert.NotNull(gateResponse);
        Assert.Equal(HumanGateStatus.Pending.ToString(), gateResponse.Status);

        // Manager审批通过
        var approveRequest = new ApproveRequest { Comment = "方案审批通过" };
        var approvedResponse = await _context.HumanGateService.ApproveAsync(gateResponse.Id, approveRequest);
        Assert.Equal(HumanGateStatus.Approved.ToString(), approvedResponse.Status);

        // ============ 阶段6: Manager最终审核 ============
        Console.WriteLine("[阶段6] Manager最终审核");
        var finalStep = steps[6];
        finalStep.Status = TaskStepStatus.Handling;
        finalStep.WorkerAgentId = manager.Id;
        await _context.TaskStepRepository.UpdateAsync(finalStep);

        finalStep.Status = TaskStepStatus.Completed;
        await _context.TaskStepRepository.UpdateAsync(finalStep);

        // ============ 阶段7: 任务完成 ============
        Console.WriteLine("[阶段7] 任务完成");
        task.Status = Core.Entities.TaskStatus.Completed;
        await _context.TaskRepository.UpdateAsync(task);

        // ============ 验证 ============
        var allSteps = await _context.TaskStepRepository.GetByTaskIdAsync(task.Id);
        Assert.All(allSteps, s => Assert.Equal(TaskStepStatus.Completed, s.Status));

        var updatedTask = await _context.TaskRepository.GetByIdAsync(task.Id);
        Assert.Equal(Core.Entities.TaskStatus.Completed, updatedTask?.Status);

        Console.WriteLine("✅ 完整任务生命周期测试完成！");
    }

    /// <summary>
    /// 测试Agent状态机完整转换流程
    /// </summary>
    [Fact]
    public async Task AgentStateMachine_Should_TransitionCorrectly()
    {
        Console.WriteLine("🔄 测试：Agent状态机完整转换");

        var agent = TestDataFactory.CreateWorker("StateMachineTestAgent");
        await _context.AgentRepository.CreateAsync(agent);

        var stateMachine = new AgentStateMachine();
        
        await stateMachine.HandleEventAsync(agent, StateEvent.AssignTask);
        await _context.AgentRepository.UpdateAsync(agent);
        var handlingAgent = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Handling, handlingAgent?.State);
        Console.WriteLine("  Idle → Handling ✓");

        await stateMachine.HandleEventAsync(agent, StateEvent.CompleteTask);
        await _context.AgentRepository.UpdateAsync(agent);
        var idleAgent = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Idle, idleAgent?.State);
        Console.WriteLine("  Handling → Idle ✓");

        await stateMachine.HandleEventAsync(agent, StateEvent.StartLearning);
        await _context.AgentRepository.UpdateAsync(agent);
        var learningAgent = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Learning, learningAgent?.State);
        Console.WriteLine("  Idle → Learning ✓");

        await stateMachine.HandleEventAsync(agent, StateEvent.CompleteLearning);
        await _context.AgentRepository.UpdateAsync(agent);
        var idleAfterLearning = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Idle, idleAfterLearning?.State);
        Console.WriteLine("  Learning → Idle ✓");

        await stateMachine.HandleEventAsync(agent, StateEvent.EnterDormant);
        await _context.AgentRepository.UpdateAsync(agent);
        var dormantAgent = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Dormant, dormantAgent?.State);
        Console.WriteLine("  Idle → Dormant ✓");

        await stateMachine.HandleEventAsync(agent, StateEvent.WakeUp);
        await _context.AgentRepository.UpdateAsync(agent);
        var wokeAgent = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Idle, wokeAgent?.State);
        Console.WriteLine("  Dormant → Idle ✓");

        Console.WriteLine("✅ Agent状态机转换测试完成！");
    }

    /// <summary>
    /// 测试上下文链式传递与Token截断机制
    /// </summary>
    [Fact]
    public async Task ContextChain_Should_PassAndTruncateCorrectly()
    {
        Console.WriteLine("🔗 测试：上下文链式传递与Token截断");

        var task = TestDataFactory.CreateTask("Context Chain Test");
        task.Input = "Initial requirement";
        await _context.TaskRepository.CreateAsync(task);

        var steps = TestDataFactory.CreateStepsForTask(task.Id,
            TaskStepType.DemandAnalyse,
            TaskStepType.QueryCurrent,
            TaskStepType.MakePlan);

        foreach (var step in steps)
        {
            await _context.TaskStepRepository.CreateAsync(step);
        }

        var step1 = steps[0];
        step1.Status = TaskStepStatus.Completed;
        step1.Output = new string('x', 10000);
        await _context.TaskStepRepository.UpdateAsync(step1);

        var context = await _context.ContextChainService.BuildStepContextAsync(task.Id, steps[1].Id);
        Assert.NotNull(context);
        Assert.Equal(task.Id, context.TaskId);

        var stepInput = await _context.ContextChainService.BuildStepInputAsync(task.Id, steps[1].Id);
        Assert.NotNull(stepInput);
        
        int maxTokens = 8192;
        Assert.True(stepInput.Length <= maxTokens, 
            $"Step input should be truncated to {maxTokens} tokens, but got {stepInput.Length}");

        Console.WriteLine("✅ 上下文链式传递与Token截断测试完成！");
    }

    /// <summary>
    /// 测试HumanGate门控的创建与审批流程
    /// </summary>
    [Fact]
    public async Task HumanGate_Should_SupportCreateAndApprove()
    {
        Console.WriteLine("🚪 测试：HumanGate门控创建与审批");

        var task = TestDataFactory.CreateTask("HumanGate Control Test");
        await _context.TaskRepository.CreateAsync(task);

        var step = TestDataFactory.CreateStep(task.Id, 1, TaskStepType.DemandAnalyse);
        await _context.TaskStepRepository.CreateAsync(step);

        var gateRequest = new CreateHumanGateRequest
        {
            TaskId = task.Id,
            TaskStepId = step.Id,
            GateType = "RequirementConfirm",
            Reason = "需求确认门控"
        };

        var gate = await _context.HumanGateService.CreateGateAsync(gateRequest);
        Assert.Equal(HumanGateStatus.Pending.ToString(), gate.Status);
        Console.WriteLine("  门控创建成功 ✓");

        var approveRequest = new ApproveRequest { Comment = "审批通过" };
        var approvedGate = await _context.HumanGateService.ApproveAsync(gate.Id, approveRequest);
        Assert.Equal(HumanGateStatus.Approved.ToString(), approvedGate.Status);
        Console.WriteLine("  门控审批通过 ✓");

        Console.WriteLine("✅ HumanGate门控测试完成！");
    }

    /// <summary>
    /// 测试失败恢复与重试机制
    /// </summary>
    [Fact]
    public async Task FailureRecovery_Should_HandleRetryLogic()
    {
        Console.WriteLine("🔧 测试：失败恢复与重试机制");

        var task = TestDataFactory.CreateTask("Failure Recovery Test");
        await _context.TaskRepository.CreateAsync(task);

        var step = TestDataFactory.CreateStep(task.Id, 1, TaskStepType.Development);
        await _context.TaskStepRepository.CreateAsync(step);

        step.Status = TaskStepStatus.Failed;
        step.RetryCount = 1;
        await _context.TaskStepRepository.UpdateAsync(step);

        var recoveryService = _context.FailureRecoveryService;
        var result = await recoveryService.HandleFailureAsync(step.Id, FailureCategory.CodeError, "Simulated step failure");

        Assert.NotNull(result);
        Assert.Equal(RecoveryStatus.Retry, result.Status);

        step.RetryCount = 3;
        await _context.TaskStepRepository.UpdateAsync(step);

        result = await recoveryService.HandleFailureAsync(step.Id, FailureCategory.CodeError, "Simulated step failure");
        Assert.Equal(RecoveryStatus.Degradation, result.Status);

        Console.WriteLine("✅ 失败恢复与重试机制测试完成！");
    }

    /// <summary>
    /// 测试Agent服务状态转换功能
    /// </summary>
    [Fact]
    public async Task AgentService_Should_HandleStateTransitions()
    {
        Console.WriteLine("👤 测试：Agent服务状态转换");

        var agent = TestDataFactory.CreateWorker("StateTransitionAgent");
        await _context.AgentRepository.CreateAsync(agent);

        var assignRequest = new AssignTaskRequest { TaskId = Guid.NewGuid() };
        await _context.AgentService.AssignTaskAsync(agent.Id, assignRequest);
        
        var handlingAgent = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Handling, handlingAgent?.State);
        Console.WriteLine("  分配任务 → Handling ✓");

        await _context.AgentService.CompleteTaskAsync(agent.Id);
        var idleAgent = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Idle, idleAgent?.State);
        Console.WriteLine("  完成任务 → Idle ✓");

        await _context.AgentService.StartLearningAsync(agent.Id);
        var learningAgent = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Learning, learningAgent?.State);
        Console.WriteLine("  开始学习 → Learning ✓");

        var completeLearningRequest = new CompleteLearningRequest { Summary = "学习完成", Result = "学习成果" };
        await _context.AgentService.CompleteLearningAsync(agent.Id, completeLearningRequest);
        var idleAfterLearning = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Idle, idleAfterLearning?.State);
        Console.WriteLine("  完成学习 → Idle ✓");

        var enterDormantRequest = new EnterDormantRequest { Reason = "测试休眠" };
        await _context.AgentService.EnterDormantAsync(agent.Id, enterDormantRequest);
        var dormantAgent = await _context.AgentRepository.GetByIdAsync(agent.Id);
        Assert.Equal(AgentState.Dormant, dormantAgent?.State);
        Console.WriteLine("  进入休眠 → Dormant ✓");

        Console.WriteLine("✅ Agent服务状态转换测试完成！");
    }

    /// <summary>
    /// 测试任务创建与Agent状态关联 - 完整流程：创建任务→分配Agent→执行→完成
    /// </summary>
    [Fact]
    public async Task TaskAndAgentState_Should_BeLinked()
    {
        Console.WriteLine("🔗 测试：任务与Agent状态关联");

        // 1. 创建任务
        var task = TestDataFactory.CreateTask("Task-Agent Link Test");
        await _context.TaskRepository.CreateAsync(task);
        Assert.Equal(Core.Entities.TaskStatus.Pending, task.Status);
        Console.WriteLine("  任务创建成功 ✓");

        // 2. 创建Worker Agent（初始状态Idle）
        var worker = TestDataFactory.CreateWorker("TaskWorker");
        await _context.AgentRepository.CreateAsync(worker);
        Assert.Equal(AgentState.Idle, worker.State);
        Console.WriteLine("  Worker创建成功（状态：Idle） ✓");

        // 3. 创建7步工序
        await _context.TaskStepService.InitializeStepsAsync(task.Id);
        var steps = await _context.TaskStepRepository.GetByTaskIdAsync(task.Id);
        Assert.Equal(7, steps.Count);
        Console.WriteLine("  7步工序初始化 ✓");

        // 4. 通过TaskOrchestrator分配任务给Worker（只增加任务计数，不改变状态）
        var (assignedAgent, _) = await _context.TaskOrchestrator.AssignTaskAsync(task.Id, AgentRole.Worker);
        Assert.NotNull(assignedAgent);
        Assert.Equal(worker.Id, assignedAgent.Id);
        Console.WriteLine("  任务分配给Worker ✓");

        // 5. 通过AgentService更新Agent状态为Handling（实际业务流程）
        var assignRequest = new AssignTaskRequest { TaskId = task.Id };
        await _context.AgentService.AssignTaskAsync(worker.Id, assignRequest);
        Console.WriteLine("  Agent状态更新为Handling ✓");

        // 6. 验证Agent状态变为Handling
        var updatedWorker = await _context.AgentRepository.GetByIdAsync(worker.Id);
        Assert.Equal(AgentState.Handling, updatedWorker?.State);
        Assert.Equal(task.Id, updatedWorker?.CurrentTaskId);
        Console.WriteLine("  Worker状态验证通过 ✓");

        // 7. 更新任务状态为处理中
        task.Status = Core.Entities.TaskStatus.Running;
        await _context.TaskRepository.UpdateAsync(task);

        // 8. Worker执行步骤
        foreach (var step in steps)
        {
            step.Status = TaskStepStatus.Completed;
            step.WorkerAgentId = worker.Id;
            await _context.TaskStepRepository.UpdateAsync(step);
        }
        Console.WriteLine("  所有步骤执行完成 ✓");

        // 9. 完成任务
        await _context.AgentService.CompleteTaskAsync(worker.Id);
        task.Status = Core.Entities.TaskStatus.Completed;
        await _context.TaskRepository.UpdateAsync(task);

        // 10. 验证Agent状态变回Idle
        var finalWorker = await _context.AgentRepository.GetByIdAsync(worker.Id);
        Assert.Equal(AgentState.Idle, finalWorker?.State);
        Assert.Null(finalWorker?.CurrentTaskId);
        Console.WriteLine("  Worker状态变回Idle ✓");

        // 11. 验证任务状态为完成
        var finalTask = await _context.TaskRepository.GetByIdAsync(task.Id);
        Assert.Equal(Core.Entities.TaskStatus.Completed, finalTask?.Status);
        Console.WriteLine("  任务状态变为Completed ✓");

        Console.WriteLine("✅ 任务与Agent状态关联测试完成！");
    }

    /// <summary>
    /// 测试多Agent任务分配 - 不同任务分配给不同角色的Agent
    /// </summary>
    [Fact]
    public async Task MultipleAgents_Should_HandleDifferentTasks()
    {
        Console.WriteLine("👥 测试：多Agent任务分配");

        // 1. 创建多个不同角色的Agent
        var secretary1 = TestDataFactory.CreateSecretary("Secretary_Alice");
        var secretary2 = TestDataFactory.CreateSecretary("Secretary_Bob");
        var manager = TestDataFactory.CreateManager("Manager_Charlie");
        var worker1 = TestDataFactory.CreateWorker("Worker_Dave");
        var worker2 = TestDataFactory.CreateWorker("Worker_Eve");
        
        await _context.AgentRepository.CreateAsync(secretary1);
        await _context.AgentRepository.CreateAsync(secretary2);
        await _context.AgentRepository.CreateAsync(manager);
        await _context.AgentRepository.CreateAsync(worker1);
        await _context.AgentRepository.CreateAsync(worker2);
        Console.WriteLine("  创建5个Agent（2个Secretary, 1个Manager, 2个Worker） ✓");

        // 2. 创建多个任务
        var task1 = TestDataFactory.CreateTask("Task_WebDev");
        var task2 = TestDataFactory.CreateTask("Task_DataAnalysis");
        var task3 = TestDataFactory.CreateTask("Task_Report");
        
        await _context.TaskRepository.CreateAsync(task1);
        await _context.TaskRepository.CreateAsync(task2);
        await _context.TaskRepository.CreateAsync(task3);
        Console.WriteLine("  创建3个任务 ✓");

        // 3. 分配任务1给Secretary
        var (secAssigned1, _) = await _context.TaskOrchestrator.AssignTaskAsync(task1.Id, AgentRole.Secretary);
        Assert.Equal(AgentRole.Secretary, secAssigned1?.Role);
        Console.WriteLine($"  任务1分配给 {secAssigned1?.Name} ✓");

        // 4. 分配任务2给Worker（最小负载策略）
        var (workerAssigned1, _) = await _context.TaskOrchestrator.AssignTaskAsync(task2.Id, AgentRole.Worker);
        Assert.Equal(AgentRole.Worker, workerAssigned1?.Role);
        Console.WriteLine($"  任务2分配给 {workerAssigned1?.Name} ✓");

        // 5. 分配任务3给Worker（应该分配给另一个Worker）
        var (workerAssigned2, _) = await _context.TaskOrchestrator.AssignTaskAsync(task3.Id, AgentRole.Worker);
        Assert.Equal(AgentRole.Worker, workerAssigned2?.Role);
        Assert.NotEqual(workerAssigned1?.Id, workerAssigned2?.Id); // 不同的Worker
        Console.WriteLine($"  任务3分配给 {workerAssigned2?.Name} ✓");

        // 6. 验证负载均衡 - Worker任务数量
        var worker1After = await _context.AgentRepository.GetByIdAsync(worker1.Id);
        var worker2After = await _context.AgentRepository.GetByIdAsync(worker2.Id);
        
        // 一个Worker有1个任务，另一个可能有0或1个任务（取决于负载分配）
        int totalTasks = (worker1After?.CurrentTaskCount ?? 0) + (worker2After?.CurrentTaskCount ?? 0);
        Assert.Equal(2, totalTasks);
        Console.WriteLine("  负载均衡验证通过 ✓");

        // 7. 分配审核任务给Manager
        var (managerAssigned, _) = await _context.TaskOrchestrator.AssignTaskAsync(task1.Id, AgentRole.Manager);
        Assert.Equal(AgentRole.Manager, managerAssigned?.Role);
        Assert.Equal(manager.Id, managerAssigned?.Id);
        Console.WriteLine($"  审核任务分配给 {managerAssigned?.Name} ✓");

        Console.WriteLine("✅ 多Agent任务分配测试完成！");
    }

    /// <summary>
    /// 测试Agent状态与任务状态的同步更新
    /// </summary>
    [Fact]
    public async Task AgentAndTaskStatus_Should_Sync()
    {
        Console.WriteLine("🔄 测试：Agent与任务状态同步");

        // 创建任务和Agent
        var task = TestDataFactory.CreateTask("Sync Test Task");
        await _context.TaskRepository.CreateAsync(task);

        var worker = TestDataFactory.CreateWorker("SyncWorker");
        await _context.AgentRepository.CreateAsync(worker);

        // 初始状态验证
        Assert.Equal(Core.Entities.TaskStatus.Pending, task.Status);
        Assert.Equal(AgentState.Idle, worker.State);
        Console.WriteLine("  初始状态验证 ✓");

        // 分配任务 - Agent变为Handling, 任务变为Processing
        var assignRequest = new AssignTaskRequest { TaskId = task.Id };
        await _context.AgentService.AssignTaskAsync(worker.Id, assignRequest);
        
        task.Status = Core.Entities.TaskStatus.Running;
        await _context.TaskRepository.UpdateAsync(task);

        var updatedWorker = await _context.AgentRepository.GetByIdAsync(worker.Id);
        var updatedTask = await _context.TaskRepository.GetByIdAsync(task.Id);
        
        Assert.Equal(AgentState.Handling, updatedWorker?.State);
        Assert.Equal(Core.Entities.TaskStatus.Running, updatedTask?.Status);
        Console.WriteLine("  分配任务后状态同步 ✓");

        // 任务失败 - Agent变回Idle, 任务变为Failed
        await _context.AgentService.CompleteTaskAsync(worker.Id);
        task.Status = Core.Entities.TaskStatus.Failed;
        await _context.TaskRepository.UpdateAsync(task);

        var failedWorker = await _context.AgentRepository.GetByIdAsync(worker.Id);
        var failedTask = await _context.TaskRepository.GetByIdAsync(task.Id);
        
        Assert.Equal(AgentState.Idle, failedWorker?.State);
        Assert.Equal(Core.Entities.TaskStatus.Failed, failedTask?.Status);
        Console.WriteLine("  任务失败后状态同步 ✓");

        Console.WriteLine("✅ Agent与任务状态同步测试完成！");
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}