namespace AutoCodeForge.Application.Configuration;

public class OrchestrationSettings
{
    public int MaxConcurrentTasksPerManager { get; set; } = 5;
    
    public int MaxConcurrentTasksPerSecretary { get; set; } = 10;
    
    public int MaxConcurrentTasksPerWorker { get; set; } = 3;
    
    public string LoadBalancingStrategy { get; set; } = "LeastLoad";
    
    public bool EnableEscalation { get; set; } = true;
}