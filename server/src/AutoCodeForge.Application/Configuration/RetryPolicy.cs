using AutoCodeForge.Core.Entities;

namespace AutoCodeForge.Application.Configuration;

public class RetryPolicySettings
{
    public Dictionary<FailureCategory, RetryPolicy> RetryPolicies { get; set; } = new();
    
    public RetryPolicy GetPolicy(FailureCategory category)
    {
        if (RetryPolicies.TryGetValue(category, out var policy))
            return policy;
        
        return category switch
        {
            FailureCategory.RequirementIssue => new RetryPolicy { MaxRetries = 0, IntervalMs = 0, DegradationAction = DegradationAction.TriggerHumanGate },
            FailureCategory.CodeError => new RetryPolicy { MaxRetries = 3, IntervalMs = 5000 },
            FailureCategory.LlmException => new RetryPolicy { MaxRetries = 5, IntervalMs = 10000 },
            FailureCategory.ReviewRejection => new RetryPolicy { MaxRetries = 3, IntervalMs = 5000 },
            FailureCategory.Timeout => new RetryPolicy { MaxRetries = 2, IntervalMs = 30000, BackoffMultiplier = 2 },
            _ => new RetryPolicy { MaxRetries = 1, IntervalMs = 1000 }
        };
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