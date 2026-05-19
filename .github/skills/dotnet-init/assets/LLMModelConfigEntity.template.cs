using SqlSugar;

namespace __ProjectName__.Entities;

[SugarTable("LLMModelConfigs")]
public class LLMModelConfigEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NtId { get; set; } = string.Empty;

    public string ModelName { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public string Endpoint { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public int ContextWindow { get; set; } = 8192;

    public double Temperature { get; set; } = 0.7;

    public int MaxTokens { get; set; } = 4096;

    public double TopP { get; set; } = 1.0;

    public double FrequencyPenalty { get; set; } = 0.0;

    public double PresencePenalty { get; set; } = 0.0;

    public int Weight { get; set; } = 1;

    public int TimeoutSeconds { get; set; } = 60;

    public int MaxRetries { get; set; } = 3;

    public bool IsActive { get; set; } = true;

    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum LLMProvider { OpenAI, AzureOpenAI }

[SugarTable("AISessionConfigs")]
public class AISessionConfigEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string NtId { get; set; } = string.Empty;

    public int MaxMessagesPerSession { get; set; } = 100;

    public int MaxContextTokens { get; set; } = 4096;

    public int SessionTimeoutHours { get; set; } = 24;

    public bool AutoCleanupEnabled { get; set; } = true;

    public int AutoCleanupDays { get; set; } = 30;

    public bool MessageCompression { get; set; } = false;

    public int ToolCallTimeoutSeconds { get; set; } = 30;

    public int ToolCallMaxRetries { get; set; } = 3;

    public int CircuitBreakerThreshold { get; set; } = 5;

    public int CircuitBreakerResetMinutes { get; set; } = 30;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
