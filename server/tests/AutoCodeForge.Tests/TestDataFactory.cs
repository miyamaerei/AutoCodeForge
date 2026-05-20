using AutoCodeForge.Core.DTOs.Agent;
using AutoCodeForge.Core.DTOs.Auth;
using AutoCodeForge.Core.DTOs.Chat;

namespace AutoCodeForge.Tests;

public static class TestDataFactory
{
    public static RegisterRequest CreateRegisterRequest(string prefix)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return new RegisterRequest
        {
            NtId = $"{prefix}.{suffix}",
            UserName = $"{prefix} user {suffix}",
            Email = $"{prefix}.{suffix}@example.com",
        };
    }

    public static CreateAgentRequest CreateAgentRequest(string prefix)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        return new CreateAgentRequest
        {
            Name = $"{prefix}-agent-{suffix}",
            Description = $"{prefix} integration test agent",
            SystemPrompt = "You are a helpful assistant.",
            IsEnabled = true,
        };
    }

    public static CreateSessionRequest CreateSessionRequest(Guid agentId, string prefix)
    {
        return new CreateSessionRequest
        {
            Title = $"{prefix} session {Guid.NewGuid():N}"[..24],
            AgentId = agentId,
        };
    }

    public static SendMessageRequest CreateSendMessageRequest(string message, Guid? agentId = null)
    {
        return new SendMessageRequest
        {
            Message = message,
            AgentId = agentId,
        };
    }
}