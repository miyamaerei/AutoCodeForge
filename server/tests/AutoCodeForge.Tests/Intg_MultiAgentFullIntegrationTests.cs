/**
 * 多Agent分层协作系统完整集成测试
 * 
 * 测试覆盖：
 * 1. Agent注册与初始化流程
 * 2. 任务创建与7步工序流转
 * 3. Agent间通信与事件发布
 * 4. 任务分配与负载均衡
 * 5. HumanGate门控审批流程
 * 6. Agent状态机与生命周期管理
 * 7. 角色分配正确性验证
 * 
 * 参考现有测试：
 * - Intg_AgentServiceTests - Agent生命周期管理
 * - Intg_AgentCommunicationTests - Agent间通信
 * - Intg_AgentRegistrationTests - Agent注册
 */

using AutoCodeForge.Application.Services;
using AutoCodeForge.Core.DTOs.Agent;
using AutoCodeForge.Core.DTOs.Task;
using AutoCodeForge.Core.Entities;
using AutoCodeForge.Core.Interfaces;
using AutoCodeForge.Core.Models;
using AutoCodeForge.Infrastructure.Repositories;

namespace AutoCodeForge.Tests;

/// <summary>
/// 多Agent分层协作系统完整集成测试
/// </summary>
public sealed class Intg_MultiAgentFullIntegrationTests : IDisposable
{
    private readonly IntegrationTestContext _context;
    private readonly IAgentRegistryService _agentRegistryService;
    private readonly TaskStepFlowService _taskStepFlowService;

    public Intg_MultiAgentFullIntegrationTests()
    {
        _context = new IntegrationTestContext("multi-agent-full-integration");
        _agentRegistryService = new AgentRegistryService(
            _context.AgentRegistrationRepository,
            _context.AgentRepository,
            _context.InMemoryTaskEventPublisher);
        
        var taskReviewService = new TaskReviewService(
            new TaskReviewRepository(_context.Db, _context.CurrentUser),
            _context.TaskStepRepository,
            _context.AgentRepository);
        
        _taskStepFlowService = new TaskStepFlowService(
            _context.TaskStepRepository,
            _context.TaskRepository,
            taskReviewService,
            _context.HumanGateService);
    }

    #region Agent注册与初始化

    [Fact]
    public async Task AgentRegistration_FullFlow_Should_Work()
    {
        Console.WriteLine("[测试1] Agent注册完整流程");

        var agent = TestDataFactory.CreateWorker("RegistrationTestAgent");
        await _context.AgentRepository.CreateAsync(agent);
        Console.WriteLine("  创建Agent ✓");

        var registration = await _agentRegistryService.RegisterAgentAsync(
            agent.Id, "server-001", "instance-001");
        Assert.Equal(AgentRegistrationStatus.Online, registration.Status);
        Console.WriteLine("  注册Agent ✓");

        await _agentRegistryService.RenewHeartbeatAsync(agent.Id);
        var updatedRegistration = await _context.AgentRegistrationRepository.GetByAgentIdAsync(agent.Id);
        Assert.NotNull(updatedRegistration);
        Assert.Equal(AgentRegistrationStatus.Online, updatedRegistration.Status);
        Console.WriteLine("  心跳续约 ✓");

        var availableAgents = await _agentRegistryService.GetAvailableAgentsAsync();
        Assert.Contains(availableAgents, a => a.AgentId == agent.Id);
        Console.WriteLine("  获取可用Agent ✓");

        await _agentRegistryService.DeregisterAgentAsync(agent.Id);
        var unregistered = await _context.AgentRegistrationRepository.GetByAgentIdAsync(agent.Id);
        Assert.Equal(AgentRegistrationStatus.Offline, unregistered?.Status);
        Console.WriteLine("  注销Agent ✓");

        Console.WriteLine("[测试1] ✓ Agent注册完整流程测试完成");
    }

    #endregion

    #region 任务创建与7步工序

    [Fact]
    public async Task TaskCreation_WithSevenSteps_Should_Work()
    {
        Console.WriteLine("[测试2] 任务创建与7步工序");

        var task = TestDataFactory.CreateTask("Seven Step Task");
        await _context.TaskRepository.CreateAsync(task);
        Console.WriteLine("  创建任务 ✓");

        await _context.TaskStepService.InitializeStepsAsync(task.Id);
        var steps = await _context.TaskStepRepository.GetByTaskIdAsync(task.Id);
        Assert.Equal(7, steps.Count);
        
        var expectedSteps = new[] 
        { 
            TaskStepType.DemandAnalyse, 
            TaskStepType.QueryCurrent, 
            TaskStepType.MakePlan,
            TaskStepType.Development,
            TaskStepType.TestVerify,
            TaskStepType.CommitPr,
            TaskStepType.FinalAudit
        };
        
        for (int i = 0; i < 7; i++)
        {
            Assert.Equal(expectedSteps[i], steps[i].StepType);
            Assert.Equal(i + 1, steps[i].Step);
            Assert.Equal(i == 0 ? TaskStepStatus.Handling : TaskStepStatus.Pending, steps[i].Status);
        }
        Console.WriteLine("  7步工序初始化 ✓");

        var firstStep = steps[0];
        await _taskStepFlowService.MoveToNextStepAsync(task.Id, firstStep.Id);
        
        var updatedFirstStep = await _context.TaskStepRepository.GetByIdAsync(firstStep.Id);
        Assert.Equal(TaskStepStatus.Completed, updatedFirstStep.Status);
        
        var secondStep = await _context.TaskStepRepository.GetByIdAsync(steps[1].Id);
        Assert.Equal(TaskStepStatus.Handling, secondStep.Status);
        Console.WriteLine("  工序流转 ✓");

        Console.WriteLine("[测试2] ✓ 任务创建与7步工序测试完成");
    }

    #endregion

    #region Agent间通信与事件发布

    [Fact]
    public async Task AgentCommunication_ThroughEventSystem_Should_Work()
    {
        Console.WriteLine("[测试3] Agent间通信与事件发布");

        var secretary = TestDataFactory.CreateSecretary("CommSec");
        var worker = TestDataFactory.CreateWorker("CommWorker");
        await _context.AgentRepository.CreateAsync(secretary);
        await _context.AgentRepository.CreateAsync(worker);
        Console.WriteLine("  创建Agent ✓");

        var task = TestDataFactory.CreateTask("Communication Task");
        await _context.TaskRepository.CreateAsync(task);
        await _context.TaskStepService.InitializeStepsAsync(task.Id);
        Console.WriteLine("  创建任务 ✓");

        var receivedEvents = new List<TaskEvent>();
        _context.InMemoryTaskEventPublisher.Subscribe((e, ct) =>
        {
            receivedEvents.Add(e);
            return Task.CompletedTask;
        });
        Console.WriteLine("  订阅事件 ✓");

        await _context.InMemoryTaskEventPublisher.PublishTaskCreatedAsync(task);
        Assert.Single(receivedEvents);
        Assert.Equal("TaskCreated", receivedEvents[0].EventType);
        Console.WriteLine("  发布TaskCreated事件 ✓");

        var steps = await _context.TaskStepRepository.GetByTaskIdAsync(task.Id);
        var step = steps.First();
        step.Status = TaskStepStatus.Completed;
        await _context.TaskStepRepository.UpdateAsync(step);
        await _context.InMemoryTaskEventPublisher.PublishStepTransitionAsync(step, "Pending");
        Assert.Equal(2, receivedEvents.Count);
        Assert.Equal("StepTransition", receivedEvents[1].EventType);
        Console.WriteLine("  发布StepTransition事件 ✓");

        await _context.InMemoryTaskEventPublisher.PublishArtifactCreatedAsync(task.Id, step.Id, worker.Id);
        Assert.Equal(3, receivedEvents.Count);
        Assert.Equal("ArtifactCreated", receivedEvents[2].EventType);
        Console.WriteLine("  发布ArtifactCreated事件 ✓");

        Console.WriteLine("[测试3] ✓ Agent间通信与事件发布测试完成");
    }

    #endregion

    #region 任务分配与负载均衡

    [Fact]
    public async Task TaskAssignment_WithLoadBalancing_Should_Work()
    {
        Console.WriteLine("[测试4] 任务分配与负载均衡");

        var worker1 = TestDataFactory.CreateWorker("LoadWorker1");
        var worker2 = TestDataFactory.CreateWorker("LoadWorker2");
        var worker3 = TestDataFactory.CreateWorker("LoadWorker3");
        await _context.AgentRepository.CreateAsync(worker1);
        await _context.AgentRepository.CreateAsync(worker2);
        await _context.AgentRepository.CreateAsync(worker3);
        Console.WriteLine("  创建3个Worker ✓");

        var tasks = new List<TaskEntity>();
        for (int i = 0; i < 5; i++)
        {
            var task = TestDataFactory.CreateTask($"LoadTask{i + 1}");
            await _context.TaskRepository.CreateAsync(task);
            await _context.TaskStepService.InitializeStepsAsync(task.Id);
            tasks.Add(task);
        }
        Console.WriteLine("  创建5个任务 ✓");

        var assignments = new List<AgentEntity?>();
        foreach (var task in tasks)
        {
            var (agent, _) = await _context.TaskOrchestrator.AssignTaskAsync(task.Id, AgentRole.Worker);
            assignments.Add(agent);
        }

        var worker1Tasks = assignments.Count(a => a?.Id == worker1.Id);
        var worker2Tasks = assignments.Count(a => a?.Id == worker2.Id);
        var worker3Tasks = assignments.Count(a => a?.Id == worker3.Id);

        var maxTasks = Math.Max(Math.Max(worker1Tasks, worker2Tasks), worker3Tasks);
        var minTasks = Math.Min(Math.Min(worker1Tasks, worker2Tasks), worker3Tasks);
        Assert.True(maxTasks - minTasks <= 1, "负载均衡应确保任务分配均匀");
        Console.WriteLine($"  负载均衡验证: Worker1={worker1Tasks}, Worker2={worker2Tasks}, Worker3={worker3Tasks} ✓");

        Console.WriteLine("[测试4] ✓ 任务分配与负载均衡测试完成");
    }

    #endregion

    #region HumanGate门控审批流程

    [Fact]
    public async Task HumanGate_FullApprovalFlow_Should_Work()
    {
        Console.WriteLine("[测试5] HumanGate门控审批流程");

        var task = TestDataFactory.CreateTask("GateTask");
        await _context.TaskRepository.CreateAsync(task);
        await _context.TaskStepService.InitializeStepsAsync(task.Id);
        
        var steps = await _context.TaskStepRepository.GetByTaskIdAsync(task.Id);
        var planStep = steps.First(s => s.StepType == TaskStepType.MakePlan);
        Console.WriteLine("  创建任务和步骤 ✓");

        var manager = TestDataFactory.CreateManager("GateManager");
        await _context.AgentRepository.CreateAsync(manager);
        Console.WriteLine("  创建Manager ✓");

        var createRequest = new CreateHumanGateRequest
        {
            TaskId = task.Id,
            TaskStepId = planStep.Id,
            GateType = "PlanApproval",
            Reason = "方案需要审批"
        };
        var gate = await _context.HumanGateService.CreateGateAsync(createRequest);
        Assert.Equal(HumanGateStatus.Pending.ToString(), gate.Status);
        Console.WriteLine("  创建HumanGate ✓");

        var approveRequest = new ApproveRequest { Comment = "方案审批通过" };
        var approvedGate = await _context.HumanGateService.ApproveAsync(gate.Id, approveRequest);
        Assert.Equal(HumanGateStatus.Approved.ToString(), approvedGate.Status);
        Console.WriteLine("  Manager审批通过 ✓");

        planStep.Status = TaskStepStatus.Completed;
        await _context.TaskStepRepository.UpdateAsync(planStep);
        Console.WriteLine("  更新步骤状态 ✓");

        Console.WriteLine("[测试5] ✓ HumanGate门控审批流程测试完成");
    }

    #endregion

    #region Agent状态机与生命周期

    [Fact]
    public async Task AgentLifecycle_FullStateTransitions_Should_Work()
    {
        Console.WriteLine("[测试6] Agent完整生命周期");

        var agent = await _context.CreateTestAgentWithLifecycleAsync(
            name: "LifecycleAgent",
            state: AgentState.Idle);
        Console.WriteLine("  创建Agent (Idle) ✓");

        var assignResult = await _context.AgentService.AssignTaskAsync(agent.Id, 
            new AssignTaskRequest { TaskId = Guid.NewGuid() });
        Assert.Equal(AgentState.Handling.ToString(), assignResult.State);
        Console.WriteLine("  分配任务 (Idle → Handling) ✓");

        var completeResult = await _context.AgentService.CompleteTaskAsync(agent.Id);
        Assert.Equal(AgentState.Idle.ToString(), completeResult.State);
        Console.WriteLine("  完成任务 (Handling → Idle) ✓");

        var learnResult = await _context.AgentService.StartLearningAsync(agent.Id);
        Assert.Equal(AgentState.Learning.ToString(), learnResult.State);
        Console.WriteLine("  开始学习 (Idle → Learning) ✓");

        var completeLearnResult = await _context.AgentService.CompleteLearningAsync(agent.Id,
            new CompleteLearningRequest { Summary = "学习完成" });
        Assert.Equal(AgentState.Idle.ToString(), completeLearnResult.State);
        Console.WriteLine("  完成学习 (Learning → Idle) ✓");

        var dormantResult = await _context.AgentService.EnterDormantAsync(agent.Id,
            new EnterDormantRequest { Reason = "空闲超时" });
        Assert.Equal(AgentState.Dormant.ToString(), dormantResult.State);
        Console.WriteLine("  进入休眠 (Idle → Dormant) ✓");

        var wakeResult = await _context.AgentService.WakeUpAsync(agent.Id);
        Assert.Equal(AgentState.Idle.ToString(), wakeResult.State);
        Console.WriteLine("  唤醒 (Dormant → Idle) ✓");

        Console.WriteLine("[测试6] ✓ Agent完整生命周期测试完成");
    }

    #endregion

    #region 角色分配正确性验证

    [Fact]
    public async Task RoleAssignment_Should_Match_StepType()
    {
        Console.WriteLine("[测试7] 角色分配正确性验证");

        var secretary = TestDataFactory.CreateSecretary("RoleSec");
        var manager = TestDataFactory.CreateManager("RoleManager");
        var worker = TestDataFactory.CreateWorker("RoleWorker");
        await _context.AgentRepository.CreateAsync(secretary);
        await _context.AgentRepository.CreateAsync(manager);
        await _context.AgentRepository.CreateAsync(worker);
        Console.WriteLine("  创建三种角色Agent ✓");

        var task = TestDataFactory.CreateTask("RoleAssignmentTask");
        await _context.TaskRepository.CreateAsync(task);
        await _context.TaskStepService.InitializeStepsAsync(task.Id);
        Console.WriteLine("  创建任务和7步工序 ✓");

        var steps = await _context.TaskStepRepository.GetByTaskIdAsync(task.Id);

        var stepRoleMap = new Dictionary<TaskStepType, AgentRole>
        {
            { TaskStepType.DemandAnalyse, AgentRole.Secretary },
            { TaskStepType.QueryCurrent, AgentRole.Secretary },
            { TaskStepType.MakePlan, AgentRole.Manager },
            { TaskStepType.Development, AgentRole.Worker },
            { TaskStepType.TestVerify, AgentRole.Worker },
            { TaskStepType.CommitPr, AgentRole.Worker },
            { TaskStepType.FinalAudit, AgentRole.Manager }
        };

        foreach (var step in steps)
        {
            var expectedRole = stepRoleMap[step.StepType];
            var (assignedAgent, _) = await _context.TaskOrchestrator.AssignTaskAsync(task.Id, expectedRole);
            
            Assert.NotNull(assignedAgent);
            Assert.Equal(expectedRole, assignedAgent.Role);
            Console.WriteLine($"  Step {step.Step} ({step.StepType}) → {expectedRole} ✓");
        }

        Console.WriteLine("[测试7] ✓ 角色分配正确性验证完成");
    }

    #endregion

    #region 端到端完整流程

    [Fact]
    public async Task EndToEnd_MultiAgentCollaboration_Should_Work()
    {
        Console.WriteLine("[测试8] 端到端多Agent协作流程");

        var secretary = TestDataFactory.CreateSecretary("E2E_Secretary");
        var manager = TestDataFactory.CreateManager("E2E_Manager");
        var worker1 = TestDataFactory.CreateWorker("E2E_Worker1");
        var worker2 = TestDataFactory.CreateWorker("E2E_Worker2");
        await _context.AgentRepository.CreateAsync(secretary);
        await _context.AgentRepository.CreateAsync(manager);
        await _context.AgentRepository.CreateAsync(worker1);
        await _context.AgentRepository.CreateAsync(worker2);
        Console.WriteLine("[阶段1] 创建4个角色Agent ✓");

        var task = TestDataFactory.CreateTask("E2E_FullFlow_Task");
        await _context.TaskRepository.CreateAsync(task);
        await _context.TaskStepService.InitializeStepsAsync(task.Id);
        Console.WriteLine("[阶段2] 创建任务和7步工序 ✓");

        var steps = await _context.TaskStepRepository.GetByTaskIdAsync(task.Id);

        await ProcessStepWithCorrectRole(task.Id, steps[0], AgentRole.Secretary);
        Console.WriteLine("[阶段3] Secretary完成需求梳理 (DemandAnalyse) ✓");

        await ProcessStepWithCorrectRole(task.Id, steps[1], AgentRole.Secretary);
        Console.WriteLine("[阶段4] Secretary完成查询当前 (QueryCurrent) ✓");

        await ProcessStepWithCorrectRole(task.Id, steps[2], AgentRole.Manager);
        Console.WriteLine("[阶段5] Manager完成方案计划 (MakePlan) ✓");

        await ProcessStepWithCorrectRole(task.Id, steps[3], AgentRole.Worker);
        Console.WriteLine("[阶段6] Worker完成代码开发 (Development) ✓");

        await ProcessStepWithCorrectRole(task.Id, steps[4], AgentRole.Worker);
        Console.WriteLine("[阶段7] Worker完成测试校验 (TestVerify) ✓");

        await ProcessStepWithCorrectRole(task.Id, steps[5], AgentRole.Worker);
        Console.WriteLine("[阶段8] Worker完成版本提交 (CommitPr) ✓");

        await ProcessStepWithCorrectRole(task.Id, steps[6], AgentRole.Manager);
        Console.WriteLine("[阶段9] Manager完成最终审核 (FinalAudit) ✓");

        task.Status = Core.Entities.TaskStatus.Completed;
        await _context.TaskRepository.UpdateAsync(task);
        Console.WriteLine("[阶段10] 任务完成 ✓");

        var allSteps = await _context.TaskStepRepository.GetByTaskIdAsync(task.Id);
        Assert.All(allSteps, s => Assert.Equal(TaskStepStatus.Completed, s.Status));
        
        var finalTask = await _context.TaskRepository.GetByIdAsync(task.Id);
        Assert.Equal(Core.Entities.TaskStatus.Completed, finalTask?.Status);

        Console.WriteLine("[测试8] ✓ 端到端多Agent协作流程测试完成");
    }

    private async Task ProcessStepWithCorrectRole(Guid taskId, TaskStepEntity step, AgentRole expectedRole)
    {
        var (agent, _) = await _context.TaskOrchestrator.AssignTaskAsync(taskId, expectedRole);
        Assert.NotNull(agent);
        Assert.Equal(expectedRole, agent.Role);

        await _context.AgentService.AssignTaskAsync(agent.Id, new AssignTaskRequest { TaskId = taskId });
        
        step.Status = TaskStepStatus.Completed;
        step.WorkerAgentId = agent.Id;
        await _context.TaskStepRepository.UpdateAsync(step);
        
        await _context.AgentService.CompleteTaskAsync(agent.Id);
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context.Dispose();
    }

    #endregion
}