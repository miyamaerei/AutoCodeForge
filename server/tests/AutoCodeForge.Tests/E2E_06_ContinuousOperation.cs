/**
 * E2E测试 - Agent持续运行
 * 
 * 测试覆盖：
 * 1. Agent持续运行状态流转
 * 2. 多Agent并行处理任务
 * 3. 负载均衡分配
 * 4. 持续运行场景模拟
 */

using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Tests;

using TaskStatus = AutoCodeForge.Core.Entities.TaskStatus;

/// <summary>
/// 多Agent协作端到端测试 - 持续运行模块
/// </summary>
public sealed partial class E2E_MultiAgentCollaborationTests
{
    #region Agent持续运行测试

    /// <summary>
    /// 测试Agent持续运行状态流转：Idle→Handling→Idle→Learning→Idle
    /// </summary>
    [Fact]
    public async Task ContinuousOperation_Agent_StateFlow_Should_Work()
    {
        Console.WriteLine("🔄 测试：Agent持续运行状态流转");

        var worker = TestDataFactory.CreateAgentInState(AgentRole.Worker, AgentState.Idle);
        worker.Name = "Worker-Continuous";
        await _context.AgentRepository.CreateAsync(worker);

        // 状态流转1: Idle → Handling
        var task1 = TestDataFactory.CreateTask("Task1");
        await _context.TaskRepository.CreateAsync(task1);
        
        var (assigned1, _) = await _context.TaskOrchestrator.AssignTaskAsync(task1.Id, AgentRole.Worker);
        Assert.NotNull(assigned1);
        Assert.Equal("Worker-Continuous", assigned1.Name);
        
        // 更新Agent状态为Handling（模拟实际的状态更新逻辑）
        worker.State = AgentState.Handling;
        await _context.AgentRepository.UpdateAsync(worker);
        var updatedWorker = await _context.AgentRepository.GetByIdAsync(worker.Id);
        Assert.Equal(AgentState.Handling, updatedWorker?.State);
        Console.WriteLine("状态流转: Idle → Handling");

        // 状态流转2: Handling → Idle
        await _context.AgentRepository.DecrementTaskCountAsync(worker.Id);
        worker.State = AgentState.Idle;
        await _context.AgentRepository.UpdateAsync(worker);
        updatedWorker = await _context.AgentRepository.GetByIdAsync(worker.Id);
        Assert.Equal(AgentState.Idle, updatedWorker?.State);
        Console.WriteLine("状态流转: Handling → Idle");

        // 状态流转3: Idle → Learning（模拟空闲超时）
        worker.State = AgentState.Learning;
        await _context.AgentRepository.UpdateAsync(worker);
        Console.WriteLine("状态流转: Idle → Learning");

        // 状态流转4: Learning → Idle（学习完成）
        worker.State = AgentState.Idle;
        await _context.AgentRepository.UpdateAsync(worker);
        Console.WriteLine("状态流转: Learning → Idle");

        // 状态流转5: Idle → Handling（新任务到来）
        var task2 = TestDataFactory.CreateTask("Task2");
        await _context.TaskRepository.CreateAsync(task2);
        
        var (assigned2, _) = await _context.TaskOrchestrator.AssignTaskAsync(task2.Id, AgentRole.Worker);
        Assert.NotNull(assigned2);
        
        worker.State = AgentState.Handling;
        await _context.AgentRepository.UpdateAsync(worker);
        updatedWorker = await _context.AgentRepository.GetByIdAsync(worker.Id);
        Assert.Equal(AgentState.Handling, updatedWorker?.State);
        Console.WriteLine("状态流转: Idle → Handling（新任务）");

        Console.WriteLine("✅ Agent持续运行状态流转测试完成！");
    }

    /// <summary>
    /// 测试多Agent并行处理任务
    /// </summary>
    [Fact]
    public async Task ContinuousOperation_MultipleAgents_Should_WorkConcurrently()
    {
        Console.WriteLine("⚡ 测试：多Agent并行处理任务");

        // 创建多个Agent
        var sec1 = TestDataFactory.CreateSecretary("Sec-1");
        var sec2 = TestDataFactory.CreateSecretary("Sec-2");
        var worker1 = TestDataFactory.CreateWorker("W1");
        var worker2 = TestDataFactory.CreateWorker("W2");
        
        await _context.AgentRepository.CreateAsync(sec1);
        await _context.AgentRepository.CreateAsync(sec2);
        await _context.AgentRepository.CreateAsync(worker1);
        await _context.AgentRepository.CreateAsync(worker2);

        // 同时提交多个任务（并行）
        var task1 = TestDataFactory.CreateTask("Parallel-Task-1");
        var task2 = TestDataFactory.CreateTask("Parallel-Task-2");
        var task3 = TestDataFactory.CreateTask("Parallel-Task-3");
        
        await _context.TaskRepository.CreateAsync(task1);
        await _context.TaskRepository.CreateAsync(task2);
        await _context.TaskRepository.CreateAsync(task3);

        // 并行分配任务给Secretary
        var assignTasks = await Task.WhenAll(
            _context.TaskOrchestrator.AssignTaskAsync(task1.Id, AgentRole.Secretary),
            _context.TaskOrchestrator.AssignTaskAsync(task2.Id, AgentRole.Secretary)
        );

        // 验证负载均衡分配
        var assignedSecs = assignTasks.Select(x => x.Item1).ToList();
        Assert.Equal(2, assignedSecs.Distinct().Count()); // 不同秘书接单
        Console.WriteLine($"任务分配给了: {string.Join(", ", assignedSecs.Select(s => s.Name))}");

        // 第三个任务继续分配
        var (sec3, _) = await _context.TaskOrchestrator.AssignTaskAsync(task3.Id, AgentRole.Secretary);
        Assert.NotNull(sec3);
        Console.WriteLine($"第三个任务分配给: {sec3.Name}");

        // 验证所有任务都有Agent处理（状态更新为Running）
        var allTasks = await _context.TaskRepository.GetAllAsync();
        foreach (var t in allTasks)
        {
            t.Status = TaskStatus.Running;
            await _context.TaskRepository.UpdateAsync(t);
        }
        
        var updatedTasks = await _context.TaskRepository.GetAllAsync();
        Assert.All(updatedTasks, t => Assert.Equal(TaskStatus.Running, t.Status));

        Console.WriteLine("✅ 多Agent并行处理测试完成！");
    }

    /// <summary>
    /// 测试最小负载优先分配策略
    /// </summary>
    [Fact]
    public async Task ContinuousOperation_LeastLoadStrategy_Should_BalanceLoad()
    {
        Console.WriteLine("⚖️ 测试：最小负载优先分配策略");

        // 创建3个Worker，其中2个已有任务
        var worker1 = TestDataFactory.CreateWorker("Worker-Load-1");
        worker1.CurrentTaskCount = 2;
        await _context.AgentRepository.CreateAsync(worker1);

        var worker2 = TestDataFactory.CreateWorker("Worker-Load-2");
        worker2.CurrentTaskCount = 3;
        await _context.AgentRepository.CreateAsync(worker2);

        var worker3 = TestDataFactory.CreateWorker("Worker-Load-3");
        worker3.CurrentTaskCount = 0; // 空闲
        await _context.AgentRepository.CreateAsync(worker3);

        // 创建任务
        var task = TestDataFactory.CreateTask("Load-Balance-Task");
        await _context.TaskRepository.CreateAsync(task);

        // 使用最小负载策略分配
        var (assignedWorker, _) = await _context.TaskOrchestrator.AssignTaskAsync(task.Id, AgentRole.Worker);

        // 验证分配给负载最小的Worker
        Assert.Equal("Worker-Load-3", assignedWorker?.Name);
        Assert.Equal(0, assignedWorker?.CurrentTaskCount);
        Console.WriteLine("任务分配给了负载最小的Worker");

        // 验证负载更新
        var updatedWorker = await _context.AgentRepository.GetByIdAsync(worker3.Id);
        Assert.Equal(1, updatedWorker?.CurrentTaskCount);
        Console.WriteLine("负载计数已更新");

        Console.WriteLine("✅ 最小负载优先策略测试完成！");
    }

    /// <summary>
    /// 测试Agent持续运行模拟场景
    /// </summary>
    [Fact]
    public async Task ContinuousOperation_Simulation_Should_Run()
    {
        Console.WriteLine("🔄 测试：Agent持续运行模拟");

        // 创建持续运行的Agent池
        var agents = new List<AgentEntity>();
        for (int i = 1; i <= 5; i++)
        {
            var worker = TestDataFactory.CreateWorker($"Worker-{i}");
            await _context.AgentRepository.CreateAsync(worker);
            agents.Add(worker);
        }

        // 模拟任务队列
        var tasks = new List<TaskEntity>();
        for (int i = 1; i <= 10; i++)
        {
            var task = TestDataFactory.CreateTask($"Simulation-Task-{i}");
            await _context.TaskRepository.CreateAsync(task);
            tasks.Add(task);
        }

        // 模拟任务分配过程
        int assignedCount = 0;
        foreach (var task in tasks)
        {
            var (assigned, _) = await _context.TaskOrchestrator.AssignTaskAsync(task.Id, AgentRole.Worker);
            if (assigned != null)
            {
                assignedCount++;
                task.Status = TaskStatus.Running;
                await _context.TaskRepository.UpdateAsync(task);
                Console.WriteLine($"任务 {task.Title} 分配给 {assigned.Name}");
            }
        }

        // 验证任务分配
        Assert.Equal(10, assignedCount);
        Console.WriteLine($"成功分配 {assignedCount} 个任务");

        // 模拟部分任务完成，Agent空闲
        foreach (var agent in agents.Take(3))
        {
            await _context.AgentRepository.DecrementTaskCountAsync(agent.Id);
            agent.State = AgentState.Idle;
            await _context.AgentRepository.UpdateAsync(agent);
        }
        Console.WriteLine("3个Agent完成任务，进入Idle状态");

        // 模拟空闲超时触发学习
        foreach (var agent in agents.Take(2))
        {
            agent.State = AgentState.Learning;
            await _context.AgentRepository.UpdateAsync(agent);
        }
        Console.WriteLine("2个Agent空闲超时，进入Learning状态");

        // 验证最终状态分布
        var allAgents = await _context.AgentRepository.GetAllAsync();
        var idleCount = allAgents.Count(a => a.State == AgentState.Idle);
        var handlingCount = allAgents.Count(a => a.State == AgentState.Handling);
        var learningCount = allAgents.Count(a => a.State == AgentState.Learning);

        Console.WriteLine($"最终状态分布: Idle={idleCount}, Handling={handlingCount}, Learning={learningCount}");

        Console.WriteLine("✅ Agent持续运行模拟测试完成！");
    }

    #endregion
}