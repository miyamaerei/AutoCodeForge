using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Application.Configuration;

public class RetryPolicySettings
{
    public Dictionary<FailureCategory, RetryPolicy> RetryPolicies { get; set; } = new();
    
    public RetryPolicy GetPolicy(FailureCategory category)
    {
        if (RetryPolicies.TryGetValue(category, out var policy))
            return policy;
        
        return new RetryPolicy { MaxRetries = 1, IntervalMs = 1000 };
    }
}

public class RetryPolicy
{
    public int MaxRetries { get; set; } = 3;
    
    public int IntervalMs { get; set; } = 5000;
    
    public double BackoffMultiplier { get; set; } = 1.0;
    
    public DegradationAction DegradationAction { get; set; } = DegradationAction.LogOnly;
}

public enum DegradationAction
{
    LogOnly = 0,
    Fallback = 1,
    Terminate = 2,
    TriggerHumanGate = 3
}