namespace AutoCodeForge.Core.Enums;

/// <summary>
/// Defines the types of configuration entries supported by the system.
/// </summary>
public enum ConfigType
{
    /// <summary>
    /// Global configuration (admin-only).
    /// </summary>
    Global,

    /// <summary>
    /// User-specific configuration.
    /// </summary>
    User,

    /// <summary>
    /// User preferences (language, timezone, theme).
    /// </summary>
    Preferences,

    /// <summary>
    /// Repository integration configuration.
    /// </summary>
    Repository,

    /// <summary>
    /// Knowledge base configuration.
    /// </summary>
    Knowledge,

    /// <summary>
    /// Skill configuration.
    /// </summary>
    Skill,

    /// <summary>
    /// Scheduled task configuration.
    /// </summary>
    Schedule,

    /// <summary>
    /// DeepWiki vector index configuration.
    /// </summary>
    DeepWiki,

    /// <summary>
    /// Code review configuration.
    /// </summary>
    Review,

    /// <summary>
    /// Third-party integration configuration.
    /// </summary>
    Integration,

    /// <summary>
    /// Notification strategy configuration.
    /// </summary>
    Notification,

    /// <summary>
    /// Sandbox execution configuration.
    /// </summary>
    Sandbox,

    /// <summary>
    /// Workflow orchestration configuration.
    /// </summary>
    Workflow,

    /// <summary>
    /// LLM configuration (unified model and credential management).
    /// Supports multiple models with different providers.
    /// </summary>
    Llm,

    /// <summary>
    /// Git configuration (repository and credential management).
    /// </summary>
    Git,

    /// <summary>
    /// System configuration (legacy compatibility).
    /// </summary>
    System
}